using Data;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class StructureController : BaseController
{
	[SerializeField]
	StructureType _structureType;
	public StructureType StructureType { get { return _structureType; } protected set { _structureType = value; } }

	public Material _highlightMaterial;
	private List<Renderer> _renderers = new List<Renderer>();
	private List<Material[]> _originalMaterials = new List<Material[]>();

	float _shakeDuration = 0.5f;
	float _shakeMagnitude = 0.1f;

	// public AudioClip clip;

	#region ObjectInfoBinder
	public BuildingObjectInfo BuildingObjectInfo { get; set; }

	public void Bind(BuildingObjectInfo info)
	{
		BuildingObjectInfo = info;

		TemplateId = BuildingObjectInfo.ObjectId;
		Hp = BuildingObjectInfo.Hp;
	}

	public override void Refresh()
	{
		if (BuildingObjectInfo == null)
			return;


	}

	public override void Unbind()
	{
		BuildingObjectInfo.Hp = Hp;
		BuildingObjectInfo = null;
	}
	#endregion

	void Start()
	{
		ObjectData objectData = null;
		Managers.Data.ObjectDict.TryGetValue(TemplateId, out objectData);
		ObjectType = objectData.objectType;
		MaterialType = objectData.materialType;
		StructureType = (objectData as StructureData).structureType;
		MaxHp = objectData.stat.maxHp;
		Defense = objectData.stat.defense;
		if (BuildingObjectInfo != null)
			Hp = objectData.stat.maxHp;

		IsDead = false;

		_highlightMaterial = Managers.Resource.Load<Material>("Materials/Preview_Red");
		_renderers.AddRange(GetComponentsInChildren<Renderer>());
		foreach (Renderer rend in _renderers)
			_originalMaterials.Add(rend.materials);
	}

	void Update()
	{
		if (Hp <= 0)
			OnDead();
	}

	/*
	public override void OnDamaged(float damage, BaseController attacker)
	{
		//Managers.Sound.Play(clip);
		base.OnDamaged(damage, attacker);

		if (_isDead)
			return;

		Shake();
	}
	*/

    public override void OnDamaged(DamageCollider dm)
    {

		Hp -= dm.DamageCalculate(this);

        if (IsDead)
            return;
		Debug.Log("Damaged");
        Shake();
    }


    protected override void OnDead()
	{
		if (IsDead)
			return;
		
		IsDead = true;

		if (BuildingObjectInfo != null)
			Managers.Map.ApplyLeave(Info);
		else
			Managers.Resource.Destroy(gameObject);
	}

	public void Shake()
	{
		Debug.Log("Shake");
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

	public void SetHighlightMat()
	{
		foreach (Renderer rend in _renderers)
		{
			Material[] highlightMats = new Material[rend.materials.Length];
			for (int i = 0; i < highlightMats.Length; i++)
				highlightMats[i] = _highlightMaterial;
			rend.materials = highlightMats;
		}
	}

	public void SetOriginalMat()
	{
		for (int i = 0; i < _renderers.Count; i++)
		{
			_renderers[i].materials = _originalMaterials[i];
		}
	}

	void OnDisable()
	{
		IsDead = false;
	}
}
