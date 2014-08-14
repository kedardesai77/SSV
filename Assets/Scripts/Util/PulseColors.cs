using UnityEngine;
using System;
using System.Collections;

public class PulseColors : MonoBehaviour
{

    float time = 0.25f;
    Color color1 = Color.magenta;
    Color color2 = Color.cyan;

    bool looping = false;

    public void StartPulse(Color c1, Color c2)
    {
        color1 = c1;
        color2 = c2;
        looping = true;
    }

    public void Stop()
    {
        looping = false;
        GetComponent<SSObject>().Reset_Original_Colors();
        Destroy(this);
    }

    public void Stop(Color end)
    {
        looping = false;
        GetComponent<SSObject>().Set_Mat_Color(end , true);
        Destroy(this);
    }

    void Update()
    {
        if (looping)
        {
            float lerp = Mathf.PingPong(Time.time, time) / time;
            renderer.material.color = Color.Lerp(color1, color2, lerp);
        }

    }
}

