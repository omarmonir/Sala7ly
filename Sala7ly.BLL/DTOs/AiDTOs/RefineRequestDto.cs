using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.BLL.DTOs.AiDTOs
{
    public class RefineRequestDto
    {
        public string RawDescription { get; set; }   // ما كتبه العميل
        public string? CategoryHint { get; set; }    // optional
        public List<string> Categories { get; set; } = new(); // passed from frontend
    }
}
