using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

public class ActiveDirectoryService
{
    private readonly string _domain;
    private readonly string _container;
    private readonly string _ldapServer;
    private readonly string _serviceAccountUsername;
    private readonly string _serviceAccountPassword;

    public ActiveDirectoryService(IOptions<ActiveDirectorySettings> settings)
    {
        _domain = settings.Value.Domain;
        _container = settings.Value.Container;
        _ldapServer = settings.Value.LdapServer;
        _serviceAccountUsername = settings.Value.ServiceAccountUsername;
        _serviceAccountPassword = settings.Value.ServiceAccountPassword;
    }

    // Validerer brugerens legitimationsoplysninger
    public bool ValidateUser(string username, string password)
    {
        try
        {
            using (PrincipalContext context = new PrincipalContext(
                ContextType.Domain,
                _ldapServer,
                _container,
                username,
                password))
            {
                return context.ValidateCredentials(username, password);
            }
        }
        catch (Exception ex)
        {
            // Brug et logging-framework her
            Console.WriteLine($"Fejl ved validering af bruger: {ex.Message}");
            return false;
        }
    }

    // Henter brugerens grupper ved hjælp af DirectorySearcher
    public List<string> GetGroups(string username)
    {
        List<string> groups = new List<string>();

        try
        {
            // Brug DirectoryEntry og DirectorySearcher for bedre kontrol over henvisninger
            string ldapPath = $"LDAP://{_ldapServer}/{_container}";
            using (DirectoryEntry entry = new DirectoryEntry(ldapPath, _serviceAccountUsername, _serviceAccountPassword))
            {
                using (DirectorySearcher searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = $"(userPrincipalName={username})";
                    searcher.PropertiesToLoad.Add("memberOf");

                    SearchResult result = searcher.FindOne();

                    if (result != null)
                    {
                        if (result.Properties.Contains("memberOf"))
                        {
                            foreach (var group in result.Properties["memberOf"])
                            {
                                string groupDn = group.ToString();
                                string groupName = GetCN(groupDn);
                                groups.Add(groupName);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Bruger '{username}' blev ikke fundet i AD.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved hentning af grupper: {ex.Message}");
        }

        return groups;
    }

    // Ekstraherer CN fra DN
    private string GetCN(string dn)
    {
        if (string.IsNullOrEmpty(dn))
            return string.Empty;

        int cnIndex = dn.IndexOf("CN=", StringComparison.OrdinalIgnoreCase);
        if (cnIndex == -1)
            return dn;

        int commaIndex = dn.IndexOf(",", cnIndex);
        if (commaIndex == -1)
            return dn.Substring(cnIndex + 3);

        return dn.Substring(cnIndex + 3, commaIndex - (cnIndex + 3));
    }
}
public class ActiveDirectorySettings
{
    public string Domain { get; set; }
    public string Container { get; set; }
    public string LdapServer { get; set; }
    public string ServiceAccountUsername { get; set; }
    public string ServiceAccountPassword { get; set; }
}
