using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System;
using UnityEngine;
using Unity.Jobs;
using Unity.VisualScripting;
using System.Drawing;
//using System.Numerics;


// Most of this code was made by myself from scratch using the resource below, however I recieved significant help from Anders in the class;
// who inspired me as to how I might fix parts of my terrain, which was broken at the time.
// (https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/read-write-text-file),

public class readWriteScript : MonoBehaviour
{
    [SerializeField] bool bDataGenerated = true;

    string mergedFile = @"Assets/Resources/merged.txt";
    string terrainFile = @"Assets/Resources/terrain.txt";
    string verticesFile = @"Assets/Resources/vertices.txt";
    string indicesFile = @"Assets/Resources/indices.txt";

    // Used for deciding how many lines should be skipped when converting the initial merged file and making a smooth pointcloud.
    // NOTE: CHANGING THIS VALUE FROM "0" WILL DISTORT YOUR TERRAIN AS YOU WILL BE ACTIVELY SKIPPING SOME POINTS IN YOUR DOCUMENT:
    // THIS IS BEST SUITED FOR ABSURDLY LARGE TERRAINS.
    float lineSkips = 0;

    // Used to make the pointcloud look nice. Also helps make the triangleSurface smooth later.
    float xMin = 0; 
    float xMax = 0;

    float yMin = 0; 
    float yMax = 0;

    float zMin = 0; 
    float zMax = 0;

    // List containing the direct values of the "mergedFile-file".
    List<Vector3> mergedList = new List<Vector3>();
    // List that contains the final converted values for the terrainFile-file.
    List<Vector3> convertedList = new List<Vector3>();
    // List that contains the final converted values for the verticesFile-file.
    List<Vector3> verticesList = new List<Vector3>();
    // List that contains the indices values of the verticesList -List.
    List<int> indicesList = new List<int>();

    int xStep = 0; 
    int zStep = 0;

    public float deltaX = 1; 
    public float deltaZ = 1;
    

    // Runs before Start().
    void Awake() {
        if (bDataGenerated)
        {   
            // Converts and adjusts the initial merged file into one that is centered around the origin of the projects' scene.
            ConvertMerged(mergedFile);
            Debug.Log("Merged has been converted and modified successfully.");
            // Clears list to save memory.
            mergedList.Clear();

            // Writes convertedList over to a file.
            writeFile(convertedList, terrainFile);
            Debug.Log("Successfully wrote terrain file.");
            // Clears list to save memory.
            convertedList.Clear();

            // Converts an inputted terrain into a smooth version of itself.
            ConvertTerrainToSmooth(terrainFile);
            Debug.Log("Terrain has been smoothed successfully.");

            // Writes verticesList over to a file.
            writeFile(verticesList, verticesFile);
            Debug.Log("Successfully wrote a smooth terrain file.");

            // Fetches and writes indices.
            fetchAndWriteIndices();
            Debug.Log("Successfully fetched and wrote indices.");
            indicesList.Clear();
        }
    }
    
    // CONVERTS THE INITIAL MERGED FILE.
    void ConvertMerged(string input)
    {
        // Pass the file path and file name to the StreamReader constructor.
        StreamReader readFile = new StreamReader(input);

        // Finds out how many lines there are in the inputted .txt-document.
        var lineCount = File.ReadLines(input).Count();
        Debug.Log("Amount of lines in the original mergedFile-file: " + lineCount);

        int a = 0;

        for (int i = 0; i < lineCount; i++)
        {
            // Reads the first line of text.
            string line = readFile.ReadLine();

            // An if-check that helps us determine how many lines we want to skip in the file.
            if (a >= lineSkips)
            {
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
                
                // Could this be a matter of Unity not liking the fact that the language on my computer is Norwegian rather than its standard?^^
                mergedList.Add(
                    new Vector3(
                        float.Parse(pointValues[0], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat)
                    )
                );

                a = 0;
            }
            else
            {
                a++;
            }
        }
        
        // Calculates min and max values for the mergedList -List.
        calcMinAndMaxValues(mergedList);

        Debug.Log("xMin: " + xMin);
        Debug.Log("xMax: " + xMax);

        Debug.Log("yMin: " + yMin);
        Debug.Log("yMax: " + yMax);

        Debug.Log("zMin: " + zMin);
        Debug.Log("zMax: " + zMax);

        // Translates the contents of mergedList to convertedList while taking offset from the scenes' origin into account. 
        // (convertedList starts in the scenes' origin).

        // Calculates the offset and puts it into a Vector3.
        var offset = new Vector3((xMin + xMax) / 2, (yMin + yMax) / 2, (zMin + zMax) / 2);
        for (int i = 0; i < mergedList.Count; i++)
        {
            // Adds the current mergedList's Vector3 to the current slot in the convertedList -List after adding the offset to it.
            convertedList.Add(
                mergedList[i] - offset
            );
        }
    }

