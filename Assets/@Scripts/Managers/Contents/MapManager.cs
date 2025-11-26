using Data;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using static UnityEngine.Rendering.DebugUI.Table;
using Random = UnityEngine.Random;

public struct Pos
{
    public Pos(int y, int x) { Y = y; X = x; }
    public int Y;
    public int X;

    public static bool operator ==(Pos lhs, Pos rhs)
    {
        return lhs.Y == rhs.Y && lhs.X == rhs.X;
    }

    public static bool operator !=(Pos lhs, Pos rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
        return (Pos)obj == this;
    }

    public override int GetHashCode()
    {
        long value = (Y << 32) | X;
        return value.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}

public struct PQNode : IComparable<PQNode>
{
    public int F;
    public int G;
    public int Y;
    public int X;

    public int CompareTo(PQNode other)
    {
        if (F == other.F)
            return 0;
        return F < other.F ? 1 : -1;
    }
}

public class MapManager
{
    int[,] _objectIds;
    TileType[,] _baseTile;
    TileType[,] _curTile;
    TileType[,] _nextTile;
    List<Tuple<Vector2Int, int>> _monsterList = new List<Tuple<Vector2Int, int>>();

    int _rowSize;
    int _colSize;
    int _cellSize = 9;
    int[,] _visited;

    public TileType[,] Tile { get { return _curTile; } }
    public List<Tuple<Vector2Int, int>> MonsterList { get { return _monsterList; } }
    public int RowSize { get { return _rowSize; } }
    public int ColSize { get { return _colSize; } }
    public int CellSize { get { return _cellSize; } }
    public int[,] Visited { get { return _visited; } }

    public Maze Maze { get; set; }

    int[] dy = { -1, 1, 0, 0 };
    int[] dx = { 0, 0, -1, 1 };

	public void LoadMap(string mapName)
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Map/{mapName}");
        StringReader reader = new StringReader(textAsset.text);

        _rowSize = int.Parse(reader.ReadLine());
        _colSize = int.Parse(reader.ReadLine());

        _objectIds = new int[_rowSize, _colSize];
        reader.ReadLine();
        for (int y = 0; y < _rowSize; y++)
        {
            string line = reader.ReadLine();
            string[] cellTypes = line.Split(' ');
            for (int x = 0; x < _colSize; x++)
                _objectIds[y, x] = int.Parse(cellTypes[x]);
        }

        _baseTile = new TileType[_rowSize, _colSize];
        reader.ReadLine();
        for (int y = 0; y < _rowSize; y++)
        {
            string line = reader.ReadLine();
            string[] cellTypes = line.Split(' ');
            for (int x = 0; x < _colSize; x++)
            {
                TileType type = (TileType)Enum.Parse(typeof(TileType), cellTypes[x]);
                _baseTile[y, x] = type;
            }
        }

        for (int i = 0; i < _rowSize + 2; i++)
            reader.ReadLine();
        int monsterCount = int.Parse(reader.ReadLine());
        _monsterList.Clear();
		for (int i = 0; i < monsterCount; i++)
        {
            string line = reader.ReadLine();
            string[] monsterInfo = line.Split(' ');
            int y = int.Parse(monsterInfo[0]);
            int x = int.Parse(monsterInfo[1]);
            int monsterId = int.Parse(monsterInfo[2]);
            _monsterList.Add(new Tuple<Vector2Int, int>(new Vector2Int(x, y), monsterId));
        }

        _curTile = (TileType[,])_baseTile.Clone();

        GenerateByReculsiveBacktracking(_curTile, _rowSize, _colSize);

        Maze = Managers.Resource.Instantiate("Maze/Maze").GetOrAddComponent<Maze>();
        Maze.Initialize(_objectIds, _curTile, _rowSize, _colSize, _cellSize);
    }

