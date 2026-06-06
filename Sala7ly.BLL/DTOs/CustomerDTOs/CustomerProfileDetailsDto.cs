namespace Sala7ly.BLL.DTOs.CustomerDTOs
{
    public class CustomerProfileDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ImageUrl { get; set; }
        public string MainAddress { get; set; }
        public int TotalRequests { get; set; }
        public int TotalReviews { get; set; }
        public int MemberSinceYear { get; set; }
    }
}