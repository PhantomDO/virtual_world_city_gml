using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlaceTrees : MonoBehaviour
{
    private List<GameObject> _trees;

    public GameObject treePrefab;
    //public string path;
    public string path;
    public float spaceBetweenTrees = 1000;
    public float treeScale = 0.02f;

    private void Start()
    {
        _trees = new List<GameObject>();
        Vector3 mean = Vector3.zero;

        //GeoJSonParser parser = new GeoJSonParser(path);
        GeoJSonParser parser = new GeoJSonParser();
        parser.Parse(path, 0);

        foreach (TreeStruct ts in parser.trees)
        {
            GameObject tree = Instantiate(treePrefab, transform);
            tree.name = "[" + ts.properties.identifiant + "] " + ts.properties.essencefrancais;
            Vector3 treePos = ConvertGPStoUCS(new Vector2((float)ts.geometry.coordinates[0], (float)ts.geometry.coordinates[1]))/spaceBetweenTrees;
            //Vector3 treePos = new Vector3((float)ts.geometry.coordinates[0], 0, (float)ts.geometry.coordinates[1]) * spaceBetweenTrees;
            tree.transform.position = treePos;

            tree.GetComponent<CustomTree>().ReadValues(ts.properties, treeScale);

            _trees.Add(tree);

            mean += treePos;
        }

        mean /= parser.trees.Count;

        foreach (GameObject tree in _trees)
            tree.transform.position = tree.transform.position - mean;

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
