using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;
using System;
public class PressurePropogation : MonoBehaviour {
	
	bool started = false;

	bool run = true;

    VectorLine vLine;
	PPTree ptree;

    class PPGuid
    {
        string guid;
        Vector3[] contactPoints;
        string lineNumber;

        string operPsi;
        string designpsi;
        string designdeg;
        string minpsi;
    }

	public static Dictionary<string,string> guid_line = new Dictionary<string, string>();
    public static Dictionary<string, string> guid_contact_points = new Dictionary<string, string>();
	public static Dictionary<string, string> guid_program_codes = new Dictionary<string, string>();
    
	public static string current_line = "";

    public static List<GameObject> HOLD_GUIDS = new List<GameObject>();
    public static List<GameObject> OPEN_GUIDS = new List<GameObject>();
    public static List<GameObject> CLOSED_GUIDS = new List<GameObject>();
    public static List<GameObject> CHECK_GUIDS = new List<GameObject>();
    public static List<PPNode> ACTIVE_GUIDS = new List<PPNode>();
    public static List<Vector3> LINE_POINTS = new List<Vector3>();

    public static Boolean use_contact_points = true;
    public static Boolean got_data = false;

	//Select
	public Select selection;

    public Viewer viewer;

	//GUI
	Rect pp_window_rect = new Rect( Screen.width - 150 , 25 , 170 , Screen.height - 25 );
	public static bool showWindow = false;
	public GUISkin PP_SKIN;
    public GUIStyle tooltip_style;
	public Material line_material;

    GUIContent[] TMPPSI = new GUIContent[2];
    GUIContent[] tempFields = new GUIContent[4];

    bool vectorline = false;
    private IEnumerator ResetNode(GameObject go, Color c)
    {
        iTween.ColorTo(go, c, 0.50f);
        yield return new WaitForSeconds(0.50f);
        go.GetComponent<SSObject>().Set_Mat_Color(c, true);
        go.GetComponent<HighlightableObject>().ConstantOff();
    }

    public void Start()
    {

        NotificationCenter.DefaultCenter().AddObserver(this, "OnModelLoaded");
        line_material = (Material)Resources.Load("LineMaterial");



        TMPPSI[0] = new GUIContent("Temperature");
        TMPPSI[1] = new GUIContent("Pressure");

        tempFields[0] = new GUIContent("Design");
        tempFields[1] = new GUIContent("Oper");
        tempFields[2] = new GUIContent("MIN");
        tempFields[3] = new GUIContent("MAX");

    }
    public void OnModelLoaded(Notification n)
    {
        StartCoroutine(GetData());
    }
    public static IEnumerator GetData()
    {
        NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerON"));

        if (guid_line.Count > 0 && got_data == true)
        {
            yield return null;
        }
        else
        {
            WWWForm form = new WWWForm();
            form.AddField("sa", "get_line");
            form.AddField("project", SafeSim.PROJECT);
            WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
            yield return www;

            if (www.error == null)
            {
                guid_line.Clear();
                string[] str = www.text.Split('\n');
                for (int i = 0; i < str.Length; i++)
                {
                    string[] row = str[i].Split('|');
                    if (row.Length == 2 && !guid_line.ContainsKey(row[0]))
                        guid_line.Add(row[0], row[1]);
                }
            }
            else Debug.Log("WWW Error: " + www.error);

            if (use_contact_points)
            {
                form = new WWWForm();
                form.AddField("sa", "get_contact_points");
                form.AddField("project", SafeSim.PROJECT);
                www = new WWW(SafeSim.URL + "get_handler.ashx", form);
                yield return www;

                // check for errors
                if (www.error == null)
                {
                    guid_contact_points.Clear();
                    guid_program_codes.Clear();
                    string[] str = www.text.Split('|');
                    for (int i = 0; i < str.Length - 1; i += 2)
                    {
                        string guid = str[i];
                        string cp = str[i + 1];
                        string pc = "";

                        if (cp.Contains("(") && cp.Contains(")"))
                            pc = cp.Substring(cp.IndexOf("("), cp.IndexOf(")") + 1);

                        if (pc.Length > 0) cp = cp.Replace(pc, "");

                        guid_contact_points.Add(guid, cp);
                        guid_program_codes.Add(guid, pc);
                    }

                }
                else Debug.Log("WWW Error: " + www.error);
            }
            got_data = true;
        }

        NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerOFF"));
    }
	


