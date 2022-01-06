using GeoJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public struct FileStruct
{
    public string type;
    public string name;
    public TreeStruct[] features;
}

[System.Serializable]
public struct TreeStruct
{
    public string type;
    public TreeProperties properties;
    public TreeGeometry geometry;
}

[System.Serializable]
public struct TreeProperties
{
    public string essencefrancais;
    public int circonference_cm;
    public int hauteurtotale_m;
    public int hauteurfut_m;
    public int diametrecouronne_m;
    public int rayoncouronne_m;
    public string dateplantation;
    public string genre;
    public string espece;
    public string variete;
    public string essence;
    public string architecture;
    public string localisation;
    public string naturerevetement;
    public string mobilieurbain;
    public int anneeplantation;
    public string commune;
    public string codeinsee;
    public string nomvoie;
    public int codefuv;
    public int identifiant;
    public int numero;
    public int codegenre;
    public int gid;
}

[System.Serializable]
public struct TreeGeometry
{
    public string type;
    public double[] coordinates;
}

public class GeoJSonParser
{

    /*
    * Path to the data file
    * */
    private List<string> paths;

    [HideInInspector]
    public List<TreeStruct> trees = new List<TreeStruct>();

    [HideInInspector]
    public List<List<Vector3>> zones = new List<List<Vector3>>();

    /*
     * Constructor of the GeoJsonParser class
     * parameters string path_ : the path to the data file 
     * */
    public GeoJSonParser()
    {
    }

    public GeoJSonParser(List<string> paths_)
    {
        paths = paths_;
    }

    public void Parse(string path, int type)
    {
        StreamReader reader = new StreamReader(path);
        string geojson = reader.ReadToEnd();
        switch (type)
        {
            case 0:
                FileStruct fileStruct = JsonUtility.FromJson<FileStruct>(geojson);
                foreach (TreeStruct tree in fileStruct.features)
                {
                    trees.Add(tree);
                }

                break;
            case 1:
                //https://github.com/timokorkalainen/Unity-GeoJSONObject

                GeoJSON.FeatureCollection collection = GeoJSON.GeoJSONObject.Deserialize(geojson);
                foreach (FeatureObject feature in collection.features)
                {

                    List<Vector3> zone = new List<Vector3>();
                    foreach (var posObj in feature.geometry.AllPositions())
                    {
                        if (posObj is PositionObjectV3 pos3d)
                        {
                            //Debug.Log(pos3d.position);
                            zone.Add(pos3d.position);
                        }
                    }
                    zones.Add(zone);
                }

                break;
            default:
                Debug.LogError("Erreur de parsing");
                break;
        }


    }

    public void Parse()
    {
        
        for(int i = 0; i < 2; i++)
        {
            StreamReader reader = new StreamReader(paths[i]);
            string geojson = reader.ReadToEnd();
            switch (i)
            {
                case 0:
                    FileStruct fileStruct = JsonUtility.FromJson<FileStruct>(geojson);
                    foreach (TreeStruct tree in fileStruct.features)
                    {
                        trees.Add(tree);
                    }

                    break;
                case 1:
                    //CA MARCHE PAS CAR NESTED ARRAY
                    //TODO : https://github.com/timokorkalainen/Unity-GeoJSONObject
                    /*ZoneFileStruct zoneFileStruct = JsonUtility.FromJson<ZoneFileStruct>(geojson);
                    foreach (ZoneStruct zone in zoneFileStruct.features)
                    {
                        zones.Add(zone);
                    }*/
                   
                    GeoJSON.FeatureCollection collection = GeoJSON.GeoJSONObject.Deserialize(geojson);
                    foreach (FeatureObject feature in collection.features)
                    {

                        List<Vector3> zone = new List<Vector3>();
                        foreach (var posObj in feature.geometry.AllPositions())
                        {
                            if (posObj is PositionObjectV3 pos3d)
                            {
                                //Debug.Log(pos3d.position);
                                zone.Add(pos3d.position);
                            }
                        }
                        zones.Add(zone);  
                    }
                    


                    break;
                default:
                    Debug.LogError("Erreur de parsing");
                    break;
            }
            
            
        }
    }


}
