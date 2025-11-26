using System;
using UnityEngine;

public class SpawnEvents
{
	public event Action<int, GameObject> SpawnAction;
	public void OnSpawn(int id, GameObject go)
	{
		if (SpawnAction != null)
		{
			SpawnAction(id, go);
		}
	}

	public event Action<int, GameObject> DespawnAction;
	public void OnDespawn(int id, GameObject go)
	{
		if (DespawnAction != null)
		{
			DespawnAction(id, go);
		}
	}

	public void Clear()
	{
		SpawnAction = null;
		DespawnAction = null;
	}
}
