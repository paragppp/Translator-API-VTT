using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AzureMediaIndexer
{

    class MicrosoftTranslator
    {
        public string from = "ja";  // deault
        public string to = "en";
        private const int charactorLimit = 10000;
        private const int arrayLimit = 2000;

        private AzureAuthToken azureToken;

        public MicrosoftTranslator(string Key)
        {
            azureToken = new AzureAuthToken(Key);
        }


        public string Translate(string original)
        {
            var result = string.Empty;
            var uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" +
                System.Web.HttpUtility.UrlEncode(original) +
                $"&from={from}&to={to}&category=generalnn";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", azureToken.GetAccessToken());
            WebResponse response = null;
            using (response = httpWebRequest.GetResponse()) 
            using (Stream stream = response.GetResponseStream())
            {
                System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                result = (string)dcs.ReadObject(stream);
            }
            return result;

        }

        public string[] TranslateArray(string[] original)
        {
            if (original.Length > arrayLimit) throw new ArgumentException("The maximum number of array elements is 2000.");

            int CharactorCount = 0;
            foreach (var item in original)
            {
                CharactorCount += item.Length;
            }
            if (CharactorCount > charactorLimit) throw new ArgumentException("The total of all texts to be translated must not exceed 10000 characters.");

            List<string> result = new List<string>();
            var uri = "http://api.microsofttranslator.com/v2/Http.svc/TranslateArray";
            var texts = original.JoinString("",
                v => $"<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">{v}</string>");

            var parameter = "<TranslateArrayRequest>" +
                                    "<AppId />" +
                                    $"<From>{from}</From>" +
                                    $"<Texts>{texts}</Texts>"+
                                    $"<To>{to}</To>" +
                                "</TranslateArrayRequest>";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", azureToken.GetAccessToken());
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "text/xml";

            using (var bodyStream = httpWebRequest.GetRequestStream())
            {
                byte[] arrBytes = System.Text.Encoding.UTF8.GetBytes(parameter);
                bodyStream.Write(arrBytes, 0, arrBytes.Length);
            }   

            WebResponse response = null;
            using (response = httpWebRequest.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader rdr = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    var responseData = rdr.ReadToEnd();
                    XDocument doc = XDocument.Parse(responseData);
                    XNamespace ns = "http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2";
                    foreach (XElement xe in doc.Descendants(ns + "TranslateArrayResponse"))
                    {
                        foreach (var node in xe.Elements(ns + "TranslatedText"))
                        {
                            result.Add(node.Value.ToString());
                        }
                    }

                }

            }
            return result.ToArray<string>();

        }

    }

    public static class IEnumerableUtil
    {
        public static string JoinString<T>(this IEnumerable<T> values, string glue, Func<T, string> converter = null)
        {
            if (converter != null)
                return string.Join(glue, values.Select(converter));
            else
                return string.Join(glue, values);
        }
    }
}
