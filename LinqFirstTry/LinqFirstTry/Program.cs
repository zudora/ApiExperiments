using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace LinqFirstTry {
    public class IndicatorNode {
        public string IndicatorCode { get; set; }
        public string IndicatorName { get; set; }
        public string Language { get; set; }
    }
    public class Root {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }
        public List<IndicatorNode> value { get; set; }
    }
    class Program {
        static HttpClient client = new HttpClient();
        static void Main(string[] args) {           
            RunAsync().GetAwaiter().GetResult();           
        }
        
        static async Task RunAsync() {
            string airPoll = "Ambient air pollution attributable deaths";
            string nameExact = "Reported number of people receiving antiretroviral therapy";
            client.BaseAddress = new Uri("https://ghoapi.azureedge.net/api/Indicator?$filter=IndicatorName eq '"+ airPoll +"'");
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(
            //    new MediaTypeWithQualityHeaderValue("application/json"));

            try {
                //// Get the indicator id
                List<IndicatorNode> Indicators = await GetIndicatorAsync(client.BaseAddress);
                List<string> indicatorIds = GetIndicatorIds(Indicators);
                foreach(string id in indicatorIds) {

                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
        static async Task<List<IndicatorNode>> GetIndicatorAsync(Uri requestUri) {
            List<IndicatorNode> Indicators = null;
            Root root = null;
            HttpResponseMessage response = await client.GetAsync(requestUri);
            if (response.IsSuccessStatusCode) {
                root = await response.Content.ReadAsAsync<Root>();
                
                Indicators = root.value;
                //string str = await response.Content.ReadAsStringAsync();
            }
            return Indicators;
        }
        static List<string> GetIndicatorIds(List<IndicatorNode> nodeList) {
            List<string> idList = new List<string>();
            foreach (IndicatorNode node in nodeList) {
                if (node.Language == "EN") {
                    string id = node.IndicatorCode;
                    if (null != id && !idList.Contains(id)) {
                        idList.Add(id);
                    }
                }
            }
            return idList;
        }
    }
}
