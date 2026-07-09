using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.DTOs.AdminDTOs
{
    public class AdminDashboardDto
    {
        public int TotalCustomers { get; set; }
        public int TotalTechnicians { get; set; }
        public int ActiveRequests { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public double GrowthPercent { get; set; }
        public int CompletedJobs { get; set; }
    }
}
