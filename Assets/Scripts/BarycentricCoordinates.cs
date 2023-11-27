using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarycentricCoordinates : MonoBehaviour
{
	public static BarycentricCoordinates barycInstance { get; private set; }

	private void Awake() 
	{
		barycInstance = this;
	}

	/*public float HeightFromBaryc(Vector2 playerPos)
	{
		Vector3 v0 = new Vector3();
		Vector3 v1 = new Vector3(); 
		Vector3 v2 = new Vector3();
		Vector3 baryc = new Vector3(-1, -1, -1);

		for (int i = 0; i < TriangleScript.triangleInstance.IndAmount / 3; i++)
		{

			int i1, i2, i3;
			i1 = TriangleScript.triangleInstance.Indices[i * 3];
			i2 = TriangleScript.triangleInstance.Indices[i * 3 + 1];
			i3 = TriangleScript.triangleInstance.Indices[i * 3 + 2];

			v0 = TriangleScript.triangleInstance.Vertices[i1];
			v1 = TriangleScript.triangleInstance.Vertices[i2];
			v2 = TriangleScript.triangleInstance.Vertices[i3];

			baryc = CalcBarycentricCoords(new Vector2(v0.x, v0.z), new Vector2(v1.x, v1.z), new Vector2(v2.x, v2.z), playerPos);

			if (baryc.x >= 0 && baryc.y >= 0 && baryc.z >= 0)
			{
				break;
			}
		}

		float height = v2.y * baryc.x + v0.y * baryc.y + v1.y * baryc.z;
		
		return height;
	}*/

	// Finds average height in a quad and uses that height value for the given vertex point
    public float CalcAverageHeight(Vector3[] points, Vector2 vertex, float xRange, float zRange)
    {
        float xHalfStep = xRange * 0.5f;
        float zHalfStep = zRange * 0.5f;
        
        Vector2 topLeft = new Vector2(vertex.x - xHalfStep, vertex.y + zHalfStep);
        Vector2 topRight = new Vector2(vertex.x  + xHalfStep, vertex.y + zHalfStep);
        Vector2 bottomLeft = new Vector2(vertex.x - xHalfStep, vertex.y - zHalfStep);
        Vector2 bottomRight = new Vector2(vertex.x + xHalfStep, vertex.y - zHalfStep);

        List<float> heightValues = new List<float>();

        // Finds points that are inside the two triangles
        for (int i = 0; i < points.Length; i++)
        {
            // Finds point inside first triangle
            Vector3 barycentric = BarycentricCoordinates.barycInstance.CalcBarycentricCoords(bottomLeft, topLeft, topRight, new Vector2(points[i].x, points[i].z));

            if (barycentric is { x: >= 0, y: >= 0, z: >= 0 })
            {
                heightValues.Add(points[i].y);
            }
            // Finds point inside second triangle
            else
            {
                barycentric = BarycentricCoordinates.barycInstance.CalcBarycentricCoords(bottomLeft, topRight, bottomRight, new Vector2(points[i].x, points[i].z));
                
                if (barycentric is { x: >= 0, y: >= 0, z: >= 0 })
                    heightValues.Add(points[i].y);
            }
        }

        // Calculates average height-value
        float averageHeight = 0;

        Debug.Log("Height values count: " + heightValues.Count);

        if (heightValues.Count == 0)
        {
            return 0;
        }   
        
        for (int i = 0; i < heightValues.Count; i++)
        {
            averageHeight += heightValues[i];
        }

        averageHeight = averageHeight / heightValues.Count;
        
        // Returns average height value of the points that are inside the two triangles
        return averageHeight;
    }

	public Vector3 CalcBarycentricCoords(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 vertex)
	{
		Vector2 v10 = v1 - v0;
		Vector2 v21 = v2 - v1;

		float area = Vector3.Cross(new Vector3(v10.x, v10.y, 0.0f), new Vector3(v21.x, v21.y, 0.0f)).z;


		Vector2 v0p = v0 - vertex;
		Vector2 v1p = v1 - vertex;
		Vector2 v2p = v2 - vertex;

		float u = Vector3.Cross(new Vector3(v0p.x, v0p.y, 0.0f), new Vector3(v1p.x, v1p.y, 0.0f)).z / area;
		float v = Vector3.Cross(new Vector3(v1p.x, v1p.y, 0.0f), new Vector3(v2p.x, v2p.y, 0.0f)).z / area;
		float w = Vector3.Cross(new Vector3(v2p.x, v2p.y, 0.0f), new Vector3(v0p.x, v0p.y, 0.0f)).z / area;

		Vector3 tempBaryc = new Vector3(u, v, w);

		return tempBaryc;
	}
}
