using System;

namespace LostAndFoundAgency.Models
{
    public class ReturnReportRow
    {
        public DateTime ReturnDate { get; set; }
        public string ReturnDateStr => ReturnDate.ToString("g");
        public string ItemName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
    }
}
