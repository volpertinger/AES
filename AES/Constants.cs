namespace AES
{
    public static class Constants
    {
        /// <summary>
        /// byte length in bits
        /// </summary>
        public static readonly int byteLength = 8;

        /// <summary>
        /// int length in bits
        /// </summary>
        public static readonly int intMaxLength = 32;

        public static readonly AESParameters aes128 = new(128, 10);

        public static readonly AESParameters aes192 = new(192, 12);

        public static readonly AESParameters aes256 = new(256, 14);

        public static readonly uint modGF256 = 283; // x^8 + x^4 + x^3 + x + 1
    }

    /// <summary>
    /// 
    /// </summary>
    public class AESParameters
    {
        /// <summary>
        /// key length in bits
        /// </summary>
        public int KeyBits { get; private init; }

        /// <summary>
        /// rounds amount
        /// </summary>
        public int RoundsAmount { get; private init; }

        /// <summary>
        /// block lenght similar for every AES
        /// </summary>
        public static int BlockLength { get; }

        public AESParameters(int keyBits, int roundsAmount)
        {
            KeyBits = keyBits;
            RoundsAmount = roundsAmount;
        }
    }

    /// <summary>
    /// Available keys length for usage
    /// </summary>
    public enum AvailableKeysLength : int
    {
        key128 = 128,
        key192 = 192,
        key256 = 256
    }
}
