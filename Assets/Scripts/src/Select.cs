using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class Select : MonoBehaviour
{
	
	//Ray
	private Ray ray;	
	public LayerMask layermask;
	
	Shader selection_shader;
	Shader default_shader ;
	Material selection_material;

	Color select_color = new Color(0.196f,0.196f,0.650f,0.25f);

    class EdgePackage
    {
        public string dwg_name;
        public string selectedguid_fullpath;
        public List<string> selectedguids = new List<string>();
    }

	//Selection storage
	private GameObject selectedGUID;
	private List<GameObject> selectedGUIDs = new List<GameObject>();
	public List<GameObject> fadedGUIDs = new List<GameObject>();
	public List<GameObject> hiddenGUIDs = new List<GameObject>();
	
	//Cursor
	public bool showCursor;
	public Texture cursorImage;
	private float cursorX , cursorY , pointerY = 0.0f;
	public float dPad_sensitivity = 6.0f;
	
	RaycastHit hit = new RaycastHit();
	
	private SSObject hover_object;
	
	
	private Rect rightClickRect = new Rect(0,0,75,80);
	public bool showRightClick = false;	

	Vector3 mousePos = Vector3.zero;

	//External
	public Viewer viewer;
	SSModel model;

    public ControlManager cm;
	
	//Initializer
	void Start ()
	{	
		selectedGUID = null;
		selectedGUIDs = new List<GameObject>();
		selection_shader = Shader.Find("Outlined/Silhouetted Diffuse");
		default_shader = Shader.Find("Diffuse");
		selection_material = Resources.Load("Selection Material") as Material;
		ray = new Ray();
        //NotificationCenter.DefaultCenter().AddObserver(this, "OnModelLoaded");
        model = viewer.model;
	}
	

	public GameObject GetSelectedGUID()
	{
		return selectedGUID;
	}	


	//Return list of selected objects
	public List<GameObject> GetSelection()
	{
		return selectedGUIDs;
	}
	/// <summary>
	/// Recieves a string with one or more guids
	/// attempts to find their corresponding object and set the new selection
	/// </summary>
	/// <param name='str'>
	/// Single string of guids seperated by '|'
	/// </param>
	public void SetSelection(string str )
	{
		Clear();
		string[] guidNames = str.Split('|');
		foreach(string guid_name in guidNames)
		{
            if(model.GUIDs.ContainsKey(guid_name))
            {
                Add(model.GUIDs[guid_name]);
            }
            else if(model.PARENT_GUIDs.ContainsKey(guid_name))
            {
                GameObject parent = model.PARENT_GUIDs[guid_name];
                foreach (MeshRenderer child in parent.GetComponentsInChildren<MeshRenderer>())
                {
                    Add(child.gameObject);
                }
            }

			try
			{
			}
			catch(Exception)
			{
				Debug.LogError(guid_name + " is not included in scene model.");
			}
			
            /*
			//Check and try for insulation///////////////////////
			if(false)///////////////////////////////////////////!!!!!!!!!!!!!
			{
				try
				{
					GameObject guid;
					if(guid_name.Contains("_I"))
					{
						guid = model.GUIDs[guid_name.Substring(0,guid_name.Length-2)];
					}
					else
					{
						guid = model.GUIDs[guid_name + "_I"];
					}
					Add(guid);
					
				}
				catch(Exception)
				{
					Debug.LogError(guid_name + " is not included in scene model.");
				}
			}
             */
			
		}
		
		
	}


    public void SetSelection(List<string> gos)
    {
        if (Input.GetButton("Shift") || Input.GetButton("RB") || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {

        }
        else
        {
            Clear();
        }

        for (int i = 0; i < gos.Count; i++)
        {
            try
            {
                Add(model.GUIDs[gos[i]]);
            }
            catch (Exception e)
            {

            }
        }

    }
    public void RequestSafeSimSelection(string str)
    {

        ArrayList ar = new ArrayList();

        foreach (GameObject go in selectedGUIDs)
            ar.Add(go.name);

        Application.ExternalCall("ReturnSafeSimSelection", SafeSim.ASSET, ar);


    }
	

	//Draw the cursor
	void OnGUI()
	{
        /*
        if(GUI.Button(new Rect(60,60,75,25) , "Test"))
        {
            SetSelection("3EF9E3E7-9037-43DC-88BF-F352E7016438");
        }
        */
		if(showCursor)
		{
			GUI.Label(new Rect(cursorX, cursorY ,48,48), cursorImage);
		}
		if(showRightClick)
		{
			//GUI.Box(rightClickRect , "" , contextMenuStyle);
			GUILayout.BeginArea(rightClickRect);
			RightClickMenu();
			GUILayout.EndArea();
			//rightClickRect = GUILayout.Window(19 , rightClickRect , RightClickMenu , "");
			
		}
		//Debug.DrawRay(Camera.main.transform.position , Camera.main.ScreenPointToRay(Input.mousePosition).direction , Color.cyan);
	}
	
    public void OnModelLoaded(Notification n)
    {
        try
        {
            model = viewer.model;
        }
        catch(Exception e)
        {
        }
        

    }
	
	//For every frame...
	void Update ()
	{
		
		hit = new RaycastHit();
		/*
		//If any select button pressed draw ray from screen to object
		if(Input.GetButtonDown("Select_Xbox"))
		{
			ray = Camera.main.ScreenPointToRay( new Vector3(cursorX , pointerY, 10));					
		}
		else*/
        if(Input.GetButtonDown("Select") && GUIUtility.hotControl == 0)
		{		
			showRightClick = false;
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			//if the ray hit something (user clicked on an object)
			if (Physics.Raycast (ray , out hit , Mathf.Infinity , layermask)) 
			{		
				string name = hit.collider.gameObject.name;
				if(name.Contains("MeshPart"))
				{
					name = name.Substring(0,name.IndexOf('_'));
				}
				else if(SafeSim.dupRegex.IsMatch(name))
				{
					name = name.Substring(0 , name.IndexOf(" "));
				}
				GameObject guid = model.GUIDs[name];
				
				//...while holding shift
				if(Input.GetButton("Shift") || Input.GetButton("RB") || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
				{
					//...and the object was already part of the selection
					if(selectedGUIDs.Contains(guid))
					{
						//...then remove that object from the selection
						Remove(guid);
						selectedGUID = null;
					}
					else
					{
						//..otherwise add the object to selection.
						Add(guid);
						//External Call to EDGE360

                        if (PressurePropogation.use_contact_points)
                        {
                            Application.ExternalCall("SafeSimSelect", SafeSim.ASSET, GetGameObjectPath(selectedGUID));
                        }
                        else
                        {
                            Application.ExternalCall("SafeSimSelect", GetGameObjectPath(selectedGUID));
                        }
					}
					
				}
				else //If not holding shift,
				{
					
					//and clicked on selected item, clear selection
					if(selectedGUIDs.Contains(guid))
					{
						Clear();
					}
					else
					{
						//and clicked on new item, start new selection with clicked object
						Clear();
						Add(guid);
						
						//External Call to EDGE360
                        if (PressurePropogation.use_contact_points)
                        {
                            Application.ExternalCall("SafeSimSelect", SafeSim.ASSET , GetGameObjectPath(selectedGUID));
                        }
                        else
                        {
                            Application.ExternalCall("SafeSimSelect", GetGameObjectPath(selectedGUID));
                        }
					

						//Post Notification of single selection
						//NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "OnSelect", selectedGUID));
						
					}
					
					
				}
			}
			else
			{
                //clear selection if ray hits no objects.
                if (Input.GetButton("Shift") || Input.GetButton("RB") || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {

                }
                else
                {
                    Clear();
                }			
				
			}		
		}
		else if(Input.GetMouseButtonDown(1))
		{
			if(showRightClick)
				showRightClick = false;
			mousePos = Input.mousePosition;
		}
		else if(Input.GetMouseButtonUp(1))
		{
			if(mousePos == Input.mousePosition)
			{
				ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast (ray , out hit , Mathf.Infinity , layermask)) 
				{
					
					string name = hit.collider.gameObject.name;
					if(name.Contains("MeshPart"))
					{
						name = name.Substring(0,name.IndexOf('_'));
					}
					else if(SafeSim.dupRegex.IsMatch(name))
					{
						name = name.Substring(0 , name.IndexOf(" "));
					}
					GameObject guid = model.GUIDs[name];					
					
					if(Input.GetButton("Shift") || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
					{
						
					}
					else
					{
						Clear();
					}
					
					Add(guid);
					
					showRightClick = true;
					rightClickRect.x = mousePos.x;
					rightClickRect.y = Screen.height - mousePos.y;
					rightClickRect.width = 70;
					rightClickRect.height = 60;
				}
				else
				{
					Clear();
					showRightClick = true;
					rightClickRect.x = mousePos.x;
					rightClickRect.y = Screen.height - mousePos.y;
					rightClickRect.width = 70;
					rightClickRect.height = 50;
				}
			}
		}
		else
		{
			//Hover
			try
			{
				ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			}
			catch (Exception e)
			{
				
			}			
			
			
			if (Physics.Raycast (ray , out hit , Mathf.Infinity , layermask)) 
			{	
				GameObject go =hit.collider.gameObject;
				//if(selectedGUIDs.Contains(go))return;
				
				if(hover_object != null)
				{
					hover_object.UnHighlight();
					hover_object = null;
				}
				
				SSObject sso = go.GetComponent<SSObject>();
				if(sso == null)sso = go.transform.parent.GetComponent<SSObject>();
				sso.Highlight();
				hover_object = sso;
				
				
			}
			else
			{
				if(hover_object != null)
				{
					hover_object.UnHighlight();
					hover_object = null;
				}
			}
		}
		
		//Update the cursor via Xbox D-pad
		if(showCursor)
		{
			UpdateCursorPos();
		}

	}//end Update
	public GUIStyle contextMenuStyle;
	private void RightClickMenu()
	{
		GUILayout.BeginVertical();
		if(!selectedGUID)
		{
			if(GUILayout.Button("Unfade All" , contextMenuStyle))
			{
				UnFadeAll();
				showRightClick = false;
			}
			if(GUILayout.Button("UnHide All" , contextMenuStyle))
			{
				UnHideAll();
				showRightClick = false;
			}
		}
		else
		{
			if(fadedGUIDs.Contains(selectedGUID))
			{
				if(GUILayout.Button("Unfade" , contextMenuStyle))
				{
                    UnFade();
					showRightClick = false;
				}
			}
			else
			{
				if(GUILayout.Button("Fade" , contextMenuStyle))
				{
                    Fade();
					showRightClick = false;
				}
			}	
			if(GUILayout.Button("Hide" , contextMenuStyle))
			{
                Hide();
				showRightClick = false;
			}
            if (GUILayout.Button("Pivot Point", contextMenuStyle))
            {
                cm.MovePivot(selectedGUID.transform.position);
                showRightClick = false;
            }
            if (GUILayout.Button("Bounds", contextMenuStyle))
            {
                for (int i = 0; i < selectedGUIDs.Count; i++)
                {
                    selectedGUIDs[i].GetComponent<bbox_BoundBox>().enabled = !selectedGUID.GetComponent<bbox_BoundBox>().enabled;
                }
                showRightClick = false;
            }
		}
		GUILayout.EndVertical();
		
	}
	
	/*
	private void ClearHover()
	{
					for(int i = 0; i < hover_colors.Length; i++)
					{
						Color c = (hover_colors[i]);
						c.a = hover_object.renderer.material.color.a;
						hover_object.renderer.materials[i].color = c;
					}			
		
		
	}
	
	private void Hover(GameObject hit)
	{
		
					hover_object = hit;
					Color[] mat_Colors = new Color[hover_object.renderer.materials.Length];
						
					for(int i = 0; i < hover_object.renderer.materials.Length ; i++)
					{
							mat_Colors[i] = hover_object.renderer.materials[i].color;	
					}
					hover_colors = mat_Colors;
					
					Color tmp_c = new Color(0.505f , 0.505f , 0.827f , 1.0f);
					foreach(Material material in hover_object.renderer.materials)
					{
						material.color = tmp_c;
					}		
	}
	
	*/
	
	/// <summary>
	/// Updates the cursor position.
	/// </summary>
	private void UpdateCursorPos()
	{
		// (0,0) is NW Corner for GUI coordinates
		cursorX += Input.GetAxis("Dpad X") * dPad_sensitivity;
		cursorY -= Input.GetAxis("Dpad Y") * dPad_sensitivity;
		pointerY += Input.GetAxis("Dpad Y") * dPad_sensitivity;
		
		//Clamp cursor within bounds
		if(cursorX > Screen.width)
		{
			cursorX = Screen.width;
		}
		else if(cursorX < 0 )
		{
			cursorX = 0;
		}
		if(cursorY > Screen.height) 
		{
			cursorY = Screen.height ;
			pointerY = 0;
		}
		else if(cursorY < 0 )
		{
			cursorY = 0;
			pointerY = Screen.height ;
		}
	}
	
	/// <summary>
	/// Add the specified gameobject to selection.
	/// Change color to indicate selection.
	/// </summary>
	/// <param name='go'>
	/// GameObject to be selected
	/// </param>
	public void Add(GameObject  guid)
	{
		//Update selectedGUID
		try{		
			selectedGUID = guid;
			selectedGUIDs.Add(guid);
            //guid.GetComponent<HighlightableObject>().ConstantOnImmediate(select_color);

			guid.GetComponent<SSObject>().Set_Mat_Color(select_color);
			
			
			//GetComponent<InfoPanel>().SetData(selectedGUID.name);
		}catch(NullReferenceException e)
		{
			//Ignore	
		}
		catch(Exception e)
		{
			Debug.LogError(guid.name + " : " + e.Message);
		}
		
	}
	
	//Remove single object from selection
	public void Remove(GameObject guid)
	{
		try{
			guid.GetComponent<SSObject>().Reset_Colors();
            //guid.GetComponent<HighlightableObject>().ConstantOff();
			selectedGUIDs.Remove(guid);		
		}catch(Exception e)
		{
			Debug.LogError(guid.name + " : " + e.Message);
		}
		
	}
	
	
	/// <summary>
	/// Deselect this instance. Revert all colors
	/// </summary>/
	public void  Clear()
	{
        try
        {
            iTween.Stop();
        }
        catch (Exception e)
        {

        }
        
		
		foreach(GameObject guid in new List<GameObject>(selectedGUIDs))
		{
			Remove(guid);
		}	
		
		try
		{
			GetComponent<InfoPanel>().ClearData();
		}
		catch(NullReferenceException e)
		{
			//Ignore	
		}
		//NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "OnDeselect"));
		selectedGUIDs = new List<GameObject>();
		selectedGUID = null;
		
	}
	public static string GetGameObjectPath(GameObject obj)
	{
		string path = obj.name;
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			if(obj.name.Contains("Clone"))break;
			path += "|" + obj.name;
		}
		return path;
	}
	
	//Outer selection
	public void FadeSelected( string str )
	{
		SetSelection(str);
		Fade ();
	}
	public void UnfadeSelected( string str )
	{
		SetSelection(str);
		UnFade ();
	}
	public void HideSelected( string str )
	{
		SetSelection(str);
		Hide ();
	}
	public void UnHideSelected( string str )
	{
		SetSelection(str);
		UnHide ();
	}
	public void UnselectSelected( string str )
	{
		string[] guidNames = str.Split('|');
		foreach(string guid_name in guidNames)
		{	
			try
			{
				GameObject guid = model.GUIDs[guid_name];
				Remove(guid);
			}
			catch(Exception)
			{
				Debug.LogError(guid_name + " is not included in scene model.");
			}
			
            /*
			//Check and try for insulation
			if(false)/////////////////!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			{
				try
				{
					GameObject guid;
					if(guid_name.Contains("_I"))
					{
						guid = model.GUIDs[guid_name.Substring(0,guid_name.Length-2)];
					}
					else
					{
						guid = model.GUIDs[guid_name + "_I"];
					}
					
					Remove(guid);
					
				}
				catch(Exception)
				{
					Debug.LogError(guid_name + " is not included in scene model.");
				}
			}
             */
			
		}
	}
	
	public void UnselectAll(string ignore)
	{
		Clear ();
	}
	
	public void Fade()
	{
		foreach(GameObject guid in selectedGUIDs)
		{
			guid.GetComponent<SSObject>().Fade();
			fadedGUIDs.Add(guid);
		}
	}
	public void UnFade()
	{
		foreach(GameObject guid in selectedGUIDs)
		{
			guid.GetComponent<SSObject>().UnFade();
			fadedGUIDs.Remove(guid);
		}
	}
	public void UnFadeAll()
	{
		foreach(GameObject guid in model.GUIDs.Values)
		{
			if(guid.GetComponent<SSObject>().isFaded)
			{
				guid.GetComponent<SSObject>().UnFade();
				fadedGUIDs.Remove(guid);
			}
		}
	}	
	public void Hide()
	{
		foreach(GameObject guid in selectedGUIDs)
		{
			guid.GetComponent<SSObject>().Hide();
			hiddenGUIDs.Add(guid);
		}
	}	
	public void UnHide()
	{
		foreach(GameObject guid in selectedGUIDs)
		{
			guid.GetComponent<SSObject>().UnFade();
			hiddenGUIDs.Remove(guid);
		}
	}	
	public void UnHideAll()
	{
		foreach(GameObject guid in model.GUIDs.Values)
		{
			if(guid.GetComponent<SSObject>().isHidden)
			{	
				guid.GetComponent<SSObject>().UnHIde();
                guid.layer = (int) SafeSim.Layers.model;
				hiddenGUIDs.Remove(guid);
			}
		}
	}	
	
	
	
	
	
	
	
	
	
	
}

