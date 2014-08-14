using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ProcedureEdit : MonoBehaviour {

    //GUI
    private float side_margin = 50;
    private float vert_margin = 30;

    public Texture aButton;
    public Texture bButton;

    private bool show_action_buttons = false;

    public GUISkin procedure_skin;
    private GUIStyle description_style;


    //Control
    public Select selection;
    private Procedure procedure;
    public GameObject avatar;
    public GameObject orbiter;
    public GameObject spectator;

    private SSModel model;

    private int current_step = 1;

    private bool showTitle = false;
    private bool doneEditing = false;

    private bool isSaved = false;
    private bool isEditing = false;

    private string title_field = "";
    private string description_field = "";

    private List<string> used = new List<string>();

    private string _default_procedure_name = "<Enter_Procedure_Name>";
    private string _default_procedure_description = "<Enter_Procedure_Description>";


	// Use this for initialization
    void Start()
    {
        description_style = procedure_skin.GetStyle("description_style");
        procedure = SafeSim.PROCEDURE;
    }
        
    public void ShowTitle(Vector3 center)
    {
        //orbiter.SetActive(true);
        //orbiter.GetComponentInChildren<OrbiterCamera>().target.position = center;
        model = GetComponent<Viewer>().model;
        model.DefaultView();
        spectator.SetActive(true);
        spectator.GetComponentInChildren<PivotController>().ResetView(center);
        spectator.GetComponentInChildren<SpectatorCamera>().ResetView();
        StartCoroutine(LoadProcedure());
    }
    public IEnumerator LoadProcedure()
    {
        yield return StartCoroutine(procedure.LoadProcedure());
        showTitle = true;
    }

	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator UpdateProcedure()
    {
        WWWForm form = new WWWForm();
        form.AddField("sa", "update_procedure");
        form.AddField("project_name", SafeSim.PROJECT.Replace("PID", "") + "/" + SafeSim.ASSET);
        form.AddField("procedure_name", procedure.name);
        form.AddField("procedure_description", procedure.GetDescription());
        form.AddField("procedure_id", procedure.GetId());

        Dictionary<int, ProcedureAction> steps = procedure.GetSteps();
        List<int> removeKeys = new List<int>();
        foreach(KeyValuePair<int,ProcedureAction> step in steps)
            if(step.Value.GetGUID() == "") removeKeys.Add(step.Key);
        foreach(int rkey in removeKeys)steps.Remove(rkey);
          
        steps = procedure.GetSteps();
        foreach (KeyValuePair<int, ProcedureAction> step in steps)
        {
            ProcedureAction pa = step.Value;
            form.AddField("step_" + step.Key, pa.GetGUID() + "|" + pa.GetAction() + "|" + pa.GetDescription());
        }
        form.AddField("number_of_steps", steps.Count);

        WWW www = new WWW(SafeSim.URL + "save_handler.ashx", form);

        yield return www;

        if (www.error == null)
        {
            isSaved = true;
        }
        else
        {
            Debug.LogError(www.error);
        }

        Application.LoadLevel("main");

    }


    void OnGUI()
    {
        GUI.skin = procedure_skin;
        if (showTitle)
        {
            TitleScreen();
        }
        else if (isEditing)
        {
            EditingGUI();
        }
    }

    void TitleScreen()
    {
        Rect box = new Rect(side_margin, vert_margin, Screen.width - (2 * side_margin), Screen.height - (2 * vert_margin));
        GUI.Box(box, "");
        GUILayout.BeginArea(box);

        GUILayout.BeginVertical();
        GUILayout.Space(40);

        //Procedure Name
        GUILayout.BeginHorizontal();
        GUILayout.Space(40);
        GUILayout.Label("Name: ");
        GUI.SetNextControlName("procedure_name");
        procedure.name = GUILayout.TextField(procedure.name);

        GUILayout.Space(40);
        GUILayout.EndHorizontal();
        GUILayout.Space(40);

        //Procedure Description
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);

        GUILayout.Label("Description");
        procedure.description = GUILayout.TextField(procedure.description);

        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();

        GUILayout.Space(20);

        //Left Side
        GUILayout.BeginVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();



        //Right Side
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Continue..."))
        {
            StartProcedure();
        }
        if (GUILayout.Button("Cancel"))
        {
            Application.LoadLevel("main");
        }

        GUILayout.EndVertical();

        GUILayout.Space(20);

        GUILayout.EndHorizontal();

        GUILayout.Space(40);
        GUILayout.EndVertical();

        GUILayout.EndArea();


    }
    void EditingGUI()
    {
        GUILayout.BeginArea(new Rect(15, 30, 200, 150));
        GUILayout.BeginHorizontal();
        GUILayout.Label("Step: " + current_step + " ");
        GUILayout.Label(" " + current_action);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        //Desctiption box
        GUILayout.BeginArea(new Rect(Screen.width / 4, Screen.height - 200, Screen.width / 2, 150));
        description_field = GUILayout.TextArea(description_field, GUILayout.Height(60));
        GUILayout.EndArea();

        if (GUI.Button(new Rect(10, Screen.height - 40, 80, 30), "CANCEL"))
        {
            Application.LoadLevel("main");
        }


        //Open/Close buttons
        GUILayout.BeginArea(new Rect(0, Screen.height - 90, Screen.width, 75));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent(" Open", aButton)))
        {
            if (selection.GetSelectedGUID() == null)
            {
                SetAction(current_go , "OPEN");
            }
            else SetAction(selection.GetSelectedGUID(), "OPEN");
        }
        if (GUILayout.Button(new GUIContent(" Close", bButton)))
        {
            if (selection.GetSelectedGUID() == null)
            {
                SetAction(current_go, "CLOSE");
            }
            else SetAction(selection.GetSelectedGUID(), "CLOSE");
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 120, 100, 30), "PREVIOUS"))
        {
            Advance(false);
        }
        if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 80, 100, 30), "NEXT"))
        {
            Advance(true);
        }
        //done button
        if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 40, 100, 30), "DONE"))
        {
            EndProcedure();
        }
    }


    void Advance(bool advance)
    {
        switch (current_action.ToUpper())
        {
            case "OPEN":
                Action(current_go, true , advance);
                break;
            case "CLOSE":
                Action(current_go, false , advance);
                break;
            case "":
                if (!advance)
                {
                    current_step--;
                    UpdateCurrent();
                }
                return;
                break;
        }
    }

    void StartProcedure()
    {
        GetComponent<Viewer>().model.DefaultView();
        orbiter.SetActive(false);
        spectator.SetActive(false);
        avatar.SetActive(true);
        selection.enabled = true;
        current_step = 1;
        showTitle = false;
        isEditing = true;
        ProcedureAction pa = procedure.steps[current_step];
        current_go = model.GUIDs[pa.GetGUID()];
        current_action = pa.GetAction();
        description_field = pa.GetDescription();
        ColorCurrenet();

    }

    void ColorCurrenet()
    {
        current_go.GetComponent<SSObject>().Set_Mat_Color(current_action.ToUpper().Equals("OPEN") ? Color.green : Color.red, true);
        current_go.GetComponent<HighlightableObject>().FlashingOn(Color.blue, Color.white, 0.5f);
    }
    void UnColorCurrent()
    {
        current_go.GetComponent<SSObject>().Reset_Original_Colors();
        current_go.GetComponent<HighlightableObject>().FlashingOff();
    }
    void EndProcedure()
    {
        Advance(true);
        selection.Clear();
        avatar.SetActive(false);
        spectator.SetActive(true);
        spectator.GetComponentInChildren<PivotController>().ResetView(model.CENTER_POS);
        spectator.GetComponentInChildren<SpectatorCamera>().ResetView();
        isEditing = false;
        doneEditing = true;

        StartCoroutine(UpdateProcedure());

    }

    GameObject current_go;
    string current_action = "";
    void SetAction(GameObject go, string action)
    {
        if(current_go != null)UnColorCurrent();
        current_go = go;
        current_action = action;

        if (current_go != null) ColorCurrenet();
    }
    void Action(GameObject guid, bool open, bool advance)
    {
        selection.Clear();
        ProcedureAction pa = new ProcedureAction(guid.name, open ? "open" : "close", description_field);
        description_field = "";

        guid.GetComponent<HighlightableObject>().FlashingOff();
        
        procedure.GetSteps()[current_step] = pa;

        used.Add(guid.name);
        iTween.ColorTo(guid, open ? Color.green : Color.red, 1.0f);

        if (advance) current_step++;
        else if (current_step == 1) ;

        else current_step--;

        current_action = "";
        current_go = null;

        UpdateCurrent();

    }
    void UpdateCurrent()
    {
        ProcedureAction pa;
        try
        {
            pa = procedure.GetSteps()[current_step];
            current_go = model.GUIDs[pa.GetGUID()];
            current_action = pa.GetAction();
            description_field = pa.GetDescription();
            ColorCurrenet();
        }
        catch (Exception)
        {
            pa = new ProcedureAction("", "", "");
            procedure.GetSteps().Add(current_step , pa);
        }
        

    }
}
