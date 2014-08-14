using UnityEngine;
using System.Collections;

public class PivotController : MonoBehaviour {

	public float cameraSensitivity = 90;
	public float climbSpeed = 4;
	public float normalMoveSpeed = 5;
	public float slowMoveSpeed = 0.5f;
	public float slowClimbSpeed = 1;
	
	public float slowMoveFactor = 0.25f;
	public float fastMoveFactor = 3;
	
	CharacterController controller;
	Rigidbody rigidBody;
	
	CollisionFlags cf;
	Vector3 y = new Vector3();
	Vector3 x = new Vector3();
	
	
	public void ResetView(Vector3 center_pos)
	{
		
		StartCoroutine(DoReset(center_pos));
	}
	
	IEnumerator DoReset(Vector3 center_pos)
	{
		
		float i = 0.0f;
		float rate = 1.5f;
		
		while(i < 1.0f)
		{
			i += Time.deltaTime * rate;
			transform.position = Vector3.Lerp(transform.position , center_pos , i);	
			yield return null;
		}
		
	}
	
	void Update()
	{
		
		
		if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl)) return;
		
		
		if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift) || Input.GetKey(KeyCode.JoystickButton3)){
			y = transform.forward * normalMoveSpeed * Input.GetAxis("Vertical");
			x =  transform.right * normalMoveSpeed * Input.GetAxis("Horizontal");
			
			if (Input.GetKey (KeyCode.Q) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton5)) {y += transform.up * climbSpeed;}
			else if (Input.GetKey (KeyCode.E)|| Input.GetKey(KeyCode.JoystickButton4)) {y -= transform.up * climbSpeed;}			
			
			
		}
		else
		{
			y = transform.forward * slowMoveSpeed * Input.GetAxis("Vertical");
			x =  transform.right * slowMoveSpeed * Input.GetAxis("Horizontal");
			
			if (Input.GetKey (KeyCode.Q) || Input.GetKey(KeyCode.Space)|| Input.GetKey(KeyCode.JoystickButton5)) {y += transform.up * slowClimbSpeed;}
			else if (Input.GetKey (KeyCode.E)|| Input.GetKey(KeyCode.JoystickButton4)) {y -= transform.up * slowClimbSpeed;}			
		}
		transform.position +=  (x+y) * Time.deltaTime;
		//transform.Rotate(Vector3.up, Input.GetAxis("Horizontal") * 50 * Time.deltaTime);
		transform.eulerAngles = Camera.main.transform.eulerAngles;		
	}
}
