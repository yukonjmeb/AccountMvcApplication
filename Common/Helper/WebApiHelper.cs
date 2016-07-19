using Common.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class WebApiHelper : IWebApiHelper
    {
        public string PostData(Method method, object PostData, string targetUrl , string ContentType = "application/x-www-form-urlencoded", int CodePage = 65001)
        {
            if (string.IsNullOrEmpty(targetUrl)) throw new ArgumentException("目標位置網址不可為空或 NULL");

            string postString = ObjectParserHelper.ObjectToPostString(PostData);

            if (method == Method.GET || method == Method.DELETE)
            {
                targetUrl = string.Format("{0}?{1}", targetUrl, postString);
            }

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(targetUrl);
            webRequest.Method = method.ToString();
            webRequest.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(ContentType))
            {
                webRequest.ContentType = ContentType;
            }
            //SSL不對也不管
            ServicePointManager.ServerCertificateValidationCallback +=
            (object s, X509Certificate certificate,
                     X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            { return true; };


            if (!String.IsNullOrEmpty(postString) && (method == Method.POST || method == Method.PUT))
            {
                var parameterString = Encoding.GetEncoding(CodePage).GetBytes(postString);
                webRequest.ContentLength = parameterString.Length;

                using (var buffer = webRequest.GetRequestStream())
                {
                    buffer.Write(parameterString, 0, parameterString.Length);
                    buffer.Close();
                }
            }
            HttpWebResponse webResponse;
            string JsonResponse;
            try
            {
                webResponse = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException ex)
            {
                throw new WebException(string.Format("連線到{0} 發生錯誤:{1}", targetUrl, ex.Message), ex.InnerException, ex.Status, ex.Response);
            }

            using (var streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.GetEncoding(CodePage)))
            {
                JsonResponse = streamReader.ReadToEnd();
                streamReader.Close();
                streamReader.Dispose();

            }
            return JsonResponse;
        }

        public string getData(string action, string value) { 

            //http://localhost:1322/api/Account/

            if (string.IsNullOrEmpty(action)) throw new ArgumentException("傳入的action不可為 NULL");

            string targetUrl = "http://localhost:1322/api/Account";

            if (action != "") {
                targetUrl = string.Format("{0}/{1}/{2}", targetUrl, action, value);
            }

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(targetUrl);
            webRequest.Method = "Get";
            webRequest.AllowAutoRedirect = false;
            string ContentType = "application/x-www-form-urlencoded";
            int CodePage = 65001;

            if (!string.IsNullOrEmpty(ContentType))
            {
                webRequest.ContentType = ContentType;
            }
            //SSL不對也不管
            ServicePointManager.ServerCertificateValidationCallback +=
            (object s, X509Certificate certificate,
                     X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            { return true; };


            HttpWebResponse webResponse;
            string JsonResponse;

            try
            {
                webResponse = (HttpWebResponse)webRequest.GetResponse();

            }
            catch (WebException ex)
            {
                throw new WebException(string.Format("連線到{0} 發生錯誤:{1}", targetUrl, ex.Message), ex.InnerException, ex.Status, ex.Response);
            }

            using (var streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.GetEncoding(CodePage)))
            {
                JsonResponse = streamReader.ReadToEnd();
                streamReader.Close();
                streamReader.Dispose();

            }

            return JsonResponse;
        }

    }
}
