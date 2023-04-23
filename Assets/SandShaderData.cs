using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandShaderData : MonoBehaviour
{
    public MouseGetter mouseGetter;
    public Material sandDisp;

    // Update is called once per frame
    void Update()
    {
        // transform mouse info from world to UV space
        Vector2 uvMouseHit = transform.InverseTransformPoint(mouseGetter.mouseHit);
        Vector2 uvMouseVel = transform.InverseTransformVector(mouseGetter.mouseVel);
        uvMouseHit += new Vector2(0.5f, 0.5f);

        // set material values
        sandDisp.SetVector("_MousePos", uvMouseHit);
        sandDisp.SetVector("_MouseVel", uvMouseVel);
        sandDisp.SetFloat("_MouseDown", Convert.ToSingle(mouseGetter.mouseDown));

    }
}
