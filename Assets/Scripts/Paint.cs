using UnityEngine;
using System.Collections;

public class Paint : MonoBehaviour {


    public ColorPicker color_picker;
    public DrawGUIOrderColorPicker dcp;

    public Select selection;

	// Use this for initialization
	void Start () 
    {
        Hide();
	}


    public void Show()
    {
        color_picker.enabled = true;
        dcp.enabled = true;
    }

    public void Hide()
    {
        color_picker.enabled = false;
        dcp.enabled = false;
    }
	

    public void OnGUI()
    {
        if(color_picker.enabled)
        {
            GUILayout.BeginArea(new Rect(550, 30, 100, 40));
            if (GUILayout.Button("Paint Selected"))
            {
                foreach(GameObject go in selection.GetSelection())
                {
                    go.GetComponent<SSObject>().Set_Mat_Color(color_picker.GetColor(), true);
                }
            }
            GUILayout.EndArea();
        }

    }

	// Update is called once per frame
	void Update () 
    {
	    
	}

    public void Toggle()
    {
        if(color_picker.enabled)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }
}
