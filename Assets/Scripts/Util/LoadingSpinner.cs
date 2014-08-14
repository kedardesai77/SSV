using UnityEngine;
using System.Collections;

public class LoadingSpinner : MonoBehaviour {


    Vector3 vec = new Vector3(Screen.width - 100, 75, 0.012f);



	// Update is called once per frame
	void LateUpdate () 
    {

        transform.position = Camera.main.camera.ScreenToWorldPoint(vec);
        transform.Rotate(Vector3.up, 8);
	
	}
}
