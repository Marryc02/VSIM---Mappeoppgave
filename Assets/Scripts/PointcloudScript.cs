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

    string mergedText = @"Assets/Resources/merged.txt";
    string terrainText = @"Assets/Resources/terrain.txt";
    string smoothTerrain = @"Assets/Resources/smoothTerrain.txt";

    // Used for deciding how many lines should be skipped when converting the initial document and making a smooth pointcloud.
    float lineSkips = 25;

    // Used to make the pointcloud look nice. Also helps make the triangleSurface smoot later.
    float xMin = 0; float xMax = 0;
    float yMin = 0; float yMax = 0;
    float zMin = 0; float zMax = 0;

    float xStep = 10; float zStep = 7;
    float deltaX = 0; float deltaZ = 0;
    


    // Runs before Start().
    void Awake() {
        if (bDataGenerated)
        {
            // Finds out how many lines there are in the inputted .txt-document.
            var lineCount = File.ReadLines(mergedText).Count();
            Debug.Log("Amount of lines in the original mergedText-file: " + lineCount);

            // Puts the amount of lines in the .txt document in the top of a new .txt-document.
            File.WriteAllText(terrainText, lineCount.ToString() + "\n");

            // Finds min and max values.
            findMinAndMax(lineCount);



            // As it currently stands there are over a million lines in the text document, 
            // where virtually all of the lines have incredibly large values, and as such we will be scaling down the values to bring them closer
            // to the origin of the scene, thus making it easer for us to showcase them.
            ConvertMerged(lineCount);
            Debug.Log("Merged converted and reduced.");

            // -1 since we ignore the first line.
            lineCount = File.ReadLines(terrainText).Count() - 1;
            Debug.Log("Amount of lines in the new terrainText-file: " + lineCount);
            // Stores the entire terrain -file in a string that inherits its size from "lineCount".
            string[] lines = new string[lineCount];
            lines = File.ReadAllLines(terrainText);
            // Overwrites the very first, empty line in the terrain -file.
            lines[0] = lineCount.ToString();
            // Writes the entire lines -string into the terrainText file, without touching the very first line that tells the number of lines.
            File.WriteAllLines(terrainText, lines);



            lineCount = File.ReadLines(terrainText).Count();
            Debug.Log("Amount of lines in the terrain-file: " + lineCount);

            // Here we convert our terrain -file to one that looks smoother, and that can help us achieve a smooth texture for our triangles later.
            ConvertTerrainToSmooth(lineCount);
            Debug.Log("smoothTerrain converted and reduced.");

            // -1 since we ignore the first line.
            // Checking this again just in case
            lineCount = File.ReadLines(smoothTerrain).Count() - 1;
            Debug.Log("Amount of lines in the new smoothTerrain-file: " + lineCount);

            // Stores the entire smoothTerrain -file in a string that inherits its size from "lineCount".
            lines = new string[lineCount];
            lines = File.ReadAllLines(smoothTerrain);
            // Overwrites the very first, empty line in the smoothTerrain -file.
            lines[0] = lineCount.ToString();
            // Writes the entire lines -string into the terrainText file, without touching the very first line that tells the number of lines.
            File.WriteAllLines(smoothTerrain, lines);
        }
    }

    void findMinAndMax(int fileLength)
    {
        string line;
        
        // Pass the file path and file name to the StreamReader constructor
        StreamReader readFile = new StreamReader(mergedText);

        int a = 0;

        for (int i = 0; i < fileLength; i++)
        {
            // Read the first line of text
            line = readFile.ReadLine();

            // The value is "200" because we only want to show every 200th line (or point) in the .txt-document as there are
            // way too many of them otherwise. (Unity crashed on me several times)
            // EDIT: THE INTEGER VALUE HAS BEEN REPLACED WITH A MODULAR VALUE THAT IS DECLARED ABOVE.
            if (a >= lineSkips)
            {
                // Makes a new list of strings with the name "pointValues".
                // Assigns the mergedText .txt-document as the value of the List, however it also splits each line in the .txt-document
                // in such a way that the document writes a new line with everything that comes after a space in the .txt-document all while
                // deleting empty spaces in the .txt-document.
                // Lastly it converts the document to a List as it is technically just a really long string with a format.
                List<String> pointValues = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                
                // Creates a new list like previously, but made of floats instead. 
                // This is so that we can directly alter the individual values in the points.
                // We also swap the y- and z- values as 'kartverket' uses the z-value to denote height which is a no-go for us.
                // Lastly we subtract a high value that exists on every single line on the width- and length- axis so that we can bring the points
                // closer to the scenes' origin.
                List<float> fPointValues = new List<float>();
                fPointValues.Add(float.Parse(pointValues[0], CultureInfo.InvariantCulture.NumberFormat));
                fPointValues.Add(float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat));
                fPointValues.Add(float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat));

                // Assigns min and max values after checking them
                if (xMin == 0)
                {
                    xMin = fPointValues[0];
                }
                else if (fPointValues[0] < xMin)
                {
                    xMin = fPointValues[0];
                }

                if (xMax == 0)
                {
                    xMax = fPointValues[0];
                }
                else if (fPointValues[0] > xMax)
                {
                    xMax = fPointValues[0];
                }

                if (yMin == 0)
                {
                    yMin = fPointValues[1];
                }
                else if (fPointValues[1] < yMin)
                {
                    yMin = fPointValues[1];
                }

                if (yMax == 0)
                {
                    yMax = fPointValues[1];
                }
                else if (fPointValues[1] > yMax)
                {
                    yMax = fPointValues[1];
                }

                if (zMin == 0)
                {
                    zMin = fPointValues[2];
                }
                else if (fPointValues[2] < zMin)
                {
                    zMin = fPointValues[2];
                }

                if (zMax == 0)
                {
                    zMax = fPointValues[2];
                }
                else if (fPointValues[2] > zMax)
                {
                    zMax = fPointValues[2];
                }
                
                // Clears the list to save memory.
                fPointValues.Clear();
            }
            else
            {
                a++;
            }
        }

        Debug.Log("xMin: " + xMin);
        Debug.Log("xMax: " + xMax);

        Debug.Log("yMin: " + yMin);
        Debug.Log("yMax: " + yMax);

        Debug.Log("zMin: " + zMin);
        Debug.Log("zMax: " + zMax);
    }

    void ConvertMerged(int fileLength)
    {
        string line;
        
        // Pass the file path and file name to the StreamReader constructor
        StreamReader readFile = new StreamReader(mergedText);

        int a = 0;

        for (int i = 0; i < fileLength; i++)
        {
            // Read the first line of text
            line = readFile.ReadLine();

            // The value is "200" because we only want to show every 200th line (or point) in the .txt-document as there are
            // way too many of them otherwise. (Unity crashed on me several times)
            // EDIT: THE INTEGER VALUE HAS BEEN REPLACED WITH A MODULAR VALUE THAT IS DECLARED ABOVE.
            if (a >= lineSkips)
            {
                // Makes a new list of strings with the name "pointValues".
                // Assigns the mergedText .txt-document as the value of the List, however it also splits each line in the .txt-document
                // in such a way that the document writes a new line with everything that comes after a space in the .txt-document all while
                // deleting empty spaces in the .txt-document.
                // Lastly it converts the document to a List as it is technically just a really long string with a format.
                List<String> pointValues = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                
                // Creates a new list like previously, but made of floats instead. 
                // This is so that we can directly alter the individual values in the points.
                // We also swap the y- and z- values as 'kartverket' uses the z-value to denote height which is a no-go for us.
                // Lastly we subtract a high value that exists on every single line on the width- and length- axis so that we can bring the points
                // closer to the scenes' origin.
                List<float> fPointValues = new List<float>();
                fPointValues.Add(float.Parse(pointValues[0], CultureInfo.InvariantCulture.NumberFormat) - ((xMin + xMax) / 2));
                fPointValues.Add(float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat) - ((yMin + yMax) / 2));
                fPointValues.Add(float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat) - ((zMin + zMax) / 2));

                // Converts the points back into strings.
                pointValues[0] = fPointValues[0].ToString();
                pointValues[1] = fPointValues[1].ToString();
                pointValues[2] = fPointValues[2].ToString();
                
                // Clears the list to save memory.
                fPointValues.Clear();
                
                // Creates a new output-string that will act as a new line on the terrainText-file. This time with the right x, y, z order.
                string outputString = pointValues[0] + " " + pointValues[1] + " " + pointValues[2];

                using (StreamWriter writeFile = File.AppendText(terrainText))
                {
                    writeFile.WriteLine(outputString);
                } 
                a = 0;
            }
            else
            {
                a++;
            }
        }
    }

    void ConvertTerrainToSmooth(int fileLength)
    {
        List<float> o = new List<float>();
        List<float> p = new List<float>();

        List<List<Vector3>> buckets = new List<List<Vector3>>();

        deltaX = (xMax - xMin) / xStep;
        deltaZ = (zMax - zMin) / zStep;
        
        float averageHeight = 0;

        string line;

        // Pass the file path and file name to the StreamReader constructor
        StreamReader readFile = new StreamReader(terrainText);


        for (int i = 1; i < fileLength; i++)
        {
            // Read the first line of text
            line = readFile.ReadLine();

            // Makes a new list of strings with the name "pointValues".
            // Assigns the mergedText .txt-document as the value of the List, however it also splits each line in the .txt-document
            // in such a way that the document writes a new line with everything that comes after a space in the .txt-document all while
            // deleting empty spaces in the .txt-document.
            // Lastly it converts the document to a List as it is technically just a really long string with a format.
            List<String> pointValues = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                
            // Creates a new list like previously, but made of floats instead. 
            // This is so that we can directly alter the individual values in the points.
            // We also swap the y- and z- values as 'kartverket' uses the z-value to denote height which is a no-go for us.
            // Lastly we subtract a high value that exists on every single line on the width- and length- axis so that we can bring the points
            // closer to the scenes' origin.
            List<float> fPointValues = new List<float>();
            fPointValues.Add(float.Parse(pointValues[0], CultureInfo.InvariantCulture.NumberFormat));
            fPointValues.Add(float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat));
            fPointValues.Add(float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat));

            o[i - 1] = (fPointValues[0] - xMin) / deltaX;
            p[i - 1] = (fPointValues[2] - zMin) / deltaZ;   

            buckets[(int)Math.Round(o[i - 1])][(int)Math.Round(p[i - 1])] = (new Vector3(fPointValues[0], fPointValues[1], fPointValues[2]));

            // Clears the list to save memory.
            fPointValues.Clear();
        }

        for (int i = 0; i < buckets[0].Count; i++)
        {
            for (int j = 0; j < buckets[1].Count; j++)
            {
                averageHeight += buckets[1][j].y;
            }
            averageHeight = averageHeight / buckets[1].Count;

            string outputString = (xMin + (deltaX / 2) + (deltaX * o[i])) + " " + averageHeight + " " + (zMin + (deltaZ / 2) + (deltaZ * p[i]));

            using (StreamWriter writeFile = File.AppendText(smoothTerrain))
            {
                writeFile.WriteLine(outputString);
            }
        }
    }
}
