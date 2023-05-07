namespace AES
{
    public static class Utils
    {
        //-------------------------------------------------------------------------------------------------------------
        // Main utils
        //-------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Finds the sum of polynomial lhs and rhs in Galua field 256
        /// </summary>
        public static byte SumGF256(byte lhs, byte rhs)
        {
            return (byte)(lhs ^ rhs);
        }

        /// <summary>
        /// Multiple bytes by GF256 polynom modulo
        /// </summary>
        public static byte MultipleGF256(byte lhs, byte rhs)
        {
            uint multipleResult = Multiple(lhs, rhs);

            byte result = 0;
            result = DevideGF256(multipleResult, Constants.modGF256, out var reminder);

            return result;
        }

        public static byte DevideGF256(uint number, uint devider, out byte reminder)
        {
            byte result = 0;
            reminder = (byte)number;
            while (number >= devider)
            {
                var higherIndex = GetHigherBitIndex(number);
                var devideIndex = higherIndex - Constants.byteLength + 1;
                if (devideIndex < 0)
                    devideIndex = 0;
                var leftPart = (byte)GetLeftPart(number, devideIndex) ^ devider;
                var rightPart = GetRightPart(number, devideIndex);
                result = (byte)leftPart;
                reminder = (byte)rightPart;
                number = leftPart | rightPart;
            }
            return result;
        }

        //-------------------------------------------------------------------------------------------------------------
        // Other utils
        //-------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Get position of highest "1"
        /// </summary>
        public static int GetHigherBitIndex(uint number)
        {
            for (int i = Constants.intMaxLength - 1; i >= 0; --i)
            {
                if ((number & (1 << i)) > 0)
                    return i;
            }
            return 0;
        }

        /// <summary>
        /// Get int part of number: [intmaxIndex; index]
        /// </summary>
        public static uint GetLeftPart(uint number, int index)
        {
            return number >> index;
        }

        /// <summary>
        /// Get int part of number: [index; 0]
        /// </summary>
        public static uint GetRightPart(uint number, int index)
        {
            var shift = Constants.intMaxLength - index;
            return (number << shift) >> shift;
        }

        /// <summary>
        /// Multiple bytes without overflow protection
        /// </summary>
        public static uint Multiple(byte lhs, byte rhs)
        {
            byte result = 0;
            for (byte i = 0; i < Constants.byteLength; ++i)
            {
                for (byte j = 0; j < Constants.byteLength; ++j)
                {
                    result ^= (byte)(GetBitByIndex(lhs, i) * GetBitByIndex(rhs, j));
                }
            }
            return result;
        }

        /// <summary>
        /// Get mask for index position in byte
        /// </summary>
        public static byte GetMaskForIndex(byte index)
        {
            if (index < 0 || index > Constants.byteLength)
                throw new ArgumentException(string.Format("Invalid index value! Valid value is from 0 to {0}",
                    Constants.byteLength));
            return (byte)(1 << index);
        }

        /// <summary>
        /// Get bit value by index from number
        /// </summary>
        public static byte GetBitByIndex(byte number, byte index)
        {
            return (byte)(number & GetMaskForIndex(index));
        }
    }
}
