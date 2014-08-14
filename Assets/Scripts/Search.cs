using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Search : MonoBehaviour
{

    public float categories_column_size = 120.0f;

    public bool showWindow = false;

    private bool show_categories = false;
    private bool show_distinct_list = false;

    private string category = "Categoy";
    private string search_string = "";
    private string _default = "<Enter Search>";

    private Vector2 scrollPosition = new Vector2(0, 0);
    private Vector2 scrollPosition_L = new Vector2(0, 0);

    private Color search_color;

    public static List<string> categories;
    private List<string> distinct_vals;
    public  static List<string> all_distinct_fields;
    private List<string> searchGUIDs = new List<string>();

    public Select selection;

    public Viewer viewer;

    public GUISkin search_skin;

    void Awake()
    {
        search_color = Color.magenta;
    }
    void Start()
    {
        all_distinct_fields = new List<string>();
        all_distinct_fields.Add("LINE_NUMBER_");
        all_distinct_fields.Add("SPEC_");
        all_distinct_fields.Add("TYPE_");
        all_distinct_fields.Add("CATEGORY");
        //StartCoroutine(GetColumns());


        categories = new List<string>();
        categories.Add("SIZE_");
        categories.Add("TAG_");
        categories.Add("SHORT_DESC_");
        categories.Add("LONG_DESC_");
        categories.Add("SPEC_");
        categories.Add("LINE_NUMBER_");
        categories.Add("DB_CODE_");
        categories.Add("CATEGORY");
        categories.Add("TYPE_");
        categories.Add("BOM_TYPE");
        categories.Add("DWG_NAME");
        categories.Add("DESIGNPSI_");
        categories.Add("OPERPSI_");
        categories.Add("MAXPSI_");
        categories.Add("SPECPRESS_");




    }

    public void ToggleWindow()
    {
        showWindow = !showWindow;
        scrollPosition = new Vector2(0, 0);
    }

    void OnGUI()
    {
        if (showWindow)SearchWindow();
           
    }

    private void SearchWindow()
    {

        GUI.skin = search_skin;
        //Categories Column
        GUILayout.BeginArea(new Rect(Screen.width - categories_column_size, 30, categories_column_size ,  Screen.height - 40));
        GUILayout.BeginVertical();
        if (GUILayout.Button(category))
        {
            show_categories = !show_categories;
        }
        if (show_categories)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (string str in categories)
            {
                if (GUILayout.Button(str.Replace('_', ' ').Trim() , GUILayout.Width(categories_column_size - 10)))
                {
                    category = str;
                    show_categories = false;
                    if (all_distinct_fields.Contains(category))
                    {
                        StartCoroutine(GetDistinctFieldVals(category));
                    }
                    
                }
            }
            GUILayout.EndScrollView();
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
        



        //Search bar and buttons
        GUILayout.BeginArea(new Rect(0 , 30 , Screen.width - categories_column_size , Screen.height - 35));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (all_distinct_fields.Contains(category))
        {
            if(show_distinct_list)
            {
                scrollPosition_L = GUILayout.BeginScrollView(scrollPosition_L);
                foreach (string str in distinct_vals)
                {
                    if (GUILayout.Button(str.Trim(), GUILayout.Width(categories_column_size * 1.75f)))
                    {
                        search_string = str;
                        show_distinct_list = false;
                    }
                }
                GUILayout.EndScrollView();
            }
            else
            {
                if (GUILayout.Button(search_string, GUILayout.Width(categories_column_size * 1.5f)))
                {
                    show_distinct_list = true;
                }
            }

        }

        GUI.SetNextControlName("search_string");
         search_string = GUILayout.TextField(search_string, GUILayout.Width(190));
         if (UnityEngine.Event.current.type == EventType.Repaint)
         {
             if (GUI.GetNameOfFocusedControl() == "search_string")
             {
                 if (search_string == _default) search_string = "";
             }
             else
             {
                 if (search_string == "") search_string = _default;
             }
         }
      
        if (GUILayout.Button("Clear", GUILayout.Width(70)))
        {
            ClearSearch();
        }
        else if (GUILayout.Button("Search", GUILayout.Width(70)))
        {
            if (!search_string.Equals("") && !category.Equals("Category"))
            {
                try
                {
                    selection.Clear();
                    StartCoroutine(HighlightSearch());
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

   
    }
    private void ClearSearch()
    {
        selection.Clear();
        viewer.model.DefaultView();
        searchGUIDs.Clear();
    }


    private IEnumerator HighlightSearch()
    {

        //Fade and Gray everything
        viewer.model.FadeAndGray();

        //Get search matching guids
        WWWForm form = new WWWForm();
        form.AddField("sa", "search_like");
        form.AddField("category", category);
        form.AddField("searchString", search_string);
        form.AddField("project", SafeSim.PROJECT);

        WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
        yield return www;

        List<string> results = new List<string>(www.text.Split('|'));

        foreach (string result_guid in results)
        {
            if (!viewer.model.GUIDs.ContainsKey(result_guid))
            {
                continue;
            }

            SSObject guid = viewer.model.GUIDs[result_guid].GetComponent<SSObject>();

            guid.UnFade();
            guid.Tween_Mat_Color(search_color , true);

            searchGUIDs.Add(result_guid);

        }
    }

    IEnumerator GetColumns()
    {
        categories = new List<string>();
        WWWForm form = new WWWForm();
        form.AddField("sa", "get_columns");
        form.AddField("project", SafeSim.PROJECT);
        WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
        yield return www;
        if (www.error == null)
        {
            string[] str = www.text.Split('|');
            foreach (string s in str)
            {
                if (s.Length > 0 && !s.Equals("GUID_") && !s.Equals("GUID"))
                {
                    categories.Add(s);
                }
            }

        }
        else
        {
            Debug.Log("WWW Error: " + www.error);

        }
    }

    public static Dictionary<string, string> states = new Dictionary<string, string>();
    public static IEnumerator GetStates()
    {
        NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerON"));

        states.Clear();
        //categories = new List<string>();
        WWWForm form = new WWWForm();
        form.AddField("sa", "get_states");
        form.AddField("project", SafeSim.PROJECT);
        form.AddField("asset", SafeSim.ASSET.Substring(3));
        WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
        yield return www;

        if (www.error == null)
        {
            string[] str = www.text.Split('|');
            for (int i = 0; i < str.Length - 1; i += 2)
            {
                states.Add(str[i], str[i + 1]);
            }

        }
        else
        {
            Debug.Log("WWW Error: " + www.error);

        }

        NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerOFF"));
    }

    IEnumerator GetDistinctFieldVals(string field)
    {
        distinct_vals = new List<string>();
        WWWForm form = new WWWForm();
        form.AddField("project", SafeSim.PROJECT);
        search_string = "Select";
        form.AddField("field", field);
        form.AddField("sa", "get_distinct_field");
        form.AddField("asset", SafeSim.ASSET.Substring(3));

        WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
        yield return www;
        if (www.error == null)
        {
            foreach (string s in www.text.Split('|')) 
                if (s.Trim().Length > 0) distinct_vals.Add(s);
        }
        else  Debug.Log("WWW Error: " + www.error);

    }
}