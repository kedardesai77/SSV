using UnityEngine;
using System;
using System.Collections;
using Vectrosity;

public class ControlManager : MonoBehaviour 
{

	public GameObject spectator;
    public GameObject orbiter;
    public GameObject avatar;
    public GameObject ovr;

	public Transform spectator_pivot;
    public Transform orbiter_pivot;

    public SpectatorCamera spectator_camera;
    public OrbiterCamera orbiter_camera;
    public AvatarCamera avatar_camera;

    public void TogglePivot(bool isVisible)
    {
        try
        {
           spectator_camera.showPivotOnRotate = isVisible;
        }
        catch(NullReferenceException e)
        {

        }
        
        try
        {
           orbiter_camera.showPivotOnPan = isVisible;

        }
        catch(NullReferenceException e)
        {

        }
        
    }
    public void ToggleAA(bool isAA)
    {
        try
        {
            spectator_camera.GetComponent<AntialiasingAsPostEffect>().enabled = isAA;
            orbiter_camera.GetComponent<AntialiasingAsPostEffect>().enabled = isAA;
            avatar_camera.GetComponent<AntialiasingAsPostEffect>().enabled = isAA;
        }
        catch(Exception e)
        {

        }
    }
	public void ToggleEdgeDetection(bool edgeD)
	{
		try
		{
			spectator_camera.GetComponent<EdgeDetectEffectNormals>().enabled = edgeD;
			orbiter_camera.GetComponent<EdgeDetectEffectNormals>().enabled = edgeD;
			avatar_camera.GetComponent<EdgeDetectEffectNormals>().enabled = edgeD;
		}
		catch(Exception e)
		{
			
		}
	}
    public void ToggleEnchancedContrast(bool ec_new)
    {
        try
        {
            spectator_camera.GetComponent<ContrastEnhance>().enabled = ec_new;
            orbiter_camera.GetComponent<ContrastEnhance>().enabled = ec_new;
            avatar_camera.GetComponent<ContrastEnhance>().enabled = ec_new;
        }
        catch (Exception e)
        {

        }
    }

	public void ResetSpectator(Vector3 center)
	{
        spectator_pivot.GetComponent<PivotController>().ResetView(center);
        spectator.GetComponentInChildren<SpectatorCamera>().ResetView();
	}
    public void ResetOrbiter(Vector3 center)
    {
        orbiter_pivot.position = center;
        orbiter_camera.ResetView(center);
    }
    public void ResetAvatar(Vector3 center)
    {
        avatar.GetComponentInChildren<CharacterController>().transform.position = center;
        //throw new System.NotImplementedException();
    }
    public void ResetOVR(Vector3 center)
    {
        ovr.transform.position = center;
        //throw new System.NotImplementedException();
    }

    public void ResetView(Vector3 center)
    {

        if (spectator.activeSelf)
        {
            ResetSpectator(center);
            orbiter_pivot.position = center;
            ResetOrbiter(center);
            //ResetAvatar(center);
        }
        else if (orbiter.activeSelf)
        {
            ResetOrbiter(center);
            spectator_pivot.position = center;
        }
        else if (avatar.activeSelf) ResetAvatar(center);
        else if (ovr.activeSelf) ResetOVR(center);
    }

    public void SwitchToOrbitCam()
    {
        if (orbiter.activeSelf)
        {
            SwitchToSpectator();
        }
        else
        {
            avatar.SetActive(false);
            spectator.SetActive(false);
            ovr.SetActive(false);
            orbiter.SetActive(true);
            VectorLine.SetCamera3D();
            NotificationCenter.DefaultCenter().PostNotification(this, "SwitchedToOrbiterCamera");
        }
    }
    public void SwitchToAvatar()
    {
        if (avatar.activeSelf)
        {
            SwitchToSpectator();
        }
        else
        {
            spectator.SetActive(false);
            orbiter.SetActive(false);
            ovr.SetActive(false);
            avatar.SetActive(true);
            VectorLine.SetCamera3D();
            NotificationCenter.DefaultCenter().PostNotification(this, "SwitchedToAvatar");
        }

    }
    public void SwitchToSpectator()
    {
        avatar.SetActive(false);
        orbiter.SetActive(false);
        ovr.SetActive(false);
        spectator.SetActive(true);
        VectorLine.SetCamera3D();
        //NotificationCenter.DefaultCenter().PostNotification(this, "SwitchToSpectatorCamera");
    }
    public void SwitchToOVR()
    {
        if (ovr.activeSelf)
        {
            SwitchToSpectator();
        }
        else
        {
            //Physics.IgnoreLayerCollision((int)SafeSim.Layers.Controller, (int)SafeSim.Layers.model, true);
            spectator.SetActive(false);
            orbiter.SetActive(false);
            avatar.SetActive(false);
            ovr.SetActive(true);
            //VectorLine.SetCamera3D();
            NotificationCenter.DefaultCenter().PostNotification(this, "SwitchedToOVR");
        }

    }


    public void MovePivot(Vector3 pos)
    {
        avatar.GetComponentInChildren<CharacterController>().transform.position = pos;
        orbiter_pivot.position = pos;
        orbiter_camera.ResetView(pos , false);
        spectator_pivot.position = pos;
        
    }

    public void Focus(Vector3 size)
    {
        if (!spectator.activeSelf) SwitchToSpectator();
        spectator_camera.CloseUp(size);
        
    }


}
