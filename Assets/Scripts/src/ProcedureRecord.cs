using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProcedureRecord : MonoBehaviour
{

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
    private bool isRecording = false;
    private bool doneRecording = false;

    private bool isSaved = false;

    public float timeLimit = 10;



    private string title_field = "";
    private string description_field = "";

    private List<string> used = new List<string>();

    private string _default_procedure_name = "<Enter_Procedure_Name>";
    private string _default_procedure_description = "<Enter_Procedure_Description>";

    // Use this for initialization
    void Start()
    {
        description_style = procedure_skin.GetStyle("description_style");
        procedure = new Procedure(-1, "", "");
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
        showTitle = true;
    }

    void Update()
    {
        if(isRecording)
        {
            if (selection.GetSelectedGUID() != null )//&& !used.Contains(selection.GetSelectedGUID().name))
            {
                show_action_buttons = true;
            }
            else show_action_buttons = false;
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
        GUI.SetNextControlName("procedure_description");
        procedure.description = GUILayout.TextField(procedure.description);
        if (UnityEngine.Event.current.type == EventType.Repaint)
        {
            if (GUI.GetNameOfFocusedControl() == "procedure_name")
            {
                if (procedure.name == _default_procedure_name) procedure.name = "";
            }
            else if (GUI.GetNameOfFocusedControl() == "procedure_description")
            {
                if (procedure.description == _default_procedure_description) procedure.description = "";
            }
        }
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
        if (GUILayout.Button("Record New Procedure"))
        {
            StartProcedure();
        }
        if (GUILayout.Button("Back to Menu"))
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

    void TestingGUI()
    {
        GUILayout.BeginArea(new Rect(15, 30, 200, 150));
        GUILayout.BeginHorizontal();
        GUILayout.Label("Step: " + current_step + " ");
        GUILayout.Label(" " + current_action);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();


        if (show_action_buttons)
        {

            GUILayout.BeginArea(new Rect(Screen.width / 4, Screen.height - 200, Screen.width / 2, 150));

            description_field = GUILayout.TextArea(description_field, GUILayout.Height(60));

            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0, Screen.height - 90, Screen.width, 75));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(" Open", aButton)))
            {
                SetAction(selection.GetSelectedGUID(), "OPEN");
            }
            if (GUILayout.Button(new GUIContent(" Close", bButton)))
            {
                SetAction(selection.GetSelectedGUID(), "CLOSE");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

        }

        if (show_action_buttons)
        {
            if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 40, 80, 30), "NEXT"))
            {
                switch (current_action)
                {
                    case "OPEN":
                        Action(current_go, true);
                        break;
                    case "CLOSE":
                        Action(current_go, false);
                        break;
                    case "":
                        return;
                        break;
                }

            }
        }
        else
        {
            //done button
            if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 40, 80, 30), "DONE"))
            {
                EndProcedure();
            }
        }




    }
    GameObject current_go;
    string current_action = "";
    void SetAction(GameObject go , string action)
    {
        current_go = go;
        current_action = action;
    }


    void Action(GameObject guid, bool open)
    {
        selection.Clear();
        ProcedureAction pa = new ProcedureAction(guid.name, open ? "open" : "close", description_field);
        description_field = "";
        procedure.GetSteps().Add(current_step, pa);
        used.Add(guid.name);
        iTween.ColorTo(guid, open ? Color.green : Color.red, 1.0f);
        current_step++;
        current_action = "";
        current_go = null;
    }



    void ClosingScreen()
    {
        Rect box = new Rect(side_margin, vert_margin, Screen.width - (2 * side_margin), Screen.height - (2 * vert_margin));
        GUI.Box(box, "");
        GUILayout.BeginArea(box);

        GUILayout.BeginVertical();
        GUILayout.Space(40);

        //Procedure Name
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(procedure.GetName());
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(40);

        //Procedure Description
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.FlexibleSpace();
        GUILayout.Label(procedure.GetDescription(), description_style);
        GUILayout.FlexibleSpace();
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();

        GUILayout.Space(20);

        //Left Side
        GUILayout.BeginVertical();

        GUILayout.Label("Steps: " + procedure.GetStepCount());

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();



        //Right Side
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if (!isSaved)
        {
            if (GUILayout.Button("Save Procedure"))
            {
                StartCoroutine(WriteProcedureToDb());            
            }
        }
        else
        {
            if (GUILayout.Button("Record Another Procedure"))
            {
                Application.LoadLevel("procedure_record");
            }
        }

        if (GUILayout.Button("Back to Menu"))
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

    void OnGUI()
    {
        GUI.skin = procedure_skin;
        if (showTitle)
        {
            TitleScreen();
        }
        else if (doneRecording)
        {
            ClosingScreen();
        }
        else if (isRecording)
        {
            TestingGUI();
        }

    }

    void StartProcedure()
    {
        Debug.Log(procedure.description);
        GetComponent<Viewer>().model.DefaultView();
        orbiter.SetActive(false);
        spectator.SetActive(false);
        avatar.SetActive(true);
        selection.enabled = true;

        current_step = 1;
        doneRecording = false;
        showTitle = false;
        isRecording = true;


    }


    void EndProcedure()
    {

        selection.Clear();


        avatar.SetActive(false);
        spectator.SetActive(true);
        spectator.GetComponentInChildren<PivotController>().ResetView(model.CENTER_POS);
        spectator.GetComponentInChildren<SpectatorCamera>().ResetView();
        isRecording = false;
        doneRecording = true;

    }

    IEnumerator WriteProcedureToDb( )
    {

        WWWForm form = new WWWForm();
        form.AddField("sa", "save_procedure");
        form.AddField("project_name", SafeSim.PROJECT.Replace("PID", "") + "/" + SafeSim.ASSET);
        form.AddField("procedure_name", procedure.GetName().Substring(procedure.GetName().IndexOf(')') + 1));
        form.AddField("procedure_description", procedure.GetDescription());
        form.AddField("number_of_steps", procedure.GetStepCount());
        Dictionary<int, ProcedureAction> steps = procedure.GetSteps();
        foreach (int i in steps.Keys)
        {
            ProcedureAction pa = steps[i];
            form.AddField("step_" + i, pa.GetGUID() + "|" + pa.GetAction() + "|" + pa.GetDescription());
        }


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

    }


}
