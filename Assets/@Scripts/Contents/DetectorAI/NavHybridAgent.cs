using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class NavHybridAgent : MonoBehaviour
{
    // Public API (NavMeshAgent 호환)
    public float speed { get => _speed; set { _speed = value; if (_navAgent) _navAgent.speed = value; } }
    public float angularSpeed { get => _navAgent.angularSpeed; set { if (_navAgent) _navAgent.angularSpeed = value; } }
	public bool isStopped { get => _isStopped; set { _isStopped = value; if (gameObject.activeInHierarchy && _navAgent.enabled) _navAgent.isStopped = value; } }
    public float stoppingDistance = 0.5f;
    public bool pathPending { get; private set; } = false;
    public bool hasPath { get; private set; } = false;
    public float remainingDistance { get { return ComputeRemainingDistance(); } }
    public bool updateRotation
    {
        get
        {
            if (_navAgent != null)
                return _navAgent.updateRotation;
            else
                return false;
        }
        set
        {
            if (_navAgent != null)
                _navAgent.updateRotation = value;
        }
	}
	public NavMeshPath path
    {
        get
        {
            if (_isUsingNav && _navAgent != null && _navAgent.enabled)
                return _navAgent.path;
            else
                return null;
        }
	}
    public Vector3 velocity 
    {
        get
        {
            if (_isUsingNav && _navAgent != null && _navAgent.enabled)
                return _navAgent.velocity;
            else
                return Vector3.zero;
        }
        set
        {
            if (_isUsingNav && _navAgent != null && _navAgent.enabled)
                _navAgent.velocity = value;
		}
	}

	// 내부
	NavMeshAgent _navAgent;
    Coroutine _followRoutine;
    Coroutine _repathRoutine;
	List<Vector2Int> _cellPath;         // 전체 셀 경로 (A* 결과)
    List<Vector2Int> _compressedCells;  // 압축된 셀 경로
    int _currentSegmentIndex = 0;
    Vector3 _destination;
    float _speed = 3.5f;
    bool _isStopped = false;
    bool _isUsingNav = false; // 현재 프레임 NavMeshAgent로 이동중인지

    void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.speed = _speed;
    }

	private void OnEnable()
	{
		// 이벤트 구독: 셀 로드/언로드 / maze changed
		Managers.Event.GameEvents.LoadOrUnloadContentAction += OnLoadOrUnloadContent;
		Managers.Event.GameEvents.MazeChangedAction += OnMazeChanged;
	}

	void OnDisable()
	{
		Managers.Event.GameEvents.LoadOrUnloadContentAction -= OnLoadOrUnloadContent;
		Managers.Event.GameEvents.MazeChangedAction -= OnMazeChanged;
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            enabled = true;
        }
        else
        {
            _navAgent.enabled = false;
            enabled = false;
        }
    }

	float _nextPathUpdateTime = 0f;
	float _pathUpdateInterval = 0.5f; // 0.5초마다 갱신 허용
	// 외부 API - NavMeshAgent 호환
	public void SetDestination(Vector3 worldTarget)
	{
		if (Time.time < _nextPathUpdateTime)
			return;
		_nextPathUpdateTime = Time.time + _pathUpdateInterval;

		_destination = worldTarget;
        StartPathfinding();
    }

    public void Move(Vector3 offset)
    {
        if (_navAgent != null && _navAgent.enabled)
			_navAgent.Move(offset);
        else
            transform.position += offset;
    }

    public void ResetPath()
    {
        StopFollowing();
        _cellPath = null;
        _compressedCells = null;
        hasPath = false;
        pathPending = false;
    }

	public bool Warp(Vector3 newPosition)
	{
		// 1) 이동 중단
		_isStopped = true;

		// 2) 기존 코루틴 중단
		if (_followRoutine != null)
		{
			StopCoroutine(_followRoutine);
			_followRoutine = null;
		}

		// 3) transform 위치 강제 변경
		transform.position = newPosition;

		// 4) NavMeshAgent가 활성 상태라면 nextPosition도 동기화
		if (_navAgent != null)
		{
			if (_navAgent.enabled)
			{
				_navAgent.Warp(newPosition);
				// ※ Unity NavMeshAgent.Warp() 호출해줘도 됨 (state 초기화 가능)
			}
			else
			{
				// agent가 꺼져 있다면 다음에 켜질 때 위치 튀는 문제 방지
				_navAgent.nextPosition = newPosition;
			}
		}

		// 5) HybridAgent 내부 상태 초기화
		hasPath = false;
		pathPending = false;
		_cellPath = null;
		_compressedCells = null;
		_currentSegmentIndex = 0;

		return true;
	}

	// --- Pathfinding & Following ---
	void StartPathfinding()
    {
        pathPending = true;
        hasPath = false;

        // 1) 시작/목표 셀 결정
        Vector2Int startCell = Managers.Map.WorldToGrid(transform.position);
        Vector2Int targetCell = Managers.Map.WorldToGrid(_destination);

        // 샘플: target 토지 유효성 보정(지도 외곽 등)
        if (!Managers.Map.IsValidGridPos(targetCell))
		{
			StopFollowing();
			pathPending = false;
            hasPath = false;
            return;
        }

        // 2) A*로 전체 셀 경로 계산 (동기 혹은 코루틴으로)
        _cellPath = Managers.Map.FindPath(startCell, targetCell);
        pathPending = false;

        if (_cellPath == null || _cellPath.Count == 0)
		{
			StopFollowing();
			hasPath = false;
            return;
		}

        // 경로가 진짜 목표까지 이어졌는지 검증
        Vector2Int lastCell = _cellPath[_cellPath.Count - 1];
        bool reachedFullTarget = (lastCell == targetCell);
        if (!reachedFullTarget)
        {
            if (_repathRoutine != null)
                StopCoroutine(_repathRoutine);
            _repathRoutine = StartCoroutine(DelayedRepath(_destination, 1.0f)); // 1초 후 재시도
        }

        // 3) 압축
        _compressedCells = CompressPathByLoadedSegments(_cellPath);
        _currentSegmentIndex = 0;
        hasPath = true;

		// 4) 시작 이동 코루틴
		if (_followRoutine != null)
			StopCoroutine(_followRoutine);
		_followRoutine = StartCoroutine(FollowCompressedPath());
    }

    IEnumerator FollowCompressedPath()
    {
        Vector2Int targetCell = Managers.Map.WorldToGrid(_destination);

		while (_currentSegmentIndex < _compressedCells.Count && hasPath)
        {
            if (_isStopped)
            {
                yield return null;
                continue;
            }

            Vector2Int curCell = _compressedCells[_currentSegmentIndex];
            Vector2Int nextCell = (_currentSegmentIndex + 1 < _compressedCells.Count)
                                  ? _compressedCells[_currentSegmentIndex + 1]
                                  : curCell;

            Vector3 nextWorldPos = targetCell == nextCell
                                  ? _destination
                                  : Managers.Map.GridToWorld(nextCell);

			bool curLoaded = Managers.Map.IsCellLoaded(curCell);
            bool nextLoaded = Managers.Map.IsCellLoaded(nextCell);

			bool arrived = false;

			// 조건: 둘다 loaded -> NavMeshAgent 사용
			if (curLoaded && nextLoaded)
            {
                // enable agent
                if (!_navAgent.enabled) _navAgent.enabled = true;
                _isUsingNav = true;
                _navAgent.SetDestination(nextWorldPos);
                _navAgent.isStopped = false;

                // 기다림: agent가 도달하거나 path 이상 상태일 때 탈출
                while (!_isStopped && _navAgent.pathPending)
                    yield return null;

				// wait until close enough or path invalid/partial
				while (!_isStopped && _navAgent.hasPath && _navAgent.pathStatus == NavMeshPathStatus.PathComplete)
                {
                    // 만약 nextCell가 unload되면 break -> switch to direct move
                    if (!Managers.Map.IsCellLoaded(nextCell))
                        break;

					if (_navAgent.remainingDistance <= stoppingDistance)
					{
						arrived = true;
						break;
					}
					yield return null;
                }
            }
            else
            {
                // direct move (transform-controlled)
                if (_navAgent.enabled) { _navAgent.isStopped = true; _navAgent.enabled = false; }
                _isUsingNav = false;

                // move until within stoppingDistance of nextWorldPos
                while (!_isStopped)
                {
                    Vector3 dir = (nextWorldPos - transform.position);
                    float dist = dir.magnitude;
                    if (dist <= stoppingDistance)
					{
						arrived = true;
						break;
					}

					Vector3 move = dir.normalized * _speed * Time.deltaTime;
                    if (move.magnitude > dist) move = dir; // don't overshoot
                    transform.position += move;

                    // update navAgent next position if agent later enabled
                    if (_navAgent.enabled) _navAgent.nextPosition = transform.position;

                    // if nextCell became loaded and current cell loaded -> we can switch to NavMeshAgent next loop
                    if (Managers.Map.IsCellLoaded(nextCell) && Managers.Map.IsCellLoaded(Managers.Map.WorldToGrid(transform.position)))
                        break;

                    yield return null;
                }
            }

			if (arrived)
				_currentSegmentIndex++; 
			else
				yield return null;
		}

        // 경로 끝 도달
        hasPath = false;
        yield break;
    }

    // 압축 알고리즘: 연속된 loaded-segment는 처음/끝만 남김
    List<Vector2Int> CompressPathByLoadedSegments(List<Vector2Int> path, int startIndex = 0)
    {
        List<Vector2Int> outList = new List<Vector2Int>();
        int i = startIndex;
        while (i < path.Count)
        {
            Vector2Int cur = path[i];
            bool curLoaded = Managers.Map.IsCellLoaded(cur);

			if (!curLoaded)
            {
                outList.Add(cur);
                i++;
                continue;
            }
            // curLoaded == true, find run of consecutive loaded cells
            int j = i;

			while (j + 1 < path.Count && Managers.Map.IsCellLoaded(path[j + 1]))
                j++;

            // add start
            outList.Add(path[i]);
            // if run length > 1 add end (skip internals)
            if (j > i)
                outList.Add(path[j]);

            i = j + 1;
        }
        // remove duplicates and adjacent identical entries
        return outList;
    }

    float ComputeRemainingDistance()
    {
        if (_compressedCells == null || _compressedCells.Count == 0)
            return 0f;

		// 현재 위치에서 다음 waypoint까지 + 남은 세그먼트 거리 합
		Vector3 curPos = transform.position;
		float rem = 0f;

		int idx = Mathf.Max(0, _currentSegmentIndex + 1);
		for (int i = idx; i < _compressedCells.Count - 1; i++)
		{
			Vector3 nextWorld = Managers.Map.GridToWorld(_compressedCells[i]);
			rem += Vector3.Distance(curPos, nextWorld);
			curPos = nextWorld;
		}
		rem += Vector3.Distance(curPos, _destination);
		return rem;
    }

    void StopFollowing()
    {
		if (_followRoutine != null) StopCoroutine(_followRoutine);
        _followRoutine = null;
        if (_navAgent && _navAgent.enabled) { _navAgent.isStopped = true; _navAgent.ResetPath(); }
    }

	//bool HasNavMeshAt(Vector2Int girdPos, float maxDistance = 1f, int areaMask = NavMesh.AllAreas)
	//{
 //       Vector3 position = Managers.Map.GridToWorld(girdPos);
	//	return HasNavMeshAt(position, maxDistance, areaMask);
	//}

	//bool HasNavMeshAt(Vector3 position, float maxDistance = 1f, int areaMask = NavMesh.AllAreas)
	//{
	//	NavMeshHit hit;
	//	return NavMesh.SamplePosition(position, out hit, maxDistance, areaMask);
	//}

	IEnumerator DelayedRepath(Vector3 destPos, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_destination != destPos)
            yield break; // 목적지가 바뀌었음
        StartPathfinding();
	}

	// 이벤트: 셀 로드/언로드/맵 변경 시 경로 재검증
	void OnLoadOrUnloadContent() { ValidateOrRebuildPath(true); }
    void OnMazeChanged() { ValidateOrRebuildPath(false); }

    void ValidateOrRebuildPath(bool recompress)
    {
        if (!hasPath)
            return;

        if (recompress)
            RecompressCurrentPath();
        else
            StartPathfinding();
	}

	void RecompressCurrentPath()
	{
        int idx = 0;
        Vector2Int curGridPos = Managers.Map.WorldToGrid(transform.position);

		// 현재 위치에 가장 가까운 셀부터 재압축 시작
        for (idx = 0; idx < _cellPath.Count; idx++)
        {
            if (_cellPath[idx] == curGridPos)
                break;
        }

		// 만약 현재 위치가 경로에 없다면 전체 재탐색 (경로이탈)
		if (idx >= _cellPath.Count)
        {
            StartPathfinding();
            return;
		}

		_compressedCells = CompressPathByLoadedSegments(_cellPath, idx);
		_currentSegmentIndex = 0;

		if (_followRoutine != null)
			StopCoroutine(_followRoutine);
		_followRoutine = StartCoroutine(FollowCompressedPath());
	}

	void OnDrawGizmos()
	{
#if UNITY_EDITOR
		if (_compressedCells == null || _compressedCells.Count < 2)
			return;

        Vector2Int targetCell = Managers.Map.WorldToGrid(_destination);
		Handles.color = Color.cyan;
		for (int i = _currentSegmentIndex; i < _compressedCells.Count - 1; i++)
		{
			Vector3 from = i == _currentSegmentIndex 
                            ? transform.position
                            : Managers.Map.GridToWorld(_compressedCells[i]);
			Vector3 to = _compressedCells[i + 1] == targetCell
                        ? _destination
                        : Managers.Map.GridToWorld(_compressedCells[i + 1]);
			Handles.DrawAAPolyLine(4f, new Vector3[] { from + Vector3.up * 0.2f, to + Vector3.up * 0.2f });
		}

		for (int i = _currentSegmentIndex + 1; i < _compressedCells.Count; i++)
		{
			Vector3 worldPos = _compressedCells[i] == targetCell
                                ? worldPos = _destination
                                : Managers.Map.GridToWorld(_compressedCells[i]);
			Handles.SphereHandleCap(0, worldPos + Vector3.up * 0.2f, Quaternion.identity, 0.2f, EventType.Repaint);
			Handles.Label(worldPos + Vector3.up * 0.5f, $"{i}");
		}
#endif
	}
}