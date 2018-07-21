namespace ui.Services
{
    public class LeaderboardRecord
    {
        public string Name;
        public string PublicId { get; set; }
        public long Timestamp { get; set; }
        public string ProbNum { get; set; }
        public long Energy { get; set; }
        public long Score { get; set; }
    }
}