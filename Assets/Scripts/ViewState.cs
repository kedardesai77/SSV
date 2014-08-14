using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ViewState : MonoBehaviour {


    public Viewer viewer;
	public ControlManager control_manager;
    bool show = false;

    string id = "";
    string name = "";

    Rect rect = new Rect(0, Screen.height - 200, Screen.width, 200);


    void OnGUI()
    {
        if (show)
        {
            if (GUI.Button(new Rect(5, 30, 60, 25), "Close"))
            {
                CLose();
            }
            if (GUI.Button(new Rect(5, 60, 60, 25), "Delete"))
            {
                StartCoroutine(DeleteState());
            }


            GUI.skin.label.fontSize = 28;
            GUI.Label(new Rect(25 , Screen.height - 150 ,Screen.width - 50 , 150 ) ,  name);
            GUI.skin.label.fontSize = 12;


        }
    }

    public IEnumerator DeleteState()
    {
        NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerON"));
        WWWForm form = new WWWForm();

        form.AddField("project", SafeSim.PROJECT);
        form.AddField("asset", SafeSim.ASSET.Substring(3));
        form.AddField("sa", "delete_color_state");
        form.AddField("state_id", id);
        WWW www = new WWW(SafeSim.URL + "save_handler.ashx", form);
        yield return www;
        if (www.error == null)
        {
            Debug.Log(www.text);
        }
        else Debug.Log("WWW Error: " + www.error);

        yield return StartCoroutine(Search.GetStates());
		CLose();
        NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerOFF"));

    }

    public void LoadStateById(string state_id , string state_name)
    {
		
		viewer.model.FadeAndGray();
        StartCoroutine(LoadColorStae(state_id));
        show = true;
        id = state_id;
        name = state_name;
    }



    public void CLose()
    {

        viewer.model.DefaultView();
        show = false;
        GetComponent<CreateState>().enabled = false;
        this.enabled = false;
    }

	public IEnumerator LoadColorStae(string state )
	{
		
		WWWForm form = new WWWForm();
		form.AddField("sa", "get_state");
		form.AddField("project", SafeSim.PROJECT);
		form.AddField("state", state);
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx" , form );
		yield return www;
		
		// check for errors
		if (www.error == null)
		{
			string[] str = www.text.Split('|');

			List<Vector3> centers = new List<Vector3>();
			Bounds groupBounds = new Bounds(Vector3.zero, Vector3.zero);

			for (int i = 0; i < str.Length - 1; i = i + 2)
			{
				string[] color_str = str[i + 1].Split(',');
				Color c = new Color(float.Parse(color_str[0]), float.Parse(color_str[1]), float.Parse(color_str[2]));
				GameObject go = viewer.model.GUIDs[str[i]];
				go.GetComponent<SSObject>().Set_Mat_Color(c, true);
				go.GetComponent<SSObject>().UnFade();

				centers.Add(new Vector3(go.renderer.bounds.center.x, go.renderer.bounds.center.y, go.renderer.bounds.center.z));
				groupBounds.Encapsulate(go.renderer.bounds);
			}

			control_manager.MovePivot(SSModel.AvgVectors(centers));
			//control_manager.Focus(groupBounds.size);
		}
		else
		{
			Debug.LogError("WWW Error: " + www.error);
		}


		
	}

}
