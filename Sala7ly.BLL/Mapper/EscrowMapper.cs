using Sala7ly.BLL.DTOs.PaymentDTOs;
using Sala7ly.DAL.Entities;

namespace Sala7ly.BLL.Mapper
{
    public static class EscrowMapper
    {
        public static EscrowDto ToDto(EscrowTransaction e)
        {
            return new EscrowDto
            {
                Id = e.Id,
                RequestId = e.ServiceRequestId,
                Amount = e.Amount,
                PlatformFee = e.PlatformFee,
                TechnicianPayout = e.TechnicianPayout,
                Status = e.Status.ToString(),
                DepositedAt = e.DepositedAt,
                ReleasedAt = e.ReleasedAt
            };
        }
    }
}
