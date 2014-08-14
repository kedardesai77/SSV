using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FocusTarget : MonoBehaviour
{
    private GameObject targetObject = null;
    private GameObject secondaryObject = null;

    private PulseColors primaryPulse;
    private PulseColors secondaryPulse;
    private List<PulseColors> children = new List<PulseColors>();

    public Viewer viewer;

    public ControlManager control_manager;
    private Color savedColor;


    public Select selection;

    public Color pulse_color = new Color(0.196f, 0.196f, 0.650f, 0.25f);

    void OnGUI()
    {

        if (targetObject != null && GUI.Button(new Rect(Screen.width - 70 , 50 , 60, 25 ), "Clear"))
        {
            ClearTarget();
        }


        /*
         * 
         * For testing
         * 
         * 
        if (GUI.Button(new Rect(60, 160, 75, 25), "Test"))
        {
            SetTarget("3EF9E3E7-9037-43DC-88BF-F352E7016438");
            
        }
         * 
         *   if(GUI.Button(new Rect(25 , 25 , 50 , 50) , "Set Target"))
        {
            SetTarget("BDCF4E95-8661-4D61-A876-FBB99B3733CF");
        }
         * 
        GUILayout.BeginArea(new Rect(10,10,150,300));
        if(GUILayout.Button("set Selection"))
        {
            selection.SetSelection("75DC91F2-6BBE-436B-9657-226AD66853EB_I|763F8708-4A9A-41EB-AFBA-61540A45DE9A");
        }
        if(GUILayout.Button("Deselect"))
        {
            selection.Unselect("75DC91F2-6BBE-436B-9657-226AD66853EB_I|763F8708-4A9A-41EB-AFBA-61540A45DE9A");
        }
        if(GUILayout.Button("FadeSelected"))
        {
            selection.FadeSelected("75DC91F2-6BBE-436B-9657-226AD66853EB_I|763F8708-4A9A-41EB-AFBA-61540A45DE9A");
        }
        if(GUILayout.Button("UnfadeSelected"))
        {
            selection.UnfadeSelected("75DC91F2-6BBE-436B-9657-226AD66853EB_I|763F8708-4A9A-41EB-AFBA-61540A45DE9A");
        }
        if(GUILayout.Button("HideSelected"))
        {
            selection.HideSelected("75DC91F2-6BBE-436B-9657-226AD66853EB_I|763F8708-4A9A-41EB-AFBA-61540A45DE9A");
        }
        if(GUILayout.Button("UnHideSelected"))
        {
            selection.UnHideSelected("75DC91F2-6BBE-436B-9657-226AD66853EB_I|763F8708-4A9A-41EB-AFBA-61540A45DE9A");
        }
        if(GUILayout.Button("UnselectAll"))
        {
            selection.UnselectAll();
        }
        if(GUILayout.Button("FadeSelected true"))
        {
            selection.FadeSelected("75DC91F2-6BBE-436B-9657-226AD66853EB_I|763F8708-4A9A-41EB-AFBA-61540A45DE9A" , true);
        }
        GUILayout.EndArea();


        */
    }


    private void SetGUID(string guid)
    {
        targetObject = viewer.model.GUIDs[guid];

        if (guid.Contains("_I"))
        {
            secondaryObject = viewer.model.GUIDs[guid.Substring(0, guid.Length - 2)];
        }
        else if (viewer.model.GUIDs.ContainsKey(guid + "_I"))
        {
            secondaryObject = viewer.model.GUIDs[guid + "_I"];
        }

        //Add and start pulse
        try
        {
            primaryPulse = targetObject.AddComponent<PulseColors>();
            primaryPulse.StartPulse(targetObject.renderer.material.color, pulse_color);
            secondaryPulse = secondaryObject.AddComponent<PulseColors>();
            secondaryPulse.StartPulse(secondaryObject.renderer.material.color, pulse_color);
        }
        catch (Exception)
        {
            //no secondary (insulation)
        }

        try
        {
            //set pivot / pivot pos to targetObject.transform(.position)
            // CloseUp(targetObject.renderer.bounds.size);
            control_manager.MovePivot(targetObject.transform.position);
            control_manager.Focus(targetObject.renderer.bounds.size);

        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }


    private void  SetParentGUID(string guid)
    {
        targetObject = viewer.model.PARENT_GUIDs[guid];

        Renderer[] mychildren = targetObject.GetComponentsInChildren<Renderer>();
        List<Vector3> centers = new List<Vector3>();
        Bounds groupBounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (Renderer child in mychildren)
        {
            PulseColors pc = child.gameObject.AddComponent<PulseColors>();
            pc.StartPulse(child.materials[0].color, pulse_color);
            centers.Add(new Vector3(child.renderer.bounds.center.x, child.renderer.bounds.center.y, child.renderer.bounds.center.z));
            groupBounds.Encapsulate(child.bounds);
            this.children.Add(pc);
        }

        control_manager.MovePivot(SSModel.AvgVectors(centers));
        control_manager.Focus(groupBounds.size);
    }


    public void SetTarget(string guid)
    {

        ClearTarget();


        //set target objects
        if (viewer.model.GUIDs.ContainsKey(guid))
        {
            SetGUID(guid);
        }
        else if (viewer.model.PARENT_GUIDs.ContainsKey(guid))
        {
            SetParentGUID(guid);
        }
    }



    public void ClearTarget()
    {
        //Clear selection
        selection.Clear();

        //Stop any pulse
        try
        {
            primaryPulse.Stop();
            secondaryPulse.Stop();
        }
        catch (Exception e)
        {
        }
        
        
        foreach (PulseColors pc in children)
        {
            try
            {
                pc.Stop();
            }
            catch (Exception e)
            {
            }

        }



        targetObject = null;
    }

}