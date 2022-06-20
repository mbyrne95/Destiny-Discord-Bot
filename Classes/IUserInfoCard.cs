using System;

namespace DiscordBot
{
    public class UserInfoCard
    {
        //public string supplementalDisplayName { get; set; }
        public string iconPath { get; set; }
        public int crossSaveOverride { get; set; }
        public List<int> applicableMembershipTypes { get; set; }
        public bool isPublic { get; set; }
        public int membershipType { get; set; }
        public string membershipId { get; set; }
        public string displayName { get; set; }
        public string bungieGlobalDisplayName { get; set; }
        public int bungieGlobalDisplayNameCode { get; set; }
    }
}
