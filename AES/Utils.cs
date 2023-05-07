namespace AES
{
    public static class Utils
    {
        public static byte SumGF8(byte lhs, byte rhs)
        {
            return (byte)(lhs ^ rhs);
        }
    }
}
