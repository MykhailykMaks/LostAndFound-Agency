using System;
using System.ComponentModel.DataAnnotations;

namespace LostAndFoundAgency.Models
{
    public class Return
    {
        [Key] public int ReturnId { get; set; }
        public int FoundItemId { get; set; }
        public int PersonId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime ReturnDate { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Comment { get; set; }

        public FoundItem? FoundItem { get; set; }
        public Person? Person { get; set; }
        public Employee? Employee { get; set; }
    }
}
