using DomainModels;
using System.Net.Http.Json;

namespace Blazor.Services
{
    public partial class APIService
    {
        public async Task<RoomGetDto[]?> GetRoomsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<RoomGetDto[]>("api/rooms");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fejl ved hentning af rum: " + ex.Message);
                return null;
            }
        }

        public async Task<RoomGetDto?> GetRoomAsync(string id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<RoomGetDto>($"api/rooms/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved hentning af rum {id}: " + ex.Message);
                return null;
            }
        }

        public async Task<RoomGetDto[]?> GetRoomsByHotelAsync(string hotelId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<RoomGetDto[]>($"api/rooms/hotel/{hotelId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved hentning af rum for hotel {hotelId}: " + ex.Message);
                return null;
            }
        }

        public async Task<RoomGetDto?> CreateRoomAsync(RoomPostDto roomDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/rooms", roomDto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RoomGetDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fejl ved oprettelse af rum: " + ex.Message);
                return null;
            }
        }

        public async Task<bool> UpdateRoomAsync(string id, RoomPutDto roomDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/rooms/{id}", roomDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved opdatering af rum {id}: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteRoomAsync(string id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/rooms/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved sletning af rum {id}: " + ex.Message);
                return false;
            }
        }
    }
}
