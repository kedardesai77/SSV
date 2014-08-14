using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Highlight : MonoBehaviour {
	

    private List<String> selected_distincts = new List<string>();

	public Viewer viewer;

    private Dictionary<string, Color> distinct_vals = new Dictionary<string, Color>();
    public static bool show = false;

    public GUISkin highlight_skin;

    List<GameObject> highlighted = new List<GameObject>();

    Vector2 ScrollPos = new Vector2();
    private Vector2 savedStatesScrollBar = Vector2.zero;

	public string category = "";

    void Start()
    {
        ScrollPos = Vector2.zero;
        distinct_vals = new Dictionary<string, Color>();
    }

    public void PaintHighlighted(Color c)
    {
        for (int i = 0; i < highlighted.Count; i++)
        {
            GameObject go = highlighted[i];
            go.GetComponent<SSObject>().Set_Mat_Color(c, true);
        }
        highlighted.Clear();
    }

    public void Close()
    {
        viewer.model.DefaultView();
        show = false;  
    }

    private string save_name = "";
    void OnGUI()
    {
        if(show)
        {
            GUI.skin = highlight_skin;
			GUILayout.BeginArea(new Rect(10 , 35 , Screen.width - 20 , Screen.height - 70));

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(category)) Close();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Color All"))
                {
                    StartCoroutine(ColorAllByField(category));
                }
                if (GUILayout.Button("Clear All"))
                {
                    viewer.model.FadeAndGray();
                    selected_distincts.Clear();
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                int tmp = distinct_vals.Count * 28;
                if (tmp >= Screen.height)
                {
                    ScrollPos = GUILayout.BeginScrollView(ScrollPos);
                    GUILayout.Label("", GUILayout.Height(tmp) , GUILayout.Width(1));
                    GUILayout.EndScrollView();
                }

                GUILayout.BeginScrollView(ScrollPos, new GUIStyle(), new GUIStyle());


				for(int i = 0; i < keypairList.Count ; i++)
				{
					KeyValuePair<string,Color> distinct = keypairList[i];
					GUILayout.BeginHorizontal();
					
					//Color box
					Texture2D texture = new Texture2D(1, 1);
					texture.SetPixel(0, 0, distinct.Value);
					texture.Apply();
					GUI.skin.box.normal.background = texture;
					GUILayout.Box("", GUILayout.Width(20));
					
					if (selected_distincts.Contains(distinct.Key))
					{
						if (GUILayout.Button(distinct.Key, GUI.skin.GetStyle("selected_highlight")))
						{
							StartCoroutine(HighlightSearch(distinct.Key));
						}
					}
					else
					{
						if (GUILayout.Button(distinct.Key))
						{
							StartCoroutine(HighlightSearch(distinct.Key));
						}
					}
					
					GUILayout.EndHorizontal();
				}

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

			GUILayout.EndArea();





        }
    }
    

    public void DoHighlight(string category)
    {

        //Get unique items in category
        //with same dwg
        StartCoroutine(GetDistinctFieldVals(category));
       

    }

    public void ClearHighlight()
    {

        for (int i = 0; i < highlighted.Count; i++)
        {
            SSObject sso = highlighted[i].GetComponent<SSObject>();
            sso.Set_Mat_Color(Color.gray, true);
            sso.Fade();
        }
        highlighted.Clear();
    }

    private IEnumerator ColorAllByField(string field )
    {
        //Get search matching guids
        WWWForm form = new WWWForm();
        form.AddField("sa", "get_field");
        form.AddField("field", field);
        form.AddField("project", SafeSim.PROJECT);
        form.AddField("asset", SafeSim.ASSET.Substring(3));

        WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
        yield return www;

        if(www.error == null)
        {
            List<string> results = new List<string>();
            if (www.text.Length > 0)
            {
                results = new List<string>(www.text.Split('\n'));
                //NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", results.Count + " Results found."));
            }
                
			List<string> tr = new List<string>();
			foreach(string s in results)if(!tr.Contains(s))tr.Add(s);

            int cnt = 0;
            foreach (string result_guid in tr)
            {
                string guid = "";
                string val = "";

                try
                {
                    guid = result_guid.Substring(0, result_guid.IndexOf('|'));
                    val = result_guid.Substring(result_guid.IndexOf('|') + 1);
                }
                catch(Exception )
                {
                    continue;
                }

                if (viewer.model.GUIDs.ContainsKey(guid))
                {
                    if(distinct_vals.ContainsKey(val))
                    {
                        GameObject go = viewer.model.GUIDs[guid];
                        SSObject sso = go.GetComponent<SSObject>();
                        if (go.renderer.material.color == distinct_vals[val])
                        {
                            sso.Set_Mat_Color(Color.gray, true);
                            sso.Fade();
                            selected_distincts.Remove(val);
                        }
                        else
                        {
                            sso.UnFade();
                            sso.Set_Mat_Color(distinct_vals[val], true);
                            if(!selected_distincts.Contains(val))selected_distincts.Add(val);
                        }
                        cnt++;
                    }

                }
            }
        }
        else
        {
            Debug.LogError(www.error);
        }


    }

	private IEnumerator HighlightSearch(string distinct)
	{

        //ClearHighlight();

		//Get search matching guids
		WWWForm form = new WWWForm();
		form.AddField("sa", "search");
		form.AddField("category", category);
		form.AddField("searchString", distinct);
		form.AddField("project", SafeSim.PROJECT);
		
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
		yield return www;

        List<string> results = new List<string>();
        if(www.error == null && www.text.Length > 0)
        {
            results = new List<string>(www.text.Split('|'));
            //NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", results.Count + " Results found."));
        }

		List<string> tr = new List<string>();

		foreach(string s in results) if(!tr.Contains(s)) tr.Add(s);
		    

        int cnt = 0;
		foreach (string result_guid in tr)
		{
            
			if(viewer.model.GUIDs.ContainsKey(result_guid))
			{
                GameObject go = viewer.model.GUIDs[result_guid];
				SSObject guid = go.GetComponent<SSObject>();
				if(go.renderer.material.color == distinct_vals[distinct])
                {
                    guid.Set_Mat_Color(Color.gray, true);
                    guid.Fade();
                    selected_distincts.Remove(distinct);
                }
                else
                {
                    guid.UnFade();
                    guid.Set_Mat_Color(distinct_vals[distinct], true);
                    selected_distincts.Add(distinct);
                }
                cnt++;
			}
		}

        //if(cnt == 0)NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", "No Results found."));
	}

    IEnumerator GetDistinctFieldVals(string field)
    {
		category = field;
        distinct_vals = new Dictionary<string, Color>();
        keypairList = new List<KeyValuePair<string, Color>>();
        WWWForm form = new WWWForm();
        form.AddField("project", SafeSim.PROJECT);
        form.AddField("asset", SafeSim.ASSET.Substring(3));
        form.AddField("sa", "get_distinct_field");
        form.AddField("field", field );
        WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
        yield return www;
        if (www.error == null)
        {
            foreach (string s in www.text.Split('|'))
                if (s.Trim().Length > 0)
                    distinct_vals.Add(s, new Color(UnityEngine.Random.Range(0.00f, 1.0f), UnityEngine.Random.Range(0.0F, 1.0F), UnityEngine.Random.Range(0.0F, 1.0F)));

            show = true;

            keypairList.AddRange(distinct_vals);
            viewer.model.FadeAndGray();
        }
        else Debug.Log("WWW Error: " + www.error);

    }
    List<KeyValuePair<string, Color>> keypairList = new List<KeyValuePair<string, Color>>();
}
