using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CompareVersions : MonoBehaviour
{

    public ControlManager control_manager;
    public GameObject default_target;

    public List<string> versions = new List<string>();

    public List<OldModel> archived_models = new List<OldModel>();
    // Use this for initialization

    public List<GameObject> additions = new List<GameObject>();
    public List<GameObject> subtractions = new List<GameObject>();
    public List<GameObject> moved = new List<GameObject>();

    public bool isActive = false;
    private Vector2 scrollPosition = Vector2.zero;

    public Viewer viewer;


    void Start()
    {
        NotificationCenter.DefaultCenter().AddObserver(this, "OnModelLoaded");
    }

    public void OnModelLoaded(Notification n)
    {
        RefreshVersions();
    }

    public void RefreshVersions()
    {
        versions = new List<string>();
        StartCoroutine(GetVersions());
    }

    public void LoadAsset(string asset)
    {
        StartCoroutine(DownloadAndCache(SafeSim.ASSET + asset));
    }

    public void ListButtions(List<GameObject> go_list)
    {
        foreach (GameObject go in go_list)
        {
            if (GUILayout.Button(go.name, GUILayout.Width(50)))
            {
                control_manager.MovePivot(go.transform.position);
            }
        }
    }

    public void OnGUI()
    {

        if (!isActive) return;

        GUILayout.BeginArea(new Rect(5, 30, 100, Screen.height));
        GUILayout.BeginVertical();

        //Unload Button(s)
        foreach (OldModel om in archived_models)
        {
            if (om.loaded && GUILayout.Button(om.version))
            {
                foreach (GameObject go in additions)
                {
                    viewer.model.GUIDs.Remove(go.name);
                }

                Destroy(om.go);
                om.loaded = false;
                archived_models.Remove(om);
                viewer.model.DefaultView();
                additions = new List<GameObject>();
                subtractions = new List<GameObject>();
                moved = new List<GameObject>();
                isActive = false;
                break;
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(Screen.width - 60, 30, 100, Screen.height));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.BeginVertical();
        GUILayout.Label("Added");
        ListButtions(additions);
        GUILayout.Label("Removed");
        ListButtions(subtractions);
        GUILayout.Label("Moved");
        ListButtions(moved);
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();

    }

    IEnumerator GetVersions()
    {
        WWWForm form = new WWWForm();

        form.AddField("sa", "get_asset_versions");
        form.AddField("project", SafeSim.PROJECT.Replace("PID", ""));
        form.AddField("asset", SafeSim.ASSET);

        WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
        yield return www;

        if (www.error == null)
        {
            string[] fileNames = www.text.Split('|');
            foreach (string filename in fileNames)
            {
                if (filename.Length > 0)
                    versions.Add(filename.Replace(SafeSim.ASSET, ""));
            }
        }
        else
        {
            Debug.Log("WWW Error: " + www.error + "\n" + www.url);
        }

    }

    IEnumerator DownloadAndCache(string archiveName)
    {

        // Wait for the Caching system to be ready
        while (!Caching.ready)
            yield return null;


        string url = SafeSim.URL.Replace("UnitySafeSim", "UnityModels") + SafeSim.PROJECT.Replace("PID", "") + "/Archive/" + archiveName + ".unity3d";


        // Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache
        //using(WWW www = new WWW ( (SafeSim.url + modelPath).Replace(" " ,"%20")  ))
        using (WWW www = new WWW(WWW.UnEscapeURL(url)))
        {
            yield return www;
            if (www.error != null)
            {
                Application.ExternalEval("alert('" + www.error + "');");
                throw new Exception("WWW download had an error:" + www.error);
            }
            else
            {
                AssetBundle bundle;
                bundle = www.assetBundle;

                viewer.model.FadeAndGray();


                GameObject dup = (GameObject)Instantiate(bundle.mainAsset);
                dup.transform.position = Vector3.zero;
                OldModel om = new OldModel();
                om.version = archiveName.Replace(SafeSim.ASSET, "");
                om.loaded = true;
                om.go = dup;
                om.name = archiveName;
                archived_models.Add(om);
                Save_Gaps(viewer.model.GetModelGameObject(), dup);
                //dup.transform.Translate(new Vector3(0,0,SafeSim.CENTER.z * 3));
                bundle.Unload(false);
                isActive = true;
                
            }
            www.Dispose();
        }
    }


    private void Save_Gaps(GameObject old_go, GameObject new_go)
    {
        List<Renderer> old_renderers = new List<Renderer>(old_go.GetComponentsInChildren<Renderer>());
        List<Renderer> new_renderers = new List<Renderer>(new_go.GetComponentsInChildren<Renderer>());


        foreach (Renderer i in new_renderers)
        {
            bool match = false;
            foreach (Renderer j in old_renderers)
            {
                if (i.GetComponent<MeshFilter>().name.Equals(j.GetComponent<MeshFilter>().name))
                {
                    match = true;

                    if (i.transform.position == j.transform.position)
                    {
                        i.enabled = false;
                    }
                    else
                    {
                        SSObject sso = i.gameObject.AddComponent<SSObject>();
                        i.gameObject.AddComponent<MeshCollider>();
                        sso.Set_Mat_Color(Color.yellow);
                        moved.Add(i.gameObject);
                    }

                    break;

                }
            }
            if (!match)
            {
                if (viewer.model.GUIDs.ContainsKey(i.name))
                {
                    Debug.Log("no match but key : " + i.name);
                }
                else
                {
                    SSObject sso = i.gameObject.AddComponent<SSObject>();
                    i.gameObject.AddComponent<MeshCollider>();
                    sso.Set_Mat_Color(Color.red);
                    additions.Add(i.gameObject);
                    viewer.model.GUIDs.Add(i.name, i.gameObject);
                }
            }

        }

        foreach (Renderer i in old_renderers)
        {
            bool match = false;
            foreach (Renderer j in new_renderers)
            {
                if (i.GetComponent<MeshFilter>().name.Equals(j.GetComponent<MeshFilter>().name))
                {
                    match = true;
                    break;
                }
            }
            if (!match)
            {

                try
                {
                    i.gameObject.GetComponent<SSObject>().Set_Mat_Color(Color.green, true);
                }
                catch (Exception)
                {
                    //Debug.Log(i.GetComponent<MeshFilter>().name);
                }
                subtractions.Add(i.gameObject);
            }
        }

    }


    public class OldModel
    {
        public string version;
        public string name;
        public bool loaded;
        public GameObject go;

    }

}
