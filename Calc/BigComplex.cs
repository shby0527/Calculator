using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace Calc;

// 复数
[DebuggerDisplay("\\{{ToString(),nq}\\}")]
public readonly struct BigComplex
    : IEquatable<BigComplex>
{

    // 实部
    public BigDecimal Real { get; init; }

    // 虚部
    public BigDecimal Imaginary { get; init; }

    // 模长
    public BigDecimal Modulus { get; init; }

    // 相角
    public BigDecimal Argument { get; init; }

    // 是否是复数
    public bool IsComplex
    {
        get
        {
            return Imaginary != 0;
        }
    }

    // 通过实部虚部创建复数
    public BigComplex(BigDecimal real, BigDecimal imaginary)
    {
        this.Real = real;
        this.Imaginary = imaginary;
        this.Modulus = MyMath.Sqrt(MyMath.Pow(real, 2) + MyMath.Pow(imaginary, 2)).Real;
        this.Argument = Math.Atan2((double)imaginary, (double)real);
    }

    // 通过相角模长确定复数
    public BigComplex(BigDecimal modulus, BigDecimal argument, bool _)
    {
        this.Modulus = modulus;
        this.Argument = argument;
        this.Real = modulus * MyMath.Cos(argument);
        this.Imaginary = modulus * MyMath.Sin(argument);
    }

    public bool Equals(BigComplex other)
    {
        return this.Real == other.Real && this.Imaginary == other.Imaginary;
    }

    public static implicit operator BigComplex(Complex v)
    {
        return new(v.Real, v.Imaginary);
    }

    public static implicit operator BigComplex(BigDecimal v)
    {
        return new(v, 0);
    }

    public static implicit operator BigComplex(BigInteger v)
    {
        return new(v, 0);
    }

    public static implicit operator BigComplex(decimal v)
    {
        return new(v, 0);
    }

    public static implicit operator BigComplex(int v)
    {
        return new(v, 0);
    }

    public static implicit operator BigComplex(double v)
    {
        return new(v, 0);
    }

    public static implicit operator BigComplex(float v)
    {
        return new(v, 0);
    }

    public static implicit operator BigComplex(long v)
    {
        return new(v, 0);
    }

    public static explicit operator Complex(BigComplex v)
    {
        return new((double)v.Real, (double)v.Imaginary);
    }

    public static BigComplex operator +(BigComplex v)
    {
        return v;
    }

    public static BigComplex operator -(BigComplex v)
    {
        return new(-v.Real, -v.Imaginary);
    }

    public static BigComplex operator ++(BigComplex v)
    {
        return new(v.Real + 1, v.Imaginary);
    }

    public static BigComplex operator --(BigComplex v)
    {
        return new(v.Real - 1, v.Imaginary);
    }

    public static BigComplex operator +(BigComplex l, BigComplex r)
    {
        return new(l.Real + r.Real, l.Imaginary + r.Imaginary);
    }

    public static BigComplex operator -(BigComplex l, BigComplex r)
    {
        return new(l.Real - r.Real, l.Imaginary - r.Imaginary);
    }

    public static BigComplex operator *(BigComplex l, BigComplex r)
    {
        return new(l.Real * r.Real - l.Imaginary * r.Imaginary, l.Imaginary * r.Real + l.Real * r.Imaginary);
    }

    public static BigComplex operator /(BigComplex l, BigComplex r)
    {
        return new((l.Real * r.Real + l.Imaginary * r.Imaginary) / (r.Real * r.Real + r.Imaginary * r.Imaginary),
        (l.Imaginary * r.Real - l.Real * r.Imaginary) / (r.Real * r.Real + r.Imaginary * r.Imaginary));
    }

    public static BigDecimal Abs(BigComplex v)
    {
        return v.Modulus;
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        if (this.Real != 0) sb.Append(this.Real);
        if (this.Imaginary != 0)
        {
            if (this.Imaginary < 0)
            {
                if (this.Imaginary == -1) sb.Append('-');
                else sb.Append(this.Imaginary);
            }
            else
            {
                if (this.Real != 0)
                {
                    sb.Append('+');
                }
                if (this.Imaginary != 1) sb.Append(this.Imaginary);

            }
            sb.Append('i');
        }
        if (this.Real == 0 && this.Imaginary == 0) sb.Append('0');
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