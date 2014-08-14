using UnityEngine;
using System.Collections;

public class ErrorWindow : MonoBehaviour {


    bool show = false;
    string text = "";


	// Use this for initialization
	void Start () 
    {
        NotificationCenter.DefaultCenter().AddObserver(this, "ErrorMessage");
	}
	
	// Update is called once per frame
	void OnGUI () 
    {
        if(show)
        {
            float x = Screen.width / 4;
            Rect box = new Rect( x , Screen.height/4 , 2 * x , 100 );
            GUI.Box(box, "");
            GUILayout.BeginArea(box);
            GUILayout.BeginVertical();
            GUILayout.Label(text);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("OK" , GUILayout.Width(70)))
            {
                text = "";
                show = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();

        }
	}

    public void ErrorMessage(Notification n)
    {
        text = n.data.ToString();
        show = true;
    }
}
