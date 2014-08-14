using UnityEngine;
using Vectrosity;
using System.Collections;

public class Grid : MonoBehaviour {

    public bool show_grid = false;

    public Material grid_material;

    public float size = 100;
    public int density = 100;

    private Vector3[] points;
    public VectorLine vLine;

	// Use this for initialization
	void Start () 
    {
        return;
        points = new Vector3[density * 4];

        for (int i = 0; i < points.Length; i += 4)
        {
            //z
            points[i] = new Vector3(-size + i/2, 0, -size);
            points[i+1] = new Vector3(-size + i/2, 0, size);

            //x
            points[i+2] = new Vector3(-size, 0, -size + i/2);
            points[i+3] = new Vector3(size, 0, -size + i/2);

        }

        vLine = new VectorLine("Grid", points, new Color(0.5f,0.5f,0.5f), grid_material, 1.0f, LineType.Discrete, Joins.None);

	}
	
	// Update is called once per frame
	void Update () 
    {
        if (show_grid)
            vLine.Draw3D();
	}

    public void ShowGrid()
    {
        show_grid = true;
        vLine.active = true;
        vLine.Draw3D();
    }

    public void HideGrid()
    {
        //show_grid = false;
        vLine.active = false;
        vLine.Draw3D();
    }

}
