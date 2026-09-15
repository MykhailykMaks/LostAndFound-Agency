using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LostAndFoundAgency.Models
{
    public class LostRequest
    {
        [Key] public int LostRequestId { get; set; }
        public int PersonId { get; set; }
        public int CategoryId { get; set; }
        public int LocationId { get; set; }
        public int? EmployeeId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Brand { get; set; }
        public DateTime DateLost { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = ItemStatuses.Pending;

        public Person? Person { get; set; }
        public Category? Category { get; set; }
        public Location? Location { get; set; }
        public Employee? Employee { get; set; }
        public ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}
