using System;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;
using blazorlisteapp.Data;

namespace TempApp
{

    public sealed class Holiday
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        [JsonPropertyName("nationalHoliday")]
        public bool NationalHoliday { get; set; }
    }

    public static class HolidayGetter
    {
        private const string APIKey = "ea28738f-971f-4791-a682-4d14ba6b0ddd"; // very bad form, should be in some form of config file, but i can't be fucked
        public static IEnumerable<Holiday> GetHolidays(DateTime startDate, DateTime endDate)
        {
            IEnumerable<Holiday> GetFromWeb()
            {
                foreach (Holiday holiday in GetHolidays_FromWeb(startDate, endDate))
                {
                    HolidaysDatabase.SaveHolidayToDatabase(holiday);
                    yield return holiday;

                }
            }

            IEnumerable<Holiday> GetFromDB()
            {
                Holiday[] DBHolidays = HolidaysDatabase.LoadHolidays(startDate, endDate).ToArray();
                if(DBHolidays.Length > 0)
                {
                    return DBHolidays;
                }
                else
                {
                    return GetFromWeb();
                }
            }

            bool DatabaseNotNew = HolidaysDatabase.EnsureDatabaseStructure();
            if (DatabaseNotNew) { return GetFromDB(); }
            else { return GetFromWeb(); }
        }
        private static IEnumerable<Holiday> GetHolidays_FromWeb(DateTime startDate, DateTime endDate)
        {
            string responseValue = GetHolidaysJsonString(startDate, endDate);
            Holiday[] holidays = JsonSerializer.Deserialize<Holiday[]>(responseValue);
            return holidays;
        }
        private static ConcurrentDictionary<string, string> URLDataCache = new();
        public static string GetHolidaysJsonString(DateTime startDate, DateTime endDate)
        {
            const string baseURL = @"https://api.sallinggroup.com/v1/holidays?";
            const string dateTimeFormatString = "yyyy-MM-dd";

            string startDateString = startDate.ToString(dateTimeFormatString, System.Globalization.CultureInfo.InvariantCulture);
            string endDateString = endDate.ToString(dateTimeFormatString, System.Globalization.CultureInfo.InvariantCulture);
            string URL = baseURL
                + "startDate=" + startDateString
                + '&'
                + "endDate=" + endDateString
                + '&'
                + "translation=da-dk"
                ;

            if (!URLDataCache.ContainsKey(URL))
            {
                WebRequest request = WebRequest.Create(URL);

                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", "Bearer " + APIKey);
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(dataStream);
                string responseValue = streamReader.ReadToEnd();
                URLDataCache[URL] = responseValue;
                response.Close();
                dataStream.Close();
                streamReader.Close();
            }


            return URLDataCache[URL];
        }

        //private static IEnumerable<Holiday> GetHolidays_FromDB(DateTime startDate, DateTime endDate)
        //{
        //    HolidaysDatabase.EnsureDatabaseStructure();
        //    return HolidaysDatabase.LoadHolidays(startDate, endDate);
        //}
    }
}


