using System;
using UnityEngine;
using System.Collections;

public class ProcedureDemo : MonoBehaviour
{

    //GUI
    private float side_margin = 50;
    private float vert_margin = 30;

    private bool show_action_buttons = false;

    public GUISkin procedure_skin;
    private GUIStyle description_style;


    //Control
    public ControlManager control_manager;
    private Procedure procedure;
    public GameObject spectator;
    public SpectatorCamera spectator_camera;
    public GameObject orbiter;

    private int current_step = 1;

    private bool showTitle = false;
    private bool demo_started = false;
    private bool demo_over = false;

    private string commentText = "";

    Vector2 proc_desc_scroll_pos = Vector2.zero;

    SSModel model;

    GUIStyle comment_style;


    // Use this for initialization
    void Start()
    {
        comment_style = procedure_skin.GetStyle("comment_style");
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

    void TitleScreen()
    {
        Rect box = new Rect(side_margin, vert_margin, Screen.width - (2 * side_margin), Screen.height - (2 * vert_margin));
        GUI.Box(box, "");
        GUILayout.BeginArea(box);

        GUILayout.BeginVertical();
        GUILayout.Space(40);

        //Procedure Name
        GUILayout.BeginHorizontal();
        GUILayout.Space(50);
        GUILayout.Label(procedure.GetName());
        //GUILayout.FlexibleSpace();
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
        /*
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Space(50);
        GUILayout.Label("Steps: " + procedure.GetStepCount());
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        
        GUILayout.Space(40);
        */

        //Right Side
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Start Demo"))
        {
            StartProcedure();
        }
        if (GUILayout.Button("Back to Menu"))
        {
            Application.LoadLevel("main");
        }

        GUILayout.EndVertical();

        GUILayout.Space(40);

        GUILayout.EndHorizontal();

        GUILayout.Space(50);
        GUILayout.EndVertical();

        GUILayout.EndArea();


    }

    void TestingGUI()
    {

        //show open/close buttons
        if (demo_started)
        {
            //Title
            Rect title_box = new Rect(20, 20, Screen.width - 80, 30);
            GUILayout.BeginArea(title_box, "");
            GUILayout.BeginHorizontal();
            GUILayout.Label(procedure.GetName());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            //GUI.Label(new Rect(40, 70, Screen.width - 80, 100), procedure.GetDescription() , description_style);
            //Description
            Rect description_box = new Rect(30, 70, Screen.width - 80, 100);
            GUI.Box(description_box, "" , new GUIStyle());
            GUILayout.BeginArea(description_box, "");
            GUILayout.BeginHorizontal();
            GUILayout.Label(commentText  , comment_style);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0, Screen.height - 60, Screen.width, 60));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("<", GUILayout.Height(50), GUILayout.Width(50)))
            {
                current_step--;
                if (current_step < 1)
                {
                    current_step = 1;
                }
                else
                {
                    ProcedureAction pa = procedure.steps[current_step + 1];

                    GameObject guid = model.GUIDs[pa.GetGUID()];
                    PulseColors pc = guid.GetComponent<PulseColors>();
                    pc.Stop();

                    if(model.GUIDs.ContainsKey(pa.GetInsulation()))
                    {
                        GameObject insulation = model.GUIDs[pa.GetInsulation()];
                        PulseColors pci = insulation.GetComponent<PulseColors>();
                        pci.Stop();
                    }

                    MoveTarget();
                }
            }
            GUILayout.Label(current_step + " / " + procedure.GetStepCount());
            if (GUILayout.Button(">", GUILayout.Height(50), GUILayout.Width(50)))
            {
                if (current_step >= procedure.GetStepCount())
                {
                    return;
                }

                //Turn off current
                ProcedureAction pa = procedure.steps[current_step];
                GameObject guid = model.GUIDs[pa.GetGUID()];
                PulseColors pc = guid.GetComponent<PulseColors>();

                if (pa.GetAction().Equals("open"))
                {
                    pc.Stop(Color.green);
                }
                else if (pa.GetAction().Equals("close"))
                {
                    pc.Stop(Color.red);
                }
                if(model.GUIDs.ContainsKey(pa.GetInsulation()))
                {
                    GameObject insulation = model.GUIDs[pa.GetInsulation()];
                    PulseColors pci = insulation.GetComponent<PulseColors>();
                    if (pa.GetAction().Equals("open"))
                    {
                        pci.Stop(Color.green);
                    }
                    else if (pa.GetAction().Equals("close"))
                    {
                        pci.Stop(Color.red);
                    }
                }
                
                current_step++;
                MoveTarget();
                
                

            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        //done button
        if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 40, 80, 30), "DONE"))
        {
            EndProcedure();
        }

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

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();



        //Right Side
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("ReStart"))
        {
            Application.LoadLevel("procedure_demo");
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
        else if (demo_over)
        {
            ClosingScreen();
        }
        else if (demo_started)
        {
            TestingGUI();
        }

    }

    void StartProcedure()
    {
        model = GetComponent<Viewer>().model;

        model.DefaultView();

        orbiter.SetActive(false);
        spectator.SetActive(true);

        demo_over = false;
        showTitle = false;
        demo_started = true;
        current_step = 1;
        MoveTarget();
        
    }


    void EndProcedure()
    {
        spectator.SetActive(true);
        spectator.GetComponentInChildren<PivotController>().ResetView(model.CENTER_POS);
        spectator.GetComponentInChildren<SpectatorCamera>().ResetView();
        //orbiter.SetActive(true);
        //orbiter.GetComponentInChildren<OrbiterCamera>().target.position = GetComponent<Viewer>().model.CENTER_POS;
        demo_started = false;
        demo_over = true;

    }

    void MoveTarget()
    {
        ProcedureAction pa = procedure.steps[current_step];
        GameObject guid = model.GUIDs[pa.GetGUID()];

		if (PressurePropogation.use_contact_points)
		{
			Application.ExternalCall("SafeSimSelect", SafeSim.ASSET, Select.GetGameObjectPath(guid));	
		}
		else
		{
			Application.ExternalCall("SafeSimSelect", Select.GetGameObjectPath(guid));	
		}
        //camera.target.position = go.transform.renderer.bounds.center;
        iTween.MoveTo(spectator_camera.target.gameObject, guid.transform.renderer.bounds.center, 0.50f);
        spectator_camera.CloseUp(guid.renderer.bounds.size);



        PulseColors pulseEffect = guid.AddComponent<PulseColors>();

        Color gcolor;
        try
        {
            gcolor = guid.GetComponent<SSObject>().Get_Origianl_Color();
        }
        catch (Exception e)
        {
            gcolor = guid.renderer.material.color;
        }
        
        
        commentText = pa.GetDescription();

        if (pa.GetAction().Equals("open"))
        {
            pulseEffect.StartPulse(gcolor, Color.green);
        }
        else if (pa.GetAction().Equals("close"))
        {
            pulseEffect.StartPulse(gcolor, Color.red);
        }


        //Insulation
        try
        {
            GameObject insulation = model.GUIDs[guid.name + "_I"];
            PulseColors pulseEffectInul = insulation.AddComponent<PulseColors>();

            if (pa.GetAction().Equals("open"))
            {
                pulseEffectInul.StartPulse(insulation.renderer.material.color, Color.green);
            }
            else if (pa.GetAction().Equals("close"))
            {
                pulseEffectInul.StartPulse(insulation.renderer.material.color, Color.red);
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }


    }

}
