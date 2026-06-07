using System.ComponentModel.DataAnnotations;

namespace Sala7ly.DAL.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime? DeletedOn { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? DeletedBy { get; set; }

        public bool? IsDeleted { get; set; }
        public void MarkCreated(string createdBy)
        {
            CreatedBy = createdBy;
            CreatedOn = DateTime.Now;
        }

        public void MarkUpdated(string updatedBy)
        {
            UpdatedBy = updatedBy;
            UpdatedOn = DateTime.Now;
        }



        public bool ToggaleStatus(string DeletedUser)
        {
            if (!string.IsNullOrEmpty(DeletedUser))
            {
                IsDeleted = !(IsDeleted ?? false); // treats null as false first
                DeletedBy = DeletedUser;
                DeletedOn = DateTime.Now;
                return true;
            }
            return false;
        }


    }
}
