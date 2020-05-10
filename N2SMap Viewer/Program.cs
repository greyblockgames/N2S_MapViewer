using System;
using System.IO;
using ConsoleToolkit;
using ConsoleToolkit.ApplicationStyles;
using FlatBuffers;

namespace N2SMap_Viewer
{
    internal class Program : CommandDrivenApplication
    {
        public static void Main(string[] args)
        {
            Toolkit.Execute<Program>(args);
        }
        
        protected override void OnCommandLineValid(object command)
        {
            //Initialisation code here
            base.OnCommandLineValid(command);
        }
        
        
        protected override void Initialise()
        {
            base.HelpCommand<HelpCommand>(c => c.Topic);
            base.Initialise();
        }
        
    }
}