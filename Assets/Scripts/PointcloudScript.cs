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

public class PointcloudScript : MonoBehaviour
{
    public static PointcloudScript PointCInstance { get; private set; } 

    [SerializeField] bool bDataGenerated = true;

    string mergedFile = @"Assets/Resources/merged.txt";
    string terrainFile = @"Assets/Resources/terrain.txt";
    string smoothTerrainFile = @"Assets/Resources/smoothTerrain.txt";
    string indicesFile = @"Assets/Resources/indices.txt";

    // Used for deciding how many lines should be skipped when converting the initial merged file and making a smooth pointcloud.
    // NOTE: CHANGING THIS VALUE FROM "0" WILL DISTORT YOUR TERRAIN AS YOU WILL BE ACTIVELY SKIPPING SOME POINTS IN YOUR DOCUMENT:
    // THIS IS BEST SUITED FOR ABSURDLY LARGE TERRAINS.
    float lineSkips = 0;

    // List containing the direct values of the "mergedFile-file".
    List<Vector3> mergedList = new List<Vector3>();

    // List that contains the final converted values for the terrainFile-file.
    List<Vector3> convertedList = new List<Vector3>();
    // List that contains the final converted values for the smoothTerrainFile-file.
    List<List<Vector3>> smoothTerrainList = new List<List<Vector3>>();

    // LIst that contains the indices values of the smoothTerrainFile-file.
    List<int> indicesList = new List<int>();

    Vector3 offset = new Vector3();

    int xStep = 0; 
    int zStep = 0;

    // Used to make the pointcloud look nice. Also helps make the triangleSurface smooth later.
    float xMin = 0; 
    float xMax = 0;

    float yMin = 0; 
    float yMax = 0;

    float zMin = 0; 
    float zMax = 0;

    public float deltaX = 1; 
    public float deltaZ = 1;
    

    // Runs before Start().
    void Awake() {
        PointCInstance = this;

        if (bDataGenerated)
        {   
            // Converts and adjusts the initial merged file into one that is centered around the origin of the projects' scene.
            ConvertMerged(mergedFile);
            Debug.Log("Merged has been converted and modified successfully.");

            // Writes convertedList over to a file.
            writePointcloud(convertedList, terrainFile);
            Debug.Log("Successfully wrote terrain file.");

            // Converts an inputted terrain into a smooth version of itself.
            ConvertTerrainToSmooth(terrainFile);
            Debug.Log("Terrain has been smoothed successfully.");

            // Writes smoothTerrainList over to a file.
            writeSmoothPointcloud(smoothTerrainList, smoothTerrainFile);
            Debug.Log("Successfully wrote a smooth terrain file.");

            fetchIndicesFromSmoothPointcloud();
            Debug.Log("Successfully fetched indices.");

            writeIndicesToFile(indicesList, indicesFile);
            Debug.Log("Successfully wrote an indices file.");
        }
    }


    // CALCULATES MIN AND MAX VALUES OF A LIST OF VECTOR3'S.
    void calculateMinAndMax(List<Vector3> input)
        {
            // Assigns the specified x/y/z value of the previous vector in the list to the x/y/z- min's and max's.
            xMin = xMax = input[^1].x;
            yMin = yMax = input[^1].y;
            zMin = zMax = input[^1].z;

            // Finds the smallest and largest values 
            for (int i = 0; i < input.Count; i++)
            {
                // Assigns min and max values of x, y and z after checking them.
                // x
                if (input[i].x < xMin)
                {
                    xMin = input[i].x;
                }
                else if (input[i].x > xMax)
                {
                    xMax = input[i].x;
                }

                // y
                if (input[i].y < yMin)
                {
                    yMin = input[i].y;
                }
                else if (input[i].y > yMax)
                {   
                    yMax = input[i].y;
                }

                // z
                if (input[i].z < zMin)
                {
                    zMin = input[i].z;
                }
                    else if (input[i].z > zMax)
                {
                    zMax = input[i].z;
                }
            }

            Debug.Log("xMin: " + xMin);
            Debug.Log("xMax: " + xMax);

            Debug.Log("yMin: " + yMin);
            Debug.Log("yMax: " + yMax);

            Debug.Log("zMin: " + zMin);
            Debug.Log("zMax: " + zMax);

            
            // Calculates the offset and puts it into a Vector3.
            offset = new Vector3((xMin + xMax) / 2, (yMin + yMax) / 2, (zMin + zMax) / 2);
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

        calculateMinAndMax(mergedList);
        
        for (int i = 0; i < mergedList.Count; i++)
        {
            // Adds the current mergedList's Vector3 to the current slot in the convertedList -List after adding the offset to it.
            convertedList.Add(
                mergedList[i] - offset
            );
        }

        // Clears list to save memory.
        mergedList.Clear();
        // Resets min- and max values.
        calculateMinAndMax(convertedList);
    }


