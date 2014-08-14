using UnityEngine;
using System.Collections;

public class OptionsScreen : MonoBehaviour {


	private bool show = false;
	private float left_panel_width = 100;
    private float side_margin = 50;
    private float vert_margin = 30;

	enum Switch {General , Video , Control , Propogation};

	Switch current = Switch.General;

    //References
	public ControlManager cm;
    public Select selection;
    public SpectatorCamera vcam;
    public OrbiterCamera ocam;
    public ThirdPersonShooter tpc;

    public Grid grid;

	bool collision = false;
    bool collision_new = true;
	bool insulation = true;
    bool insulation_new = true;
	bool showPivot = true;
    bool showPivot_new = true;
    bool aa = true;
    bool aa_new = true;
    bool enhanced_contrast = true;
    bool enhanced_contrast_new = true;
	bool edge_detection = false;
	bool edge_detection_new = false;
    bool show_grid = false;
    bool show_grid_new = false;


	//References- assign in spector
	public InfoPanel infoPanel;


    private Rect box;


    void Start()
    {
        aa = true;
        aa_new = true;
    }

	public void ShowOptions()
	{
		show = true;
		GetComponent<TopMenu>().CloseAll();
        Camera.main.GetComponent<SepiaToneEffect>().enabled = true ;
        selection.enabled = false; 
		
	}
	private void CloseOptions()
	{
        Camera.main.GetComponent<SepiaToneEffect>().enabled = false;
		show = false;
		GetComponent<TopMenu>().enabled = true;
        selection.enabled = true;
		
	}

    public void ToggleInsulation(bool show)
    {
        GameObject[] ins = GameObject.FindGameObjectsWithTag("Insulation");
        foreach (GameObject go in ins)
        {
            go.renderer.enabled = show;
            go.GetComponent<MeshCollider>().enabled = show;
        }
        insulation = show;
        insulation_new = show;
    }


