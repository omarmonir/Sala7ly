using Sala7ly.DAL.Entities;

public class TechnicianCategory : BaseEntity
{
    public int TechnicianId { get; private set; }
    public int CategoryId { get; private set; }

    public int YearsInCategory { get; private set; }
    public bool IsPrimary { get; private set; } = false;

    public TechnicianProfile Technician { get; private set; }
    public ServiceCategory Category { get; private set; }

    private TechnicianCategory() { }

    public TechnicianCategory(int categoryId)
    {
        CategoryId = categoryId;
    }

    public void SetTechnician(int technicianId)
    {
        TechnicianId = technicianId;
    }
}