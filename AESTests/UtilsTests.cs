namespace AESTests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void TestSumGF8()
        {
            Assert.AreEqual(0, AES.Utils.SumGF8(0, 0));
            Assert.AreEqual(0, AES.Utils.SumGF8(1, 1));
            Assert.AreEqual(0, AES.Utils.SumGF8(128, 128));
            Assert.AreEqual(0, AES.Utils.SumGF8(255, 255));
        }
    }
}