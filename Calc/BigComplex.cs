using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace Calc;

// 复数
[DebuggerDisplay(@"\{{ToString(),nq}\}")]
public readonly struct BigComplex
    : IEquatable<BigComplex>
{
    public static bool ShowModulus { get; set; }

    static BigComplex()
    {
        ShowModulus = false;
    }

    // 实部
    public BigDecimal Real { get; }

    // 虚部
    public BigDecimal Imaginary { get; }

    private readonly Lazy<BigDecimal> _modulus;

    private readonly Lazy<BigDecimal> _argument;

    // 模长
    public BigDecimal Modulus => _modulus.Value;

    // 幅角
    public BigDecimal Argument => _argument.Value;

    // 是否是复数
    public bool IsComplex => Imaginary != 0;

    // 通过实部虚部创建复数
    public BigComplex(BigDecimal real, BigDecimal imaginary)
    {
        this.Real = real;
        this.Imaginary = imaginary;
        this._modulus = new Lazy<BigDecimal>(() => MyMath.Sqrt(MyMath.Pow(real, 2) + MyMath.Pow(imaginary, 2)).Real);
        this._argument = new Lazy<BigDecimal>(() => MyMath.Arctan2(imaginary, real));
    }

    public bool Equals(BigComplex other)
    {
        return Real == other.Real && Imaginary == other.Imaginary;
    }

    public static implicit operator BigComplex(Complex v)
    {
        return new BigComplex(v.Real, v.Imaginary);
    }

    public static implicit operator BigComplex(BigDecimal v)
    {
        return new BigComplex(v, 0);
    }

    public static implicit operator BigComplex(BigInteger v)
    {
        return new BigComplex(v, 0);
    }

    public static implicit operator BigComplex(decimal v)
    {
        return new BigComplex(v, 0);
    }

    public static implicit operator BigComplex(int v)
    {
        return new BigComplex(v, 0);
    }

    public static implicit operator BigComplex(double v)
    {
        return new BigComplex(v, 0);
    }

    public static implicit operator BigComplex(float v)
    {
        return new BigComplex(v, 0);
    }

    public static implicit operator BigComplex(long v)
    {
        return new BigComplex(v, 0);
    }

    public static explicit operator Complex(BigComplex v)
    {
        return new Complex((double)v.Real, (double)v.Imaginary);
    }

    public static BigComplex operator +(BigComplex v)
    {
        return v;
    }

    public static BigComplex operator -(BigComplex v)
    {
        return new BigComplex(-v.Real, -v.Imaginary);
    }

    public static BigComplex operator ++(BigComplex v)
    {
        return new BigComplex(v.Real + 1, v.Imaginary);
    }

    public static BigComplex operator --(BigComplex v)
    {
        return new BigComplex(v.Real - 1, v.Imaginary);
    }

    public static BigComplex operator +(BigComplex l, BigComplex r)
    {
        return new BigComplex(l.Real + r.Real, l.Imaginary + r.Imaginary);
    }

    public static BigComplex operator -(BigComplex l, BigComplex r)
    {
        return new BigComplex(l.Real - r.Real, l.Imaginary - r.Imaginary);
    }

    public static BigComplex operator *(BigComplex l, BigComplex r)
    {
        return new BigComplex(l.Real * r.Real - l.Imaginary * r.Imaginary, l.Imaginary * r.Real + l.Real * r.Imaginary);
    }

    public static BigComplex operator /(BigComplex l, BigComplex r)
    {
        return new BigComplex(
            (l.Real * r.Real + l.Imaginary * r.Imaginary) / (r.Real * r.Real + r.Imaginary * r.Imaginary),
            (l.Imaginary * r.Real - l.Real * r.Imaginary) / (r.Real * r.Real + r.Imaginary * r.Imaginary));
    }

    public static BigDecimal Abs(BigComplex v)
    {
        return v.Modulus;
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        if (!ShowModulus)
        {
            if (Real != 0) sb.Append(Real.Truncate(100));
            if (Imaginary != 0)
            {
                if (Imaginary < 0)
                {
                    if (Imaginary == -1) sb.Append('-');
                    else sb.Append(Imaginary.Truncate(100));
                }
                else
                {
                    if (Real != 0)
                    {
                        sb.Append('+');
                    }

                    if (Imaginary != 1) sb.Append(Imaginary.Truncate(100));
                }

                sb.Append('i');
            }

            if (Real == 0 && Imaginary == 0) sb.Append('0');
        }
        else
        {
            if (this.Modulus == 0) return "0";
            if (this.Modulus != 1) sb.Append(this.Modulus);
            sb.Append("e^(").Append(this.Argument)
                .Append("i)");
        }

        return sb.ToString();
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return obj is BigComplex v && Equals(v);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Real.GetHashCode() * 397) ^ Imaginary.GetHashCode();
        }
    }

    public static bool operator ==(BigComplex left, BigComplex right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BigComplex left, BigComplex right)
    {
        return !left.Equals(right);
    }
}