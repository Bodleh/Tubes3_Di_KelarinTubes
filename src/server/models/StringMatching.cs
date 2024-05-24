namespace server
{
    public class StringRequest
    {
        public string? Realname { get; set; }
        public bool? IsKMP { get; set; }
    }

    public class StringResult
    {
        public Biodata? Biodata { get; set; }
    }
}