    // CONVERTS AN INPUTTED POINTCLOUD INTO A SMOOTH VERSION OF ITSELF.
    void ConvertTerrainToSmooth(string input)
    {
        // Pass the file path and file name to the StreamReader constructor.
        StreamReader readFile = new StreamReader(input);

        // Finds out how many lines there are in the inputted .txt-document.
        var lineCount = int.Parse(readFile.ReadLine());
        Debug.Log("Amount of lines in the original terrain-file: " + lineCount);

        // Calculates min and max values for the mergedList -List.
        calcMinAndMaxValues(convertedList);
        convertedList.Clear();

        // Calculates xStep and zStep
        xStep = (int)Mathf.Ceil((xMax - xMin) / deltaX);
        zStep = (int)Mathf.Ceil((zMax - zMin) / deltaZ);
        
        // Used for calculating the final smooth points.
        float middleX = 0;
        float middleZ = 0;


        // List used for making new points in the pointcloud.
        // This is best imagined as a plane, that acts as a List of rows, that contain Lists of given areas (squares for example),
        // who themselves act as a List of Vector3's.
        List<Vector3>[,] buckets = new List<Vector3>[xStep, zStep];
        // Adds Vector3's to the "squares" inside the "rows" of the "plane" called 'buckets'.
            for (var j = 0; j < xStep; j++) {
                for (var k = 0; k < zStep; k++) {
                    // Add a List of Vector3's to the current "square" (k) in the current "row" (j).
                    buckets[j, k] = new List<Vector3>();
                }
            }


        for (int i = 0; i < lineCount; i++)
        {
            // Reads the current line in the text-document.
            var line = readFile.ReadLine();

            // Makes a new list of strings with the name "pointValues".
            // Assigns the mergedFile .txt-document as the value of the List, however it also splits each line in the .txt-document
            // in such a way that the document writes a new line with everything that comes after a space in the .txt-document all while
            // deleting empty spaces in the .txt-document.
            // Lastly it converts the document to a List as it is technically just a really long string with a format.
            List<String> pointValues = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                
            // Adds the recently split three string values to a Vector3 of floats by Parsing them.
            // NOTE: For some reason not adding "CultureInfo.InvariantCulture.NumberFormat" 
            // makes the renderer unable to recognise the file as valid. I assume that this is because it sees the ".'s" in the float values
            // and gets confused. I therefore believe that "CultureInfo.InvariantCulture.NumberFormat" makes the code read the ".'s" as ",".
                
            // Could this be a matter of Unity not liking the fact that the language on my computer is Norwegian rather than its standard?^^
            var convertedPoint = new Vector3(
                    float.Parse(pointValues[0], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat)
                    );


            // Defines "o" and "p" in the current iteration.
            var o = (int)((convertedPoint.x - xMin) / deltaX);
            var p = (int)((convertedPoint.z - zMin) / deltaZ);

            // If-check that makes sure "o" and "p" are not out of bounds.
            if (o < 0) 
            { 
                o = 0; 
            }
            else if (o >= xStep)
            {
                o = xStep - 1; 
            }

            if (p < 0) 
            { 
                p = 0; 
            }
            else if (p >= zStep) 
            { 
                p = zStep - 1; 
            }

            // Adds the converted point to the buckets-List.
            buckets[o,p].Add(convertedPoint);
        }
        Debug.Log("Made it past the a-b -loop!");


        // THIS IS INSPIRED BY ANDERS' CODE.
        //// ---------------------------------------------------------------------------------------------------------------------------------
        // Makes a 2d array of bools which will be used later to determine which buckets are empty and which are not.
        bool[,] bMaskfilled = new bool[xStep, zStep];
        for (int i = 0; i < xStep; i++)
        {
            for (int j = 0; j < zStep; j++)
            {
                // If the bucket has points in it, then:
                if (buckets[i, j].Count > 0)
                {
                    bMaskfilled[i, j] = true;
                }
                else
                {
                    bMaskfilled[i, j] = false;
                }
            }
        }
        //// ---------------------------------------------------------------------------------------------------------------------------------

        
        float averageHeight = 0;
        var numberOfPoints = 0;

        // GENERATES THE POINTS IN THE SMOOTH "PLANE". ASSIGNS "0" AS THE Y-VALUE FOR EMPTY "SQUARES".
        // Loop through the "rows" of the "plane" called 'buckets'.
        for (int i = 0; i < xStep; i++)
        {   
            // Loop through the "squares" of the current "row" in 'buckets'.
            for (int j = 0; j < zStep; j++)
            {   
                // If the mask is filled, then do this:
                if (bMaskfilled[i, j])
                {
                    // THIS IS INSPIRED BY ANDERS' CODE.
                    //// ---------------------------------------------------------------------------------------------------------------------
                    // "Do this to each point that exists in the bucket".
                    // In other words: Loop through each "square" in each "row" and do this to its points.
                    foreach (var currentPoint in buckets[i,j])
                    {
                        // Add all height-values to a singular value called 'averageHeight'.
                        averageHeight += currentPoint.y;
                    }

                    // This is in essence an if-check that makes sure that we do not continue making the points if there are no points in the current bucket,
                    // as that would give us an incorrect value (You cannot divide by "0" as that gives an error, and dividing by "1" returns the same number).
                    if (buckets[i, j].Count > 0)
                    {
                        numberOfPoints = buckets[i,j].Count;
                    }
                    else
                    {
                        numberOfPoints = 1;
                    }
                    //// ---------------------------------------------------------------------------------------------------------------------
                }
                else
                {
                    numberOfPoints = 1;
                    averageHeight = 0;
                }

                // Divide said 'averageHeight' on the amount of items (Height's) to get the actual average height.
                averageHeight /= numberOfPoints;
                // Find the middle value of the x-coordinate in the "square" in 'buckets'.
                middleX = xMin + (deltaX / 2) + (deltaX * i);
                // Find the middle value of the z-coordinate in the "square" in 'buckets'.
                middleZ = zMin + (deltaZ / 2) + (deltaZ * j);

                // Creates a final point for each square that is then added to a verticesList.
                verticesList.Add(
                    new Vector3(
                        middleX,
                        averageHeight,
                        middleZ
                    )
                );

                // Resets averageHeight.
                averageHeight = 0;
            }
        }
    }


