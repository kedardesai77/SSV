using UnityEngine;
using System.Collections;

public class SpectatorCamera : MonoBehaviour 
{

	public Transform target;
	
	public float targetHeight = 0.0f;
	
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
	
	private bool switchingToFp = false;
	
	public bool showPivotOnRotate = false;
	private bool isDoingReset = false;

    public bool pantarget;

	float def_x , def_y;


    public GameObject spinner;

	public void Start ()
	{
		
		Vector3 angles = transform.eulerAngles;
		
		x = def_x = angles.x;
		y = def_y = angles.y;

		
		currentDistance = distance;
		desiredDistance = distance;
		correctedDistance = distance;



        NotificationCenter.DefaultCenter().AddObserver(this, "LoadingSpinnerON");
        NotificationCenter.DefaultCenter().AddObserver(this, "LoadingSpinnerOFF");

        
	}
	
	
	public void ResetView()
	{
		
		StartCoroutine(DoReset());
	}
	
	IEnumerator DoReset()
	{
		float i = 0.0f;
		float rate = 1.5f;

		isDoingReset = true;
		while (i < 1.0)
		{
			i += Time.deltaTime * rate;
			x = Mathf.LerpAngle(x , def_x , i );
			y = Mathf.LerpAngle(y , def_y , i );
			currentDistance = Mathf.Lerp(currentDistance, distance , i);
			desiredDistance = Mathf.Lerp(desiredDistance, distance , i);
			correctedDistance = Mathf.Lerp(correctedDistance, distance , i);       		
			yield return null;
		}
        camera.orthographicSize = 5;
		isDoingReset = false;


	}
	public void ResetDistance()
	{
		desiredDistance = distance;	
	}
	public void CloseUp()
	{
		desiredDistance = 0.5f;
	}
	public void CloseUp(Vector3 size)
	{
		float max = 0.0f;
		if(size.x > max)
			max = size.x;
		if(size.y > max)
			max = size.y;
		if(size.z > max)
			max = size.z;
		
		desiredDistance = max * 2.0f;
	}	


	

	
	/**

     * Camera logic on LateUpdate to only update after all character movement logic has been handled.

     */
	void LateUpdate ()
	{
		// Don't do anything if target is not defined
		if (!target)
			return;

		//roate camera as per right analog stick
		if ( Input.GetMouseButton(1))
		{
			if(!isDoingReset)
			{
				x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
				y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			}
            if (showPivotOnRotate && pantarget)
            {
                target.GetComponent<HighlightableObject>().ConstantOn(Color.green);
                target.renderer.enabled = true;
            }

		}
		else if( Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
		{
			x += Input.GetAxis("Horizontal") * xSpeed * 0.02f;
			y -= Input.GetAxis("Vertical") * ySpeed * 0.02f;
		}
		else if(Input.GetMouseButton(2) && pantarget)
		{
			target.transform.position -= transform.right * Input.GetAxis("Mouse X") ; 
			target.transform.position -= transform.up * Input.GetAxis("Mouse Y") ;
            if (showPivotOnRotate)
            {
                target.GetComponent<HighlightableObject>().ConstantOn(Color.green);
                target.renderer.enabled = true;
            }
		}
		else
		{
			x += Input.GetAxis("Joy X") * xSpeed * 0.003f;
			y -= Input.GetAxis("Joy Y") * ySpeed * 0.003f;
            if (pantarget)
            {
                target.renderer.enabled = false;
                //target.GetComponent<HighlightableObject>().ConstantOff();
            }
                
		}
		

        desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);	
        

		
		y = ClampAngle(y, yMinLimit, yMaxLimit);
		
		desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
		Quaternion rotation = Quaternion.Euler(y, x, 0);
		correctedDistance = desiredDistance;
		
		// calculate desired camera position
		Vector3 position = new Vector3();
		Vector3 trueTargetPosition = new Vector3();
		if(target.renderer == null)
		{
			position = target.transform.position - (rotation * Vector3.forward * desiredDistance + new Vector3(0, -targetHeight, 0));
			trueTargetPosition = new Vector3(
				target.transform.position.x, target.transform.position.y + targetHeight, target.transform.position.z);
		}
		else
		{
			position = target.renderer.bounds.center - (rotation * Vector3.forward * desiredDistance + new Vector3(0, -targetHeight, 0));
			trueTargetPosition = new Vector3(
				target.renderer.bounds.center.x, target.renderer.bounds.center.y + targetHeight, target.renderer.bounds.center.z);
		}
		correctedDistance = Vector3.Distance(trueTargetPosition, position);
		// For smoothing, lerp distance only if either distance wasn't corrected, or correctedDistance is more than currentDistance
		
		currentDistance = Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomDampening);
		
		// recalculate position based on the new currentDistance
		if(target.renderer == null)
		{
			position = target.transform.position - (rotation * Vector3.forward * currentDistance + new Vector3(0, -targetHeight, 0));
		}
		else
		{
			position = target.renderer.bounds.center - (rotation * Vector3.forward * currentDistance + new Vector3(0, -targetHeight, 0));	
		}
		
		transform.rotation = rotation;
		//if(!isLocked && target.renderer == null)
		//	target.eulerAngles = transform.eulerAngles;
		transform.position = position;
		
	}

	public void SetTargetEuler(float  _euler)
	{
		x = _euler;
	}

	
	private static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360)angle += 360;
		if (angle > 360)angle -= 360;
		
		return Mathf.Clamp(angle, min, max);
	}





    /*
     * Loading spinner
     * 
     * 
     */

    public void LoadingSpinnerON(Notification n)
    {
        spinner.SetActive(true);
    }


    public void LoadingSpinnerOFF(Notification n)
    {
        spinner.SetActive(false);
    }
	



}
