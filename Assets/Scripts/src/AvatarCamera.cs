using UnityEngine;
using System.Collections;



public class AvatarCamera : MonoBehaviour
{

    public Transform target;

    public float targetHeight = 1.7f;

    public float distance = 5.0f;

    public float maxDistance = 20;
    public float minDistance = .6f;

    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;

    public int yMinLimit = -80;
    public int yMaxLimit = 80;

    public int zoomRate = 40;

    public float rotationDampening = 3.0f;
    public float zoomDampening = 5.0f;

    private float x = 0.0f;
    private float y = 0.0f;

    private float currentDistance;
    private float desiredDistance;
    private float correctedDistance;

    private bool isSwitchingPerspective = false;

    public GameObject spinner;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;

        x = angles.x;
        y = angles.y;

        currentDistance = distance;
        desiredDistance = distance;
        correctedDistance = distance;

        // Make the rigid body not change rotation
        if (rigidbody) rigidbody.freezeRotation = true;

        NotificationCenter.DefaultCenter().AddObserver(this, "LoadingSpinnerON");
        NotificationCenter.DefaultCenter().AddObserver(this, "LoadingSpinnerOFF");


    }

    public void SetTargetEuler(float _euler)
    {
        x = _euler;
    }
    public void CloseUp()
    {
        desiredDistance = 0.1f;
    }

    //For drcs
    public void TurnVehicle()
    {
        x += Input.GetAxis("Horizontal") * 50 * Time.deltaTime;
    }


    /**

     * Camera logic on LateUpdate to only update after all character movement logic has been handled.

     */
    void LateUpdate()
    {
        // Don't do anything if target is not defined
        if (!target)
            return;

        //roate camera as per right analog stick


        if (Input.GetMouseButton(1))
        {
            // If the right mouse button is held down, let the mouse govern camera position
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }
        else if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
        {
            //If cntr is held, use input from movement axis to move camera
            x += Input.GetAxis("Horizontal") * xSpeed * 0.02f;
            y -= Input.GetAxis("Vertical") * ySpeed * 0.02f;
        }
        else
        {
            //else get input from controller
            x += Input.GetAxis("Joy X") * xSpeed * 0.003f;
            y -= Input.GetAxis("Joy Y") * ySpeed * 0.003f;

        }

        y = ClampAngle(y, yMinLimit, yMaxLimit);


        // calculate the desired distance
        desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);


        /*
        if (Input.GetButtonDown("Perspective"))
        {
            if (currentDistance < 0.5)
            {
                desiredDistance = maxDistance / 2;
            }
            else
            {
                isSwitchingPerspective = true;
                desiredDistance = 0.1f;
            }

        }
         */

        if (isSwitchingPerspective)
        {
            float targetRotationAngle = target.eulerAngles.y;
            float currentRotationAngle = transform.eulerAngles.y;
            x = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, rotationDampening * Time.deltaTime);
            if ((int)x == (int)targetRotationAngle) isSwitchingPerspective = false;
        }

        if (currentDistance > 0.5)
        {
            foreach (Renderer r in target.GetComponentsInChildren<Renderer>())
                r.enabled = true;
        }
        else
        {
            foreach (Renderer r in target.GetComponentsInChildren<Renderer>())
                r.enabled = false;
        }

        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        correctedDistance = desiredDistance;

        // calculate desired camera position
        Vector3 position = target.position - (rotation * Vector3.forward * desiredDistance + new Vector3(0, -targetHeight, 0));

        Vector3 trueTargetPosition = new Vector3(target.position.x, target.position.y + targetHeight, target.position.z);
        correctedDistance = Vector3.Distance(trueTargetPosition, position);
        // For smoothing, lerp distance only if either distance wasn't corrected, or correctedDistance is more than currentDistance

        currentDistance = Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomDampening);

        // recalculate position based on the new currentDistance

        position = target.position - (rotation * Vector3.forward * currentDistance + new Vector3(0, -targetHeight, 0));
        transform.rotation = rotation;
        transform.position = position;

    }

    //Helper function that keeps angle within bounds
    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        return Mathf.Clamp(angle, min, max);
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