using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public readonly struct HexPosition
{
    public int X { get; init; }
    public int Y { get; init; }

    // properties
    public HexPosition Top => new HexPosition(X, Y + 1);
    public HexPosition TopRight => new HexPosition(X + 1, Y);
    public HexPosition BottomRight => new HexPosition(X + 1, Y - 1);
    public HexPosition Bottom => new HexPosition(X, Y - 1);
    public HexPosition BottomLeft => new HexPosition(X - 1, Y);
    public HexPosition TopLeft => new HexPosition(X - 1, Y + 1);

    // constructors
    public HexPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    // operators
    #region operators
    public static bool operator ==(HexPosition a, HexPosition b)
    {
        return a.X == b.X && a.Y == b.Y;
    }
    public static bool operator !=(HexPosition a, HexPosition b)
    {
        return a.X != b.X || a.Y != b.Y;
    }
    public static HexPosition operator +(HexPosition a, HexPosition b)
    {
        return new HexPosition(a.X + b.X, a.Y + b.Y);
    }
    public static HexPosition operator -(HexPosition a, HexPosition b)
    {
        return new HexPosition(a.X - b.X, a.Y - b.Y);
    }
    public static HexPosition operator *(HexPosition a, int b)
    {
        return new HexPosition(a.X * b, a.Y * b);
    }
    public static HexPosition operator /(HexPosition a, int b)
    {
        return new HexPosition(a.X / b, a.Y / b);
    }
    public static explicit operator Vector2Int(HexPosition hex) => new Vector2Int(hex.X, hex.Y);
    public static explicit operator HexPosition(Vector2Int v2) => new HexPosition(v2.x, v2.y);
    #endregion

    #region methods
    public int Distance(HexPosition other)
    {
        var o = this - other;
        if (o.X * o.Y >= 0)
            return Mathf.Abs(o.X) + Mathf.Abs(o.Y);

        if (o.X < 0)
            o = new(-o.X, -o.Y);

        if (o.X >= -o.Y)
            return o.X;
        return -o.Y;
    }
    public Vector2 ToGrid() => new Vector2(X * 1.5f, MathF.Sqrt(3) * (Y + (float) X/2));
    public IEnumerable<HexPosition> GetNeighbours()
    {
        yield return Top;
        yield return TopRight;
        yield return BottomRight;
        yield return Bottom;
        yield return BottomLeft;
        yield return TopLeft;
    }
    public static IEnumerable<Vector2> GetVertexOffsets()
    {
        // todo: cache
        float h = Mathf.Sqrt(3) / 2;
        yield return new Vector2(0.5f, h);
        yield return new Vector2(1f, 0);
        yield return new Vector2(0.5f, -h);
        yield return new Vector2(-0.5f, -h);
        yield return new Vector2(-1f, 0);
        yield return new Vector2(-0.5f, h);
    }
    public static IEnumerable<Vector2> GetEdgeCenterOffsets()
    {
        // todo: cache
        float h = Mathf.Sqrt(3) / 2;
        yield return new Vector2(0, h);
        yield return new Vector2(0.75f, h/2);
        yield return new Vector2(0.75f, -h/2);
        yield return new Vector2(0, -h);
        yield return new Vector2(-0.75f, -h/2);
        yield return new Vector2(-0.75f, h/2);
    }

    public override bool Equals(object obj)
    {
        return obj is HexPosition position &&
               X == position.X &&
               Y == position.Y;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
    public override string ToString() => $"hex {X} {Y}";
    #endregion
};
