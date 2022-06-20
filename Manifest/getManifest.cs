﻿using Newtonsoft.Json.Linq;
using System;
using System.IO.Compression;

namespace DiscordBot
{
    public class getManifest
    {
        public string? database;

        public async void start()
        {
            string zipPath = @"C:\Users\matth\Downloads\world_sql_content.zip";
            string unzippedFolder = @"C:\Users\matth\Downloads\test";
            String[] fileName = Directory.GetFiles(unzippedFolder);

            if(fileName.Length > 0)
            {
                database = Directory.GetFiles(unzippedFolder)[0];
                return;
            }

            //Console.WriteLine("hi");
            HttpClient client = new HttpClient();
            string apiKey = System.IO.File.ReadAllText("apiKey.txt");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            string url = "https://www.bungie.net/Platform/Destiny2/Manifest/";

            var request = client.GetAsync(url).Result;
            var result = request.Content.ReadAsStringAsync().Result;
            var o = JObject.Parse(result);
            var response = o.Children().First().Children().First()["mobileWorldContentPaths"]["en"];

            string manifestURL = "https://www.bungie.net" + response.ToString();

            var manifestRequest = await client.GetAsync(manifestURL);

            using (var fs = new FileStream(zipPath, FileMode.CreateNew))
            {
                await manifestRequest.Content.CopyToAsync(fs);
            }

            ZipFile.ExtractToDirectory(zipPath, unzippedFolder);

            File.Move(fileName[0].ToString(), Path.ChangeExtension(fileName[0].ToString(), ".sqlite3"));

            database = Directory.GetFiles(unzippedFolder)[0];

        }
    }
}
