using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;


namespace LinqFirstTry {
    public class Indicator {
        public string IndicatorCode { get; set; }
        public string IndicatorName { get; set; }      
        public string Language { get; set; }
    }
    class Program {
        static HttpClient client = new HttpClient();
        static void Main(string[] args) {           
            RunAsync().GetAwaiter().GetResult();           
        }
        
        static async Task RunAsync() {
            // Update port # in the following line.
            string nameExact = "Reported number of people receiving antiretroviral therapy";
            client.BaseAddress = new Uri("https://ghoapi.azureedge.net/api/Indicator?$filter=IndicatorName eq '"+ nameExact +"'/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            try {
                //// Get the indicator id
                Indicator indicator = await GetIndicatorAsync(client.BaseAddress);                
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
        static async Task<Indicator> GetIndicatorAsync(Uri requestUri) {
            Indicator Indicator = null;
            HttpResponseMessage response = await client.GetAsync(requestUri);
            if (response.IsSuccessStatusCode) {
                Indicator = await response.Content.ReadAsAsync<Indicator>();
            }
            return Indicator;
        }
    }
}
