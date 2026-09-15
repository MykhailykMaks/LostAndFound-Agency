using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LostAndFoundAgency.Models
{
    public class Person
    {
        [Key]
        public int PersonId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Address { get; set; }

        public ICollection<LostRequest> LostRequests { get; set; } = new List<LostRequest>();
        public ICollection<FoundItem> FoundItems { get; set; } = new List<FoundItem>();
        public ICollection<Return> Returns { get; set; } = new List<Return>();
    }
}
