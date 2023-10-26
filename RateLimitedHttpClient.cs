using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace protoType
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    internal class RateLimitedHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore;
        private readonly int _requestsPerMinute;

        public RateLimitedHttpClient(int requestsPerMinute)
        {
            _httpClient = TOR.CreateHttpClientWithTor();
            _semaphore = new SemaphoreSlim(requestsPerMinute, requestsPerMinute);
            _requestsPerMinute = requestsPerMinute;
        }

        public async Task<string> GetStringAsync(string request)
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _httpClient.GetStringAsync(request);
            }
            finally
            {
                await Task.Delay(TimeSpan.FromMinutes(1d / _requestsPerMinute));
                _semaphore.Release();
            }
        }
    }
}
