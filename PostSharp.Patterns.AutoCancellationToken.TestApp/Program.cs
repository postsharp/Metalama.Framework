using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PostSharp.Patterns.AutoCancellationToken.TestApp
{
    class Program
    {
        static async Task Main()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
            try
            {
                await C.MakeRequests(cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    [AutoCancellationToken]
    class C
    {
        public static async Task MakeRequests(CancellationToken ct)
        {
            using var client = new HttpClient();
            await MakeRequest(client);
            Console.WriteLine("request 1 succeeded");
            await MakeRequest(client);
            Console.WriteLine("request 2 succeeded");
        }

        private static async Task MakeRequest(HttpClient client) => await client.GetAsync("https://httpbin.org/delay/5");
    }
}