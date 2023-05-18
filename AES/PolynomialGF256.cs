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

        private static PolynomialGF256 Zero { get => new(0); }
        private static PolynomialGF256 One { get => new(1); }

        private static readonly uint uintMaxLength = 32;

        private static readonly uint higherBitMask = 1u << ((int)uintMaxLength - 1);

        // x^8 + x^4 + x^3 + x + 1
        private static readonly PolynomialGF256 mod = new(283);

        // x^7 + x^6 + x^5 + x^4 + x^3 + x^2 + x^1 + x^0
        private static readonly PolynomialGF256 border = new((1 << 8) - 1);

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

        public override int GetHashCode()
        {
            return base.GetHashCode();
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
            return new PolynomialGF256(arg.Coefficients << shift);
        }

        public static PolynomialGF256 operator >>(PolynomialGF256 arg, int shift)
        {
            return new PolynomialGF256(arg.Coefficients >> shift);
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
            PolynomialGF256 result = Zero;
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
                lhs = ToMod(lhs);
            return lhs;
        }

        public static PolynomialGF256 operator *(PolynomialGF256 lhs, PolynomialGF256 rhs)
        {
            var result = Zero;
            while (!rhs.IsZero())
            {
                if ((rhs.Coefficients & 1) != 0)
                {
                    result += lhs;
                }
                lhs <<= 1;
                rhs >>= 1;
            }
            return ToMod(result);
        }

        // ------------------------------------------------------------------------------------------------------------
        // Public
        // ------------------------------------------------------------------------------------------------------------

        public PolynomialGF256()
        {
            Coefficients = Zero.Coefficients;
        }

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

        /// <summary>
        /// Extended Euclidean algorithm for GF256 polynomial
        /// </summary>
        /// <param name="lhs">First polynomial</param>
        /// <param name="rhs">Second polynomial</param>
        /// <param name="lhsCoefficient">First bezout ratio</param>
        /// <param name="rhsCoefficient">Second bezout ratio</param>
        /// <returns>GCD of lhs and rhs</returns>
        public static PolynomialGF256 ExtendedGCD(PolynomialGF256 lhs, PolynomialGF256 rhs,
            out PolynomialGF256 lhsCoefficient, out PolynomialGF256 rhsCoefficient)
        {
            var isNeedToReplaceCoefficients = false;
            if (lhs > rhs)
            {
                isNeedToReplaceCoefficients = true;
                (rhs, lhs) = (lhs, rhs);
            }
            PolynomialGF256 upL = Zero, upR = One, downL = One, downR = Zero;
            while (!lhs.IsZero())
            {
                PolynomialGF256 quotient = rhs / lhs;
                PolynomialGF256 remainder = rhs % lhs;
                PolynomialGF256 newDownL = ToMod(upL + downL * quotient);
                PolynomialGF256 newDownR = ToMod(upR + downR * quotient);
                rhs = lhs;
                lhs = remainder;
                upL = downL;
                upR = downR;
                downL = newDownL;
                downR = newDownR;
            }
            if (!isNeedToReplaceCoefficients)
            {
                lhsCoefficient = ToMod(upL);
                rhsCoefficient = ToMod(upR);
            }
            else
            {
                lhsCoefficient = ToMod(upR);
                rhsCoefficient = ToMod(upL);
            }
            return ToMod(rhs);
        }

        /// <summary>
        /// Get reverse polynomial by modulo deductions. If GCD != 1, returns 0.
        /// </summary>
        public PolynomialGF256 GetReverse()
        {
            var gcd = ExtendedGCD(this, mod, out PolynomialGF256 leftCoefficient, out _);
            if (!gcd.IsOne())
                return Zero;
            return leftCoefficient;
        }

        // ------------------------------------------------------------------------------------------------------------
        // Private
        // ------------------------------------------------------------------------------------------------------------

        private static PolynomialGF256 ToMod(PolynomialGF256 arg)
        {
            if (arg > border)
                return arg %= mod;
            return arg;
        }

        private bool IsZero()
        {
            return this == Zero;
        }

        private bool IsOne()
        {
            return this == One;
        }
    }
}
