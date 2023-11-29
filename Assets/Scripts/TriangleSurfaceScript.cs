using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using UnityEngine.UIElements;


public class Triangle
{
    public Triangle(int i0, int i1, int i2, int triangle0, int triangle1, int triangle2, int triangleIndex)
    {
        indices = new[]{i0, i1, i2};
        neighbours = new[]{triangle0, triangle1, triangle2};
        index = triangleIndex;
    }

    public int[]indices;
    public int[]neighbours;
    public int index;

    public Vector3 surfaceNormal;
    public Vector3 unitNormal;

    void GetNormalVectors()
    {
        Vector3 v1 = TriangleSurfaceScript.triangleSurfaceInstance.verticesList[indices[1]] - TriangleSurfaceScript.triangleSurfaceInstance.verticesList[indices[0]];
        Vector3 v2 = TriangleSurfaceScript.triangleSurfaceInstance.verticesList[indices[2]] - TriangleSurfaceScript.triangleSurfaceInstance.verticesList[indices[0]];
        Vector3 normal = Vector3.Cross(v1, v2);
        surfaceNormal = normal;
    }

    public void NormalizeNormal()
    {
        unitNormal = surfaceNormal;
        unitNormal.Normalize();
    }

    public bool IsInTriangle(Vector3 ballPos)
	{
		Vector3 baryc = new Vector3();
        baryc = BarycentricCoordinates.barycInstance.CalcBarycentricCoords
        (
            new Vector2(TriangleSurfaceScript.triangleSurfaceInstance.verticesList[indices[0]].x, TriangleSurfaceScript.triangleSurfaceInstance.verticesList[indices[0]].z),
            new Vector2(TriangleSurfaceScript.triangleSurfaceInstance.verticesList[indices[1]].x, TriangleSurfaceScript.triangleSurfaceInstance.verticesList[indices[1]].z),
            new Vector2(TriangleSurfaceScript.triangleSurfaceInstance.verticesList[indices[2]].x, TriangleSurfaceScript.triangleSurfaceInstance.verticesList[indices[2]].z),
            new Vector2(ballPos.x, ballPos.z)
        );

        /*for (int i = 0; i < TriangleSurfaceScript.triangleSurfaceInstance.madeTriangles.Count; i++)
        {
            if (this == TriangleSurfaceScript.triangleSurfaceInstance.madeTriangles[i])
            {
                Debug.Log($"Ball over current triangle: {i}");
            }
        }*/

        return true;
	}
}

public class TriangleSurfaceScript : MonoBehaviour
{   
    public static TriangleSurfaceScript triangleSurfaceInstance { get; private set; }

    [SerializeField] bool bGenerateTriangleSurface = false;
    [SerializeField] Material terrainMaterial;

    string verticesFile = @"Assets/Resources/vertices.txt";
    string indicesFile = @"Assets/Resources/indices.txt";

    // List containing the direct values of the "smoothTerrainFile -file".
    [HideInInspector] public List<Vector3> verticesList = new List<Vector3>();
    [HideInInspector] public List<int> indicesList = new List<int>();
    [HideInInspector] public List<int> neighboursList = new List<int>();
    [HideInInspector] public List<Triangle> madeTriangles = new List<Triangle>();

    void Awake() {
        if (bGenerateTriangleSurface)
        {
            triangleSurfaceInstance = this;
            fetchVertices(verticesFile);
            fetchIndicesAndNeighbours(indicesFile);
            makeTriangles(indicesList, neighboursList);

            generateSurface();
        }
    }

