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
        public void GetMaskForIndex()
        {
            Assert.AreEqual(1, AES.Utils.GetMaskForIndex(0));
            Assert.AreEqual(2, AES.Utils.GetMaskForIndex(1));
            Assert.AreEqual(4, AES.Utils.GetMaskForIndex(2));
            Assert.AreEqual(8, AES.Utils.GetMaskForIndex(3));
            Assert.AreEqual(16, AES.Utils.GetMaskForIndex(4));
            Assert.AreEqual(32, AES.Utils.GetMaskForIndex(5));
            Assert.AreEqual(64, AES.Utils.GetMaskForIndex(6));
            Assert.AreEqual(128, AES.Utils.GetMaskForIndex(7));
        }

        [TestMethod]
        public void GetBitByIndexTest()
        {
            // zeros
            Assert.AreEqual(0u, AES.Utils.GetBitByIndex(128, 6));
            Assert.AreEqual(0u, AES.Utils.GetBitByIndex(64, 5));
            Assert.AreEqual(0u, AES.Utils.GetBitByIndex(32, 4));
            Assert.AreEqual(0u, AES.Utils.GetBitByIndex(16, 3));
            Assert.AreEqual(0u, AES.Utils.GetBitByIndex(8, 2));
            Assert.AreEqual(0u, AES.Utils.GetBitByIndex(4, 1));
            Assert.AreEqual(0u, AES.Utils.GetBitByIndex(2, 0));
            Assert.AreEqual(0u, AES.Utils.GetBitByIndex(1, 7));

            // not zeros
            Assert.AreEqual(128u, AES.Utils.GetBitByIndex(255, 7));
            Assert.AreEqual(64u, AES.Utils.GetBitByIndex(255, 6));
            Assert.AreEqual(32u, AES.Utils.GetBitByIndex(255, 5));
            Assert.AreEqual(16u, AES.Utils.GetBitByIndex(255, 4));
            Assert.AreEqual(8u, AES.Utils.GetBitByIndex(255, 3));
            Assert.AreEqual(4u, AES.Utils.GetBitByIndex(255, 2));
            Assert.AreEqual(2u, AES.Utils.GetBitByIndex(255, 1));
            Assert.AreEqual(1u, AES.Utils.GetBitByIndex(255, 0));
        }

        [TestMethod]
        public void MultipleWithOverflowTest()
        {
            // zeros
            Assert.AreEqual(0u, AES.Utils.Multiple(0b00000000, 0b00000000));
            Assert.AreEqual(0u, AES.Utils.Multiple(0b11111111, 0b00000000));
            Assert.AreEqual(0u, AES.Utils.Multiple(0b10000000, 0b10000000));

            // main
            Assert.AreEqual(0b00000001u, AES.Utils.Multiple(0b00000001, 0b00000001));
            Assert.AreEqual(0b00000101u, AES.Utils.Multiple(0b10000011, 0b10000011));
            Assert.AreEqual(0b00000000u, AES.Utils.Multiple(0b00000000, 0b00000000));
            Assert.AreEqual(0b00000000u, AES.Utils.Multiple(0b00000000, 0b00000000));
            Assert.AreEqual(0b00000000u, AES.Utils.Multiple(0b00000000, 0b00000000));
            Assert.AreEqual(0b00000000u, AES.Utils.Multiple(0b00000000, 0b00000000));
            Assert.AreEqual(0b00000000u, AES.Utils.Multiple(0b00000000, 0b00000000));
            Assert.AreEqual(0b00000000u, AES.Utils.Multiple(0b00000000, 0b00000000));
        }

        [TestMethod]
        public void GetRightPartTest()
        {
            Assert.AreEqual(0b00000000_00000000_00000000_00000000u,
                AES.Utils.GetRightPart(0b00000000_00000000_00000000_00000000, 31));

            Assert.AreEqual(0b00000000_00000000_00000000_00000001u,
                AES.Utils.GetRightPart(0b00000000_00000000_00000000_00000001, 31));

            Assert.AreEqual(0b00000000_00000000_00000000_00000001u,
                AES.Utils.GetRightPart(0b00000000_00000000_00000000_00000001, 0));

            Assert.AreEqual(0b0000111_00000000_00000000_00000000u,
                AES.Utils.GetRightPart(0b11111111_00000000_00000000_00000000, 26));

            Assert.AreEqual(0b00000000_00000000_00000000_11111111u,
                AES.Utils.GetRightPart(0b00000000_00000000_00000000_11111111, 20));

            Assert.AreEqual(0b00000000_00000000_00000000_00000001u,
                AES.Utils.GetRightPart(0b00000000_00000000_00000000_11111111, 0));
        }

        [TestMethod]
        public void GetLeftPartTest()
        {
            Assert.AreEqual(0b00000000_00000000_00000000_00000000u,
                AES.Utils.GetLeftPart(0b00000000_00000000_00000000_00000000, 31));

            Assert.AreEqual(0b00000000_00000000_00000000_00000001u,
                AES.Utils.GetLeftPart(0b10000000_00000000_00000000_00000000, 31));

            Assert.AreEqual(0b00000000_00000000_00000000_00000010u,
                AES.Utils.GetLeftPart(0b10000000_00000000_00000000_00000000, 30));

            Assert.AreEqual(0b0000000_00000000_00000000_00111111u,
                AES.Utils.GetLeftPart(0b11111111_00000000_00000000_00000000, 26));

            Assert.AreEqual(0b00000000_00000000_00001111_11110000u,
                AES.Utils.GetLeftPart(0b11111111_00000000_00000000_00000000, 20));
        }

        [TestMethod]
        public void GetHigherBitIndexTest()
        {
            Assert.AreEqual(31, AES.Utils.GetHigherBitIndex(0b11111111_00000000_00000000_00000000));
            Assert.AreEqual(27, AES.Utils.GetHigherBitIndex(0b00001111_00000000_00000000_00000000));
            Assert.AreEqual(0, AES.Utils.GetHigherBitIndex(0b00000000_00000000_00000000_00000000));
            Assert.AreEqual(0, AES.Utils.GetHigherBitIndex(0b00000000_00000000_00000000_00000001));
            Assert.AreEqual(1, AES.Utils.GetHigherBitIndex(0b00000000_00000000_00000000_00000010));
            Assert.AreEqual(15, AES.Utils.GetHigherBitIndex(0b00000000_00000000_10100000_11100000));
        }

        [TestMethod]
        public void DevideGF256Test()
        {
            byte reminder = 0;


            // zero reminder
            Assert.AreEqual(0b00000000, AES.Utils.DevideGF256(0b00000000, 0b00000001, out reminder));
            Assert.AreEqual(0, reminder);
            Assert.AreEqual(0b00000001, AES.Utils.DevideGF256(0b11111111, 0b11111111, out reminder));
            Assert.AreEqual(0, reminder);
        }

        [TestMethod]
        public void MaxPower()
        {
            Assert.AreEqual(AES.Constants.intMaxLength, AES.Utils.MaxNumberPower2(0));
            for (int i = 0; i < AES.Constants.intMaxLength; ++i)
            {
                Assert.AreEqual(1u << i, AES.Utils.MaxNumberPower2(1u << i));
                if (i != 0)
                    Assert.AreEqual(1u, AES.Utils.MaxNumberPower2((1u << i) - 1));
            }
            Assert.AreEqual(1u, AES.Utils.MaxNumberPower2(0b000001));
            Assert.AreEqual(1u, AES.Utils.MaxNumberPower2(0b1110101010100101));
            Assert.AreEqual(1u, AES.Utils.MaxNumberPower2(0b1010100010100101));
            Assert.AreEqual(1u, AES.Utils.MaxNumberPower2(0b01001010101010101010101010111111));
            Assert.AreEqual(1u << 2, AES.Utils.MaxNumberPower2(0b00000000000100));
            Assert.AreEqual(1u << 5, AES.Utils.MaxNumberPower2(0b000010101010101100000));
            Assert.AreEqual(1u << 30, AES.Utils.MaxNumberPower2(0b11000000000000000000000000000000));
            Assert.AreEqual(1u << 29, AES.Utils.MaxNumberPower2(0b10100000000000000000000000000000));
            Assert.AreEqual(1u << 28, AES.Utils.MaxNumberPower2(0b10010000000000000000000000000000));
            Assert.AreEqual(1u << 27, AES.Utils.MaxNumberPower2(0b10001000000000000000000000000000));
            Assert.AreEqual(1u << 26, AES.Utils.MaxNumberPower2(0b10000100000000000000000000000000));
        }

        // TODO: Devide, Multiple in GF156 tests

    }
}