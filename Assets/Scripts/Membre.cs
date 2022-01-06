using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Membre
{
    public string Id { get; private set; }
    public string Type { get; private set; }
    public string IntTexId { get; private set; }
    public string ExtTexId { get; private set; }
    public List<Vector3> positionsExt { get; private set; }
    public List<Vector3> positionsInt { get; private set; }
    public List<Vector2> textures { get; set; }

    public Membre(string identifiant, string type)
    {
        Id = identifiant;
        Type = type;
        IntTexId = "";
        ExtTexId = "";
        positionsExt = new List<Vector3>();
        positionsInt = new List<Vector3>();
    }

    /// <summary>
    /// Internal buffer for ear clipping multicall optimisation. Do not touch.
    /// </summary>
    private int[] clipBuffer;
    /// <summary>
    /// Computes a list of triangular surfaces for a polygon. This method is buffered.
    /// </summary>
    /// <returns>An array of vertice index containing triangle data for this polygon. Multiple calls to the function will return a buffered result, not to be modified.</returns>
    public int[] EarClipping()
    {
        if (clipBuffer != null) return clipBuffer;
        Vector2[] workbuffer = new Vector2[positionsExt.Count];

        Vector3 cross = Vector3.Cross((positionsExt[0] - positionsExt[1]).normalized, (positionsExt[0] - positionsExt[2]).normalized).normalized;
        bool vertical = Mathf.Abs(Vector3.Dot(cross, Vector3.up)) < 0.01f;

        Debug.Log("-------------------------------------------------------------");
        Debug.Log(this);
        Debug.Log("Vectors used to define plan : " + (positionsExt[0] - positionsExt[1]).normalized + " and " + (positionsExt[0] - positionsExt[2]).normalized + " generate cross : " + cross);
        Debug.Log("Dot check" + Vector3.Dot(cross, Vector3.up) + " / vertical " + vertical);

        for (int i = 0; i < workbuffer.Length; i++)
            workbuffer[i] = new Vector2((float)positionsExt[i].x, vertical ? (float)(positionsExt[i].y + positionsExt[i].z * 0.17f) : (float)positionsExt[i].z);

        int[] answer;
        string error;
        if (!TriangulatePolygon.Triangulate(workbuffer, out answer, out error, false)) {
            Debug.Log("Triangulation error : " + error + "\nTrying with anticonvex polygonisation...");
            if (!TriangulatePolygon.Triangulate(workbuffer, out answer, out error, true))
                Debug.Log("Triangulation error : " + error + "\nGiving up on this one. Sorry.");
        }
            

        clipBuffer = answer;
        return answer;
    }
    /// <summary>
    /// Set the poslist of an exterior surface
    /// </summary>
    /// <param name="id"></param>
    /// <param name="positions"></param>
    public void SetExt(string id, List<Vector3> positions)
    {
        //Adding the ExtId
        ExtTexId = id;
        //Adding the positions
        positionsExt = positions;


    }
    /// <summary>
    /// Set the poslist of an interior surface
    /// </summary>
    /// <param name="id"></param>
    /// <param name="positions"></param>
    public void SetInt(string id, List<Vector3> positions)
    {
        //Adding the ExtId
        IntTexId = id;
        //Adding the positions
        positionsInt = positions;
    }

    public override string ToString()
    {
        string toreturn = "Member " + Id + " : {";
        for (int i = 0; i < positionsExt.Count; i++)
            toreturn += (i == 0 ? "" : ",") + positionsExt[i];
        return toreturn + "}";
    }
}
