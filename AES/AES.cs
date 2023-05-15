namespace AES
{
    // TODO: Blocks chain
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

            public static State operator +(State lhs, State rhs)
            {
                var result = new State();
                for (int i = 0; i < rowColLength; ++i)
                {
                    for (int j = 0; j < rowColLength; ++j)
                    {
                        result[i, j] = (byte)(lhs[i, j] ^ rhs[i, j]);
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

        private State[] ExtendedKey { get; set; }

        private string BlockChainMethod { get; set; }

        private uint BatchSize { get; set; }

        /// <summary>
        /// Block length = 16 bytes as in documentation
        /// </summary>
        public static readonly int blockLength = 16;

        /// <summary>
        /// Byte length in bits
        /// </summary>
        public static readonly int byteLength = 8;

        /// <summary>
        /// Uint length in bits
        /// </summary>
        public static readonly int uintLength = 32;

        public static readonly uint byteMask = 0b00000000_00000000_00000000_11111111;


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


        // ------------------------------------------------------------------------------------------------------------
        // Public
        // ------------------------------------------------------------------------------------------------------------

        public AES(int seed, AESParameters parameters, byte[] key, string blockChain, uint batchSize)
        {
            ForwardSBox = new SubstitutionBox(seed);
            InverseSBox = new SubstitutionBox(ForwardSBox);
            InverseSBox.Inverse();

            ExtendedKey = KeyExtension(parameters, key);
            BlockChainMethod = blockChain;
            BatchSize = batchSize;
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

        public byte[] EncryptBlock(byte[] block)
        {
            var state = new State(block);
            state += ExtendedKey[0];
            for (int i = 1; i < ExtendedKey.Length - 1; ++i)
            {
                state = ForwardBytesSubstitution(state);
                state = ForwardShiftRows(state);
                state = ForwardMixColumns(state);
                state += ExtendedKey[i];
            }
            state = ForwardBytesSubstitution(state);
            state = ForwardShiftRows(state);
            state += ExtendedKey[ExtendedKey.Length - 1];
            return state.ToPlainBytes();
        }


        public byte[] DecryptBlock(byte[] block)
        {
            var state = new State(block);
            state += ExtendedKey[ExtendedKey.Length - 1];
            for (int i = ExtendedKey.Length - 2; i > 0; --i)
            {
                state = InverseShiftRows(state);
                state = InverseBytesSubstitution(state);
                state += ExtendedKey[i];
                state = InverseMixColumns(state);
            }
            state = InverseShiftRows(state);
            state = InverseBytesSubstitution(state);
            state += ExtendedKey[0];
            return state.ToPlainBytes();
        }

        public bool Encrypt(FileStream ifs, FileStream ofs)
        {
            return CryptProcessing(ifs, ofs, true);
        }

        public bool Decrypt(FileStream ifs, FileStream ofs)
        {
            return CryptProcessing(ifs, ofs, false);
        }

        // ------------------------------------------------------------------------------------------------------------
        // Private
        // ------------------------------------------------------------------------------------------------------------

        private bool CryptProcessing(FileStream ifs, FileStream ofs, bool encrypt)
        {
            return BlockChainMethod switch
            {
                BlockChainModes.ECB => CryptProcessingECB(ifs, ofs, encrypt),
                BlockChainModes.CBC => CryptProcessingCBC(ifs, ofs, encrypt),
                BlockChainModes.OFB => CryptProcessingOFB(ifs, ofs, encrypt),
                BlockChainModes.CFB => CryptProcessingCFB(ifs, ofs, encrypt),
                _ => throw new ArgumentException(),
            };
        }

        private bool CryptProcessingECB(FileStream ifs, FileStream ofs, bool encrypt)
        {
            var buffer = new byte[blockLength * BatchSize];
            int length;
            while ((length = ifs.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Need to know if there is less data than the buffer can hold
                int realBlocksIndex = 0;
                for (; realBlocksIndex < BatchSize; ++realBlocksIndex)
                {
                    var block = new byte[blockLength];
                    for (int j = 0; j < blockLength; ++j)
                    {
                        block[j] = buffer[realBlocksIndex * blockLength + j];
                    }

                    if (encrypt)
                        block = EncryptBlock(block);
                    else
                        block = DecryptBlock(block);
                    for (int j = 0; j < blockLength; ++j)
                    {
                        buffer[realBlocksIndex * blockLength + j] = block[j];
                    }
                    if ((realBlocksIndex + 1) * blockLength >= length)
                        break;
                }
                ofs.Write(buffer, 0, (realBlocksIndex + 1) * blockLength);
                buffer = new byte[blockLength * BatchSize];
            }
            return true;
        }

        private bool CryptProcessingCBC(FileStream ifs, FileStream ofs, bool encrypt)
        {
            var buffer = new byte[blockLength * BatchSize];
            int length;
            var chain = new byte[blockLength];
            while ((length = ifs.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Need to know if there is less data than the buffer can hold
                int realBlocksIndex = 0;
                for (; realBlocksIndex < BatchSize; ++realBlocksIndex)
                {
                    var block = new byte[blockLength];
                    for (int j = 0; j < blockLength; ++j)
                    {
                        block[j] = buffer[realBlocksIndex * blockLength + j];
                    }
                    if (encrypt)
                    {
                        block = Chain(block, chain);
                        block = EncryptBlock(block);
                        chain = block;
                    }
                    else
                    {
                        var tmp = block;
                        block = DecryptBlock(block);
                        block = Chain(block, chain);
                        chain = tmp;
                    }
                    for (int j = 0; j < blockLength; ++j)
                    {
                        buffer[realBlocksIndex * blockLength + j] = block[j];
                    }
                    if ((realBlocksIndex + 1) * blockLength >= length)
                        break;
                }
                ofs.Write(buffer, 0, (realBlocksIndex + 1) * blockLength);
                buffer = new byte[blockLength * BatchSize];
            }
            return true;
        }

        // TODO
        private bool CryptProcessingOFB(FileStream ifs, FileStream ofs, bool encrypt)
        {
            var buffer = new byte[blockLength * BatchSize];
            int length;
            while ((length = ifs.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Need to know if there is less data than the buffer can hold
                int realBlocksIndex = 0;
                for (; realBlocksIndex < BatchSize; ++realBlocksIndex)
                {
                    var block = new byte[blockLength];
                    for (int j = 0; j < blockLength; ++j)
                    {
                        block[j] = buffer[realBlocksIndex * blockLength + j];
                    }

                    var encrypted = new byte[blockLength];
                    if (encrypt)
                        encrypted = EncryptBlock(block);
                    else
                        encrypted = DecryptBlock(block);
                    for (int j = 0; j < blockLength; ++j)
                    {
                        buffer[realBlocksIndex * blockLength + j] = encrypted[j];
                    }
                    if ((realBlocksIndex + 1) * blockLength >= length)
                        break;
                }
                ofs.Write(buffer, 0, (realBlocksIndex + 1) * blockLength);
                buffer = new byte[blockLength * BatchSize];
            }
            return true;
        }

        // TODO
        private bool CryptProcessingCFB(FileStream ifs, FileStream ofs, bool encrypt)
        {
            var buffer = new byte[blockLength * BatchSize];
            int length;
            while ((length = ifs.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Need to know if there is less data than the buffer can hold
                int realBlocksIndex = 0;
                for (; realBlocksIndex < BatchSize; ++realBlocksIndex)
                {
                    var block = new byte[blockLength];
                    for (int j = 0; j < blockLength; ++j)
                    {
                        block[j] = buffer[realBlocksIndex * blockLength + j];
                    }

                    var encrypted = new byte[blockLength];
                    if (encrypt)
                        encrypted = EncryptBlock(block);
                    else
                        encrypted = DecryptBlock(block);
                    for (int j = 0; j < blockLength; ++j)
                    {
                        buffer[realBlocksIndex * blockLength + j] = encrypted[j];
                    }
                    if ((realBlocksIndex + 1) * blockLength >= length)
                        break;
                }
                ofs.Write(buffer, 0, (realBlocksIndex + 1) * blockLength);
                buffer = new byte[blockLength * BatchSize];
            }
            return true;
        }

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

        private State[] KeyExtension(AESParameters parameters, byte[] key)
        {
            if (key.Length * byteLength != parameters.KeyBits)
                throw new ArgumentException();

            var groups = new uint[(parameters.RoundsAmount + 1) * State.rowColLength];
            var groupLength = parameters.KeyBits / (byteLength * State.rowColLength);

            // initial group filling
            for (int i = 0; i < groupLength; ++i)
            {
                var group = new byte[State.rowColLength];
                for (int j = 0; j < State.rowColLength; ++j)
                {
                    group[j] = key[i * State.rowColLength + j];
                }
                groups[i] = BytesToUint(group);
            }

            for (int i = 1; i <= (groups.Length - 1) / groupLength; ++i)
            {
                groups[i * groupLength] = groups[(i - 1) * groupLength] ^ KeyExtensionFirstGroupProcessing(groups[i * groupLength - 1], i);
                for (int j = 1; j < groupLength; ++j)
                {
                    // overflow check when filling incomplete blocks
                    if (i * groupLength + j < groups.Length)
                        groups[i * groupLength + j] = groups[i * groupLength + j - 1] ^ groups[(i - 1) * groupLength + j];
                }
            }

            var result = new State[parameters.RoundsAmount + 1];
            for (int i = 0; i <= parameters.RoundsAmount; ++i)
            {
                var stateBytes = new byte[blockLength];
                for (int j = 0; j < State.rowColLength; ++j)
                {
                    var group = UintToBytes(groups[i * State.rowColLength + j]);
                    for (int k = 0; k < State.rowColLength; ++k)
                    {
                        stateBytes[j * State.rowColLength + k] = group[k];
                    }
                }
                result[i] = new State(stateBytes);
            }

            return result;
        }

        /// <summary>
        /// First word in each round key changes according to the documentation
        /// </summary>
        private uint KeyExtensionFirstGroupProcessing(uint arg, int round)
        {
            var block = UintToBytes(RotateWord(arg));
            for (int i = 0; i < State.rowColLength; ++i)
            {
                block[i] = ForwardSBox[block[i]];
            }
            block[0] = (byte)(block[0] ^ (1 << round));
            return BytesToUint(block);
        }

        private static byte[] UintToBytes(uint arg)
        {
            var result = new byte[State.rowColLength];
            for (int i = 0; i < State.rowColLength; ++i)
            {
                result[i] = (byte)(arg & byteMask);
                arg >>= byteLength;
            }
            return result;
        }

        private static uint BytesToUint(byte[] arg)
        {
            if (arg.Length != State.rowColLength)
                throw new ArgumentException();
            uint result = 0;
            for (int i = 0; i < State.rowColLength; ++i)
            {
                result <<= byteLength;
                result += arg[i];
            }
            return result;
        }

        private static uint RotateWord(uint arg)
        {
            return (arg << byteLength) | (arg >> (uintLength - byteLength));
        }

        private static byte[] Chain(byte[] lhs, byte[] rhs)
        {
            for (int i = 0; i < lhs.Length && i < rhs.Length; ++i)
            {
                lhs[i] ^= rhs[i];
            }
            return lhs;
        }
    }

    /// <summary>
    /// Parameters for AES crypt processing
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

        public AESParameters(int keyBits, int roundsAmount)
        {
            KeyBits = keyBits;
            RoundsAmount = roundsAmount;
        }
    }
}
