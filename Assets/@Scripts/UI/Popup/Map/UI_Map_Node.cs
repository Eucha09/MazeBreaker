using NUnit.Framework.Internal;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Map_Node : UI_Base, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    UI_Map _mapUI;
    RectTransform _rectTransform;

	Minimap_Marker _obj;
    Sprite _icon;
    float _iconSize;
    Color _color;
    bool _clickable;

    float _scaleWeight = 1.0f;
    float _animationDuration = 0.2f;

    public Minimap_Marker MinimapObject { get { return _obj; } }
    public int TemplateId { get; set; }
	public Sprite Icon { get { return _icon; } set { _icon = value; } }
	public bool Clicked { get; set; } = false;
	public bool Hover { get; set; } = false;
    
	public override void Init()
	{
		//gameObject.BindEvent(OnPointerEnter, Define.UIEvent.PointerEnter);
		//gameObject.BindEvent(OnPointerExit, Define.UIEvent.PointerExit);
		//gameObject.BindEvent(OnClick);

		_mapUI = GetComponentInParent<UI_Map>();

        Image image = GetComponent<Image>();
        image.sprite = _icon;
        image.color = _color;

        _rectTransform = GetComponent<RectTransform>();
        _rectTransform.sizeDelta = new Vector2(_iconSize, _iconSize);

        StartCoroutine(CoScale());
    }

    public void SetInfo(Minimap_Marker obj)
	{
		TemplateId = obj.TemplateId;
		_obj = obj;
		_icon = obj.Icon;
        _iconSize = obj.IconSize;
        _color = Color.white;
        _clickable = obj.IsPin;

        if (obj.IsPin)
            gameObject.GetOrAddComponent<UI_SnapTarget>();
	}

	public void SetTemplateId(int templateId)
	{
        _obj.SetTemplateId(templateId, _obj.transform.position);
		_icon = _obj.Icon;
		Image image = GetComponent<Image>();
		image.sprite = _icon;
	}

	void Update()
    {
        if (_obj == null)
        {
            Managers.Resource.Destroy(gameObject);
            return;
        }

        //Vector3 xAxis = Camera.main.transform.right;
        //Vector3 zAxis = Camera.main.transform.forward;
        //zAxis.y = 0;

        Vector3 worldPos = _obj.transform.position;
        //Vector3 transformedPosition = new Vector3(
        //        Vector3.Dot(worldPos, xAxis.normalized),
        //        0,
        //        Vector3.Dot(worldPos, zAxis.normalized)
        //    );

        Vector3 newRectLocalPos = Vector3.zero;
        newRectLocalPos.x = _mapUI.PivotPos.x + worldPos.x * _mapUI.Scale;
        newRectLocalPos.y = _mapUI.PivotPos.y + worldPos.z * _mapUI.Scale;
        _rectTransform.localPosition = newRectLocalPos;

        Vector3 targetForward = _obj.transform.up;
        targetForward.y = 0;
		Vector3 SyncRotationTargetForward = _obj.SyncRotationTarget.forward; // Camera.main.transform.forward;
		SyncRotationTargetForward.y = 0;
		if (_obj.SyncRotationTarget != Camera.main.transform)
            SyncRotationTargetForward = Vector3.forward;
		float angle = Vector3.SignedAngle(targetForward, SyncRotationTargetForward, Vector3.up);
        _rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        float scale = _mapUI.Scale / 5.0f * _scaleWeight;
        _rectTransform.localScale = new Vector3(scale, scale);
	}

	public void OnPointerEnter(PointerEventData data)
	{
        if (_clickable == false)
            return;
        if (Clicked)
            return;

        Hover = true;
		_scaleWeight = 1.2f;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (_clickable == false)
			return;
		if (Clicked)
			return;

		Hover = false;
		_scaleWeight = 1.0f;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (_clickable == false)
			return;
		if (Clicked)
			return;

        Select();
		_mapUI.SelectNode(this);
	}

	public void Select()
	{
		_scaleWeight = 1.2f;
		Clicked = true;
	}

	public void Cancel()
	{
		_scaleWeight = 1.0f;
		Clicked = false;
		Hover = false;
	}

	IEnumerator CoScale()
    {
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;

        float elapsedTime = 0f;

        while (elapsedTime < _animationDuration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / _animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
