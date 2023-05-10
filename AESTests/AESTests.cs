using AESS = AES.AES;

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

        public void TestForwardSBox(int seed)
        {
            var sBox = AESS.GetForwardSBox(seed);
            Assert.AreEqual(sBoxElementsCount, sBox.Length);

            HashSet<byte> result = new();
            for (int i = 0; i < sBoxLength; ++i)
            {
                for (int j = 0; j < sBoxLength; ++j)
                {
                    result.Add(sBox[i, j]);
                }
            }
            Assert.AreEqual(sBoxElementsCount, result.Count);
        }

        public void TestInverseSBox(int seed)
        {
            var forwardSBox = AESS.GetForwardSBox(seed);
            var inverseSBox = AESS.GetInverseSBox(forwardSBox);

            HashSet<byte> resultForward = new();
            HashSet<byte> resultInverse = new();
            for (int i = 0; i < sBoxLength; ++i)
            {
                for (int j = 0; j < sBoxLength; ++j)
                {
                    resultForward.Add(forwardSBox[i, j]);
                    resultInverse.Add(inverseSBox[i, j]);

                    var forwardByte = forwardSBox[i, j];
                    Assert.AreEqual((i << halfByteLength) | j, 
                        inverseSBox[(forwardByte & leftByteMask) >> halfByteLength, forwardByte & rightByteMask]);
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
    }
}
