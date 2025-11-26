using UnityEngine;
using static Define;

public class RewardObjectInfo : ObjectInfo
{
	public int ItemId { get; set; }
	public int Count { get; set; }

	public bool IsDropping { get; set; }
	public Vector3 DroppingPos { get; set; }

	public void Init(int itemId, int count, Vector3 droppingPos)
	{
		Type = ObjectInfoType.RewardObject;
		PrefabPath = "Item/RewardObject";
		ItemId = itemId;
		Count = count;
		IsDropping = true;
		DroppingPos = droppingPos;
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
			gameObject.GetComponent<RewardObject>().Unbind();
			Managers.Resource.Destroy(gameObject);
			gameObject = null;
		}
	}

	public override void Refresh()
	{

	}

	public void OnSpawn(GameObject go)
	{
		_loading = false;

		if (_active)
		{
			gameObject = go;
			RewardObject bc = gameObject.GetComponent<RewardObject>();
			bc.Bind(this);
		}
		else
		{
			Managers.Resource.Destroy(go);
		}
	}
}
