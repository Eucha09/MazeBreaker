using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using Object = UnityEngine.Object;
using UnityEngine.InputSystem;

public class ResourceManager
{
	Dictionary<string, Object> _resources = new Dictionary<string, Object>();

	int _perFrameCount = 5; // 한 프레임당 생성 개수
	Queue<SpawnRequest> _spawnQueue = new Queue<SpawnRequest>();
	Coroutine _coRunner;

	public T Load<T>(string key) where T : Object
    {
		if (_resources.TryGetValue(key, out Object resource))
		{
			// Addressable에서는 png를 Texture2D로 로드해오기 때문에 Sprite 타입이 필요한 경우 변환 필요
			if (typeof(T) == typeof(Sprite))
			{
				if (resource is Sprite sprite)
					return sprite as T;

				if (resource is Texture2D tex)
				{
					sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
					_resources[key] = sprite; // 캐싱을 Sprite로 교체
					return sprite as T;
				}
			}

			return resource as T;
		}

		return null;
    }

    public GameObject Instantiate(string key, Transform parent = null)
    {
        GameObject prefab = Load<GameObject>($"Prefabs/{key}");
        if (prefab == null)
        {
            Debug.Log($"Failed to load prefab : {key}");
            return null;
        }

        if (prefab.GetComponent<Poolable>() != null)
            return Managers.Pool.Pop(prefab, parent).gameObject;

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;
        return go;
    }

    public GameObject Instantiate(string key, Vector3 pos, Quaternion rot, Transform parent = null)
	{
		GameObject prefab = Load<GameObject>($"Prefabs/{key}");
		if (prefab == null)
		{
			Debug.Log($"Failed to load prefab : {key}");
			return null;
		}

		if (prefab.GetComponent<Poolable>() != null)
        {
            GameObject pool = Managers.Pool.Pop(prefab, parent).gameObject;
            pool.transform.position = pos;
            pool.transform.rotation = rot;
            return pool;
        }

        GameObject go = Object.Instantiate(prefab, pos, rot, parent);
        go.name = prefab.name;
        return go;
    }

    public GameObject Instantiate(GameObject gameObject, Vector3 pos, Quaternion rot, Transform parent = null)
	{
		GameObject prefab = gameObject;
		if (prefab == null)
		{
			Debug.Log($"Failed to load prefab : {prefab.name}");
			return null;
		}

		if (prefab.GetComponent<Poolable>() != null)
		{
			GameObject pool = Managers.Pool.Pop(prefab, parent).gameObject;
			pool.transform.position = pos;
			pool.transform.rotation = rot;
			return pool;
		}

		GameObject go = Object.Instantiate(prefab, pos, rot, parent);
		go.name = prefab.name;
		return go;
	}

	public void EnqueueInstantiate(string key, Vector3 pos, Quaternion rot, Transform parent = null, Action<GameObject> onComplete = null, bool forceQueue = true)
	{
		GameObject original = Load<GameObject>($"Prefabs/{key}");
		if (original == null)
		{
			Debug.Log($"Failed to load prefab : {key}");
			return;
		}

		if (!forceQueue && original.GetComponent<Poolable>() != null)
		{
			GameObject pooled = Managers.Pool.Pop(original, parent).gameObject;
			pooled.transform.SetPositionAndRotation(pos, rot);
			onComplete?.Invoke(pooled);
			return;
		}

		_spawnQueue.Enqueue(new SpawnRequest
		{
			original = original,
			parent = parent,
			pos = pos,
			rot = rot,
			onComplete = onComplete
		});
		EnsureRunner();
	}

