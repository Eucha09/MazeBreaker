using Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_InteractionMap : UI_Popup
{
    enum Images
    {
        Radar,
    }
    
    enum GameObjects
    { 
        Pivot,
        Nodes,
        Edges,
        GridLines,
        UI_InteractionMenu,
        UI_InteractionDir,
    }

    public Vector3 PivotPos 
    { 
        get { return GetObject((int)GameObjects.Pivot).transform.localPosition; } 
        private set { GetObject((int)GameObjects.Pivot).transform.localPosition = value; } 
    }
    public float Scale { get; private set; } = 6.0f;
    public Dictionary<StarPiece, int> OnThePath { get; set; } = new Dictionary<StarPiece, int>();

    PlayerController _player;

	float _pivotDistance;
	float _pivotAngle;
	Vector3 _cameraForward;
	Vector3 _clickPoint;

    GameObject _targetTracker;
    Vector3 _targetPos;

	float _fadeDuration = 0.4f;
    Coroutine _coFade;
    bool _closed;

    UI_InteractionMap_Node _selectedNode;
    GameObject _selectedObject;
    GameObject _hoveringObject;
    InteractionType _selectedInteractionType = InteractionType.None;

	public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

		GetImage((int)Images.Radar).gameObject.SetActive(false);

        _player = Managers.Object.GetPlayer();
        PivotPos = -WorldToRect(_player.transform.position);

        Managers.Sound.Play("UISOUND/MapOpen"); //M키 누를 때

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.HideUI();

		//_coFade = StartCoroutine(CoFadeOut());
        //StartCoroutine(CoShowMap());
    }

    void Update()
    {
        if (_closed)
            return;

        if (!(_player.CurrentState is PlayerInteractionSkillReadyState || _player.CurrentState is PlayerInteractionSkillUsingState))
            ClosePopupUI();

        if (_selectedInteractionType == InteractionType.Interact && _selectedObject == null)
        {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			bool raycastHit = Physics.Raycast(ray, out hit, 100.0f, 1 << (int)Define.Layer.Block);
            if (raycastHit && hit.collider.GetComponentInParent<PatternedWall>())
            {
                PatternedWall wall = hit.collider.GetComponentInParent<PatternedWall>();
                Vector2Int gridPos = Managers.Map.WorldToGrid(_selectedNode.Target.transform.position);
                if ((wall.GridPos - gridPos).sqrMagnitude <= 1 && wall.IsMoving == false)
                {
                    if (_hoveringObject != null)
                        _hoveringObject.GetComponent<SelectedOutline>().ClearOutline();
                    _hoveringObject = wall.gameObject;
                    _hoveringObject.GetComponent<SelectedOutline>().DrawOutline();
                }

            }
            else if (_hoveringObject != null)
            {
                _hoveringObject.GetComponent<SelectedOutline>().ClearOutline();
                _hoveringObject = null;
            }
		}

		GetInputKey();

		if (_targetTracker != null)
            _targetTracker.transform.position = Vector3.Lerp(_targetTracker.transform.position, _targetPos, Time.deltaTime * 2.0f);
	}

    void GetInputKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OnEsc();

        if (Input.GetMouseButtonDown(1))
        {
            // 피벗 거리, 피벗 각도, 카메라 각도
            _pivotDistance = PivotPos.magnitude;
            _pivotAngle = Vector3.SignedAngle(GetImage((int)Images.Radar).transform.right, PivotPos, Vector3.forward);
			_cameraForward = Camera.main.transform.forward;
			_cameraForward.y = 0;
		}
        else if (Input.GetMouseButton(1))
        {
            Vector3 curForward = Camera.main.transform.forward;
			curForward.y = 0;
            float cameraAngle = Vector3.SignedAngle(_cameraForward, curForward, Vector3.up);
            float radian = (_pivotAngle + cameraAngle) * Mathf.Deg2Rad;
            Vector3 newPos = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0.0f) * _pivotDistance;
            PivotPos = newPos;
		}
        else if (Input.GetMouseButtonDown(0))
        {
            Vector3 rectLocalPos = (Vector3)ScreenPointToRectPoint(Input.mousePosition) - PivotPos;
            _clickPoint = rectLocalPos;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 rectLocalPos = (Vector3)ScreenPointToRectPoint(Input.mousePosition) - PivotPos;
            Vector3 dir = rectLocalPos - _clickPoint;
            if (dir.magnitude > 0.1f)
                PivotPos += dir;
		}

		if (Input.GetMouseButtonUp(0))
        {
            if (_selectedInteractionType == InteractionType.Interact && _hoveringObject != null)
            {
                SelectObject(_hoveringObject);
                _hoveringObject = null;
			}
        }

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

    public void ShowRadar()
	{
		_coFade = StartCoroutine(CoFadeOut());
		StartCoroutine(CoShowMap());
	}

    public void SelectNode(UI_InteractionMap_Node node)
    {
        if (_selectedNode != null && _selectedNode != node)
			_selectedNode.Cancel();

		_selectedNode = node;
		// 카메라 컨트롤
		Camera.main.GetComponent<MainCameraController>().SetTarget(node.Target, Define.CameraType.Follow);
		// 메뉴 띄우기
		ShowInteractionMenu(node.Target);


  //      if (_selectedInteractionType == InteractionType.None)
  //      {
  //          _selectedNode = node;
  //          // 메뉴 띄우기
  //          ShowInteractionMenu(node);
  //          // 관련 없는 애들은 비활성화
  //          ActiveInteractableObjects();
  //      }
  //      else if (_selectedInteractionType == InteractionType.Exchange) // 이제 필요없을듯
  //      {
  //          StarPiece star = _selectedNode.Target.GetComponent<StarPiece>();
  //          if (star != null)
  //          {
  //              OnThePath[star]--;
  //              if (OnThePath[star] == 0)
  //                  OnThePath.Remove(star);
  //          }
  //          star = node.Target.GetComponent<StarPiece>();
  //          if (star != null)
  //          {
  //              OnThePath[star]--;
  //              if (OnThePath[star] == 0)
  //                  OnThePath.Remove(star);
  //          }
  //          Managers.Interact.Exchange(_selectedNode.Target, node.Target, OnThePath.Keys.ToList());
		//	ClosePopupUI();
		//}
	}

    public void SelectInteractionType(InteractionType type)
    {
        _selectedInteractionType = type;
        if (type == InteractionType.Interact)
        {
            Vector2Int gridPos = Managers.Map.WorldToGrid(_selectedNode.Target.transform.position);
            _targetPos = Managers.Map.GridToWorld(gridPos);
            if (_targetTracker == null)
                _targetTracker = new GameObject("InteractionTargetTracker");
            _targetTracker.transform.position = _targetPos;
			Camera.main.GetComponent<MainCameraController>().SetTarget(_targetTracker, Define.CameraType.TopDown);
			
			GetObject((int)GameObjects.UI_InteractionMenu).SetActive(false);
			if (_coFade != null)
				StopCoroutine(_coFade);
			_coFade = StartCoroutine(CoFadeIn());
		}
		else if (type == InteractionType.Push || type == InteractionType.Fall)
		{
            GetObject((int)GameObjects.UI_InteractionMenu).SetActive(false);
            UI_InteractionDir interactionDirUI = GetObject((int)GameObjects.UI_InteractionDir).GetComponent<UI_InteractionDir>();
            interactionDirUI.gameObject.SetActive(true);
            interactionDirUI.SetTarget(_selectedObject);
			//_interactionDirUI = Managers.UI.MakeSubItem<UI_InteractionDir>(transform);
			//_interactionDirUI.SetNode(_selectedNode);
		}
        else if (type == InteractionType.Up)
        {
            Managers.Interact.Up(_selectedObject.GetComponent<PatternedWall>(), OnThePath.Keys.ToList());
			ClosePopupUI(2.0f);
		}
        else if (type == InteractionType.Down)
		{
			Managers.Interact.Down(_selectedObject.GetComponent<PatternedWall>(), OnThePath.Keys.ToList());
			ClosePopupUI(2.0f);
		}
        else if (type == InteractionType.Teleport)
        {
            Managers.Interact.Teleport(_selectedNode.Target.transform.position, OnThePath.Keys.ToList());
            ClosePopupUI();
			//ActiveInteractableObjects();
		}
    }

    public void SelectObject(GameObject go)
    {
        _selectedObject = go;
        _targetPos = go.transform.position;

        if (_selectedNode == null)
        {
            if (_targetTracker == null)
                _targetTracker = new GameObject("InteractionTargetTracker");
            _targetTracker.transform.position = _targetPos;
            Camera.main.GetComponent<MainCameraController>().SetTarget(_targetTracker, Define.CameraType.TopDown);
        }

		ShowInteractionMenu(go);
    }

    public void SelectDirection(Vector3 dir)
    {
        if (_selectedInteractionType == InteractionType.Push)
        {
            Managers.Interact.Push(_selectedObject.GetComponent<PatternedWall>(), dir, OnThePath.Keys.ToList());
			ClosePopupUI(2.0f);
		}
        else  if (_selectedInteractionType == InteractionType.Fall)
		{
			Managers.Interact.Fall(_selectedObject.GetComponent<PatternedWall>(), dir, OnThePath.Keys.ToList());
			ClosePopupUI(2.0f);
		}
	}

  //  public void ActiveInteractableObjects()
  //  {
  //      if (_selectedInteractionType == InteractionType.None)
		//{
		//	int nodeCount = GetObject((int)GameObjects.Nodes).transform.childCount;
		//	for (int i = 0; i < nodeCount; i++)
		//	{
		//		UI_InteractionMap_Node child = GetObject((int)GameObjects.Nodes).transform.GetChild(i).GetComponent<UI_InteractionMap_Node>();
  //              if (_selectedNode == null || child == _selectedNode)
  //                  child.Active = true;
  //              else
  //                  child.Active = false;
		//	}
		//	int edgeCount = GetObject((int)GameObjects.Edges).transform.childCount;
		//	for (int i = 0; i < edgeCount; i++)
		//	{
		//		UI_InteractionMap_Edge child = GetObject((int)GameObjects.Edges).transform.GetChild(i).GetComponent<UI_InteractionMap_Edge>();
  //              if (_selectedNode == null)
  //                  child.Active = true;
  //              else
  //  				child.Active = false;
		//	}
		//}
  //      else if (_selectedInteractionType == InteractionType.Exchange)
		//{
		//	int nodeCount = GetObject((int)GameObjects.Nodes).transform.childCount;
		//	for (int i = 0; i < nodeCount; i++)
		//	{
		//		UI_InteractionMap_Node child = GetObject((int)GameObjects.Nodes).transform.GetChild(i).GetComponent<UI_InteractionMap_Node>();
  //              if (_selectedNode.Target.GetComponent<MazeWall>() != null && child.Target.GetComponent<MazeWall>() != null)
  //                  child.Active = true;
  //              else if (_selectedNode.Target.GetComponent<MazeWall>() == null && child.Target.GetComponent<MazeWall>() == null)
  //                  child.Active = true;
  //              else
  //                  child.Active = false;
		//	}
		//	int edgeCount = GetObject((int)GameObjects.Edges).transform.childCount;
		//	for (int i = 0; i < edgeCount; i++)
		//	{
		//		UI_InteractionMap_Edge child = GetObject((int)GameObjects.Edges).transform.GetChild(i).GetComponent<UI_InteractionMap_Edge>();
		//		child.Active = false;
		//	}
		//}
  //  }

	public void ShowInteractionMenu(GameObject go)
	{
		UI_InteractionMenu ui = GetObject((int)GameObjects.UI_InteractionMenu).GetComponent<UI_InteractionMenu>();
		ui.gameObject.SetActive(true);
		ui.SetTarget(go);
    }

 //   public void Cancel()
 //   {
 //       if (_selectedNode != null)
 //       {
 //           _selectedNode.Cancel();
 //           _selectedNode = null;
	//	}
        
	//	GetObject((int)GameObjects.UI_InteractionMenu).SetActive(false);
 //       GetObject((int)GameObjects.UI_InteractionDir).SetActive(false);
	//	_selectedInteractionType = InteractionType.None;
 //       OnThePath.Clear();

 //       ActiveInteractableObjects();
	//}

	void OnEsc()
	{

		if (_selectedInteractionType != InteractionType.None && _selectedInteractionType != InteractionType.Interact)
		{
            _selectedInteractionType = InteractionType.Interact;
			GetObject((int)GameObjects.UI_InteractionDir).SetActive(false);
            ShowInteractionMenu(_selectedObject);
		}
        else if (_selectedObject != null && _selectedNode == null)
		{
			ClosePopupUI();
		}
		else if (_selectedObject != null)
        {
            _selectedObject.GetComponent<SelectedOutline>().ClearOutline();
            _selectedObject = null;

            Vector2Int gridPos = Managers.Map.WorldToGrid(_selectedNode.Target.transform.position);
			_targetPos = Managers.Map.GridToWorld(gridPos);

			GetObject((int)GameObjects.UI_InteractionMenu).SetActive(false);

		}
        else if (_selectedInteractionType == InteractionType.Interact)
		{
            _selectedInteractionType = InteractionType.None;

			if (_hoveringObject != null)
			{
				_hoveringObject.GetComponent<SelectedOutline>().ClearOutline();
                _hoveringObject = null;
			}

			Camera.main.GetComponent<MainCameraController>().SetCameraType(Define.CameraType.Follow);

            if (_coFade != null)
                StopCoroutine(_coFade);
			_coFade = StartCoroutine(CoFadeOut());

            ShowInteractionMenu(_selectedNode.Target);
		}
        else if (_selectedNode != null)
		{
			_selectedNode.Cancel();
			_selectedNode = null;

			Camera.main.GetComponent<MainCameraController>().SetTarget(Managers.Object.GetPlayer().gameObject, Define.CameraType.Follow);

			GetObject((int)GameObjects.UI_InteractionMenu).SetActive(false);
			OnThePath.Clear();

		}
        else
        {
            ClosePopupUI();
        }
	}

    public void ShowPath(List<StarPiece> path)
	{
		foreach (StarPiece star in path)
		{
			if (OnThePath.ContainsKey(star))
				OnThePath[star]++;
			else
				OnThePath.Add(star, 1);
		}
	}

    public void ClosePath(List<StarPiece> path)
    {
        foreach (StarPiece star in path)
        {
            if (OnThePath.ContainsKey(star))
            {
                OnThePath[star]--;
                if (OnThePath[star] == 0)
                    OnThePath.Remove(star);
            }
		}
	}

    void ClosePopupUI(float time)
	{
        _closed = true;
		if (_selectedObject != null)
			_selectedObject.GetComponent<SelectedOutline>().ClearOutline();
        _selectedObject = null;

		GetImage((int)Images.Radar).gameObject.SetActive(false);
		GetObject((int)GameObjects.UI_InteractionMenu).SetActive(false);
		GetObject((int)GameObjects.UI_InteractionDir).SetActive(false);

		Invoke("ClosePopupUI", time);
	}

	public override void Clear()
	{
		if (_targetTracker != null)
			Managers.Resource.Destroy(_targetTracker);

        if (_selectedObject != null)
			_selectedObject.GetComponent<SelectedOutline>().ClearOutline();

        if (_hoveringObject != null)
			_hoveringObject.GetComponent<SelectedOutline>().ClearOutline();

		Camera.main.GetComponent<MainCameraController>().SetTarget(Managers.Object.GetPlayer().gameObject, Define.CameraType.Follow);

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.ShowUI();

        if (_player.CurrentState is PlayerInteractionSkillReadyState || _player.CurrentState is PlayerInteractionSkillUsingState)
			_player.CurrentState = new PlayerIdleState();
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
        newRectLocalPos.x = PivotPos.x + transformedPosition.x * Scale;
        newRectLocalPos.y = PivotPos.y + transformedPosition.z * Scale;
        return newRectLocalPos;
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
            color.a = Mathf.Clamp01(elapsedTime / _fadeDuration);
            fadeImage.color = color;
            yield return null;
		}
        color.a = 1.0f;
		fadeImage.color = color;
        _coFade = null;
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
		_coFade = null;
	}

    IEnumerator CoShowMap()
    {
        yield return new WaitForSeconds(_fadeDuration);

        // 플레이어 주변에 있는 별조각을 찾는다.
        // 그 별조각과 연결되어 있는 모든 별조각들을 찾는다.
        // 찾은 별조각과 엣지, 플레이어를 표시한다.
        // 별조각 주변에 있는 벽들을 얻어와 표시한다.

        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };

        PlayerController player = Managers.Object.GetPlayer();
		Vector2Int playerGridPos = player.GridPos;
        Vector3 playerPos = player.transform.position;


		StarPiece nearestStar = null;
		GameObject nearestObject = Managers.Object.FindNearestObjectWithTag("Star", player.transform.position, 18.0f);
		if (nearestObject != null)
			nearestStar = nearestObject.GetComponent<StarPiece>();
  //      float nearestDist = 0.0f;
		//List<MinimapObject> objs = Managers.Minimap.MinimapObjects;
		//foreach (MinimapObject obj in objs)
  //      {
  //          if (obj.IsStarPiece && playerGridPos == Managers.Map.WorldToGrid(obj.StarPiecePos))
  //          {
  //              if (nearestStar == null || nearestDist > Vector3.Distance(playerPos, obj.StarPiecePos))
  //              {
  //                  nearestStar = obj.SyncPositionTarget.GetComponent<StarPiece>();
  //                  nearestDist = Vector3.Distance(playerPos, obj.StarPiecePos);
		//			break;
  //              }
		//	}
  //      }

        Dictionary<Vector2Int, bool> nearbyWall = new Dictionary<Vector2Int, bool>();
		if (nearestStar != null)
        {
			List<GameObject> connectedObjects = Managers.Star.FindAllConnectedStarsAndEdges(nearestStar);
            foreach (GameObject go in connectedObjects)
            {
                if (go.GetComponent<Minimap_Marker>())
				{
					UI_InteractionMap_Node node = Managers.UI.MakeSubItem<UI_InteractionMap_Node>(GetObject((int)GameObjects.Nodes).transform);
					node.SetInfo(go.GetComponent<Minimap_Marker>());
					Vector2Int cur = Managers.Map.WorldToGrid(go.transform.position);
					for (int i = 0; i < 4; i++)
					{
						Vector2Int next = new Vector2Int(cur.x + dx[i], cur.y + dy[i]);
						if (!nearbyWall.ContainsKey(next))
							nearbyWall.Add(next, true);
					}
				}
                else if (go.GetComponent<StarEdge>())
				{
					UI_InteractionMap_Edge e = Managers.UI.MakeSubItem<UI_InteractionMap_Edge>(GetObject((int)GameObjects.Edges).transform);
					e.SetInfo(go.GetComponent<StarEdge>());
				}
            }
        }

        {
			UI_InteractionMap_Node node = Managers.UI.MakeSubItem<UI_InteractionMap_Node>(GetObject((int)GameObjects.Nodes).transform);
            node.SetInfo(player.GetComponent<Minimap_Marker>());
            Vector2Int cur = playerGridPos;
			for (int i = 0; i < 4; i++)
			{
				Vector2Int next = new Vector2Int(cur.x + dx[i], cur.y + dy[i]);
				if (!nearbyWall.ContainsKey(next))
					nearbyWall.Add(next, true);
			}
		}

   //     foreach (Vector2Int gridPos in nearbyWall.Keys)
   //     {
   //         MazeCell cell = Managers.Map.Maze.Cells[gridPos.y, gridPos.x];
   //         if (cell != null && cell.GetComponent<MinimapObjectGenerator>())
			//{
			//	UI_InteractionMap_Node node = Managers.UI.MakeSubItem<UI_InteractionMap_Node>(GetObject((int)GameObjects.Nodes).transform);
			//	node.SetInfo(cell.GetComponent<MinimapObjectGenerator>());
			//}
   //     }

        int rowSize = Managers.Map.RowSize;
        int colSize = Managers.Map.ColSize;
        for (int i = 0; i < rowSize; i += 2)
        {
            Vector3 firstPos = Managers.Map.GridToWorld(new Vector2Int(0, i));
            Vector3 secondPos = Managers.Map.GridToWorld(new Vector2Int(colSize - 1, i));
            UI_InteractionMap_GridLine gl = Managers.UI.MakeSubItem<UI_InteractionMap_GridLine>(GetObject((int)GameObjects.GridLines).transform);
            gl.SetInfo(firstPos, secondPos);
        }
        for (int i = 0; i < colSize; i += 2)
        {
            Vector3 firstPos = Managers.Map.GridToWorld(new Vector2Int(i, 0));
            Vector3 secondPos = Managers.Map.GridToWorld(new Vector2Int(i, rowSize - 1));
			UI_InteractionMap_GridLine gl = Managers.UI.MakeSubItem<UI_InteractionMap_GridLine>(GetObject((int)GameObjects.GridLines).transform);
            gl.SetInfo(firstPos, secondPos);
        }
    }
}
