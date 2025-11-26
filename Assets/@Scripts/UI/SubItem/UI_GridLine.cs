using Unity.VisualScripting;
using UnityEngine;

public class UI_GridLine : UI_Base
{
    UI_Map _parent;
    RectTransform _rectTransform;

    public Vector3 WorldPos { get; set; }
    public bool IsHorizontal { get; set; }

    public override void Init()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parent = GetComponentInParent<UI_Map>();
    }

    void Update()
    {
        Vector3 xAxis = Camera.main.transform.right;
        Vector3 zAxis = Camera.main.transform.forward;
        zAxis.y = 0;

        Vector3 transformedPosition = new Vector3(
                Vector3.Dot(WorldPos, xAxis.normalized),
                0,
                Vector3.Dot(WorldPos, zAxis.normalized)
            );

        Vector3 newRectLocalPos = Vector3.zero;
        newRectLocalPos.x = _parent.PivotPos.x + transformedPosition.x * _parent.Scale;
        newRectLocalPos.y = _parent.PivotPos.y + transformedPosition.z * _parent.Scale;
        _rectTransform.localPosition = newRectLocalPos;

        float rotationAngle = Camera.main.transform.eulerAngles.y;
        if (IsHorizontal)
            rotationAngle += 90.0f;
        _rectTransform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, rotationAngle));
    }
}
