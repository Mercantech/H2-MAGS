using DomainModels;
using Blazor.Models;
using System.Text;
using System.Text.Json;

namespace Blazor.Services
{
    public partial class APIService
    {
        /// <summary>
        /// Hent alle tickets
        /// </summary>
        public async Task<ApiResponse<IEnumerable<TicketGetDto>>> GetTicketsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/tickets");
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var tickets = JsonSerializer.Deserialize<IEnumerable<TicketGetDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ApiResponse<IEnumerable<TicketGetDto>>
                    {
                        IsSuccess = true,
                        Data = tickets
                    };
                }
                else
                {
                    return new ApiResponse<IEnumerable<TicketGetDto>>
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Fejl ved hentning af tickets: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<TicketGetDto>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception ved hentning af tickets: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Opret nyt ticket
        /// </summary>
        public async Task<ApiResponse<TicketGetDto>> CreateTicketAsync(TicketCreateDto ticketDto)
        {
            try
            {
                var json = JsonSerializer.Serialize(ticketDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/tickets", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var ticket = JsonSerializer.Deserialize<TicketGetDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ApiResponse<TicketGetDto>
                    {
                        IsSuccess = true,
                        Data = ticket
                    };
                }
                else
                {
                    return new ApiResponse<TicketGetDto>
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Fejl ved oprettelse af ticket: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<TicketGetDto>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception ved oprettelse af ticket: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Tildel ticket til medarbejder
        /// </summary>
        public async Task<ApiResponse<TicketGetDto>> AssignTicketAsync(string ticketId, string assigneeId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/tickets/{ticketId}/assign/{assigneeId}", null);
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var ticket = JsonSerializer.Deserialize<TicketGetDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ApiResponse<TicketGetDto>
                    {
                        IsSuccess = true,
                        Data = ticket
                    };
                }
                else
                {
                    return new ApiResponse<TicketGetDto>
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Fejl ved tildeling af ticket: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<TicketGetDto>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception ved tildeling af ticket: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Hent mine tickets (tickets hvor brugeren er requester)
        /// </summary>
        public async Task<ApiResponse<IEnumerable<TicketGetDto>>> GetMyTicketsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/tickets/my-tickets");
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var tickets = JsonSerializer.Deserialize<IEnumerable<TicketGetDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ApiResponse<IEnumerable<TicketGetDto>>
                    {
                        IsSuccess = true,
                        Data = tickets
                    };
                }
                else
                {
                    return new ApiResponse<IEnumerable<TicketGetDto>>
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Fejl ved hentning af mine tickets: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<TicketGetDto>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception ved hentning af mine tickets: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Luk ticket
        /// </summary>
        public async Task<ApiResponse<TicketGetDto>> CloseTicketAsync(string ticketId, string resolution)
        {
            try
            {
                var json = JsonSerializer.Serialize(new { resolution });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"api/tickets/{ticketId}/close", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var ticket = JsonSerializer.Deserialize<TicketGetDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ApiResponse<TicketGetDto>
                    {
                        IsSuccess = true,
                        Data = ticket
                    };
                }
                else
                {
                    return new ApiResponse<TicketGetDto>
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Fejl ved lukning af ticket: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<TicketGetDto>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception ved lukning af ticket: {ex.Message}"
                };
            }
        }
    }
}
