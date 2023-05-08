using Polynomial = AES.PolynomialGF256;

namespace AESTests
{
    [TestClass]
    public class PolynomialGF256
    {
        [TestMethod]
        public void Compare()
        {
            // equal
            Assert.AreEqual(new Polynomial(1), new Polynomial(1));
            Assert.AreEqual(new Polynomial(uint.MaxValue), new Polynomial(uint.MaxValue));
            Assert.AreEqual(new Polynomial(0), new Polynomial(0));
            Assert.AreEqual(new Polynomial(482397401u), new Polynomial(482397401u));

            // not equal
            Assert.AreNotEqual(new Polynomial(1), new Polynomial(0));
            Assert.AreNotEqual(new Polynomial(uint.MaxValue), new Polynomial(7372));
            Assert.AreNotEqual(new Polynomial(123), new Polynomial(456));
            Assert.AreNotEqual(new Polynomial(482397401u), new Polynomial(482397400u));

            // direct equal call
            Assert.IsTrue(new Polynomial(1).Equals(new Polynomial(1)));
            Assert.IsFalse(new Polynomial(1).Equals(new Polynomial(0)));
            Assert.IsFalse(new Polynomial(0).Equals(1920));

            // == !=
            Assert.IsTrue(new Polynomial(1) == new Polynomial(1));
            Assert.IsTrue(new Polynomial(uint.MaxValue) == new Polynomial(uint.MaxValue));
            Assert.IsTrue(new Polynomial(0) == new Polynomial(0));
            Assert.IsTrue(new Polynomial(482397401u) == new Polynomial(482397401u));
            Assert.IsTrue(new Polynomial(1) != new Polynomial(0));
            Assert.IsTrue(new Polynomial(uint.MaxValue) != new Polynomial(7372));
            Assert.IsTrue(new Polynomial(123) != new Polynomial(456));
            Assert.IsTrue(new Polynomial(482397401u) != new Polynomial(482397400u));

            // > <
            Assert.IsTrue(new Polynomial(1) > new Polynomial(0));
            Assert.IsTrue(new Polynomial(1377) > new Polynomial(890));
            Assert.IsTrue(new Polynomial(uint.MaxValue) > new Polynomial(uint.MaxValue - 1));
            Assert.IsTrue(new Polynomial(26171) > new Polynomial(2921));
            Assert.IsTrue(new Polynomial(0) < new Polynomial(1));
            Assert.IsTrue(new Polynomial(376) < new Polynomial(38291));
            Assert.IsTrue(new Polynomial(uint.MaxValue - 1) < new Polynomial(uint.MaxValue));
            Assert.IsTrue(new Polynomial(371) < new Polynomial(372771));
        }

        [TestMethod]
        public void Shifts()
        {
            // <<
            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_00000000) << 2);

            Assert.AreEqual(new Polynomial(0b11111100_00000000_00000000_00000000),
                new Polynomial(0b11111111_00000000_00000000_00000000) << 2);

            Assert.AreEqual(new Polynomial(0b11111111_11111111_11111111_11111111),
                new Polynomial(0b11111111_11111111_11111111_11111111) << 32);

            Assert.AreEqual(new Polynomial(0b00000000_11111111_00000000_00000000),
                new Polynomial(0b11111111_00000000_11111111_00000000) << 8);

