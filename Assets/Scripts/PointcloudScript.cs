using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System;
using UnityEngine;
using Unity.Jobs;
//using System.Numerics;
//using System.Numerics;


// I borrowed a lot of code from Joachim in the class. Ordinarily I would not borrow this much code from someone,
// but seeing on it as I did not know too much about file-reading or writing when doing this task, 
// and just so happened to be using the exact same source as him as a reference 
// (https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/read-write-text-file),
// he ended up contributing significantly to the parts of this code that relate to adjusting the values of the points so that they are closer
// to the scenes' origin as well as giving me some handy tools from other sources so that I could improve upon already existing code.
// Yes, I am aware that the code looks nearly identical, but that is because we colaborate on virtually every single obligatory task and our
// programming styles have blended into each other quite a bit.
// Approximately half of this code was originally his and modified to suit my already existing code that I got from the previously mentioned source. 
public class PointcloudScript : MonoBehaviour
{
    [SerializeField] bool bDataGenerated = true;

    string mergedFile = @"Assets/Resources/merged.txt";
    string terrainFile = @"Assets/Resources/terrain.txt";
    string smoothTerrainFile = @"Assets/Resources/smoothTerrain.txt";

    // Used for deciding how many lines should be skipped when converting the initial document and making a smooth pointcloud.
    // NOTE: CHANGING THIS VALUE FROM "0" WILL DISTORT YOUR TERRAIN AS YOU WILL BE ACTIVELY SKIPPING SOME POINTS IN YOUR DOCUMENT:
    // THIS IS BEST SUITED FOR ABSURDLY LARGE TERRAINS.
    float lineSkips = 0;

    // Used to make the pointcloud look nice. Also helps make the triangleSurface smoot later.
    float xMin = 0; 
    float xMax = 0;

    float yMin = 0; 
    float yMax = 0;

    float zMin = 0; 
    float zMax = 0;

    // List that contains the final converted values for the terrainFile-file.
    List<Vector3> convertedList = new List<Vector3>();
    // List that contains the final converted values for the smoothTerrainFile-file.
    List<Vector3> smoothTerrainList = new List<Vector3>();

    int xStep = 0; 
    int zStep = 0;

    [SerializeField] float deltaX = 1; 
    [SerializeField] float deltaZ = 1;
    

    // Runs before Start().
    void Awake() {
        if (bDataGenerated)
        {            
            // As it currently stands there are over a million lines in the text document, 
            // where virtually all of the lines have incredibly large values, and as such we will be scaling down the values to bring them closer
            // to the origin of the scene, thus making it easer for us to showcase them.
            ConvertMerged(mergedFile);
            Debug.Log("Merged has been converted and modified successfully.");

            // Writes an inputted List over to a file.
            writePointcloud(convertedList, terrainFile);
            Debug.Log("Successfully wrote terrain file.");

            // Converts an inputted terrain into a smooth version of itself.
            ConvertTerrainToSmooth(terrainFile);
            Debug.Log("Terrain has been smoothed successfully.");

            // Writes an inputted List over to a file.
            writePointcloud(smoothTerrainList, smoothTerrainFile);
            Debug.Log("Successfully wrote a smooth terrain file.");
        }
    }
    
    // Converts merged.txt
    void ConvertMerged(string input)
    {
        // Pass the file path and file name to the StreamReader constructor.
        StreamReader readFile = new StreamReader(input);

        // Finds out how many lines there are in the inputted .txt-document.
        var lineCount = File.ReadLines(input).Count();
        Debug.Log("Amount of lines in the original mergedFile-file: " + lineCount);


        // List containing the direct values of the "mergedFile-file".
        List<Vector3> mergedList = new List<Vector3>();

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
                // Could this be a matter of Unity not liking the fact that the language on my computer is Norwegain rather than its standard?^^
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
        // Assigns the specified x/y/z value of the previous vector in the list to the x/y/z- min's and max's.
        xMin = xMax = mergedList[^1].x;
        yMin = yMax = mergedList[^1].y;
        zMin = zMax = mergedList[^1].z;

        // Finds the smallest and largest values 
        for (int i = 0; i < mergedList.Count; i++)
        {
            // Assigns min and max values of x, y and z after checking them.
            // x
            if (mergedList[i].x < xMin)
            {
                xMin = mergedList[i].x;
            }
            else if (mergedList[i].x > xMax)
            {
                xMax = mergedList[i].x;
            }

            // y
            if (mergedList[i].y < yMin)
            {
                yMin = mergedList[i].y;
            }
            else if (mergedList[i].y > yMax)
            {
                yMax = mergedList[i].y;
            }

            // z
            if (mergedList[i].z < zMin)
            {
                zMin = mergedList[i].z;
            }
            else if (mergedList[i].z > zMax)
            {
                zMax = mergedList[i].z;
            }
        }

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

            // Resets min- and max values.
            if (i == 0) {
                xMin = xMax = convertedList[^1].x;
                yMin = yMax = convertedList[^1].y;
                zMin = zMax = convertedList[^1].z;
            }

            // x
            if (convertedList[^1].x < xMin)
            {
                xMin = convertedList[^1].x;
            }
            else if (convertedList[^1].x > xMax)
            {
                xMax = convertedList[^1].x;
            }

            // y
            if (convertedList[^1].y < yMin)
            {
                yMin = convertedList[^1].y;
            }
            else if (convertedList[^1].y > yMax)
            {
                yMax = convertedList[^1].y;
            }

            // z
            if (convertedList[^1].z < zMin)
            {
                zMin = convertedList[^1].z;
            }
            else if (convertedList[^1].z > zMax)
            {
                zMax = convertedList[^1].z;
            }
        }

