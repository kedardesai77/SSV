using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

using System;
using System.IO;

public class PointCloud : MonoBehaviour {

    List<List<Vector3>> points_divided = new List<List<Vector3>>();

	// Use this for initialization
	void Start () 
    {
        try
        {
            StreamReader sr;
            foreach (string file in Directory.GetFiles("pointcloud/", "*.3d")) 
            {
                    sr = new StreamReader(file);
                
                    string line = "";
                    List<Vector3> points = new List<Vector3>();
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] pts = line.Split(' ');
                        points.Add(new Vector3(float.Parse(pts[0]), float.Parse(pts[1]), float.Parse(pts[2])));
                    }

                    points_divided.AddRange(splitList(points, 16000));
                
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }


        Debug.Log("done reading");

        List<VectorPoints> vector_points = new List<VectorPoints>();
        for (int i = 0; i < points_divided.Count; i++ )
        {
            vector_points.Add(new VectorPoints("ptc" + i, points_divided[i].ToArray(), null, 1.0f));
        }

        foreach (VectorLine vline in vector_points)
        {
            vline.Draw3D();
        }



	}


    public static List<List<Vector3>> splitList(List<Vector3> locations, int nSize = 30)
    {
        List<List<Vector3>> list = new List<List<Vector3>>();

        for (int i = 0; i < locations.Count; i += nSize)
        {
            list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
        }

        return list;
    } 


	// Update is called once per frame
	void Update () {
	
	}
}
