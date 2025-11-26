using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;
using static UnityEngine.EventSystems.PointerEventData;

public class UI_Map : UI_Popup
{
    enum Images
    {
        Radar,
		Cursor,
    }

    enum GameObjects
    { 
        Pivot,
        Nodes,
        Edges,
        GridLines,
		Zones,
        UI_PinSelection
    }

    public Vector3 PivotPos 
    { 
        get { return GetObject((int)GameObjects.Pivot).transform.localPosition; } 
        private set { GetObject((int)GameObjects.Pivot).transform.localPosition = value; } 
    }
    public float Scale { get; private set; } = 5.0f;

	Vector3 _clickPoint;
    float _fadeDuration = 0.4f;

	int _defaultPinMarkerId = 8;
	UI_Map_Node _selectedNode;

    Coroutine _coShow;

	public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        GetImage((int)Images.Radar).gameObject.BindEvent(OnPointerDown, Define.UIEvent.PointerDown);
		GetImage((int)Images.Radar).gameObject.BindEvent(OnPointerDrag, Define.UIEvent.Drag);
		GetImage((int)Images.Radar).gameObject.BindEvent(OnPointerClick, Define.UIEvent.Click);

		Vector3 playerPos = Managers.Object.GetPlayer().transform.position;
        PivotPos = -WorldToRect(playerPos);

        Managers.Sound.Play("UISOUND/MapOpen"); //M키 누를 때
        StartCoroutine(CoFadeOut());
		_coShow = StartCoroutine(CoShowMap());

		_canvas = GetComponent<Canvas>();
		_cursor = GetImage((int)Images.Cursor).GetComponent<RectTransform>();
		_pointerData = new PointerEventData(EventSystem.current);
		OnControlsChanged(Managers.Input.DeviceMode);
		Managers.Input.ControlsChangedHandler -= OnControlsChanged;
		Managers.Input.ControlsChangedHandler += OnControlsChanged;