    // WRITES THE POINTCLOUD OVER TO A FILE.
    void writeFile(List<Vector3> input, string output)
    {
        // Puts the amount of lines in the inputted List at the top of the output-file.
        File.WriteAllText(output, input.Count.ToString() + "\n");

        // Loops throught the inputted List and formats each vector into a string, which is then printed out to the output-file.
        for (int i = 0; i < input.Count; i++)
        {
            // Replaces ","'s with a ".".
            var outputLine = input[i].x + " " + input[i].y + " " + input[i].z;
            outputLine = outputLine.Replace(",", ".");

            // Writes the current line over to a file.
            using (StreamWriter writeFile = File.AppendText(output))
            {
                writeFile.WriteLine(outputLine);
            }
        }
    }

    // FETCHES INDICES.
    void fetchAndWriteIndices()
    {
        // Fills up the indices -List.
        for (int i = 0; i < xStep; i++)
        {
            for (int j = 0; j < zStep; j++)
            {
                // First triangle
                indicesList.Add(j + (i * zStep));
                indicesList.Add(j + ((i + 1) * zStep));
                indicesList.Add(j + 1 + ((i + 1) * zStep));
                
                // Second triangle
                indicesList.Add(j + (i * zStep));
                indicesList.Add(j + 1 + ((i + 1) * zStep));
                indicesList.Add(j + 1 + (i * zStep));
            }
        }

        // Multiplies amount of "squares" in the "plane" by two to get amount of triangles.
        var indicesListSize = (xStep * zStep) * 2;

        // Puts the amount of lines in the inputted List at the top of the output-file.
        File.WriteAllText(indicesFile, indicesListSize.ToString() + "\n");

        // Loops throught the inputted List and formats each vector into a string, which is then printed out to the output-file.
        for (int i = 0; i < indicesListSize; i++)
        {
            // Replaces ","'s with a ".".
            var outputLine = indicesList[i] + " " + indicesList[i + 1] + " " + indicesList[i + 2];

            // Writes the current line over to a file.
            using (StreamWriter writeFile = File.AppendText(indicesFile))
            {
                writeFile.WriteLine(outputLine);
            }
        }
    }

    // CALCULATES MIN- AND MAX VALUES.
    void calcMinAndMaxValues(List<Vector3> input)
    {
        // Finds the smallest and largest values 
        for (int i = 0; i < input.Count; i++)
        {
            // Resets min- and max values.
            if (i == 0) {
                xMin = xMax = input[i].x;
                yMin = yMax = input[i].y;
                zMin = zMax = input[i].z;
            }

            // Assigns min and max values of x, y and z after checking them.
            // x
            if (input[i].x < xMin)      {xMin = input[i].x;}
            else if (input[i].x > xMax) {xMax = input[i].x;}

            // y
            if (input[i].y < yMin)      {yMin = input[i].y;}
            else if (input[i].y > yMax) {yMax = input[i].y;}

            // z
            if (input[i].z < zMin)      {zMin = input[i].z;}
            else if (input[i].z > zMax) {zMax = input[i].z;}
        }
    }
}
