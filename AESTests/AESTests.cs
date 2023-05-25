using AESA = AES.AES;
using AESP = AES.AESParameters;
using AESS = AES.AES.State;
using AESSB = AES.AES.SubstitutionBox;

namespace AESTests
{
    [TestClass]
    public class AESTests
    {
        private static readonly int sBoxLength = (int)Math.Sqrt(byte.MaxValue + 1);

        private static readonly int sBoxElementsCount = byte.MaxValue + 1;

        private static readonly int leftByteMask = 0b00000000_00000000_00000000_11110000;

        private static readonly int rightByteMask = 0b00000000_00000000_00000000_00001111;

        private static readonly int halfByteLength = 4;

        private static readonly int blockLength = 16;

        private static readonly int blockRowColLength = 4;

        private static readonly AESP aes128 = new(128, 10);

        private static readonly AESP aes192 = new(192, 12);

        private static readonly AESP aes256 = new(256, 14);

        private static readonly byte[] initialKey128 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static readonly byte[] initialKey192 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static readonly byte[] initialKey256 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static readonly string chainMode = "ECB";

        public static readonly uint batchSize = 1;


        public void TestForwardSBox()
        {
            var sBox = new AESSB();
            Assert.AreEqual(sBoxElementsCount, sBox.Box.Length);

            HashSet<byte> result = new();
            for (int i = 0; i < sBoxLength; ++i)
            {
                for (int j = 0; j < sBoxLength; ++j)
                {
                    result.Add(sBox.Box[i, j]);
                }
            }
            Assert.AreEqual(sBoxElementsCount, result.Count);
        }

        public void TestInverseSBox()
        {
            var forwardSBox = new AESSB();
            var inverseSBox = new AESSB(forwardSBox);
            inverseSBox.Inverse();

            HashSet<byte> resultForward = new();
            HashSet<byte> resultInverse = new();
            for (int i = 0; i < sBoxLength; ++i)
            {
                for (int j = 0; j < sBoxLength; ++j)
                {
                    resultForward.Add(forwardSBox.Box[i, j]);
                    resultInverse.Add(inverseSBox.Box[i, j]);

                    var forwardByte = forwardSBox.Box[i, j];
                    Assert.AreEqual((i << halfByteLength) | j,
                        inverseSBox.Box[(forwardByte & leftByteMask) >> halfByteLength, forwardByte & rightByteMask]);
                }
            }
            Assert.AreEqual(sBoxElementsCount, resultForward.Count);
            Assert.AreEqual(sBoxElementsCount, resultInverse.Count);
        }

        [TestMethod]
        public void SBoxGeneration()
        {
            // forward
            TestForwardSBox();

            // inverse
            TestInverseSBox();
        }

        public void TestState(byte[] arg)
        {
            var block = new AESS(arg);
            Assert.AreEqual(blockLength, block.Block.Length);

            foreach (var element in arg)
            {
                var isExists = false;
                for (int i = 0; i < blockRowColLength && !isExists; ++i)
                {
                    for (int j = 0; j < blockRowColLength && !isExists; ++j)
                    {
                        if (block[i, j] == element)
                            isExists = true;
                    }
                }
                Assert.IsTrue(isExists);
            }

            var plainBlock = block.ToPlainBytes();
            for (int i = 0; i < arg.Length; ++i)
            {
                Assert.AreEqual(arg[i], plainBlock[i]);
            }
            for (int i = arg.Length; i < blockLength; ++i)
            {
                Assert.AreEqual(0, plainBlock[i]);
            }
        }

