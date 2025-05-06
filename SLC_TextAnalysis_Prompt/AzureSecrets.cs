using System.IO;
using Newtonsoft.Json;

namespace TextAnalysis
{
    public class AzureSecrets
    {
        public string DocumentIntelligenceEndpoint { get; set; }

        public string DocumentIntelligenceKey { get; set; }

        public string AzureOpenAIEndpoint { get; set; }

        public string AzureOpenAIKey { get; set; }

        public string ModelDeploymentName { get; set; }

        public static AzureSecrets GetUserSecrets()
        {
            using (StreamReader r = new StreamReader("C:\\Skyline DataMiner\\AI-sample\\secrets\\secrets.json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<AzureSecrets>(json);
            }
        }
    }
}
