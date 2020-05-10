using ConsoleToolkit.CommandLineInterpretation.ConfigurationAttributes;

namespace N2SMap_Viewer
{
    [Command]
    [Description("Display command line help.")]
    public class HelpCommand
    {
        [Positional(DefaultValue = null)] public string Topic { get; set; }
    }
}