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


public class TriangleSurfaceScript : MonoBehaviour
{   
    [SerializeField] bool bGenerateTriangleSurface = false;
    [SerializeField] Material terrainMaterial;


    string smoothTerrainFile = @"Assets/Resources/smoothTerrain.txt";


    // List containing the direct values of the "smoothTerrainFile -file".
    List<Vector3> vertices = new List<Vector3>();
    List<int> indices = new List<int>();

    //Vector2[] UVs = new Vector2[3];


    float xMin = 0; 
    float xMax = 0;

    float zMin = 0; 
    float zMax = 0;

    float deltaX = 1;
    float deltaZ = 1;

    int xStep = 0;
    int zStep = 0;


    void Awake() {
        if (bGenerateTriangleSurface)
        {
            fetchVertices(smoothTerrainFile);
            fetchIndices();

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
            vertices.Add(
                new Vector3(
                    float.Parse(pointValues[0], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat)
                )
            );
        }
    }

    void fetchIndices()
    {
        // Finds the smallest and largest values 
        for (int i = 0; i < vertices.Count; i++)
        {
            // Assigns min and max values of x, y and z after checking them.
            // x
            if (vertices[i].x < xMin)
            {
                xMin = vertices[i].x;
            }
            else if (vertices[i].x > xMax)
            {
                xMax = vertices[i].x;
            }

            // z
            if (vertices[i].z < zMin)
            {
                zMin = vertices[i].z;
            }
            else if (vertices[i].z > zMax)
            {
                zMax = vertices[i].z;
            }
        }


        // Calculates xStep and zStep
        xStep = (int)Mathf.Ceil((xMax - xMin) / deltaX);
        zStep = (int)Mathf.Ceil((zMax - zMin) / deltaZ);


        // Fills up the indices -List.
        for (int i = 0; i < xStep; i++)
        {
            for (int j = 0; j < zStep; j++)
            {
                // First triangle
                indices.Add(j);
                indices.Add(j + 1 + i);
                indices.Add(j + 1);
                
                // Second triangle
                indices.Add(j);
                indices.Add(j + 1+ i);
                indices.Add(i + 1);
            }
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
        triangleSurfaceMesh.vertices = vertices.ToArray();
        triangleSurfaceMesh.triangles = indices.ToArray();

        // Recalculates normals and tangents for the mesh.
        triangleSurfaceMesh.RecalculateNormals();
        triangleSurfaceMesh.RecalculateTangents();

        return triangleSurfaceMesh;
    }
}

/*public class Triangle()
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
}*/
