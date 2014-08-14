using UnityEngine;
using System.Collections;

public class OVRController : MonoBehaviour {

    public float cameraSensitivity = 90;
    public float climbSpeed = 0.005f;
    public float normalMoveSpeed = 5;
    public float slowMoveSpeed = 0.5f;
    public float slowClimbSpeed = 1;

    private float moveSpeed = 1.0f;

    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;

    CharacterController controller;
    Rigidbody rigidBody;

    CollisionFlags cf;
    float y = 0.0f;
    float x = 0.0f;
    float z = 0.0f;


    public void ResetView(Vector3 center_pos)
    {

        StartCoroutine(DoReset(center_pos));
    }

    IEnumerator DoReset(Vector3 center_pos)
    {

        float i = 0.0f;
        float rate = 1.5f;

        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            transform.position = Vector3.Lerp(transform.position, center_pos, i);
            yield return null;
        }

    }

    void Update()
    {

        x = 0.0f;
        y = 0.0f;
        z = 0.0f;

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) return;


        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            moveSpeed = normalMoveSpeed;
            
        }
        else
        {
            moveSpeed = slowMoveSpeed;
        }

        z = moveSpeed * Input.GetAxis("Vertical");
        x = moveSpeed * Input.GetAxis("Horizontal");

    
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Space)) { y += 1 * climbSpeed; }
        else if (Input.GetKey(KeyCode.E)) { y -= 1 * climbSpeed; }

        transform.Translate(transform.forward += (new Vector3(0, y, z)));

        //transform.position += ( y) * Time.deltaTime;

        //transform.RotateAround(transform.position, transform.up, Time.deltaTime * 90f);
        //transform.forward = OVRCam.transform.forward;
        //transform.Rotate(Vector3.up, Input.GetAxis("Horizontal") * 3 * Time.deltaTime);

    }
}
