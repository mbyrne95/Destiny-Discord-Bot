using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordBot
{
    public class PrimaryModule : BaseCommandModule
    {
        protected string apiKey = System.IO.File.ReadAllText("apiKey.txt");
        protected string apiRoot = "https://www.bungie.net/Platform";
        protected string manifest = "https://www.bungie.net/Platform/Destiny2/Manifest/";
        HttpClient client = new HttpClient();

        //convert string platform to platform id
        private int platformConvert(string platformString)
        {
            platformString = platformString.ToLower();
            switch (platformString)
            {
                case "xbox":
                    return 1;
                case "xsx":
                    return 1;
                case "xbx":
                    return 1;
                case "xss":
                    return 1;
                case "playstation":
                    return 2;
                case "ps4":
                    return 2;
                case "ps5":
                    return 2;
                case "psn":
                    return 2;
                case "pc":
                    return 3;
                case "steam":
                    return 3;
                case "all":
                    return -1;
                default:
                    return -1;
            }
        }

        //convert raceID to corresponding race
        private string convertRace(string raceID)
        {
            switch (raceID)
            {
                case "0":
                    return "Human";
                case "1":
                    return "Awoken";
                case "2":
                    return "Exo";
                default:
                    return "Unkown";
            }
        }

        //convert classID to corresponding class
        private string convertClass(string classID)
        {
            switch (classID)
            {
                case "0":
                    return "Titan";
                case "1":
                    return "Hunter";
                case "2":
                    return "Warlock";
                default:
                    return "Unkown";
            }
        }

        //convert genderID to corresponding gender
        //idk why this is a thing, sigh
        private string convertGender(string genderID)
        {
            switch (genderID)
            {
                case "0":
                    return "Male";
                case "1":
                    return "Female";
                default:
                    return "Unkown";
            }
        }

        //convert item hash to associated id
        private int convertHashToId(string hashString)
        {
            long hash = long.Parse(hashString);
            var id = unchecked((int)hash);
            return id;
        }

        //add headers to the http call - must be added to every call
        private void addHeaders()
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        }

        //get the "SearchDestinyPlayer" object for a given displayname/platform
        private SearchDestinyPlayer getDestinyPlayerInfo (string platform, string name)
        {
            name = CleanString.ReplaceHashtag(name);
            int platformId = platformConvert(platform);
            string endpoint = "/Destiny2/SearchDestinyPlayer/" + platformId + "/" + name;
            var url = apiRoot + endpoint;
            addHeaders();
            var request = client.GetAsync(url).Result;
            var result = request.Content.ReadAsStringAsync().Result;
            var destinyPlayer = JsonConvert.DeserializeObject<SearchDestinyPlayer>(result);
            return destinyPlayer;
        }


        //returns an array of item hashes and associated instance ids
        //  [ kineticHash, energyHash, powerHash ]
        //  [ kineticId  , energyId  , powerId   ]
        private string[,] getItemHashPerCharacter (string platform, string membershipId, string characterId)
        {
            string platformId = platformConvert(platform).ToString();
            string endPoint = "/Destiny2/" + platformId + "/Profile/" + membershipId + "/Character/" + characterId + "/?components=205";
            var url = apiRoot + endPoint;
            addHeaders();
            var request = client.GetAsync(url).Result;
            var result = request.Content.ReadAsStringAsync().Result;

            var o = JObject.Parse(result);

            var itemContainer = o["Response"]["equipment"]["data"]["items"].Children().First();

            String[,] itemInfo = new String[2,3];

            //obtaining character item hashes and instance ids
            itemInfo[0, 0] = itemContainer["itemHash"].ToString();
            itemInfo[1, 0] = itemContainer["itemInstanceId"].ToString();

            itemContainer = itemContainer.Next;

            itemInfo[0, 1] = itemContainer["itemHash"].ToString();
            itemInfo[1, 1] = itemContainer["itemInstanceId"].ToString();

            itemContainer = itemContainer.Next;

            itemInfo[0, 2] = itemContainer["itemHash"].ToString();
            itemInfo[1, 2] = itemContainer["itemInstanceId"].ToString();

            return itemInfo;
        }

        private string itemInstanceIdtoPerks (string platform, string membershipId, string itemInstanceId)
        {
            string platformId = platformConvert(platform).ToString();
            string endPoint = "/Destiny2/" + platformId + "/Profile/" + membershipId + "/Item/" + itemInstanceId + "/?components=302";
            var url = apiRoot + endPoint;
            addHeaders();
            var request = client.GetAsync(url).Result;
            var result = request.Content.ReadAsStringAsync().Result;

            var o = JObject.Parse(result);

            var perkBucket = o["Response"]["perks"]["data"]["perks"].Children().First();

            //List<String> perkList = new List<String>();

            string formattedPerks = "";;

            //iterate through perks, leaving out null perks and converting to Ids
            for (int i = 0; i < o["Response"]["perks"]["data"]["perks"].Children().Count(); i++)
            {
                var x = JObject.Parse(SQLCheck.perkLookupById(convertHashToId(perkBucket["perkHash"].ToString())));

                //second to last
                if (i == o["Response"]["perks"]["data"]["perks"].Children().Count() - 2)
                {
                    if (perkBucket.Next["visible"].ToString().Equals("false", StringComparison.OrdinalIgnoreCase) || perkBucket.Next["isActive"].ToString().Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((perkBucket["visible"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                            && (perkBucket["isActive"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                            && !(x["displayProperties"]["name"].ToString().Contains("Burst"))
                            && !(x["displayProperties"]["name"].ToString().Contains("Glaive"))
                            && !(x["displayProperties"]["name"].ToString().Contains("Frame")))
                        {
                            formattedPerks += x["displayProperties"]["name"].ToString();
                        }
                    }
                    else
                    {
                        if ((perkBucket["visible"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                            && (perkBucket["isActive"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                            && !(x["displayProperties"]["name"].ToString().Contains("Burst"))
                            && !(x["displayProperties"]["name"].ToString().Contains("Glaive"))
                            && !(x["displayProperties"]["name"].ToString().Contains("Frame")))
                        {
                            formattedPerks += x["displayProperties"]["name"].ToString() + " | ";
                        }
                    }
                }

                //last
                else if (i == o["Response"]["perks"]["data"]["perks"].Children().Count() - 1)
                {
                    if ((perkBucket["visible"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        && (perkBucket["isActive"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        && !(x["displayProperties"]["name"].ToString().Contains("Burst"))
                        && !(x["displayProperties"]["name"].ToString().Contains("Glaive"))
                        && !(x["displayProperties"]["name"].ToString().Contains("Frame")))
                    {
                        formattedPerks += x["displayProperties"]["name"].ToString();
                    }
                }

                //every other case
                else
                {
                    if ((perkBucket["visible"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        && (perkBucket["isActive"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                        && !(x["displayProperties"]["name"].ToString().Contains("Burst"))
                        && !(x["displayProperties"]["name"].ToString().Contains("Glaive"))
                        && !(x["displayProperties"]["name"].ToString().Contains("Frame")))
                    {
                        formattedPerks += x["displayProperties"]["name"].ToString() + " | ";
                    }

                }   

                perkBucket = perkBucket.Next;

            }

            return formattedPerks;
        }

        //gets info of a destiny player (using getDestinyPlayerInfo) - uses bungie id to query profile endpoint, pulling basic info about the account's associated characters
        //added - equipped weapons of the relevant character
        //need to add: Exception Handling, Perks of equipped weapons
        [Command("overview")]
        public async Task overview (CommandContext ctx, string platform, [RemainingText] string name)
        {
            string cleanName = CleanString.ReplaceHashtag(name);
            string platformId = platformConvert(platform).ToString();
            SearchDestinyPlayer player = getDestinyPlayerInfo(platformId, cleanName);
            string membershipId = player.Response[0].membershipId;
            string officialName = player.Response[0].displayName + "#" + player.Response[0].bungieGlobalDisplayNameCode.ToString();

            string endpoint = "/Destiny2/" + platformId + "/Profile/" + membershipId + "/?components=200";
            var url = apiRoot + endpoint;
            addHeaders();
            var request = client.GetAsync(url).Result;
            var result = request.Content.ReadAsStringAsync().Result;

            var o = JObject.Parse(result);

            //first character info
            var firstChar = o["Response"]["characters"]["data"].Children().First();
            var firstCharProperties = firstChar.Children().First();

            //getting the path name for the characteritemhash
            string x = firstChar.Path.ToString();
            string[] parts = x.Split('.');
            string firstCharId = parts[parts.Length - 1];

            //getting weapon info for first character
            string[,] firstCharWeapons = getItemHashPerCharacter(platform, membershipId, firstCharId);
            string firstKineticJson = SQLCheck.weaponLookupById(convertHashToId(firstCharWeapons[0, 0]));
            string firstKineticName = (JObject.Parse(firstKineticJson))["displayProperties"]["name"].ToString();
            string firstKineticPerks = itemInstanceIdtoPerks(platform, membershipId, firstCharWeapons[1, 0]);

            string firstEnergyJson = SQLCheck.weaponLookupById(convertHashToId(firstCharWeapons[0, 1]));
            string firstEnergyName = (JObject.Parse(firstEnergyJson))["displayProperties"]["name"].ToString();
            string firstEnergyPerks = itemInstanceIdtoPerks(platform, membershipId, firstCharWeapons[1, 1]);

            string firstPowerJson = SQLCheck.weaponLookupById(convertHashToId(firstCharWeapons[0, 2]));
            string firstPowerName = (JObject.Parse(firstPowerJson))["displayProperties"]["name"].ToString();
            string firstPowerPerks = itemInstanceIdtoPerks(platform, membershipId, firstCharWeapons[1, 2]);

            //generalized info for first character
            string firstCharLight = firstCharProperties["light"].ToString();
            string firstCharRace = convertRace(firstCharProperties["raceType"].ToString());
            string firstCharClass = convertClass(firstCharProperties["classType"].ToString());
            string firstCharGender = convertGender(firstCharProperties["genderType"].ToString());
            int firstCharTimePlayed = Int32.Parse(firstCharProperties["minutesPlayedTotal"].ToString());
            string firstCharEmblemPath = firstCharProperties["emblemPath"].ToString();

            var firstMsg = new DiscordEmbedBuilder()
            {

                Color = DiscordColor.Blurple,
                Title = officialName + "'s Guardians:",
                Description = "**" + firstCharClass + "**, " + firstCharRace + " " + firstCharGender,

                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = "https://www.bungie.net/" + firstCharEmblemPath
                }

            };

            firstMsg.AddField("Light Level: ", firstCharLight, true);
            firstMsg.AddField("Hours Played: ", (firstCharTimePlayed / 60).ToString(), true);
            firstMsg.AddField("Equipped Weapons:\n"+ firstKineticName, firstKineticPerks);
            firstMsg.AddField(firstEnergyName, firstEnergyPerks);
            firstMsg.AddField(firstPowerName, firstPowerPerks);

            await ctx.RespondAsync(firstMsg);

            //second char
            var secondChar = firstChar.Next;

            if (secondChar != null)
            {
                x = secondChar.Path.ToString();
                parts = x.Split('.');
                var secondCharId = parts[parts.Length - 1];

                //second char weapon info
                string[,] secondCharWeapons = getItemHashPerCharacter(platform, membershipId, secondCharId);
                string secondKineticJson = SQLCheck.weaponLookupById(convertHashToId(secondCharWeapons[0, 0]));
                string secondKineticName = (JObject.Parse(secondKineticJson))["displayProperties"]["name"].ToString();
                string KineticPerks = itemInstanceIdtoPerks(platform, membershipId, secondCharWeapons[1, 0]);


                string secondEnergyJson = SQLCheck.weaponLookupById(convertHashToId(secondCharWeapons[0, 1]));
                string secondEnergyName = (JObject.Parse(secondEnergyJson))["displayProperties"]["name"].ToString();
                string EnergyPerks = itemInstanceIdtoPerks(platform, membershipId, secondCharWeapons[1, 1]);


                string secondPowerJson = SQLCheck.weaponLookupById(convertHashToId(secondCharWeapons[0, 2]));
                string secondPowerName = (JObject.Parse(secondPowerJson))["displayProperties"]["name"].ToString();
                string PowerPerks = itemInstanceIdtoPerks(platform, membershipId, secondCharWeapons[1, 2]);


                var secondCharProperties = secondChar.Children().First();
                string secondCharLight = secondCharProperties["light"].ToString();
                string secondCharRace = convertRace(secondCharProperties["raceType"].ToString());
                string secondCharClass = convertClass(secondCharProperties["classType"].ToString());
                string secondCharGender = convertGender(secondCharProperties["genderType"].ToString());
                int secondCharTimePlayed = Int32.Parse(secondCharProperties["minutesPlayedTotal"].ToString());
                string secondCharEmblemPath = secondCharProperties["emblemPath"].ToString();

                var secondMsg = new DiscordEmbedBuilder()
                {

                    Color = DiscordColor.Blurple,
                    Title = officialName + "'s Guardians:",
                    Description = "**" + secondCharClass + "**, " + secondCharRace + " " + secondCharGender,

                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = "https://www.bungie.net/" + secondCharEmblemPath
                    }

                };

                secondMsg.AddField("Light Level: ", secondCharLight, true);
                secondMsg.AddField("Hours Played: ", (secondCharTimePlayed / 60).ToString(), true);
                secondMsg.AddField("Equipped Weapons:\n" + secondKineticName, KineticPerks);
                secondMsg.AddField(secondEnergyName, EnergyPerks);
                secondMsg.AddField(secondPowerName, PowerPerks);

                await ctx.RespondAsync(secondMsg);
            }

            //third char info
            var thirdChar = secondChar.Next;

            if(thirdChar != null)
            {
                x = thirdChar.Path.ToString();
                parts = x.Split('.');
                var thirdCharId = parts[parts.Length - 1];

                string[,] thirdCharWeapons = getItemHashPerCharacter(platform, membershipId, thirdCharId);
                string thirdKineticJson = SQLCheck.weaponLookupById(convertHashToId(thirdCharWeapons[0, 0]));
                string thirdKineticName = (JObject.Parse(thirdKineticJson))["displayProperties"]["name"].ToString();
                string KineticPerks = itemInstanceIdtoPerks(platform, membershipId, thirdCharWeapons[1, 0]);

                string thirdEnergyJson = SQLCheck.weaponLookupById(convertHashToId(thirdCharWeapons[0, 1]));
                string thirdEnergyName = (JObject.Parse(thirdEnergyJson))["displayProperties"]["name"].ToString();
                string EnergyPerks = itemInstanceIdtoPerks(platform, membershipId, thirdCharWeapons[1, 1]);

                string thirdPowerJson = SQLCheck.weaponLookupById(convertHashToId(thirdCharWeapons[0, 2]));
                string thirdPowerName = (JObject.Parse(thirdPowerJson))["displayProperties"]["name"].ToString();
                string PowerPerks = itemInstanceIdtoPerks(platform, membershipId, thirdCharWeapons[1, 2]);


                var thirdCharProperties = thirdChar.Children().First();
                string thirdCharLight = thirdCharProperties["light"].ToString();
                string thirdCharRace = convertRace(thirdCharProperties["raceType"].ToString());
                string thirdCharClass = convertClass(thirdCharProperties["classType"].ToString());
                string thirdCharGender = convertGender(thirdCharProperties["genderType"].ToString());
                int thirdCharTimePlayed = Int32.Parse(thirdCharProperties["minutesPlayedTotal"].ToString());
                string thirdCharEmblemPath = thirdCharProperties["emblemPath"].ToString();

                var thirdMsg = new DiscordEmbedBuilder()
                {

                    Color = DiscordColor.Blurple,
                    Title = officialName + "'s Guardians:",
                    Description = "**" + thirdCharClass + "**, " + thirdCharRace + " " + thirdCharGender,

                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = "https://www.bungie.net/" + thirdCharEmblemPath
                    }

                };

                thirdMsg.AddField("Light Level: ", thirdCharLight, true);
                thirdMsg.AddField("Hours Played: ", (thirdCharTimePlayed / 60).ToString(), true);
                thirdMsg.AddField("Equipped Weapons:\n" + thirdKineticName, KineticPerks);
                thirdMsg.AddField(thirdEnergyName, EnergyPerks);
                thirdMsg.AddField(thirdPowerName, PowerPerks);

                await ctx.RespondAsync(thirdMsg);
            }
        }

    }
}
