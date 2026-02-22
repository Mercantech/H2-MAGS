using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.Extensions.Logging;

namespace API.Services
{
    /// <summary>
    /// Service til håndtering af Active Directory autentificering og brugerinformation
    /// </summary>
    public class ActiveDirectoryService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ActiveDirectoryService> _logger;
        
        // AD konfiguration fra appsettings.json
        private readonly string _server;
        private readonly string _username;
        private readonly string _password;
        private readonly string _domain;
        private readonly int _port;
        private readonly bool _useSSL;
        private readonly int _connectionTimeout;
        private readonly int _maxRetries;
        private readonly int _retryDelayMs;

        /// <summary>
        /// Initialiserer en ny instans af ActiveDirectoryService
        /// </summary>
        /// <param name="configuration">Konfiguration til AD indstillinger</param>
        /// <param name="logger">Logger til fejlhåndtering</param>
        public ActiveDirectoryService(IConfiguration configuration, ILogger<ActiveDirectoryService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Læs AD konfiguration fra appsettings.json
            _server = _configuration["ActiveDirectory:Server"] ?? "10.133.71.100";
            _domain = _configuration["ActiveDirectory:Domain"] ?? "mags.local";
            _username = _configuration["ActiveDirectory:ReaderUsername"] ?? "adReader";
            _password = _configuration["ActiveDirectory:ReaderPassword"] ?? "Merc1234!";
            _port = int.Parse(_configuration["ActiveDirectory:Port"] ?? "389");
            _useSSL = bool.Parse(_configuration["ActiveDirectory:UseSSL"] ?? "false");
            _connectionTimeout = int.Parse(_configuration["ActiveDirectory:ConnectionTimeout"] ?? "30");
            _maxRetries = int.Parse(_configuration["ActiveDirectory:MaxRetries"] ?? "3");
            _retryDelayMs = int.Parse(_configuration["ActiveDirectory:RetryDelayMs"] ?? "1000");
            
            _logger.LogInformation("ActiveDirectoryService initialiseret - Server: {Server}:{Port}, Domain: {Domain}", 
                _server, _port, _domain);
        }

