namespace AES
{
    /// <summary>
    /// Allows you to perform the necessary operations on a finite Galois field 
    /// constructed as an extension of the field GF(2) {0,1} modulo an irreducible polynomial
    /// x^8 + x^4 + x^3 + x + 1.
    /// </summary>
    public class PolynomialGF256
    {
        // ------------------------------------------------------------------------------------------------------------
        // Fields
        // ------------------------------------------------------------------------------------------------------------
        public uint Coefficients { get; private set; }

        private static readonly int byteLength = 8;

        private static readonly uint uintMaxLength = 32;

        private static readonly uint mod = 283; // x^8 + x^4 + x^3 + x + 1

        private static readonly uint higherBitMask = 1u << ((int)uintMaxLength - 1);

        private static readonly PolynomialGF256 modPolynomial = new PolynomialGF256(mod);

        // ------------------------------------------------------------------------------------------------------------
        // Overloads
        // ------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            var result = new List<string>();
            uint mask = 1;
            for (var i = 0; i < uintMaxLength; ++i)
            {
                if ((mask & Coefficients) != 0)
                {
                    result.Add(string.Format("x^{0}", i));
                }
                mask <<= 1;
            }
            if (result.Count == 0)
                return "0";
            result.Reverse();
            return string.Join(" + ", result);
        }

        public override bool Equals(object? obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                PolynomialGF256 other = (PolynomialGF256)obj;
                return Coefficients == other.Coefficients;
            }
        }

        public static PolynomialGF256 operator <<(PolynomialGF256 arg, int shift)
        {
            arg.Coefficients <<= shift;
            return arg;
        }

        public static PolynomialGF256 operator >>(PolynomialGF256 arg, int shift)
        {
            arg.Coefficients >>= shift;
            return arg;
        }

        public static bool operator >(PolynomialGF256 lhs, PolynomialGF256 rhs)
        {
            return lhs.Coefficients > rhs.Coefficients;
        }

        public static bool operator <(PolynomialGF256 lhs, PolynomialGF256 rhs)
        {
            return lhs.Coefficients < rhs.Coefficients;
        }

        public static bool operator ==(PolynomialGF256 lhs, PolynomialGF256 rhs)
        {
            return lhs.Coefficients == rhs.Coefficients;
        }

        public static bool operator !=(PolynomialGF256 lhs, PolynomialGF256 rhs)
        {
            return lhs.Coefficients != rhs.Coefficients;
        }

        public static PolynomialGF256 operator +(PolynomialGF256 lhs, PolynomialGF256 rhs)
        {
            return new PolynomialGF256(lhs.Coefficients ^ rhs.Coefficients);
        }

        public static PolynomialGF256 operator /(PolynomialGF256 lhs, PolynomialGF256 rhs)
        {
            throw new Exception();
        }

        public static PolynomialGF256 operator %(PolynomialGF256 lhs, PolynomialGF256 rhs)
        {
            if (rhs.IsZero())
                throw new DivideByZeroException();
            while (lhs.Length() >= rhs.Length())
            {
                PolynomialGF256 sub = new(rhs);
                while (lhs.Length() > sub.Length())
                {
                    sub <<= 1;
                }
                lhs += sub;
            }
            if (lhs > modPolynomial)
                lhs = ToMod(lhs);
            return lhs;
        }

        public static PolynomialGF256 operator *(PolynomialGF256 lhs, PolynomialGF256 rhs)
        {
            throw new Exception();
        }

        // ------------------------------------------------------------------------------------------------------------
        // Public
        // ------------------------------------------------------------------------------------------------------------

        public PolynomialGF256(uint coefficients)
        {
            Coefficients = coefficients;
        }

        public PolynomialGF256(PolynomialGF256 other)
        {
            Coefficients = other.Coefficients;
        }

        /// <summary>
        /// Returns higher bit index
        /// </summary>
        public uint Length()
        {
            var mask = higherBitMask;
            for (uint i = 0; i < uintMaxLength; ++i)
            {
                if ((mask & Coefficients) != 0)
                    return uintMaxLength - i;
                mask >>= 1;
            }
            return 0;
        }



        // ------------------------------------------------------------------------------------------------------------
        // Private
        // ------------------------------------------------------------------------------------------------------------

        private static PolynomialGF256 ToMod(PolynomialGF256 arg)
        {
            return arg %= modPolynomial;
        }

        private bool IsZero()
        {
            return Coefficients == 0;
        }
    }
}
