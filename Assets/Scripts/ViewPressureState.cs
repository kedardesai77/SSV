using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ViewPressureState : MonoBehaviour {

	
	public Viewer viewer;
	public ControlManager control_manager;
	public PressurePropogation pressurePropogation;

	bool show = false;
	
	string id = "";
	string name = "";

	public static Dictionary<string , string> pressureStates = new Dictionary<string, string>();
	
	Rect rect = new Rect(0, Screen.height - 200, Screen.width, 200);
	
	void Start()
	{
		pressurePropogation = GetComponent<PressurePropogation>();
	}
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
	public static IEnumerator GetPressureStates()
	{
		NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerON"));
		
		pressureStates.Clear();
		//categories = new List<string>();
		WWWForm form = new WWWForm();
		form.AddField("sa", "get_operation_states");
		form.AddField("project", SafeSim.PROJECT);
		form.AddField("asset", SafeSim.ASSET.Substring(3));
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
		yield return www;
		
		if (www.error == null)
		{
			string[] str = www.text.Split('|');
			for (int i = 0; i < str.Length - 1; i += 2)
			{
				pressureStates.Add(str[i], str[i + 1]);
			}
			
		}
		else
		{
			Debug.Log("WWW Error: " + www.error);
			
		}
		
		NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerOFF"));

	}
	public IEnumerator DeleteState()
	{
		NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerON"));
		WWWForm form = new WWWForm();
		
		form.AddField("project", SafeSim.PROJECT);
		form.AddField("asset", SafeSim.ASSET.Substring(3));
		form.AddField("sa", "delete_operation_state");
		form.AddField("state_id", id);
		WWW www = new WWW(SafeSim.URL + "save_handler.ashx", form);
		yield return www;
		if (www.error == null)
		{
			Debug.Log(www.text);
		}
		else Debug.Log("WWW Error: " + www.error);
		
		yield return StartCoroutine(GetPressureStates());
		CLose();
		NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerOFF"));
		
	}
	
	public void LoadStateById(string state_id , string state_name)
	{
		pressurePropogation.Stop();
		viewer.model.FadeAndGray();
		StartCoroutine(LoadColorStae(state_id));
		show = true;
		id = state_id;
		name = state_name;
	}
	
	
	
	public void CLose()
	{
		//pressurePropogation.Stop();
		PressurePropogation.showWindow = false;

		show = false;
		iTween.Stop();
		viewer.model.DefaultView();
		GetComponent<CreatePressureState>().enabled = false;

		this.enabled = false;

	}
	
	public IEnumerator LoadColorStae(string state )
	{
		
		WWWForm form = new WWWForm();
		form.AddField("sa", "get_operation_state");
		form.AddField("project", SafeSim.PROJECT);
		form.AddField("state", state);
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx" , form );
		yield return www;
		
		// check for errors
		if (www.error == null)
		{
			string[] str = www.text.Split('|');
			
			//List<Vector3> centers = new List<Vector3>();
			//Bounds groupBounds = new Bounds(Vector3.zero, Vector3.zero);

			GameObject start = null;
			for (int i = 0; i < str.Length - 1; i = i + 2)
			{
				string st = str[i + 1].ToUpper();
				GameObject go = viewer.model.GUIDs[str[i]];
			

				switch (st)
				{
				case "OPEN":
					go.GetComponent<SSObject>().Set_Mat_Color(Color.green, true);
					go.GetComponent<SSObject>().UnFade();
					PressurePropogation.OPEN_GUIDS.Add(go);
					break;
				case "CLOSE":
					go.GetComponent<SSObject>().Set_Mat_Color(Color.red, true);
					go.GetComponent<SSObject>().UnFade();
					PressurePropogation.CLOSED_GUIDS.Add(go);
					break;
                case "CHECK":
                    go.GetComponent<SSObject>().Set_Mat_Color(Color.yellow, true);
                    go.GetComponent<SSObject>().UnFade();
                    PressurePropogation.CHECK_GUIDS.Add(go);
                    break;
				case "START":
					go.GetComponent<SSObject>().Set_Mat_Color(Color.magenta, true);
					go.GetComponent<SSObject>().UnFade();
					start = go;
					break;
				}


				
				//centers.Add(new Vector3(go.renderer.bounds.center.x, go.renderer.bounds.center.y, go.renderer.bounds.center.z));
				//groupBounds.Encapsulate(go.renderer.bounds);
			}
			if(start != null)
			{
				pressurePropogation.Stop();
				PressurePropogation.showWindow = true;
				pressurePropogation.StartPropogation(start);
			}
			//control_manager.MovePivot(SSModel.AvgVectors(centers));
			//control_manager.Focus(groupBounds.size);
		}
		else
		{
			Debug.LogError("WWW Error: " + www.error);
		}
		
		
		
	}
}