	public void EnqueueInstantiate(GameObject original, Vector3 pos, Quaternion rot, Transform parent = null, Action<GameObject> onComplete = null, bool forceQueue = true)
	{
		if (original == null)
		{
			Debug.LogError("EnqueueInstantiate original is null");
			return;
		}

		if (!forceQueue && original.GetComponent<Poolable>() != null)
		{
			GameObject pooled = Managers.Pool.Pop(original, parent).gameObject;
			pooled.transform.SetPositionAndRotation(pos, rot);
			onComplete?.Invoke(pooled);
			return;
		}

		_spawnQueue.Enqueue(new SpawnRequest
		{
			original = original,
			parent = parent,
			pos = pos,
			rot = rot,
			onComplete = onComplete
		});
		EnsureRunner();
	}

	public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        }

        Object.Destroy(go);
    }

    public void Clear()
    {
        _spawnQueue.Clear();
	}

	#region ProcessQueue
	struct SpawnRequest
	{
		public GameObject original;
		public Transform parent;
		public Vector3 pos;
		public Quaternion rot;
		public Action<GameObject> onComplete;
	}

	void EnsureRunner()
	{
		if (_coRunner != null)
			return;

		_coRunner = Managers.Instance.StartCoroutine(ProcessQueue());
	}

	IEnumerator ProcessQueue()
	{
		while (_spawnQueue.Count > 0)
		{
			int count = Mathf.Min(_perFrameCount, _spawnQueue.Count);

			for (int i = 0; i < count; i++)
			{
				var req = _spawnQueue.Dequeue();

				GameObject instance = Instantiate(req.original, req.pos, req.rot, req.parent);

				req.onComplete?.Invoke(instance);
			}

			yield return null;
		}
		_coRunner = null;
	}
	#endregion

	#region Addressables
	public void LoadAsync<T>(string key, Action<T> callback = null) where T : Object
    {
        if (_resources.TryGetValue(key, out Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        var asyncOperation = Addressables.LoadAssetAsync<T>(key);
        asyncOperation.Completed += (op) =>
		{
			if (!_resources.ContainsKey(key))
				_resources.Add(key, op.Result);
			//else
			//	Debug.LogWarning($"[ResourceManager] Duplicate key ignored: {key} Sprite: {op.Result is Sprite} Texture2D: {op.Result is Texture2D}");
			callback?.Invoke(op.Result);
        };
    }

    public void LoadAllAsync<T>(string label, Action<string, int, int> callback) where T : Object
    {
		var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        opHandle.Completed += (op) =>
        {
            int loadCount = 0;
            int totalCount = op.Result.Count;

            foreach (var result in op.Result)
            {
                LoadAsync<T>(result.PrimaryKey, (obj) =>
                {
                    loadCount++;
                    callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                });
            }
        };
    }

	public void InstantiateAsync(string key, Action<GameObject> callback = null, Transform parent = null)
	{
		LoadAsync<GameObject>(key, (prefab) =>
		{
			if (prefab == null)
			{
				Debug.LogError($"Failed to load prefab : {key}");
				callback?.Invoke(null);
				return;
			}

			if (prefab.GetComponent<Poolable>() != null)
			{
				callback?.Invoke(Managers.Pool.Pop(prefab, parent).gameObject);
				return;
			}

			GameObject go = Object.Instantiate(prefab, parent);
			go.name = prefab.name;
			callback?.Invoke(go);
		});
	}

	public void InstantiateAsync(string key, Vector3 pos, Quaternion rot, Action<GameObject> callback = null, Transform parent = null)
	{
		LoadAsync<GameObject>(key, (prefab) =>
		{
			if (prefab == null)
			{
				Debug.LogError($"Failed to load prefab : {key}");
				callback?.Invoke(null);
				return;
			}
			if (prefab.GetComponent<Poolable>() != null)
			{
				GameObject pooled = Managers.Pool.Pop(prefab, parent).gameObject;
				pooled.transform.SetPositionAndRotation(pos, rot);
				callback?.Invoke(pooled);
				return;
			}
			GameObject go = Object.Instantiate(prefab, pos, rot, parent);
			go.name = prefab.name;
			callback?.Invoke(go);
		});
	}
	#endregion
}
