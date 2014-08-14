using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SSModel : MonoBehaviour {
	
	GameObject asset_model;
	public Vector3 CENTER_POS;
	private AssetBundle bundle;

	public Dictionary<string , GameObject> GUIDs = new Dictionary<string, GameObject>();
	public Dictionary<string , GameObject> PARENT_GUIDs = new Dictionary<string, GameObject>();
	
	Color open = Color.green;
	Color closed = Color.red;
	Color check = Color.yellow;

	public enum View {Default , Normal , Current}
	public View current_view = View.Default;

	bool use_lod = false;


	public class TempData
	{
		public string GUID_ { get; set; }
		public string DESIGNPSI_ { get; set; }
		public string OPERPSI_ { get; set; }
		public string MAXPSI_ { get; set; }
		public string MINPSI_ { get; set; }
		public string DESIGNDEG_ { get; set; }
		public string OPERDEG_ { get; set; }
		public string MAXTEMP_ { get; set; }
		public string MINTEMP_ { get; set; }
		public string SPECPRESS_ { get; set; }
		
	}


	public void StartModel() 
	{
		if(SafeSim.ASSET != "")
		{
            string path;
			string job_id = SafeSim.PROJECT.Replace("PID" ,"");

            if (SafeSim.MODEL_URL.EndsWith("/"))
            {
                path = SafeSim.MODEL_URL + job_id;
            }
            else
            {
                path = SafeSim.MODEL_URL + "/" + job_id;
            }
			
			path += "/" + SafeSim.ASSET + ".unity3d" ;

			path = path.Replace(" " ,"%20");

            Debug.Log(path);

			StartCoroutine (DownloadAndCache(path));
		}
	}




    public GameObject GetModelGameObject()
    {
        return asset_model;
    }

	public void FadeAndGray()
	{
		foreach(GameObject guid in GUIDs.Values)
		{
			SSObject temp = guid.GetComponent<SSObject>();
			temp.Set_Mat_Color(Color.gray , true);
			temp.Fade();
		}
	}

	public void DefaultView()
	{
        PressurePropogation.CHECK_GUIDS.Clear();
        PressurePropogation.OPEN_GUIDS.Clear();
        PressurePropogation.CLOSED_GUIDS.Clear();
        PressurePropogation.HOLD_GUIDS.Clear();

		foreach(GameObject guid in GUIDs.Values)
		{
            try
            {
                SSObject temp = guid.GetComponent<SSObject>();
                temp.Reset_Original_Colors(); 
                guid.GetComponent<HighlightableObject>().ConstantOff();
            }
            catch (Exception e)
            {

            }


		}
		current_view = View.Default;
	}
	public void NormalView()
	{
		DefaultView();
		FadeAndGray();
		WWWForm form = new WWWForm();
		form.AddField("sa" , "get_valve_state");
		form.AddField("project" , SafeSim.PROJECT);
		form.AddField("view" , "Normal");
		form.AddField("guid" , "0");
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx" , form);
		StartCoroutine(ColorState(www));
		current_view = View.Normal;
	}
	public void CurrentView()
	{
		DefaultView();
		FadeAndGray();
		WWWForm form = new WWWForm();
		form.AddField("sa" , "get_valve_state");
		form.AddField("project" , SafeSim.PROJECT);
		form.AddField("view" , "Current");
		form.AddField("guid" , "0");
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx" , form);
		StartCoroutine(ColorState(www));
		current_view = View.Current;
	}

	public IEnumerator DownloadAndCache (string path)
	{

        NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerON"));

		// Wait for the Caching system to be ready
		while (!Caching.ready)
			yield return null;
		
		// Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache
		//using(WWW www = new WWW ( (SafeSim.url + modelPath).Replace(" " ,"%20")  ))
		path = path.Replace("_" , "-");
        path = path.Replace("MasterModel-Test", "MasterModel_Test");
		using(WWW www = WWW.LoadFromCacheOrDownload (  path  , 5))
		{
			
			yield return www;
			if (www.error != null)
			{
				Application.ExternalEval( "alert('" + www.error + "');" );	
				Debug.Log(www.url);
				//throw new Exception("WWW download had an error:" + www.error);
			}

			bundle = www.assetBundle;	//necessary
			if(path.Contains("MasterModeler"))
			{
                AsyncOperation async ;

                if (path.Contains("4X"))
                    async = Application.LoadLevelAdditiveAsync("MasterModel4X");
                else if(path.Contains("Test"))
                    async = Application.LoadLevelAdditiveAsync("MM");
                else async = Application.LoadLevelAdditiveAsync("MM");
                    
                yield return async;

                Debug.Log("Loaded MM");
                asset_model = GameObject.Find("/MasterModel");
                //asset_model.transform.Translate(new Vector3(-39.809041f, -35.087411f, -12.440000f));
			}
			else
			{
				asset_model = (GameObject)Instantiate(bundle.mainAsset);
                 
			}


            try
            {
                SaveParenets(asset_model);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            try
            {
                InitializeModel(asset_model);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            bundle.Unload(false);
            www.Dispose();
            bundle = null;

		}


        Application.ExternalCall("SetTheSSFileName", SafeSim.ASSET);

        /*
        WWWForm form = new WWWForm();
        form.AddField("sa", "create_session");
        form.AddField("project", SafeSim.PROJECT.Replace("PID", ""));
        using (WWW www = new WWW(SafeSim.URL + "get_handler.ashx" , form))
        {
            yield return www;
        }
        */

        NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerOFF"));
	}

	IEnumerator ColorState(WWW www)
	{

        NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerON"));
		

		yield return www;

		// check for errors
		if (www.error == null)
		{
			string[] str = www.text.Split('|');


			for(int i = 0 ; i < str.Length-1 ; i = i+2)
			{
				try
				{
					GameObject guid = GUIDs[str[i]]; 
					SSObject temp = guid.GetComponent<SSObject>();
					if(str[i+1] == "OPEN")
					{
						temp.UnFade();
                        temp.Tween_Mat_Color(open, true);

                        //guid.GetComponent<HighlightableObject>().ConstantOn(open);

						//temp.Set_Mat_Color(open , true);
                        PressurePropogation.OPEN_GUIDS.Add(guid);
					}
					else if(str[i+1] == "CLOSED")
					{
                        //guid.GetComponent<HighlightableObject>().ConstantOn(closed);
						temp.UnFade();
                        temp.Tween_Mat_Color(closed, true);
						//temp.Set_Mat_Color(closed , true);
                        PressurePropogation.CLOSED_GUIDS.Add(guid);
					}
					else if(str[i+1] == "CHECK")
					{
                        //guid.GetComponent<HighlightableObject>().ConstantOn(check);
						temp.UnFade();
                        temp.Tween_Mat_Color(check, true);

						//temp.Set_Mat_Color(check , true);
                        PressurePropogation.CHECK_GUIDS.Add(guid);
					}
					
				}
				catch(Exception)
				{
					if(!PARENT_GUIDs.ContainsKey(str[i]))
					{
						continue;
					}
					GameObject go = PARENT_GUIDs[str[i]];					
					if(str[i+1] == "OPEN")
					{
						MeshRenderer[] allChildren = go.GetComponentsInChildren<MeshRenderer>();
						foreach (MeshRenderer child in allChildren) 
						{	
							if(GUIDs.ContainsKey(child.gameObject.name))
							{
								GameObject guid = GUIDs[child.gameObject.name];
								//guid.GetComponent<SSObject>().UnFade();
								guid.GetComponent<SSObject>().Set_Mat_Color(open);
								if(!PressurePropogation.OPEN_GUIDS.Contains(guid))
									PressurePropogation.OPEN_GUIDS.Add(guid);
								//guid.tag = "OPEN_VALVE";
							}

						}
					}
					else if(str[i+1] == "CLOSED")
					{
						MeshRenderer[] allChildren = go.GetComponentsInChildren<MeshRenderer>();
						foreach (MeshRenderer child in allChildren) 
						{		
							GameObject  guid = GUIDs[child.gameObject.name];
							//guid.GetComponent<SSObject>().UnFade();
							
							guid.GetComponent<SSObject>().Set_Mat_Color(closed);
							if(!PressurePropogation.CLOSED_GUIDS.Contains(guid))
                            	PressurePropogation.CLOSED_GUIDS.Add(guid);
                            //guid.tag = "CLOSED_VALVE";
						}
					}
					else if(str[i+1] == "CHECK")
					{
						MeshRenderer[] allChildren = go.GetComponentsInChildren<MeshRenderer>();
						foreach (MeshRenderer child in allChildren) 
						{		
							GameObject guid = GUIDs[child.gameObject.name];
							guid.GetComponent<SSObject>().UnFade();
							
							guid.GetComponent<SSObject>().Set_Mat_Color(open);
                            PressurePropogation.CHECK_GUIDS.Add(guid);
                            //guid.tag = "CHECK_GUID";
						}
					}					
				}
				
			}
			
		} else {
			Debug.LogError("WWW Error: "+ www.error);
			
		}


        NotificationCenter.DefaultCenter().PostNotification(new Notification(null, "LoadingSpinnerOFF"));
	}

    void OnGUI()
    {
        if (PressurePropogation.showWindow)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 200, 170 , 175, 150));
            GUILayout.BeginVertical();
            foreach (KeyValuePair<string, Texture2D> distinct in distincts)
            {
                GUILayout.BeginHorizontal();
                Texture2D texture = distinct.Value;

                GUI.skin.box.normal.background = texture;
                GUILayout.Box("", GUILayout.Width(20));
                GUILayout.Label(distinct.Key);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

    }

    Dictionary<string, Texture2D> distincts = new Dictionary<string, Texture2D>();
    public Dictionary<string, Texture2D> func(Dictionary<string, Texture2D> mytd)
	{
        List<int> arr = new List<int>();
        foreach (KeyValuePair<string, Texture2D> td in mytd)
		{
			int x = 0;
            string s = td.Key.Replace("PSIG" , "").Trim();
			s = s.Replace("F" , "").Trim();
            if (s.Contains(" ")) s = s.Substring(0, s.IndexOf(' '));
			if(int.TryParse(s , out x))arr.Add(x);
			
		}
        arr.Sort();
        arr.Reverse();

        Dictionary<string, Texture2D> newdt = new Dictionary<string, Texture2D>();

        for(int i = 0; i < arr.Count ; i++)
        {
            string z = arr[i] + "";
            foreach (KeyValuePair<string, Texture2D> td in mytd)
            {
                if (td.Key.StartsWith(z))
				{
                    if (!newdt.ContainsKey(td.Key))
                    {
                        try
                        {
                            newdt.Add(td.Key, lcs[i]);
                        }
                        catch (Exception)
                        {
                            newdt.Add(td.Key, lcs[0]);
                        }
                    }
				}

            }
        }


        return newdt;

	}
	public IEnumerator ColorTemp(string field )
	{

		WWWForm form = new WWWForm();
		form.AddField("sa", "get_temps");
		form.AddField("asset", SafeSim.ASSET.Substring(3));
		form.AddField("project", SafeSim.PROJECT);
		form.AddField("field", field);
		
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
		yield return www;
	
		if(www.error == null)
		{
			List<string> results = new List<string>();
			if (www.text.Length > 0)
			{

				string[] s = www.text.Split('\n');
				foreach(string str in s)
				{
					if(str.Trim().Length > 0) results.Add(str);
				}
				//NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "ErrorMessage", results.Count + " Results found."));
			}

            distincts = new Dictionary<string, Texture2D>();
			foreach(string s in results)
			{
				string str = s.Substring(s.IndexOf('|')+1).Trim();
				if(!distincts.ContainsKey(str))
				{
					distincts.Add(str , null);
				}
			}


			distincts = func(distincts);


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

					if(val.Trim().Length == 0)continue;
				}
				catch(Exception )
				{
					continue;
				}
				
				if (GUIDs.ContainsKey(guid))
				{
					if(distincts.ContainsKey(val))
					{							
						GameObject go = GUIDs[guid];

                        if (PressurePropogation.OPEN_GUIDS.Contains(go)) continue;
                        if (PressurePropogation.CLOSED_GUIDS.Contains(go)) continue;
                        if (PressurePropogation.HOLD_GUIDS.Contains(go)) continue;
                        if (PressurePropogation.CHECK_GUIDS.Contains(go)) continue;
                        else
                        {
                            SSObject sso = go.GetComponent<SSObject>();
                            //sso.UnFade();
                            sso.Set_Mat_Color(distincts[val].GetPixel(0,0), true);
                            cnt++;
                        }
					}
				}
			}
		}
		else
		{
			Debug.LogError(www.error);
		}


	}


	/*
	Dictionary<string , TempData> tempData = new Dictionary<string, TempData>();
	Dictionary<string , Color> designPSIColors = new Dictionary<string, Color>();
	Dictionary<string , Color> designDEGColors = new Dictionary<string, Color>();
	Dictionary<string , Color> operPSIColors = new Dictionary<string, Color>();
	Dictionary<string , Color> operDEGColors = new Dictionary<string, Color>();
	public IEnumerator ColorTemp()
	{

		WWWForm form = new WWWForm();
		form.AddField("sa", "get_field");
		form.AddField("asset", SafeSim.ASSET);
		form.AddField("project", SafeSim.PROJECT);
		form.AddField("field", "DESIGNPSI_");
		
		WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
		yield return www;
		
		List<string> results = new List<string>();

		if (www.error == null && www.text.Length > 0)
		{
			tempData.Clear();
			results = new List<string>(www.text.Split('\n'));


			for(int i = 0; i < results.Count ; i++)
			{
				string[] guitemps = results[i].Split('|');

				if(guitemps.Length == 12)
				{
					TempData td = new TempData();

					td.GUID_ = guitemps[0];
					td.DESIGNPSI_ = guitemps[2];
					td.OPERPSI_ = guitemps[3];
					td.MAXPSI_ = guitemps[4];
					td.MINPSI_ = guitemps[5];
					td.DESIGNDEG_ = guitemps[6];
					td.OPERDEG_ = guitemps[7];
					td.MAXTEMP_ = guitemps[8];
					td.MINTEMP_ = guitemps[9];
					td.SPECPRESS_ = guitemps[10];

					if(!tempData.ContainsKey(td.GUID_))
					{
						tempData.Add(td.GUID_ , td);
                        if (!designPSIColors.ContainsKey(td.DESIGNPSI_))
                        {
                            designPSIColors.Add(td.DESIGNPSI_, Color.red);
                        }

						if(!designDEGColors.ContainsKey(td.DESIGNDEG_))designDEGColors.Add(td.DESIGNDEG_ , Color.red);
						if(!operPSIColors.ContainsKey(td.OPERPSI_))operPSIColors.Add(td.OPERPSI_ , Color.red);
						if(!operDEGColors.ContainsKey(td.OPERDEG_))operDEGColors.Add(td.OPERDEG_ , Color.red);
					}

				}

			}

            Debug.Log(designPSIColors.Count + " , " + designDEGColors.Count + " , " + operPSIColors.Count + " , " + operDEGColors.Count);
            designPSIColors = func(designPSIColors);
            designDEGColors = func(designDEGColors);
            operPSIColors = func(operPSIColors);
            operDEGColors = func(operDEGColors);
            Debug.Log(designPSIColors.Count + " , " + designDEGColors.Count + " , " + operPSIColors.Count + " , " + operDEGColors.Count);
            foreach (KeyValuePair<string,TempData> td in tempData)
            {
                GameObject go = GUIDs[td.Key];

                go.GetComponent<SSObject>().Set_Mat_Color(designPSIColors[td.Value.DESIGNPSI_]);
            }
		}
	}
*/
    public void SaveParenets(GameObject go)
    {
        Transform[] allChildren = go.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.GetComponent<MeshRenderer>() == null)
            {
                if (SafeSim.guidRegex.IsMatch(child.gameObject.name))
                {
                    if(!PARENT_GUIDs.ContainsKey(child.gameObject.name))
                    PARENT_GUIDs.Add(child.gameObject.name, child.gameObject);
                }
            }
        }
    }
    private void InitializeModel(GameObject go)
	{
		List<GameObject> duplicates = new List<GameObject>();
		List<GameObject> originals = new List<GameObject>();
		
		List<Vector3> centers = new List<Vector3>();
        List<Vector3> extents = new List<Vector3>();
		/*
		LODGroup lod_group = go.AddComponent<LODGroup>();
		LOD[] lods = new LOD[4];
		List<Renderer> l3r = new List<Renderer>();
		List<Renderer> l2r = new List<Renderer>();
		List<Renderer> l1r = new List<Renderer>();
		List<Renderer> l0r = new List<Renderer>();
		*/

		try
		{
			MeshRenderer[] solids = go.GetComponentsInChildren<MeshRenderer>();


			//for each solid
			foreach (MeshRenderer solid in solids)
			{

				/*
				float sz = solid.bounds.size.x * solid.bounds.size.y * solid.bounds.size.z;
				sizes.Add(sz);
				l0r.Add(solid);
				*/
				GameObject tmp = solid.gameObject;
				
				//add a collider (make clickable)
				if (tmp.GetComponent<MeshCollider>() == null)
					tmp.AddComponent<MeshCollider>();
				
				//put solid on "clickboard" layer
				tmp.layer =  (int)SafeSim.Layers.model;
				
				//Multi-gameobject part
				if(tmp.name.Contains("MeshPart"))
				{
					string s = tmp.name.Substring(0,tmp.name.IndexOf('_'));
					if (!GUIDs.ContainsKey(s))
					{
						tmp.transform.parent.gameObject.AddComponent<SSObject>();
						GUIDs.Add(tmp.transform.parent.name , tmp.transform.parent.gameObject);
					}
					else
					{
						continue;
					}
				}
				//Regular dups
				else if (SafeSim.dupRegex.IsMatch(tmp.name))
				{
					duplicates.Add(tmp);
				}
				//Normal case
				else if (!GUIDs.ContainsKey(tmp.name))
				{	
					GUIDs.Add(tmp.name, tmp);
					tmp.AddComponent<SSObject>();
                    tmp.AddComponent<HighlightableObject>();
                    tmp.AddComponent<bbox_BoundBox>();
                    tmp.GetComponent<bbox_BoundBox>().enabled = false;

					centers.Add(new Vector3(tmp.renderer.bounds.center.x , tmp.renderer.bounds.center.y , tmp.renderer.bounds.center.z ));
                    extents.Add(new Vector3(tmp.renderer.bounds.extents.x, tmp.renderer.bounds.extents.y, tmp.renderer.bounds.extents.z));
				}
				
			}
			
			//Prepare originals from dups and fix thei'r parent
			foreach(GameObject dup in duplicates)
			{
				string str = dup.name.Substring(0,dup.name.IndexOf(" "));
				if(GUIDs.ContainsKey(str))
				{
					GameObject orig = GUIDs[str];
					
					if(!originals.Contains(orig))
					{
						originals.Add(orig);
					}
					dup.transform.parent = orig.transform;
					//dup.layer = (int)Layer.Model;
				}
			}
			
			//Make empty parent and place dups as children
			foreach(GameObject orig in originals)
			{
				Destroy(orig.GetComponent<SSObject>());
				GameObject new_parent = new GameObject(orig.name);
				orig.name = orig.name + " 0";
				
				new_parent.transform.parent = orig.transform.parent.transform;
				
				orig.transform.parent = new_parent.transform;
				
				Transform[] dups = orig.GetComponentsInChildren<Transform>();
				foreach(Transform dup in dups)
				{
					dup.parent = new_parent.transform;
				}
				new_parent.AddComponent<SSObject>();
				GUIDs[new_parent.name] = new_parent;
			}


			//Calculate lod groups sizes
			/*
			float temp_min = 99999.0f;
			float temp_max = -1.0f;
			foreach(float sz in sizes)
			{
				if(sz < temp_min) temp_min = sz;
				else if(sz > temp_max) temp_max = sz;
			}

			sizes.Sort();


			float variance = Variance(solids);
			Debug.Log("Variance: " + variance + "\nMin: " + temp_min + "\nMax: " + temp_max );

			string st = "";
			foreach(float size in sizes)
			{
				st +=  size + "\n";
			}
			Debug.Log(st);
			foreach(MeshRenderer solid in solids)
			{
				float sz = solid.bounds.size.x * solid.bounds.size.y * solid.bounds.size.z;


			}
			*/

		}
		catch (Exception e)
		{
			
		}


		/*
		lods[0] = new LOD(1.0F / (1), l0r.ToArray());
		lods[1] = new LOD(1.0F / (2), l1r.ToArray());
		lods[2] = new LOD(1.0F / (3), l2r.ToArray());
		lods[3] = new LOD(1.0F / (4), l3r.ToArray());

		lod_group.SetLODS(lods);
		lod_group.RecalculateBounds();

		*/
        eMax = MaxVectors(extents);
        eMin = MinVectors(extents);
        size = new Vector3(eMax.x - eMin.x, eMax.y - eMin.y, eMax.z - eMin.z);
		CENTER_POS = AvgVectors(centers);

		NotificationCenter.DefaultCenter().PostNotification(new Notification(this, "OnModelLoaded" , CENTER_POS));
		
	}
    public IEnumerator SaveColorState(string name, string str)
    {


        WWWForm form = new WWWForm();
        form.AddField("sa", "save_color_state");
        form.AddField("project", SafeSim.PROJECT);
        form.AddField("asset", SafeSim.ASSET.Substring(3));
        form.AddField("state_name", name);
        form.AddField("state_color", "0.0,0.0,0.0");
        form.AddField("data", str);

        WWW www = new WWW(SafeSim.URL + "save_handler.ashx", form);
        yield return www;

        if (www.error == null)
        {
            SafeSim.prompt = false;
        }
        else
        {
            Debug.Log(www.error);
        }


    }

    public Vector3 eMax = Vector3.zero;
    public Vector3 eMin = Vector3.zero;
    public Vector3 size = Vector3.zero;

	public static Vector3 AvgVectors(List<Vector3> vectors)
	{
		float x = 0.0f , y = 0.0f , z = 0.0f ;
		foreach(Vector3 vector in vectors)
		{
			x += vector.x;
			y += vector.y;
			z += vector.z;
		}
		return new Vector3(x/vectors.Count , y/vectors.Count , z/vectors.Count );
	}
    public static Vector3 MaxVectors(List<Vector3> vectors)
    {
        float x = 0.0f, y = 0.0f, z = 0.0f;
        foreach (Vector3 vector in vectors)
        {
            if (vector.x > x) x = vector.x;
            if (vector.y > y) y = vector.y;
            if (vector.z > z) z = vector.z;
        }
        return new Vector3(x , y , z );
    }
    public static Vector3 MinVectors(List<Vector3> vectors)
    {
        float x = 99999.0f, y = 99999.0f, z = 99999.0f;
        foreach (Vector3 vector in vectors)
        {
            if (vector.x < x) x = vector.x;
            if (vector.y < y) y = vector.y;
            if (vector.z < z) z = vector.z;
        }
        return new Vector3(x, y, z);
    }

	float Variance(MeshRenderer[] solids)
	{
		int n = 0;
		float mean = 0.0f;
		float m2 = 0.0f;

		foreach(MeshRenderer solid in solids)
		{
			n = n + 1;
			float sz = solid.bounds.size.x * solid.bounds.size.y * solid.bounds.size.z;
			float delta = sz - mean;
			mean = mean + delta/n;

			m2 = m2 + delta * (sz - mean) ;

		}

		Debug.Log("MEAN: " + mean );
		return m2 / (n-1);

	}

    public List<Texture2D> lcs = new List<Texture2D>();


}
