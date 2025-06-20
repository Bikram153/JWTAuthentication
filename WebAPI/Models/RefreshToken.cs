namespace WebAPI.Models
{
    public class RefreshToken
    {
        public required string Token { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime Expires { get; set; }
        public bool Enabled { get; set; }
        public string Email { get; set; }
    }
}
