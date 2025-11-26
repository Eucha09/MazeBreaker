using Data;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RewardObject : Interactable
{
    int _itemId;
    int _count;

    public int ItemId 
    { 
        get { return _itemId; } 
        set 
        {
			_itemId = value;
			ItemData itemData = null;
			Managers.Data.ItemDict.TryGetValue(_itemId, out itemData);
			if (itemData != null)
				_name = itemData.name;
            _update = true;
		} 
    }
	public int Count { get { return _count; } set { _count = value; } }

    Coroutine _coDropping;

    public RewardObjectInfo Info { get; set; }

    public void Bind(RewardObjectInfo info)
    {
        Info = info;

        if (Info.IsDropping)
        {
            Info.IsDropping = false;
            Init(Info.ItemId, Info.Count, Info.DroppingPos, Info.SpawnPos);
        }
        else
            Init(Info.ItemId, Info.Count);
	}

    public void Unbind()
    {
        Info.Count = _count;
        Info = null;
    }

	public void Init(int itemId, int count)
    {
        _itemId = itemId;
        _count = count;

        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(_itemId, out itemData);
        if (itemData != null)
        {
            _name = itemData.name;
            GameObject go = Managers.Resource.Instantiate(itemData.prefabPath, transform);
            go.transform.Rotate(new Vector3(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
        }
	}

	public void Init(int itemId, int count, Vector3 startPos, Vector3 targetPos)
	{   
        Init(itemId, count);

		_coDropping = StartCoroutine(CoDropping(startPos, targetPos, 0.5f));
	}

	public override void Interact()
    {
        Managers.Sound.Play("FarmingSound/PickupItem");
        ItemInfo itemInfo = new ItemInfo()
        {
            TemplateId = _itemId,
            Count = _count,
            Equipped = false
        };

        Item newItem = Item.MakeItem(itemInfo);
        _count = Managers.Inven.Add(newItem);

        if (_count == 0)
		{
            if (Info != null)
                Managers.Map.ApplyLeave(Info);
            else
    			Managers.Resource.Destroy(gameObject);
		}
    }


	IEnumerator CoDropping(Vector3 startPos, Vector3 targetPos, float time)
	{
        transform.position = startPos;
		for (float i = 0; i < time; i += Time.deltaTime)
		{
            float height = Mathf.Sin(i / time * Mathf.PI) * 1.5f;
            transform.position = Vector3.Lerp(startPos, targetPos, i / time) + new Vector3(0.0f, height, 0.0f);
			yield return null;
		}
        transform.position = targetPos;
        _coDropping = null;
	}
}
