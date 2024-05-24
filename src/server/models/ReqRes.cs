using System.Text.Json.Serialization;

namespace server
{
    public class SearchRequest
    {
        public string? Data { get; set; }
    }


    public class SearchResult
    {
        [JsonPropertyName("sidikJari")]
        public SidikJari? SidikJari { get; set; }
        
        [JsonPropertyName("matchPercentage")]
        public int MatchPercentage { get; set; }
    }
}
