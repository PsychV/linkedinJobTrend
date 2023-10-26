using HtmlAgilityPack;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace protoType
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var keywords = ".NET";
            var location = "barcelona";

            await LinkedinWebCrawler.getJobTrendsAsync(keywords, location);
        }
    }
}