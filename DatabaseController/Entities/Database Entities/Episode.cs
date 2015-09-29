namespace DatabaseController.Entities
{
    public class Episode : CompleteWatchable
    {
        public long SeasonId { get; set; }
        public Season Season { get; set; }
    }
}