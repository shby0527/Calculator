using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Calc
{
    /// <summary>
    /// Arbitrary precision decimal.
    /// All operations are exact, except for division. Division never determines more digits than the given precision.
    /// Source: https://gist.github.com/JcBernack/0b4eef59ca97ee931a2f45542b9ff06d
    /// Based on https://stackoverflow.com/a/4524254
    /// Author: Jan Christoph Bernack (contact: jc.bernack at gmail.com)
    /// License: public domain
    /// </summary>
    [DebuggerDisplay("\\{{ToString(),nq}\\}")]
    public readonly struct BigDecimal
        : IComparable
        , IComparable<BigDecimal>
        , IEquatable<BigDecimal>
    {
        /// <summary>
        /// Sets the maximum precision of division operations.
        /// If AlwaysTruncate is set to true all operations are affected.
        /// </summary>
        public static readonly int Precision = 500;

        public BigInteger Mantissa { get; init; }
        public int Exponent { get; init; }

        public BigDecimal(BigInteger mantissa, int exponent)
        {
            int exp = exponent;
            BigInteger man = mantissa;
            if (man.IsZero)
            {
                exp = 0;
            }
            else
            {
                BigInteger remainder = 0;
                while (remainder == 0)
                {
                    var shortened = BigInteger.DivRem(man, 10, out remainder);
                    if (remainder == 0)
                    {
                        man = shortened;
                        exp++;
                    }
                }
            }
            Mantissa = man;
            Exponent = exp;
        }

        /// <summary>
        /// Truncate the number to the given precision by removing the least significant digits.
        /// </summary>
        /// <returns>The truncated number</returns>
        public BigDecimal Truncate(int precision)
        {
            BigInteger mantissa = Mantissa;
            int exponent = Exponent;
            if (exponent >= 0) return new BigDecimal(mantissa, exponent);
            int d = NumberOfDigits(mantissa);
            while (d > precision)
            {
                mantissa /= 10;
                exponent++;
                d--;
            }
            int td = NumberOfDigits(mantissa);
            int k = td + exponent;
            if (k < -precision)
            {
                // 小数位太多太多了,直接返回0
                return new BigDecimal(BigInteger.Zero, 0);
            }
            return new BigDecimal(mantissa, exponent);
        }

        public BigDecimal Truncate()
        {
            return Truncate(Precision);
        }

        public BigDecimal Floor()
        {
            return Truncate(NumberOfDigits(Mantissa) + Exponent);
        }

        public static int NumberOfDigits(BigInteger value)
        {
            return (int)Math.Ceiling(BigInteger.Log10(value * value.Sign));
        }

        #region Conversions

        public static implicit operator BigDecimal(int value)
        {
            return new BigDecimal(value, 0);
        }

        public static implicit operator BigDecimal(long v)
        {
            return new BigDecimal(v, 0);
        }

        public static implicit operator BigDecimal(double value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            double scaleFactor = 1;
            while (Math.Abs(value * scaleFactor - (double)mantissa) > 0)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigDecimal(mantissa, exponent);
        }

        public static implicit operator BigDecimal(decimal value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            decimal scaleFactor = 1;
            while ((decimal)mantissa != value * scaleFactor)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigDecimal(mantissa, exponent);
        }

        public static implicit operator BigDecimal(BigInteger value)
        {
            return new BigDecimal(value, 0);
        }

        public static explicit operator double(BigDecimal value)
        {
            return double.Parse(value.ToString());
        }

        public static explicit operator float(BigDecimal value)
        {
            return Convert.ToSingle((double)value);
        }

        public static explicit operator decimal(BigDecimal value)
        {
            return decimal.Parse(value.ToString());
        }

        public static explicit operator int(BigDecimal value)
        {
            return (int)(BigInteger)value;
        }

        public static explicit operator uint(BigDecimal value)
        {
            return (uint)(BigInteger)value;
        }

        public static explicit operator BigInteger(BigDecimal value)
        {
            if (value.Exponent < 0)
            {
                string integer = value.Mantissa.ToString();
                int index = integer.Length + value.Exponent;
                if (index <= 0) return BigInteger.Zero;
                return BigInteger.Parse(integer[0..index]);
            }
            return value.Mantissa * BigInteger.Pow(10, value.Exponent);
        }

        #endregion

        #region Operators

        public static BigDecimal operator +(BigDecimal value)
        {
            return value;
        }

        public static BigDecimal operator -(BigDecimal value)
        {
            return new BigDecimal(-value.Mantissa, value.Exponent);
        }

        public static BigDecimal operator ++(BigDecimal value)
        {
            return value + 1;
        }

        public static BigDecimal operator --(BigDecimal value)
        {
            return value - 1;
        }

        public static BigDecimal operator +(BigDecimal left, BigDecimal right)
        {
            return Add(left, right);
        }

        public static BigDecimal operator -(BigDecimal left, BigDecimal right)
        {
            return Add(left, -right);
        }

        private static BigDecimal Add(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent
                ? new BigDecimal(AlignExponent(left, right) + right.Mantissa, right.Exponent)
                : new BigDecimal(AlignExponent(right, left) + left.Mantissa, left.Exponent);
        }

        public static BigDecimal operator *(BigDecimal left, BigDecimal right)
        {
            return new BigDecimal(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent);
        }

        public static BigDecimal operator /(BigDecimal dividend, BigDecimal divisor)
        {
            var exponentChange = Precision - (NumberOfDigits(dividend.Mantissa) - NumberOfDigits(divisor.Mantissa));
            if (exponentChange < 0)
            {
                exponentChange = 0;
            }
            var dividendMan = dividend.Mantissa;
            dividendMan *= BigInteger.Pow(10, exponentChange);
            return new BigDecimal(dividendMan / divisor.Mantissa, dividend.Exponent - divisor.Exponent - exponentChange);
        }

        public static BigDecimal operator %(BigDecimal left, BigDecimal right)
        {
            return left - right * (left / right).Floor();
        }

        public static bool operator ==(BigDecimal left, BigDecimal right)
        {
            return left.Exponent == right.Exponent && left.Mantissa == right.Mantissa;
        }

        public static bool operator !=(BigDecimal left, BigDecimal right)
        {
            return left.Exponent != right.Exponent || left.Mantissa != right.Mantissa;
        }

        public static bool operator <(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) < right.Mantissa : left.Mantissa < AlignExponent(right, left);
        }

        public static bool operator >(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) > right.Mantissa : left.Mantissa > AlignExponent(right, left);
        }

        public static bool operator <=(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) <= right.Mantissa : left.Mantissa <= AlignExponent(right, left);
        }

        public static bool operator >=(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) >= right.Mantissa : left.Mantissa >= AlignExponent(right, left);
        }

        /// <summary>
        /// Returns the mantissa of value, aligned to the exponent of reference.
        /// Assumes the exponent of value is larger than of reference.
        /// </summary>
        private static BigInteger AlignExponent(BigDecimal value, BigDecimal reference)
        {
            return value.Mantissa * BigInteger.Pow(10, value.Exponent - reference.Exponent);
        }

        #endregion

        #region Additional mathematical functions

        public static BigDecimal Exp(double exponent)
        {
            var tmp = (BigDecimal)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= Math.Exp(diff);
                exponent -= diff;
            }
            return tmp * Math.Exp(exponent);
        }

        public static BigDecimal Pow(double basis, double exponent)
        {
            var tmp = (BigDecimal)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= Math.Pow(basis, diff);
                exponent -= diff;
            }
            return tmp * Math.Pow(basis, exponent);
        }

        #endregion

        public override string ToString()
        {
            StringBuilder sb = new();
            string main = BigInteger.Abs(Mantissa).ToString();
            if (Mantissa.Sign < 0) sb.Append('-');
            if (Exponent == 0)
            {
                sb.Append(main);
                return sb.ToString();
            }
            if (Exponent is > 0 and < 10)
            {
                sb.Append(main);
                for (int i = 0; i < Exponent; i++)
                {
                    sb.Append('0');
                }
                return sb.ToString();
            }
            if (Exponent >= 10)
            {
                sb.Append(main[0]);
                if (main.Length > 1)
                {
                    sb.Append('.');
                    sb.Append(main[1..]);
                }
                sb.Append('E').Append(Exponent + main.Length - 1);
                return sb.ToString();
            }
            int absExp = Math.Abs(Exponent);
            if (main.Length == absExp)
            {
                sb.Append("0.").Append(main);
                return sb.ToString();
            }
            if (main.Length > absExp)
            {
                int index = main.Length - absExp;
                sb.Append(main[0..index]);
                sb.Append('.')
                    .Append(main[index..]);
                return sb.ToString();
            }
            // 负数
            if (absExp is > 0 and < 15)
            {
                int zeroCount = absExp - main.Length;
                sb.Append("0.");
                for (int i = 0; i < zeroCount; i++)
                {
                    sb.Append('0');
                }
                sb.Append(main);
                return sb.ToString();
            }
            sb.Append(main[0]);
            if (main.Length > 1)
            {
                sb.Append('.').Append(main[1..]);
            }
            sb.Append('E').Append(Exponent + main.Length - 1);
            return sb.ToString();
        }

        public bool Equals(BigDecimal other)
        {
            return other.Mantissa.Equals(Mantissa) && other.Exponent == Exponent;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            return obj is BigDecimal @decimal && Equals(@decimal);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mantissa.GetHashCode() * 397) ^ Exponent;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj is null || !(obj is BigDecimal))
            {
                throw new ArgumentException("obj is null", nameof(obj));
            }
            return CompareTo((BigDecimal)obj);
        }

        public int CompareTo(BigDecimal other)
        {
            return this < other ? -1 : (this > other ? 1 : 0);
        }


        public static BigDecimal Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new FormatException();
            string numberCharacters = "-0.1234567890";

            NumberFormatInfo BigDecimalNumberFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            int exponent = 0;
            int decimalPlace = 0;
            bool isNegative = false;
            string localInput = new(s.Trim().Where(c => numberCharacters.Contains(c)).ToArray());

            if (localInput.StartsWith(BigDecimalNumberFormatInfo.NegativeSign))
            {
                isNegative = true;
                localInput = localInput.Replace(BigDecimalNumberFormatInfo.NegativeSign, string.Empty);
            }

            if (localInput.Contains(BigDecimalNumberFormatInfo.NumberDecimalSeparator))
            {
                decimalPlace = localInput.IndexOf(BigDecimalNumberFormatInfo.NumberDecimalSeparator);

                exponent = ((decimalPlace + 1) - (localInput.Length));
                localInput = localInput.Replace(BigDecimalNumberFormatInfo.NumberDecimalSeparator, string.Empty);
            }

            BigInteger mantessa = BigInteger.Parse(localInput);
            if (isNegative)
            {
                mantessa = BigInteger.Negate(mantessa);
            }

            return new BigDecimal(mantessa, exponent);
        }

        public static bool TryParse(string s, out BigDecimal big)
        {
            try
            {
                big = Parse(s);
                return true;
            }
            catch (Exception)
            {
                big = 0;
                return false;
            }
        }
    }
}
