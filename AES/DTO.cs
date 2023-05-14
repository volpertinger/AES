#pragma warning disable CS8618

namespace AES
{
    public class Settings
    {
        public int KeyLength { get; set; }
        public string Key { get; set; }
        public string BlockChainMode { get; set; }
        public AtomicOperation[] Operations { get; set; }
    }

    public class AtomicOperation
    {
        public string PathInput { get; set; }
        public string PathOutput { get; set; }
        public string Operation { get; set; }
    }
    public static class Operations
    {
        public const string Encrypt = "Encrypt";
        public const string Decrypt = "Decrypt";
    }

    public static class BlockChainModes
    {
        public const string ECB = "ECB";
        public const string CBC = "CBC";
        public const string OFB = "OFB";
        public const string CFB = "CFB";
    }


}
#pragma warning restore CS8618
