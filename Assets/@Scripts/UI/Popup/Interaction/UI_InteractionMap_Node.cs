using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_InteractionMap_Node : UI_Base
{
	enum Texts
	{
		PositiveRatioText,
	}

	UI_InteractionMap _mapUI;
	RectTransform _rectTransform;
	Image _image;

	GameObject _target;
	Sprite _icon;
	float _iconSize;
	Color _color;
	bool _syncToCameraRotation;
	bool _isStarPiece = false;

	float _scaleWeight = 1.0f;
	float _animationDuration = 0.2f;

	StarPiece _star;
	List<StarPiece> _path;

	public GameObject Target { get { return _target; } }
	public bool Clicked { get; set; } = false;
	public bool Hover { get; set; } = false;
	public bool Active { get; set; } = true;
	public bool IsMoving { get; set; } = false;

	public override void Init()
	{
		Bind<Text>(typeof(Texts));

		gameObject.BindEvent(OnPointerEnter, Define.UIEvent.PointerEnter);
		gameObject.BindEvent(OnPointerExit, Define.UIEvent.PointerExit);
		gameObject.BindEvent(OnClick);

		_mapUI = GetComponentInParent<UI_InteractionMap>();

		_image = GetComponent<Image>();
		_image.sprite = _icon;
		_image.color = _color;

		_rectTransform = GetComponent<RectTransform>();
		_rectTransform.sizeDelta = new Vector2(_iconSize, _iconSize);

		_star = Target.GetComponent<StarPiece>();

		if (Target.GetComponent<StarPiece>())
		{
			StarPiece star = Target.GetComponent<StarPiece>();
			float ratio = star.CurrentPositiveEnergy / star.MaxPositiveEnergy;
			GetText((int)Texts.PositiveRatioText).text = (ratio * 100.0f).ToString("0.0") + "%";
			_isStarPiece = true;
		}
		GetText((int)Texts.PositiveRatioText).gameObject.SetActive(false);

		StartCoroutine(CoScale());
	}

	public void SetInfo(Minimap_Marker obj)
	{
		_target = obj.gameObject;
		_icon = obj.Icon;
		_iconSize = obj.IconSize;
		_color = Color.white;
		//_syncToCameraRotation = obj.SyncToCameraRotation;
	}

	void Update()
	{
		if (_target == null)
		{
			Managers.Resource.Destroy(gameObject);
			return;
		}

		Vector3 xAxis = Camera.main.transform.right;
		Vector3 zAxis = Camera.main.transform.forward;
		zAxis.y = 0;

		Vector3 worldPos = _target.transform.position;
		Vector3 transformedPosition = new Vector3(
				Vector3.Dot(worldPos, xAxis.normalized),
				0,
				Vector3.Dot(worldPos, zAxis.normalized)
			);

		Vector3 newRectLocalPos = Vector3.zero;
		newRectLocalPos.x = _mapUI.PivotPos.x + transformedPosition.x * _mapUI.Scale;
		newRectLocalPos.y = _mapUI.PivotPos.y + transformedPosition.z * _mapUI.Scale;
		_rectTransform.localPosition = newRectLocalPos;

		if (_syncToCameraRotation == false)
		{
			Vector3 targetForward = _target.transform.forward;
			targetForward.y = 0;
			Vector3 cameraForward = Camera.main.transform.forward;
			cameraForward.y = 0;
			float angle = Vector3.SignedAngle(targetForward, cameraForward, Vector3.up);
			_rectTransform.rotation = Quaternion.Euler(0, 0, angle);
		}

		// TEMP
		//if (_obj.IsStarPiece && _mapUI.OnThePath.Contains(_obj.SyncPositionTarget.GetComponent<StarPiece>()))
		//	_scaleWeight = Mathf.Lerp(1.0f, 1.3f, (Mathf.Sin(Time.time * 4.0f) + 1) / 2);
		//else if (_obj.IsStarPiece)
		//	_scaleWeight = 1.0f;
		float scale = _mapUI.Scale / 5.0f * _scaleWeight;
		_rectTransform.localScale = new Vector3(scale, scale);

		Color color = _color;
		MazeWall wall = Target.GetComponent<MazeWall>();
		if (wall != null && wall.Type == Define.TileType.Empty)
			color = Color.blue;
		if (wall != null)
			IsMoving = wall.IsMoving;
		if (_star != null && _star.CurrentPositiveEnergy <= 10.0f && Hover == false && _mapUI.OnThePath.ContainsKey(_star))
			color = Color.red;
		if (!Active || IsMoving)
			color.a = 0.2f;
		_image.color = color;
	}

	public void OnPointerEnter(PointerEventData data)
	{
		GetText((int)Texts.PositiveRatioText).gameObject.SetActive(_isStarPiece);
		if (Clicked || !Active || IsMoving)
			return;

		Hover = true;
		_scaleWeight = 1.4f;

		//if (_path == null)
		//	FindPath();
		//_mapUI.ShowPath(_path);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		GetText((int)Texts.PositiveRatioText).gameObject.SetActive(false);
		if (Clicked || !Active || IsMoving)
			return;

		Hover = false;
		_scaleWeight = 1.0f;

		//_mapUI.ClosePath(_path);
		//_path = null;
	}

	public void OnClick(PointerEventData eventData)
	{
		if (Clicked || !Active || IsMoving)
			return;
		_scaleWeight = 1.4f;
		Clicked = true;
		_mapUI.SelectNode(this);

		if (_path == null)
			FindPath();
		_mapUI.ShowPath(_path);
	}

	public void Cancel()
	{
		_scaleWeight = 1.0f;
		Clicked = false;
		Hover = false;

		_mapUI.ClosePath(_path);
		_path = null;
	}

	void FindPath()
	{
		_path = new List<StarPiece>();
		PlayerController player = Managers.Object.GetPlayer();
		Vector2Int gridPos = Managers.Map.WorldToGrid(Target.transform.position);
		if (Target.GetComponent<MazeWall>())
			gridPos = Target.GetComponent<MazeWall>().GridPos;

		if ((player.GridPos - gridPos).sqrMagnitude > 1)
		{
			GameObject go = Managers.Object.FindNearestObjectWithTag("Star", player.transform.position, 5);
			if (go == null)
				return;
			StarPiece star = go.GetComponent<StarPiece>();
			if (Target.GetComponent<StarPiece>())
				_path = Managers.Star.FindPath(star, Target.GetComponent<StarPiece>());
			else
				_path = Managers.Star.FindPath(star, gridPos);
		}
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
