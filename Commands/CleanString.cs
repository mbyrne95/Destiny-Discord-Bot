using System;
using System.Text.RegularExpressions;

namespace DiscordBot
{
    public class CleanString
    {
        public static string ReplaceHashtag(string strIn)
        {
            return Regex.Replace(strIn, "[#]", "%23");
        }
    }
}