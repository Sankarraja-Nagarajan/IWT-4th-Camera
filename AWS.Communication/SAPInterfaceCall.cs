using AWS.Communication.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Communication
{
    public class SAPInterfaceCall
    {
        private readonly string url;
        private readonly string username;
        private readonly string password;
        private readonly HttpClient httpClient;

        public SAPInterfaceCall()
        {
            url = ConfigurationManager.AppSettings["SAP_API_Address"]; ;
            username = ConfigurationManager.AppSettings["SAP_Username"];
            password = ConfigurationManager.AppSettings["SAP_Password"];
            httpClient = new HttpClient();
        }

        public async Task<string> PostGrossWeight(GrossWeight grossWeight)
        {
            try
            {
                var authenticationString = $"{username}:{password}";
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(url + "api/grossweight"),
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(grossWeight), Encoding.UTF8, "application/json")
                };

                var response = await httpClient.SendAsync(request);

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return ($@"success-{responseString}");
                }
                else
                {
                    return ($@"failed-{responseString}");
                }
            }
            catch (Exception ex)
            {
                return ("failed-" + ex.Message);
            }
        }

        public async Task<string> PostTareWeight(TareWeight tareWeight)
        {
            try
            {
                var authenticationString = $"{username}:{password}";
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(url + "api/tareweight"),
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(tareWeight), Encoding.UTF8, "application/json")
                };

                var response = await httpClient.SendAsync(request);

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return ($@"success-{responseString}");
                }
                else
                {
                    return ($@"failed-{responseString}");
                }
            }
            catch (Exception ex)
            {
                return ("failed-" + ex.Message);
            }
        }
    }
}