    void LateUpdate()
    {

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                SetCollision(!collision_new);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInsulation(!insulation);
            }

        }


        if (show)
        {
            if (insulation_new != insulation)
            {
                ToggleInsulation(insulation_new);
            }

            if (showPivot_new != showPivot)
            {
                cm.TogglePivot(showPivot_new);
                showPivot = showPivot_new;
            }

            if (collision_new != collision)
            {
                SetCollision(collision_new);
            }

            if (aa_new != aa)
            {
                cm.ToggleAA(aa_new);
                aa = aa_new;
            }

            if (enhanced_contrast_new != enhanced_contrast)
            {
                cm.ToggleEnchancedContrast(enhanced_contrast_new);
                enhanced_contrast = enhanced_contrast_new;
            }
			if(edge_detection_new != edge_detection)
			{
				cm.ToggleEdgeDetection(edge_detection_new);
				edge_detection = edge_detection_new;
			}
            if (show_grid_new != show_grid)
            {
                if (show_grid_new) grid.ShowGrid();
                else grid.HideGrid();
            }
        }
    }


    private void SetCollision(bool isOn)
    {
        Physics.IgnoreLayerCollision((int)SafeSim.Layers.model,( int)SafeSim.Layers.Controller, !isOn);
        collision = isOn;
        collision_new = isOn;
    }

    private void ControlOptions()
    {
        GUI.Box(box, "Control");
        GUILayout.BeginArea(new Rect(box.x + 50 , box.y , box.width , box.height ));


        GUILayout.BeginVertical();
        GUILayout.Space(80);

        //Zoom Speed
        GUILayout.Label("Zoom Speed: ");
        if (vcam.enabled)
        {
            vcam.zoomRate = Mathf.FloorToInt(GUILayout.HorizontalSlider(vcam.zoomRate, 1, 100, GUILayout.Width(200)));
        }
        else
        {
            ocam.scrollSpeed = GUILayout.HorizontalSlider(ocam.scrollSpeed, 0.01f, 25.0f, GUILayout.Width(200));
        }

        GUILayout.Space(50);

        //Auto Orbit Speed
        GUILayout.Label("Auto Orbit Speed: ");
        if (ocam.enabled)
        {
            ocam.autoOrbitSpeed = (GUILayout.HorizontalSlider(ocam.autoOrbitSpeed, 0.5f, 10, GUILayout.Width(200)));
        }

        GUILayout.EndVertical();


        GUILayout.Space(50);

        GUILayout.EndArea();





    }
	void GeneralOptions()
	{
		//Rect box = new Rect(side_margin + left_panel_width , vert_margin , Screen.width - (left_panel_width + side_margin ), Screen.height - (vert_margin * 2));
		GUI.Box(box , "General");
		GUILayout.BeginArea(box);


		GUILayout.BeginVertical();

		GUILayout.Space(70);

		GUILayout.BeginHorizontal();
		GUILayout.Space(20);
		infoPanel.enabled = GUILayout.Toggle(infoPanel.enabled , "Info Panel");
		GUILayout.EndHorizontal();

        GUILayout.Space(30);

		GUILayout.BeginHorizontal();
		GUILayout.Space(20);
		collision_new = GUILayout.Toggle(collision, "Collision");
		GUILayout.EndHorizontal();

        GUILayout.Space(30);


        //Avatar Gravity
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        tpc.useGravity = GUILayout.Toggle(tpc.useGravity, "Gravity");
        GUILayout.EndHorizontal();

        GUILayout.Space(30);




		GUILayout.BeginHorizontal();
		GUILayout.Space(20);
		insulation_new = GUILayout.Toggle(insulation, "Insulation");
		GUILayout.EndHorizontal();

        GUILayout.Space(30);

		GUILayout.BeginHorizontal();
		GUILayout.Space(20);
		showPivot_new = GUILayout.Toggle(showPivot , "Show Pivot When Rotating");
		GUILayout.EndHorizontal();



        /*
         
         * 


         * 
        GUILayout.Space(30);

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        PressurePropogation.use_contact_points = GUILayout.Toggle(PressurePropogation.use_contact_points, "Use Contact Points (Pressure Propogation)");
        GUILayout.EndHorizontal();

         * 
         * 
         * 
         
        GUILayout.Space(30);

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        PPNode.animate = GUILayout.Toggle(PPNode.animate, "Animate Propogation");
        GUILayout.EndHorizontal();
        */

        /*
        GUILayout.Space(30);

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        grid.show_grid = GUILayout.Toggle(grid.show_grid, "Show Grid");
        GUILayout.EndHorizontal();
                */
        GUILayout.EndVertical();


		GUILayout.EndArea();
	}

	void PropogationOptions()
	{
		//Rect box = new Rect(left_panel_width , 0 , Screen.width - left_panel_width , Screen.height);
		GUI.Box(box , "Propogation");
		GUILayout.BeginArea(box);
		
		GUILayout.EndArea();
	}
	void VideoOptions()
	{
		//Rect box = new Rect(left_panel_width , 0 , Screen.width - left_panel_width , Screen.height);
		GUI.Box(box , "Video");
		GUILayout.BeginArea(box);


		GUILayout.BeginVertical();
		GUILayout.Space(60);

        //Fullscreen
		GUILayout.BeginHorizontal();
		GUILayout.Space(20);
		Screen.fullScreen = GUILayout.Toggle(Screen.fullScreen , "Fullscreen");
		GUILayout.EndHorizontal();

        GUILayout.Space(30);

        //FOV
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label("Field Of View: ");
        Camera.main.fieldOfView = GUILayout.HorizontalSlider(Camera.main.fieldOfView, 10, 90, GUILayout.Width(200));
        if (GUILayout.Button("Default"))
        {
            Camera.main.fieldOfView = 31;
        }
        GUILayout.Space(20);
        GUILayout.EndHorizontal();


        GUILayout.Space(30);

        //Anti-Aliasing
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        aa_new = GUILayout.Toggle(aa, "Anti-Aliasing");
        GUILayout.EndHorizontal();

        GUILayout.Space(30);

        //Enhanced Contrast
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        enhanced_contrast_new = GUILayout.Toggle(enhanced_contrast, "Enhanced Contrast");
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

		//Edge Detection 
		GUILayout.BeginHorizontal();
		GUILayout.Space(20);
		edge_detection_new = GUILayout.Toggle(edge_detection, "Edge Detection");
		GUILayout.Space(20);
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
		
		GUILayout.EndArea();
	}
	void LeftPanel()
	{
		Rect box = new Rect(side_margin , vert_margin , left_panel_width , Screen.height - (vert_margin*2));
		GUI.Box(box , "");
		GUILayout.BeginArea(box);

		if(GUILayout.Button("General"))
		{
			current = Switch.General;
		}
		if(GUILayout.Button("Video"))
		{
			current = Switch.Video;
		}
        if (GUILayout.Button("Control"))
        {
            current = Switch.Control;
        }
        /*
        if (GUILayout.Button("Propogation"))
        {
            current = Switch.Propogation;
        }
        */
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("Close"))
		{
			CloseOptions();

		}

		GUILayout.EndArea();
	}

	void OnGUI()
	{
		if(show)
		{
            box = new Rect(side_margin + left_panel_width, vert_margin, Screen.width - (left_panel_width + side_margin*2), Screen.height - (vert_margin * 2));
			LeftPanel();
			switch (current) {
			case Switch.General:
				GeneralOptions();
				break;
			case Switch.Propogation:
				PropogationOptions();
				break;
			case Switch.Video:
				VideoOptions();
				break;
            case Switch.Control:
                ControlOptions();
                break;
			}
		}


	}


}
