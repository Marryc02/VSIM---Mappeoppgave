using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionScript : MonoBehaviour
{
    public static CollisionScript collisionScriptInstance { get; private set; }
    [SerializeField] GameObject collisionObject;

    Vector3 centre;
    Vector3 barycVector;
    [System.NonSerialized] public Vector3 collisionPoint;
    public Triangle CurrentTriangle;
    [HideInInspector] public bool currentTriangleCheckedOnce = false;


    void Awake() 
    {
        collisionScriptInstance = this;   
    }

    // Main loop that finds a current triangle and adjusts the ball's y-value based on the point where it collides with the triangle.
    void FixedUpdate()
    {
        centre = collisionObject.transform.position;
        CurrentTriangle = CheckCurrentTriangle();
        Debug.Log("Current triangle is: " + CurrentTriangle);
        barycVector = collisionObject.transform.position;
        barycVector.y = BarycentricCoordinates.barycInstance.HeightFromBaryc(new Vector2(collisionObject.transform.position.x, collisionObject.transform.position.z));

        if (CurrentTriangle != null)
        {
            collisionPoint = centre + (Vector3.Dot(barycVector - centre, CurrentTriangle.unitNormal) * CurrentTriangle.unitNormal);
        }
        
    }

    // Checks if the ball is inside the current triangle.
    Triangle CheckCurrentTriangle()
    {
        // If-check that is used to check if we have already checked for an intial triangle.
        //if (currentTriangleCheckedOnce == false)
        //{
            // Loop through all triangles and check if the balls centre is in one of them. 
            // If true; then set the current triangle in the "madeTriangles" -List as the current triangle, and set "currentTriangleCheckedOnce" to "true".
            for (int i = 0; i < TriangleSurfaceScript.triangleSurfaceInstance.madeTriangles.Count; i++)
            {
                if (TriangleSurfaceScript.triangleSurfaceInstance.madeTriangles[i].IsInTriangle(centre))
                {
                    currentTriangleCheckedOnce = true;
                    return TriangleSurfaceScript.triangleSurfaceInstance.madeTriangles[i];
                }
            }
        //}
        /*// Otherwise if we have already checked for an initial triangfle once, then do this:
        else
        {
            // Loop through the current triangle's neighbours.
            for (int i = 0; i < CurrentTriangle.neighbours.Length; i++)
            {
                // Loop through the "madeTriangles" -List based on which triangles are neighbours of the current one,
                // then check if the ball is inside one of them and set said triangle as the new current triangle.
                if (TriangleSurfaceScript.triangleSurfaceInstance.madeTriangles[CurrentTriangle.neighbours[i].index].IsInTriangle(centre))
                {
                    return TriangleSurfaceScript.triangleSurfaceInstance.madeTriangles[i];
                }
            }
        }*/

        //Debug.LogWarning("Could not find triangle");
        return null;
    }
}
