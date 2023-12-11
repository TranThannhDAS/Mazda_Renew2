namespace Mazda.Model
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Pass {  get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpireTime { get; set; }
    }
}
