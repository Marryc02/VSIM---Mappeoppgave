using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System;
using UnityEngine;
using Unity.Jobs;
using Unity.VisualScripting;
using System.Drawing;


// Most of this code was made by myself from scratch using the resource below, however I recieved significant help from Anders in the class;
// who inspired me as to how I might fix parts of my terrain, which was broken at the time.
// (https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/read-write-text-file),

public class ReadWriteScript : MonoBehaviour
{
    [SerializeField] bool bGenerateData = true;

    string mergedFile = @"Assets/Resources/merged.txt";
    string terrainFile = @"Assets/Resources/terrain.txt";
    string verticesFile = @"Assets/Resources/vertices.txt";
    string indicesFile = @"Assets/Resources/indices.txt";


    // List containing the direct values of the "mergedFile-file".
    List<Vector3> mergedList = new List<Vector3>();
    // List that contains the final converted values for the terrainFile-file.
    List<Vector3> convertedList = new List<Vector3>();
    // List that contains the final converted values for the verticesFile-file.
    List<Vector3> verticesList = new List<Vector3>();
    // List that contains the indices values of the verticesList -List.
    List<int> indicesList = new List<int>();
    List<int> trianglesList = new List<int>();


    // Used for deciding how many lines should be skipped when converting the initial merged file and making a smooth pointcloud.
    // NOTE: CHANGING THIS VALUE FROM "0" WILL DISTORT YOUR TERRAIN AS YOU WILL BE ACTIVELY SKIPPING SOME POINTS IN YOUR DOCUMENT:
    // THIS IS BEST SUITED FOR ABSURDLY LARGE TERRAINS.
    float lineSkips = 0;

    // Used to make the pointcloud look nice. Also helps make the triangleSurface smooth later among other things.
    float xMin = 0; 
    float xMax = 0;

    float yMin = 0; 
    float yMax = 0;

    float zMin = 0; 
    float zMax = 0;

    int xStep = 0; 
    int zStep = 0;

    // Used when deciding space between each square, or in other words: steplength.
    float deltaX = 5; 
    float deltaZ = 5;


