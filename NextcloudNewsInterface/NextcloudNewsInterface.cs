using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace NextcloudNewsInterface
{
    public class NextcloudNewsInterface
    {
        private const String url_base_string = "index.php/apps/news/api/v1-2/";
        private String host;
        private String username;
        private String password;
        private NextcloudNewsFeeds feedCache = null;
        private NextcloudNewsItems itemCache = null;
        private static NextcloudNewsInterface instance = null;

        public NextcloudNewsInterface(String host, String username, String password)
        {
            if (!host.EndsWith("/"))
                host = host + "/";
            this.host = host;
            this.username = username;
            this.password = password;
        }

        public static NextcloudNewsInterface getInstance(String host = null, String username = null, String password = null)
        {
            if(instance == null)
            {
                if(host == null || username == null || password == null)
                {
                    throw new Exception("NextcloudNewsInterface parameters not valid.");
                }
                instance = new NextcloudNewsInterface(host, username, password);
            }
            return instance;
        }

        private static void setHeader(HttpWebRequest request, string header, string value)
        {
            //Stupid work around for user-agent not being exposed in the PCL version of HttpWebRequest
            //---STUPID!---
            PropertyInfo propertyInfo = request.GetType().GetRuntimeProperty(header.Replace("-", string.Empty));
            if (propertyInfo != null)
                propertyInfo.SetValue(request, value, null);
            else
                request.Headers[header] = value;
        }

        private String buildFullURL(String method)
        {
            return String.Format("{0}{1}{2}", this.host, url_base_string, method);
        }

        public async Task<NextcloudNewsItems> getItems(Boolean getRead = false, int batchSize = -1, int offset = 0)
        {
            if(itemCache != null)
            {
                return itemCache;
            }
            var getReadString = "false";
            if (getRead)
                getReadString = "true";
            String url = String.Format(buildFullURL("items")+"?batchSize={0}&offset={1}&getRead={2}", batchSize, offset, getReadString);
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            String hash = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            request.Headers["Authorization"] = "Basic " + hash;
            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(NextcloudNewsItems));
                NextcloudNewsItems jsonResponse = jsonSerializer.ReadObject(response.GetResponseStream()) as NextcloudNewsItems;
                itemCache = jsonResponse;
                return jsonResponse;
            }
            throw new Exception("getItems failed.");
        }

        public async Task<NextcloudNewsFeeds> getFeeds()
        {
            if(feedCache == null)
            {
                HttpWebRequest request = WebRequest.Create(buildFullURL("feeds")) as HttpWebRequest;
                String hash = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                request.Headers["Authorization"] = "Basic " + hash;
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(String.Format(
                            "Server error (HTTP {0}: {1}).",
                            response.StatusCode,
                            response.StatusDescription));
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(NextcloudNewsFeeds));
                    NextcloudNewsFeeds jsonResponse = jsonSerializer.ReadObject(response.GetResponseStream()) as NextcloudNewsFeeds;
                    feedCache = jsonResponse;
                    return feedCache;
                }
                throw new Exception("getFeeds failed.");
            }
            return feedCache;
        }

        public async void markItemRead(NextcloudNewsItem item)
        {
            HttpWebRequest request = WebRequest.Create(buildFullURL("items/read/multiple")) as HttpWebRequest;
            request.Method = "PUT";
            request.ContentType = "application/json";
            String hash = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            request.Headers["Authorization"] = "Basic " + hash;
            using (var writer = new StreamWriter(await request.GetRequestStreamAsync()))
            {
                var payload = String.Format("{{\"items\": [{0}]}}", item.id);
                writer.Write(payload);
            }
            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
            }
        }
    }
}
