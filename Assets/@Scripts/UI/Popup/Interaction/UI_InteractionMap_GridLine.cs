using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_InteractionMap_GridLine : UI_Base
{
    UI_InteractionMap _mapUI;
    RectTransform _rectTransform;

    Vector3 _firstPos;
    Vector3 _secondPos;
    
    public override void Init()
    {
        _mapUI = GetComponentInParent<UI_InteractionMap>();
        _rectTransform = GetComponent<RectTransform>();
	}

    public void SetInfo(Vector3 firstPos, Vector3 secondPos)
    {
        _firstPos = firstPos;
        _secondPos = secondPos;
    }

    void Update()
    {
        Vector3 firstRectPos = WorldToRect(_firstPos);
        Vector3 secondRectPos = WorldToRect(_secondPos);
        float scale = _mapUI.Scale / 5.0f;
        _rectTransform.localPosition = firstRectPos;
        _rectTransform.localScale = new Vector2(Vector3.Distance(secondRectPos, firstRectPos), scale);
        _rectTransform.localRotation = Quaternion.Euler(0, 0, AngleInDeg(firstRectPos, secondRectPos));
    }

    Vector3 WorldToRect(Vector3 worldPos)
    {
        Vector3 xAxis = Camera.main.transform.right;
        Vector3 zAxis = Camera.main.transform.forward;
        zAxis.y = 0;

        Vector3 transformedPosition = new Vector3(
                Vector3.Dot(worldPos, xAxis.normalized),
                0,
                Vector3.Dot(worldPos, zAxis.normalized)
            );

        Vector3 newRectLocalPos = Vector3.zero;
        newRectLocalPos.x = _mapUI.PivotPos.x + transformedPosition.x * _mapUI.Scale;
        newRectLocalPos.y = _mapUI.PivotPos.y + transformedPosition.z * _mapUI.Scale;
        return newRectLocalPos;
    }

    public float AngleInRad(Vector3 vec1, Vector3 vec2)
    {
        return Mathf.Atan2(vec2.y - vec1.y, vec2.x - vec1.x);
    }

    public float AngleInDeg(Vector3 vec1, Vector3 vec2)
    {
        return AngleInRad(vec1, vec2) * 180 / Mathf.PI;
	}
}
