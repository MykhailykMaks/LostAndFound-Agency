using System;
using System.ComponentModel.DataAnnotations;

namespace LostAndFoundAgency.Models
{
    public class Match
    {
        [Key] public int MatchId { get; set; }
        public int FoundItemId { get; set; }
        public int LostRequestId { get; set; }
        public int MatchPercent { get; set; }
        public string MatchStatus { get; set; } = MatchStatuses.Pending;
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public FoundItem? FoundItem { get; set; }
        public LostRequest? LostRequest { get; set; }
    }
}