    // CONVERTS AN INPUTTED POINTCLOUD INTO A SMOOTH VERSION OF ITSELF.
    void ConvertTerrainToSmooth(string input)
    {
        // Pass the file path and file name to the StreamReader constructor.
        StreamReader readFile = new StreamReader(input);

        // Finds out how many lines there are in the inputted .txt-document.
        var lineCount = int.Parse(readFile.ReadLine());
        Debug.Log("Amount of lines in the original terrain-file: " + lineCount);


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
            for (int i = 0; i < xStep; i++) {
                for (int j = 0; j < zStep; j++) {
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

        Debug.Log("xStep: " + xStep + "  |  " + "zStep: " + zStep + "  |  " + "buckets: " + buckets[xStep, zStep].Count);
        float averageHeight = 0;
        var numberOfPoints = 0;
        var numberOfComparisons = 0;

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
                else
                {
                    numberOfPoints = 1;
                }

                // Divide said 'averageHeight' on the amount of items (Height's) to get the actual average height.
                averageHeight /= numberOfPoints;
                // Find the middle value of the x-coordinate in the "square" in 'buckets'.
                middleX = xMin + (deltaX / 2) + (deltaX * i);
                // Find the middle value of the z-coordinate in the "square" in 'buckets'.
                middleZ = zMin + (deltaZ / 2) + (deltaZ * j);

                // Creates a final point for each square that is then added to a smoothTerrainList.
                smoothTerrainList[i].Add(
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

        // FIXES THE HEIGHT OF POINTS WITH EMPTY "SQUARES".
        // Loop through the "rows" of the "plane" called 'buckets'.
        for (int i = 0; i < xStep; i++)
        {   
            // Loop through the "squares" of the current "row" in 'buckets'.
            for (int j = 0; j < zStep; j++)
            {   
                // If the mask is filled, then do this:
                if (!bMaskfilled[i, j])
                {
                    float tempY = 0;

                    // THIS TAKES A LOT OF INSPIRATION FROM ANDERS' CODE
                    //// ---------------------------------------------------------------------------------------------------------------------
                    // Compares the x-values of 20 neighbours in the x-direction (xStep) to get an accurate x-value.
                    for (int xN = i - 20; xN <= i + 20; xN++)
                    {
                        if (xN < 0 || xN >= xStep) 
                        {
                            continue;
                        }
                        
                        // Compares the z-values of 20 neighbours in the z-direction (zStep) to get an accurate z-value.
                        for (int zN = j - 20; zN <= j + 20; zN++)
                        {
                            if (zN < 0 || zN >= zStep || !bMaskfilled[xN, zN]) 
                            {
                                continue;
                            }
                            
                            // Adds the y-values of the x- and z- neighbours.
                            tempY += smoothTerrainList[i][j].y;
                            numberOfComparisons++;
                        }
                    }
                    //// ---------------------------------------------------------------------------------------------------------------------

                    // Divides the temporary, new y-value on the amount of points to get an average y-value.
                    if (numberOfComparisons > 0)
                    {
                        tempY /= numberOfComparisons;
                    }
                    // Creates a temporary Vector.
                    Vector3 tempVec = smoothTerrainList[i][j];
                    // Adds said temporary vector to its proper position in the smoothTerrainList -List.
                    smoothTerrainList[i][j] = new Vector3(tempVec.x, tempY, tempVec.z);
                    // Sets this mask as filled.
                    bMaskfilled[i, j] = true;
                    numberOfComparisons = 0;
                }
            }
        }
    }


    void fetchIndicesFromSmoothPointcloud()
    {
        // Fills up the indicesList -List.
        for (int i = 0; i < xStep; i++)
        {
            for (int j = 0; j < zStep; j++)
            {
                int k = j + (zStep * i);
                // First triangle
                indicesList.Add(k);
                indicesList.Add(k + 1 + zStep);
                indicesList.Add(k + 1);
                
                // Second triangle
                indicesList.Add(k);
                indicesList.Add(k + zStep);
                indicesList.Add(k + 1 + zStep);
            }
        }
    }


    // WRITES THE POINTCLOUD OVER TO A FILE.
    void writePointcloud(List<Vector3> input, string output)
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

        input.Clear();
    }


    // WRITES THE SMOOTH POINTCLOUD TO A FILE.
    void writeSmoothPointcloud(List<List<Vector3>> input, string output)
    {   
        var amountOfLines = 0;

        // Counts how many lines there are in the inputted List of Lists.
        for (int i = 0; i < input.Count; i++)
        {
            for (int j = 0; j < input[i].Count; j++)
            {
                amountOfLines++;
            }
        }

        // Puts the amount of lines in the inputted List of LIsts at the top of the output-file.
        File.WriteAllText(output, amountOfLines.ToString() + "\n");

        // Loops through the inputted List of Lists and formats each vector into a string, which is then printed out to the output-file.
        for (int i = 0; i < input.Count; i++)
        {
            for (int j = 0; j < input[i].Count; j++)
            {
                // Replaces ","'s with a ".".
                var outputLine = input[i][j].x + " " + input[i][j].y + " " + input[i][j].z;
                outputLine = outputLine.Replace(",", ".");

                // Writes the current line over to a file.
                using (StreamWriter writeFile = File.AppendText(output))
                {
                    writeFile.WriteLine(outputLine);
                }
            }
        }

        input.Clear();
    }


    // WRITES THE INDICES TO A FILE.¨
    void writeIndicesToFile(List<int> input, string output)
    {
        // Puts the amount of lines in the inputted List at the top of the output-file.
        File.WriteAllText(output, (input.Count / 3).ToString() + "\n");

        // Loops throught the inputted List and formats each vector into a string, which is then printed out to the output-file.
        for (int i = 0; i < input.Count / 3; i += 3)
        {
            // Replaces ","'s with a ".".
            string outputLine = input[i] + " " + input[i + 1] + " " + input[i + 2];

            // Writes the current line over to a file.
            using (StreamWriter writeFile = File.AppendText(output))
            {
                writeFile.WriteLine(outputLine);
            }
        }

        input.Clear();
    }
}