    public void GenerateWorldSpawnData()
    {
        if (Maze == null)
            return;

		// MonsterGroup Spawn <- TODO update spawn system
		foreach (Tuple<Vector2Int, int> monster in _monsterList)
        {
            MazeCell mazeCell = Maze.Cells[monster.Item1.y, monster.Item1.x];

			ObjectData data = null;
			Managers.Data.ObjectDict.TryGetValue(monster.Item2, out data);
			MonsterGroupData monsterGroupData = data as MonsterGroupData;
			Vector3 pos = GridToWorld(monster.Item1);

			foreach (int monsterId in monsterGroupData.monsterIds)
			{
				ObjectInfo objectInfo = Managers.Object.GenerateObjectInfo(monsterId, pos, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
				mazeCell.Enter(objectInfo);
			}
		}

		// Other Spawn <- new system
		foreach (SpawnRuleData rule in Managers.Data.SpawnRuleDict.Values)
        {
            DistributeSpawns(rule);
		}
	}

	void DistributeSpawns(SpawnRuleData rule)
	{
        List<Tuple<CellType, int>> tuples = new List<Tuple<CellType, int>>();
        int totalCellCount = 0;
        int totalSpawnCount = 0;

        foreach (CellType cellType in rule.cellTypes)
        {
            List<MazeCell> cells = Maze.GetCellsByType(cellType);
			totalCellCount += cells.Count;
			tuples.Add(new Tuple<CellType, int>(cellType, cells.Count));
		}

        totalSpawnCount = (int)(totalCellCount * rule.frequency);

        for (int i = 0; i < totalSpawnCount; i++)
		{
			MazeCell mazeCell = null;
			int rand = Random.Range(0, totalCellCount);
			foreach (Tuple<CellType, int> t in tuples)
			{
				int cnt = t.Item2;
				if (rand < cnt)
				{
					mazeCell = Managers.Map.Maze.GetCellsByType(t.Item1)[rand];
					break;
				}
				rand -= cnt;
			}

            ObjectInfo objectInfo = Managers.Object.GenerateObjectInfo(rule.objectTemplateId, Vector3.zero, Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
            if (objectInfo == null)
                break;

            if (mazeCell.EnterRandomPosition(objectInfo) == false)
                i--;
		}
	}

	public void GenerateByReculsiveBacktracking(TileType[,] tile, int rowSize, int colSize)
    {
        List<Stack<Vector2Int>> stacks = new List<Stack<Vector2Int>>();
        int[,] visited = new int[rowSize, colSize];
        int[] direction = { 0, 1, 2, 3 };
        int[] dy = { -1, 1, 0, 0 };
        int[] dx = { 0, 0, -1, 1 };

        // Init
        for (int y = 0; y < rowSize; y++)
        {
            for (int x = 0; x < colSize; x++)
            {
                if (tile[y, x] == TileType.Seed)
                {
                    tile[y, x] = TileType.Empty;
                    stacks.Add(new Stack<Vector2Int>());
                    stacks[stacks.Count - 1].Push(new Vector2Int(x, y));
                    visited[y, x] = stacks.Count + 1;
                }
                else if (tile[y, x] != TileType.None)
                {
                    visited[y, x] = 1;
                }
                else if (Managers.Star.IsIntersecting(new Vector2Int(x, y)))
                {
                    tile[y, x] = TileType.Empty;
                }
                else
                {
                    if (Mathf.Abs(y - rowSize / 2) % 2 == 1 || Mathf.Abs(x - colSize / 2) % 2 == 1)
                        tile[y, x] = TileType.Wall;
                    else
                        tile[y, x] = TileType.Empty;
                }
            }
        }

        bool loop = true;
        while (loop)
        {
            loop = false;
            for (int i = 0; i < stacks.Count; i++)
            {
                if (stacks[i].Count == 0)
                    continue;
                loop = true;

                Vector2Int cur = stacks[i].Peek();
                Vector2Int link = Vector2Int.zero;

                RandomDir(direction, cur, tile, rowSize, colSize);
                bool flag = false;
                foreach (int dir in direction)
                {
                    int wy = cur.y + dy[dir];
                    int wx = cur.x + dx[dir];
                    int ny = cur.y + dy[dir] * 2;
                    int nx = cur.x + dx[dir] * 2;

                    if (ny < 0 || rowSize <= ny || nx < 0 || colSize <= nx)
                        continue;

                    if (visited[wy, wx] == 0 && visited[ny, nx] == 0)
                    {
                        tile[wy, wx] = TileType.Empty;
                        visited[ny, nx] = i + 2;
                        stacks[i].Push(new Vector2Int(nx, ny));
                        flag = true;
                        break;
                    }
                    else if (visited[wy, wx] == 0 && visited[ny, nx] > 1 && visited[ny, nx] != i + 2)
                        link = new Vector2Int(cur.x + dx[dir], cur.y + dy[dir]);
                }

                if (!flag)
                {
                    stacks[i].Pop();
                    if (link != Vector2.zero && Random.Range(0, 10) < 1)
                        tile[link.y, link.x] = TileType.Empty;
                }
            }
        }
    }

    void RandomDir(int[] direction, Vector2Int curPos, TileType[,] tile, int rowSize, int colSize)
    {
        int idx = 0;

        for (int i = 0; i < 3; i++)
        {
            int ny = curPos.y + dy[direction[i]];
            int nx = curPos.x + dx[direction[i]];

            if (ny < 0 || rowSize <= ny || nx < 0 || colSize <= nx)
                continue;

            if (tile[ny, nx] == TileType.Empty)
            {
                int temp = direction[i];
                direction[i] = direction[idx];
                direction[idx] = temp;
                idx++;
            }
        }


        for (int i = idx; i < 3; i++)
        {
            int randIdx = Random.Range(i, 4);
            int temp = direction[i];
            direction[i] = direction[randIdx];
            direction[randIdx] = temp;
        }
    }

    public void LoadContent(Vector2Int gridPos, int range, bool immediately = false)
    {
        if (Maze == null)
            return;
        Maze.LoadContent(gridPos, range, immediately);
    }

    public void UnloadContent(Vector2Int gridPos, int range)
	{
		if (Maze == null)
			return;
		Maze.UnloadContent(gridPos, range);
    }

    public bool IsCellLoaded(Vector2Int gridPos)
    {
        if (Maze == null)
            return false;
        return Maze.IsCellLoaded(gridPos);
	}

	public void ChangeMaze()
    {
        _curTile = (TileType[,])_baseTile.Clone();
        GenerateByReculsiveBacktracking(_curTile, _rowSize, _colSize);
        //Maze.RefreshMazeWall();
        Managers.Event.GameEvents.OnMazeChanged();
    }

    // 지금은 못사용함
    //public void ExchangedWall(MazeWall wall1, MazeWall wall2)
    //{
    //    Vector2Int gridPos1 = wall1.GridPos;
    //    Vector2Int gridPos2 = wall2.GridPos;
    //    Maze.Cells[gridPos2.y, gridPos2.x] = wall1;
    //    Maze.Cells[gridPos1.y, gridPos1.x] = wall2;
    //    wall1.Init(gridPos2);
    //    wall2.Init(gridPos1);

    //    If there is a need to change the position of the wall, we need to modify that part

    //    Maze.UpdateGridCells(Managers.Object.GetPlayer().GridPos, 11);
    //}

	public List<Vector2Int> BFS(Vector2Int start, int distance)
    {
        List<Vector2Int> ret = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        int[,] visited = new int[_rowSize, _colSize];

        queue.Enqueue(start);
        visited[start.y, start.x] = 1;
        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                Vector2Int next = new Vector2Int();
                next.y = cur.y + dy[i];
                next.x = cur.x + dx[i];
                if (_curTile[next.y, next.x] != TileType.Wall && visited[next.y, next.x] == 0)
                {
                    visited[next.y, next.x] = visited[cur.y, cur.x] + 1;
                    if (visited[next.y, next.x] == distance * 2 + 1)
                        ret.Add(next);
                    else
                        queue.Enqueue(next);
                }
            }
        }

        return ret;
	}

