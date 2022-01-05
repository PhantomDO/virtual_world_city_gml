using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.XR;
#endif

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class RoadDisplay : MonoBehaviour
{
    [System.Serializable]
    public struct RoadPoint
    {
        public Vector2 position;
        public float height;
        public float width;

        public RoadPoint(Vector2 position, float height = 0, float width = 1)
        {
            this.position = position;
            this.height = height;
            this.width = width;
        }
    }

    public RoadPoint[] points;
    public int cornerVertices = 0;

    private Mesh roadMesh;

    private void Start()
    {
        UpdateMesh();
    }

    public Vector2 GetPointRight(int point, out Vector2 forward)
    {
        if (points.Length > 0)
        {
            if (point == 0)
            {
                forward = (points[point + 1].position - points[point].position).normalized;
                Vector2 normal = new Vector2(-forward.y, forward.x).normalized;
                return normal* points[point].width;
            }
            else if (point == points.Length - 1)
            {
                forward = (points[point].position - points[point-1].position).normalized;
                Vector2 normal = new Vector2(-forward.y, forward.x).normalized;
                return normal * points[point].width;
            }
            else
            {
                Vector2 line = points[point].position - points[point - 1].position;
                Vector2 normal = new Vector2(-line.y, line.x).normalized;

                Vector2 roadIn = (points[point-1].position - points[point].position).normalized;
                Vector2 roadOut = (points[point + 1].position - points[point].position).normalized;

                Vector2 tangent = ((points[point + 1].position - points[point].position).normalized +
                                   (points[point].position - points[point - 1].position).normalized).normalized;

                Vector2 miter = new Vector2(-tangent.y, tangent.x);
                float length = Mathf.Min(points[point].width / Vector2.Dot(miter, normal), points[point].width*3);

                forward = roadOut;
                return miter.normalized*length;
            }
        }

        forward = Vector2.up;
        return Vector2.right;
    }

    public Vector3 GetPointPos(int point)
    {
        return new Vector3(points[point].position.x, points[point].position.y, points[point].height);
    }

    public void UpdateMesh()
    {
        if (points.Length > 0)
        {
            roadMesh = new Mesh();

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> triangles = new List<int>();

            float length = 0;

            for (int i = 0; i < points.Length; i++)
            {
                if (i != 0)
                    length += (points[i].position - points[i - 1].position).magnitude;

                Vector2 right = GetPointRight(i, out Vector2 forward);

                if (i != 0 && i < points.Length - 1 && cornerVertices > 0)
                {
                    Vector2 lineIn = (points[i].position - points[i - 1].position);
                    Vector2 lineOut = (points[i + 1].position - points[i].position);
                    
                    if (Mathf.FloorToInt(Mathf.Abs(Vector2.Dot(lineIn.normalized, lineOut.normalized))) == 1)
                    {
                        Vector3 pointLeft = points[i].position - right;
                        Vector3 pointRight = points[i].position + right;

                        vertices.Add(pointRight + points[i].height * Vector3.forward);
                        vertices.Add(pointLeft + points[i].height * Vector3.forward);

                        uv.Add(new Vector2(length, 1));
                        uv.Add(new Vector2(length, 0));
                        
                        int index = vertices.Count - 2;

                        triangles.AddRange(new int[] { index, index + 2, index + 1 });
                        triangles.AddRange(new int[] { index + 2, index + 3, index + 1 });
                    }
                    else
                    {
                        float side = -Mathf.Sign(Vector3.Cross(lineIn, lineOut).z);

                        Vector2 normalIn = new Vector2(-lineIn.y, lineIn.x).normalized * (side * points[i].width);
                        Vector2 normalOut = new Vector2(-lineOut.y, lineOut.x).normalized * (side * points[i].width);

                        

                        if (side < 0)
                        {
                            int cornerIndex = vertices.Count;

                            Vector3 pointCorner = points[i].position - right * side;
                            vertices.Add(pointCorner + points[i].height * Vector3.forward);
                            uv.Add(new Vector2(length, 1));

                            int smoothStart = vertices.Count;

                            Vector3 pointSmooth = points[i].position + normalIn;
                            vertices.Add(pointSmooth + points[i].height * Vector3.forward);
                            uv.Add(new Vector2(length, 0));


                            float smoothAngle = Vector2.Angle(normalIn, normalOut);
                            float angleStep = smoothAngle / (cornerVertices + 1);

                            for (int j = 1; j <= cornerVertices; j++)
                            {
                                Vector3 pointSmoothStep = (Vector3)points[i].position + Quaternion.Euler(0, 0, j*angleStep) * normalIn;
                                vertices.Add(pointSmoothStep + points[i].height * Vector3.forward);
                                uv.Add(new Vector2(length, 0));

                                triangles.AddRange(new int[] { cornerIndex, vertices.Count-1, vertices.Count-2 });
                            }


                            int smoothEnd = vertices.Count;

                            Vector3 pointSmoothEnd = points[i].position + normalOut;
                            vertices.Add(pointSmoothEnd + points[i].height * Vector3.forward);
                            uv.Add(new Vector2(length, 0));

                            int index = vertices.Count;

                            triangles.AddRange(new int[] { cornerIndex, smoothEnd, smoothEnd - 1 });

                            triangles.AddRange(new int[] { cornerIndex, index, smoothEnd });
                            triangles.AddRange(new int[] { index, index + 1, smoothEnd });
                        }
                        else
                        {
                            int smoothStart = vertices.Count;

                            Vector3 pointSmooth = points[i].position + normalIn;
                            vertices.Add(pointSmooth + points[i].height * Vector3.forward);
                            uv.Add(new Vector2(length, 1));

                            int cornerIndex = vertices.Count;

                            Vector3 pointCorner = points[i].position - right * side;
                            vertices.Add(pointCorner + points[i].height * Vector3.forward);
                            uv.Add(new Vector2(length, 0));


                            float smoothAngle = Vector2.Angle(normalIn, normalOut);
                            float angleStep = smoothAngle / (cornerVertices + 1);

                            for (int j = 1; j <= cornerVertices; j++)
                            {
                                Vector3 pointSmoothStep = (Vector3)points[i].position + Quaternion.Euler(0, 0, j * -angleStep) * normalIn;
                                vertices.Add(pointSmoothStep + points[i].height * Vector3.forward);
                                uv.Add(new Vector2(length, 1));

                                if(j==1)
                                    triangles.AddRange(new int[] { cornerIndex, vertices.Count - 3, vertices.Count - 1 });
                                else
                                    triangles.AddRange(new int[] { cornerIndex, vertices.Count - 2, vertices.Count - 1 });
                            }


                            int smoothEnd = vertices.Count;

                            Vector3 pointSmoothEnd = points[i].position + normalOut;
                            vertices.Add(pointSmoothEnd + points[i].height * Vector3.forward);
                            uv.Add(new Vector2(length, 1));

                            int index = vertices.Count;

                            triangles.AddRange(new int[] { smoothEnd-1, smoothEnd, cornerIndex });

                            triangles.AddRange(new int[] { smoothEnd, index + 1, cornerIndex });
                            triangles.AddRange(new int[] { index, index + 1, smoothEnd });
                        }
                    }

                    
                }
                else
                {
                    Vector3 pointLeft = points[i].position - right;
                    Vector3 pointRight = points[i].position + right;

                    vertices.Add(pointRight + points[i].height * Vector3.forward);
                    vertices.Add(pointLeft + points[i].height * Vector3.forward);

                    uv.Add(new Vector2(length, 1));
                    uv.Add(new Vector2(length, 0));

                    if (i < points.Length - 1)
                    {
                        int index = vertices.Count - 2;

                        triangles.AddRange(new int[] { index, index + 2, index + 1 });
                        triangles.AddRange(new int[] { index + 2, index + 3, index + 1 });
                    }
                }

                
            }

            roadMesh.vertices = vertices.ToArray();
            roadMesh.uv = uv.ToArray();
            roadMesh.triangles = triangles.ToArray();
            roadMesh.RecalculateNormals();

            GetComponent<MeshFilter>().sharedMesh = roadMesh;
        }
        
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(RoadDisplay))]
public class BezierCurveDrawer : Editor
{
    private int currentHandle = 0;

