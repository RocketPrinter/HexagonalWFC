using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct HexSide
{
    public readonly int value;
    public HexSide Opposite => new(value + 3);

    public HexSide(int value=0)
    {
        this.value = value % 6;
    }

    //operators
    public static HexSide operator +(HexSide a, HexSide b) => new(a.value + b.value);
    public static HexSide operator -(HexSide a, HexSide b) => new(a.value - b.value);

    public static bool operator ==(HexSide a, HexSide b) => a.value == b.value;
    public static bool operator !=(HexSide a, HexSide b) => a.value != b.value;
    public static bool operator <(HexSide a, HexSide b) => a.value < b.value;
    public static bool operator >(HexSide a, HexSide b) => a.value > b.value;

    public static implicit operator int(HexSide a) => a.value;
    public static implicit operator HexSide(int a) => new(a);

    // methods
    public override bool Equals(object obj)
    {
        return obj is HexSide side &&
               value == side.value;
    }
    public override int GetHashCode() => HashCode.Combine(value);
}
