using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.DTOs.BidDTOs
{
    public class SubmitBidDto
    {
        public decimal Price { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public string ProposalMessage { get; set; }
    }
}
