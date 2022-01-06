using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using UnityEngine.Events;


public struct TerrainArea
{
    public Vector3 center;
    public Bounds bounds;
    public float scale;
}

[System.Serializable]
public class TerrainUnityEvent : UnityEvent<TerrainArea>
{
}

public class terrain : MonoBehaviour
{
    public string folderPathGMLTerrain;
    public string nomFichierGMLTerrain;
    public Material matOrigin;
    public float scaleFactor;

    public TerrainUnityEvent OnTerrainLoaded;

    void Start()
    {
        importDonneesGML();
    }

    void importDonneesGML()
    {

        XDocument doc = XDocument.Load($"{folderPathGMLTerrain}{nomFichierGMLTerrain}");
        XElement indoorFeatures = doc.Root;
        XNamespace xsGml = indoorFeatures.GetNamespaceOfPrefix("gml");
        XNamespace xsCore = indoorFeatures.GetNamespaceOfPrefix("core");
        XNamespace xsApp = indoorFeatures.GetNamespaceOfPrefix("app");
        XNamespace xsDem = indoorFeatures.GetNamespaceOfPrefix("dem");


        var results = doc.Elements(xsCore+"CityModel").Select(x => new {
            boundaries = x.Descendants(xsGml + "Envelope").Select(y => new {
                lowerbound = (string)y.Element(xsGml + "lowerCorner"),
                upperbound = (string)y.Element(xsGml + "upperCorner")
            }),
            parcelle=x.Descendants(xsDem+"tin").Select(y=> new {
                posId = y.Element(xsGml + "TriangulatedSurface").Attribute(xsGml + "id").Value,
                position = y.Descendants(xsGml + "LinearRing").Select(z => new {
                    coord = (string)z.Element(xsGml + "posList")
                }).ToList(),
            }),
            texture =x.Descendants(xsApp + "ParameterizedTexture").Select(z=> new {
                URIimage = z.Element(xsApp + "imageURI"),
                target = z.Element(xsApp + "target").Attribute("uri").Value.Remove(0,1),
                target2 = z.Element(xsApp+"target").Descendants(xsApp + "TexCoordList").Select(y => new
                {
                    textCoord = y.Elements(xsApp + "textureCoordinates").ToList()
                }).ToList()
            })
        }).FirstOrDefault();

        
        Vector3 ptMoy = Vector3.zero;
        foreach (var item in results.boundaries)
		{
            string[] coordPtMin = item.lowerbound.Split(' ');
            string[] coordPtMax = item.upperbound.Split(' ');
            ptMoy.x = (float.Parse(coordPtMin[0], CultureInfo.InvariantCulture) + float.Parse(coordPtMax[0], CultureInfo.InvariantCulture))/2;
            ptMoy.y = float.Parse(coordPtMin[2], CultureInfo.InvariantCulture); // stay at min
            ptMoy.z = (float.Parse(coordPtMin[1], CultureInfo.InvariantCulture) + float.Parse(coordPtMax[1], CultureInfo.InvariantCulture)) / 2; 
        }
        //Debug.Log("point moy : "+ptMoy);

        Bounds globalBounds = new Bounds(Vector3.zero, Vector3.zero);
        
        List<GameObject> parcelles = new List<GameObject>();
        
        foreach (var par in results.parcelle)
		{
            int increment = 0;

            //création obj
            GameObject parcelleObj = new GameObject();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            parcelleObj.AddComponent<MeshFilter>();
            parcelleObj.AddComponent<MeshRenderer>();
            Material mat = new Material(matOrigin);

            // création des triangles
            foreach (var pos in par.position)
			{
                string[] subs = pos.coord.Split(' ');
                vertices.Add((new Vector3(float.Parse(subs[0], CultureInfo.InvariantCulture), float.Parse(subs[2], CultureInfo.InvariantCulture), float.Parse(subs[1], CultureInfo.InvariantCulture)) - ptMoy) / scaleFactor);
                vertices.Add((new Vector3(float.Parse(subs[3], CultureInfo.InvariantCulture), float.Parse(subs[5], CultureInfo.InvariantCulture), float.Parse(subs[4], CultureInfo.InvariantCulture)) - ptMoy) / scaleFactor);
                vertices.Add((new Vector3(float.Parse(subs[6], CultureInfo.InvariantCulture), float.Parse(subs[8], CultureInfo.InvariantCulture), float.Parse(subs[7], CultureInfo.InvariantCulture)) - ptMoy) / scaleFactor);
                triangles.Add(increment);
                triangles.Add(increment + 2);
                triangles.Add(increment + 1);
                increment += 3;
            }
            
            
            // application de la texture
            increment = 0;
            foreach (var textu in results.texture)
            {
                if (textu.target == par.posId)
                {
                    mat.mainTexture = Resources.Load(textu.URIimage.Value.Split('.')[0]) as Texture2D;
                    foreach (var item2 in textu.target2)
                    {
                        //Debug.Log(item2.FirstAttribute); //id ring
						foreach (var coordTextu in item2.textCoord)
						{
                            string[] subs = coordTextu.Value.Split(' ');
                            uvs.Add(new Vector2(float.Parse(subs[0], CultureInfo.InvariantCulture), float.Parse(subs[1], CultureInfo.InvariantCulture)));
                            uvs.Add(new Vector2(float.Parse(subs[2], CultureInfo.InvariantCulture), float.Parse(subs[3], CultureInfo.InvariantCulture)));
                            uvs.Add(new Vector2(float.Parse(subs[4], CultureInfo.InvariantCulture), float.Parse(subs[5], CultureInfo.InvariantCulture)));
                            increment += 3;
                        }
                    }
                }
            }
        

            // création du mesh
            Mesh msh = new Mesh();
            msh.vertices = vertices.ToArray();
            msh.uv = uvs.ToArray();
            msh.triangles = triangles.ToArray();
            msh.RecalculateNormals();

            if (globalBounds.size == Vector3.zero)
                globalBounds = msh.bounds;
            else
                globalBounds.Encapsulate(msh.bounds);

            parcelleObj.GetComponent<MeshFilter>().mesh = msh;
            parcelleObj.GetComponent<MeshRenderer>().material = mat;

            parcelleObj.transform.SetParent(this.transform);
            parcelles.Add(parcelleObj);
            
        }

        OnTerrainLoaded.Invoke(new TerrainArea{bounds = globalBounds,center = ptMoy, scale = scaleFactor});
    }    
}