    void OnSceneGUI()
    {
        Event evt = Event.current;
        

        RoadDisplay road = (RoadDisplay)target;
        Handles.CapFunction handleCap = Handles.SphereHandleCap;

        if (road.points != null && road.points.Length > 0)
        {
            Handles.matrix = road.transform.localToWorldMatrix;

            for (int i = 0; i < road.points.Length; i++)
            {
                Handles.color = Color.blue;

                Vector3 point = road.GetPointPos(i);
                Vector3 roadRight = road.GetPointRight(i, out Vector2 roadForward);

                Handles.DrawLine(point - roadRight, point + roadRight);

                if (i < road.points.Length - 1 && road.points.Length > 1)
                {
                    Vector3 nextRight = road.GetPointRight(i + 1, out Vector2 _);
                    Vector3 next = road.GetPointPos(i + 1);

                    Handles.DrawLine(point, next);

                    Handles.DrawLine(point - roadRight, next - nextRight);
                    Handles.DrawLine(point + roadRight, next + nextRight);
                }
                else if (i != 0 && road.points[i].position==road.points[i-1].position)
                {
                    road.points[i].position += Vector2.up;
                }

                if (i > 0 && i < road.points.Length - 1)
                {
                    Handles.DrawWireDisc(point,Vector3.forward, road.points[i].width);

                    Vector2 lineIn = (road.points[i].position - road.points[i - 1].position);
                    Vector2 lineOut = (road.points[i + 1].position - road.points[i].position);

                    float side = -Mathf.Sign(Vector3.Cross(lineIn, lineOut).z);

                    Vector3 normalIn = new Vector2(-lineIn.y, lineIn.x).normalized * (side * road.points[i].width);
                    Vector3 normalOut = new Vector2(-lineOut.y, lineOut.x).normalized * (side * road.points[i].width);

                    Handles.color = Color.red;
                    Handles.DrawLine(point,point+ normalIn);
                    Handles.color = Color.yellow;
                    Handles.DrawLine(point,point+ normalOut);

                    Handles.color = Color.blue;
                }

                bool isCurrent = currentHandle == i;

                if (isCurrent)
                {
                    if (Tools.current == Tool.Move)
                    {
                        Vector3 pos = new Vector3(road.points[i].position.x, road.points[i].position.y,
                            road.points[i].height);

                        pos = Handles.PositionHandle(pos, Quaternion.identity);

                        road.points[i].position = pos;
                        road.points[i].height = pos.z;
                    }
                    else if (Tools.current == Tool.Scale)
                    {
                        road.points[i].width = Mathf.Max(Handles.ScaleSlider(road.points[i].width, road.points[i].position,
                            roadRight, Quaternion.identity, 1, 0),0.1f);
                    }

                    Handles.color = Color.red;
                    Handles.SphereHandleCap(0, point, Quaternion.identity, 0.05f, EventType.Repaint);
                }
                else
                {
                    Handles.color = Color.green;

                    if (Handles.Button(point, Quaternion.identity, 0.05f, 0.05f, handleCap))
                    {
                        currentHandle = i;
                    }
                }

                Handles.Label(point, i.ToString());
            }
        }

        if (evt.type == EventType.Layout)
        {
            road.UpdateMesh();
        }

    }
}
#endif