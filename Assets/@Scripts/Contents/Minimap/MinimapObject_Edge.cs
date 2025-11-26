using UnityEngine;

public class MinimapObject_Edge : MonoBehaviour
{
    [SerializeField]
    Color _color;
    [SerializeField]
    float _width;
    [SerializeField]
    Transform _target;
    [SerializeField]
    Vector3 _firstPos;
    [SerializeField]
    Vector3 _secondPos;

    public Color Color { get { return _color; } set { _color = value; } }
    public float Width { get { return _width; } set { _width = value; } }
    public Transform Target { get { return _target; } set { _target = value; } }
    public Vector3 FirstPos { get { return _firstPos; } set { _firstPos = value; } }
    public Vector3 SecondPos { get { return _secondPos; } set { _secondPos = value; } }

    LineRenderer _lineRenderer;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.startWidth = _lineRenderer.endWidth = Width;
        _lineRenderer.material.color = Color;
        _firstPos.y = -20.0f;
        _secondPos.y = -20.0f;
        _lineRenderer.SetPosition(0, FirstPos);
        _lineRenderer.SetPosition(1, SecondPos);
    }

    void Update()
    {
        if (Target == null)
            Managers.Minimap.RemoveObject(this);
    }
}
