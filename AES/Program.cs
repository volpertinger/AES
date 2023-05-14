using AES;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Reflection;
using System.Text;
using System.Text.Json;

class Program
{
    private static readonly string logo = @"
 █████╗ ███████╗███████╗                                                                                              
██╔══██╗██╔════╝██╔════╝                                                                                              
███████║█████╗  ███████╗                                                                                              
██╔══██║██╔══╝  ╚════██║                                                                                              
██║  ██║███████╗███████║                                                                                              
╚═╝  ╚═╝╚══════╝╚══════╝                                                                                              
                                                                                                                      
██████╗ ██╗   ██╗    ██╗   ██╗ ██████╗ ██╗     ██████╗ ███████╗██████╗ ████████╗██╗███╗   ██╗ ██████╗ ███████╗██████╗ 
██╔══██╗╚██╗ ██╔╝    ██║   ██║██╔═══██╗██║     ██╔══██╗██╔════╝██╔══██╗╚══██╔══╝██║████╗  ██║██╔════╝ ██╔════╝██╔══██╗
██████╔╝ ╚████╔╝     ██║   ██║██║   ██║██║     ██████╔╝█████╗  ██████╔╝   ██║   ██║██╔██╗ ██║██║  ███╗█████╗  ██████╔╝
██╔══██╗  ╚██╔╝      ╚██╗ ██╔╝██║   ██║██║     ██╔═══╝ ██╔══╝  ██╔══██╗   ██║   ██║██║╚██╗██║██║   ██║██╔══╝  ██╔══██╗
██████╔╝   ██║        ╚████╔╝ ╚██████╔╝███████╗██║     ███████╗██║  ██║   ██║   ██║██║ ╚████║╚██████╔╝███████╗██║  ██║
╚═════╝    ╚═╝         ╚═══╝   ╚═════╝ ╚══════╝╚═╝     ╚══════╝╚═╝  ╚═╝   ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝╚═╝  ╚═╝
";

    private static LoggingConfiguration config = new LoggingConfiguration();

    private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

    private static readonly int Success = 0;

    private static readonly int SettingsError = 1;

    private static readonly int JsonFormatError = 2;
    static int Main(string[] args)
    {
        // logger settings
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, new ConsoleTarget
        {
            Name = "console",
            Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}",
        }, "*");
        LogManager.Configuration = config;

        Console.WriteLine(logo);

        Settings? settings = null;
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", @"Settings.json");
        try
        {
            string jsonString = File.ReadAllText(path);
            settings = JsonSerializer.Deserialize<Settings>(jsonString);
        }
        catch
        {
            logger.Error(String.Format("Can`t find Settings.json at path {0}", path));
            return SettingsError;
        }

        if (settings is null)
        {
            logger.Error("Invalid Settings.json file. Check Readme!");
            return JsonFormatError;
        }

        byte[] key = Encoding.UTF8.GetBytes(settings.Key);

        BlockChain chainMod = settings.BlockChainMode switch
        {
            BlockChainModes.ECB => Constants.ecb,
            BlockChainModes.CBC => Constants.cbc,
            BlockChainModes.OFB => Constants.ofb,
            BlockChainModes.CFB => Constants.cfb,
            _ => throw new ArgumentException(string.Format("Unsupported BlockChainMode {0}", settings.BlockChainMode)),
        };
        return Success;
    }
}
