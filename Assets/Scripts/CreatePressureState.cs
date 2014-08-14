using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CreatePressureState : MonoBehaviour {

	class ValveState
	{
		public string name;
		public Color color;
		public ValveState(string n , Color c)
		{
			name = n; color = c;
		}
	}
	
	public GUISkin stateSkin;

	public Highlight highlight;

	public Dictionary<string , string> operStateData = new Dictionary<string , string>();
	
	private Rect rect = new Rect(Screen.width - 200, Screen.height - 100, 190, 100);
	private Rect create = new Rect(Screen.width - 100, 30, 90, Screen.height - 60);
	private Rect select = new Rect(0, 30, 400, Screen.height - 30);
	// Use this for initialization
	
	public Viewer viewer;
	public Select selection;
	
	string state_name = "";
	string category_name = "";
	
	Dictionary<string, Color> categories = new Dictionary<string, Color>();
	private Dictionary<string, Color> distinct_vals = new Dictionary<string, Color>();
	
	List<ValveState> valveStates = new List<ValveState>();
	List<string> include = new List<string>();
	Color active_color;
	string active_category = "<Category>";
	
	Vector2 categoryScroll = new Vector2();
	Vector2 distinctScroll = new Vector2();
	
	void Start () 
	{
		valveStates = new List<ValveState>();
		
		//colors.Add(Color.clear);

		valveStates.Add(new ValveState("Open" , Color.green));
		valveStates.Add(new ValveState("Close" , Color.red));
		valveStates.Add(new ValveState("Start" , Color.magenta));

		List<string> include = new List<string>();
		include.Add("SIZE_");
		include.Add("TAG_");
		include.Add("SHORT_DESC_");
		include.Add("LONG_DESC_");
		include.Add("SPEC_");
		include.Add("LINE_NUMBER_");
		include.Add("DB_CODE_");
		include.Add("CATEGORY");
		include.Add("TYPE_");
		include.Add("BOM_TYPE");
		include.Add("DWG_NAME");
		include.Add("DESIGNPSI_");
		include.Add("OPERPSI_");
		include.Add("SPECPRESS_");
		include.Add("OPERDEG_");
		include.Add("DESIGNDEG_");
		include.Add("MINTEMP_");
		include.Add("MAXTEMP_");
		include.Add("MINPSI_");
		include.Add("MAXPSI_");



	}
    private string UppercaseFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }
        else
        {
            s = s.ToLower();
            return char.ToUpper(s[0]) + s.Substring(1).Replace('_', ' ').Trim();
        }

    }
	void DrawColorPallette(List<ValveState> colors)
	{
		Texture2D tex = GUI.skin.button.normal.background;

		for (int i = 0; i < colors.Count-1; i++)
		{
			//Color box
			Texture2D texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, colors[i].color);
			texture.Apply();
			GUI.skin.button.normal.background = texture;
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(texture , GUILayout.Width(20) , GUILayout.Height(20)))
			{
				active_color = colors[i].color;
				foreach (GameObject go in selection.GetSelection())
				{
					go.GetComponent<SSObject>().Set_Mat_Color(active_color, true);
                    if (operStateData.ContainsKey(go.name))
                        operStateData[go.name] = colors[i].name;
                    else operStateData.Add(go.name, colors[i].name);
				}

			}
			GUILayout.Label(colors[i].name);
			GUILayout.EndHorizontal();
		}


		Texture2D tx = new Texture2D(1, 1);
		tx.SetPixel(0, 0, Color.magenta);
		tx.Apply();
		GUI.skin.button.normal.background = tx;
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(tx , GUILayout.Width(20) , GUILayout.Height(20)))
		{
			
			GameObject go = selection.GetSelectedGUID();
            if (go == null)
            {
                GUI.skin.button.normal.background = tex;
                return;
            }
            active_color = colors[2].color;
			go.GetComponent<SSObject>().Set_Mat_Color(active_color, true);


			List<string> keys = new List<string>(operStateData.Keys);
			for(int i = 0 ; i < keys.Count ; i++)
			{
				string s = operStateData[keys[i]];
				if(s.Equals("START"))
				{

					SSObject  sso = viewer.model.GUIDs[keys[i]].GetComponent<SSObject>();

					sso.Set_Mat_Color(Color.gray , true);
					sso.Fade();

					operStateData.Remove(keys[i]);

				}
			}

			operStateData.Add(selection.GetSelectedGUID().name , "START");

			
		}
		GUILayout.Label("START");
		GUILayout.EndHorizontal();

		GUI.skin.button.normal.background = tex;
	}
	
	
	public void Close()
	{
		viewer.model.DefaultView();
		operStateData.Clear();
		this.enabled = false;
	}
	
	
	void OnGUI()
	{
		
		GUI.skin = stateSkin;
		
		GUILayout.BeginArea(select);
		if (active_category.Equals("<Category>"))
		{
			if (GUILayout.Button("Close"))
			{
				Close();
			}
			for (int i = 0; i < include.Count; i++)
                if (GUILayout.Button(UppercaseFirst(include[i])))
			{
				StartCoroutine(GetDistinctFieldVals(include[i]));
				active_category = include[i];
			}
			
		}
		else
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Change Category"))
			{
				active_category = "<Category>";
			}
			if (GUILayout.Button("Clear"))
			{
				operStateData .Clear();
				viewer.model.FadeAndGray();
			}
			if (GUILayout.Button("Close"))
			{
				Close();
			}
			GUILayout.EndHorizontal();
			
			GUILayout.Space(10);
			
			GUILayout.BeginHorizontal();
			int tmp = distinct_vals.Count * 28;
			if (tmp >= Screen.width)
			{
				distinctScroll = GUILayout.BeginScrollView(distinctScroll);
				GUILayout.Label("", GUILayout.Height(tmp));
				GUILayout.EndScrollView();
			}
			
			GUILayout.BeginScrollView(distinctScroll, new GUIStyle(), new GUIStyle());
			List<KeyValuePair<string , Color>> kvpList = new List<KeyValuePair<string, Color>>();
			kvpList.AddRange(distinct_vals);
			for(int i = 0; i < kvpList.Count ; i++)
			{
				KeyValuePair<string, Color> kvp = kvpList[i];
				if (GUILayout.Button(kvp.Key))
				{
					StartCoroutine(AddSelection(kvp.Key));
				}
			}
			GUILayout.EndScrollView();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		GUILayout.EndArea();
		create = new Rect(Screen.width - 75, 30, 70, Screen.height - 60);
		GUILayout.BeginArea(create);
		DrawColorPallette(valveStates);
		
		
		
		/*
        GUILayout.BeginHorizontal();
        GUILayout.Label("Categories: ");
        GUILayout.EndHorizontal();

        foreach(KeyValuePair<string , Color> kvp in categories)
        {
            GUILayout.BeginHorizontal();

             //Color box
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0 , kvp.Value);
            texture.Apply();
            GUI.skin.box.normal.background = texture;
            GUILayout.Box("", GUILayout.Width(20));

            if (GUILayout.Button(kvp.Key))
            {
                categories.Remove(kvp.Key);
                return;
            }

            GUILayout.EndHorizontal();

        }

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        //Color box
        Texture2D texture2 = new Texture2D(1, 1);
        texture2.SetPixel(0, 0, cp.GetColor());
        texture2.Apply();
        GUI.skin.box.normal.background = texture2;
        GUILayout.Box("", GUILayout.Width(20));
        category_name = GUILayout.TextField(category_name);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Add New Category"))
        {
            if(categories.ContainsKey(category_name))
            {
                categories[category_name] = cp.GetColor();
            }
            else categories.Add(category_name, cp.GetColor());
        }
        */
		GUILayout.EndArea();
		rect = new Rect(Screen.width - 200, Screen.height - 100, 190, 100);
		GUILayout.BeginArea(rect);
		GUILayout.BeginVertical();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Name: ");
		state_name = GUILayout.TextField(state_name , GUILayout.Width(140));
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace();



		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.skin = null;
		if (GUILayout.Button("Save" , GUILayout.Width(70) , GUILayout.Height(40)))
		{
			string str = "";
			foreach(KeyValuePair<string,string> kvp in operStateData)
			{
				str += kvp.Key + "|" + kvp.Value + "|" ;
			}
            for (int i = 0; i < PressurePropogation.OPEN_GUIDS.Count; i++)
            {
                string g = PressurePropogation.OPEN_GUIDS[i].name;
                if (operStateData.ContainsKey(g)) continue;
                str += g + "|OPEN|";
            }
            for (int i = 0; i < PressurePropogation.CLOSED_GUIDS.Count; i++)
            {
                string g = PressurePropogation.CLOSED_GUIDS[i].name;
                if (operStateData.ContainsKey(g)) continue;
                str += g + "|CLOSE|";
            }
            for (int i = 0; i < PressurePropogation.HOLD_GUIDS.Count; i++)
            {
                string g = PressurePropogation.HOLD_GUIDS[i].name;
                if (operStateData.ContainsKey(g)) continue;
                str += g + "|CLOSE|";
            }
            for (int i = 0; i < PressurePropogation.CHECK_GUIDS.Count; i++)
            {
                string g = PressurePropogation.CHECK_GUIDS[i].name;
                if (operStateData.ContainsKey(g)) continue;
                str += PressurePropogation.CHECK_GUIDS[i].name + "|CHECK|";
            }
			StartCoroutine(SaveState(str));

		}
		GUI.skin = stateSkin;
		GUILayout.EndHorizontal();
		
		GUILayout.Space(10);
		GUILayout.EndVertical();
		GUILayout.EndArea();
		
		
	}
	
	private IEnumerator SaveState(string str)
	{
		NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerON"));
		yield return StartCoroutine(SavePressureState(state_name , str));
		yield return StartCoroutine(ViewPressureState.GetPressureStates());
		NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerOFF"));

		Close();
	}

	public IEnumerator SavePressureState(string name , string str)
	{
		
		
		WWWForm form = new WWWForm();
		form.AddField("sa", "save_operation_state");
		form.AddField("project", SafeSim.PROJECT);
		form.AddField("asset", SafeSim.ASSET.Substring(3));
		form.AddField("state_name", name);
		form.AddField("data", str);
		
		WWW www = new WWW(SafeSim.URL + "save_handler.ashx" , form);
		yield return www;
		
		if(www.error == null)
		{
			SafeSim.prompt = false;
		}
		else
		{
			Debug.Log(www.error);
		}
		
		
	}

	
	IEnumerator GetDistinctFieldVals(string field)
	{
		
		distinct_vals = new Dictionary<string, Color>();
		WWWForm form = new WWWForm();
		form.AddField("project", SafeSim.PROJECT);
		form.AddField("asset", SafeSim.ASSET.Substring(3));
		form.AddField("sa", "get_distinct_field");
		form.AddField("field", field);
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
		yield return www;
		if (www.error == null)
		{
			foreach (string s in www.text.Split('|'))
				if (s.Trim().Length > 0)
					distinct_vals.Add(s, new Color(UnityEngine.Random.Range(0.00f, 1.0f), UnityEngine.Random.Range(0.0F, 1.0F), UnityEngine.Random.Range(0.0F, 1.0F)));
			
			//show = true;
			//viewer.model.FadeAndGray();
		}
		else Debug.Log("WWW Error: " + www.error);
		
	}
	
	private IEnumerator AddSelection(string distinct)
	{
		
		//ClearHighlight();
		
		//Get search matching guids
		WWWForm form = new WWWForm();
		form.AddField("sa", "search");
		form.AddField("category", active_category);
		form.AddField("searchString", distinct);
		form.AddField("project", SafeSim.PROJECT);
		
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
		yield return www;
		
		List<string> results = new List<string>();
		if (www.text.Length > 0)
		{
			results = new List<string>(www.text.Split('|'));
			selection.SetSelection(results);
		}
		
		//if(cnt == 0)NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", "No Results found."));
	}
}
