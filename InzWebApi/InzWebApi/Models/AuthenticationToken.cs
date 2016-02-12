using System;

namespace InzApp
{
    public class AuthenticationToken
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public long ExpiresIn { get; set; }
        public DateTime Expires { get; set; }
        public string UserId { get; set; }
    }
}