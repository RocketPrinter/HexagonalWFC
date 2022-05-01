using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct Connection
{
    public readonly TerrainType left, right;
    public readonly UtilityType utility;

    public Connection(TerrainType left, UtilityType utility, TerrainType right)
    {
        this.left = left;
        this.right = right;
        this.utility = utility;
    }

    // operators
    public static bool operator ==(Connection a, Connection b) => a.left == b.left && a.right == b.right && a.utility == b.utility;
    public static bool operator !=(Connection a, Connection b) => a.left != b.left || a.right != b.right || a.utility != b.utility;

    // methods
    public bool Connects(Connection other) => left == other.right && utility == other.utility && right == other.left;
    public override bool Equals(object obj)
    {
        return obj is Connection connection && this == connection;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(left, right, utility);
    }
}
