using NLog;
using NLog.Config;
using NLog.Targets;

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

        return Success;
    }
}
