using System.Data.SQLite;

namespace DiscordBot

{
    static class SQLCheck
    {

        static string database;

        static SQLCheck()
        {
            database = getManifest.database;
        }

        //get weapon ID (CONVERT HASH FIRST), return json from manifest with associated data
        public static string weaponLookupById(int id)
        {
            SQLiteConnection myConnection = new SQLiteConnection("Data Source=" + @database);

            myConnection.Open();

            SQLiteCommand cmd = new SQLiteCommand(myConnection);
            cmd.CommandText = "SELECT id, json FROM DestinyInventoryItemDefinition WHERE id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Prepare();
            string json = "";

            using SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                json += ($"{reader.GetString(1)}");
            }

            reader.Close();
            myConnection.Close();

            return json;
        }

        public static string perkLookupById(int id)
        {
            SQLiteConnection myConnection = new SQLiteConnection("Data Source=" + @database);

            myConnection.Open();

            SQLiteCommand cmd = new SQLiteCommand(myConnection);
            cmd.CommandText = "SELECT id, json FROM DestinySandboxPerkDefinition WHERE id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Prepare();
            string json = "";

            using SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                json += ($"{reader.GetString(1)}");
            }

            reader.Close();
            myConnection.Close();

            return json;
        }
    }
}
