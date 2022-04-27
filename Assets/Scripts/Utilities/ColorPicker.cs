using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
public static class ColorPicker
{
    public readonly struct ColorPickerResult
    {
        public readonly bool hit;
        public readonly Color color;
        public readonly float height;

        public ColorPickerResult(bool hit, Color color, float height)
        {
            this.hit = hit;
            this.color = color;
            this.height = height;
        }
    }

    public static ColorPickerResult[] ColorpickMultiple(GameObject prefab, Ray[] rays)
    {
        var mesh = prefab.GetComponent<MeshFilter>().mesh;
        var materials = prefab.GetComponent<MeshRenderer>().materials;

        var output = new ColorPickerResult[rays.Length];

        // split submeshes
        var meshes = SplitMesh(mesh);

        // raycast submeshes
        float[][] distances = new float[meshes.Length][];
        for (int i = 0; i < mesh.subMeshCount; i++)
            distances[i] = Raycast(prefab ,rays);

        // calculate result
        for (int j=0;j<rays.Length;j++)
        {
            int bestIndex = 0;
            float bestDist=distances[0][j];
            for (int i = 1; i < meshes.Length;i++)
            {
                if (bestDist > distances[i][j])
                {
                    bestIndex = i;
                    bestDist = distances[i][j];
                }
            }
            if (bestDist != float.PositiveInfinity)
                output[j] = new(true, materials[bestIndex].color, 10 - bestDist);
        }

        return output;
    }

    static Mesh[] SplitMesh(Mesh mesh)
    {
        var output = new Mesh[mesh.subMeshCount];

        // https://answers.unity.com/questions/1213025/separating-submeshes-into-unique-meshes.html
        for (int i=0;i<mesh.subMeshCount;i++)
        {
            // optimisation: prune unused vertices
            Mesh m = new();
            m.indexFormat = mesh.vertexCount > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            m.vertices = mesh.vertices;
            //m.uv = mesh.uv; // probably not neccesary
            m.triangles = mesh.GetTriangles(i);

            output[i] = m;
        }

        return output;
    }

    static float[] Raycast(GameObject prefab, Ray[] rays)
    {
        var output = new float[rays.Length];

        MeshCollider tempCollider = null;
        if (!prefab.TryGetComponent<Collider>(out var collider))
        {
            tempCollider = prefab.AddComponent<MeshCollider>();
        }

        for (int i = 0; i < rays.Length; i++)
        {
            bool b = prefab.scene.GetPhysicsScene().Raycast(rays[i].origin, rays[i].direction, out RaycastHit hitInfo);
            output[i] = b ? hitInfo.distance : float.PositiveInfinity;
        }

        if (tempCollider != null)
            Object.Destroy(tempCollider);

        return output;
    }
}
#endif