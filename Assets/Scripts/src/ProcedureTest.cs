using System;
using UnityEngine;
using System.Collections;

public class ProcedureTest : MonoBehaviour {

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

    private int current_step = 1;

    private bool showTitle = false;
    private bool test_started = false;
    private bool test_over = false;
    private bool isFailed = false;

    public float timeLimit = 220;
    private float timeLeft;
    private float startTime;

    SSModel model;

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

    void Update()
    {
        if (test_started)
        {
            try
            {
                if (selection.GetSelectedGUID() != null)
                {
                    /* PROXIMITY TEST
                    if (Vector3.Distance(walker.transform.position, selection.GetSelectedGUID().renderer.bounds.center) < 2.0f)
                    {
                        showActionButtons = true;
                    }
                    else
                    {
                        showActionButtons = false;
                    }
                     */
                    show_action_buttons = true;
                }
                else
                {
                    show_action_buttons = false;
                }
            }
            catch (NullReferenceException e)
            {
                show_action_buttons = false;
            }
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0) EndProcedure();

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
        GUILayout.FlexibleSpace();
        GUILayout.Label(procedure.GetName());
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(40);

        //Procedure Description
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.FlexibleSpace();
        GUILayout.Label(procedure.GetDescription() , description_style);
        GUILayout.FlexibleSpace();
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();

        GUILayout.Space(20);

        //Left Side
        GUILayout.BeginVertical();
        GUILayout.Label("Time Limit: ");

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();



        //Right Side
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Start"))
        {
            StartProcedure();
        }
        if(GUILayout.Button("Back to Menu"))
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

        //show open/close buttons
        if (show_action_buttons)
        {
            GUILayout.BeginArea(new Rect(0, Screen.height - 100, Screen.width, 75));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("Open", aButton)))
            {
                Action(selection.GetSelectedGUID(), true);
            }
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Close", bButton)))
            {
                Action(selection.GetSelectedGUID(), false);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        //show timer
        GUI.Label(new Rect(Screen.width - 100, Screen.height - 110, 80, 50), ToTime(timeLeft));

        //done button
        if(GUI.Button(new Rect(Screen.width - 100 , Screen.height - 40 , 80 , 30) , "DONE"))
        {
            EndProcedure();
        }

    }

    void Action(GameObject go, bool open)
    {
        selection.Clear();
        iTween.ColorTo(go, open ? Color.green : Color.red, 0.50f);
        if (!procedure.CheckAction(go.name, open ? "open" : "close", current_step))
        {
            isFailed = true;
        }
        current_step++;
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
        if (isFailed) GUILayout.Label("Failure");
        else GUILayout.Label("Success");
        
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();



        //Right Side
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("ReStart"))
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

    void OnGUI()
    {
        GUI.skin = procedure_skin;
        if (showTitle)
        {
            TitleScreen();
        }
        else if (test_over)
        {
            ClosingScreen();
        }
        else if (test_started)
        {
            TestingGUI();
        }

    }

    void StartProcedure()
    {

        GetComponent<Viewer>().model.DefaultView();
        orbiter.SetActive(false);
        spectator.SetActive(false);
        avatar.SetActive(true);
        selection.enabled =  true;

        timeLeft = timeLimit;
        startTime = Time.time;

        isFailed = false;
        current_step = 1;
        test_over = false;
        showTitle = false;
        test_started = true;
    }


    void EndProcedure()
    {

        selection.Clear();
        if (current_step == procedure.GetStepCount() + 1 && timeLeft > 0 && !isFailed)
        {
            isFailed = false;
        }
        else
        {
            isFailed = true;
        }

        StartCoroutine(LogTestRport());

        avatar.SetActive(false);
        spectator.SetActive(true);
        spectator.GetComponentInChildren<PivotController>().ResetView(model.CENTER_POS);
        spectator.GetComponentInChildren<SpectatorCamera>().ResetView();
        test_started = false;
        test_over = true;

    }

    IEnumerator LogTestRport()
    {
        WWWForm form = new WWWForm();
        form.AddField("sa", "save_test_report");
        form.AddField("userName", SafeSim.USERNAME);
        form.AddField("asset", SafeSim.ASSET);
        form.AddField("procedure_name", procedure.GetName().Substring(procedure.GetName().IndexOf(')') + 1));
        form.AddField("project_name", SafeSim.PROJECT.Replace("PID", ""));
        form.AddField("result", isFailed ? "FAIL" : "PASS");

        WWW www = new WWW(SafeSim.URL + "save_handler.ashx", form);
        yield return www;

        if (www.error == null)
        {
        }
        else
        {
            Debug.Log(www.error);
        }


    }

    string ToTime(float seconds)
    {
        var span = TimeSpan.FromSeconds(seconds);
        return string.Format("{0}:{1:00}", span.Minutes, span.Seconds);
    }


}
