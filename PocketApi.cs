using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JekyllLinkPost
{
    class PocketApi
    {
        private static readonly string baseUrl = "https://getpocket.com";
        private static readonly string requestUrl = baseUrl + "/v3/oauth/request";
        private static readonly string authorizeUrl = baseUrl + "/v3/oauth/authorize";
        private static readonly string retrieveUrl = baseUrl + "/v3/get";
        private static readonly string modifyUrl = baseUrl + "/v3/send";
        private static readonly string redirectUri = "http://localhost:3000";
        private static readonly string consumerKey = "29634-aef89930278d84ef982300d6";

        private class RequestTokenRequest
        {
            public string consumer_key { get; set; }
            public string redirect_uri { get; set; }
        }

        private class AccessTokenRequest
        {
            public string consumer_key { get; set; }
            public string code { get; set; }
        }

        private class RetrieveRequest
        {
            public string consumer_key { get; set; }
            public string access_token { get; set; }
            public string detailType { get; set; }
            public int count { get; set; }
            public string tag { get; set; }
        }

        private class ModifyRequestItem
        {
            public string action { get; set; }
            public string item_id { get; set; }
        }

        private class ModifyRequest
        {
            public string consumer_key { get; set; }
            public string access_token { get; set; }
            public ModifyRequestItem[] actions { get; set; }

            public static ModifyRequest Create(IEnumerable<PocketItem> items, string updateAction, string accessToken, string consumerKey)
            {
                List<ModifyRequestItem> modifyItems = new List<ModifyRequestItem>();
                foreach (PocketItem item in items)
                {
                    ModifyRequestItem modifyItem = new ModifyRequestItem()
                    {
                        action = updateAction,
                        item_id = item.Id.ToString()
                    };
                    modifyItems.Add(modifyItem);
                }

                ModifyRequest request = new ModifyRequest()
                {
                    consumer_key = consumerKey,
                    access_token = accessToken,
                    actions = modifyItems.ToArray()
                };
                return request;
            }
        }

        private string _accessToken;

        public bool LoginToPocket()
        {
            string requestToken = GetRequestToken();

            TimeSpan timeout = new TimeSpan(0, 15, 0);
            string stuff = GetCodeFromLocalHost(requestToken, timeout);

            _accessToken = GetAccessToken(requestToken);
            return _accessToken != string.Empty;
        }

        public IEnumerable<PocketItem> RetreiveItems(string tagToRetrieve)
        {
            IEnumerable<PocketItem> result = null;

            try
            {
                RetrieveRequest requestParams = new RetrieveRequest()
                {
                    consumer_key = consumerKey,
                    access_token = _accessToken,
                    detailType = "complete",
                    tag = tagToRetrieve
                };

                result = RetreiveItems(requestParams);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw ex;
            }

            return result;
        }

        public IEnumerable<PocketItem> RetreiveItems(int count)
        {
            IEnumerable<PocketItem> result = null;

            try
            {
                RetrieveRequest requestParams = new RetrieveRequest()
                {
                    consumer_key = consumerKey,
                    access_token = _accessToken,
                    detailType = "complete",
                    count = count
                };

                result = RetreiveItems(requestParams);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw ex;
            }

            return result;
        }

        public void ArchivePocketItems(IEnumerable<PocketItem> items)
        {
            ModifyRequest request = ModifyRequest.Create(items, "archive", _accessToken, consumerKey);
            UpdateItems(request);
        }

        public void DeletePocketItems(IEnumerable<PocketItem> items)
        {
            ModifyRequest request = ModifyRequest.Create(items, "delete", _accessToken, consumerKey);
            UpdateItems(request);
        }

        private IEnumerable<PocketItem> RetreiveItems(RetrieveRequest request)
        {
            List<PocketItem> result = new List<PocketItem>();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Accept", "application/json");

            HttpResponseMessage response = client.PostAsJsonAsync<RetrieveRequest>(retrieveUrl, request).Result;
            response.EnsureSuccessStatusCode();

            string responseContent = response.Content.ReadAsStringAsync().Result;
            System.Diagnostics.Debug.WriteLine(responseContent);
            var resultSet = JObject.Parse(responseContent);
            var itemList = resultSet["list"];
            foreach (var item in itemList)
            {
                PocketItem pocketItem = CreatePocketItem(item.First);
                result.Add(pocketItem);
            }

            return result;
        }

        private void UpdateItems(ModifyRequest request)
        {

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Accept", "application/json");
            HttpResponseMessage response = client.PostAsJsonAsync<ModifyRequest>(modifyUrl, request).Result;
            response.EnsureSuccessStatusCode();
        }

        private string GetRequestToken()
        {
            string result = "";

            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("X-Accept", "application/json");


                RequestTokenRequest authInfo = new RequestTokenRequest()
                {
                    consumer_key = consumerKey,
                    redirect_uri = redirectUri
                };

                HttpResponseMessage response = client.PostAsJsonAsync<RequestTokenRequest>(requestUrl, authInfo).Result;
                response.EnsureSuccessStatusCode();

                string responseContent = response.Content.ReadAsStringAsync().Result;
                var requestTokenCode = JObject.Parse(responseContent);
                result = requestTokenCode["code"].ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw ex;
            }

            return result;
        }

        private string GetCodeFromLocalHost(string requestToken, TimeSpan timeout)
        {
            const string httpTemporaryListenAddresses = "http://+:3000/";

            string code = "";
            using (var listener = new HttpListener())
            {
                string localHostUrl = string.Format(httpTemporaryListenAddresses);

                listener.Prefixes.Add(localHostUrl);
                listener.Start();

                string pocketAuthUrl = string.Format("{0}/auth/authorize?request_token={1}&redirect_uri={2}", baseUrl, requestToken, redirectUri);
                using (Process.Start(pocketAuthUrl))
                {
                    while (true)
                    {
                        var start = DateTime.Now;
                        var context = listener.GetContext();
                        var usedTime = DateTime.Now.Subtract(start);
                        timeout = timeout.Subtract(usedTime);

                        if (context.Request.Url.AbsolutePath == "/")
                        {
                            context.Response.Close();
                            break;
                        }

                        context.Response.StatusCode = 404;
                        context.Response.Close();
                    }
                }
            }
            return code;
        }

        private string GetAccessToken(string requestToken)
        {
            string result = "";

            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("X-Accept", "application/json");


                AccessTokenRequest tokenInfo = new AccessTokenRequest()
                {
                    consumer_key = consumerKey,
                    code = requestToken
                };

                HttpResponseMessage response = client.PostAsJsonAsync<AccessTokenRequest>(authorizeUrl, tokenInfo).Result;
                response.EnsureSuccessStatusCode();

                string responseContent = response.Content.ReadAsStringAsync().Result;
                var accessTokenResponse = JObject.Parse(responseContent);
                result = accessTokenResponse["access_token"].ToString(); 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw ex;
            }

            return result;
        }

        private PocketItem CreatePocketItem(JToken item)
        {
            PocketItem pocketItem = new PocketItem();

            pocketItem.Id = Convert.ToInt32(item["item_id"]);
            pocketItem.Tags = GetTags(item);
            pocketItem.Title = GetTitle(item);
            pocketItem.Url = GetUrl(item);

            return pocketItem;
        }

        private string[] GetTags(JToken item)
        {
            List<string> tags = new List<string>();
            JToken tagsToken = item["tags"];
            foreach (JToken token in tagsToken)
            {
                JToken innerToken = token.First();
                string tag = innerToken["tag"].ToString();
                if (tag != "blog")
                {
                    tags.Add(tag);
                }
            }
            return tags.ToArray<string>();
        }

        private string GetTitle(JToken item)
        {
            JToken title;
            if (item["resolved_title"] == null)
            {
                title = item["given_title"];
            }
            else
            {
                title = item["resolved_title"];
            }
            return title.ToString();
        }
        private string GetUrl(JToken item)
        {
            JToken url;
            if (item["resolved_url"] == null)
            {
                url = item["given_url"];
            }
            else
            {
                url = item["resolved_url"];
            }
            return url.ToString();
        }
    }
}