    // Runs before Start().
    void Awake() {
        if (bGenerateData)
        {   
            // Converts and adjusts the initial merged file into one that is centered around the origin of the projects' scene.
            ConvertMerged(mergedFile);
            Debug.Log("Merged has been converted and modified successfully.");
            // Clears list to save memory.
            mergedList.Clear();

            // Writes convertedList over to a file.
            writeFile(convertedList, terrainFile);
            Debug.Log("Successfully wrote terrain file.");

            // Converts an inputted terrain into a smooth version of itself.
            ConvertTerrainToSmooth(terrainFile);
            Debug.Log("Terrain has been smoothed successfully.");

            // Writes verticesList over to a file.
            writeFile(verticesList, verticesFile);
            Debug.Log("Successfully wrote a smooth terrain file.");
            verticesList.Clear();

            // Fetches and writes indices and neighbours of triangles.
            fetchAndWriteIndicesAndNeighbours();
            Debug.Log("Successfully fetched and wrote indices.");
            // Clears list to save memory.
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

        calcMinAndMaxValues(mergedList);
        // Calculates the offset and puts it into a Vector3.
        var offset = new Vector3((xMin + xMax) / 2, (yMin + yMax) / 2, (zMin + zMax) / 2);
        for (int i = 0; i < mergedList.Count; i++)
        {
            // Adds the current mergedList's Vector3 to the current slot in the convertedList -List after adding the offset to it.
            convertedList.Add(mergedList[i] - offset);
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

        calcMinAndMaxValues(convertedList);
        // Clears list to save memory.
        convertedList.Clear();

        // Calculates xStep and zStep using the previously calculated min/max values of convertedList
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
            for (var i = 0; i < xStep; i++) {
                for (var j = 0; j < zStep; j++) {
                    // Add a List of Vector3's to the current "square" (k) in the current "row" (j).
                    buckets[i, j] = new List<Vector3>();
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
            convertedList.Add(
                new Vector3(
                    float.Parse(pointValues[0], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat)
                )
            );

            // Defines "o" and "p" in the current iteration.
            var o = (int)((convertedList[i].x - xMin) / deltaX);
            var p = (int)((convertedList[i].z - zMin) / deltaZ);

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
            buckets[o,p].Add(convertedList[i]);
        }
        Debug.Log("Made it past the a-b -loop!");

        // Clears list to save memory.
        convertedList.Clear();


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
        //verticesList = new List<Vector3>();

        // GENERATES THE POINTS IN THE SMOOTH "PLANE". ASSIGNS "0" AS THE Y-VALUE FOR EMPTY "SQUARES".
        // Loop through the "rows" of the "plane" called 'buckets'.
        for (int i = 0; i < xStep; i++)
        {   
            // Loop through the "squares" of the current "row" in 'buckets'.
            for (int j = 0; j < zStep; j++)
            {   
                //verticesList[i, j] = new List<Vector3>();
                // If the mask is filled, then do this:
                if (bMaskfilled[i, j])
                {
                    // THIS IS INSPIRED BY ANDERS' CODE.
                    //// ---------------------------------------------------------------------------------------------------------------------
                    // "Do this to each point that exists in the bucket".
                    // In other words: Loop through each "square" in each "row" and do this to its points.
                    foreach (var currentPoint in buckets[i, j])
                    {
                        // Add all height-values to a singular value called 'averageHeight'.
                        averageHeight += currentPoint.y;
                    }

                    // This is in essence an if-check that makes sure that we do not continue making the points if there are no points in the current bucket,
                    // as that would give us an incorrect value (You cannot divide by "0" as that gives an error, and dividing by "1" returns the same number).
                    if (buckets[i, j].Count > 0)
                    {
                        numberOfPoints = buckets[i, j].Count;
                    }
                    else
                    {
                        numberOfPoints = 1;
                    }
                    //// ---------------------------------------------------------------------------------------------------------------------
                }
                // If the mask is otherwise not filled, then do this:
                else if (!bMaskfilled[i, j])
                {
                    // Despite my many hard attempts I did not manage to get this part of the code to work.
                    // I tried having the "squares" that did not contain points simply borrow the height from their neighbouring points
                    // assuming that they their "sqaure" was filled.
                    // I am aware that this is not needed to finish this task of the folder assignment,
                    // but it would have been nice if I had gotten this to work.

                    /*
                    int comparisonValue = 5;

                    if (i + comparisonValue < xStep)
                    {
                        for (int xN = i; xN == i + comparisonValue; xN++)
                        {
                            if (j + comparisonValue < zStep)
                            {
                                for (int zN = j; zN == j + comparisonValue; zN++)
                                {
                                    if (zN < 0 || zN >= zStep || !bMaskfilled[xN, zN]) 
                                    {
                                        continue;
                                    }

                                    // Adds the y-values of the x- and z- triangleSurfaceInstance.Triangle.neighbours.
                                    averageHeight += verticesList[i, j][zN].y;
                                    numberOfPoints++;
                                }
                            }
                            else if (j + comparisonValue > zStep)
                            {
                                for (int zN = j; zN == j - comparisonValue; zN--)
                                {
                                    if (zN < 0 || zN >= zStep || !bMaskfilled[xN, zN]) 
                                    {
                                        continue;
                                    }

                                    // Adds the y-values of the x- and z- triangleSurfaceInstance.Triangle.neighbours.
                                    averageHeight += verticesList[i, j][zN].y;
                                    numberOfPoints++;
                                }
                            }

                            
                        }
                    }
                    else if (i + comparisonValue > xStep)
                    {
                        for (int xN = i; xN == i - comparisonValue; xN--)
                        {
                            if (j + comparisonValue < zStep)
                            {
                                for (int zN = j; zN == j + comparisonValue; zN++)
                                {
                                    if (zN < 0 || zN >= zStep || !bMaskfilled[xN, zN]) 
                                    {
                                        continue;
                                    }

                                    // Adds the y-values of the x- and z- triangleSurfaceInstance.Triangle.neighbours.
                                    averageHeight += verticesList[i, j][zN].y;
                                    numberOfPoints++;
                                }
                            }
                            else if (j + comparisonValue > zStep)
                            {
                                for (int zN = j; zN == j - comparisonValue; zN--)
                                {
                                    if (zN < 0 || zN >= zStep || !bMaskfilled[xN, zN]) 
                                    {
                                        continue;
                                    }

                                    // Adds the y-values of the x- and z- triangleSurfaceInstance.Triangle.neighbours.
                                    averageHeight += verticesList[i, j][zN].y;
                                    numberOfPoints++;
                                }
                            }                            
                        }
                    }
                    else
                    {
                        averageHeight = 0;
                        numberOfPoints++;
                    }
                    */

                    averageHeight = 0;
                    numberOfPoints = 1;
                    bMaskfilled[i, j] = true;
                }

                // Divides the temporary, new y-value on the amount of points to get an average y-value.
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

                // Resets some values.
                numberOfPoints = 0;
                averageHeight = 0;
            }
        }
    }

    // FETCHES INDICES.
    // THE CODE FOR FINDING TRIANGLE NEIGHBOURS IS HEAVILY INSPIRED BY ANDERS FROM THE CLASS.
    void fetchAndWriteIndicesAndNeighbours()
    {   
        // useful constants:
        int trianglesInARow = 2 * (zStep - 1);
        int totalTris = trianglesInARow * (xStep - 1);

        // Fills up the indices -List.
        for (int i = 0; i < xStep; i++)
        {
            // useful constant:
            int numTrianglesUptoThisRow = 2 * i * (zStep - 1);

            for (int j = 0; j < zStep; j++)
            {
                // useful constants
                int evenTriangle = 2 * (i * (zStep - 1) + j);
                int oddTriangle = evenTriangle + 1;


                // First triangle
                var I0 = j + (i * zStep);
                var I1 = j + (i + 1) * zStep;
                var I2 = j + 1 + (i + 1) * zStep;

                indicesList.Add(I0);
                indicesList.Add(I1);
                indicesList.Add(I2);

                // // calculate neighbour-triangles and set to -1 if out of bounds:
                int T0 = oddTriangle;
                if (T0 < numTrianglesUptoThisRow + trianglesInARow) {trianglesList.Add(T0);}
                else                                                {trianglesList.Add(-1);}

                int T1 = evenTriangle - 1;
                if (T1 > numTrianglesUptoThisRow)                   {trianglesList.Add(T1);}
                else                                                {trianglesList.Add(-1);}

                int T2 = evenTriangle - trianglesInARow + 1;
                if (T2 > 0)                                         {trianglesList.Add(T2);}
                else                                                {trianglesList.Add(-1);}
                


                // Second triangle
                var I3 = j + (i * zStep);
                var I4 = j + 1 + (i + 1) * zStep;
                var I5 = j + 1 + (i * zStep);

                indicesList.Add(I3);
                indicesList.Add(I4);
                indicesList.Add(I5);

                // calculate neighbour-triangles and set to -1 if out of bounds:
                int T3 = evenTriangle + trianglesInARow;
                if (T3 < totalTris)                                 {trianglesList.Add(T3);}
                else                                                {trianglesList.Add(-1);}

                int T4 = evenTriangle;
                if (T4 >= numTrianglesUptoThisRow)                  {trianglesList.Add(T4);}
                else                                                {trianglesList.Add(-1);}

                int T5 = oddTriangle + 1;
                if (T5 < numTrianglesUptoThisRow + trianglesInARow) {trianglesList.Add(T5);}
                else                                                {trianglesList.Add(-1);}
            }
        }

        // Gets the amount of indices and divides by 3 for every future triangle.
        var indicesListSize = indicesList.Count / 3;

        // Puts the amount of lines in the inputted List at the top of the output-file.
        File.WriteAllText(indicesFile, indicesListSize.ToString() + "\n");

        // Loops throught the inputted List and formats each vector into a string, which is then printed out to the output-file.
        for (int i = 0; i < indicesListSize * 2; i++)
        {
            // Assigns values to a variable that is later printed out to the text.
            var outputLine = indicesList  [i] + " " + indicesList  [i + 1] + " " + indicesList  [i + 2]   + " " +
                             trianglesList[i] + " " + trianglesList[i + 1] + " " + trianglesList[i + 2];

            // Writes the current line over to a file.
            using (StreamWriter writeFile = File.AppendText(indicesFile))
            {
                writeFile.WriteLine(outputLine);
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

    /*// WRITES THE SMOOTH POINTCLOUD TO A FILE.
    void writeVertices(List<Vector3>[,] input, string output)
    {   
        // Puts the amount of lines in the inputted List of LIsts at the top of the output-file.
        File.WriteAllText(output, input.Length.ToString() + "\n");

        // Loops through the inputted List of Lists and formats each vector into a string, which is then printed out to the output-file.
        for (int i = 0; i < xStep; i++)
        {
            for (int j = 0; j < zStep; j++)
            {
                // Replaces ","'s with a ".".
                // Writing "[0]" here is fine because every "square" is only supposed to have one Vector3 in it anyway.
                var outputLine = input[i, j][0].x + " " + input[i, j][0].y + " " + input[i, j][0].z;
                outputLine = outputLine.Replace(",", ".");

                // Writes the current line over to a file.
                using (StreamWriter writeFile = File.AppendText(output))
                {
                    writeFile.WriteLine(outputLine);
                }
            }
        }
    }*/
    
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