            // >>
            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_00000000) >> 2);

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00111111),
                new Polynomial(0b00000000_00000000_00000000_11111111) >> 2);

            Assert.AreEqual(new Polynomial(0b11111111_11111111_11111111_11111111),
                new Polynomial(0b11111111_11111111_11111111_11111111) >> 32);

            Assert.AreEqual(new Polynomial(0b00000000_00000000_11111111_00000000),
                new Polynomial(0b00000000_11111111_00000000_11111111) >> 8);
        }

        [TestMethod]
        public void Sum()
        {
            // zeros
            Assert.AreEqual(new Polynomial(0), new Polynomial(0) + new Polynomial(0));
            Assert.AreEqual(new Polynomial(0), new Polynomial(1) + new Polynomial(1));
            Assert.AreEqual(new Polynomial(0), new Polynomial(uint.MaxValue) + new Polynomial(uint.MaxValue));
            Assert.AreEqual(new Polynomial(0), new Polynomial(25559) + new Polynomial(25559));

            // main
            Assert.AreEqual(new Polynomial(0b11111111_11111111_11111111_11111111),
                new Polynomial(0b11111111_00000000_11111111_00000000) + new Polynomial(0b00000000_11111111_00000000_11111111));

            Assert.AreEqual(new Polynomial(0b00000000_11111111_00000000_00000000),
                new Polynomial(0b00000000_11111111_00000000_00000000) + new Polynomial(0b00000000_00000000_00000000_00000000));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_11111111_00000000),
                new Polynomial(0b00000000_00000000_11111111_11111111) + new Polynomial(0b00000000_00000000_00000000_11111111));

            Assert.AreEqual(new Polynomial(0b11111111_00000000_00000000_11111111),
                new Polynomial(0b11111111_11111111_11111111_00000000) + new Polynomial(0b00000000_11111111_11111111_11111111));
        }

        [TestMethod]
        public void ToStr()
        {
            Assert.AreEqual("0", new Polynomial(0).ToString());
            Assert.AreEqual("x^0", new Polynomial(0b00000000_00000000_00000000_00000001).ToString());
            Assert.AreEqual("x^31 + x^23 + x^15 + x^7", new Polynomial(0b10000000_10000000_10000000_10000000).ToString());
            Assert.AreEqual("x^31 + x^23 + x^15 + x^7 + x^6 + x^5 + x^4 + x^3 + x^2 + x^1",
                new Polynomial(0b10000000_10000000_10000000_11111110).ToString());
        }

        [TestMethod]
        public void Length()
        {
            Assert.AreEqual(0u,
                new Polynomial(0b00000000_00000000_00000000_00000000).Length());

            Assert.AreEqual(32u,
                new Polynomial(0b10000000_00000000_00000000_00000000).Length());

            Assert.AreEqual(32u,
                new Polynomial(0b11111111_00000000_00000000_00000000).Length());

            Assert.AreEqual(24u,
                new Polynomial(0b00000000_10000000_10000000_10000000).Length());

            Assert.AreEqual(1u,
                new Polynomial(0b00000000_00000000_00000000_00000001).Length());
        }

        [TestMethod]
        public void DivisionReminder()
        {
            // zero reminder
            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_00000000) %
                new Polynomial(0b00000000_00000000_00000000_00000001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_00000000) %
                new Polynomial(0b10000000_10000000_10000000_00000001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_00000001) %
                new Polynomial(0b00000000_00000000_00000000_00000001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_11111111) %
                new Polynomial(0b00000000_00000000_00000000_11111111));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_10111001) %
                new Polynomial(0b00000000_00000000_00000000_10111001));

            // without ToMod()
            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000001),
                new Polynomial(0b00000000_00000000_00000000_00000001) %
                new Polynomial(0b00000000_00000000_00000000_00000010));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00010000),
                new Polynomial(0b00000000_00000000_00000000_10110000) %
                new Polynomial(0b00000000_00000000_00000000_00110000));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00001101),
                new Polynomial(0b00000000_00000000_00000000_10110110) %
                new Polynomial(0b00000000_00000000_00000000_01101001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000001),
                new Polynomial(0b10000000_00000000_00000000_00000000) %
                new Polynomial(0b01111111_11111111_11111111_11111111));

            // with ToMod() after division
            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00100101),
                new Polynomial(0b00000000_00000000_10101110_10110110) %
                new Polynomial(0b00000000_00000000_00100100_01101001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_11111001),
                new Polynomial(0b00000000_00000000_11111000_00000000) %
                new Polynomial(0b00000000_00000000_00100001_11111111));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_11011000),
                new Polynomial(0b00000000_00000000_00001000_00000000) %
                new Polynomial(0b00000000_00000000_00100000_00000001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_11000111),
                new Polynomial(0b00000000_00000000_00111000_00000111) %
                new Polynomial(0b00000000_00000000_00110000_00011000));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_10100111),
                new Polynomial(0b00000000_00000000_11111000_00000111) %
                new Polynomial(0b00000000_00000000_00110000_00011000));
        }

        [TestMethod]
        public void DivisionInteger()
        {
            // zero
            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_00000000) /
                new Polynomial(0b00000000_00000000_00000000_00000001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_00000000) /
                new Polynomial(0b10000000_10000000_10000000_00000001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_00000000) /
                new Polynomial(0b11111111_11111111_11111111_11111111));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_00000000) /
                new Polynomial(0b00000000_11111111_00000000_11111111));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_00000000) /
                new Polynomial(0b10101010_10101010_10101010_10101010));

            // without ToMod()
            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_00000001) /
                new Polynomial(0b00000000_00000000_00000000_00000010));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000110),
                new Polynomial(0b00000000_00000000_00000000_10110000) /
                new Polynomial(0b00000000_00000000_00000000_00110000));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000011),
                new Polynomial(0b00000000_00000000_00000000_10110110) /
                new Polynomial(0b00000000_00000000_00000000_01101001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000011),
                new Polynomial(0b00000000_00000000_10000000_00000000) /
                new Polynomial(0b00000000_00000000_01111111_11111111));

            // with ToMod() after division
            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_01000001),
                new Polynomial(0b00000000_00000000_10101110_10110110) /
                new Polynomial(0b00000000_00000000_00000000_01101001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00010010),
                new Polynomial(0b00000000_00000000_11111000_00000000) /
                new Polynomial(0b00000000_00000000_00000000_11111111));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00011110),
                new Polynomial(0b00000000_00000000_10001000_00000000) /
                new Polynomial(0b00000000_00000000_00000000_01000001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00110101),
                new Polynomial(0b00000000_00000000_11111111_11111111) /
                new Polynomial(0b00000000_00000000_00000000_00000001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00110100),
                new Polynomial(0b00000000_00000000_10101010_10101010) /
                new Polynomial(0b00000000_00000000_00000000_01010101));
        }

        [TestMethod]
        public void GetCoefficient()
        {
            Assert.AreEqual(0b00000000_00000000_00000000_00000000u,
                new Polynomial(0b00000000_00000000_00000000_00000000)[0]);

            Assert.AreEqual(0b00000000_00000000_00000000_00000000u,
                new Polynomial(0b00000000_00000000_00000000_00000000)[31]);

            Assert.AreEqual(0b00000000_00000000_10000000_00000000u,
                new Polynomial(0b00000000_00000000_10000000_00000000)[15]);

            Assert.AreEqual(0b00000000_00001000_00000000_00000000u,
                new Polynomial(0b11000000_11001000_11111111_00000000)[19]);


        }

        [TestMethod]
        public void Multiplication()
        {
            // without ToMod
            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b00000000_00000000_00000000_00000000) *
                new Polynomial(0b00000000_00000000_00000000_00000000));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00000000),
                new Polynomial(0b11111111_11111111_11111111_11111111) *
                new Polynomial(0b00000000_00000000_00000000_00000000));
            
            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_11111111),
                new Polynomial(0b00000000_00000000_00000000_11111111) *
                new Polynomial(0b00000000_00000000_00000000_00000001));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_0000000_11111110),
                new Polynomial(0b00000000_00000000_00000000_01111111) *
                new Polynomial(0b00000000_00000000_00000000_00000010));

            // with ToMod
            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_10011010),
                new Polynomial(0b00000000_00000000_00000000_10000000) *
                new Polynomial(0b00000000_00000000_00000000_10000000));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_00010011),
                new Polynomial(0b00000000_00000000_00000000_11111111) *
                new Polynomial(0b00000000_00000000_00000000_11111111));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_01000110),
                new Polynomial(0b00000000_00000000_00000000_10000000) *
                new Polynomial(0b00000000_00000000_00000000_10101010));

            Assert.AreEqual(new Polynomial(0b00000000_00000000_00000000_01011001),
                new Polynomial(0b00000000_00000000_00000000_10101010) *
                new Polynomial(0b00000000_00000000_00000000_01010101));
        }
    }
}
