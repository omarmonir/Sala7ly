using Sala7ly.DAL.Enums;

namespace Sala7ly.DAL.Entities
{
    public class ServiceRequest : BaseEntity
    {
        public ServiceRequest() { }
        
        public string Title { get; private set; }
        public string Description { get; private set; }
        public List<string> ImageUrls { get; private set; }  // for photos of the issue
        public Urgency Urgency { get; private set; }
        public Status Status { get; private set; }
        public BookingMode BookingMode { get; private set; }
        public decimal AiPriceMin { get; private set; }  // AI estimated price range
        public decimal AiPriceMax { get; private set; }
        public bool IsEmergency { get; private set; } = false;
        public decimal SurgeMultiplier { get; private set; } = 1.0m;  // for dynamic pricing during high demand
        public DateTime ScheduledAt { get; private set; }  // when customer wants the service
        public DateTime? CompletedAt { get; private set; }  // when service was completed
        public DateTime? StartedAt { get; private set; }  // when technician started the job
        public int CustomerId { get; private set; }
        public int AddressId { get; private set; }
        public int CategoryId { get; private set; }
        public int? SelectedBidId { get; private set; }  // nullable until customer selects a bid

        // navigation
        public CustomerProfile Profile { get; private set; }
        public ServiceCategory Category { get; private set; }
        public Address Address { get; private set; }
        public Bid SelectedBid { get; private set; }
        public EscrowTransaction EscrowTransaction { get; private set; }
        public ICollection<Bid> Bids { get; private set; }
        public ICollection<ChatMessage> ChatMessages { get; private set; }
        public ICollection<Ai_Interaction> AiInteractions { get; private set; }
        public ICollection<TechnicianPortfolio> TechnicianPortfolios { get; private set; }
        public Review Review { get; private set; }
    }



}