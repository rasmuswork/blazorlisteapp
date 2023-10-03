using System;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;

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
        public static IEnumerable<Holiday> GetHolidays(DateTime startDate, DateTime endDate, string APIKey)
        {

            string responseValue = GetHolidaysJsonString(startDate, endDate, APIKey);
            Holiday[] holidays = JsonSerializer.Deserialize<Holiday[]>(responseValue);

            return holidays;
        }


        private static ConcurrentDictionary<string, string> URLDataCache = new();
        public static string GetHolidaysJsonString(DateTime startDate, DateTime endDate, string APIKey)
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

    }
}


