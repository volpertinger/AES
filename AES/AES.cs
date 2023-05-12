namespace AES
{
    public class AES
    {
        // ------------------------------------------------------------------------------------------------------------
        // Fields
        // ------------------------------------------------------------------------------------------------------------

        public byte[,] ForwardSBox { get; private set; }

        public byte[,] InverseSBox { get; private set; }

        private static readonly int sBoxLength = (int)Math.Sqrt(byte.MaxValue + 1);

        private static readonly int leftByteMask = 0b00000000_00000000_00000000_11110000;

        private static readonly int rightByteMask = 0b00000000_00000000_00000000_00001111;

        private static readonly int halfByteLength = 4;

        // x^4 + x^3 + x^2 +x + 1. As in documentation
        private static readonly PolynomialGF256 affineMultiplier = new(0b00011111);

        // x^6 + x^5 +x + 1. As in documentation
        private static readonly PolynomialGF256 affineAddendum = new(0b01100011);

        // ------------------------------------------------------------------------------------------------------------
        // Public
        // ------------------------------------------------------------------------------------------------------------

        public AES(int seed)
        {
            ForwardSBox = GetForwardSBox(seed);
            InverseSBox = GetInverseSBox(ForwardSBox);
        }
        public static byte[,] GetForwardSBox(int seed)
        {
            List<byte> initial = new();
            for (int i = 0; i <= byte.MaxValue; ++i)
            {
                initial.Add((byte)i);
            }

            byte[,] result = new byte[sBoxLength, sBoxLength];
            var random = new Random(seed);
            for (int i = 0; i < sBoxLength; ++i)
            {
                for (int j = 0; j < sBoxLength; ++j)
                {
                    var index = random.Next(initial.Count);
                    result[i, j] = AffineTransformation(initial[index]);
                    initial.RemoveAt(index);
                }
            }
            return result;
        }

        public static byte[,] GetInverseSBox(byte[,] forwardSBox)
        {
            byte[,] result = new byte[sBoxLength, sBoxLength];
            for (int i = 0; i < sBoxLength; ++i)
            {
                for (int j = 0; j < sBoxLength; ++j)
                {
                    var forwardByte = forwardSBox[i, j];
                    result[GetLeftBytePart(forwardByte), GetRightBytePart(forwardByte)] = BytePaste(i, j);
                }
            }
            return result;
        }


        // ------------------------------------------------------------------------------------------------------------
        // Private
        // ------------------------------------------------------------------------------------------------------------

        private static byte GetLeftBytePart(byte arg)
        {
            return (byte)((arg & leftByteMask) >> halfByteLength);
        }

        private static byte GetRightBytePart(byte arg)
        {
            return (byte)(arg & rightByteMask);
        }

        private static byte BytePaste(int left, int right)
        {
            return (byte)((left << halfByteLength) | right);
        }

        private static byte AffineTransformation(byte arg)
        {
            PolynomialGF256 polynomial = new(arg);
            return (byte)(polynomial.GetReverse() * affineMultiplier + affineAddendum).Coefficients;
        }
    }
}
