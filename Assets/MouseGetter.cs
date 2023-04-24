using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseGetter : MonoBehaviour
{
    public Vector3 mouseHit { get; private set; }
    public Vector3 mouseVel { get; private set; }
    public bool mouseDown { get; private set; } = false;

    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = (mouseDown) ? Color.red : Color.gray;
        Gizmos.DrawSphere(mouseHit, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(mouseHit, mouseHit + mouseVel);
    }
    */


    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Sand")))
        {
            
            if (mouseDown == true)
            {
                mouseVel = (hit.point - mouseHit) / Time.deltaTime;
            }
            mouseHit = hit.point;

        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            mouseDown = true;
        } else
        {
            mouseDown = false;
        }
    }
}
