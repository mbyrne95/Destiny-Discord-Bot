using System;

namespace DiscordBot
{
    public class ProfileResponse
    {
        public ProfileContainer Response { get; set; }
        public int ErrorCode { get; set; }
        public int ThrottleSeconds { get; set; }
        public string ErrorStatus { get; set; }
        public string Message { get; set; }
        public string MessageData { get; set; }
    }
}
