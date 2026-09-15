namespace LostAndFoundAgency.Models
{
    public class CategoryReportRow
    {
        public string CategoryName { get; set; } = string.Empty;
        public int LostCount { get; set; }
        public int FoundCount { get; set; }
        public int ReturnedCount { get; set; }
        public int ActiveBalance => LostCount + FoundCount - ReturnedCount;
    }
}