        [TestMethod]
        public void StateGeneration()
        {
            TestState(new byte[] { });
            TestState(new byte[] { 0 });
            TestState(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            TestState(new byte[] { 1 });
            TestState(new byte[] { 255, 0 });
            TestState(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            TestState(new byte[] { 234, 11, 232, 34, 24, 15, 26, 37, 48, 59, 78, 101, 201, 76, 33, 55 });
            TestState(new byte[] { 1, 1, 1, 1, 5, 5, 5, 5, 200, 200, 200, 200, 255, 255, 255, 255 });
        }

        public void TestSubBytes(AES.AES aes, byte[] block)
        {
            var state = new AESS(block);
            var forward = aes.ForwardBytesSubstitution(new(state));
            var inverse = aes.InverseBytesSubstitution(new(forward));

            for (int i = 0; i < blockRowColLength; ++i)
            {
                for (int j = 0; j < blockRowColLength; ++j)
                {
                    Assert.AreNotEqual(state[i, j], forward[i, j]);
                    Assert.AreNotEqual(inverse[i, j], forward[i, j]);
                    Assert.AreEqual(state[i, j], inverse[i, j]);
                }
            }
        }

        [TestMethod]
        public void SubBytes()
        {
            var aes = new AES.AES(1, aes128, initialKey128, chainMode, batchSize);
            TestSubBytes(aes, new byte[] { });
            TestSubBytes(aes, new byte[] { 0 });
            TestSubBytes(aes, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            TestSubBytes(aes, new byte[] { 1 });
            TestSubBytes(aes, new byte[] { 255, 0 });
            TestSubBytes(aes, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            TestSubBytes(aes, new byte[] { 234, 11, 232, 34, 24, 15, 26, 37, 48, 59, 78, 101, 201, 76, 33, 55 });
            TestSubBytes(aes, new byte[] { 1, 1, 1, 1, 5, 5, 5, 5, 200, 200, 200, 200, 255, 255, 255, 255 });
        }

        public void TestShiftRows(byte[] arg)
        {
            AESS initial = new(arg);
            AESS forward = AESA.ForwardShiftRows(new(initial));
            AESS inverse = AESA.InverseShiftRows(new(forward));

            for (int i = 0; i < blockRowColLength; ++i)
            {
                for (int j = 0; j < blockRowColLength; ++j)
                {
                    Assert.AreEqual(initial[i, j], inverse[i, j]);
                    Assert.AreEqual(initial[i, (j + i) % blockRowColLength], forward[i, j]);
                }
            }
        }

        [TestMethod]
        public void ShiftRows()
        {
            TestShiftRows(new byte[] { });
            TestShiftRows(new byte[] { 0 });
            TestShiftRows(new byte[] { 1 });
            TestShiftRows(new byte[] { 1, 255 });
            TestShiftRows(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            TestShiftRows(new byte[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 });
            TestShiftRows(new byte[] { 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0 });
            TestShiftRows(new byte[] { 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0 });
            TestShiftRows(new byte[] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 });
            TestShiftRows(new byte[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 });
            TestShiftRows(new byte[] { 1, 2, 4, 8, 16, 32, 64, 128, 128, 64, 32, 16, 8, 4, 2, 1 });
            TestShiftRows(new byte[] { 255, 127, 63, 31, 15, 7, 3, 1, 1, 3, 7, 15, 31, 63, 127, 255 });
            TestShiftRows(new byte[] { 10, 0, 98, 111, 209, 74, 55, 32, 255, 43, 12, 84, 163, 201, 192, 15 });
        }

        public void TestMixColumns(byte[] arg)
        {
            AESS initial = new(arg);
            AESS forward = AESA.ForwardMixColumns(new(initial));
            AESS inverse = AESA.InverseMixColumns(new(forward));

            for (int i = 0; i < blockRowColLength; ++i)
            {
                for (int j = 0; j < blockRowColLength; ++j)
                {
                    Assert.AreEqual(initial[i, j], inverse[i, j]);
                    if (initial[i, j] != 0)
                        Assert.AreNotEqual(forward[i, j], inverse[i, j]);
                }
            }
        }

        [TestMethod]
        public void MixColumns()
        {
            TestMixColumns(new byte[] { });
            TestMixColumns(new byte[] { 0 });
            TestMixColumns(new byte[] { 1 });
            TestMixColumns(new byte[] { 255 });
            TestMixColumns(new byte[] { 1, 255 });
            TestMixColumns(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            TestMixColumns(new byte[] { 1, 2, 4, 8, 16, 32, 64, 128, 128, 64, 32, 16, 8, 4, 2, 1 });
            TestMixColumns(new byte[] { 1, 3, 7, 15, 31, 67, 127, 255, 255, 63, 31, 15, 7, 3, 1 });
            TestMixColumns(new byte[] { 1, 45, 33, 10, 95, 201, 172 });
            TestMixColumns(new byte[] { 10, 43, 61, 109, 222, 204, 4, 129, 32, 157, 211, 20, 3, 45, 143, 32 });
        }

        public bool TestKeyExtension(byte[] key, AESP parameters)
        {
            var aes = new AES.AES(0, parameters, key, chainMode, batchSize);
            return true;
        }

        [TestMethod]
        public void KeyGeneration()
        {
            Assert.IsTrue(TestKeyExtension(initialKey128, aes128));
            Assert.IsTrue(TestKeyExtension(new byte[] { 254, 1, 23, 203, 255, 128, 7, 10,
                104, 36, 98, 41, 13, 127, 21, 7 }, aes128));
            Assert.IsTrue(TestKeyExtension(initialKey192, aes192));
            Assert.IsTrue(TestKeyExtension(new byte[] { 254, 1, 23, 203, 255, 128, 7, 10,
                104, 36, 98, 41, 13, 127, 21, 7,
                8, 123, 22, 204, 65, 3, 10, 87 }, aes192));
            Assert.IsTrue(TestKeyExtension(initialKey256, aes256));
            Assert.IsTrue(TestKeyExtension(new byte[] { 254, 1, 23, 203, 255, 128, 7, 10,
                104, 36, 98, 41, 13, 127, 21, 7,
                8, 123, 22, 204, 65, 3, 10, 87,
                12, 34, 205, 32, 197, 4, 22, 48}, aes256));
        }

        public void TestBlockEncryption(byte[] block, AESA aes)
        {
            var encrypted = aes.EncryptBlock(block);
            var decrypted = aes.DecryptBlock(encrypted);

            for (int i = 0; i < block.Length; ++i)
            {
                Assert.AreEqual(block[i], decrypted[i]);
            }
            for (int i = block.Length; i < blockLength; ++i)
            {
                Assert.AreEqual(0, decrypted[i]);
            }
            for (int i = 0; i < blockLength; ++i)
            {
                Assert.AreNotEqual(encrypted[i], decrypted[i]);
            }
        }

        [TestMethod]
        public void BlockEncryptionDecryption()
        {
            // aes 128
            var aes = new AESA(100, aes128, new byte[] {
                1, 200, 19, 176, 106, 8, 231, 203,
                2, 9, 14, 153, 21, 16, 19, 1 },
                chainMode, batchSize);
            TestBlockEncryption(new byte[] { }, aes);
            TestBlockEncryption(new byte[] { 0 }, aes);
            TestBlockEncryption(new byte[] { 255 }, aes);
            TestBlockEncryption(new byte[] { 1, 2, 0, 255 }, aes);
            TestBlockEncryption(new byte[] { 1, 2, 4, 8, 16, 32, 64, 128, 128, 64, 32, 16, 8, 4, 2, 1 }, aes);
            TestBlockEncryption(new byte[] { 1, 3, 7, 15, 31, 63, 127, 255, 255, 127, 63, 31, 15, 7, 3, 1 }, aes);
            TestBlockEncryption(new byte[] { 1, 32, 143, 2, 43, 67, 209, 4, 23, 19, 103, 31, 120, 235, 4, 3 }, aes);

            // aes 192
            aes = new AESA(267192, aes192, new byte[] {
                91, 182, 191, 68, 10, 46, 152, 222,
                1, 99, 123, 56, 5, 19, 172, 10,
                16, 203, 16, 101, 20, 2, 1, 44 },
                chainMode, batchSize);
            TestBlockEncryption(new byte[] { }, aes);
            TestBlockEncryption(new byte[] { 0 }, aes);
            TestBlockEncryption(new byte[] { 1, 2, 4, 8, 16, 32, 64, 128, 128, 64, 32, 16, 8, 4, 2, 1 }, aes);
            TestBlockEncryption(new byte[] { 1, 32, 143, 2, 43, 67, 209, 4, 23, 19, 103, 31, 120, 235, 4, 3 }, aes);

            // aes 256
            aes = new AESA(6378291, aes256, new byte[] {
                91, 182, 191, 68, 10, 46, 152, 222,
                1, 99, 123, 56, 5, 19, 172, 10,
                16, 203, 16, 101, 20, 2, 1, 44,
                12, 45, 66, 142, 231, 9, 53, 44},
                chainMode, batchSize);
            TestBlockEncryption(new byte[] { }, aes);
            TestBlockEncryption(new byte[] { 0 }, aes);
            TestBlockEncryption(new byte[] { 255 }, aes);
            TestBlockEncryption(new byte[] { 1, 2, 0, 255 }, aes);
            TestBlockEncryption(new byte[] { 1, 2, 4, 8, 16, 32, 64, 128, 128, 64, 32, 16, 8, 4, 2, 1 }, aes);
            TestBlockEncryption(new byte[] { 1, 3, 7, 15, 31, 63, 127, 255, 255, 127, 63, 31, 15, 7, 3, 1 }, aes);
            TestBlockEncryption(new byte[] { 1, 32, 143, 2, 43, 67, 209, 4, 23, 19, 103, 31, 120, 235, 4, 3 }, aes);
        }
    }
}
