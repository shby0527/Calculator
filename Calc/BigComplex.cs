using System;
using System.Diagnostics;
using System.Numerics;

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

    // 通过实部虚部创建复数
    public BigComplex(BigDecimal real, BigDecimal imaginary)
    {
        this.Real = real;
        this.Imaginary = imaginary;
        this.Modulus = MyMath.Sqrt(MyMath.Pow(real, 2) + MyMath.Pow(imaginary, 2));
    }

    // 通过相角模长确定复数
    public BigComplex(BigDecimal modulus, BigDecimal argument, bool _)
    {

    }

    public bool Equals(BigComplex other)
    {
        throw new NotImplementedException();
    }
}