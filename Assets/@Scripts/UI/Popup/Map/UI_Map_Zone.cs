using NUnit.Framework.Internal;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Map_Zone : UI_Base
{
    UI_Map _mapUI;
    RectTransform _rectTransform;

	Minimap_Zone _obj;
    int _range;
    Color _color;

    float _scaleWeight = 1.0f;

    public Minimap_Zone MinimapObject { get { return _obj; } }
    
	public override void Init()
	{
		_mapUI = GetComponentInParent<UI_Map>();

        Image image = GetComponent<Image>();
        image.color = _color;

		float size = (_obj.Range * 2 + 1) * Managers.Map.CellSize * 10.0f;
		_rectTransform = GetComponent<RectTransform>();
        _rectTransform.sizeDelta = new Vector2(size, size);
    }

    public void SetInfo(Minimap_Zone obj)
	{
		_obj = obj;
        _range = obj.Range;
        _color = obj.Color;
	}

	void Update()
    {
        if (_obj == null)
        {
            Managers.Resource.Destroy(gameObject);
            return;
        }

        Vector3 worldPos = _obj.transform.position;

        Vector3 newRectLocalPos = Vector3.zero;
        newRectLocalPos.x = _mapUI.PivotPos.x + worldPos.x * _mapUI.Scale;
        newRectLocalPos.y = _mapUI.PivotPos.y + worldPos.z * _mapUI.Scale;
        _rectTransform.localPosition = newRectLocalPos;

        float scale = _mapUI.Scale / 5.0f * _scaleWeight;
        _rectTransform.localScale = new Vector3(scale, scale);
	}
}
