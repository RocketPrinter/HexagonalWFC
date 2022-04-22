using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MeshColorPicker
{
    public Color?[] Pick(Mesh mesh, Ray[] rays)
    {
        Color?[] output = new Color?[rays.Length];

        // split into submeshes
        var meshes = SplitMesh(mesh);

        // raycast submeshes and get result
        for (int i=0;i<rays.Length;i++)
        {
            var r = meshes.Select(mesh => ColorpickMesh(mesh, rays[i]))
                .Where(x => x.hit)
                .Aggregate((best, x) => best.d < x.d ? best : x);

            output[i] = r.hit ? r.color : null;
        }

        return output;
    }

    static Mesh[] SplitMesh(Mesh mesh)
    {

    }

    static (bool hit, float d, Color color) ColorpickMesh(Mesh mesh, Ray r)
    {

    }
}
