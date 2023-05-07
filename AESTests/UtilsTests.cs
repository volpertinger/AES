namespace AESTests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void TestSumGF256()
        {
            // zeros
            Assert.AreEqual(0, AES.Utils.SumGF256(0, 0));
            Assert.AreEqual(0, AES.Utils.SumGF256(1, 1));
            Assert.AreEqual(0, AES.Utils.SumGF256(128, 128));
            Assert.AreEqual(0, AES.Utils.SumGF256(255, 255));

            // main
            Assert.AreEqual(0b00110011, AES.Utils.SumGF256(0b11110000, 0b11000011));
            Assert.AreEqual(0b00101010, AES.Utils.SumGF256(0b11111111, 0b11010101));
            Assert.AreEqual(0b11111001, AES.Utils.SumGF256(0b10100011, 0b01011010));
        }

        [TestMethod]
        public void TestGCD()
        {
            // zeros
            Assert.AreEqual(0, AES.Utils.GCD(0, 0));
            Assert.AreEqual(1, AES.Utils.GCD(0, 1));
            Assert.AreEqual(1, AES.Utils.GCD(1, 0));
            Assert.AreEqual(byte.MaxValue, AES.Utils.GCD(byte.MaxValue, 0));
            Assert.AreEqual(byte.MaxValue / 2, AES.Utils.GCD(byte.MaxValue / 2, 0));

            // same
            for (byte i = 1; i < byte.MaxValue; i += 1)
                Assert.AreEqual(i, AES.Utils.GCD(i, i));

            // main
            Assert.AreEqual(4, AES.Utils.GCD(4, 60));
            Assert.AreEqual(20, AES.Utils.GCD(40, 60));
            Assert.AreEqual(5, AES.Utils.GCD(25, 255));
            Assert.AreEqual(1, AES.Utils.GCD(251, 255));
            Assert.AreEqual(1, AES.Utils.GCD(byte.MaxValue, 1));
            Assert.AreEqual(64, AES.Utils.GCD(64, 128));
            Assert.AreEqual(5, AES.Utils.GCD(25, 110));
            Assert.AreEqual(1, AES.Utils.GCD(103, 207));
        }
    }
}