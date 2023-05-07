using System.Numerics;

namespace AES
{
    public static class Utils
    {
        /// <summary>
        /// Finds the sum of lhs and rhs in Galua field 256
        /// </summary>
        public static byte SumGF256(byte lhs, byte rhs)
        {
            return (byte)(lhs ^ rhs);
        }

        /// <summary>
        /// Euclidean algorithm
        /// </summary>
        /// <returns>GCD of lhs and rhs</returns>
        public static byte GCD(byte lhs, byte rhs)
        {
            if (lhs == 0)
                return rhs;
            if (rhs == 0)
                return lhs;
            while (lhs != 0 && rhs != 0)
            {
                if (lhs > rhs)
                    lhs %= rhs;
                else
                    rhs %= lhs;
            }
            if (lhs > rhs)
                return lhs;
            return rhs;
        }
    }
}
