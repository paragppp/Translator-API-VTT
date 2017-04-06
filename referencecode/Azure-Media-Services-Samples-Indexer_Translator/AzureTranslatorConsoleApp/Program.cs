using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace AzureTranslatorConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:\n\nAzureTranslatorConsoleApp <source VTT file> <target VTT file>\neg. AzureTranslatorConsoleApp source_japanese.vtt target_english.vtt\n\n");
            }
            else
            {
                VTTTranslator translator = new VTTTranslator();

                translator.FromLanguage = "ja";
                translator.ToLanguage = "en";
                translator.SourceFilePath = args[0];
                translator.TargetFilePath = args[1];

                translator.Key = ConfigurationManager.AppSettings["MicrosoftTranslatorKey"];

                translator.Translate();
            }
        }
    }
}
