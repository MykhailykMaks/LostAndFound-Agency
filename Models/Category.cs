using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LostAndFoundAgency.Models
{
    public class Category
    {
        [Key] public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<LostRequest> LostRequests { get; set; } = new List<LostRequest>();
        public ICollection<FoundItem> FoundItems { get; set; } = new List<FoundItem>();
    }
}
