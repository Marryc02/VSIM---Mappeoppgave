using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System;
using UnityEngine;
using System.Xml.Schema;
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
    float lineSkips = 25;

    // Used to make the pointcloud look nice. Also helps make the triangleSurface smoot later.
    float xMin = 0; float xMax = 0;
    float yMin = 0; float yMax = 0;
    float zMin = 0; float zMax = 0;

    // List that contains the final converted values for the terrainFile-file.
    List<Vector3> convertedList = new List<Vector3>();
    // List that contains the final converted values for the smoothTerrainFile-file.
    List<Vector3> smoothTerrainList = new List<Vector3>();

    float xStep = 10; float zStep = 7;
    float deltaX = 0; float deltaZ = 0;
    


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
            Debug.Log("Terrain has been smoothed successfully.")

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

            // An if-check that helps us determine how many lines we want to skip in the file. (I want fewer lines in the new terrain-file).
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

        // Finds the smallst and largest values 
        for (int i = 0; i < mergedList.Count; i++)
        {
            // Assigns min and max values of x, y and z after checking them.
            // x
            if (xMin == 0)
            {
                xMin = mergedList[i].x;
            }
            else if (mergedList[i].x < xMin)
            {
                xMin = mergedList[i].x;
            }

            if (xMax == 0)
            {
                xMax = mergedList[i].x;
            }
            else if (mergedList[i].x > xMax)
            {
                xMax = mergedList[i].x;
            }

            // y
            if (yMin == 0)
            {
                yMin = mergedList[i].y;
            }
            else if (mergedList[i].y < yMin)
            {
                yMin = mergedList[i].y;
            }

            if (yMax == 0)
            {
                yMax = mergedList[i].y;
            }
            else if (mergedList[i].y > yMax)
            {
                yMax = mergedList[i].y;
            }

            // z
            if (zMin == 0)
            {
                zMin = mergedList[i].z;
            }
            else if (mergedList[i].z < zMin)
            {
                zMin = mergedList[i].z;
            }

            if (zMax == 0)
            {
                zMax = mergedList[i].z;
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
        // (convertedList starts in the scenes origin).
        for (int i = 0; i < mergedList.Count; i++)
        {
            float tempX = mergedList[i].x - (xMin + xMax) / 2;
            float tempY = mergedList[i].y - (yMin + yMax) / 2;
            float tempZ = mergedList[i].z - (zMin + zMax) / 2;

            // Creates a final point for each square that is then added to a convertedList.
            convertedList.Add(
                new Vector3(
                    tempX,
                    tempY,
                    tempZ
                )
            );
        }

        mergedList.Clear();
    }

    // Converts an inputted terrain into a smooth version of itself.
    void ConvertTerrainToSmooth(string input)
    {
        // Pass the file path and file name to the StreamReader constructor.
        StreamReader readFile = new StreamReader(input);

        // Finds out how many lines there are in the inputted .txt-document.
        var lineCount = File.ReadLines(input).Count();
        Debug.Log("Amount of lines in the original terrain-file: " + lineCount);


        // Calculates deltaX and deltaZ
        deltaX = (xMax - xMin) / xStep;
        deltaZ = (zMax - zMin) / zStep;
        
        // used for calculating the final smooth points.
        float averageHeight = 0;
        float middleX = 0;
        float middleZ = 0;


        // List for converting the terrain-file to a list of Vector3's.
        List<Vector3> smoothConvertedList = new List<Vector3>();


        // Used in the List "buckets".
        List<float> o = new List<float>;
        List<float> p = new List<float>;

        // List used for making new points in the pointcloud.
        // This mess is best imagined as a plane, that acts as a List of rows, that contain Lists of given areas (squares for example),
        // who themselves act as a List of Vector3's.
        List<List<List<Vector3>>> buckets = new List<List<List<Vector3>>>();


        // Reads the first line of text.
        string line = readFile.ReadLine();

        for (int i = 0; i < lineCount - 1; i++)
        {
            // Reads the second line of text.
            line = readFile.ReadLine();

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
            smoothConvertedList.Add(
                new Vector3(
                    float.Parse(pointValues[0], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat)
                    )
            );

            // Defines "o" and "p" in the current iteration.
            o[i] = (smoothConvertedList[i].x - xMin) / deltaX;
            p[i] = (smoothConvertedList[i].z - zMin) / deltaZ;

            // Adds Vector3's to the "squares" inside the "rows" of the "plane" called 'buckets'.
            buckets[(int)Math.Round(o[i])][(int)Math.Round(p[i])].Add(
                new Vector3(
                    smoothConvertedList[i].x,
                    smoothConvertedList[i].y,
                    smoothConvertedList[i].z
                )
            );
        }

        // Loop through the "rows" of the "plane" called 'buckets'.
        for (int i = 0; i < buckets[(int)Math.Round(o[i])].Count; i++)
        {   
            // Loop through the "squares" of the current "row" in 'buckets'.
            for (int j = 0; j < buckets[(int)Math.Round(p[j])].Count; j++)
            {
                // Loop through the list of Vector3's in the "square" in 'buckets'.
                for (int k = 0; k < buckets[k].Count; k++)
                {
                    // Add all height-values to a singular value called 'averageHeight'.
                    averageHeight += buckets[(int)Math.Round(o[i])][(int)Math.Round(p[j])][k].y;
                }
                // Find the middle value of the x-coordinate in the "square" in 'buckets'.
                middleX = xMin + (deltaX / 2) + (deltaX * o[i]);
                // Divide said 'averageHeight' on the amount of items (Height's) to get the actual average height.
                averageHeight = averageHeight / buckets[(int)Math.Round(o[i])][(int)Math.Round(p[j])].Count;
                // Find the middle value of the z-coordinate in the "square" in 'buckets'.
                middleZ = zMin + (deltaZ / 2) + (deltaZ * p[j]);

                // Creates a final point for each square that is then added to a smoothTerrainList.
                smoothTerrainList.Add(
                    new Vector3(
                        middleX,
                        averageHeight,
                        middleZ
                    )
                );
            }
        }

        smoothConvertedList.Clear();
    }

    // Writes an inputted List over to a file.
    void writePointcloud(List<Vector3> input, string output)
    {
        // Puts the amount of lines in the inputted List at the top of the terrainFile-file.
        File.WriteAllText(output, input.Count.ToString() + "\n");

        // Loops throught convertedList and formats each vector into a string, which is then printed out in the terrainFile-file.
        for (int i = 0; i < input.Count; i++)
        {
            var outputLine = input[i].x + " " + input[i].y + " " + input[i].z;
            outputLine = outputLine.Replace(",", ".");

            using (StreamWriter writeFile = File.AppendText(output))
            {
                writeFile.WriteLine(outputLine);
            }
        }

        input.Clear();
    }
}
