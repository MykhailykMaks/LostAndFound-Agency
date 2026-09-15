using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LostAndFoundAgency.Models
{
    public class Location
    {
        [Key] public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Description { get; set; }

        public ICollection<LostRequest> LostRequests { get; set; } = new List<LostRequest>();
        public ICollection<FoundItem> FoundItems { get; set; } = new List<FoundItem>();
    }
}
