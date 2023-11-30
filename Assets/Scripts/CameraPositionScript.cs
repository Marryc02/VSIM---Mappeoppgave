using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (PointCloudRenderScript.renderInstance.fileHasBeenChosen == true)
        {
            this.transform.position = new Vector3(0, 200, -500);
        }
        else if (PointCloudRenderScript.renderInstance.fileHasBeenChosen == false && 
                 TriangleSurfaceScript.triangleSurfaceInstance.bGenerateTriangleSurface == true)
        {
            this.transform.position = new Vector3(200, 500, 465);
            this.transform.Rotate(15, -90, 0, 0);
        }
    }
}
