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
        private int convertItemHashToId(string hashString)
        {
            int hash = int.Parse(hashString);
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

        //using a membership id and platform, this will return a list of a players character ids
        //best used in conjunction with getDestinyPlayerInfo
        /*
        private List<string> getCharacterIds(string platformId, string membershipId)
        {
            string endpoint = "/Destiny2/" + platformId + "/Profile/" + membershipId + "/?components=200";
            var url = apiRoot + endpoint;
            addHeaders();
            var request = client.GetAsync(url).Result;
            var result = request.Content.ReadAsStringAsync().Result;

            Console.WriteLine("hello");

            dynamic json = (JObject)JsonConvert.DeserializeObject(result);

            var response = json.Children().First;

            Console.WriteLine(response.ToString());

            //var destinyPlayer = JsonConvert.DeserializeObject<ProfileResponse>(result);

            //var characterIds = destinyPlayer.Response.profile.data.characterIds;


            //Console.WriteLine("hi" + destinyPlayer.ErrorCode);

            return characterIds;
        }
        */

        [Command("overview")]
        public async Task getShit (CommandContext ctx, string platform, [RemainingText] string name)
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

            //var json = JsonConvert.DeserializeObject(result);
            var o = JObject.Parse(result);

            //first character info
            var firstChar = o.Properties().First().First().First().First().First().First().First();
            var firstCharProperties = firstChar.Children().First();
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

            firstMsg.AddField("Light Level: ", firstCharLight);
            firstMsg.AddField("Hours Played: ", (firstCharTimePlayed / 60).ToString());

            await ctx.RespondAsync(firstMsg);

            //second char
            var secondChar = firstChar.Next;

            if (secondChar != null)
            {
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

                secondMsg.AddField("Light Level: ", secondCharLight);
                secondMsg.AddField("Hours Played: ", (secondCharTimePlayed / 60).ToString());

                await ctx.RespondAsync(secondMsg);
            }

            //third char info
            var thirdChar = secondChar.Next;

            if(thirdChar != null)
            {
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

                thirdMsg.AddField("Light Level: ", thirdCharLight);
                thirdMsg.AddField("Hours Played: ", (thirdCharTimePlayed / 60).ToString());

                await ctx.RespondAsync(thirdMsg);
            }
        }

        [Command("test")]
        public async Task test(CommandContext ctx)
        {
            
        }
    }
}