	public void StartPropogation(GameObject start)
	{
		if(started)return;

		started = true;

        if(use_contact_points)StartCoroutine(SaveContactPoints(start));
        else StartCoroutine(SaveLineNumbers(start));
        
	}
	public void Stop()
	{
		GameObject[] pp = GameObject.FindGameObjectsWithTag("PP_ON");
        LINE_POINTS.Clear();

        
		foreach(GameObject go in pp)
		{
			PPNode n = go.GetComponent<PPNode>();

            if(OPEN_GUIDS.Contains(go))StartCoroutine(ResetNode(go , Color.green));
            else if (CHECK_GUIDS.Contains(go)) StartCoroutine(ResetNode(go, Color.yellow));
            else if (CLOSED_GUIDS.Contains(go)||HOLD_GUIDS.Contains(go)) StartCoroutine(ResetNode(go, Color.red));
            else
            {
                go.GetComponent<SSObject>().Set_Mat_Color(Color.gray, true);
                go.GetComponent<SSObject>().Fade();
				go.GetComponent<HighlightableObject>().ConstantOff();
            }



			n.RevertTag();

			Destroy(n);
		}
        
		pp = GameObject.FindGameObjectsWithTag("PP_LINE");
		foreach(GameObject go in pp)
		{
			Destroy(go);
		}

        ACTIVE_GUIDS.Clear();


        Destroy(GetComponent<PPTree>());

		ptree = null;
		started = false;

	}

    IEnumerator SaveContactPoints(GameObject start)
    {
        
        /*
		WWWForm form = new WWWForm();
		form.AddField("sa" , "get_line");
		form.AddField("project" , SafeSim.PROJECT);
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx" , form);
		yield return www;
		
		// check for errors
		if (www.error == null)
		{
			guid_line.Clear();
			string[] str = www.text.Split('\n');
			for ( int i = 0 ; i < str.Length ; i++)
			{
				string[] row = str[i].Split('|');
				if(row.Length == 2 && !guid_line.ContainsKey(row[0]))
				{
					guid_line.Add(row[0], row[1]);
				}
				
				
			}
			
		}
		else Debug.Log("WWW Error: "+ www.error);


        */


        WWWForm form = new WWWForm();
        form.AddField("sa", "get_contact_points");
        form.AddField("project", SafeSim.PROJECT);
        WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
        yield return www;

        // check for errors
        if (www.error == null)
        {
            guid_contact_points.Clear();
			guid_program_codes.Clear();
            string[] str = www.text.Split('|');
            for (int i = 0; i < str.Length-1; i+=2)
            {
				string guid = str[i];
				string cp = str[i + 1];
				string pc = cp.Substring(cp.IndexOf("(") , cp.IndexOf(")") + 1);

				cp = cp.Replace(pc , "");

                guid_contact_points.Add(guid, cp);
				guid_program_codes.Add(guid , pc);
            }
        }
		else Debug.Log("WWW Error: " + www.error);


        //start pressure prop
        if (guid_contact_points.ContainsKey(start.name))
        {
            current_line = guid_contact_points[start.name];
            ptree = gameObject.AddComponent<PPTree>();
            ptree.Init(start);
        }
        else
        {
            Debug.LogError("Start object has no Line Number. (" + start.name + ")");
        }



        

        //
        //if (run) ptree.ContinuousTraversalAnimated(ptree.root);
        //else ptree.JunctionTraversal(ptree.root);

        if (vectorline)
        {
            yield return new WaitForSeconds(1.0f);
            ShowVLineProp();
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
            if (run) StartCoroutine(ptree.ContinuousTraversal(ptree.root));
            else ptree.JunctionTraversal(ptree.root);
        }
        

		started = false;
    }
    
    public void ShowVLineProp()
    {
        if (LINE_POINTS.Count == 0)
        {
            StartCoroutine(ptree.ContinuousTraversal(ptree.root));
            return;
        }
        VectorLine.SetCamera3D();
        vLine = new VectorLine(transform.name, LINE_POINTS.ToArray(), line_material, 16, LineType.Discrete, Joins.None);
        vLine.vectorObject.tag = "PP_LINE";
        vLine.AddNormals();
        vLine.active = false;
        vLine.vectorObject.renderer.material = line_material;
        vLine.continuousTexture = false;
        if (vLine != null) vLine.active = true;

        /*
        for (int i = 0; i < LINE_POINTS.Count; i += 2)
        {
            GameObject a = new GameObject();
            a.transform.position = LINE_POINTS[i];
            LineWave lwa = a.AddComponent<LineWave>();
            lwa.traceMaterial = line_material;
            
            GameObject b = new GameObject();
            b.transform.position = LINE_POINTS[i+1];
            lwa.targetOptional = b;
            lwa.traceWidth = 2;
            lwa.GetComponent<LineRenderer>().material = line_material;
        }
        */

    }

