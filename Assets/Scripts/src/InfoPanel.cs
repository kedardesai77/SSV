using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfoPanel : MonoBehaviour 
{
	
	private Dictionary<string, string> data = new Dictionary<string, string>();
	
	private bool showPanel;
	private bool edit ;
	private bool insert;
	
	private Vector2 scrollPosition;
	private Vector3 default_position;
	private Rect info_box_rect;
	
	public GUIStyle valueStyle = new GUIStyle();
	public GUIStyle attributeStyle = new GUIStyle();
	
	private int width , height;
	
	// Use this for initialization
	void Start () 
	{

		showPanel 	= false;
		
		width = 300;
		height = 273;
		
		scrollPosition 		= new Vector2(0,0);
		default_position 	= new Vector3(0,23,1);
		
		data 		= new Dictionary<string, string>();
		
		gameObject.transform.position = default_position;
		
		attributeStyle = new GUIStyle(valueStyle);
		attributeStyle.fixedWidth = 100;		
		attributeStyle.padding.right = 5;
		info_box_rect = new Rect(0,23,width,height);
		
	}
	
	public void HidePanel()
	{
		showPanel = false;
	}
	
	private void PanelWindow(int winId)
	{		
	
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(width), GUILayout.Height(height));
		GUILayout.BeginVertical();		
			foreach(string key in data.Keys)
			{
				if(key.Equals("GUID"))
				{
					continue;
				}
				else
				{
					GUILayout.BeginHorizontal();
						GUILayout.Label(key.Replace('_',' ').Trim() , attributeStyle);
						GUILayout.Label(data[key] , valueStyle);
					GUILayout.EndHorizontal();
				}
			}
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("Close"))
		{
			showPanel = false ;
		}
		GUILayout.EndHorizontal();
		GUI.DragWindow(new Rect(0,0,1000,20));
	}
	
	void OnGUI()
	{
		if(showPanel)
		{
			//info_box_rect = new Rect(transform.position.x,transform.position.y,500,500);
			info_box_rect = GUILayout.Window(0, info_box_rect, PanelWindow, GetComponent<Select>().GetSelectedGUID().name);

		}
		
	}
	
	public void SetData(string guid)
	{
		WWWForm form = new WWWForm();
		form.AddField("sa" , "get_data");
		form.AddField("guid" , guid);
        
		form.AddField("project" , SafeSim.PROJECT );
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx" , form);
		//StartCoroutine(WaitForRequest(www));
	}
	
	public void ClearData()
	{
		showPanel = false;
		data.Clear();
        
	}
	
    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
           string[] str = www.text.Split('|');
			for ( int i = 0 ; i < str.Length - 1 ; i = i + 2)
			{
				data.Add(str[i] , str[i+1]);
			}
        } 
		else 
		{ 
            Debug.Log("WWW Error: "+ www.error);
        }
		showPanel = true;
    }	
	
}
