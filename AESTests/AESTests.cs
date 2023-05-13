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

        public void TestForwardSBox(int seed)
        {
            var sBox = new AESSB(seed);
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

        public void TestInverseSBox(int seed)
        {
            var forwardSBox = new AESSB(seed);
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
            TestForwardSBox(0);
            TestForwardSBox(int.MaxValue);
            TestForwardSBox(36728);
            TestForwardSBox(11111);

            // inverse
            TestInverseSBox(0);
            TestInverseSBox(int.MaxValue);
            TestInverseSBox(36728);
            TestInverseSBox(11111);
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
            var aes = new AES.AES(1);
            TestSubBytes(aes, new byte[] { });
            TestSubBytes(aes, new byte[] { 0 });
            TestSubBytes(aes, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            TestSubBytes(aes, new byte[] { 1 });
            TestSubBytes(aes, new byte[] { 255, 0 });
            TestSubBytes(aes, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            TestSubBytes(aes, new byte[] { 234, 11, 232, 34, 24, 15, 26, 37, 48, 59, 78, 101, 201, 76, 33, 55 });
            TestSubBytes(aes, new byte[] { 1, 1, 1, 1, 5, 5, 5, 5, 200, 200, 200, 200, 255, 255, 255, 255 });
        }
    }
}
