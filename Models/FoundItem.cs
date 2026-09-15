using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LostAndFoundAgency.Models
{
    public class FoundItem
    {
        [Key] public int FoundItemId { get; set; }
        public int CategoryId { get; set; }
        public int LocationId { get; set; }
        public int FinderId { get; set; }
        public int? EmployeeId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Brand { get; set; }
        public DateTime DateFound { get; set; }
        public string? StorageLocation { get; set; }
        public string Status { get; set; } = ItemStatuses.Pending;
        public string? PhotoPath { get; set; }

        public Category? Category { get; set; }
        public Location? Location { get; set; }
        public Person? Finder { get; set; }
        public Employee? Employee { get; set; }
        public ICollection<Match> Matches { get; set; } = new List<Match>();
        public Return? Return { get; set; }
    }
}
