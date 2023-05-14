namespace AES
{
    public static class Constants
    {
        public static readonly AESParameters aes128 = new(128, 10);

        public static readonly AESParameters aes192 = new(192, 12);

        public static readonly AESParameters aes256 = new(256, 14);
    }

    /// <summary>
    /// Available keys length for usage
    /// </summary>
    public static class AvailableKeysLength
    {
        public const int key128 = 128;
        public const int key192 = 192;
        public const int key256 = 256;
    }
}
