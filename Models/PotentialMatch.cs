namespace LostAndFoundAgency.Models
{
    public class PotentialMatch
    {
        public LostRequest Lost { get; set; } = null!;
        public FoundItem Found { get; set; } = null!;
        public string ItemName => Lost.ItemName;
        public string CategoryName { get; set; } = string.Empty;
        public string LostDateStr => Lost.DateLost.ToString("d");
        public string FoundDateStr => Found.DateFound.ToString("d");
        public string DescriptionSummary => $"Втрачено: {Lost.Description} | Знайдено: {Found.Description}";
    }
}
