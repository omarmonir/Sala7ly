using Sala7ly.BLL.DTOs.VerificationDTOs;
using Sala7ly.DAL.Entities;

namespace Sala7ly.BLL.Mapper
{
    public static class VerificationMapper
    {
        public static VerificationDetailsDto ToDetailsDto(TechnicianVerification v)
        {
            return new VerificationDetailsDto
            {
                Id = v.Id,
                TechnicianId = v.TechnicianId,
                TechnicianName = v.Technician?.User?.Name,
                //DocType = v.DocType.ToString(),
                DocumentUrlFront = v.DocumentUrlFront,
                DocumentUrlBack = v.DocumentUrlBack,
                Status = v.Status.ToString(),
                RejectionReason = v.RejectionReason,
                SubmittedAt = v.SubmittedAt,
                ReviewedAt = v.ReviewedAt
            };
        }
    }
}