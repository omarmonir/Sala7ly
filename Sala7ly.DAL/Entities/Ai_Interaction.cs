using Sala7ly.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Entities
{
    public class Ai_Interaction :BaseEntity
    {

        public int? RequestId { get; set; }   

        public string UserId { get; set; }      

        public AiInteractionType InteractionType { get; set; }

        public string ModelUsed { get; set; }

        public string PromptSnapshot { get; set; }

        public string ResponseSnapshot { get; set; } 

        public float? ConfidenceScore { get; set; }

        public int InputTokens { get; set; }

        public int OutputTokens { get; set; }

        public int LatencyMs { get; set; }



        // NP

        public User User { get; set; } = null!;
        public ServiceRequest? ServiceRequest { get; set; }



    }
}
