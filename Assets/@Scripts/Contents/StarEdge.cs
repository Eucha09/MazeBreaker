using System;
using System.Collections.Generic;
using UnityEngine;

public class StarEdge : MonoBehaviour
{
    [SerializeField]
    Color _color = Color.cyan;
    [SerializeField]
    float _width = 1.0f;
	[SerializeField]
	List<LineRenderer> _lineRenderers;
    MinimapObject_Edge _edgeMinimap;

    public Color Color { get { return _color; } }
    public GameObject FirstObject { get; set; }
    public GameObject SecondObject { get; set; }
    public int Start { get; set; }
    public int End { get; set; }
    public float Distance { get; set; }
    public bool Active { get; set; }

    public List<MazeCell> Cells { get; set; } = new List<MazeCell>();
    public MinimapObject_Edge EdgeMinimap { get { return _edgeMinimap; } }

    public void Init(GameObject firstObject, GameObject secondObject)
    {
        FirstObject = firstObject;
        SecondObject = secondObject;
        Distance = Vector3.Distance(FirstObject.transform.position, SecondObject.transform.position);

        transform.position = Vector3.Lerp(FirstObject.transform.position, SecondObject.transform.position, 0.5f);
        LineRender();
		_lineRenderers[0].gameObject.SetActive(false);
		_lineRenderers[1].gameObject.SetActive(false);

        // set coliider
        transform.LookAt(FirstObject.transform.position);
        GetComponent<BoxCollider>().size = new Vector3(1.0f, 1.0f, Distance);

		Vector3 origin = FirstObject.transform.position;
        origin.y = 1.0f;
        Vector3 dir = Vector3.down;
        RaycastHit[] hits = Physics.RaycastAll(origin, dir, 2.0f, 1 << (int)Define.Layer.Block);
        foreach (var hit in hits)
        {
            MazeCell cell = hit.collider.GetComponentInParent<MazeCell>();
            Cells.Add(cell);
            Managers.Star.Intersect(cell.GridPos);
        }
        dir = SecondObject.transform.position - FirstObject.transform.position;
        dir.y = 0;
        origin = FirstObject.transform.position;
        origin.y = -1.0f;
        hits = Physics.RaycastAll(origin, dir, dir.magnitude, 1 << (int)Define.Layer.Block);
        foreach (var hit in hits)
        {
            MazeCell cell = hit.collider.GetComponentInParent<MazeCell>();
            Cells.Add(cell);
            Managers.Star.Intersect(cell.GridPos);
        }
    }

    void Update()
    {
        if (FirstObject == null || SecondObject == null)
        {
            Disconnect();
        }
    }

    void LineRender()
    {
        Vector3 dir = SecondObject.transform.position - FirstObject.transform.position;
        float size = Mathf.Min(8.0f, dir.magnitude / 2.0f);
        _lineRenderers[0].SetPosition(0, FirstObject.transform.position + new Vector3(0.0f, 0.1f, 0.0f));
        _lineRenderers[0].SetPosition(1, FirstObject.transform.position + new Vector3(0.0f, 0.1f, 0.0f) + dir.normalized * size);

		_lineRenderers[1].SetPosition(0, SecondObject.transform.position + new Vector3(0.0f, 0.1f, 0.0f));
        _lineRenderers[1].SetPosition(1, SecondObject.transform.position + new Vector3(0.0f, 0.1f, 0.0f) + -dir.normalized * size);
	}

    public void Disconnect()
	{
		if (FirstObject != null)
			FirstObject.GetComponent<StarPiece>().Edges.Remove(this);
		if (SecondObject != null)
			SecondObject.GetComponent<StarPiece>().Edges.Remove(this);

		foreach (MazeCell cell in Cells)
			Managers.Star.Separate(cell.GridPos);

		Managers.Resource.Destroy(gameObject);
	}

    public void DrawEdge()
    {
        if (_edgeMinimap != null)
            return;

        Active = true;

        _edgeMinimap = Managers.Resource.Instantiate("Minimap/MinimapObject_Edge").GetComponent<MinimapObject_Edge>();
        _edgeMinimap.Color = _color;
        _edgeMinimap.Width = _width;
        _edgeMinimap.Target = transform;
        _edgeMinimap.FirstPos = FirstObject.transform.position;
        _edgeMinimap.SecondPos = SecondObject.transform.position;
        Managers.Minimap.AddObject(_edgeMinimap);

        _lineRenderers[0].gameObject.SetActive(true);
		_lineRenderers[1].gameObject.SetActive(true);
	}

    public void EraseEdge()
    {
        if (_edgeMinimap == null)
            return;

        Active = false;

        Managers.Minimap.RemoveObject(_edgeMinimap);
        _edgeMinimap = null;

		_lineRenderers[0].gameObject.SetActive(false);
		_lineRenderers[1].gameObject.SetActive(false);
	}
}
