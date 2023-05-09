using AESS = AES.AES;

namespace AESTests
{
    [TestClass]
    public class AESTests
    {

        public void TestForwardSBox(int seed)
        {
            var sBox = AESS.GetForwardSBox(seed);
            Assert.AreEqual(byte.MaxValue + 1, sBox.Count);
            for (int i = 0; i <= byte.MaxValue; ++i)
            {
                Assert.IsTrue(sBox.Contains((byte)i));
            }
        }

        public void TestInverseSBox(int seed)
        {
            var forwardSBox = AESS.GetForwardSBox(seed);
            var inverseSBox = AESS.GetInverseSBox(forwardSBox);
            for (int i = 0; i <= byte.MaxValue; ++i)
            {
                Assert.IsTrue(forwardSBox.Contains((byte)i));
                Assert.IsTrue(inverseSBox.Contains((byte)i));
                Assert.AreEqual(i, inverseSBox[forwardSBox[i]]);
            }
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
