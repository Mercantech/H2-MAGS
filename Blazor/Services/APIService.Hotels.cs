using DomainModels;
using System.Net.Http.Json;

namespace Blazor.Services
{
    public partial class APIService
    {
        public async Task<HotelGetDto[]?> GetHotelsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<HotelGetDto[]>("api/hotels");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fejl ved hentning af hoteller: " + ex.Message);
                return null;
            }
        }

        public async Task<HotelGetDto?> GetHotelAsync(string id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<HotelGetDto>($"api/hotels/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved hentning af hotel {id}: " + ex.Message);
                return null;
            }
        }

        public async Task<HotelGetDto?> CreateHotelAsync(HotelPostDto hotelDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/hotels", hotelDto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HotelGetDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fejl ved oprettelse af hotel: " + ex.Message);
                return null;
            }
        }

        public async Task<bool> UpdateHotelAsync(string id, HotelPutDto hotelDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/hotels/{id}", hotelDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved opdatering af hotel {id}: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteHotelAsync(string id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/hotels/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved sletning af hotel {id}: " + ex.Message);
                return false;
            }
        }
    }
}
