using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using Newtonsoft.Json;

using Amazon.Lambda.Core;

namespace LAFitnessClubs
{
    public class ClassesData
    {
        //Construct JSON


        //LAFitness URL
        string baseURL = "https://www.lafitness.com/pages/findclubresultszip.aspx?state=&zipCode=";
        string baseClassScheduleURL = "https://www.lafitness.com/pages/ClassSchedulePrintVersion.aspx?clubid=";

        //Variables
        HttpClient http = new HttpClient();
        List<string> Days = new List<string>() { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public string Execute(ILambdaContext context, string state, string zipcode)
        {
            StringBuilder toReturn = new StringBuilder();
            List<Club> Clubs = getClubs(state, int.Parse(zipcode));   
            foreach (Club c in Clubs)
            {
                context.Logger.Log(c.ToString());
                string URL = baseClassScheduleURL.Replace("clubid=", string.Format("clubid={0}", c.clubID));
                string clubName = c.clubID.Split('&')[1].Replace('+', ' ');

                //Classes
                context.Logger.Log("Before GetClassScheduleData: " + URL);
                List<Class> _classes = GetClassScheduleData(URL);

                //Convert to JSON
                string json = JsonConvert.SerializeObject(new resultJson {
                    clubName = clubName,
                    clubID = c.clubID.Split('&')[0],
                    clubScheduleURL = URL,
                    classes = GetClassScheduleData(URL)
                });
                toReturn.Append(json);
                context.Logger.Log("After GetClassScheduleData: ");
            }

            //foreach (Class c in ClassesList)
             //   msg.Append(c.ToString()).Append(Environment.NewLine);

            //Return
            return toReturn.ToString();
        }
        internal class resultJson
        {
            public string clubName;
            public string clubID;
            public string clubScheduleURL;
            public List<Class> classes;
        }

        //Get data
        private List<Class> GetClassScheduleData(string URL)
        {
            try
            {
                //Html doc
                var doc = new HtmlDocument();
                Task<string> dURL = ReadData(URL);
                doc.LoadHtml(dURL.Result);
                
                //List of classes
                List<Class> ClassesList = new List<Class>();

                var table = doc.GetElementbyId("tblSchedule");

                foreach (var tr in table.SelectSingleNode("tbody").SelectNodes("tr"))
                {
                    string time = tr.SelectSingleNode("th").SelectSingleNode("h5").InnerText;
                    string className = string.Empty;
                    int currDayInt = 0;
                    foreach (var td in tr.SelectNodes("td"))
                    {
                        string currDay = Days[currDayInt];
                        if (td.HasChildNodes)
                            className = td.InnerText;
                        else
                            className = "No Class";
                        ClassesList.Add(new Class(time, currDay, className));
                        currDayInt += 1;
                    }
                }
                return ClassesList;
            }
            catch (Exception e) { throw new Exception(e.Message); }
        }

        internal class Class
        {
            public string day { get; set; }
            public string time { get; set; }
            public string className { get; set; }

            public Class(string time, string day, string className)
            {
                this.time = time;
                this.day = day;
                this.className = className;
            }

            //Override to string method
            public override string ToString()
            {
                return string.Format("{0}: {1} - {2}", this.time, this.day, this.className);
            }
        }

        //Get club ID's
        internal List<Club> getClubs(string State, int ZipCode)
        {
            List<Club> toReturn = new List<Club>();
            //Construct URL
            string URL = baseURL.Replace("state=", string.Format("state={0}", State))
                .Replace("zipCode=", string.Format("zipCode={0}", ZipCode));

            //Get HTML document
            var doc = new HtmlDocument();
            Task<string> dURL = ReadData(URL);
            doc.LoadHtml(dURL.Result);
            var allClubs = doc.DocumentNode.SelectNodes("//*[contains(@class,'TextDataColumn')]");
            foreach (var td in allClubs)
            {
                if (td.InnerText.Trim() != "Club")
                {
                    string clubURI = td.SelectSingleNode(".//a").GetAttributeValue("href", "Club Link not found");
                    string clubID = clubURI.Split('=')[1];
                    Club club = new Club(clubURI, clubID);
                    toReturn.Add(club);
                }
            }

            //Return
            return toReturn;
        }
        internal class Club
        {
            public string clubURI;
            public string clubID;

            public Club(string clubURI, string clubID)
            {
                this.clubURI = clubURI;
                this.clubID = clubID;
            }

            public override string ToString()
            {
                return string.Format("{0}, {1}", this.clubID, this.clubURI);
            }
        }
        private async Task<string> ReadData(string URL)
        {
            try
            {
                WebRequest request = WebRequest.Create(URL);
                WebResponse response = await request.GetResponseAsync();
                Stream data = response.GetResponseStream();
                string html = String.Empty;
                using (StreamReader sr = new StreamReader(data))
                {
                    html = sr.ReadToEnd();
                }
                return html;
            }
            catch (Exception e) { throw new Exception(e.Message); }
        }
    }
}
