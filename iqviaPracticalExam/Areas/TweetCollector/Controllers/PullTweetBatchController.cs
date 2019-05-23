using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using iqviaPracticalExam.Areas.TweetCollector.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;

namespace iqviaPracticalExam.Areas.TweetCollector.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PullTweetBatchController : ControllerBase
    {
        //Initialize Props
        public DateTime StartDate = new DateTime();
        public DateTime IntervalledDate = new DateTime();
        public DateTime EndDate = new DateTime();

        public string StartDateString = "";
        public string IntervalledDateString = "";
        public string EndDateString = "";
        public int TweetCountDivider = 5;




        // GET: api/v1/PullTweetBatch
        [HttpGet]
        public IActionResult GetAsync()
        {
            //Set Initial Dates
            StartDate = DateTime.Parse("2016-01-01T00:00:00Z", null, DateTimeStyles.RoundtripKind);
            IntervalledDate = DateTime.Parse("2016-01-02T00:00:00Z", null, DateTimeStyles.RoundtripKind);
            EndDate = DateTime.Parse("2017-12-31T00:00:00Z", null, DateTimeStyles.RoundtripKind);

            //Set Initial Date Strings for Query
            StartDateString = StartDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
            IntervalledDateString = IntervalledDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
            EndDateString = EndDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);


            // Set up RestSharp Parameters
            var request = new RestRequest(Method.GET);
            var client = new RestClient();
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("accept-encoding", "gzip, deflate");
            request.AddHeader("Host", "badapi.iqvia.io");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            IRestResponse response = new RestResponse();


            //Set Tweet Object Container
            List<iqviaTweetObject> objects = new List<iqviaTweetObject>();


            //While Loop to Cycle through Calling Tweets.
            while (IntervalledDate <= EndDate)
            {
                
                client = new RestClient("https://badapi.iqvia.io/api/v1/Tweets?startDate=" + StartDateString + "&endDate=" + IntervalledDateString);
                response = client.Execute(request);
                objects.AddRange(JsonConvert.DeserializeObject<List<iqviaTweetObject>>(response.Content));

                //Divide Next Interval Date should count be 100 which we know to be limit number of records of GET.
                if (JsonConvert.DeserializeObject<List<iqviaTweetObject>>(response.Content).Count() == 100)
                {
                    TweetCountDivider = TweetCountDivider / 2;
                    if(TweetCountDivider == 0)
                    {
                        TweetCountDivider = 1;
                    }
                    IntervalledDate = StartDate.AddDays(TweetCountDivider);
                    IntervalledDateString = IntervalledDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
                    continue;
                }

                //Iterate TimeScale to Call Forward.
                StartDate = IntervalledDate;
                StartDateString = StartDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
                IntervalledDate = IntervalledDate.AddDays(5);
                IntervalledDateString = IntervalledDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
                TweetCountDivider = 5;

                //Time BreakPoint Used to Test Time Reduction at final limits.
                //if (IntervalledDate > EndDate)
                //{
                //    var Test = "Test";
                //}

                //Narrow End Range
                while (IntervalledDate > EndDate && StartDate < EndDate)
                {
                    TweetCountDivider = TweetCountDivider / 2;
                    if (TweetCountDivider == 0)
                    {
                        TweetCountDivider = 1;
                    }
                    IntervalledDate = IntervalledDate.AddDays(-TweetCountDivider);
                    IntervalledDateString = IntervalledDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
                    continue;
                }
            }





            //Distinct The List of Tweets.
            objects = objects.GroupBy(t => t.id).Select(g => g.First()).ToList();

            return new ObjectResult(objects);
        }

    }
}
