namespace Blazor.Models
{
    /// <summary>
    /// Generisk API response wrapper
    /// </summary>
    /// <typeparam name="T">Type af data der returneres</typeparam>
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string? WarningMessage { get; set; }
    }
}
