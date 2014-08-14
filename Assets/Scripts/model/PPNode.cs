using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PPNode : MonoBehaviour {


	BoxCollider bc;
	Rigidbody rb;
    MeshCollider mc;

	public List<PPNode> children = new List<PPNode>();
	
	public Material line_material;
	
	public static float line_width = 14.0f;
	public Texture texture;

    private List<GameObject> children_to_add = new List<GameObject>();

    private string saveTag;

	private int trigger_coroutines = 0;
	private bool active = false;

	void Start()
	{
		line_material = (Material)Resources.Load("LineMaterial");
		if(GetComponent<Rigidbody>() != null)
		{
			Destroy(this);	//go already being processed by another branch so kill this one
			return;
		}
		if(PressurePropogation.CHECK_GUIDS.Contains(gameObject))
		{
			CheckNode();
		}
		else
		{
			StartCoroutine(Routine());
		}
	}

	private void CheckNode()
	{
		saveTag = gameObject.tag;
		gameObject.tag = "PP_ON";
	}

    private void PrepareNode()
    {
		active = true;
        saveTag = gameObject.tag;
        gameObject.tag = "PP_ON";

        //Disable default collider
        mc = GetComponent<MeshCollider>();
        mc.enabled = false;

        //Add box collider and rigidbody for propogation
        bc = gameObject.AddComponent<BoxCollider>();
        rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        bc.isTrigger = true;

        //Increase size by 1% to catch adjacent SSObjects
        //"caught" in OnTriggerEnter()
        Vector3 newsize = bc.size;
        if (bc.size.x > 1) newsize.x = bc.size.x * 1.001f;
        else newsize.x = bc.size.x * 1.08f;
        if (bc.size.y > 1) newsize.y = bc.size.y * 1.001f;
        else newsize.y = bc.size.y * 1.08f;
        if (bc.size.y > 1) newsize.y = bc.size.y * 1.001f;
        else newsize.z = bc.size.z * 1.08f;

        bc.size = newsize;
    }

	public IEnumerator Routine()
	{
        PrepareNode();
		yield return new WaitForSeconds(1.0f);
        End();
	}

    public void End()
    {
		//PrepLine();
        Destroy(bc);
        Destroy(rb);
		mc.enabled = true;
		active = false;
    }

    public void RevertTag()
    {
        gameObject.tag = saveTag;
    }



    ////////////////////////////////
    //FOR BUILDING TREE (INITIALIZE)
    ////////////////////////////////

    void OnTriggerEnter(Collider other)
    {
		trigger_coroutines++;
		StartCoroutine(TriggerRoutine(other));
    }
	/*
    void OnTriggerStay(Collider other)
    {
        trigger_coroutines++;
        StartCoroutine(TriggerRoutine(other));
    }
    */
	public IEnumerator TriggerRoutine(Collider other)
	{

		if (Validate(other.gameObject))
		{
			if (PressurePropogation.CHECK_GUIDS.Contains(other.gameObject))
			{
				while(active)
					yield return null;
				GameObject checkResult = CheckValve(other.gameObject);
				if((checkResult != null) && (Validate(checkResult)))
				{
					children.Add(other.gameObject.AddComponent<PPNode>());
					PPNode check_node = checkResult.AddComponent<PPNode>();
					children.Add(check_node);
				}
			}
			else
			{
				PPNode n = other.gameObject.AddComponent<PPNode>();
				children.Add(n);
			}
		}
        trigger_coroutines--;


	}

    private void PrepLine()
    {

        string[] points_A = PressurePropogation.guid_contact_points[gameObject.name].Split(',');

        List<Vector3> p = new List<Vector3>();
        List<Vector3> cp = new List<Vector3>();
        foreach (PPNode child in children)
        {
            try
            {
                string[] points_B = PressurePropogation.guid_contact_points[child.gameObject.name].Split(',');

                List<Vector3> AList = new List<Vector3>();
                List<Vector3> BList = new List<Vector3>();

                for (int i = 0; i < points_A.Length - 2; i += 3)
                    AList.Add(new Vector3(float.Parse(points_A[i]), float.Parse(points_A[i + 2]), float.Parse(points_A[i + 1])));

                for (int i = 0; i < points_B.Length - 2; i += 3)
                    BList.Add(new Vector3(float.Parse(points_B[i]), float.Parse(points_B[i + 2]), float.Parse(points_B[i + 1])));


                cp.AddRange(SharedContactPoints(AList, BList));

                List<Vector3> scp = SharedContactPoints(AList, BList);


                foreach (Vector3 v in scp)
                {
                    if (!p.Contains(v))
                    {
                        if (renderer.bounds.Contains(v))
                        {
                            p.Add(renderer.bounds.center);
                            p.Add(v);
                            p.Add(v);
                            p.Add(child.renderer.bounds.center);

                        }
                    }
                }
                //p.Add(transform.position);
                //p.Add(child.transform.position);
            }
            catch (Exception)
            {

            }


        }


        /*

        if (p.Count == 0) return;
        line = new VectorLine(transform.name, p.ToArray(), line_material, line_width, LineType.Discrete, Joins.Weld);
        line.vectorObject.tag = "PP_LINE";
        line.AddNormals();
        line.active = true;
        line.vectorObject.renderer.material = line_material;
        line.continuousTexture = false;
         */
    }


    //true if pressure can flow through given GameObject
    private bool Validate(GameObject go)
    {

		if      (go.tag.Equals("PP_ON")) return false;
        else if (PressurePropogation.CLOSED_GUIDS.Contains(go)) return false;
        else if (PressurePropogation.HOLD_GUIDS.Contains(go)) return false;
        try
        {
            if (go.GetComponent<SSObject>().isHidden) return false;
        }
        catch (Exception) { }

        //Check if go's line number matches starting guid's line number
        //if (!PressurePropogation.current_line.Equals(PressurePropogation.guid_line[go.name])) return false;

		if(PressurePropogation.use_contact_points)
		{

			//Check for matching contact points
			if(PressurePropogation.guid_program_codes.ContainsKey(go.name) && PressurePropogation.guid_program_codes[go.name].Contains("198"))
			{

				return (HasLineNumber(go));
			}
			else if(PressurePropogation.guid_program_codes.ContainsKey(gameObject.name) && PressurePropogation.guid_program_codes[gameObject.name].Contains("198"))
			{

				return (HasLineNumber(go));
			}
			else
			{

				return ContactPoints (gameObject , go);
			}
		}
		else
		{
			//Check to make sure go has a line number
			return (HasLineNumber(go));
		}
    }
	private bool ContactPoints(GameObject local , GameObject foreign )
	{

        string nameA = local.name;
        string nameB = foreign.name;

		try
		{
			string[] points_A = PressurePropogation.guid_contact_points[nameA].Split(',');
			string[] points_B = PressurePropogation.guid_contact_points[nameB].Split(',');
			
			List<Vector3> AList = new List<Vector3>();
			List<Vector3> BList = new List<Vector3>();
			
			for(int i = 0; i < points_A.Length - 2 ; i += 3)
				AList.Add(new Vector3(float.Parse(points_A[i]), float.Parse(points_A[i+2]), float.Parse(points_A[i+1])));
			
			for(int i = 0; i < points_B.Length - 2 ; i += 3)
				BList.Add(new Vector3(float.Parse(points_B[i]), float.Parse(points_B[i+2]), float.Parse(points_B[i+1])));
			
			
			List<Vector3> cps = SharedContactPoints(AList,BList );

            if (cps.Count == 0)
            {
                //check olet
                List<Vector3> olets = Olet(AList, BList);
                foreach (Vector3 cp in olets)
                {
                    //float x = Vector3.Distance(renderer.bounds.center, cp);

                    //Bounds b = renderer.bounds;
                    //b.Expand(0.01f);

                    if (renderer.bounds.Contains(cp))
                        AddTOLinePts(cp, foreign.renderer.bounds.center);


                }
                if (olets.Count == 0) return false;
                else return true;
            }
            else
            {
                
                foreach (Vector3 cp in cps)
                {

                    //float x = Vector3.Distance(renderer.bounds.center, cp);
                    //Bounds b = renderer.bounds;
                    //b.Expand(0.01f);

                    if (renderer.bounds.Contains(cp))
                        AddTOLinePts(cp, foreign.renderer.bounds.center);
                    
                    
                }
                 
                return true;
            } 
		}
		catch (Exception e)
		{
			//Debug.Log(e.Message);
			return false;
		}
	}

    private void AddTOLinePts(Vector3 v , Vector3 r)
    {
        
        if (PressurePropogation.LINE_POINTS.Contains(v))
        {
            if (PressurePropogation.LINE_POINTS.IndexOf(v) - PressurePropogation.LINE_POINTS.IndexOf(renderer.bounds.center) == 1)
            {
                return;
            }
            else
            {
                PressurePropogation.LINE_POINTS.Add(renderer.bounds.center);
                PressurePropogation.LINE_POINTS.Add(v);
                PressurePropogation.LINE_POINTS.Add(v);
                PressurePropogation.LINE_POINTS.Add(r);
            }
        }
        else
        {
        
            PressurePropogation.LINE_POINTS.Add(renderer.bounds.center);
            PressurePropogation.LINE_POINTS.Add(v);
            PressurePropogation.LINE_POINTS.Add(v);
            PressurePropogation.LINE_POINTS.Add(r);
        }
    }


	private List<Vector3> Olet(List<Vector3> AList , List<Vector3> BList)
	{
		List<Vector3> result = new List<Vector3>();
		foreach (Vector3 a in AList)
		{
			foreach (Vector3 b in BList)
			{
				int cnt = 0;
				if(a.x == b.x)cnt++;
				if(a.y == b.y)cnt++;
				if(a.z == b.z)cnt++;
				if(cnt == 2)result.Add(b);
			}
		}
        return result;
	}

    private bool HasLineNumber(GameObject go )
    {
        string name = go.name;
        if (PressurePropogation.guid_line.ContainsKey(name))
        {
            if (PressurePropogation.guid_line[name].Length == 0)
            {
                //Debug.Log(name + " has no line number.");
                return false;
            }
            else
            {
                Bounds b = renderer.bounds;
                b.Expand(0.1f);

                Vector3 va = renderer.bounds.center;
                Vector3 vb = go.renderer.bounds.center;
                int cnt = 0;
                if (va.x == vb.x) cnt++;
                if (va.y == vb.y) cnt++;
                if (va.z == vb.z) cnt++;
                if (cnt == 1)
                {

                }
                else if (cnt == 2)
                {
                    PressurePropogation.LINE_POINTS.Add(renderer.bounds.center);
                    PressurePropogation.LINE_POINTS.Add(go.renderer.bounds.center);
                }
                
                return true;
            }
            

        }
        else
        {
            //Debug.Log(name + " has no line number.");
            return false;
        }
    }
    private bool HasContactPoints(string name)
    {
        if (PressurePropogation.guid_contact_points.ContainsKey(name))
        {
            if (PressurePropogation.guid_contact_points[name].Length == 0)
                return false;

            else return true;
        }
        else
        {
            return false;
        }
    }

	private List<Vector3> SharedContactPoints(List<Vector3> AList , List<Vector3> BList)
    {
		List<Vector3> result = new List<Vector3>();

		foreach (Vector3 a in AList)
        {
			foreach (Vector3 b in BList)
            {
                if (Vector3.Distance(a, b) <= 0.01f) result.Add(b);
            }
        }
        return result;
    }

    public void AddChild(GameObject go)
    {
        if (PressurePropogation.CHECK_GUIDS.Contains(go))
        {

            GameObject checkResult = CheckValve(go);
			if((checkResult != null) && (Validate(checkResult)))
            {
				children.Add(go.AddComponent<PPNode>());
                PPNode check_node = checkResult.AddComponent<PPNode>();
				children.Add(check_node);
            }
        }
		else
		{
			PPNode n = go.AddComponent<PPNode>();
			children.Add(n);
		}

        
               //go.GetComponent<SSObject>().Set_Mat_Color(Color.magenta, true);
         
         /*
        List<Vector3> p = new List<Vector3>();
        foreach (PPNode child in children)
        {
            p.Add(transform.position);
            p.Add(child.transform.position);
        }
        line = new VectorLine(transform.name, p.ToArray(), line_material, line_width, LineType.Discrete, Joins.Weld);
        line.vectorObject.tag = "PP_LINE";
        line.AddNormals();
        line.active = false;
        line.vectorObject.renderer.material = line_material;
        line.continuousTexture = false;
        */
         
    }

    private GameObject CheckValve(GameObject go)
    {

        float centerx = go.renderer.bounds.center.x;
        float centery = go.renderer.bounds.center.y;
        float centerz = go.renderer.bounds.center.z;

        Vector3 center = new Vector3(centerx, centery, centerz);

        float pivotx = go.transform.position.x;
        float pivoty = go.transform.position.y;
        float pivotz = go.transform.position.z;

        Vector3 center_minus_pivot = new Vector3(centerx - pivotx, centery - pivoty, centerz - pivotz);

        Ray ray = new Ray(center, center_minus_pivot);
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            return hit.collider.gameObject;
        }
        else
        {
            return null;
        }
    }

	
    /////////////////
	//FOR TRAVERSAL
    ////////////////

    public IEnumerator SetPropColor(Color c)
    {
        iTween.ColorTo(gameObject, c, 0.25f);
        yield return new WaitForSeconds(0.25f);
		GetComponent<SSObject>().Set_Mat_Color(c, true);
    }
    public IEnumerator SetPropColorStutter(Color c)
    {
        iTween.ColorTo(gameObject, c, 0.50f);
        yield return new WaitForSeconds(0.15f);
        GetComponent<SSObject>().Set_Mat_Color(c, true);
    }


	//Repaint


	void OnGUI()
	{
        try
        {
            //Animate
			//line_material.mainTextureOffset = new Vector2(-Time.time , 0);
			//line_material.mainTextureOffset = new Vector2(0 , Time.time);
			//line.Draw3D();
            
        }catch(Exception )
        {

        }
	}



}
