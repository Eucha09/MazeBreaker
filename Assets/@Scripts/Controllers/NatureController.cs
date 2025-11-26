using Data;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Define;

public class NatureController : BaseController
{
	float _shakeDuration = 0.5f;
    float _shakeMagnitude = 0.1f;

	protected bool _init;
	// public AudioClip clip;

	[SerializeField]
	Renderer _meshRenderer;
	MaterialPropertyBlock[] _blocks;

	Coroutine _coAppearDissolve;

	#region ObjectInfoBinder
	public NatureInfo NatureInfo { get { return Info as NatureInfo; } }

	public override void Bind(CreatureInfo info)
	{
		Info = info;

		TemplateId = Info.ObjectId;
		IsDead = Info.IsDead;

		if (_init)
		{
			Hp = IsDead ? 0.0f : MaxHp;
			gameObject.SetActive(!IsDead);
		}
	}

	public override void Refresh()
	{
		if (Info == null)
			return;

		if (IsDead && Info.IsDead == false)
		{
			IsDead = false;
			Hp = MaxHp;
			gameObject.SetActive(true);
			LerpAppearDissolveStart();
		}
	}

	public override void Unbind()
	{
		Info = null;
	}

	#endregion
	void Start()
    {
        Init();
    }

    void Init()
	{
		if (_init)
			return;
		_init = true;

		ObjectData objectData = null;
		Managers.Data.ObjectDict.TryGetValue(TemplateId, out objectData);
		if (objectData != null)
		{
			ObjectType = objectData.objectType;
			MaterialType = objectData.materialType;
			Stat = objectData.stat;
			Hp = objectData.stat.maxHp;
		}

		if (_meshRenderer == null)
			_meshRenderer = GetComponentInChildren<Renderer>();
		if (_meshRenderer != null)
		{
			int matCount = _meshRenderer.sharedMaterials.Length;
			_blocks = new MaterialPropertyBlock[matCount];
			for (int i = 0; i < matCount; i++)
				_blocks[i] = new MaterialPropertyBlock();
		}

		if (Info != null)
		{
			Hp = IsDead ? 0.0f : MaxHp;
			gameObject.SetActive(!IsDead);
		}
	}

	void OnEnable()
	{

	}

	void OnDisable()
	{
		if (!_init)
			return;

		if (_meshRenderer != null)
		{
			for (int i = 0; i < _blocks.Length; i++)
			{
				_meshRenderer.GetPropertyBlock(_blocks[i], i);
				_blocks[i].SetFloat("_DissolveAmount", 0.0f);
				_meshRenderer.SetPropertyBlock(_blocks[i], i);
			}
		}
	}

	void Update()
    {
        if (Hp <= 0 && MaxHp > 0.0f)
            OnDead();
    }

	public override void OnDamaged(DamageCollider dm) 
	{
		float damage = dm.DamageCalculate(this);
		Hp = Mathf.Max(Hp - damage, 0.0f);

		CheckDrop();

		if (Hp > 0.0f)
			Shake();

		if (dm.Caster is PlayerController)
			Managers.Event.PlayerEvents.OnAttack(this);
	}

	protected override void OnDead()
    {
        if (IsDead)
            return;

		IsDead = true;
		if (NatureInfo != null)
		{
			NatureInfo.RewardList = null;
			NatureInfo.DropsGiven = 0;
		}
		gameObject.SetActive(false);
	}

	void CheckDrop()
	{
		if (NatureInfo == null)
			return;

		if (NatureInfo.RewardList == null || NatureInfo.RewardList.Count == 0)
		{
			NatureInfo.RewardList = new List<int>();
			NatureInfo.DropsGiven = 0;

			ObjectData objectData = null;
			Managers.Data.ObjectDict.TryGetValue(TemplateId, out objectData);
			foreach (RewardData rewardData in objectData.rewards)
			{
				int rand = Random.Range(0, 100);
				if (rand < rewardData.probability)
				{
					int count = Random.Range(rewardData.minCount, rewardData.maxCount + 1);
					for (int i = 0; i < count; i++)
						NatureInfo.RewardList.Add(rewardData.itemId);
				}
			}
		}

		float damagePerDrop = MaxHp / (float)NatureInfo.RewardList.Count;

		int expectedDrops = (int)((MaxHp - Hp) / damagePerDrop);

		while (NatureInfo.DropsGiven < expectedDrops && NatureInfo.DropsGiven < NatureInfo.RewardList.Count)
		{
			DropItem(NatureInfo.RewardList[NatureInfo.DropsGiven]);
			NatureInfo.DropsGiven++;
		}
	}

    void DropItem(int rewardItemId)
	{
        if (rewardItemId == 0)
            return;

		Vector3 pos = GetRandomPosOfDropItem(transform.position, 1.0f);

		RewardObjectInfo rewardObject = new RewardObjectInfo();
		rewardObject.SpawnPos = pos;
		rewardObject.Init(rewardItemId, 1, transform.position);
		Managers.Map.ApplyEnter(rewardObject);
	}

    Vector3 GetRandomPosOfDropItem(Vector3 center, float radius)
	{
        Vector3 pos = center;
        for (int i = 0; i < 10; i++)
        {
			Vector2 randWithinCircle = Random.insideUnitCircle * radius;
            randWithinCircle += randWithinCircle.normalized;
            pos.x = center.x + randWithinCircle.x;
            pos.z = center.z + randWithinCircle.y;

            Vector3 dir = pos - center;
            RaycastHit hit;
            if (!Physics.SphereCast(center, 0.5f, dir, out hit, dir.magnitude, 1 << (int)Layer.Block))
                return pos;
        }
        return center;
    }

    public void Shake()
    {
        StartCoroutine(ShakeCoroutine(_shakeDuration, _shakeMagnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.5f, 0.5f) * magnitude;
            float y = Random.Range(-0.5f, 0.5f) * magnitude;

            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPosition;
	}

	IEnumerator CoLerpAppearDissolve()
	{
		float elapsedTime = 0f;
		float duration = 2.0f;

		_meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		while (elapsedTime < duration)
		{
			float dissolve = Mathf.Lerp(1, 0f, elapsedTime / duration);
			for (int i = 0; i < _blocks.Length; i++)
			{
				_meshRenderer.GetPropertyBlock(_blocks[i], i);
				_blocks[i].SetFloat("_DissolveAmount", dissolve);
				_meshRenderer.SetPropertyBlock(_blocks[i], i);
			}
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		_meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		for (int i = 0; i < _blocks.Length; i++)
		{
			_meshRenderer.GetPropertyBlock(_blocks[i], i);
			_blocks[i].SetFloat("_DissolveAmount", 0.0f);
			_meshRenderer.SetPropertyBlock(_blocks[i], i);
		}

		_coAppearDissolve = null;
	}

	IEnumerator CoScaleUp()
	{
		float duration = 2.0f;

		for (float i = 0; i < duration; i += Time.deltaTime)
		{
			transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, i / duration);
			yield return null;
		}
		transform.localScale = Vector3.one;

		_coAppearDissolve = null;
	}

	public void LerpAppearDissolveStart()
	{
		if (_meshRenderer == null)
			return;
		if (gameObject.tag == "Star") // TEMP
			_coAppearDissolve = StartCoroutine(CoScaleUp());
		else
			_coAppearDissolve = StartCoroutine(CoLerpAppearDissolve());
	}
}
