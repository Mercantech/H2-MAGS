namespace DomainModels.DTOs.Users
{
public class GuestDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime LastLogin { get; set; }
    public int TotalBookings { get; set; }
    public bool IsEmailConfirmed { get; set; }
}
}