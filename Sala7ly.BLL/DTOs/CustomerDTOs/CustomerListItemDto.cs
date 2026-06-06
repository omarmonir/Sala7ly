namespace Sala7ly.BLL.DTOs.CustomerDTOs
{
    public class CustomerListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public int TotalRequests { get; set; }
        public bool IsActive { get; set; }
    }
}