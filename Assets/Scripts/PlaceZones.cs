using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceZones : MonoBehaviour
{

    public string path;
    public float scale = 1000;


    public Material mat;
    public Color color;

    // Start is called before the first frame update
    void Start()
    {

        GeoJSonParser parser = new GeoJSonParser();
        parser.Parse(path,1);

        Vector3 meanZone = Vector3.zero;

        foreach (List<Vector3> zs in parser.zones)
        {
            for (int i = 0; i < zs.Count; i++)
            {
                zs[i] = ConvertGPStoUCS(new Vector2(zs[i].x, zs[i].z)) / scale;
            }
        }

                int count = 0;
        foreach (List<Vector3> zs in parser.zones)
        {
            foreach (Vector3 pointPos in zs)
            {
                //meanZone += ConvertGPStoUCS(new Vector2(pointPos.x, pointPos.z)) / scale;
                meanZone += pointPos;
                count += 1;
            }
        }
        meanZone /= count;

        foreach (List<Vector3> zs in parser.zones)
        {
            /*LineRenderer lineRenderer;
            lineRenderer = new GameObject(this.name + "Line").AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = mat;
            lineRenderer.colorGradient = gradient;
            int lengthOfLineRenderer = zs.Count;
            lineRenderer.positionCount = lengthOfLineRenderer;*/

            Mesh newMesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();

            Vector3 meanVertex = Vector3.zero;
            for (int i = 0; i < zs.Count; i++)
            {
                //lineRenderer.SetPosition(i, (zs[i] / scale) - meanZone);
                meanVertex += (zs[i]) - meanZone;
                vertices.Add((zs[i]) - meanZone);
                normals.Add(Vector3.up);
            }
            meanVertex /= zs.Count;

            vertices.Add(meanVertex);
            normals.Add(Vector3.up);
            newMesh.SetVertices(vertices);

            newMesh.SetNormals(normals);
            

            int[] triangles = new int[vertices.Count * 3];
            int k = 0;
            int j = 0;
            for (; j < vertices.Count-1; j+=3)
            {
                triangles[j] = k;
                triangles[j + 1] = k + 1;
                triangles[j + 2] = vertices.Count - 1;
                k++;
            }

            triangles[j] = k;
            triangles[j + 1] = 0;
            triangles[j + 2] = vertices.Count - 1;

            newMesh.triangles = triangles;
            GameObject mesh = new GameObject(this.name + "Mesh");
            mesh.AddComponent<MeshFilter>();
            mesh.AddComponent<MeshRenderer>();
            mesh.GetComponent<MeshFilter>().mesh = newMesh;
            mesh.GetComponent<MeshRenderer>().material = mat;
            mesh.GetComponent<MeshRenderer>().material.color = color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // https://github.com/MichaelTaylor3D/UnityGPSConverter/blob/master/GPSEncoder.cs
    private Vector2 _localOrigin = Vector2.zero;
    private float _LatOrigin { get { return _localOrigin.x; } }
    private float _LonOrigin { get { return _localOrigin.y; } }

    private float metersPerLat;
    private float metersPerLon;

    private void FindMetersPerLat(float lat) // Compute lengths of degrees
    {
        float m1 = 111132.92f;    // latitude calculation term 1
        float m2 = -559.82f;        // latitude calculation term 2
        float m3 = 1.175f;      // latitude calculation term 3
        float m4 = -0.0023f;        // latitude calculation term 4
        float p1 = 111412.84f;    // longitude calculation term 1
        float p2 = -93.5f;      // longitude calculation term 2
        float p3 = 0.118f;      // longitude calculation term 3

        lat = lat * Mathf.Deg2Rad;

        // Calculate the length of a degree of latitude and longitude in meters
        metersPerLat = m1 + (m2 * Mathf.Cos(2 * (float)lat)) + (m3 * Mathf.Cos(4 * (float)lat)) + (m4 * Mathf.Cos(6 * (float)lat));
        metersPerLon = (p1 * Mathf.Cos((float)lat)) + (p2 * Mathf.Cos(3 * (float)lat)) + (p3 * Mathf.Cos(5 * (float)lat));
    }

    private Vector3 ConvertGPStoUCS(Vector2 gps)
    {
        FindMetersPerLat(_LatOrigin);
        float zPosition = metersPerLat * (gps.x - _LatOrigin); //Calc current lat
        float xPosition = metersPerLon * (gps.y - _LonOrigin); //Calc current lat
        return new Vector3((float)xPosition, 0, (float)zPosition);
    }
}
