using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionScript : MonoBehaviour
{
    public static CollisionScript ColInstance { get; private set; }
    [SerializeField] GameObject collisionObject; 
    Vector3 centre;
    Vector3 barycVector;
    Vector3 triangleUnitNormal;
    //float ballRadius = SpherePhysics.sphereInstance.SphereRadius;
    [System.NonSerialized]
    public Vector3 collisionPoint;

    public Triangle CurrentTriangle;


    private void Awake() 
    {
        ColInstance = this;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        centre = collisionObject.transform.position;
        CurrentTriangle = CheckCurrentTriangle();
        barycVector = collisionObject.transform.position;
        barycVector.y = BarycentricCoordinates.barycInstance.HeightFromBaryc(new Vector2(collisionObject.transform.position.x, collisionObject.transform.position.z));

        if (CurrentTriangle != null)
        {
            collisionPoint = centre + (Vector3.Dot(barycVector - centre, CurrentTriangle.unitNormal)) * CurrentTriangle.unitNormal;
        }
    }

    Triangle CheckCurrentTriangle()
    {
        for (int i = 0; i < TriangleSurfaceScript.triangleSurfaceInstance.madeTriangles.Count; i++)
        {
            if (TriangleSurfaceScript.triangleSurfaceInstance.madeTriangles[i].IsInTriangle(centre))
            {
                //Debug.Log("Found triangle");
                return TriangleSurfaceScript.triangleSurfaceInstance.madeTriangles[i];
            }
        }

        //Debug.LogWarning("Could not find triangle");
        return null;
        
    }
}
