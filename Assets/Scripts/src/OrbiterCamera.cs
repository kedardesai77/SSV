using UnityEngine;
using System.Collections;

public class OrbiterCamera : MonoBehaviour {

    public Transform target;
    public float xSpeed = 12.0f;
    public float ySpeed = 12.0f;
    public float scrollSpeed = 10.0f;

    public bool showPivotOnPan = false;

    public float zoomMin = 1.0f;

    public float zoomMax = 20.0f;

    public float distance;

    public Vector3 position;

    public bool isActivated;
    public bool isPanning;

    float x = 0.0f;
    float y = 0.0f;


    public bool autoOrbit = false;
    public float autoOrbitSpeed = 4.0f;


    private Quaternion start_rot;
    private Vector3 start_euler;

    public GameObject spinner;

    // Use this for initialization
    void Start()
    {
         start_rot = transform.rotation;
         start_euler = transform.eulerAngles;
         x = start_euler.y;
         y = start_euler.x;

         NotificationCenter.DefaultCenter().AddObserver(this, "LoadingSpinnerON");
         NotificationCenter.DefaultCenter().AddObserver(this, "LoadingSpinnerOFF");
    }

    void OnGUI()
    {
        GUI.Box(new Rect(2, Screen.height - 25, 78, 20) , "");
        autoOrbit = GUI.Toggle(new Rect(5, Screen.height - 25, 100, 15), autoOrbit, new GUIContent("Animate"));
    }




    public void ResetView(Vector3 center , bool oritentation = true)
    {
        if(oritentation)
        {
            transform.eulerAngles = start_euler;
            transform.rotation = start_rot;
        }

        target.position = center;

        // get the distance between camera and target
        distance = Vector3.Distance(transform.position, target.position);

        // position the camera FORWARD the right distance towards target
        position = -(transform.forward * 5) + target.position;

        // move the camera
        transform.position = position;


    }


    void LateUpdate()
    {

        // only update if the mousebutton is held down
        if (Input.GetMouseButtonDown(1)) isActivated = true;
        
        // if mouse button is let UP then stop rotating camera
        if (Input.GetMouseButtonUp(1)) isActivated = false;
        

        if (autoOrbit)
        {
            transform.LookAt(target);
            transform.position += transform.right * autoOrbitSpeed * Time.deltaTime;
        }
        if (Input.GetMouseButton(2))
        {
            isPanning = true;
            target.transform.position -= transform.right * Input.GetAxis("Mouse X");
            target.transform.position -= transform.up * Input.GetAxis("Mouse Y");
            transform.position -= transform.right * Input.GetAxis("Mouse X");
            transform.position -= transform.up * Input.GetAxis("Mouse Y");

            if (showPivotOnPan) target.renderer.enabled = true;
        }
        else
        {
            isPanning = false;
            target.renderer.enabled = false;
        }

        if (target && (isActivated || isPanning))
        {

            //  get the distance the mouse moved in the respective direction
            if (!isPanning)
            {
                x += Input.GetAxis("Mouse X") * xSpeed;
                y -= Input.GetAxis("Mouse Y") * ySpeed;
            }

            // when mouse moves left and right we actually rotate around local y axis	
            transform.RotateAround(target.position, transform.up, x);

            // when mouse moves up and down we actually rotate around the local x axis	
            transform.RotateAround(target.position, transform.right, y);

            // reset back to 0 so it doesn't continue to rotate while holding the button
            x = 0;
            y = 0;

        }
        else
        {

            // see if mouse wheel is used 	
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {

                // get the distance between camera and target
                distance = Vector3.Distance(transform.position, target.position);

                // get mouse wheel info to zoom in and out	
                distance = ZoomLimit(distance - Input.GetAxis("Mouse ScrollWheel") * scrollSpeed, zoomMin, zoomMax);

                // position the camera FORWARD the right distance towards target
                position = -(transform.forward * distance) + target.position;
                
                // move the camera
                transform.position = position;

            }

        }

    }

    public static float ZoomLimit(float dist, float min, float max)
    {
        if (dist < min)dist = min;
        if (dist > max)dist = max;
        return dist;
    }


    public void LoadingSpinnerON(Notification n)
    {
        spinner.SetActive(true);
    }


    public void LoadingSpinnerOFF(Notification n)
    {
        spinner.SetActive(false);
    }

    
}