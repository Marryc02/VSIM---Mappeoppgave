using System;
using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.Timeline;
using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    public static SpherePhysics sphereInstance { get; private set; }

    [SerializeField] public float SphereRadius = 1.0f;
    [SerializeField] float mass = 0.06f;
    Vector3 acceleration = new Vector3();

    Vector3 gravity = new Vector3(0, -9.81f, 0);
    Vector3 hitNormal;
    Vector3 gravitationalForce;
    Vector3 normalForce;
    Vector3 sumOfForces;

    // They're both zero-vectors, AKA. Vector3(0.0f, 0.0f, 0.0f)
    Vector3 Velocity = Vector3.zero;
    //Vector3 Acceleration = Vector3.zero;

    private void Awake() {
        sphereInstance = this;
        // Calculates gravitational force
        gravitationalForce = gravity * mass;
    }
    
    // Fixed update runs at a constant FPS, important for physics
    // In FixedUpdate we used Time.fixedDeltaTime as opposed to Time.deltaTime in Update()
    void FixedUpdate() 
    {
        Triangle triangleRef = CollisionScript.collisionScriptInstance.CurrentTriangle;
        
        /*if (triangleRef != null)
        {
            // Velocity after collision
            Vector3 colVel = Velocity - Vector3.Dot(Velocity, triangleRef.unitNormal) * triangleRef.unitNormal;
            float velNorm = Vector3.Dot(triangleRef.unitNormal, colVel);
            colVel += -velNorm * triangleRef.unitNormal;

            Velocity = colVel;

        }*/
        hitNormal = triangleRef.surfaceNormal;
        hitNormal.Normalize();
        normalForce = mass * 9.81f * hitNormal * Mathf.Cos(hitNormal.z);

        sumOfForces = gravitationalForce + normalForce;

        acceleration = sumOfForces / mass;

        // Velocity before potential collision
        Vector3 startVel = Velocity;
        Velocity += acceleration * Time.fixedDeltaTime;
        transform.position += Velocity;


        transform.position = new Vector3(transform.position.x, 
        BarycentricCoordinates.barycInstance.HeightFromBaryc(new Vector2(transform.position.x, transform.position.z)) + 
        sphereInstance.SphereRadius, transform.position.z);
    }  
}
