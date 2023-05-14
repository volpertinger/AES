using AES;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Reflection;
using System.Security.Cryptography;
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

        logger.Info("Get settings");
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

        logger.Info("Get key");
        byte[] key = Encoding.UTF8.GetBytes(settings.Key);

        logger.Info("Get block chain mod");
        BlockChain chainMod = settings.BlockChainMode switch
        {
            BlockChainModes.ECB => Constants.ecb,
            BlockChainModes.CBC => Constants.cbc,
            BlockChainModes.OFB => Constants.ofb,
            BlockChainModes.CFB => Constants.cfb,
            _ => throw new ArgumentException(string.Format("Unsupported block chain mode {0}", settings.BlockChainMode)),
        };

        logger.Info("Get parameters");
        AESParameters parameters = settings.KeyLength switch
        {
            AvailableKeysLength.key128 => Constants.aes128,
            AvailableKeysLength.key192 => Constants.aes192,
            AvailableKeysLength.key256 => Constants.aes256,
            _ => throw new ArgumentException(string.Format("Unsupported key length {0}", settings.KeyLength)),

        };

        logger.Info("Creating object for crypt processing");
        AES.AES aes = new(settings.SBoxSeed, parameters, key, chainMod, settings.BatchSize);

        logger.Info("Start encryption, decryption process");


        foreach (var setting in settings.Operations)
        {
            if (File.Exists(setting.PathOutput))
            {
                logger.Error(String.Format("File with path {0} Already exists!", setting.PathInput));
                continue;
            }

            using (FileStream fsi = File.OpenRead(setting.PathInput))
            {
                using (FileStream fso = File.OpenWrite(setting.PathOutput))
                {
                    switch (setting.Operation)
                    {
                        case Operations.Encrypt:
                            logger.Info(String.Format("Start file {0} encryption, writing result to {1}",
                                setting.PathInput, setting.PathOutput));
                            aes.Encrypt(fsi, fso);
                            logger.Info(String.Format("Encryption of {0} finished successfully, result wrote to {1}",
                                setting.PathInput, setting.PathOutput));
                            break;
                        case Operations.Decrypt:
                            logger.Info(String.Format("Start file {0} decryption, writing result to {1}",
                                setting.PathInput, setting.PathOutput));
                            aes.Decrypt(fsi, fso);
                            logger.Info(String.Format("Decryption of {0} finished successfully, result wrote to {1}",
                                setting.PathInput, setting.PathOutput));
                            break;
                        default:
                            logger.Error("Something went wrong. Better pray.");
                            continue;
                    };
                    fso.Close();
                }
                fsi.Close();
            }
        }

        logger.Info("Process finished successfully");
        return Success;
    }
}
