using Data;
using System.Security.Cryptography;
using UnityEngine;
using static Define;

public class ObjectInfo
{
	public MazeCell Cell { get; set; }
	public Vector3 SpawnPos { get; set; }
	public Quaternion Rotation { get; set; }

	public ObjectInfoType Type { get; set; }
	public string PrefabPath { get; set; }
	public float ColliderRadius { get; set; }

	public GameObject gameObject { get; set; }
	public GameObject DummyObject { get; set; }
	public Minimap_Marker MinimapMarker { get; set; }

	protected bool _loading;
	protected bool _active;

	public virtual void LoadObject(bool immediately = false)
	{

	}

	public virtual void UnLoadObject()
	{

	}

	public virtual void Refresh()
	{
		
	}

	public virtual bool IsLoaded()
	{
		return gameObject != null;
	}
}
