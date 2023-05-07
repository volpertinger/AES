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

        /// <summary>
        /// Devide uint numbers in GF256 polynim modulo
        /// </summary>
        public static byte DevideGF256(uint number, uint divider, out byte reminder)
        {
            var higherBit = GetHigherBitIndex(number);
            var higherBitDivider = GetHigherBitIndex(divider);
            var result = 0;
            reminder = 0;
            while (higherBit >= higherBitDivider && higherBit > 0)
            {
                var leftPart = GetLeftPart(number, higherBit - higherBitDivider) ^ divider;
                var rightPart = GetRightPart(number, higherBit - higherBitDivider - 1);
                var delta = higherBitDivider - GetHigherBitIndex(leftPart);
                reminder = (byte)leftPart;
                result <<= delta;
                number = leftPart | rightPart;
                ++result;
                higherBit = GetHigherBitIndex(number);
            }
            return (byte)result;
        }

        //-------------------------------------------------------------------------------------------------------------
        // Other utils
        //-------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns maximum number (that is a power of 2) that an input number is divisible by
        /// </summary>
        public static uint MaxNumberPower2(uint number)
        {
            if (number == 0)
                return Constants.intMaxLength;
            return number & ~(number - 1);
        }

        /// <summary>
        /// Get position of highest "1"
        /// </summary>
        public static byte GetHigherBitIndex(uint number)
        {
            for (var i = (byte)(Constants.intMaxLength - 1); i > 0; --i)
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
            if (index < 0)
                index = 0;
            if (index >= Constants.intMaxLength)
                return 0;
            return number >> index;
        }

        // TODO: маска после сдвигов
        /// <summary>
        /// Get int part of number: [index; 0]
        /// </summary>
        public static uint GetRightPart(uint number, int index)
        {
            if (index < 0)
                index = 0; ;
            int shift = Constants.intMaxLength - index - 1;
            if (shift < 0)
                shift = 0;
            number <<= shift;
            return number >> shift;
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
        public static uint GetBitByIndex(uint number, byte index)
        {
            return number & GetMaskForIndex(index);
        }
    }
}
