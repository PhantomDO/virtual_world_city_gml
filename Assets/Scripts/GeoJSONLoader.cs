using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GeoJSON;

public class GeoJSONLoader : MonoBehaviour {

	public TextAsset encodedGeoJSON;
    public RoadDisplay roadPrefab;

	public GeoJSON.FeatureCollection collection;

	// Use this for initialization
	public void LoadRoads (TerrainArea terrainArea)
    {
        collection = GeoJSON.GeoJSONObject.Deserialize(encodedGeoJSON.text); 

        if (collection.features.Count > 0)
        {
            var pos = SetupPositions(collection, terrainArea, out uint count);
            //Debug.LogError(count);
            StartCoroutine(SpawnRoads(pos));

        }
    }

    private IEnumerator SpawnRoads(List<List<RoadDisplay.RoadPoint>> pos)
    {
        int spawnCount = 20;
        foreach (List<RoadDisplay.RoadPoint> points in pos)
        {
            RoadDisplay road = Instantiate(roadPrefab,transform);
            road.points = points.ToArray();

            road.UpdateMesh();

            spawnCount--;
            if (spawnCount <= 0)
            {
                spawnCount = 20;
                yield return null;
            }
        }
    }

    public List<List<RoadDisplay.RoadPoint>> SetupPositions(FeatureCollection coll, TerrainArea terrainArea, out uint count)
    {
        count = 0;
        var center = terrainArea.center / Mathf.Max(1, terrainArea.scale);
        var cartesianPositions = new List<List<RoadDisplay.RoadPoint>>();
        foreach (var feature in coll.features)
        {
            var allPositions = feature.geometry.AllPositions();
            var allPositionsList = new List<RoadDisplay.RoadPoint>();

            float width = 1;
            if (feature.properties.TryGetValue("LARGEUR", out string widthValue))
            {
                width = float.Parse(widthValue);
            }

            foreach (var pos in allPositions)
            {
                var position =
                    (pos is PositionObjectV3 pos3D
                        ? pos3D.position
                        : GPSEncoder.GPSToUCS(pos.longitude, pos.latitude)) 
                    / Mathf.Max(1, terrainArea.scale);

                position -= center;

                //Debug.Log($"Position: {position}");
                if(terrainArea.bounds.Contains(position))
                    allPositionsList.Add(new RoadDisplay.RoadPoint(new Vector2(position.x, position.z), position.y, 1 / terrainArea.scale * width));
            }
            if(allPositionsList.Count > 1)
                cartesianPositions.Add(allPositionsList);
        }

        return cartesianPositions;
    }

}
