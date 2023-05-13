﻿namespace AES
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

            public State()
            {
                Block = new State(new byte[] { }).Block;
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

            public PolynomialGF256[,] ToPolynomial()
            {
                var result = new PolynomialGF256[rowColLength, rowColLength];
                for (int i = 0; i < rowColLength; ++i)
                {
                    for (int j = 0; j < rowColLength; ++j)
                    {
                        result[i, j] = new PolynomialGF256(Block[i, j]);
                    }
                }
                return result;
            }

            public static State FromPolynomial(PolynomialGF256[,] arg)
            {
                var result = new State();
                for (int i = 0; i < rowColLength; ++i)
                {
                    for (int j = 0; j < rowColLength; ++j)
                    {
                        result[i, j] = (byte)arg[i, j].Coefficients;
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

        
        /// <summary>
        /// Matrix for forward mix columns as in documentation
        /// |x  |x+1|1  |1  |
        /// |1  |x  |x+1| 1 |
        /// |1  |1  |x  |x+1|
        /// |x+1|1  |1  |x  |
        /// </summary>
        private static readonly PolynomialGF256[,] forwardMixColumnsMatrix = new PolynomialGF256[,] {
            { new(2), new(3), new(1), new(1)},
            { new(1), new(2), new(3), new(1)},
            { new(1), new(1), new(2), new(3)},
            { new(3), new(1), new(1), new(2)}
        };

        /// <summary>
        /// Matrix for inverse mix columns as in documentation
        /// |x^3 + x^2 + x^1|x^3 + x + 1    |x^3 + x^2 + 1  |x^3 + 1        |
        /// |x^3 + 1        |x^3 + x^2 + x^1|x^3 + x + 1    |x^3 + x^2 + 1  |
        /// |x^3 + x^2 + 1  |x^3 + 1        |x^3 + x^2 + x^1|x^3 + x + 1    |
        /// |x^3 + x + 1    |x^3 + x^2 + 1  |x^3 + 1        |x^3 + x^2 + x^1|
        /// </summary>
        private static readonly PolynomialGF256[,] inverseMixColumnsMatrix = new PolynomialGF256[,] {
            { new(14), new(11), new(13), new(9)},
            { new(9), new(14), new(11), new(13)},
            { new(13), new(9), new(14), new(11)},
            { new(11), new(13), new(9), new(14)},
        };
        
        /// <summary>
        /// mask for get last bit with &
        /// </summary>
        private static readonly uint lastBitMask = 1;


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

        public static State ForwardShiftRows(State block)
        {
            return ShiftRows(block, false);
        }

        public static State InverseShiftRows(State block)
        {
            return ShiftRows(block, true);
        }

        public static State ForwardMixColumns(State block)
        {
            return MixColumns(block, true);
        }

        public static State InverseMixColumns(State block)
        {
            return MixColumns(block, false);
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

        private static State ShiftRows(State block, bool shiftRight)
        {
            State result = new();
            for (int i = 0; i < State.rowColLength; ++i)
            {
                for (int j = 0; j < State.rowColLength; ++j)
                {
                    if (shiftRight)
                        result[i, j] = block[i, (j - i + State.rowColLength) % State.rowColLength];
                    else
                        result[i, j] = block[i, (j + i) % State.rowColLength];
                }
            }
            return result;
        }

        private static State MixColumns(State block, bool forward)
        {
            var mixMatrix = forwardMixColumnsMatrix;
            if (!forward)
                mixMatrix = inverseMixColumnsMatrix;
            var result = new PolynomialGF256[State.rowColLength, State.rowColLength];
            var polynomialBlock = block.ToPolynomial();

            for (int i = 0; i < State.rowColLength; ++i)
            {
                for (int j = 0; j < State.rowColLength; ++j)
                {
                    var accumulator = new PolynomialGF256();
                    for (int k = 0; k < State.rowColLength; ++k)
                    {
                        accumulator += mixMatrix[i, k] * polynomialBlock[k, j];
                    }
                    result[i, j] = accumulator;
                }
            }
            return State.FromPolynomial(result);
        }
    }
}
