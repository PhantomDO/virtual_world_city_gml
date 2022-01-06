using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lisenced under MIT <br/>
/// https://github.com/twobitcoder101/Polygon-Triangulation/blob/main/TriangulatePolygon.cs
/// </summary>
public static class TriangulatePolygon
{

    public const bool DEBUGMODE = false;

    public const int maxiterations = 32;

    /// <summary>
    /// Computes the triangulation of a polygon defined by a 2d vertex array, and generates a triangle array to display it.
    /// </summary>
    /// <param name="vertices">Vertex location input for the polygon.</param>
    /// <param name="triangles">Sets this array pointer to vertices pointers. Undefined if this method returns false.</param>
    /// <param name="errorMessage">Error message outing. This pointer will be set to an error description if this method returns false</param>
    /// <returns>True if the triangulation was sucessful.</returns>
    public static bool Triangulate(Vector2[] vertices, out int[] triangles, out string errorMessage, bool inversePoly)
    {
        triangles = null;
        errorMessage = "Unknown error, Iteration culled after" + (maxiterations + 2) + " attempts.";

        if (vertices.Length == 4)
        {
            triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            return true;
        }

        if (DEBUGMODE) for (int i = 0; i < vertices.Length; i++)
                Debug.Log("Poly triangulation flat : " + vertices[i]);

        if (vertices is null)
        {
            errorMessage = "The vertex list is null.";
            return false;
        }
        else if (vertices.Length < 3)
        {
            errorMessage = "The vertex list must have at least 3 vertices.";
            return false;
        }
        else if (vertices.Length > maxiterations)
        {
            errorMessage = "The max vertex list length is " + maxiterations;
            return false;
        }

        List<int> indexList = new List<int>();
        for (int i = 0; i < vertices.Length; i++)
            indexList.Add(i);

        int totalTriangleCount = vertices.Length - 2;
        int totalTriangleIndexCount = totalTriangleCount * 3;

        triangles = new int[totalTriangleIndexCount];
        int triangleIndexCount = 0;

        int alarm = maxiterations + 2;

        while (indexList.Count > 3 && alarm >= 0)
        {
            alarm--;
            for (int i = 0; i < indexList.Count; i++)
            {
                int a = indexList[i];
                int b = GetLooping(indexList, i - 1);
                int c = GetLooping(indexList, i + 1);

                Vector2 va = vertices[a];
                Vector2 vb = vertices[b];
                Vector2 vc = vertices[c];

                Vector2 va_to_vb = vb - va;
                Vector2 va_to_vc = vc - va;

                // Is ear test vertex convex?
                if ((!inversePoly && Cross2(va_to_vb, va_to_vc) < 0f) || (inversePoly && Cross2(va_to_vb, va_to_vc) >= 0f))
                {
                    if (DEBUGMODE) Debug.Log("Convex angle, skipping " + i + " with va_to_vb=" + va_to_vb + " and va_to_vc=" + va_to_vc);
                    continue;
                }

                bool isEar = true;

                // Does test ear contain any polygon vertices?
                for (int j = 0; j < vertices.Length; j++)
                {
                    if (j == a || j == b || j == c)
                    {
                        continue;
                    }

                    Vector2 p = vertices[j];

                    if (IsPointInTriangle(p, vb, va, vc))
                    {
                        isEar = false;
                        if (DEBUGMODE) Debug.Log("Point is in triangle. Point " + p + " at index " + j + " is not in triangle {" + vb + va + vc + "}");
                        break;
                    }
                }

                if (isEar)
                {
                    triangles[triangleIndexCount++] = b;
                    triangles[triangleIndexCount++] = c;
                    triangles[triangleIndexCount++] = a;

                    if (DEBUGMODE) Debug.Log("Adding triangle for b.a.c. " + b + "/" + a + "/" + c + " / culled vertice index : " + i);

                    indexList.RemoveAt(i);
                    break;
                }
            }
        }

        triangles[triangleIndexCount++] = indexList[0];
        triangles[triangleIndexCount++] = indexList[2];
        triangles[triangleIndexCount++] = indexList[1];

        return alarm > 0;
    }

    public static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 ab = b - a;
        Vector2 bc = c - b;
        Vector2 ca = a - c;

        Vector2 ap = p - a;
        Vector2 bp = p - b;
        Vector2 cp = p - c;

        float cross1 = Cross2(ab, ap);
        float cross2 = Cross2(bc, bp);
        float cross3 = Cross2(ca, cp);

        if (cross1 > 0f || cross2 > 0f || cross3 > 0f)
        {
            return false;
        }

        return true;
    }

    private static T GetLooping<T>(List<T> storage, int index)
    {
        if (index < 0)
            return storage[index % storage.Count + storage.Count];
        return storage[index % storage.Count];
    }

    private static float Cross2(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }
}
