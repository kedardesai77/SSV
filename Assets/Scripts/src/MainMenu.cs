using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {
	
	//State control
	bool loading = false;
	bool show_scenarios = false;
	bool show_scenario_editor = false;
	
	//Graphics
	public Texture2D fbxIcon;

	public GUISkin MainMenuSkin;
	public GUIStyle assetStyle;

	//Asset box
	Rect assetBox  = new Rect(5, 5 , Screen.width/3 - 5 , Screen.height - 100); 
	Rect assetsRect = new Rect(5, 5 , Screen.width/3 - 5 , Screen.height - 100);
	Vector2 assetScrollPos = new Vector2();
	GUIContent[] assetList = new GUIContent[0];
	int assetIndex = 0;
	int prevAssetIndex = 0;
	
	//Procedure box
	Rect procedureBox = new Rect(Screen.width /3 + 5 , 5 , Screen.width - Screen.width/3 - 10, Screen.height - 100);
	Rect procedureRect = new Rect(Screen.width /3 + 15 , 5 , Screen.width - Screen.width/3 -25, Screen.height - 100);
	Vector2 procedureScrollPos = new Vector2();
	List<ListItem> procedureList = new List<ListItem>();
	int procedureIndex = 0;

	void Start () 
	{
		if(Application.absoluteURL.Length > 0)
		{
			SafeSim.URL = Application.absoluteURL.Substring(0,Application.absoluteURL.IndexOf("viewer"));
            SafeSim.MODEL_URL = SafeSim.URL.Replace("UnitySafeSim", "UnityModels") ;
		}

        string srcValue = Application.srcValue;
        string model_name = "";


		Dictionary<string,string> queryParams = GetQueryParams(srcValue);

		try
		{
			SafeSim.USERNAME = queryParams["userName"];	
		}
		catch(Exception e)
		{
			SafeSim.USERNAME = "guest_user";
		}
		
		try
		{
			model_name = queryParams["model"];
		}
		catch(Exception)
		{
            model_name = "9007";
            //model_name = "2376-A062";
			//model_name = "9007/SS-ISPS1-305";
		}
		
		if(model_name.Contains("/"))
		{
			SafeSim.ASSET = model_name.Substring(model_name.IndexOf('/')+1);

            if (PressurePropogation.use_contact_points)
            {
                SafeSim.PROJECT = model_name.Substring(0, model_name.IndexOf('/'));
            }
            else
            {
                SafeSim.PROJECT = model_name.Substring(0, model_name.IndexOf('/')).Replace('-', '_');
            }
			
			SafeSim.CURRRENT_MODE = SafeSim.Modes.viewer;
			Application.LoadLevel("viewer");
		}
		else
		{
            if (PressurePropogation.use_contact_points)
            {
                SafeSim.PROJECT = model_name;
            }
            else
            {
                SafeSim.PROJECT = model_name.Replace('-', '_');
            }
			StartCoroutine(GetAssetDir());

            Application.ExternalCall("SetTheSSFileName", "SafeSim");
		}
	}
	
	public static Dictionary<string , string> GetQueryParams(string srcValue)
	{
		Dictionary<string,string> queryParams = new Dictionary<string, string>();
		try
		{
			srcValue = srcValue.Substring(srcValue.IndexOf('?')+1);	
			string[] temp = srcValue.Split('&');
			
			foreach(string s in temp)
			{
				if(s.Length == 0)continue;
				string[] tmp = s.Split('=');
				Debug.Log(tmp[0]);
				queryParams.Add(tmp[0],tmp[1]);
				Debug.Log(queryParams[tmp[0]]);
			}
			
		}
		catch(Exception e)
		{
			
		}
		return queryParams;
	}

	void Update()
	{
		if(assetIndex != prevAssetIndex)
		{
			SafeSim.ASSET = assetList[assetIndex].text;
			if(show_scenarios)
			{
				//StartCoroutine(GetScenarios(SafeSim.ASSET));			
			}
			else
			{
				StartCoroutine(GetProcedures(SafeSim.ASSET));
			}
			prevAssetIndex = assetIndex;
		}
	}


	//GUI METHODS
	void LoadingScreen()
	{
		GUI.skin.label.normal.textColor = Color.black;
		GUI.skin.label.fontSize = 18;
		GUI.Label(new Rect(Screen.width /2 - 10, Screen.height /2 , 100, 30), "Loading...");
		GUI.skin.label.normal.textColor = Color.white;
		GUI.skin.label.fontSize = 12;
	}
	void AssetBox()
	{
		GUI.Box(new Rect(5, 5 , Screen.width/3 - 5 , Screen.height - 100), "");
		GUILayout.BeginArea(new Rect(10, 5 , Screen.width/3 - 15 , Screen.height - 100));
		GUILayout.Label("Assets" );
		assetScrollPos = GUILayout.BeginScrollView(assetScrollPos);
		assetIndex = GUILayout.SelectionGrid(assetIndex , assetList ,1 );
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}
	void ProcedureBox()
	{
		GUI.Box(new Rect(Screen.width /3 + 5 , 5 , Screen.width - Screen.width/3 - 10, Screen.height - 100), "");
		GUILayout.BeginArea(new Rect(Screen.width /3 + 15 , 5 , Screen.width - Screen.width/3 -25, Screen.height - 100));
		GUILayout.Label("Procedures" );
		procedureScrollPos = GUILayout.BeginScrollView(procedureScrollPos);

		List<GUIContent> temp = new List<GUIContent>();
		foreach(ListItem li in procedureList)
		{
			temp.Add (new GUIContent(li.name));	
		}				
		procedureIndex = GUILayout.SelectionGrid(procedureIndex  , temp.ToArray() ,1);
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}
	void ModesBox()
	{
		GUI.Box(new Rect(Screen.width /3 + 5, Screen.height - 90 , Screen.width - Screen.width /3 - 10, 80) , "");
		GUILayout.BeginArea(new Rect(Screen.width /3 + 15, Screen.height - 60 , Screen.width - Screen.width /3 - 25, 80) , "");

		GUILayout.BeginHorizontal();

		if(GUILayout.Button("Add"))
		{
			if(show_scenarios)
			{
				//NewScenario();
				show_scenario_editor = true;
			}
			else
			{
				loading = true;
				SafeSim.CURRRENT_MODE = SafeSim.Modes.record;

                SafeSim.PROCEDURE = new Procedure(-1, "<Enter Name>", "<Enter Description>");
                Application.LoadLevel("procedure_record");
			}                                                       
		}			
		else if(GUILayout.Button("Edit"))
		{
            loading = true;
            SafeSim.CURRRENT_MODE = SafeSim.Modes.edit;
            ListItem li = procedureList[procedureIndex];
            SafeSim.PROCEDURE = new Procedure(li.id, li.name, li.description);
            Application.LoadLevel("procedure_edit");
		}
        else if (GUILayout.Button("Delete"))
        {
            ListItem li = procedureList[procedureIndex];
            SafeSim.PROCEDURE = new Procedure(li.id, li.name, li.description);
            StartCoroutine(DeleteProcedure());
            
        }			
		else if(GUILayout.Button("Train") && procedureList.Count > 0)
		{
			loading = true;
			SafeSim.CURRRENT_MODE = SafeSim.Modes.demo;
            ListItem li = procedureList[procedureIndex];
            SafeSim.PROCEDURE = new Procedure(li.id, li.name, li.description);
            Application.LoadLevel("procedure_demo");
		
		}
		else if(GUILayout.Button("Teach"))
		{
			
		}
		if(GUILayout.Button("Test") && procedureList.Count > 0)
		{
			loading = true;
			SafeSim.CURRRENT_MODE = SafeSim.Modes.test;
            ListItem li = procedureList[procedureIndex];
            SafeSim.PROCEDURE = new Procedure(li.id , li.name , li.description );
            Application.LoadLevel("procedure_test");	
		}
		
		
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();
	}
	void ViewerBox()
	{
		GUI.Box(new Rect(5, Screen.height - 90 , Screen.width/3 - 5 , 80) , "");
		GUILayout.BeginArea(new Rect(15, Screen.height - 70 , Screen.width/3 - 5 , 70));
        /*
		if(GUILayout.Button("2D Demo" , GUILayout.Width(Screen.width /3 - 25) ))
		{
			Application.LoadLevel("viewer2d");
		}
        */
		if(GUILayout.Button("Open in viewer" , GUILayout.Width(Screen.width /3 - 25) ))
		{
			loading = true;
			SafeSim.CURRRENT_MODE = SafeSim.Modes.viewer;
			Application.LoadLevel("viewer");

		}
        GUILayout.FlexibleSpace();
		GUILayout.EndArea();
	}

	void OnGUI()
	{

		if (loading)	//Loading Screen
		{
			LoadingScreen();
		}
		else
		{
			GUI.skin = MainMenuSkin;

			AssetBox();

			ProcedureBox();

			GUI.skin.button.alignment = TextAnchor.MiddleCenter;
			ViewerBox();
			GUI.skin.button.alignment = TextAnchor.MiddleLeft;

			ModesBox();

		}
	}

	//Back end
	IEnumerator GetAssetDir()
	{
		List<GUIContent> temp = new List<GUIContent>();
		WWWForm form = new WWWForm();
		form.AddField("sa" , "get_assets");
		form.AddField("project" , SafeSim.PROJECT.Replace("PID" , ""));
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx" , form );
		yield return www;

		if (www.error == null)
		{
			string[] fileNames = www.text.Split('|');
			foreach(string filename in fileNames)
			{
				if(filename.Length > 0)
					temp.Add(new GUIContent(filename , fbxIcon ));
			}
			assetList = temp.ToArray();
			SafeSim.ASSET = assetList[0].text;
			StartCoroutine(GetProcedures(assetList[0].text));
		} 
		else 
		{
			Debug.Log("WWW Error: "+ www.error);
		}
	}
	IEnumerator GetProcedures(string asset)
	{
		procedureList.Clear();
		WWWForm form = new WWWForm();
		form.AddField("sa" , "get_procedures");
		form.AddField("project", (SafeSim.PROJECT.Replace("PID" ,"") + "/" + asset)); 
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx" , form);
		yield return www;
		
		if (www.error == null)
		{
			string[] procedures = www.text.Split('\n');
			foreach(string procedure in procedures)
			{
				if(procedure.Length > 0)
				{
					string[] elements = procedure.Split('|');
					procedureList.Add(new ListItem(int.Parse (elements[0]) , elements[1] , elements[2]));
				}
			}
		} 
		else 
		{
			Debug.Log("WWW Error: "+ www.error + "\n" + www.url);
		}
		show_scenarios = false;
	}
    IEnumerator DeleteProcedure()
    {
        yield return StartCoroutine(SafeSim.PROCEDURE.Delete());
        StartCoroutine(GetProcedures(SafeSim.ASSET));
    }

	//Helper class
	//sores database's procedure id nad procedure name
	// locl list index is used for menu selection
	private class ListItem
	{
		public int id;
		public string name;
        public string description;
        public ListItem(int id, string name , string description)
        {
            this.id = id;
            this.name = name;
            this.description = description;
        }
	}


}
