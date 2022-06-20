using System.Data.SQLite;

namespace DiscordBot

{
    class SQLCheck
    {
        public SQLiteConnection myConnection;

        public SQLCheck(string database)
        {
            myConnection = new SQLiteConnection("Data Source=" + @database);
        }

        public void OpenConnection()
        {
            if (myConnection.State != System.Data.ConnectionState.Open)
            {
                myConnection.Open();
            }
        }

        public void CloseConnection()
        {
            if (myConnection.State != System.Data.ConnectionState.Closed)
            {
                myConnection.Close();
            }
        }
    }
}
