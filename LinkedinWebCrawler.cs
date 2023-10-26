using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace protoType
{
    internal class LinkedinWebCrawler
    {

        public static async Task getJobTrendsAsync(string keywords, string location)
        {
            var httpClient = new RateLimitedHttpClient(5);

            var start = 0;
            var maxResults = 25;
            var baseSearchUrl = "https://www.linkedin.com/jobs-guest/jobs/api/seeMoreJobPostings/search?";
            var linksFilter = "/jobs/view/";
            var timeLimit = DateTime.Now.AddMinutes(15);

            // get all job links
            var allLinks = new List<string>();
            var linksOnThisRequest = new List<string>();

            do
            {
                var searchTerms = $"keywords={keywords}&location={location}&refresh=true&start={start}";
                var html = await httpClient.GetStringAsync(baseSearchUrl + searchTerms);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                linksOnThisRequest = getJobLinks(htmlDocument, linksFilter);

                allLinks.AddRange(linksOnThisRequest);

                start += 25;
                linksOnThisRequest.Clear();

            } while (DateTime.Now < timeLimit && allLinks.Count() < maxResults && linksOnThisRequest.Count() > 0);

            // get all job info

            var allJobs = new List<Job>();

            foreach (var link in allLinks)
            {
                var job = await GetJobInfo(link, httpClient);

                allJobs.Add(job);
            }

            SaveJobsToFile(allJobs, "jobs.json");
        }
        static void SaveJobsToFile(List<Job> jobs, string filePath)
        {
            string json = JsonConvert.SerializeObject(jobs, Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(filePath, json);
        }
        private static async Task<Job> GetJobInfo(string link, RateLimitedHttpClient httpClient)
        {
            var html = await httpClient.GetStringAsync(link);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var jobTitle = htmlDocument.DocumentNode.SelectSingleNode("//*[contains(@class, 'job-title')]").InnerText;

            var jobDescription = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='job-details']").InnerText;

            var job = new Job();

            job.title = jobTitle;

            job.description = jobDescription;

            return job;
        }

        private static void ParseHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection names = doc.DocumentNode.SelectNodes("//a/h2");

            HtmlNodeCollection prices = doc.DocumentNode.SelectNodes("//div/main/ul/li/a/span");

            for (int i = 0; i < names.Count; i++)
            {
                Console.WriteLine("Name: {0}, Price: {1}", names[i].InnerText, prices[i].InnerText);
            }
        }

        private static List<string> getJobLinks(HtmlDocument htmlDocument, string filter)
        {
            var links = new List<string>();

            var anchorNodes = htmlDocument.DocumentNode.SelectNodes("//a[@href]");
            if (anchorNodes != null)
            {
                foreach (var anchorNode in anchorNodes)
                {
                    var href = anchorNode.GetAttributeValue("href", "");
                    if (href.Contains(filter))
                    {
                        links.Add(href);
                    }
                }
            }

            return links;
        }
    }
}
