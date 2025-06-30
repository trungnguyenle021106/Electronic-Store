namespace UserService.Infrastructure.Setting
{
    public class JWTSetting
    {
        public string Issuer { get; set; } = string.Empty;
        public string[] Audiences { get; set; } = Array.Empty<string>();
        public string Key { get; set; } = string.Empty;
        public int ExpirationMinutes;
    }
}
