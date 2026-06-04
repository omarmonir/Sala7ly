using System;

namespace Sala7ly.DAL.Entities
{
    public class FavoriteTechnician : BaseEntity
    {
        public int CustomerId { get; private set; }
        public int TechnicianId { get; private set; }
        public DateTime AddedAt { get; private set; } = DateTime.UtcNow;

        // navigation
        public CustomerProfile Customer { get; private set; }
        public TechnicianProfile Technician { get; private set; }
    }
}