		Managers.Event.PlayerEvents.OnShowRadar();
    }

    void Update()
    {
        //GetInputKey();

		if (GetImage((int)Images.Cursor).gameObject.activeInHierarchy)
		{
			MoveCursor();
			UpdateHover();
		}


		float scrollWheel = ZoomScale * 0.1f;
		if (scrollWheel != 0.0f)
		{
			Vector3 rectPos = (Vector3)ScreenPointToRectPoint(Input.mousePosition);
			Vector3 rectLocalPos = rectPos - PivotPos;
			Vector3 worldPos = new Vector3(rectLocalPos.x / Scale, 0.0f, rectLocalPos.y / Scale);
			Scale = Mathf.Max(2.0f, Scale + scrollWheel * 4.0f);
			PivotPos = new Vector3(rectPos.x - worldPos.x * Scale, rectPos.y - worldPos.z * Scale, 0.0f);
		}
	}

    void GetInputKey()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			ClosePopupUI();
		//      if (Input.GetMouseButtonDown(1))
		//      {
		//          Vector3 rectLocalPos = (Vector3)ScreenPointToRectPoint(Input.mousePosition);
		//          Vector3 worldPos = RectToWorld(rectLocalPos);

		//          Managers.Resource.Instantiate("Pin", worldPos, Quaternion.identity);
		//}

		//if (Input.GetMouseButtonDown(0))
		//{
		//	if (_selectedNode != null)
		//		_selectedNode.Cancel();

		//	Vector3 rectLocalPos = (Vector3)ScreenPointToRectPoint(Input.mousePosition) - PivotPos;
		//          _clickPoint = rectLocalPos;
		//      }
		//      else if (Input.GetMouseButton(0))
		//      {
		//          Vector3 rectLocalPos = (Vector3)ScreenPointToRectPoint(Input.mousePosition) - PivotPos;
		//          Vector3 dir = rectLocalPos - _clickPoint;
		//          if (dir.magnitude > 0.1f)
		//              PivotPos += dir;
		//      }

		float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0.0f)
        {
            Vector3 rectPos = (Vector3)ScreenPointToRectPoint(Input.mousePosition);
            Vector3 rectLocalPos = rectPos - PivotPos;
            Vector3 worldPos = new Vector3(rectLocalPos.x / Scale, 0.0f, rectLocalPos.y / Scale);
            Scale = Mathf.Max(2.0f, Scale + scrollWheel * 4.0f);
            PivotPos = new Vector3(rectPos.x - worldPos.x * Scale, rectPos.y - worldPos.z * Scale, 0.0f);
        }
    }

    public Vector2 ScreenPointToRectPoint(Vector2 mousePos)
    {
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            mousePos,
			GetComponent<Canvas>().worldCamera,
			out canvasPos
        );

        return canvasPos;
	}

	public void OnControlsChanged(DeviceMode deviceMode)
	{
		if (deviceMode == DeviceMode.KeyboardMouse)
		{
			GetImage((int)Images.Cursor).gameObject.SetActive(false);
		}
		else if (deviceMode == DeviceMode.Gamepad)
		{
			GetImage((int)Images.Cursor).gameObject.SetActive(true);
		}
	}

	public void OnPointerDown(PointerEventData data)
	{
		if (_selectedNode != null)
		{
			_selectedNode.Cancel();
			_selectedNode = null;
			GetObject((int)GameObjects.UI_PinSelection).GetComponent<UI_PinSelection>().CloseUI();
		}

		if (data.button == PointerEventData.InputButton.Right)
		{
			Vector3 rectLocalPos = (Vector3)ScreenPointToRectPoint(data.position) - PivotPos;
			_clickPoint = rectLocalPos;
		}
	}

	public void OnPointerDrag(PointerEventData data)
	{
		if (data.button == PointerEventData.InputButton.Right)
		{
			Vector3 rectLocalPos = (Vector3)ScreenPointToRectPoint(data.position) - PivotPos;
			Vector3 dir = rectLocalPos - _clickPoint;
			if (dir.magnitude > 0.1f)
				PivotPos += dir;
		}
	}

	public void OnPointerClick(PointerEventData data)
	{
		if (data.button == PointerEventData.InputButton.Left)
		{
			Vector3 rectLocalPos = (Vector3)ScreenPointToRectPoint(data.position);
			Vector3 worldPos = RectToWorld(rectLocalPos);

			Minimap_Marker pin = Managers.Resource.Instantiate("Minimap/Minimap_Marker").GetComponent<Minimap_Marker>();
			pin.SetTemplateId(_defaultPinMarkerId, worldPos);
			Managers.Minimap.AddMarker(pin);
		}
	}

	Vector3 WorldToRect(Vector3 worldPos)
    {
        //Vector3 xAxis = Camera.main.transform.right;
        //Vector3 zAxis = Camera.main.transform.forward;
        //zAxis.y = 0;

        //Vector3 transformedPosition = new Vector3(
        //        Vector3.Dot(worldPos, xAxis.normalized),
        //        0,
        //        Vector3.Dot(worldPos, zAxis.normalized)
        //    );

        Vector3 newRectLocalPos = Vector3.zero;
        newRectLocalPos.x = PivotPos.x + worldPos.x * Scale;
        newRectLocalPos.y = PivotPos.y + worldPos.z * Scale;
        return newRectLocalPos;
    }

	Vector3 RectToWorld(Vector3 rectPos)
	{
        Vector3 xAxis = Vector3.right; // Camera.main.transform.right;
        Vector3 zAxis = Vector3.forward; // Camera.main.transform.forward;
		zAxis.y = 0;

		Vector3 transformedPosition = new Vector3(
			(rectPos.x - PivotPos.x) / Scale,
			0,
			(rectPos.y - PivotPos.y) / Scale
		);

		Vector3 worldPosition = (transformedPosition.x * xAxis.normalized) + (transformedPosition.z * zAxis.normalized);

		return worldPosition;
	}


	public void SelectNode(UI_Map_Node node, bool isTemp = false)
	{
		if (_selectedNode != null && _selectedNode != node)
			_selectedNode.Cancel();

		_selectedNode = node;
        if (node.Clicked == false)
            node.Select();

		GetObject((int)GameObjects.UI_PinSelection).GetComponent<UI_PinSelection>().ShowUI(node, isTemp);
	}

	public void AddMinimapObject(Minimap_Marker obj)
    {
        UI_Map_Node node = Managers.UI.MakeSubItem<UI_Map_Node>(GetObject((int)GameObjects.Nodes).transform);
        node.SetInfo(obj);

        if (obj.IsPin)
            SelectNode(node, true);
    }

    public void AddMinimapObject(MinimapObject_Edge edge)
    {
        UI_Map_Edge e = Managers.UI.MakeSubItem<UI_Map_Edge>(GetObject((int)GameObjects.Edges).transform);
        e.SetInfo(edge);
    }

	public void AddMinimapObject(Minimap_Zone zone)
	{
		UI_Map_Zone z = Managers.UI.MakeSubItem<UI_Map_Zone>(GetObject((int)GameObjects.Zones).transform);
		z.SetInfo(zone);
	}


	IEnumerator CoFadeOut()
    {
        float elapsedTime = 0f;
        Image fadeImage = GetImage((int)Images.Radar);
        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / _fadeDuration * 0.95f);
            fadeImage.color = color;
            yield return null;
        }
    }

    IEnumerator CoFadeIn()
    {
        float elapsedTime = 0f;
        Image fadeImage = GetImage((int)Images.Radar);
        Color color = fadeImage.color;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1.0f - Mathf.Clamp01(elapsedTime / _fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
    }

    IEnumerator CoShowMap()
	{
		Minimap_Marker[] markers = Managers.Minimap.Markers.ToArray();
		MinimapObject_Edge[] edges = Managers.Minimap.Edges.ToArray();
		Minimap_Zone[] zones = Managers.Minimap.Zones.ToArray();

		yield return new WaitForSeconds(_fadeDuration);

		foreach (Minimap_Marker marker in markers)
        {
            if (marker.gameObject.activeSelf == false)
                continue;
            UI_Map_Node node = Managers.UI.MakeSubItem<UI_Map_Node>(GetObject((int)GameObjects.Nodes).transform);
            node.SetInfo(marker);
			//yield return new WaitForSeconds(0.25f / objs.Length);
		}

		{
			UI_Map_Node node = Managers.UI.MakeSubItem<UI_Map_Node>(GetObject((int)GameObjects.Nodes).transform);
			node.SetInfo(Managers.Minimap.PlayerMarker);
		}

		foreach (MinimapObject_Edge edge in edges)
        {
            UI_Map_Edge e = Managers.UI.MakeSubItem<UI_Map_Edge>(GetObject((int)GameObjects.Edges).transform);
            e.SetInfo(edge);
			//yield return new WaitForSeconds(0.25f / edges.Length);
		}

		foreach (Minimap_Zone zone in zones)
		{
			UI_Map_Zone z = Managers.UI.MakeSubItem<UI_Map_Zone>(GetObject((int)GameObjects.Zones).transform);
			z.SetInfo(zone);
		}

		int rowSize = Managers.Map.RowSize;
		int colSize = Managers.Map.ColSize;
		for (int i = 0; i < rowSize; i += 2)
		{
			Vector3 firstPos = Managers.Map.GridToWorld(new Vector2Int(0, i));
			Vector3 secondPos = Managers.Map.GridToWorld(new Vector2Int(colSize - 1, i));
			UI_Map_GridLine gl = Managers.UI.MakeSubItem<UI_Map_GridLine>(GetObject((int)GameObjects.GridLines).transform);
			gl.SetInfo(firstPos, secondPos);
		}
		for (int i = 0; i < colSize; i += 2)
		{
			Vector3 firstPos = Managers.Map.GridToWorld(new Vector2Int(i, 0));
			Vector3 secondPos = Managers.Map.GridToWorld(new Vector2Int(i, rowSize - 1));
			UI_Map_GridLine gl = Managers.UI.MakeSubItem<UI_Map_GridLine>(GetObject((int)GameObjects.GridLines).transform);
			gl.SetInfo(firstPos, secondPos);
		}

		_coShow = null;
    }

	public override void Clear()
	{
		GetObject((int)GameObjects.UI_PinSelection).GetComponent<UI_PinSelection>().CloseUI();
		Managers.Input.ControlsChangedHandler -= OnControlsChanged;
	}


	#region GamePad

	Canvas _canvas;
	RectTransform _cursor;
	float _moveSpeed = 800;
	private GameObject _currentHoverTarget;
	private GameObject _currentPressTarget;
	private PointerEventData _pointerData;
	private float _inputRepeatDelay = 0.2f;
	private float _lastInputTime = 0f;
	private Vector2 _prevInput = Vector2.zero;

	void MoveCursor()
	{
		UI_PinSelection pinSelectionUI = GetObject((int)GameObjects.UI_PinSelection).GetComponent<UI_PinSelection>();
		if (pinSelectionUI.IsActive)
		{
			int x = Mathf.RoundToInt(MoveDir.x);
			int y = Mathf.RoundToInt(MoveDir.y);

			if ((new Vector2(x, y) != _prevInput && (x != 0 || y != 0)) || Time.time - _lastInputTime > _inputRepeatDelay)
			{
				pinSelectionUI.MoveCursor(x, y);
				_lastInputTime = Time.time;
			}

			_prevInput = new Vector2(x, y);
			return;
		}
		
		if (MoveDir.magnitude < 0.3f)
			TrySnapCursor();
		else
			GetObject((int)GameObjects.Pivot).GetComponent<RectTransform>().anchoredPosition += -MoveDir * _moveSpeed * Time.unscaledDeltaTime;
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
		UI_PinSelection pinSelectionUI = GetObject((int)GameObjects.UI_PinSelection).GetComponent<UI_PinSelection>();
		if (pinSelectionUI.IsActive)
			return;

		_currentPressTarget = RaycastUI();
		if (_currentPressTarget != null)
		{
			_pointerData.pointerPressRaycast = new RaycastResult { gameObject = _currentPressTarget };
			_pointerData.pressPosition = _pointerData.position = GetCursorScreenPosition();
			_pointerData.pointerPress = _currentPressTarget;
			_pointerData.button = inputButton;

			ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(_currentPressTarget, _pointerData, ExecuteEvents.pointerDownHandler);
		}
	}

	public override void OnPointerUpAndClick(PointerEventData.InputButton inputButton)
	{
		UI_PinSelection pinSelectionUI = GetObject((int)GameObjects.UI_PinSelection).GetComponent<UI_PinSelection>();
		if (pinSelectionUI.IsActive)
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
			_pointerData.pointerDrag = null;
		}
	}

	public override void OnOkButton()
	{
		UI_PinSelection pinSelectionUI = GetObject((int)GameObjects.UI_PinSelection).GetComponent<UI_PinSelection>();
		if (pinSelectionUI.IsActive)
		{
			pinSelectionUI.OnOkButtonClick(_pointerData);
		}
	}

	public override void OnCancelButton()
	{
		if (_selectedNode != null)
		{
			_selectedNode.Cancel();
			_selectedNode = null;
			GetObject((int)GameObjects.UI_PinSelection).GetComponent<UI_PinSelection>().CloseUI();
		}
	}

	public override void OnDeleteButton()
	{
		UI_PinSelection pinSelectionUI = GetObject((int)GameObjects.UI_PinSelection).GetComponent<UI_PinSelection>();
		if (pinSelectionUI.IsActive)
		{
			pinSelectionUI.OnDeleteButtonClick(_pointerData);
		}
	}

	void TrySnapCursor()
	{
		float snapRadius = Screen.height * 0.05f;
		UI_SnapTarget snapTarget = FindClosestSnapTarget(snapRadius);

		if (snapTarget != null)
		{
			Vector2 snapPos = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, snapTarget.Rect.position);

			Vector2 anchored;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, snapPos, _canvas.worldCamera, out anchored);
			RectTransform Pivot = GetObject((int)GameObjects.Pivot).GetComponent<RectTransform>();
			Pivot.anchoredPosition = Pivot.anchoredPosition - anchored;
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
	#endregion
}
