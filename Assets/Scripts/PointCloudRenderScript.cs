using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System;
using UnityEngine;
using UnityEngine.AI;

public class PointCloudRenderScript : MonoBehaviour
{
    public static PointCloudRenderScript renderInstance { get; private set; }

    [SerializeField] bool regularTerrain = true;
    [SerializeField] bool smoothTerrain = false;

    string chosenFile;
    string terrainFile = @"Assets/Resources/terrain.txt";
    string verticesFile = @"Assets/Resources/vertices.txt";
    [HideInInspector] public bool fileHasBeenChosen = true;

    List<Vector3> points = new List<Vector3>();
    int pointsCount;


    GraphicsBuffer meshTriangles;
    GraphicsBuffer vertexPositions;
    GraphicsBuffer meshPositions;

    [SerializeField] Material material;
    [SerializeField] Mesh mesh;


    private void Awake() {
        renderInstance = this;
    }

    // Start is called before the first frame update
    void Start() {
        if (regularTerrain == true && smoothTerrain == false)
        {
            chosenFile = terrainFile;
        }
        else if (regularTerrain == false && smoothTerrain == true)
        {
            chosenFile = verticesFile;
        }
        else
        {
            fileHasBeenChosen = false;
        }

        if (fileHasBeenChosen == false)
        {
            Debug.Log("You did not select a pointcloud to render.");
        }
        else
        {
            // Finds out how many points there are in the inputted .txt-document.
            pointsCount = File.ReadLines(chosenFile).Count();

            // Pass the file path and file name to the StreamReader constructor
            StreamReader read = new StreamReader(chosenFile);

            for (int i = 0; i < pointsCount; i++)
            {   
                // Read  the first line of text
                string line = read.ReadLine();
                
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

                // Adds the recently split three string values to the mergedList as a Vector3 of floats by Parsing them.
                // NOTE: For some reason not adding "CultureInfo.InvariantCulture.NumberFormat" 
                // makes the renderer unable to recognise the file as valid. I assume that this is because it sees the ".'s" in the float values
                // and gets confused. I therefore believe that "CultureInfo.InvariantCulture.NumberFormat" makes the code read the ".'s" as ",".
                // Could this be a matter of Unity not liking the fact that the language on my computer is Norwegain rather than its standard?^^
                points.Add(
                    new Vector3(
                        float.Parse(pointValues[0], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat)
                    )
                );
            }

            /*
            Code below obtained form Unity's documentation on RenderPrimitives
            https://docs.unity3d.com/ScriptReference/Graphics.RenderPrimitives.html
            */
            meshTriangles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length, sizeof(int));
            meshTriangles.SetData(mesh.triangles);
        
            meshPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, pointsCount, 3 * sizeof(float));
            meshPositions.SetData(points.ToArray());
            points.Clear();

            vertexPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertices.Length, 3 * sizeof(float));
            vertexPositions.SetData(mesh.vertices);
        }
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
