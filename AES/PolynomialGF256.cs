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

        private static readonly uint higherBitMask = 1u << ((int)uintMaxLength - 1);

        // x^8 + x^4 + x^3 + x + 1
        private static readonly PolynomialGF256 mod = new PolynomialGF256(283);

        // x^7 + x^6 + x^5 + x^4 + x^3 + x^2 + x^1 + x^0
        private static readonly PolynomialGF256 border = new PolynomialGF256((1 << 8) - 1);

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

        public uint this[int key]
        {
            get
            {
                if (key >= uintMaxLength || key < 0)
                    throw new IndexOutOfRangeException();
                return (uint)(Coefficients & (1 << key));
            }
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
            if (rhs.IsZero())
                throw new DivideByZeroException();
            PolynomialGF256 result = new(0);
            while (lhs.Length() >= rhs.Length())
            {
                PolynomialGF256 sub = new(rhs);
                var startLength = lhs.Length();
                while (lhs.Length() > sub.Length())
                {
                    sub <<= 1;
                }
                lhs += sub;
                var shift = startLength - lhs.Length();
                result.Coefficients |= 1;
                result <<= (int)shift;
            }
            var correctionShift = rhs.Length() - lhs.Length();
            result >>= (int)correctionShift;
            if (result > border)
                result = ToMod(result);
            return result;
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
            if (lhs > border)
                lhs = ToMod(lhs);
            return lhs;
        }

        public static PolynomialGF256 operator *(PolynomialGF256 lhs, PolynomialGF256 rhs)
        {
            var result = new PolynomialGF256(0);
            for (var i = 0; i < uintMaxLength; ++i)
            {
                for (var j = 0; j < uintMaxLength; ++j)
                {
                    result.Coefficients ^= lhs[i] * rhs[j];
                }
            }
            if (result > border)
                result = ToMod(result);
            return result;
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
            return arg %= mod;
        }

        private bool IsZero()
        {
            return Coefficients == 0;
        }
    }
}
