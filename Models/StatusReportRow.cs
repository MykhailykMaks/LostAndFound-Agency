namespace LostAndFoundAgency.Models
{
    public class StatusReportRow
    {
        public string Status { get; set; } = string.Empty;
        public int LostCount { get; set; }
        public int FoundCount { get; set; }
    }
}
