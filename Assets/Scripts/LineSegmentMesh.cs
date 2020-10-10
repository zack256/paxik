using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSegmentMesh : MonoBehaviour
{
    void RecalculateMeshUVs (Mesh mesh)
    {
        Vector2[] newUVs = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            newUVs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].y);
        }
        mesh.uv = newUVs;
    }

    public void CreateLineSegmentMesh (GameObject lineSegmentObject, GameObject firstPoint, GameObject secondPoint, int numSides)
    {
        float diameter = lineSegmentObject.GetComponent<LineRenderer>().startWidth;
        float radius = diameter * 0.5f;

        Vector3 A = firstPoint.transform.position;
        Vector3 B = secondPoint.transform.position;
        Vector3 AB = B - A;
        float d = Vector3.Dot(AB, A);

        Vector3 C;
        if (A.z != B.z)
        {
            C = new Vector3(0, 0, d / AB.z);
        } else if (A.y != B.y)
        {
            C = new Vector3(0, d / AB.y, 0);
        } else
        {
            C = new Vector3(d / AB.x, 0, 0);
        }

        Vector3 AC = C - A;
        AC.Normalize();
        AC *= radius;   // AC is now AD
        Vector3 D = A + AC;
        Vector3 AE = Vector3.Cross(AC, AB);
        AE.Normalize();
        AE *= radius;   // AE is now AF
        Vector3 F = A + AE;

        Mesh mesh = lineSegmentObject.AddComponent<MeshFilter>().mesh;

        Vector3 G = (A - D) + A;
        Vector3 H = (A - F) + A;
        Vector3 D1 = D + AB;
        Vector3 F1 = F + AB;
        Vector3 G1 = G + AB;
        Vector3 H1 = H + AB;

        mesh.vertices = new Vector3[]
        {
            D, F, G, H, D1, F1, G1, H1
        };
        mesh.triangles = new int[] { 0, 4, 5, 0, 1, 5, 1, 5, 6, 1, 2, 6, 2, 6, 7, 2, 3, 7, 3, 7, 4, 3, 0, 4 }; ;


        mesh.RecalculateNormals();
        RecalculateMeshUVs(mesh);

        MeshCollider meshCollider = lineSegmentObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }
}
