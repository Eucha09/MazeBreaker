using Data;
using UnityEngine;
using static Define;

public class TileObjectInfo : ObjectInfo
{
	public Vector2Int GridPos { get; set; }
	public TileType TileType { get; set; }
	public float SizeX { get; set; }
	public float SizeY { get; set; }

	public void Init(MazeObjectData data, Vector2Int gridPos)
	{
		Type = ObjectInfoType.Tile;

		GridPos = gridPos;
		TileType = Managers.Map.GetTileType(GridPos);
		SizeX = data.sizeX;
		SizeY = data.sizeY;
		PrefabPath = data.prefabPaths[Random.Range(0, data.prefabPaths.Count)];

		if (data.minimapMarkerId != 0 && data.isAwaysVisibleOnMinimap)
		{
			MinimapMarker = Managers.Resource.Instantiate("Minimap/Minimap_Marker").GetComponent<Minimap_Marker>();
			MinimapMarker.SetTemplateId(data.minimapMarkerId, SpawnPos);
			Managers.Minimap.AddMarker(MinimapMarker);
		}
	}

	public override void LoadObject(bool immediately = false)
	{
		_active = true;

		if (gameObject == null && _loading == false)
		{
			if (immediately)
			{
				gameObject = Managers.Resource.Instantiate(PrefabPath, SpawnPos, Rotation, Cell.transform);
				OnSpawn(gameObject);
			}
			else
			{
				_loading = true;
				Managers.Resource.EnqueueInstantiate(PrefabPath, SpawnPos, Rotation, Cell.transform, OnSpawn);
			}
		}
	}

	public override void UnLoadObject()
	{
		_active = false;

		if (gameObject != null)
		{
			gameObject.GetComponent<MovingWall>()?.Unbind(); // TODO
			gameObject.GetComponent<GladeDoor>()?.Unbind(); // TODO
			Managers.Resource.Destroy(gameObject);
			gameObject = null;
		}
	}

	public override void Refresh()
	{
		if (gameObject != null)
		{
			gameObject.GetComponent<MovingWall>()?.Refresh(); // TODO
			gameObject.GetComponent<GladeDoor>()?.Refresh(); // TODO
		}
	}

	public void OnSpawn(GameObject go)
	{
		_loading = false;

		if (_active)
		{
			gameObject = go;
			gameObject.GetComponent<MovingWall>()?.Bind(this); // TODO
			gameObject.GetComponent<GladeDoor>()?.Bind(this); // TODO
		}
		else
		{
			Managers.Resource.Destroy(go);
		}
	}
}
