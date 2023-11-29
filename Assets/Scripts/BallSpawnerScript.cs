using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawnerScript : MonoBehaviour
{
    public GameObject Ball;

    // Start is called before the first frame update
    void Start()
    {
        if (TriangleSurfaceScript.triangleSurfaceInstance.bGenerateTriangleSurface == true)
        {
            Instantiate(Ball, new Vector3(0, 300, 700), Quaternion.identity);
        }
    }
}