    public void ApplyEnter(ObjectInfo objectInfo)
    {
        MazeCell cell = GetCell(objectInfo.SpawnPos);
        cell.Enter(objectInfo);
    }

    public void ApplyMove(ObjectInfo objectInfo)
    {
        GameObject gameObject = objectInfo.gameObject != null ? objectInfo.gameObject : objectInfo.DummyObject;
		if (gameObject == null)
            return;

        MazeCell now = objectInfo.Cell;
        MazeCell after = GetCell(gameObject.transform.position);

        if (now != after)
        {
            now.Leave(objectInfo);
            after.Enter(objectInfo);
        }
    }

	public void ApplyLeave(ObjectInfo objectInfo)
	{
        MazeCell cell = objectInfo.Cell;
		if (cell != null)
            cell.Leave(objectInfo);

        if (objectInfo.gameObject != null)
            Managers.Resource.Destroy(objectInfo.gameObject);
        if (objectInfo.DummyObject != null)
            Managers.Resource.Destroy(objectInfo.DummyObject);
        if (objectInfo.MinimapMarker != null)
            Managers.Minimap.RemoveMarker(objectInfo.MinimapMarker);
	}

	public MazeCell GetCell(Vector3 worldPos)
	{
        return GetCell(WorldToGrid(worldPos));
	}

	public MazeCell GetCell(Vector2Int gridPos)
    {
        if (Maze == null)
            return null;
        return Maze.Cells[gridPos.y, gridPos.x];
    }

