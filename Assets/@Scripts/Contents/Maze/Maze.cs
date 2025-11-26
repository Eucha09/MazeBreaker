using Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using VolumetricFogAndMist2;
using static Define;

// 자원 스폰
// 구조 바뀌는거
// 마커 어떻게 할건지 (글로벌하게 스폰할거 구분)

public class Maze : MonoBehaviour
{
    //MazeCellData[,] _mazeCellData;
    MazeCell[,] _mazeCells;
    Dictionary<Vector2Int, int> _activeFlag = new Dictionary<Vector2Int, int>();
    public MazeCell[,] Cells { get { return _mazeCells; } }

    List<MazeCell>[] _cellsByType = new List<MazeCell>[(int)CellType.MaxCount];

    int _rowSize;
    int _colSize;
    int _cellSize;
    public bool IsStableState;

    int[] dy = { 1, 0, -1, 0 };
    int[] dx = { 0, -1, 0, 1 };

    void Start()
    {
        Managers.Event.TimeEvents.DayAction += OnDay;
        Managers.Event.TimeEvents.NightAction += OnNight;

        Managers.Resource.Instantiate("Minimap/MinimapObject_Grid");
        OnDay(); // TODO
	}

    void Update()
    {

    }

    public void Initialize(int[,] objectIds, TileType[,] tile, int rowSize, int colSize, int cellSize)
    {
        _rowSize = rowSize;
        _colSize = colSize;
        _cellSize = cellSize;

		_mazeCells = new MazeCell[_rowSize, _colSize];
        for (int y = 0; y < _rowSize; y++)
        {
            for (int x = 0; x < _colSize; x++)
			{
				if (objectIds[y, x] == 0)
					continue;

				MazeObjectData data = null;
				Managers.Data.MazeObjectDict.TryGetValue(objectIds[y, x], out data);
                if (data != null)
				{
					Vector2Int gridPos = new Vector2Int(x, y);
					Vector3 worldPos = Managers.Map.GridToWorld(gridPos);

					GameObject go = Managers.Resource.Instantiate("Maze/MazeCell", worldPos, Quaternion.identity, transform);
                    go.name = "MazeCell_" + y + "_" + x;
                    MazeCell cell = go.GetOrAddComponent<MazeCell>();
                    cell.Init(data, gridPos);

                    int yMin = y - data.cellSizeY / 2;
                    int yMax = yMin + data.cellSizeY - 1;
                    int xMin = x - data.cellSizeX / 2;
                    int xMax = xMin + data.cellSizeX - 1;
                    for (int yy = yMin; yy <= yMax; yy++)
                        for (int xx = xMin; xx <= xMax; xx++)
                            _mazeCells[yy, xx] = cell;

                    if (_cellsByType[(int)data.cellType] == null)
                        _cellsByType[(int)data.cellType] = new List<MazeCell>();
                    _cellsByType[(int)data.cellType].Add(_mazeCells[y, x]);
                }
			}
        }
    }

    public void LoadContent(Vector2Int gridPos, int range, bool immediately = false)
    {
        // 보정
        range = range * 2 + 1;

		int minY = gridPos.y - range;
		int maxY = gridPos.y + range;
		int minX = gridPos.x - range;
		int maxX = gridPos.x + range;

		for (int y = minY; y <= maxY; y++)
		{
			for (int x = minX; x <= maxX; x++)
			{
				if (x < 0 || x >= _colSize || y < 0 || y >= _rowSize)
					continue;
				if (_mazeCells[y, x] == null)
					continue;

                Vector2Int gpos = _mazeCells[y, x].GridPos;
                if (_activeFlag.ContainsKey(gpos))
                    _activeFlag[gpos]++;
                else
                {
					_activeFlag[gpos] = 1;
					_mazeCells[gpos.y, gpos.x].LoadContent(immediately);
				}
			}
		}
	}

    public void UnloadContent(Vector2Int gridPos, int range)
	{
		// 보정
		range = range * 2 + 1;

		int minY = gridPos.y - range;
		int maxY = gridPos.y + range;
		int minX = gridPos.x - range;
		int maxX = gridPos.x + range;

		for (int y = minY; y <= maxY; y++)
		{
			for (int x = minX; x <= maxX; x++)
			{
				if (x < 0 || x >= _colSize || y < 0 || y >= _rowSize)
					continue;
				if (_mazeCells[y, x] == null)
					continue;
				Vector2Int gpos = _mazeCells[y, x].GridPos;
				if (_activeFlag.ContainsKey(gpos))
				{
					_activeFlag[gpos]--;
                    if (_activeFlag[gpos] <= 0)
					{
						_mazeCells[gpos.y, gpos.x].UnLoadContent();
						_activeFlag.Remove(gpos);
					}
				}
			}
		}
	}

    public bool IsCellLoaded(Vector2Int gridPos)
	{
		if (gridPos.x < 0 || gridPos.x >= _colSize || gridPos.y < 0 || gridPos.y >= _rowSize)
			return false;
		if (_mazeCells[gridPos.y, gridPos.x] == null)
			return false;
        return _mazeCells[gridPos.y, gridPos.x].IsLoaded();
	}

	public void Refresh()
	{
		foreach (Vector2Int gridPos in _activeFlag.Keys)
		{
			if (_mazeCells[gridPos.y, gridPos.x] != null)
				_mazeCells[gridPos.y, gridPos.x].Refresh();
		}
	}

    public void EnterMaze(GameObject player, Vector2Int gridPos)
    {
        Vector3 pos = Managers.Map.GridToWorld(gridPos);


        player.transform.position = pos;
    }

    public List<MazeCell> GetCellsByType(CellType type)
    {
        if (_cellsByType[(int)type] == null)
            _cellsByType[(int)type] = new List<MazeCell>();
		return _cellsByType[(int)type];
    }

    void OnNight()
    {
        Managers.Map.ChangeMaze();
        //GenerateNightmare(16);

        foreach (MazeCell cell in _cellsByType[(int)CellType.GladeDoor])
			Managers.Map.SetTileType(cell.GridPos, Define.TileType.Wall);

        Refresh();
	}

    void OnDay()
    {
		foreach (MazeCell cell in _cellsByType[(int)CellType.GladeDoor])
			Managers.Map.SetTileType(cell.GridPos, Define.TileType.Empty);

		int respawnPeriod = Managers.Game.RespawnPeriod;
		if (Managers.Time.Days % respawnPeriod == 0)
			StartCoroutine(CoGlowsEffect(8.0f));

        Refresh();
	}

    IEnumerator CoGlowsEffect(float time)
    {
        GameObject go = Managers.Resource.Instantiate("Effects/AmbientGlows");
        go.GetComponent<SynchronousPosition>().SetTarget(Managers.Object.GetPlayer().transform);

        yield return new WaitForSeconds(time);

        go.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void GenerateNightmare(int distance)
    {

        GameObject p = Managers.Object.GetPlayer().gameObject;
        Vector2Int gridPos = Managers.Map.WorldToGrid(p.transform.position);
        List<Vector2Int> list = Managers.Map.BFS(gridPos, distance);


        foreach (Vector2Int gPos in list)
        {
            if (_mazeCells[gPos.y, gPos.x] == null || _mazeCells[gPos.y, gPos.x].CellType == CellType.Glade3x3 || _mazeCells[gPos.y, gPos.x].CellType == CellType.SectorA)
                continue;
            Vector3 pos = Managers.Map.GridToWorld(gPos);
            //Managers.Object.SpawnObject(18, pos, Quaternion.identity);
        }

    }
}