    IEnumerator SaveLineNumbers(GameObject start)
	{
		WWWForm form = new WWWForm();
		form.AddField("sa" , "get_line");
		form.AddField("project" , SafeSim.PROJECT);
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx" , form);
		yield return www;
		
		// check for errors
		if (www.error == null)
		{
			guid_line.Clear();
			string[] str = www.text.Split('\n');
			for ( int i = 0 ; i < str.Length ; i++)
			{
				string[] row = str[i].Split('|');
				if(row.Length == 2 && !guid_line.ContainsKey(row[0]))
                {
                   guid_line.Add(row[0], row[1]);
                }


			}

		}
		else Debug.Log("WWW Error: "+ www.error);
		
		//start pressure prop
		if(guid_line.ContainsKey(start.name))
		{
			current_line = guid_line[start.name];
            ptree = gameObject.AddComponent<PPTree>();
            ptree.Init(start );
		}
		else
		{
			Debug.LogError("Start object has no Line Number.");
		}

		yield return new WaitForSeconds(1.0f);

        if (run) StartCoroutine(ptree.ContinuousTraversal(ptree.root));
        else ptree.JunctionTraversal(ptree.root);

        started = false;

	}


	//GUI
	void OnGUI()
	{
		if(showWindow)
		{
            try
            {
                //Animate
			    line_material.mainTextureOffset = new Vector2(-Time.time , 0);
			    //line_material.mainTextureOffset = new Vector2(0 , Time.time);
                if(vLine !=null)vLine.Draw3D(transform);
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
            if (use_contact_points)
            {
                pp_window_rect = new Rect(Screen.width - 230, 35, 220, 130);
            }
            else
            {
                pp_window_rect = new Rect(Screen.width - 230, 35, 220, 85);
            }


			GUI.skin = PP_SKIN;
			GUI.Box(pp_window_rect , "");
            
			PressurePropWindow();


		}

	}

	private void PressurePropWindow()
	{				
		GUILayout.BeginArea(pp_window_rect);
		GUILayout.BeginVertical();	

		GUILayout.Space(10);

		GUILayout.BeginHorizontal();

		GUILayout.FlexibleSpace();
		if(GUILayout.Button(new GUIContent("","Start Propogation"),  PP_SKIN.FindStyle("Play")))
		{
			if(!run)Stop();
			StartPropogation(selection.GetSelectedGUID());
		}
		GUILayout.FlexibleSpace();
		if(run)
		{
			if (GUILayout.Button(new GUIContent("","Pause "), PP_SKIN.FindStyle("Pause")))
			{
				if(PPTree.pp_paused)
				{
					PPTree.pp_paused = false;
					for (int i = 0; i < ACTIVE_GUIDS.Count; i++)
					{
						PPNode p = ACTIVE_GUIDS[i];
						//StartCoroutine(ptree.ContinuousTraversal(p));
						ptree.ContinuousTraversalAnimated(p);
						ACTIVE_GUIDS.Remove(p);
					}
				}
				else
				{
					PPTree.pp_paused = true;
				}

			}
		}
		else
		{
			if (GUILayout.Button(new GUIContent(""," Next"), PP_SKIN.FindStyle("Next")))
			{
				if(!run)
				{
					if(ACTIVE_GUIDS.Count == 0)
					{
						NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage" , "Propogation Tree Complete."));
					}
					for (int i = 0; i < ACTIVE_GUIDS.Count; i++)
					{
						PPNode p = ACTIVE_GUIDS[i];
						//if (run) StartCoroutine(ptree.ContinuousTraversal(p));
						//else ptree.JunctionTraversal(p);
						ptree.JunctionTraversal(p);
						ACTIVE_GUIDS.Remove(p);
					}
				}
			}
		}
		GUILayout.FlexibleSpace();
		if(GUILayout.Button(new GUIContent("","Clear Propogation"),  PP_SKIN.FindStyle("Stop")))
		{
			Stop();
		}
		GUILayout.FlexibleSpace();

        //NOTHING SELECTED
        if (selection.GetSelectedGUID() == null)
        {
            if (GUILayout.Button(new GUIContent("","Clear All Stop Points"), PP_SKIN.FindStyle("ClearHold")))
            {
                foreach(GameObject go in HOLD_GUIDS)
                {
                    go.GetComponent<SSObject>().Set_Mat_Color(Color.gray, true);
                }
                HOLD_GUIDS.Clear();
            }
        }
        //HOLD GUID SELECTED
        else if (HOLD_GUIDS.Contains(selection.GetSelectedGUID()))
        {
            if (GUILayout.Button(new GUIContent("","Clear Stop Point"), PP_SKIN.FindStyle("Open")))
            {
                foreach (GameObject go in selection.GetSelection())
                {
                    if (HOLD_GUIDS.Contains(go))
                    {
                        go.GetComponent<SSObject>().Set_Mat_Color(Color.gray, true);    
                        HOLD_GUIDS.Remove(go);
                    }
                }
                if (selection.GetSelection().Count == 1)
                {
                    if (GameObject.FindGameObjectsWithTag("PP_ON").Length > 0) StartPropogation(selection.GetSelectedGUID());
                }
            }
        }
        //closed guid selected
        else if (CLOSED_GUIDS.Contains(selection.GetSelectedGUID()))
        {
            if (GUILayout.Button(new GUIContent("","Open  "), PP_SKIN.FindStyle("Open")))
            {
                GameObject go = selection.GetSelectedGUID();
				selection.Clear();
				while(CLOSED_GUIDS.Contains(go))
					CLOSED_GUIDS.Remove(go);
				OPEN_GUIDS.Add(go);
				go.GetComponent<SSObject>().Set_Mat_Color(Color.green, true);



                if (GameObject.FindGameObjectsWithTag("PP_ON").Length > 0) StartPropogation(go);

            }
        }
        //OPEN GUID SELECTED
        else if (OPEN_GUIDS.Contains(selection.GetSelectedGUID()))
        {
            if (GUILayout.Button(new GUIContent(""," Close"), PP_SKIN.FindStyle("Close")))
            {
                GameObject go = selection.GetSelectedGUID();
				selection.Clear();
				OPEN_GUIDS.Remove(go);
                CLOSED_GUIDS.Add(go);
				go.GetComponent<SSObject>().Set_Mat_Color(Color.red, true);
            }
        }
        else
        {
            if (GUILayout.Button(new GUIContent(""," Set Stop Point"), PP_SKIN.FindStyle("Close")))
            {
                foreach (GameObject go in selection.GetSelection())
                {
                    go.GetComponent<SSObject>().Set_Mat_Color(Color.red, true);
                    HOLD_GUIDS.Add(go);
                }
                selection.Clear();
                
            }
        }
        
        GUILayout.FlexibleSpace();
        if(GUILayout.Button( new GUIContent(run ? "Run " : "Step"  , "Propogation mode"), GUILayout.Width(40)))
		{
			Stop();
			run = !run;
		}
        GUILayout.Space(5);
         
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(15);
        GUILayout.Label("Speed: ");
        PPTree.traversal_delay = GUILayout.HorizontalSlider(PPTree.traversal_delay, 1.0f, 0.0f , GUILayout.Width(140));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (use_contact_points)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            TMPPSIint = GUILayout.SelectionGrid(TMPPSIint, TMPPSI, 2 , GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            tempFieldint = GUILayout.SelectionGrid(tempFieldint, tempFields, 4, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        
		GUILayout.EndVertical();
		GUILayout.EndArea();






        //ToolTIp
        if (GUI.tooltip != "")
        {

            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;

            Rect rect = new Rect();


            //y
            rect.y = Screen.height - (y - 30);
            //width
            rect.width = GUI.tooltip.Length * 7;

            //x
            if (Screen.width - x < rect.width) rect.x = x - rect.width;
            else rect.x = x + 10;

            //height
            rect.height = 25;
            GUI.Box(rect, GUI.tooltip , tooltip_style);
            //GUI.Label(rect, GUI.tooltip);
        }


	}

    int TMPPSIint = 0;
    int tempFieldint = 0;

    int prevTMPPSIint = 0;
    int prevtempFieldint = 0;

    void LateUpdate()
    {
        if (TMPPSIint != prevTMPPSIint)
        {
            CTemp();
            prevTMPPSIint = TMPPSIint;
        }
        else if (tempFieldint != prevtempFieldint)
        {
            CTemp();
            prevtempFieldint = tempFieldint;
        }
    }

    void CTemp()
    {
        string field = "";
        switch (tempFieldint)
        {
            case 0:
                field = "DESIGN";
                break;
            case 1:
                field = "OPER";
                break;
            case 2:
                field = "MAX";
                break;
            case 3:
                field = "MIN";
                break;
        }

        switch (TMPPSIint)
        {
            case 0:
                if (field.Equals("DESIGN") || field.Equals("OPER")) field += "DEG_";
                else field += "TEMP_";
                break;
            case 1:
                field += "PSI_";
                break;
        }

        StartCoroutine(viewer.model.ColorTemp(field));
    }

}
