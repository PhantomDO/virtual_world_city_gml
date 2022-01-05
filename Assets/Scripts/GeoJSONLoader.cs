using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GeoJSON;

public class GeoJSONLoader : MonoBehaviour {

	public TextAsset encodedGeoJSON;
    public RoadDisplay roadPrefab;

	public GeoJSON.FeatureCollection collection;
    public float scaleFactor = 100f;

	// Use this for initialization
	void Start ()
    {
        collection = GeoJSON.GeoJSONObject.Deserialize(encodedGeoJSON.text); 

        if (collection.features.Count > 0)
        {
            var pos = SetupPositions(collection, scaleFactor, out uint count);
            //Debug.LogError(count);
            StartCoroutine(SpawnRoads(pos));

        }
    }

    private IEnumerator SpawnRoads(List<List<Vector3>> pos)
    {
        foreach (List<Vector3> points in pos)
        {
            RoadDisplay road = Instantiate(roadPrefab,transform);
            road.points = new RoadDisplay.RoadPoint[points.Count];

            for (int i = 0; i < road.points.Length; i++)
            {
                road.points[i] = new RoadDisplay.RoadPoint(new Vector2(points[i].x, points[i].z), points[i].y,1/scaleFactor);
            }

            road.UpdateMesh();
            yield return null;
        }
    }

    public List<List<Vector3>> SetupPositions(FeatureCollection coll, float scale, out uint count)
    {
        count = 0;
        var center = Vector3.zero;
        var cartesianPositions = new List<List<Vector3>>();
        foreach (var feature in coll.features)
        {
            var allPositions = feature.geometry.AllPositions();
            var allPositionsList = new List<Vector3>();
            foreach (var pos in allPositions)
            {
                var position =
                    (pos is PositionObjectV3 pos3D
                        ? pos3D.position
                        : GPSEncoder.GPSToUCS(pos.longitude, pos.latitude)) 
                    / Mathf.Max(1, scale);

                count++;
                center += position;

                //Debug.Log($"Position: {position}");
                allPositionsList.Add(position);
            }
            cartesianPositions.Add(allPositionsList);
        }

        center /= count;

        for (int i = 0; i < cartesianPositions.Count; i++)
        {
            for (int j = 0; j < cartesianPositions[i].Count; j++)
            {
                cartesianPositions[i][j] -= center;
            }
        }

        return cartesianPositions;
    }

}
