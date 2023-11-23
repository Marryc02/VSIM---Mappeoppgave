using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System;
using UnityEngine;

public class TriangleSurfaceScript : MonoBehaviour
{   
    string smoothTerrainFile = @"Assets/Resources/smoothTerrain.txt";

    // List containing the direct values of the "smoothTerrainFile -file".
    List<Vector3> vertices = new List<Vector3>();

    void readTerrainFile(string input)
    {
        // Pass the file path and file name to the StreamReader constructor.
        StreamReader readFile = new StreamReader(input);

        // Finds out how many lines there are in the inputted .txt-document.
        var lineCount = File.ReadLines(input).Count();
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
                    float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat)
                )
            );
        }

        
    }  
}

public class Triangles
{

}
