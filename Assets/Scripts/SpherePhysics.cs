using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.Timeline;
using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    public static SpherePhysics sphereInstance { get; private set; }

    [SerializeField] public float SphereRadius = 1.0f;
    [SerializeField] float SphereWeight = 0.06f;

    public float gravity = 9.81f;

    // They're both zero-vectors, AKA. Vector3(0.0f, 0.0f, 0.0f)
    Vector3 Velocity = Vector3.zero;
    Vector3 Acceleration = Vector3.zero;

    private void Awake() {
        sphereInstance = this;
    }
    
    // Fixed update runs at a constant FPS, important for physics
    // In FixedUpdate we used Time.fixedDeltaTime as opposed to Time.deltaTime in Update()
    void FixedUpdate() 
    {
        Triangle triangleRef = CollisionScript.collisionScriptInstance.CurrentTriangle;
        
        // Velocity before potential collision
        Vector3 startVel = Velocity;
        Velocity += Vector3.down * gravity * Time.fixedDeltaTime;
        transform.position += Velocity * Time.fixedDeltaTime;
        
        if (triangleRef != null)
        {
            // Velocity after collision
            Vector3 colVel = Velocity - Vector3.Dot(Velocity, triangleRef.unitNormal) * triangleRef.unitNormal;
            float velNorm = Vector3.Dot(triangleRef.unitNormal, colVel);
            colVel += -velNorm * triangleRef.unitNormal;

            Velocity = colVel;

        }
       
        transform.position = new Vector3(transform.position.x, 
        BarycentricCoordinates.barycInstance.HeightFromBaryc(new Vector2(transform.position.x, transform.position.z)) + 
        sphereInstance.SphereRadius, transform.position.z);

        Acceleration = (Velocity - startVel) / Time.deltaTime;      
    }  
}
