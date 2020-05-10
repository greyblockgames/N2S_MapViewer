using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;
using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;
using ConsoleToolkit.ConsoleIO;
using FlatBuffers;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace N2SMap_Viewer
{
    [Command]
    [Description("Extracts the map into a .Json file.")]
    public class ExtractCommand
    {
        [Positional]
        [Description("The name of the map to be processed.")]
        public string InputFile { set; get; }


        [Positional]
        [Description("The name of the .JSON to be output.")]
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

                if (File.Exists($"{OutputFile}.json"))
                {
                    if (!console.Confirm("Output file exists... Would you like to overwrite it?".Red()))
                    {
                        return;
                    }
                }
            }


            console.WrapLine("Reading map file...");
            var bytes = File.ReadAllBytes(InputFile);

            console.WrapLine("Decompressing map...");
            byte[] lengthBuffer = new byte[4];
            Array.Copy(bytes, bytes.Length - 4, lengthBuffer, 0, 4);
            int uncompressedSize = BitConverter.ToInt32(lengthBuffer, 0);

            var buffer = new byte[uncompressedSize];
            using (var ms = new MemoryStream(bytes))
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    gzip.Read(buffer, 0, uncompressedSize);
                }
            }

            ByteBuffer bb = new ByteBuffer(buffer);


            if (!N2S.FileFormat.Map.MapBufferHasIdentifier(bb))
            {
                if (!console.Confirm(
                    "The input map might be corrupted or not be in the correct format, would you like to continue?"
                        .Red()))
                {
                    return;
                }
            }

            console.WrapLine("Unpacking map...");
            var data = N2S.FileFormat.Map.GetRootAsMap(bb).UnPack();



         
                console.WrapLine("Serialized into JSON...");
                string output = JsonConvert.SerializeObject(data, Formatting.Indented);



                if (Xml)
                {
                    console.WrapLine("Converting into XML");
                    XmlDocument xmlDocument = (XmlDocument) JsonConvert.DeserializeXmlNode(output);

                    xmlDocument.Save($"{OutputFile}.xml");
                    
                    console.WrapLine($"Complete! File written to {OutputFile}.xml");
                }
                else
                {
                    File.WriteAllText($"{OutputFile}.json", output);
                    console.WrapLine($"Complete! File written to {OutputFile}.json");
                }





        }
    }
}