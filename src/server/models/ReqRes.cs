namespace server
{
    public class SearchRequest
    {
        public string? Data { get; set; }
    }

    public class SearchResult
    {
        public List<SidikJari> SidikJari { get; set; }
    }
}
