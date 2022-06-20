using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot_2.Classes
{
    public class UserInfo
    {
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
