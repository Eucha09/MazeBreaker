using Data;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricFogAndMist2;
using static Define;
using static UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class ObjectManager
{
    PlayerController _player;
    List<GameObject> _objects = new List<GameObject>();

    public void Add(GameObject go)
    {
        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc != null)
        {
            _player = pc;
            return;
        }

        _objects.Add(go);
    }

    public ObjectInfo GenerateObjectInfo(int templateId, Vector3 pos, Quaternion rot)
	{
		ObjectData objectData = null;
		Managers.Data.ObjectDict.TryGetValue(templateId, out objectData);

		if (objectData == null)
			return null;

		if (objectData.objectType == GameObjectType.Monster)
		{
			CreatureInfo creatureInfo = new CreatureInfo();
			creatureInfo.SpawnPos = pos;
			creatureInfo.Rotation = rot;
			creatureInfo.Init(objectData.id);
            return creatureInfo;
		}
		else if (objectData.objectType == GameObjectType.Nature)
		{
			NatureInfo natureInfo = new NatureInfo();
			natureInfo.SpawnPos = pos;
			natureInfo.Rotation = rot;
			natureInfo.Init(objectData.id);
            return natureInfo;
		}
		else if (objectData.objectType == GameObjectType.Structure)
		{
			BuildingObjectInfo buildingInfo = new BuildingObjectInfo();
			buildingInfo.SpawnPos = pos;
			buildingInfo.Rotation = rot;
			buildingInfo.Init(objectData.id);
            return buildingInfo;
		}

		return null;
	}

    public PlayerController GetPlayer()
    {
        if (_player == null)
        {
            GameObject go = GameObject.Find("Player");
            if (go != null)
                _player = go.GetComponent<PlayerController>();
        }
        return _player;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject obj in _objects)
        {
            if (obj != null && condition.Invoke(obj))
                return obj;
        }

        return null;
    }

    public List<GameObject> FindAll(Func<GameObject, bool> condition)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (GameObject obj in _objects)
        {
            if (obj != null && condition.Invoke(obj))
                list.Add(obj);
        }

        return list;
    }

    public void Remove(GameObject go)
    {
        int objectId = go.GetComponent<BaseController>().TemplateId;

		Managers.Event.SpawnEvents.OnDespawn(objectId, go);
		_objects.Remove(go);
		Managers.Resource.Destroy(go);
	}

    public void RemoveAll(Func<GameObject, bool> condition)
    {
        for (int i = _objects.Count - 1; i >= 0; i--)
        {
            GameObject go = _objects[i];
            if (go == null || condition.Invoke(go))
            {
                _objects.RemoveAt(i);
                Managers.Resource.Destroy(go);
            }
        }
    }

    public GameObject FindNearestObjectWithTag(string tag, Vector3 pos, float radius)
    {
        GameObject nearestObject = null;
        float nearestDistance = 0.0f;

        Collider[] colliders = Physics.OverlapSphere(pos, radius);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(tag))
            {
                Vector3 dir = pos - collider.transform.position;
				float distance = Vector3.Distance(pos, collider.transform.position);
				bool hit = Physics.Raycast(collider.transform.position, dir, distance, 1 << (int)Define.Layer.Block);

				if (hit == false && (nearestObject == null || distance < nearestDistance))
                {
                    nearestObject = collider.gameObject;
                    nearestDistance = distance;
                }
            }
        }

        return nearestObject;
    }

	public void Clear()
    {
        _player = null;
        _objects.Clear();
    }
}