using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace AzureTranslatorConsoleApp
{
    class VTTTranslator
    {
        // Source file path
        public string SourceFilePath;

        // Target file path
        public string TargetFilePath;

        // From Language Code (ja = Japanese)
        public string FromLanguage;

        // To Language Code (en = English)
        public string ToLanguage;

        // Azure Cognitive Services Translation Key
        public string Key;

        public VTTTranslator()
        {
        }

        public void Translate()
        {
            Console.Write($"\rReading Source: {SourceFilePath}...");

            // Reads all lines into the array we would be processing/translating
            string[] lines = System.IO.File.ReadAllLines(SourceFilePath);

            // This array will hold the translated lines to be flushed to the output file
            string[] translated = lines;

            // This variable will hold the interim translation as received from the service
            string translated_text;

            Console.WriteLine("Done.");

            char[] computeString = { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };

            // Translate the line/dialogue and update the translated array
            var authTokenSource = new AzureAuthToken(Key);
            var token = string.Empty;

            // Start iterating each line in the VTT file format
            for (int counter=0;counter<lines.Length;counter++)
            {
                if (lines[counter].Length > 0)
                {
                    // Start processing text after the time indicator
                    if (lines[counter][2] == ':')
                    {
                        while (true)
                        {
                            counter++;

                            if (counter >= lines.Length)
                                break;
                            else
                            {
                              

                                

                                // Every token has a timeout of 10 mins, so lets generate a new token for each iteration
                                token = authTokenSource.GetAccessToken();

                                // translation code - call the service endpoint using Http Binding
                                {
                                    var result = string.Empty;

                                    var uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" +
                                        System.Web.HttpUtility.UrlEncode(lines[counter]) +
                                        $"&from={FromLanguage}&to={ToLanguage}&category=generalnn";

                                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                                    httpWebRequest.Headers.Add("Authorization", token);
                                    WebResponse response = null;

                                    using (response = httpWebRequest.GetResponse())
                                    using (Stream stream = response.GetResponseStream())
                                    {
                                        System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                                        result = (string)dcs.ReadObject(stream);
                                    }

                                    translated_text = result;
                                }

                                translated[counter] = translated_text;

                                // Provide running update of the line being processed
                                Console.Write($"\rTranslating [{new string(computeString)}] {counter} of {lines.Length}");
                                
                                computeString[(counter * 20)/lines.Length] = 'o';
                            }

                            if (counter < lines.Length - 1)
                            {
                                if (lines[counter + 1].Length > 0)
                                {
                                    if (lines[counter + 1][2] == ':')
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            Console.Write($"\rTranslating [oooooooooooooooooooo] Done.              ");

            Console.Write($"\n\rWriting target: {TargetFilePath}...");

            // Flush the translated array into the new file
            System.IO.File.WriteAllLines(TargetFilePath, translated);

            Console.WriteLine("Done.");
        }
    }
}
