using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace DiscordBot
{
    class Program
    {

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            
            string db = getManifest.database;

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = System.IO.File.ReadAllText("token.txt"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            });
            
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" }
            });

            commands.RegisterCommands<PrimaryModule>();
            //commands.SetHelpFormatter<HelpFormatter>();
            
            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

    }

}