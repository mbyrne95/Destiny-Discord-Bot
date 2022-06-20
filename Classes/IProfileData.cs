using System;

namespace DiscordBot
{
    public class ProfileData
    {
        public UserInfoCard userInfo { get; set; }
        public DateTime dateLastPlayed { get; set; }
        public int versionsOwned { get; set; }
        public List<string> characterIds { get; set; }
        public List<object> seasonHashes { get; set; }
        public long currentSeasonHash { get; set; }
        public int currentSeasonRewardPowerCap { get; set; }

    }
}