        Debug.Log("xMin: " + xMin);
        Debug.Log("xMax: " + xMax);

        Debug.Log("yMin: " + yMin);
        Debug.Log("yMax: " + yMax);

        Debug.Log("zMin: " + zMin);
        Debug.Log("zMax: " + zMax);

        mergedList.Clear();
    }

    // Converts an inputted terrain into a smooth version of itself.
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
        
        // used for calculating the final smooth points.
        float middleX = 0;
        float middleZ = 0;


        // List used for making new points in the pointcloud.
        // This mess is best imagined as a plane, that acts as a List of rows, that contain Lists of given areas (squares for example),
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
                
            // Adds the recently split three string values to the mergedList as a Vector3 of floats by Parsing them.
            // NOTE: For some reason not adding "CultureInfo.InvariantCulture.NumberFormat" 
            // makes the renderer unable to recognise the file as valid. I assume that this is because it sees the ".'s" in the float values
            // and gets confused. I therefore believe that "CultureInfo.InvariantCulture.NumberFormat" makes the code read the ".'s" as ",".
            // Could this be a matter of Unity not liking the fact that the language on my computer is Norwegain rather than its standard?^^
            var convertedPoint = new Vector3(
                    float.Parse(pointValues[0], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat)
                    );


            // Defines "o" and "p" in the current iteration.
            var o = (int)((convertedPoint.x - xMin) / deltaX);
            var p = (int)((convertedPoint.z - zMin) / deltaZ);

            // Makes sure "o" and "p" are not out of bounds.
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


        float averageHeight = 0;

        // Loop through the "rows" of the "plane" called 'buckets'.
        for (int i = 0; i < xStep; i++)
        {   
            // Loop through the "squares" of the current "row" in 'buckets'.
            for (int j = 0; j < zStep; j++)
            {   
                // If the mask is filled, then do this:
                if (bMaskfilled[i, j])
                {
                    // "Do this to each point that exists in the bucket".
                    // In other words: Loop through each "square" in each "row" and do this to its points.
                    foreach (var currentPoint in buckets[i,j])
                    {
                        // Add all height-values to a singular value called 'averageHeight'.
                        averageHeight += currentPoint.y;
                    }

                    // This is in essence an if-check that makes sure that we do not continue making the points if there are no points in the current bucket,
                    // as that would give us an incorrect value (You cannot divide by "0" as that gives an error, and dividing by "1" returns the same number).

                    var numberOfPoints = 0;
                    if (buckets[i, j].Count > 0)
                    {
                        numberOfPoints = buckets[i,j].Count;
                    }
                    else
                    {
                        numberOfPoints = 1;
                    }

                    // Find the middle value of the x-coordinate in the "square" in 'buckets'.
                    middleX = xMin + (deltaX / 2) + (deltaX * i);
                    // Divide said 'averageHeight' on the amount of items (Height's) to get the actual average height.
                    averageHeight /= numberOfPoints;
                    // Find the middle value of the z-coordinate in the "square" in 'buckets'.
                    middleZ = zMin + (deltaZ / 2) + (deltaZ * j);

                    // Creates a final point for each square that is then added to a smoothTerrainList.
                    smoothTerrainList.Add(
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

        for (int i = 0; i < xStep; i++)
        {
            for (int j = 0; j < zStep; j++)
            {
                // If the mask is filled, and does not require further editing, then simply skip this double for-loop.
                if (bMaskfilled[i, j] == true)
                {
                    continue;
                }
                else
                {
                    // THIS IS HEAVILY INSPIRED BY ANDERS' CODE.

                    int numberOfPoints = 0;
                    // Used to find a proper y-value to the new points
                    var tempY = 0f;

                    // Compares the x-values of 10 neighbours in the x-direction (xStep) to get an accurate x-value.
                    for (int xn = i - 10; xn <= i + 10; xn++)
                    {
                        if (xn < 0 || xn >= xStep) 
                        {
                            continue;
                        }
                        
                        // Compares the z-values of 10 neighbours in the z-direction (zStep) to get an accurate z-value.
                        for (int zn = j - 10; zn <= j + 10; zn++)
                        {
                            if (zn < 0 || zn >= zStep || !bMaskfilled[xn, zn]) 
                            {
                                continue;
                            }
                            
                            // Adds the y-values of the x- and z- neighbours.
                            tempY += smoothTerrainList[xn*zStep + zn].y;
                            numberOfPoints++;
                        }
                    }

                    // Divides the temporary, new y-value on the amount of points to get an average y-value.
                    if (numberOfPoints > 0)
                    {
                        tempY /= numberOfPoints;
                    }
                    // Creates a temporary Vector.
                    var tempVec = smoothTerrainList[i*zStep + j];
                    // Adds said temporary vector to its proper position in the smoothTerrainList -List.
                    smoothTerrainList[i*zStep + j] = new Vector3(tempVec.x, tempY, tempVec.z);
                    // Sets this mask as filled.
                    bMaskfilled[i, j] = true;
                }
            }
        }
    }

    // Writes an inputted List over to a file.
    void writePointcloud(List<Vector3> input, string output)
    {
        // Puts the amount of lines in the inputted List at the top of the terrainFile-file.
        File.WriteAllText(output, input.Count.ToString() + "\n");

        // Loops throught convertedList and formats each vector into a string, which is then printed out in the terrainFile-file.
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
}
