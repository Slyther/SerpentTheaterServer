namespace DatabaseController.Entities
{
    public enum ContentType
    {
        Movies = 0,
        Series = 1
    }
    public class ContentPath
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public ContentType ContentType { get; set; }
    }
}
