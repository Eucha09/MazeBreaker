using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEngine.EventSystems.PointerEventData;

public class UI_VirtualCursor : UI_Base
{
	[SerializeField]
	RectTransform _cursor;
	Canvas _canvas;
	float _moveSpeed = 800;

	private GameObject _currentHoverTarget;
	private GameObject _currentPressTarget;
	private PointerEventData _pointerData;
	private bool _isDragging = false;

	public override void Init()
	{
		_canvas = GetComponent<Canvas>();
		_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		_canvas.overrideSorting = true;
		_canvas.sortingOrder = 1000;
		_pointerData = new PointerEventData(EventSystem.current);
	}

	void Update()
	{
		//if (MoveDir == Vector2.zero)
		//	return;

		MoveCursor();
		UpdateHover();

		if (_currentPressTarget != null)
			OnDrag();
	}

	void MoveCursor()
	{
		if (MoveDir.magnitude < 0.3f)
			TrySnapCursor();
		else
		{
			_cursor.anchoredPosition += MoveDir * _moveSpeed * Time.unscaledDeltaTime;

			Rect rect = GetComponent<RectTransform>().rect;
			_cursor.anchoredPosition = new Vector2(
				Mathf.Clamp(_cursor.anchoredPosition.x, rect.xMin, rect.xMax),
				Mathf.Clamp(_cursor.anchoredPosition.y, rect.yMin, rect.yMax)
			);
		}
	}


	void UpdateHover()
	{
		GameObject target = RaycastUI();

		if (target != _currentHoverTarget)
		{
			if (_currentHoverTarget != null)
			{
				ExecuteEvents.ExecuteHierarchy<IPointerExitHandler>(_currentHoverTarget, _pointerData, ExecuteEvents.pointerExitHandler);
			}

			if (target != null)
			{
				ExecuteEvents.ExecuteHierarchy<IPointerEnterHandler>(target, _pointerData, ExecuteEvents.pointerEnterHandler);
			}

			_currentHoverTarget = target;
		}
	}

	public override void OnPointerDown(PointerEventData.InputButton inputButton)
	{
		if (_isDragging)
		{
			ExecuteEvents.ExecuteHierarchy<IEndDragHandler>(_currentPressTarget, _pointerData, ExecuteEvents.endDragHandler);
			_isDragging = false;
			_pointerData.pointerDrag = null;
			return;
		}

		_currentPressTarget = RaycastUI();
		if (_currentPressTarget != null)
		{
			Debug.Log(_currentPressTarget.gameObject.name);
			_pointerData.pointerPressRaycast = new RaycastResult { gameObject = _currentPressTarget };
			_pointerData.pressPosition = _pointerData.position = GetCursorScreenPosition();
			_pointerData.pointerPress = _currentPressTarget;
			_pointerData.button = inputButton;

			ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(_currentPressTarget, _pointerData, ExecuteEvents.pointerDownHandler);

			UI_EventHandler eventHandler = _currentPressTarget.GetComponentInParent<UI_EventHandler>();
			if (inputButton == InputButton.Left && eventHandler != null && eventHandler.enabled && eventHandler.OnDragHandler != null) // Drag 준비
			{
				ExecuteEvents.ExecuteHierarchy<IBeginDragHandler>(_currentPressTarget, _pointerData, ExecuteEvents.beginDragHandler);
				_pointerData.pointerDrag = _currentPressTarget;
				_isDragging = true;
			}
		}
	}

	public void OnDrag()
	{
		if (_isDragging && _pointerData.pointerDrag != null)
		{
			_pointerData.position = GetCursorScreenPosition();
			ExecuteEvents.ExecuteHierarchy<IDragHandler>(_pointerData.pointerDrag, _pointerData, ExecuteEvents.dragHandler);
		}
	}

	public override void OnPointerUpAndClick(PointerEventData.InputButton inputButton)
	{
		if (_isDragging)
			return;

		_pointerData.position = GetCursorScreenPosition();
		_pointerData.button = inputButton;

		if (_currentPressTarget != null)
		{
			ExecuteEvents.ExecuteHierarchy<IPointerUpHandler>(_currentPressTarget, _pointerData, ExecuteEvents.pointerUpHandler);

			// 클릭인지 확인
			if (RaycastUI() == _currentPressTarget)
			{
				ExecuteEvents.ExecuteHierarchy<IPointerClickHandler>(_currentPressTarget, _pointerData, ExecuteEvents.pointerClickHandler);
			}

			_currentPressTarget = null;
		}
	}

	void TrySnapCursor()
	{
		float snapRadius = Screen.height * 0.5f;
		UI_SnapTarget snapTarget = FindClosestSnapTarget(snapRadius);

		if (snapTarget != null)
		{
			Vector2 snapPos = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, snapTarget.Rect.position);

			Vector2 anchored;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, snapPos, _canvas.worldCamera, out anchored);
			_cursor.anchoredPosition = Vector2.Lerp(_cursor.anchoredPosition, anchored, Time.deltaTime * 30f);
			//_cursor.anchoredPosition = anchored;
		}
	}

	UI_SnapTarget FindClosestSnapTarget(float snapRadius)
	{
		Vector2 cursorPos = GetCursorScreenPosition();
		var results = RaycastUIAtCursor(cursorPos);

		UI_SnapTarget bestTarget = null;
		float bestDist = float.MaxValue;

		foreach (var result in results)
		{
			var snap = result.gameObject.GetComponent<UI_SnapTarget>();
			if (snap == null) continue;

			Vector2 targetCenter = RectTransformUtility.WorldToScreenPoint(null, snap.Rect.position);
			float dist = Vector2.Distance(cursorPos, targetCenter);

			if (dist < snapRadius && dist < bestDist)
			{
				bestTarget = snap;
				bestDist = dist;
			}
		}

		return bestTarget;
	}

	public GameObject RaycastUI()
	{
		Vector2 screenPos = GetCursorScreenPosition();

		_pointerData.position = screenPos;
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(_pointerData, results);

		return results.Count > 0 ? results[0].gameObject : null;
	}

	List<RaycastResult> RaycastUIAtCursor(Vector2 screenPos)
	{
		_pointerData.position = screenPos;
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(_pointerData, results);

		return results;
	}

	public Vector2 GetCursorScreenPosition()
	{
		return RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, _cursor.position);
	}

	public void Clear()
	{
		if (_currentHoverTarget != null)
			ExecuteEvents.ExecuteHierarchy<IPointerExitHandler>(_currentHoverTarget, _pointerData, ExecuteEvents.pointerExitHandler);
		_currentHoverTarget = null;
		_currentPressTarget = null;
		_pointerData = new PointerEventData(EventSystem.current);
		_isDragging = false;
	}
}