    public int GetMazeObjectId(Vector3 pos)
    {
        int x = (int)(pos.x + _cellSize * (_colSize / 2) + _cellSize / 2) / (_cellSize);
        int y = (int)(_cellSize * (_rowSize / 2) - pos.z + _cellSize / 2) / (_cellSize);
        Vector2Int gridPos = new Vector2Int(x, y);

        if (gridPos.x < 0 || gridPos.x >= _colSize)
            return 0;
        if (gridPos.y < 0 || gridPos.y >= _rowSize)
            return 0;

        return _objectIds[gridPos.y, gridPos.x];
    }

    public int GetMazeObjectId(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= _colSize)
            return 0;
        if (gridPos.y < 0 || gridPos.y >= _rowSize)
            return 0;

        return _objectIds[gridPos.y, gridPos.x];
    }

    public TileType GetTileType(Vector3 pos)
    {
        Vector2Int gridPos = WorldToGrid(pos);

        return GetTileType(gridPos);
    }

    public TileType GetTileType(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= _colSize)
            return TileType.Wall;
        if (gridPos.y < 0 || gridPos.y >= _rowSize)
            return TileType.Wall;

        return _curTile[gridPos.y, gridPos.x];
    }

    public void SetTileType(Vector2Int gridPos, TileType type)
    {
        if (gridPos.x < 0 || gridPos.x >= _colSize)
            return;
        if (gridPos.y < 0 || gridPos.y >= _rowSize)
            return;

        _curTile[gridPos.y, gridPos.x] = type;
    }

    public TileType GetNextTileType(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= _colSize)
            return TileType.Wall;
        if (gridPos.y < 0 || gridPos.y >= _rowSize)
            return TileType.Wall;

        return _nextTile[gridPos.y, gridPos.x];
    }

    public void SetUnchangeableTile(Vector3 pos)
    {
        int x = (int)(pos.x + _cellSize * (_colSize / 2) + _cellSize / 2) / (_cellSize);
        int y = (int)(_cellSize * (_rowSize / 2) - pos.z + _cellSize / 2) / (_cellSize);
        Vector2Int gridPos = new Vector2Int(x, y);

        if (_baseTile[gridPos.y, gridPos.x] == TileType.None)
            _baseTile[gridPos.y, gridPos.x] = TileType.Unchangeable;
        //_baseTile[gridPos.y, gridPos.x] = _curTile[gridPos.y, gridPos.x];
    }

    public bool IsConnectedGridCell(Vector3 pos)
    {
        Vector2Int gridPos = WorldToGrid(pos);
        if (_baseTile[gridPos.y, gridPos.x] == TileType.Unchangeable)
            return true;
        else
            return false;
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x * _cellSize - _cellSize * (_colSize / 2), 0, _cellSize * (_rowSize / 2) - gridPos.y * _cellSize);
        return worldPos;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = (int)(worldPos.x + _cellSize * 2 * (_colSize / 4) + _cellSize) / (_cellSize * 2);
        int y = (int)(_cellSize * 2 * (_rowSize / 4) - worldPos.z + _cellSize) / (_cellSize * 2);
        Vector2Int gridPos = new Vector2Int(x * 2 + 1, y * 2 + 1);
        return gridPos;
    }

    public bool CanGo(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= _colSize)
            return false;
        if (gridPos.y < 0 || gridPos.y >= _rowSize)
            return false;

        return _curTile[gridPos.y, gridPos.x] != TileType.Wall;
    }

    public bool IsValidGridPos(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= _colSize)
            return false;
        if (gridPos.y < 0 || gridPos.y >= _rowSize)
            return false;

        return true;
    }

    #region A* PathFinding

    // U D L R
    int[] _deltaY = new int[] { 1, -1, 0, 0 };
    int[] _deltaX = new int[] { 0, 0, -1, 1 };
    int[] _cost = new int[] { 10, 10, 10, 10 };


	// (y, x) 이미 방문했는지 여부 (방문 = closed 상태)
	HashSet<Pos> closeList = new HashSet<Pos>();  // CloseList
	// (y, x) 가는 길을 한 번이라도 발견했는지
	// 발견X => MaxValue
	// 발견O => F = G + H
	Dictionary<Pos, int> openList = new Dictionary<Pos, int>(); // OpenList
	Dictionary<Pos, Pos> parent = new Dictionary<Pos, Pos>();
	// 오픈리스트에 있는 정보들 중에서, 가장 좋은 후보를 빠르게 뽑아오기 위한 도구
	PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

	public List<Vector2Int> FindPath(Vector2Int startCellPos, Vector2Int destCellPos, bool ignoreDestCollision = false, int maxDist = 30)
    {
        List<Pos> path = new List<Pos>();
		closeList.Clear();
		openList.Clear();
		parent.Clear();
		pq.Clear();

		// 점수 매기기
		// F = G + H
		// F = 최종 점수 (작을 수록 좋음, 경로에 따라 달라짐)
		// G = 시작점에서 해당 좌표까지 이동하는데 드는 비용 (작을 수록 좋음, 경로에 따라 달라짐)
		// H = 목적지에서 얼마나 가까운지 (작을 수록 좋음, 고정)


		// CellPos -> ArrayPos
		Pos pos = Cell2Pos(startCellPos);
        Pos dest = Cell2Pos(destCellPos);

        // 시작점 발견 (예약 진행)
        openList.Add(pos, 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)));
        pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
        parent.Add(pos, pos);

		Pos best = pos;
		int bestHeuristic = int.MaxValue;
        int maxExpandedNodes = 100;
		int expanded = 0;

		while (pq.Count > 0)
        {
            // 제일 좋은 후보를 찾는다
            PQNode pqNode = pq.Pop();
            Pos node = new Pos(pqNode.Y, pqNode.X);
            // 동일한 좌표를 여러 경로로 찾아서, 더 빠른 경로로 인해서 이미 방문(closed)된 경우 스킵
            if (closeList.Contains(node))
                continue;

            // 방문한다
            closeList.Add(node);
            // 목적지 도착했으면 바로 종료
            if (node.Y == dest.Y && node.X == dest.X)
                break;
			// 너무 많은 노드를 확장했으면 강제 종료
			if (++expanded > maxExpandedNodes)
			{
				break; // 탐색 강제 종료
			}


			// 상하좌우 등 이동할 수 있는 좌표인지 확인해서 예약(open)한다
			for (int i = 0; i < _deltaY.Length; i++)
            {
                Pos next = new Pos(node.Y + _deltaY[i], node.X + _deltaX[i]);

                // 너무 멀면 스킵
                if (Math.Abs(pos.Y - next.Y) + Math.Abs(pos.X - next.X) > maxDist)
                    continue;

                // 유효 범위를 벗어났으면 스킵
                // 벽으로 막혀서 갈 수 없으면 스킵
                if (!ignoreDestCollision || next.Y != dest.Y || next.X != dest.X)
                {
                    if (CanGo(Pos2Cell(next)) == false) // CellPos
                        continue;
                }

                // 이미 방문한 곳이면 스킵
                if (closeList.Contains(next))
                    continue;

                // 비용 계산
                int g = 0; // pqNode.G + _cost[i];
                int h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));
				if (h < bestHeuristic)
				{
					bestHeuristic = h;
					best = next;
				}
				// 다른 경로에서 더 빠른 길 이미 찾았으면 스킵
				int value = 0;
                if (openList.TryGetValue(next, out value) == false)
                    value = Int32.MaxValue;
                if (value < g + h)
                    continue;

                // 예약 진행
                if (openList.TryAdd(next, g + h) == false)
                    openList[next] = g + h;
                pq.Push(new PQNode() { F = g + h, G = g, Y = next.Y, X = next.X });
                if (parent.TryAdd(next, node) == false)
                    parent[next] = node;
            }
        }
        //Debug.Log($"Path Found. CloseList Count: {closeList.Count}, OpenList Count: {openList.Count}");
		if (!parent.ContainsKey(dest))
			dest = best;
		return CalcCellPathFromParent(parent, dest);
    }

    List<Vector2Int> CalcCellPathFromParent(Dictionary<Pos, Pos> parent, Pos dest)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        if (parent.ContainsKey(dest) == false)
        {
            Pos best = new Pos();
            int bestDist = Int32.MaxValue;

            foreach (Pos pos in parent.Keys)
            {
                int dist = Math.Abs(dest.X - pos.X) + Math.Abs(dest.Y - pos.Y);
                // 제일 우수한 후보를 뽑는다
                if (dist < bestDist)
                {
                    best = pos;
                    bestDist = dist;
                }
            }

            dest = best;
        }

        {
            Pos pos = dest;
            while (parent[pos] != pos)
            {
                cells.Add(Pos2Cell(pos));
                pos = parent[pos];
            }
            cells.Add(Pos2Cell(pos));
            cells.Reverse();
        }

        return cells;
    }

    Pos Cell2Pos(Vector2Int cell)
    {
        // CellPos -> ArrayPos
        return new Pos(cell.y, cell.x);
    }

    Vector2Int Pos2Cell(Pos pos)
    {
        // ArrayPos -> CellPos
        return new Vector2Int(pos.X, pos.Y);
    }

    #endregion
}
