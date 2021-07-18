using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LinqFirstTry {    
    public class IndicatorNode {        
        public string IndicatorCode { get; set; }
        public string IndicatorName { get; set; }
        public string Language { get; set; }
    }
    public class IndicatorRoot {
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
            //string airPoll = "Ambient air pollution attributable deaths";
            string nameExact = "Reported number of people receiving antiretroviral therapy";
            client.BaseAddress = new Uri(@"https://ghoapi.azureedge.net/api/");
            string indicatorPath = @"Indicator?$filter=IndicatorName eq '" + nameExact + "'";
            
            try {
                //// Get the indicator id
                List<IndicatorNode> Indicators = await GetIndicatorAsync(indicatorPath);
                List<string> indicatorIds = GetIndicatorIds(Indicators);

                // Get country codes for Americas
                string ccPath = @"DIMENSION/COUNTRY/DimensionValues";
                string region = "Americas";
                ccPath = ccPath + "?$filter=ParentTitle eq '"+ region + "'";
                List<string> cCodes = await GetFilteredCountryCodes(ccPath);

                // Get indicator data for matching countries only          
                //https://ghoapi.azureedge.net/api/WHOSIS_000001
                JArray filteredByCountry = new JArray();
                foreach (string id in indicatorIds) {
                    // Filter to country values, excluding global and regional
                    string query = id + "?$filter=SpatialDimType eq 'COUNTRY'";
                    string resp = await GetIndicatorData(query);
                    JObject o = JObject.Parse(resp);
                    //IList<JObject> allRows = o["value"].Select(t => (JObject)t).ToList();
                    JArray jarr = (JArray)o["value"];
                    // Match to country list
                    foreach (JObject jo in jarr) {
                        string cc = jo["SpatialDim"].ToString();
                        if (cCodes.Contains(cc)) {
                            filteredByCountry.Add(jo);
                        }                        
                    }                    
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
        static async Task<List<string>> GetFilteredCountryCodes(string query) {
            List<string> cCodes = new List<string>();
            //Root root = null;
            HttpResponseMessage response = await client.GetAsync(query);
            if (response.IsSuccessStatusCode) {
                string resp = await response.Content.ReadAsStringAsync();                
                JObject o = JObject.Parse(resp);
                foreach (dynamic v in o.SelectToken("value")) {
                    if(v.Title!= "SPATIAL_SYNONYM") {
                        string c = v.Code;
                        cCodes.Add(c); 
                    }                    
                }
                //IList<JObject> americasRows = o["Code"].Select(t => (JObject)t).ToList();                
            }
            return cCodes;
        }
        static async Task<List<IndicatorNode>> GetIndicatorAsync(string query) {
            List<IndicatorNode> Indicators = null;
            //Root root = null;
            HttpResponseMessage response = await client.GetAsync(query);
            if (response.IsSuccessStatusCode) {
                IndicatorRoot root = await response.Content.ReadAsAsync<IndicatorRoot>();
                Indicators = root.value;
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
        static async Task<string> GetIndicatorData(string query) {
            string resp = null;

            HttpResponseMessage response = await client.GetAsync(query);
            if (response.IsSuccessStatusCode) {
                resp = await response.Content.ReadAsStringAsync();
            }
            return resp;
        }
    }
}