    void fetchVertices(string input)
    {
        // Pass the file path and file name to the StreamReader constructor.
        StreamReader readFile = new StreamReader(input);

        // Finds out how many lines there are in the inputted .txt-document.
        var lineCount = int.Parse(readFile.ReadLine());
        Debug.Log("Amount of lines to be read into the vertices list: " + lineCount);
        

        for (int i = 0; i < lineCount; i++)
        {
            // Reads the first line of text.
            string line = readFile.ReadLine();

            // Makes a new list of strings with the name "pointValues".
            // Assigns the mergedFile .txt-document as the value of the List, however it also splits each line in the .txt-document
            // in such a way that the document writes a new line with everything that comes after a space in the .txt-document all while
            // deleting empty spaces in the .txt-document.
            // Lastly it converts the document to a List as it is technically just a really long string with a format.
            List<String> pointValues = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                
            // Adds the recently split three string values to the mergedList as a Vector3 of floats by Parsing them.
            // NOTE: For some reason not adding "CultureInfo.InvariantCulture.NumberFormat" 
            // makes the renderer unable to recognise the file as valid. I assume that this is because it sees the ".'s" in the float values
            // and gets confused. I therefore believe that "CultureInfo.InvariantCulture.NumberFormat" makes the code read the ".'s" as ",".
            // Could this be a matter of Unity not liking the fact that the language on my computer is Norwegain rather than its standard?^^
            verticesList.Add(
                new Vector3(
                    float.Parse(pointValues[0], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat)
                )
            );
        }
    }

    void fetchIndicesAndNeighbours(string input)
    {
        // Pass the file path and file name to the StreamReader constructor.
        StreamReader readFile = new StreamReader(input);

        // Finds out how many lines there are in the inputted .txt-document.
        var lineCount = int.Parse(readFile.ReadLine());
        Debug.Log("Amount of lines to be read into the vertices list: " + lineCount);
        

        for (int i = 0; i < lineCount; i++)
        {
            // Reads the first line of text.
            string line = readFile.ReadLine();

            // Makes a new list of strings with the name "pointValues".
            // Assigns the mergedFile .txt-document as the value of the List, however it also splits each line in the .txt-document
            // in such a way that the document writes a new line with everything that comes after a space in the .txt-document all while
            // deleting empty spaces in the .txt-document.
            // Lastly it converts the document to a List as it is technically just a really long string with a format.
            List<String> pointValues = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                
            // Adds the recently split three string values to the mergedList as a Vector3 of floats by Parsing them.
            // NOTE: For some reason not adding "CultureInfo.InvariantCulture.NumberFormat" 
            // makes the renderer unable to recognise the file as valid. I assume that this is because it sees the ".'s" in the float values
            // and gets confused. I therefore believe that "CultureInfo.InvariantCulture.NumberFormat" makes the code read the ".'s" as ",".
            // Could this be a matter of Unity not liking the fact that the language on my computer is Norwegain rather than its standard?^^
            if (pointValues.Count == 6)
            {
                for (int j = 0; j < 3; j++)
                {
                    indicesList.Add(int.Parse(pointValues[j], CultureInfo.InvariantCulture.NumberFormat));
                }
                for (int j = 3; j < 6; j++)
                {
                    neighboursList.Add(int.Parse(pointValues[j], CultureInfo.InvariantCulture.NumberFormat));
                }
            }
        }
    }

    // Makes triangles based on a list of indices and neighbours.
    void makeTriangles(List<int> indicesInput, List<int> neighboursInput)
    {
        // Gets the amount of indices and divides by 3 for every future triangle.
        var indicesListSize = indicesInput.Count / 3;

        for (int i = 0; i < indicesListSize; i ++)
        {
            madeTriangles.Add(new Triangle(indicesInput[i], 
                                           indicesInput[i + 1], 
                                           indicesInput[i + 2], 
                                           neighboursInput[i], 
                                           neighboursInput[i + 1], 
                                           neighboursInput[i + 2], 
                                           i));
        }
    }

    void generateSurface()
    {
        // Initializes mesh variables
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh = generateMesh();
        meshRenderer.sharedMaterial = terrainMaterial;
    }

    Mesh generateMesh()
    {

        Mesh triangleSurfaceMesh = new Mesh();
        triangleSurfaceMesh.indexFormat = IndexFormat.UInt32;

        // Assigns triangles and vertices to our mesh.
        triangleSurfaceMesh.vertices = verticesList.ToArray();
        triangleSurfaceMesh.triangles = indicesList.ToArray();

        // Recalculates normals and tangents for the mesh.
        triangleSurfaceMesh.RecalculateNormals();
        triangleSurfaceMesh.RecalculateTangents();

        return triangleSurfaceMesh;
    }
}
