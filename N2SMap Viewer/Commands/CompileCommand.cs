using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Mime;
using System.Xml;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;
using FlatBuffers;
using N2S.FileFormat;
using Newtonsoft.Json;

namespace N2SMap_Viewer
{
    [Command]
    [Description("Compiles the map into a .N2SMAP file.")]
    public class CompileCommand
    {
        [Positional]
        [Description("The name of the file to be processed.")]
        public string InputFile { set; get; }


        [Positional]
        [Description("The name of the map to be output.")]
        public string OutputFile { set; get; }

        [Option]
        [Description("Run silently with no required input.")]
        public bool Silent { set; get; }

        [Option]
        [Description("Use XML instead of JSON.")]
        public bool Xml { set; get; }


        [CommandHandler]
        public void Handler(IConsoleAdapter console, IErrorAdapter error)
        {
            if (!Silent)
            {
                if (!File.Exists(InputFile))
                {
                    console.WrapLine("Input map file does not exist...".Red());
                    return;
                }

                if (File.Exists($"{OutputFile}.N2SMAP"))
                {
                    if (!console.Confirm("Output file exists... Would you like to overwrite it?".Red()))
                    {
                        return;
                    }
                }
            }

            string jsonText;
            if (Xml)
            {
                console.WrapLine("Loading map from XML...");
                var doc = new XmlDocument();
                doc.LoadXml(InputFile);
                jsonText = JsonConvert.SerializeXmlNode(doc);
            }
            else
            {
                console.WrapLine("Loading map from JSON...");
                jsonText = File.ReadAllText(InputFile);
            }

            var deserialized = JsonConvert.DeserializeObject<MapT>(jsonText);
            deserialized.GameVersion += ":N2SMap_Viewer";

            var fb = new FlatBufferBuilder(1);

            console.WrapLine("Packing map...");
            fb.Finish(N2S.FileFormat.Map.Pack(fb, deserialized).Value);


            var buf = fb.SizedByteArray();

            using (var outputStream = new MemoryStream())
            {
                //Here we're compressing the data to make it smaller
                console.WrapLine("Compressing map...");
                using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    gZipStream.Write(buf, 0, buf.Length);

                //Writing compressed data to a file
                console.WrapLine("Writing map to file...");
                File.WriteAllBytes(OutputFile + ".N2SMAP", outputStream.ToArray());
            }

            console.WrapLine($"Complete! File written to {OutputFile}.N2SMAP");
        }
    }
}