using System;
using TempApp;
using System.Collections.Concurrent;
using blazorlisteapp.Pages;
using System.Data.SQLite;
using System.Data.SqlClient;

namespace blazorlisteapp.Data
{
    public static class HolidaysDatabase
    {

        //On MAC the program needs SQLite core to work, it might try to install in the wrong place.
        //Add code to check if the code is in the right place BEFORE starting.
        private const string DatabaseFileName = "holidays.sqlite";
        private const string HolidaysTableName = "Holidays";

        private static SQLiteConnection CreateSqlLiteConnection()
        {
            string connectionString = "Data Source=" + DatabaseFileName;
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            return connection;
        }
        /// <summary>
        /// Creates the Holidays SQLite database file if it doesnt exist
        /// </summary>
        /// <returns>true if the database was created, otherwise false</returns>
        private static bool CreateDatabaseIfNotExists()
        {
            if (!File.Exists(DatabaseFileName))
            {
                SQLiteConnection.CreateFile(DatabaseFileName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a table with the specified name and columns if it was not found in the database
        /// </summary>
        /// <param name="tableName">The name of the table to create</param>
        /// <param name="connection">the SQLite database connection to use</param>
        /// <param name="columnDefinitions">Full inner parenthesies column definitions. Ex. "Name NVARCHAR(MAX)"</param>
        /// <returns>true if the table was created, otherwise false</returns>
        public static bool CreateTableIfNotExists(string tableName, params string[] columnDefinitions)
        {
            SQLiteConnection connection = CreateSqlLiteConnection();
            connection.Open();
            bool result = CreateTableIfNotExists(tableName, connection, columnDefinitions);
            connection.Close();
            return result;
        }

        /// <summary>
        /// Creates a table with the specified name and columns if it was not found in the database
        /// </summary>
        /// <param name="tableName">The name of the table to create</param>
        /// <param name="connection">the SQLite database connection to use</param>
        /// <param name="columnDefinitions">Full inner parenthesies column definitions. Ex. "Name NVARCHAR(MAX)"</param>
        /// <returns>true if the table was created, otherwise false</returns>
        public static bool CreateTableIfNotExists(string tableName, SQLiteConnection connection, params string[] columnDefinitions)
        {
            string query = @"CREATE TABLE IF NOT EXISTS " + tableName + @"(" + string.Join(", ", columnDefinitions) + ")";
            return RunNonDataReturningQuery(query, connection) > 0;
        }

        public static int RunNonDataReturningQuery(string queryString)
        {
            SQLiteConnection connection = CreateSqlLiteConnection();
            connection.Open();
            var result = RunNonDataReturningQuery(queryString, connection);
            connection.Close();
            return result;
        }

        public static int RunNonDataReturningQuery(string queryString, SQLiteConnection connection)
        {
            SQLiteCommand command = new SQLiteCommand(queryString, connection);
            var result = command.ExecuteNonQuery();
            return result;
        }

        public static bool EnsureDatabaseStructure()
        {
            bool CreatedDataBase = CreateDatabaseIfNotExists();
            bool CreatedHolidaysTable = CreateTableIfNotExists(HolidaysTableName, "Name VARCHAR(65535)", "Date DATE", "NationalHoliday BIT");
            return CreatedDataBase && CreatedHolidaysTable;
        }

        public static void SaveHolidayToDatabase(Holiday holiday)
        {
            using (SQLiteConnection connection = CreateSqlLiteConnection())
            {
                connection.Open();
                SaveHolidayToDatabase(holiday, connection);
                connection.Close();
            }
        }
        public static void SaveHolidayToDatabase(Holiday holiday, SQLiteConnection connection)
        {
            string SqlValuesString = ""
                + "'" + holiday.Name + "'"
                + ", "
                + "'" + holiday.Date.ToString("yyyyMMdd") + "'"
                + ", "
                + (holiday.NationalHoliday ? "1" : "0");
            string queryText = @"INSERT INTO " + HolidaysTableName + @"(Name, Date, NationalHoliday) values (" + SqlValuesString + @")";
            RunNonDataReturningQuery(queryText, connection);
        }

        public static IEnumerable<Holiday> LoadHolidays(DateTime startDate, DateTime endDate)
        {
            SQLiteConnection connection = CreateSqlLiteConnection();
            connection.Open();
            var result = LoadHolidays(startDate, endDate, connection);
            connection.Close();
            return result;
        }

        public static IEnumerable<Holiday> LoadHolidays(DateTime startDate, DateTime endDate, SQLiteConnection connection)
        { // Here i am doing the lazy thing and just checking the date time with C#, should probably be incorperated into the SQL query intstead
            string queryString = "SELECT * FROM " + HolidaysTableName;
            if (connection.State == System.Data.ConnectionState.Closed) { connection.Open(); }
            SQLiteCommand command = new SQLiteCommand(queryString, connection);
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    DateTime date = reader.GetDateTime(1);
                    bool nationalHoliday = reader.GetBoolean(2);
                    yield return new Holiday { Date = date, Name = name, NationalHoliday = nationalHoliday };
                }
            }

        }
    }
}

