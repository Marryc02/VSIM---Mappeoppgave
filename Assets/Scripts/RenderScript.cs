using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System;
using UnityEngine;
using UnityEngine.AI;

public class RenderScript : MonoBehaviour
{
    int pointsCount;

    string terrainFile = @"Assets/Resources/terrain.txt";

    GraphicsBuffer meshTriangles;
    GraphicsBuffer vertexPositions;
    GraphicsBuffer meshPositions;

    [SerializeField] Material material;
    [SerializeField] Mesh mesh;


    // Start is called before the first frame update
    void Start() {
        // Finds out how many points there are in the inputted .txt-document.
        pointsCount = File.ReadLines(terrainFile).Count();

        List<Vector3> points = new List<Vector3>();

        // Pass the file path and file name to the StreamReader constructor
        StreamReader read = new StreamReader(terrainFile);

        string line;

        for (int i = 0; i < pointsCount; i++)
        {   
            // Read the first line of text
            line = read.ReadLine();
            // Makes a new list of strings with the name "pointValues".
            // Assigns the inputText .txt-document as the value of the List, however it also splits each line in the .txt-document
            // in such a way that the document writes a new line with everything that comes after a space in the .txt-document all while
            // deleting empty spaces in the .txt-document.
            // Lastly it converts the document to a List as it is technically just a really long string with a format.
            List<String> pointValues = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            
            // Checks that there is actually three float values to put in the vector that is generated below.
            // It has the added benefit of outright ignoring the very first line in the .txt-file, 
            // that simply states how many lines are in the document in total.
            if (pointValues.Count() != 3)
            {
                continue;
            }

            Vector3 p = new Vector3(float.Parse(pointValues[0]),
                                    float.Parse(pointValues[1]),
                                    float.Parse(pointValues[2]));

            points.Add(p);
            //Debug.Log(p);
        }

        /*
        Code below obtained form Unity's documentation on RenderPrimitives
        https://docs.unity3d.com/ScriptReference/Graphics.RenderPrimitives.html
        */
        meshTriangles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length, sizeof(int));
        meshTriangles.SetData(mesh.triangles);
        
        meshPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, pointsCount, 3 * sizeof(float));
        meshPositions.SetData(points.ToArray());

        vertexPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertices.Length, 3 * sizeof(float));
        vertexPositions.SetData(mesh.vertices);
    }


    /*
    Function obtained form Unity's documentation on RenderPrimitives
    https://docs.unity3d.com/ScriptReference/Graphics.RenderPrimitives.html
    */
    void OnDestroy()
    {
        meshTriangles?.Dispose();
        meshTriangles = null;
        meshPositions?.Dispose();
        meshPositions = null;
        vertexPositions?.Dispose();
        vertexPositions = null;
    }

    void Update()
    {
        RenderParams rp = new RenderParams(material);
        rp.worldBounds = new Bounds(Vector3.zero, 10000*Vector3.one); // use tighter bounds
        rp.matProps = new MaterialPropertyBlock();
        rp.matProps.SetBuffer("_Triangles", meshTriangles);
        rp.matProps.SetBuffer("_Positions", meshPositions);
        rp.matProps.SetBuffer("_VertexPositions", vertexPositions);
        rp.matProps.SetInt("_StartIndex", (int)mesh.GetIndexStart(0));
        rp.matProps.SetInt("_BaseVertexIndex", (int)mesh.GetBaseVertex(0));
        rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));
        rp.matProps.SetFloat("_NumInstances", 10.0f);
        Graphics.RenderPrimitives(rp, MeshTopology.Triangles, (int)mesh.GetIndexCount(0), pointsCount);
    }
}
