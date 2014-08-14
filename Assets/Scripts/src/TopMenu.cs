using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TopMenu : MonoBehaviour 
{

	//GUI
	public GUISkin top_menu_skin;
	public GUIStyle icon_style;
    public GUIStyle tooltip_style;

	public Texture home_icon;
	public Texture search_icon;
    public Texture screenshot_icon;
    public Texture orbiter_icon;
    public Texture avatar_icon;
    public Texture paint_icon;
	public Texture pressure_icon;
    public Texture ovr_icon;

    public static float menu_bar_height = 25.0f;

    //private bool show_top_menu = true;
    private Vector2 highlightScrollPos = Vector2.zero;
    private enum MenuItems { None, SS, Views, Compare , Highlight , States}
    private MenuItems selectedMenuItem;

    private OptionsScreen options;

	bool viewStates = false;
	bool operatingStates = false;



    //References
    public Paint paint;
    public Search search;
	public Viewer viewer;
	public Select selection;
	public Highlight highlight;
    public Screenshot screenshot;

    public ViewState viewState;
    public CreateState createState;

	public ViewPressureState viewPressureState;
	public CreatePressureState createPressureState;

    public ControlManager control_manager;
    public CompareVersions compare_versions;
	public PressurePropogation pressure_propogation;


	void Start()
	{
		selectedMenuItem = MenuItems.None;
		options = GetComponent<OptionsScreen>();

        StartCoroutine(Search.GetStates());
		StartCoroutine(ViewPressureState.GetPressureStates());
	}

	public void CloseAll()
	{
		selectedMenuItem = MenuItems.None;
		PressurePropogation.showWindow = false;
	}


	void OnGUI()
	{
        if (SafeSim.prompt) return;
        GUI.skin = top_menu_skin;
        GUI.Box(new Rect(0, 0, Screen.width, menu_bar_height), "");

        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        if (GUILayout.Button("SafeSim"))
        {
            if (selectedMenuItem == MenuItems.SS) selectedMenuItem = MenuItems.None;
            else selectedMenuItem = MenuItems.SS;
        }
        else if (selectedMenuItem == MenuItems.SS)
        {
            if (GUILayout.Button("Menu"))
            {
                Application.LoadLevel("main");
                selectedMenuItem = MenuItems.None;
            }
            if (GUILayout.Button("Options"))
            {
                selectedMenuItem = MenuItems.None;
                options.ShowOptions();
                this.enabled = false;
            }
        }
        GUILayout.EndVertical();


        //----------------------------------------------------


        //Compare
        //----------------------------------------------------
        GUILayout.BeginVertical();
        if (GUILayout.Button("Compare"))
        {
            if (selectedMenuItem == MenuItems.Compare) selectedMenuItem = MenuItems.None;
            else selectedMenuItem = MenuItems.Compare;
        }
        else if (selectedMenuItem == MenuItems.Compare)
        {
            foreach (string version in compare_versions.versions)
            {
                if (GUILayout.Button(version))
                {
                    selectedMenuItem = 0;

                    if (PressurePropogation.showWindow)
                    {
                        NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", "Cannot use 'Compare Versions' functionality while Pressure Propogation is active."));
                        return;
                    }
                    else if (search.showWindow)
                    {
                        NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", "Cannot use 'Compare Versions' functionality  while using 'Search'"));
                        return;
                    }
                    else
                    {
                        compare_versions.LoadAsset(version);
                    }

                }
            }
        }
        GUILayout.EndVertical();

        //----------------------------------------------------



        //Highlight
        //----------------------------------------------------
        GUILayout.BeginVertical();
        if (GUILayout.Button("Highlight" ))
        {
            if (compare_versions.isActive)
            {
                NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", "Cannot highlight while using 'Compare Versions' funtionality."));
            }
            else if (createState.enabled || viewState.enabled) NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", "Cannot highlight while using 'States' funtionality."));
            else if (selectedMenuItem == MenuItems.Highlight) selectedMenuItem = MenuItems.None;
            else selectedMenuItem = MenuItems.Highlight;
        }
        else if (selectedMenuItem == MenuItems.Highlight)
        {


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













            highlightScrollPos = GUILayout.BeginScrollView(highlightScrollPos);

            foreach (string s in include)
            {
                //if (omit.Contains(s)) continue;
                //else if (s.StartsWith("PT") || s.StartsWith("USER_SHAPE_PT") || s.StartsWith("INSULATION_")) continue; 
                if (GUILayout.Button(UppercaseFirst(s)))
                {
                    selectedMenuItem = MenuItems.None;
					highlight.DoHighlight(s);
                }
            }
            //custom
            /*
            if (GUILayout.Button("STATES"))
            {
                selectedMenuItem = MenuItems.None;
                highlight.category = "STATES";
                StartCoroutine(Search.GetStates());
                viewer.model.FadeAndGray();
                Highlight.show = true;
                
            }*/
            GUILayout.EndScrollView();

        }
        GUILayout.EndVertical();


        //States
        //----------------------------------------------------
        GUILayout.BeginVertical();
        if (GUILayout.Button("States"))
        {
            if (compare_versions.isActive)
            {
                NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", "Cannot use States while using 'Compare Versions' funtionality."));
            }
            else if (selectedMenuItem == MenuItems.States) selectedMenuItem = MenuItems.None;
            else selectedMenuItem = MenuItems.States;
        }
        else if (selectedMenuItem == MenuItems.States)
        {
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();

			if(viewStates ? GUILayout.Button("View" , tooltip_style , GUILayout.Height(GUI.skin.button.fixedHeight)) : GUILayout.Button("View"))
			{
				operatingStates = false;
				viewStates = true;
			}
			if(operatingStates ? GUILayout.Button("Operating" , tooltip_style,GUILayout.Height(GUI.skin.button.fixedHeight) ) : GUILayout.Button(("Operating")))
			{
				viewStates = false;
				operatingStates = true;
			}
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			if(viewStates)
			{
				if (GUILayout.Button("CREATE"))
				{
					highlight.Close();
					viewState.enabled = false;
					createState.enabled = true;
					createState.GetComponentInChildren<ColorPicker>().enabled = true;
					viewer.model.FadeAndGray();
					selectedMenuItem = MenuItems.None;
					viewStates = false;
					pressure_propogation.GetComponent<CreatePressureState>().enabled = false;
				}
				foreach (KeyValuePair<string, string> kvp in Search.states)
				{
					if (GUILayout.Button(kvp.Value))
					{
						highlight.Close();
						createState.Close();
						viewState.enabled = true;
						viewState.LoadStateById(kvp.Key , kvp.Value);
						selectedMenuItem = MenuItems.None;
						viewStates = false;
					}
				}

			}
			else if(operatingStates)
			{
				GUILayout.Space(GUI.skin.button.fixedHeight);
				if (GUILayout.Button("CREATE"))
				{
					highlight.Close();
					viewState.enabled = false;
					createPressureState.enabled = false;
					//createState.GetComponentInChildren<ColorPicker>().enabled = false;
					viewer.model.FadeAndGray();
					selectedMenuItem = MenuItems.None;
					pressure_propogation.GetComponent<CreatePressureState>().enabled = true;
					operatingStates = false;
				}
				if (GUILayout.Button("Default"))
				{
					viewer.DefaultView();
					selectedMenuItem = MenuItems.None;
					operatingStates = false;
				}
				if (GUILayout.Button("Normal"))
				{
					viewer.NormalView();
					selectedMenuItem = MenuItems.None;
					operatingStates = false;
				}
				if (GUILayout.Button("Current"))
				{
					viewer.CurrentView();
					selectedMenuItem = MenuItems.None;
					operatingStates = false;
				}
				foreach (KeyValuePair<string, string> kvp in ViewPressureState.pressureStates)
				{
					if (GUILayout.Button(kvp.Value))
					{
						highlight.Close();
						createPressureState.Close();
						viewPressureState.enabled = true;
						viewPressureState.LoadStateById(kvp.Key , kvp.Value);
						selectedMenuItem = MenuItems.None;
						viewStates = false;
					}
				}


			}

			GUILayout.EndVertical();
			GUILayout.EndHorizontal();







        }
        GUILayout.EndVertical();


        //---------------------------Right Side Icons--------

        GUILayout.FlexibleSpace();


		//Pressure
		if (GUILayout.Button(new GUIContent ("" ,pressure_icon ,"Pressure" ), icon_style))
		{
			selectedMenuItem = MenuItems.None;
			if (compare_versions.isActive)
			{
				NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", "Cannot use Pressure Propogation while 'Compare Versions' functionality is active"));
				return;
			}
			else if(search.showWindow)
			{
				NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", "Cannot use Pressure Propogation while using 'Search'"));
				return;
			}
			else if(createState.enabled || viewState.enabled)
			{
				NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", "Cannot use Pressure Propogation while using 'States'"));
				return;
			}
			else
			{
                if (PressurePropogation.showWindow)
				{
					pressure_propogation.Stop();
                    PressurePropogation.showWindow = false;
				}
				else
				{
					StartCoroutine(PressurePropogation.GetData());
                    PressurePropogation.showWindow = true;
				}
				
			}
		}


        /*
        if (GUILayout.Button(new GUIContent("", paint_icon, "Paint"), icon_style))
        {
            paint.Toggle();
        }
         */
        if (GUILayout.Button(new GUIContent ("" ,avatar_icon ,"Avatar" ), icon_style))
        {
            control_manager.SwitchToAvatar();
        }
        if (GUILayout.Button(new GUIContent("", orbiter_icon, "Orbiter"), icon_style))
        {
            control_manager.SwitchToOrbitCam();
        }
        if (GUILayout.Button(new GUIContent("", ovr_icon, "OVR"), icon_style))
        {
            control_manager.SwitchToOVR();
        }
        if (GUILayout.Button(new GUIContent("", screenshot_icon, "Screenshot"), icon_style))
        {
            StartCoroutine(screenshot.ScreeAndSave());
        }
        if (GUILayout.Button(new GUIContent("", home_icon, "Re-Center View"), icon_style))
        {
            viewer.ReCenterView();
        }
        if (GUILayout.Button(new GUIContent("", search_icon , "Search"), icon_style))
        {
            if (PressurePropogation.showWindow)
            {
                NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", "Cannot use 'Search' functionality while Pressure Propogation is active."));
                return;
            }
            else if (compare_versions.isActive)
            {
                NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", "Cannot use 'Search' while using 'Compare Versions'"));
                return;
            }
            search.showWindow = !search.showWindow;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        
        //ToolTIp
        if(GUI.tooltip != "")
        {

            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;

            Rect rect = new Rect();


            //y
            rect.y = Screen.height - (y - 30);
            //width
            rect.width = GUI.tooltip.Length * 10;

            //x
            if (Screen.width - x < rect.width) rect.x = x - rect.width;
            else rect.x = x + 10;

            //height
            rect.height = 25;
            GUI.Box(rect, GUI.tooltip , tooltip_style);
            //GUI.Label(rect, GUI.tooltip);
        }



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


}
