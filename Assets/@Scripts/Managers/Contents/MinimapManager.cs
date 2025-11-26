using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MinimapManager
{
    int _playerMarkerId = 1;
	Minimap_Marker _playerMarker;
	List<Minimap_Marker> _markers = new List<Minimap_Marker>();
    List<MinimapObject_Edge> _edges = new List<MinimapObject_Edge>();
    List<Minimap_Zone> _zones = new List<Minimap_Zone>();

	public Minimap_Marker PlayerMarker { get { return _playerMarker; } }
	public List<Minimap_Marker> Markers { get { return _markers; } }
    public List<MinimapObject_Edge> Edges { get { return _edges; } }
    public List<Minimap_Zone> Zones { get { return _zones; } }

	GameObject _root;
    public GameObject Root
    {
        get
        {
            if (_root == null)
                _root = new GameObject { name = "@MinimapObjects" };
            return _root;
        }
    }

    public void SetPlayer(GameObject player)
    {
        if (_playerMarker == null)
        {
            _playerMarker = Managers.Resource.Instantiate("Minimap/Minimap_Marker", Root.transform).GetComponent<Minimap_Marker>();
            _playerMarker.GetComponent<SpriteRenderer>().sortingOrder = 1;
		}
		_playerMarker.SetTemplateId(_playerMarkerId, player.transform.position);
		_playerMarker.SetSyncPositionTarget(player.transform);
		_playerMarker.SetSyncRotationTarget(player.transform);
	}

	public void AddMarker(Minimap_Marker obj)
    {
        _markers.Add(obj);
        obj.transform.SetParent(Root.transform);

        UI_Popup ui = Managers.UI.TopPopup();
        if (ui != null && ui as UI_Map != null)
        {
            (ui as UI_Map).AddMinimapObject(obj);
        }
    }

    public void AddObject(MinimapObject_Edge obj)
    {
        _edges.Add(obj);
        obj.transform.SetParent(Root.transform);

        UI_Popup ui = Managers.UI.TopPopup();
        if (ui != null && ui as UI_Map != null)
        {
            (ui as UI_Map).AddMinimapObject(obj);
        }
    }

    public void AddZone(Minimap_Zone obj)
    {
        _zones.Add(obj);
        obj.transform.SetParent(Root.transform);
		obj.GetComponent<SpriteRenderer>().sortingOrder = -1;
		UI_Popup ui = Managers.UI.TopPopup();
        if (ui != null && ui as UI_Map != null)
        {
            (ui as UI_Map).AddMinimapObject(obj);
        }
	}

	public void RemoveMarker(Minimap_Marker obj)
    {
		_markers.Remove(obj);
        if (obj.IsStarPiece)
            Managers.Star.DrawConstellation(obj.StarPiecePos);
        Managers.Resource.Destroy(obj.gameObject);
    }

    public void RemoveObject(MinimapObject_Edge obj)
    {
        _edges.Remove(obj);
        Managers.Resource.Destroy(obj.gameObject);
    }

    public void RemoveZone(Minimap_Zone obj)
    {
        _zones.Remove(obj);
        Managers.Resource.Destroy(obj.gameObject);
	}

	public void Clear()
    {
        Markers.Clear();
        Edges.Clear();
        Zones.Clear();
	}
}
