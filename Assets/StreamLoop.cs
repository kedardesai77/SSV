using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StreamLoop : MonoBehaviour {


    int div = 100;

    int tolerance = 4;

    SSModel mainmodel;

    public ControlManager cm;
    public Transform imageDude;

    bool started = false;
    Vector3 size = Vector3.zero;

    int[,] array2d = new int[10, 10];

    GameObject[,] models = new GameObject[10, 10];

    string path = "";

	// Use this for initialization
	void Start ()
    {
        NotificationCenter.DefaultCenter().AddObserver(this, "OnModelLoaded");

        array2d = new int[div, div];
        for (int i = 0; i < div; i++)
            for (int j = 0; j < div; j++)
                array2d[i, j] = 0;

        array2d[0, 0] = 1;

        models = new GameObject[div, div];


        path = SafeSim.MODEL_URL + "/" + SafeSim.PROJECT.Replace("PID", "");
        path += "/" + SafeSim.ASSET + ".unity3d";
        path = path.Replace(" ", "%20");

	}

    // Update is called once per frame
    void Update() 
    {
        if (started)
        {

            Vector3 a = imageDude.position;
            //Vector3 b = cm.spectator_pivot.transform.position;
            int i = Mathf.FloorToInt(a.x / size.x);
            int j = Mathf.FloorToInt(a.z / size.z);

            if (i < 0 || j < 0) return;
            switch (array2d[i, j])
            {
                case 0:
                    //Load model
                    array2d[i,j] = 1;
                    StartCoroutine(LoadModelInit(path , i , j));
                    break;
                default:
                    for (int k = 0; k < div; k++)
                        for (int l = 0; l < div; l++)
                        {
                            if (Math.Abs(i - k) > tolerance || Math.Abs(j - l) > tolerance)
                            {
                                Destroy(models[k, l]);
                                array2d[k, l] = 0;
                            }
                        }
                    break;

            }

            int x = tolerance / 2;
            for (int y = i - x; y < i + x; y++)
            {
                for (int z = j - x; z < j + x; z++)
                {

                    {
                        array2d[y, z] = 1;
                        StartCoroutine(LoadModel(path, y, z));
                    }
                }
            }



        }
	}

    
    public void OnModelLoaded(Notification n)
    {
        mainmodel = GetComponent<SSModel>();

        //imageDude.position = new Vector3(25, 5, 25);
        Vector3 a = imageDude.position;
        //Vector3 b = cm.spectator_pivot.transform.position;
        int i = Mathf.FloorToInt(a.x / div);
        int j = Mathf.FloorToInt(a.z / div);

        size = mainmodel.eMax;

        StartCoroutine(InitLoad());

    }

    public IEnumerator InitLoad()
    {
        for (int n = 0; n < tolerance; n++)
        {
            for (int m = 0; m < tolerance; m++)
            {
                array2d[m, n] = 1;
                yield return StartCoroutine(LoadModelInit(path, m, n));
            }
        }
        started = true;
    }
    public IEnumerator LoadModelInit(string path, int i, int j)
    {

        while (!Caching.ready)
            yield return null;

        // Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache
        //using(WWW www = new WWW ( (SafeSim.url + modelPath).Replace(" " ,"%20")  ))
        using (WWW www = WWW.LoadFromCacheOrDownload(path, 5))
        {

            yield return www;
            if (www.error != null)
            {
                Application.ExternalEval("alert('" + www.error + "');");
                //throw new Exception("WWW download had an error:" + www.error);
            }
            try
            {
                AssetBundle bundle = www.assetBundle;	//necessary

                GameObject go = (GameObject)Instantiate(bundle.mainAsset);
                models[i, j] = go;
                go.transform.Translate(new Vector3(i * size.x , 0, j * size.z * 2.50f));
                array2d[i, j] = 1;
                bundle.Unload(false);
                www.Dispose();
                bundle = null;
            }
            catch (Exception e)
            {
                //Debug.Log(i + "::" + j);
                Debug.Log(e.Message);
            }
        }
    }
    public IEnumerator LoadModel(string path , int i , int j)
    {

        while (!Caching.ready)
			yield return null;
		
		// Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache
		//using(WWW www = new WWW ( (SafeSim.url + modelPath).Replace(" " ,"%20")  ))


        using (WWW www = WWW.LoadFromCacheOrDownload(path, 5))
        {

            yield return www;
            if (www.error != null)
            {
                Application.ExternalEval("alert('" + www.error + "');");
                Debug.Log(www.url);
                //throw new Exception("WWW download had an error:" + www.error);
            }


                AssetBundle bundle = www.assetBundle;	//necessary

                GameObject go = (GameObject)Instantiate(bundle.mainAsset);
                models[i, j] = go;
                go.transform.Translate(new Vector3(i * size.x, 0, j * size.z * 2.50f));



                bundle.Unload(false);
                www.Dispose();
                bundle = null;



            /*
            for (int k = 0; k < div; k++)
                for (int l = 0; l < div; l++)
                {
                    if (Math.Abs(i - k) > tolerance || Math.Abs(j - l) > tolerance)
                    {
                        Destroy(models[k, l]);
                        array2d[k, l] = 0;
                    }
                }
             * */
                    
        }
        

    }

}
