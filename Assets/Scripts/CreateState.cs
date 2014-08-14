using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CreateState : MonoBehaviour {
	
	
	public GUISkin stateSkin;
	
	public ColorPicker cp;
	public Highlight highlight;
	
	private Rect rect = new Rect(Screen.width - 200, Screen.height - 100, 190, 100);
	private Rect create = new Rect(Screen.width - 50, 30, 200, Screen.height - 60);
	private Rect select = new Rect(0, 30, 400, Screen.height - 30);
	// Use this for initialization
	
	public Viewer viewer;
	public Select selection;
	
	string state_name = "";
	string category_name = "";
	
	Dictionary<string, Color> categories = new Dictionary<string, Color>();
	private Dictionary<string, Color> distinct_vals = new Dictionary<string, Color>();
	
	List<Color> colors = new List<Color>();
	List<string> include = new List<string>();
	
	public Color custom_color1 = Color.black;
	public Color custom_color2 = Color.black;
	public Color custom_color3 = Color.black;
	public Color custom_color4 = Color.black;
	string active_category = "<Category>";
	
	Vector2 categoryScroll = new Vector2();
	Vector2 distinctScroll = new Vector2();
	
	void Start () 
	{
		colors = new List<Color>();
		
		//colors.Add(Color.clear);
		
		colors.Add(Color.red);
		colors.Add(new Color(0.502f, 0.0f, 0.0f));
		
		colors.Add(new Color(1 , 0.49f , 0.14f));
		colors.Add(new Color(1.0f, 0.627f, 0.478f));
		
		colors.Add(Color.green);
		colors.Add(Color.yellow);
		
		colors.Add(Color.magenta);
		colors.Add(new Color(0.75f, 0.25f, 1f));
		
		colors.Add(new Color(0.737f, 0.561f, 0.561f));
		colors.Add(new Color(0.196f, 0.804f, 0.196f));
		
		colors.Add(new Color(0.19f, 0.50f, 0.0784f));
		colors.Add(new Color(0.502f, 0.502f, 0.0f));
		
		colors.Add(new Color(0.400f, 0.804f, 0.667f));
		colors.Add(new Color(0.604f, 0.804f, 0.196f));
		
		
		
		colors.Add(Color.cyan);
		colors.Add(new Color(0.39f, 0.72f, 1f));
		
		
		
		
		
		include = new List<string>();
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
		
		
		cp.enabled = true;
		
	}
	
	public Texture2D CTT(Color c)
	{
		Texture2D tx = new Texture2D(1, 1);
		tx.SetPixel(0, 0, c);
		tx.Apply();
		return tx;
	}
	
	public bool c1 = false;
	public bool c2 = false;
	public bool c3 = false;
	public bool c4 = false;
	
	public void ColorSet(Color c)
	{
		if (c1) custom_color1 = c;
		else if (c2) custom_color2 = c;
		else if (c3) custom_color3 = c;
		else if (c4) custom_color4 = c;
		
		c1 = c2 = c3 = c4 = false;
		
		
	}
	void DrawColorPallette(List<Color> colors)
	{
		
		if (c1 || c2 || c3 || c4) selection.showRightClick = false;
		
		Texture2D tex = GUI.skin.button.normal.background;
		GUILayout.BeginHorizontal();
		//CUSTOM
		if (c1) custom_color1 = cp.GetColor();
		
		Texture2D txture = new Texture2D(1, 1);
		txture.SetPixel(0, 0, custom_color1);
		txture.Apply();
		GUI.skin.button.normal.background = txture;
		if (GUILayout.Button(txture, GUILayout.Width(20), GUILayout.Height(20)))
		{
			if (Event.current.button == 1)
			{
				//show color picker
				ColorPicker.openCondition = true;
				c1 = true;
			}
			else if(!c1)
			{
				foreach (GameObject go in selection.GetSelection())
				{
					go.GetComponent<SSObject>().Set_Mat_Color(custom_color1, true);
				}
			}
		}
		
		if (c2) custom_color2 = cp.GetColor();
		
		txture = new Texture2D(1, 1);
		txture.SetPixel(0, 0,  custom_color2);
		txture.Apply();
		GUI.skin.button.normal.background = txture;
		if (GUILayout.Button(txture, GUILayout.Width(20), GUILayout.Height(20)))
		{
			if (Event.current.button == 1)
			{
				//show color picker
				ColorPicker.openCondition = true;
				c2 = true;
			}
			else if(!c2)
			{
				foreach (GameObject go in selection.GetSelection())
				{
					go.GetComponent<SSObject>().Set_Mat_Color(custom_color2, true);
				}
			}
		}
		GUILayout.EndHorizontal();
		
		
		//SECOND ROW
		
		GUILayout.BeginHorizontal();
		//CUSTOM
		if (c3) custom_color3 = cp.GetColor();
		
		txture = new Texture2D(1, 1);
		txture.SetPixel(0, 0, custom_color3);
		txture.Apply();
		GUI.skin.button.normal.background = txture;
		if (GUILayout.Button(txture, GUILayout.Width(20), GUILayout.Height(20)))
		{
			if (Event.current.button == 1)
			{
				//show color picker
				ColorPicker.openCondition = true;
				c3 = true;
			}
			else if (!c3)
			{
				foreach (GameObject go in selection.GetSelection())
				{
					go.GetComponent<SSObject>().Set_Mat_Color(custom_color3, true);
				}
			}
		}
		
		if (c4) custom_color4 = cp.GetColor();
		
		txture = new Texture2D(1, 1);
		txture.SetPixel(0, 0, custom_color4);
		txture.Apply();
		GUI.skin.button.normal.background = txture;
		if (GUILayout.Button(txture, GUILayout.Width(20), GUILayout.Height(20)))
		{
			if (Event.current.button == 1)
			{
				//show color picker
				ColorPicker.openCondition = true;
				c4 = true;
			}
			else if (!c4)
			{
				foreach (GameObject go in selection.GetSelection())
				{
					go.GetComponent<SSObject>().Set_Mat_Color(custom_color4, true);
				}
			}
		}
		GUILayout.EndHorizontal();
		
		GUILayout.Space(6);
		
		
		
		//pre def colors
		for (int i = 0; i < colors.Count; i += 2)
		{
			GUILayout.BeginHorizontal();
			
			
			//Color box
			Texture2D texture = CTT(colors[i]);
			GUI.skin.button.normal.background = texture;
			if (GUILayout.Button(texture , GUILayout.Width(20) , GUILayout.Height(20)))
			{
				foreach (GameObject go in selection.GetSelection())
				{
					go.GetComponent<SSObject>().Set_Mat_Color(colors[i], true);
				}
			}
			
			texture = CTT(colors[i+1]);
			GUI.skin.button.normal.background = texture;
			if (GUILayout.Button(texture, GUILayout.Width(20), GUILayout.Height(20)))
			{
				
				foreach (GameObject go in selection.GetSelection())
				{
					go.GetComponent<SSObject>().Set_Mat_Color(colors[i + 1], true);
				}
			}
			
			GUILayout.EndHorizontal();
		}
		
		
		GUI.skin.button.normal.background = tex;
		
		
	}
	
	
	public void Close()
	{
		cp.enabled = false;
		viewer.model.DefaultView();
		GetComponent<ViewState>().enabled = false;
		this.enabled = false;
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
		create = new Rect(Screen.width - 50, 30, 200, Screen.height - 60);
		GUILayout.BeginArea(create);
		DrawColorPallette(colors);
		
		
		
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
			int cnt = 0;
			foreach (GameObject go in viewer.model.GUIDs.Values)
			{
				try
				{
					
					
					Color c = new Color(go.renderer.material.color.r, go.renderer.material.color.g, go.renderer.material.color.b, 1.0f);
					string color_str = c.r.ToString() + "," + c.g.ToString() + "," + c.b.ToString();
					if (color_str.Equals("0.5,0.5,0.5"))
					{
						continue;
					}
					str += go.name + "|" + color_str + "|";
					cnt++;
					
					
				}
				catch (Exception e)
				{
					continue;
				}
				
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
		yield return StartCoroutine(viewer.model.SaveColorState(state_name , str));
		yield return StartCoroutine(Search.GetStates());
		NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerOFF"));
		Close();
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
