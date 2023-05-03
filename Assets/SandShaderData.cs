using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandShaderData : MonoBehaviour
{
    public MouseGetter mouseGetter;
    public Material sandDisp;
    public Material sand;

    // Update is called once per frame
    void Update()
    {
        // transform mouse info from world to UV space
        Vector3 localMouseHit = transform.InverseTransformPoint(mouseGetter.mouseHit) * 0.1f;
        Vector2 uvMouseHit = new Vector2(-localMouseHit.x, -localMouseHit.z) + Vector2.one * 0.5f;
        Vector2 uvMouseVel = transform.InverseTransformVector(mouseGetter.mouseVel);
        //uvMouseHit += new Vector2(0.5f, 0.5f);

        //Debug.Log(uvMouseHit);

        // set material values
        sandDisp.SetVector("_MousePos", uvMouseHit);
        sandDisp.SetVector("_MouseVel", uvMouseVel);

        float mouseDownFloat = Convert.ToSingle(mouseGetter.mouseHeld);
        sandDisp.SetFloat("_MouseDown", mouseDownFloat);
        sand.SetFloat("_MouseDown", mouseDownFloat);

    }
}
