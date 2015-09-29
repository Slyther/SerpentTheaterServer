namespace DatabaseController.Entities
{
    public class Episode : CompleteWatchable
    {
        public int SeasonId { get; set; }
        public Season Season { get; set; }
    }
}