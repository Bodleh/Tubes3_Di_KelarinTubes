namespace server
{
    public class StringRequest
    {
        public string? Realname { get; set; }
        public bool? IsBM { get; set; }
    }

    public class StringResult
    {
        public Biodata? Biodata { get; set; }
    }
}
