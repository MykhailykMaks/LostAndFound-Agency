using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LostAndFoundAgency.Models
{
    public class Employee
    {
        [Key] public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string? Phone { get; set; }

        public ICollection<LostRequest> AssignedLostRequests { get; set; } = new List<LostRequest>();
        public ICollection<FoundItem> AssignedFoundItems { get; set; } = new List<FoundItem>();
        public ICollection<Return> ProcessedReturns { get; set; } = new List<Return>();
    }
}
