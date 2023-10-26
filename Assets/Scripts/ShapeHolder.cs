using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.Events;

public class ShapeHolder : MonoBehaviour
    /// A class for capturing a multitude of shapes and their relation to each other spatially.
{
    public UnityEvent<ShapeType, Vector2[]> onAddShapeToCanvas;
    public UnityEvent onCanvasClear;
    public CustomRenderTexture canvasTexture;

    
    public enum ShapeType
    {
        Circle,
        Line,
        Polygon,
        Triangle,
        Rectangle
    }
    private List<Tuple<ShapeType, Vector2[]>> shapes;
    public int ShapeAmount
    {
        get { return shapes.Count; }
    }

    void Start() 
    {
        shapes = new List<Tuple<ShapeType, Vector2[]>>();
        if (onAddShapeToCanvas == null)
            onAddShapeToCanvas = new UnityEvent<ShapeType, Vector2[]>();
        if (onCanvasClear == null)
            onCanvasClear = new UnityEvent();
    }

    public void AddCircle(Vector2 center, float radius) 
    {
        Vector2[] param = { center, new Vector2(radius, radius) };
        shapes.Add( new Tuple<ShapeType, Vector2[]>(ShapeType.Circle, param) );
        onAddShapeToCanvas.Invoke(ShapeType.Circle, param);
    }
    public void AddRectangle(Vector2 topLeft, Vector2 bottomRight) 
    {
        Vector2[] param = { topLeft, bottomRight };
        shapes.Add( new Tuple<ShapeType, Vector2[]>(ShapeType.Rectangle, param) );
        onAddShapeToCanvas.Invoke(ShapeType.Rectangle, param);
    }

    public void AddLine(Vector2 start, Vector2 end)
    {
        Vector2[] param = { start, end };
        shapes.Add( new Tuple<ShapeType, Vector2[]>(ShapeType.Line, param) );
        onAddShapeToCanvas.Invoke(ShapeType.Line, param);
    }

    public void AddTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Vector2[] param = { p1, p2, p3 };
        shapes.Add( new Tuple<ShapeType, Vector2[]>(ShapeType.Triangle, param) );
        onAddShapeToCanvas.Invoke(ShapeType.Triangle, param);
    }

    public void Clear()
    {
        onCanvasClear.Invoke();
        shapes.Clear();
    }

}