        /// <summary>
        /// Autentificerer en bruger mod Active Directory
        /// </summary>
        /// <param name="username">Brugernavn (kan være email eller sAMAccountName)</param>
        /// <param name="password">Adgangskode</param>
        /// <returns>ADUserInfo med brugerinformation hvis autentificering lykkes, ellers null</returns>
        public async Task<ADUserInfo?> AuthenticateUserAsync(string username, string password)
        {
            _logger.LogInformation("Forsøger at autentificere bruger: {Username} mod AD server: {Server}:{Port}", 
                username, _server, _port);

            for (int attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    _logger.LogDebug("Forsøg {Attempt}/{MaxRetries} - Opretter LDAP forbindelse til {Server}:{Port}", 
                        attempt, _maxRetries, _server, _port);

                    // Opret LDAP forbindelse
                    using var connection = new LdapConnection(new LdapDirectoryIdentifier(_server, _port));
                    connection.SessionOptions.ProtocolVersion = 3;
                    connection.SessionOptions.SecureSocketLayer = _useSSL;
                    connection.SessionOptions.VerifyServerCertificate = (conn, cert) => true;
                    connection.Timeout = TimeSpan.FromSeconds(_connectionTimeout);

                    // Opret credentials for AD reader bruger
                    var networkCredential = new NetworkCredential(_username, _password, _domain);
                    connection.Credential = networkCredential;

                    // Åbn forbindelse
                    await Task.Run(() => connection.Bind());
                    _logger.LogDebug("LDAP forbindelse etableret succesfuldt");

                    // Søg efter brugeren i AD
                    var userInfo = await SearchUserInADAsync(connection, username);
                    if (userInfo == null)
                    {
                        _logger.LogWarning("Bruger {Username} ikke fundet i Active Directory", username);
                        return null;
                    }

                    _logger.LogDebug("Bruger {Username} fundet i AD, tester credentials", username);

                    // Test brugerens credentials
                    var userCredentials = new NetworkCredential(userInfo.SamAccountName, password, _domain);
                    using var userConnection = new LdapConnection(new LdapDirectoryIdentifier(_server, _port));
                    userConnection.SessionOptions.ProtocolVersion = 3;
                    userConnection.SessionOptions.SecureSocketLayer = _useSSL;
                    userConnection.SessionOptions.VerifyServerCertificate = (conn, cert) => true;
                    userConnection.Timeout = TimeSpan.FromSeconds(_connectionTimeout);
                    userConnection.Credential = userCredentials;

                    // Test autentificering
                    await Task.Run(() => userConnection.Bind());

                    _logger.LogInformation("Bruger {Username} autentificeret succesfuldt", username);
                    return userInfo;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fejl ved forsøg {Attempt}/{MaxRetries} for bruger {Username}: {ErrorMessage}", 
                        attempt, _maxRetries, username, ex.Message);

                    if (attempt == _maxRetries)
                    {
                        _logger.LogError("Alle {MaxRetries} forsøg fejlede for bruger {Username}", _maxRetries, username);
                        return null;
                    }

                    // Vent før næste forsøg
                    await Task.Delay(_retryDelayMs);
                }
            }

            return null;
        }

        /// <summary>
        /// Søger efter en bruger i Active Directory
        /// </summary>
        /// <param name="connection">LDAP forbindelse</param>
        /// <param name="username">Brugernavn eller email at søge efter</param>
        /// <returns>ADUserInfo hvis brugeren findes, ellers null</returns>
        private async Task<ADUserInfo?> SearchUserInADAsync(LdapConnection connection, string username)
        {
            try
            {
                // Konstruer søgning - søg både på sAMAccountName og email
                var searchFilter = $"(|(sAMAccountName={username})(mail={username})(userPrincipalName={username}))";
                var searchRequest = new SearchRequest(
                    $"DC={_domain.Split('.')[0]},DC={_domain.Split('.')[1]}", // Konstruer base DN
                    searchFilter,
                    SearchScope.Subtree,
                    "sAMAccountName", "mail", "displayName", "givenName", "sn", "memberOf", "userPrincipalName"
                );

                var searchResponse = await Task.Run(() => (SearchResponse)connection.SendRequest(searchRequest));

                if (searchResponse.Entries.Count == 0)
                {
                    return null;
                }

                var entry = searchResponse.Entries[0];
                
                var userInfo = new ADUserInfo
                {
                    SamAccountName = GetAttributeValue(entry, "sAMAccountName"),
                    Email = GetAttributeValue(entry, "mail"),
                    DisplayName = GetAttributeValue(entry, "displayName"),
                    FirstName = GetAttributeValue(entry, "givenName"),
                    LastName = GetAttributeValue(entry, "sn"),
                    UserPrincipalName = GetAttributeValue(entry, "userPrincipalName"),
                    Groups = GetGroupMemberships(entry)
                };

                return userInfo;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Henter værdien af et attribut fra en AD entry
        /// </summary>
        private string GetAttributeValue(SearchResultEntry entry, string attributeName)
        {
            if (entry.Attributes[attributeName] != null && entry.Attributes[attributeName].Count > 0)
            {
                return entry.Attributes[attributeName][0].ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Henter gruppemedlemskaber fra en AD entry
        /// </summary>
        private List<string> GetGroupMemberships(SearchResultEntry entry)
        {
            var groups = new List<string>();
            
            if (entry.Attributes["memberOf"] != null)
            {
                foreach (var group in entry.Attributes["memberOf"])
                {
                    var groupDn = group.ToString();
                    
                    if (!string.IsNullOrEmpty(groupDn))
                    {
                        // Ekstraher gruppenavn fra DN (f.eks. "CN=Admins,OU=Groups,DC=mags,DC=local" -> "Admins")
                        var cnIndex = groupDn.IndexOf("CN=", StringComparison.OrdinalIgnoreCase);
                        if (cnIndex >= 0)
                        {
                            var cnEnd = groupDn.IndexOf(",", cnIndex);
                            if (cnEnd > cnIndex)
                            {
                                var groupName = groupDn.Substring(cnIndex + 3, cnEnd - cnIndex - 3);
                                groups.Add(groupName);
                            }
                            else
                            {
                                // Hvis der ikke er et komma efter CN, så er det hele gruppenavnet
                                var groupName = groupDn.Substring(cnIndex + 3);
                                groups.Add(groupName);
                            }
                        }
                    }
                }
            }

            return groups;
        }


        /// <summary>
        /// Tester LDAP forbindelsen til Active Directory serveren
        /// </summary>
        /// <returns>True hvis forbindelsen lykkes, ellers false</returns>
        public async Task<bool> TestLDAPConnectionAsync()
        {
            _logger.LogInformation("Tester LDAP forbindelse til {Server}:{Port}", _server, _port);
            
            try
            {
                // Opret LDAP forbindelse
                using var connection = new LdapConnection(new LdapDirectoryIdentifier(_server, _port));
                connection.SessionOptions.ProtocolVersion = 3;
                connection.SessionOptions.SecureSocketLayer = _useSSL;
                connection.SessionOptions.VerifyServerCertificate = (conn, cert) => true;
                connection.Timeout = TimeSpan.FromSeconds(_connectionTimeout);

                // Opret credentials for AD reader bruger
                var networkCredential = new NetworkCredential(_username, _password, _domain);
                connection.Credential = networkCredential;

                // Åbn forbindelse
                await Task.Run(() => connection.Bind());

                _logger.LogInformation("LDAP forbindelse til {Server}:{Port} lykkedes", _server, _port);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved LDAP forbindelse til {Server}:{Port}: {ErrorMessage}", 
                    _server, _port, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Mapper AD grupper til applikationsroller
        /// </summary>
        /// <param name="adGroups">Liste af AD grupper</param>
        /// <returns>Applikationsrolle navn</returns>
        public string MapADGroupToRole(List<string> adGroups)
        {
            if (adGroups.Any(g => g.Contains("Admin", StringComparison.OrdinalIgnoreCase) || 
                                 g.Contains("Administrator", StringComparison.OrdinalIgnoreCase)))
            {
                return "Admin";
            }
            
            if (adGroups.Any(g => g.Contains("Manager", StringComparison.OrdinalIgnoreCase)))
            {
                return "Manager";
            }
            
            if (adGroups.Any(g => g.Contains("Receptionist", StringComparison.OrdinalIgnoreCase) ||
                                 g.Contains("Reception", StringComparison.OrdinalIgnoreCase)))
            {
                return "Receptionist";
            }
            
            if (adGroups.Any(g => g.Contains("User", StringComparison.OrdinalIgnoreCase)))
            {
                return "User";
            }

            return "Receptionist";
        }
    }

    /// <summary>
    /// Model til AD brugerinformation
    /// </summary>
    public class ADUserInfo
    {
        /// <summary>
        /// SAM Account Name fra Active Directory
        /// </summary>
        public string SamAccountName { get; set; } = string.Empty;
        
        /// <summary>
        /// Email adresse fra Active Directory
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Display Name fra Active Directory
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
        
        /// <summary>
        /// Fornavn fra Active Directory
        /// </summary>
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// Efternavn fra Active Directory
        /// </summary>
        public string LastName { get; set; } = string.Empty;
        
        /// <summary>
        /// User Principal Name fra Active Directory
        /// </summary>
        public string UserPrincipalName { get; set; } = string.Empty;
        
        /// <summary>
        /// Liste af grupper brugeren er medlem af i Active Directory
        /// </summary>
        public List<string> Groups { get; set; } = new List<string>();
    }
}
