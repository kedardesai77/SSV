using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PPTree  : MonoBehaviour
{
	public PPNode root ;

	public List<Vector3> pp_points_list = new List<Vector3>();

    public static float traversal_delay = 0.25f;
	public static bool pp_paused = false;


    public static bool doHighlight = false;

	public void Init( GameObject go )
	{
		root = go.AddComponent<PPNode>();
		pp_points_list.Add(go.transform.position);
	}

    public IEnumerator ContinuousTraversal(PPNode node)
    {
        
        if (node != null)
        {
            if (doHighlight)
            {
                node.GetComponent<HighlightableObject>().ConstantOn(Color.red);
            }
            else
            {
                StartCoroutine(node.SetPropColor(Color.magenta));
                node.GetComponent<SSObject>().UnFade();
            }
			
			

			for (int i = 0; i < node.children.Count; i++)
			{
				PressurePropogation.ACTIVE_GUIDS.Add(node.children[i]);
			}

			if(pp_paused)
			{
				yield return null;
			}
			else
			{
				yield return new WaitForSeconds(traversal_delay);
				for (int i = 0; i < node.children.Count; i++)
				{
					StartCoroutine(ContinuousTraversal(node.children[i]));
				}
			}
        }
        else
        {
            yield return null;
        }


    }
	public void ContinuousTraversalAnimated(PPNode node)
	{
		//Base
		if (node == null)  return;
		
		//Action
		//node.SetVectorLine( );


		for(int i = 0; i < node.children.Count ; i++)
		{
			ContinuousTraversalAnimated(node.children[i]);
		}
	}


    public void JunctionTraversal(PPNode node)
    {
        if (node != null)
        {
            StartCoroutine(node.SetPropColorStutter(Color.magenta));

            if (node.children.Count == 1)
            {
                JunctionTraversal(node.children[0]);
            }
            else if (node.children.Count > 1)
            {
                for (int i = 0; i < node.children.Count; i++)
                {
                    //node.children[i].tag = "PP_TRAVERSAL";
                    PressurePropogation.ACTIVE_GUIDS.Add(node.children[i]);
                }
            }
        }
    }



	
	public static void JunctionTraversalAnimated(PPNode node)
	{
		if(node == null) return ;
		
		//node.SetVectorLine();
		
		if(node.children.Count == 1)
		{
            JunctionTraversalAnimated(node.children[0]);
			
		}else if(node.children.Count > 1)
		{
			for(int i = 0; i < node.children.Count ; i++)
			{
				node.children[i].tag = "PP_TRAVERSAL";
			}
			
		}else return;
	}
	
	
	
	
	
	
	
	
}
