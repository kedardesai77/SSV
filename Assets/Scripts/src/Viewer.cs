using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class Viewer : MonoBehaviour {
	
	public ControlManager control;
	public SSModel model;
    public TopMenu top_menu;
	//Memory and Reference




	// Use this for initialization
	void Start () 
	{
		NotificationCenter.DefaultCenter().AddObserver(this, "OnModelLoaded");

		if(SafeSim.ASSET != "")
		{
			model = GetComponent<SSModel>();
            model.StartModel();
		}
	}



	public SSModel GetModel()
	{
		return model;
	}

	//Routing
	public void DefaultView()
	{
		model.DefaultView();
	}
	public void NormalView()
	{
		model.NormalView();
	}
	public void CurrentView()
	{
		model.CurrentView();
	}

    public void ReCenterView()
    {
        control.ResetView(model.CENTER_POS);
    }

	//Events
	public void OnModelLoaded(Notification n)
	{
        switch(SafeSim.CURRRENT_MODE)
        {
            case  SafeSim.Modes.viewer:
                control.SwitchToSpectator();
                ReCenterView();
                break;
            case SafeSim.Modes.test:
                GetComponent<ProcedureTest>().ShowTitle(model.CENTER_POS);
                break;
            case SafeSim.Modes.record:
                GetComponent<ProcedureRecord>().ShowTitle(model.CENTER_POS);
                break;
            case SafeSim.Modes.demo:
                GetComponent<ProcedureDemo>().ShowTitle(model.CENTER_POS);
                break;
            case SafeSim.Modes.edit:
                GetComponent<ProcedureEdit>().ShowTitle(model.CENTER_POS);
                break;
        }

	}



}
