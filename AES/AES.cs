namespace AES
{

    public class AES
    {
        // ------------------------------------------------------------------------------------------------------------
        // Classes
        // ------------------------------------------------------------------------------------------------------------

        public class SubstitutionBox
        {
            // --------------------------------------------------------------------------------------------------------
            // Fields
            // --------------------------------------------------------------------------------------------------------

            public byte[,] Box { get; private set; }

            public byte this[byte key]
            {
                get
                {
                    return Box[GetLeftBytePart(key), GetRightBytePart(key)];
                }

                set
                {
                    Box[GetLeftBytePart(key), GetRightBytePart(key)] = value;
                }
            }

            private static readonly int length = (int)Math.Sqrt(byte.MaxValue + 1);

            private static readonly int leftByteMask = 0b00000000_00000000_00000000_11110000;

            private static readonly int rightByteMask = 0b00000000_00000000_00000000_00001111;

            private static readonly int halfByteLength = 4;

            /// <summary>
            /// x^4 + x^3 + x^2 +x + 1. As in documentation
            /// </summary>
            private static readonly PolynomialGF256 affineMultiplier = new(0b00011111);

            /// <summary>
            /// x^6 + x^5 +x + 1. As in documentation
            /// </summary>
            private static readonly PolynomialGF256 affineAddendum = new(0b01100011);

            // --------------------------------------------------------------------------------------------------------
            // Public
            // --------------------------------------------------------------------------------------------------------

            public SubstitutionBox(int seed)
            {
                Box = GetForwardSBox(seed);
            }

            public SubstitutionBox(SubstitutionBox substitutionBox)
            {
                Box = new byte[length, length];
                for (int i = 0; i < length; ++i)
                {
                    for (int j = 0; j < length; ++j)
                    {
                        Box[i, j] = substitutionBox.Box[i, j];
                    }
                }
            }

            // --------------------------------------------------------------------------------------------------------
            // Private
            // --------------------------------------------------------------------------------------------------------
            public void Inverse()
            {
                byte[,] result = new byte[length, length];
                for (int i = 0; i < length; ++i)
                {
                    for (int j = 0; j < length; ++j)
                    {
                        var forwardByte = Box[i, j];
                        result[GetLeftBytePart(forwardByte), GetRightBytePart(forwardByte)] = BytePaste(i, j);
                    }
                }
                Box = result;
            }

            private static byte[,] GetForwardSBox(int seed)
            {
                List<byte> initial = new();
                for (int i = 0; i <= byte.MaxValue; ++i)
                {
                    initial.Add((byte)i);
                }

                byte[,] result = new byte[length, length];
                var random = new Random(seed);
                for (int i = 0; i < length; ++i)
                {
                    for (int j = 0; j < length; ++j)
                    {
                        var index = random.Next(initial.Count);
                        result[i, j] = AffineTransformation(initial[index]);
                        initial.RemoveAt(index);
                    }
                }
                return result;
            }

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

        public class State
        {
            // --------------------------------------------------------------------------------------------------------
            // Fields
            // --------------------------------------------------------------------------------------------------------

            public byte[,] Block { get; private set; }

            public byte this[int i, int j]
            {
                get => Block[i, j];
                set => Block[i, j] = value;
            }

            public static readonly int rowColLength = (int)Math.Sqrt(blockLength);

            // --------------------------------------------------------------------------------------------------------
            // Public
            // --------------------------------------------------------------------------------------------------------

            public State(byte[] block)
            {
                if (block.Length > blockLength)
                    throw new IndexOutOfRangeException();

                if (block.Length < blockLength)
                {
                    var newBlock = new byte[blockLength];
                    for (int i = 0; i < block.Length; ++i)
                    {
                        newBlock[i] = block[i];
                    }
                    for (int i = block.Length; i < blockLength; ++i)
                    {
                        newBlock[i] = 0;
                    }
                    block = newBlock;
                }

                Block = new byte[rowColLength, rowColLength];
                for (int i = 0; i < rowColLength; ++i)
                {
                    for (int j = 0; j < rowColLength; ++j)
                    {
                        // filling goes by columns according to the documentation
                        Block[j, i] = block[i * rowColLength + j];
                    }
                }

            }

            public State(State state)
            {
                Block = new byte[rowColLength, rowColLength];
                for (int i = 0; i < rowColLength; ++i)
                {
                    for (int j = 0; j < rowColLength; ++j)
                    {
                        Block[i, j] = state[i, j];
                    }
                }
            }

            public byte[] ToPlainBytes()
            {
                var result = new byte[blockLength];
                for (int i = 0; i < rowColLength; ++i)
                {
                    for (int j = 0; j < rowColLength; ++j)
                    {
                        result[i * rowColLength + j] = Block[j, i];
                    }
                }
                return result;
            }

            // --------------------------------------------------------------------------------------------------------
            // Private
            // --------------------------------------------------------------------------------------------------------
        }

        // ------------------------------------------------------------------------------------------------------------
        // Fields
        // ------------------------------------------------------------------------------------------------------------

        private SubstitutionBox ForwardSBox { get; set; }

        private SubstitutionBox InverseSBox { get; set; }

        /// <summary>
        /// Block length = 16 bytes as in documentation
        /// </summary>
        public static readonly int blockLength = 16;


        // ------------------------------------------------------------------------------------------------------------
        // Public
        // ------------------------------------------------------------------------------------------------------------

        public AES(int seed)
        {
            ForwardSBox = new SubstitutionBox(seed);
            InverseSBox = new SubstitutionBox(ForwardSBox);
            InverseSBox.Inverse();
        }

        public State ForwardBytesSubstitution(State block)
        {
            return BytesSubstitution(block, ForwardSBox);
        }

        public State InverseBytesSubstitution(State block)
        {
            return BytesSubstitution(block, InverseSBox);
        }

        // ------------------------------------------------------------------------------------------------------------
        // Private
        // ------------------------------------------------------------------------------------------------------------

        private State BytesSubstitution(State block, SubstitutionBox box)
        {
            for (int i = 0; i < State.rowColLength; ++i)
            {
                for (int j = 0; j < State.rowColLength; ++j)
                {
                    block[i, j] = box[block[i, j]];
                }
            }
            return block;
        }
    }
}
