using Sala7ly.DAL.Enums;

namespace Sala7ly.DAL.Entities
{
    public class ServiceRequest : BaseEntity
    {
        public ServiceRequest() { }

        public ServiceRequest(string title, string description, List<string> imageUrls, Urgency urgency, BookingMode bookingMode, 
                              bool isEmergency, DateTime scheduledAt, int customerId, int addressId, int categoryId)
        {
            Title = title;
            Description = description;
            ImageUrls = imageUrls;
            Urgency = urgency;
            BookingMode = bookingMode;
            IsEmergency = isEmergency;
            ScheduledAt = scheduledAt;
            CustomerId = customerId;
            AddressId = addressId;
            CategoryId = categoryId;
            Status = Status.open;
        }

        public string Title { get; private set; }
        public string Description { get; private set; }
        public List<string> ImageUrls { get; private set; }
        public Urgency Urgency { get; private set; }
        public Status Status { get; private set; }
        public BookingMode BookingMode { get; private set; }
        public decimal AiPriceMin { get; private set; }
        public decimal AiPriceMax { get; private set; }
        public bool IsEmergency { get; private set; } = false;
        public decimal SurgeMultiplier { get; private set; } = 1.0m;
        public DateTime ScheduledAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public DateTime? StartedAt { get; private set; }
        public int CustomerId { get; private set; }
        public int AddressId { get; private set; }
        public int CategoryId { get; private set; }
        public int? SelectedBidId { get; private set; }

        // navigation
        public CustomerProfile Profile { get; private set; }
        public ServiceCategory Category { get; private set; }
        public Address Address { get; private set; }
        public Bid SelectedBid { get; private set; }
        public EscrowTransaction EscrowTransaction { get; private set; }
        public ICollection<Bid> Bids { get; private set; }
        public ICollection<ChatMessage> ChatMessages { get; private set; }
        public ICollection<Ai_Interaction> AiInteractions { get; private set; }
        public Review Review { get; private set; }

        public void AssignBid(int bidId)
        {
            if (Status != Status.open)
                throw new InvalidOperationException("Request is not open.");
            SelectedBidId = bidId;
            Status = Status.assigned;
        }

        public void Start()
        {
            if (Status != Status.assigned)
                throw new InvalidOperationException("Request must be assigned first.");
            Status = Status.in_progress;
            StartedAt = DateTime.UtcNow;
        }

        public void MarkCompleted()
        {
            if (Status != Status.in_progress)
                throw new InvalidOperationException("Request must be in progress.");
            Status = Status.completed;
            CompletedAt = DateTime.UtcNow;
        }
        public void Cancel()
        {
            if (Status == Status.completed)
                throw new InvalidOperationException("Cannot cancel a completed request.");
            Status = Status.cancelled;
        }

        public void SetAiData(decimal priceMin, decimal priceMax)
        {
            AiPriceMin = priceMin;
            AiPriceMax = priceMax;
        }
    }
}