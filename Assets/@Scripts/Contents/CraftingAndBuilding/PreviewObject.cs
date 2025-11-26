using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PreviewObject : MonoBehaviour
{
    [SerializeField]
    float _heightBarUI = 2.5f;

    BuildingSystem _buildingSystem;
    public int TemplateId { get; private set; }
	public float ProgressRatio { get; private set; }

	List<Collider> _colliderList = new List<Collider>();

    Material _green;
    Material _red;

    Coroutine _coBuild;


    void Start()
    {
        _green = Managers.Resource.Load<Material>("Materials/Preview_Green");
        _red = Managers.Resource.Load<Material>("Materials/Preview_Red");
        Managers.UI.MakeWorldSpaceUI<UI_BuildingBar>(transform).SetInfo(this, _heightBarUI);
    }

    public void SetInfo(int templateId, BuildingSystem buildingSystem)
    {
        TemplateId = templateId;
        _buildingSystem = buildingSystem;
    }

    void Update()
    {
        if (ProgressRatio >= 1.0f && _coBuild == null)
        {
            _buildingSystem.Build();
        }

        ChangeColor();
    }

    void ChangeColor()
    {
        if (_colliderList.Count > 0)
            SetColor(_red);
        else
            SetColor(_green);
    }

    void SetColor(Material mat)
    {
        Renderer[] renderersInChildren = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderersInChildren)
        {
            var newMaterials = new Material[r.materials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
                newMaterials[i] = mat;
            r.materials = newMaterials;
        }
    }

    public void StartBuilding(float speed)
    {
        GetComponent<Collider>().isTrigger = false;
        _coBuild = StartCoroutine(CoBuild(speed));
    }

    IEnumerator CoBuild(float speed)
    {
        ObjectData objectData = null;
        Managers.Data.ObjectDict.TryGetValue(TemplateId, out objectData);

        float curHp = 0.0f;
        while (curHp < objectData.stat.maxHp)
        {
			ProgressRatio = curHp / objectData.stat.maxHp;
			yield return null;
			curHp += speed * Time.deltaTime;
		}
        ProgressRatio = 1.0f;
		_coBuild = null;
	}

    int _layerMask = (1 << (int)Layer.Ground) | (1 << (int)Layer.DRM) | (1 << (int)Layer.IgnoreRaycast);

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _layerMask) == 0)
        {
            if (other.GetComponentInChildren<DamageCollider>() == null)
            {
                _colliderList.Add(other);

				Debug.Log(other.gameObject.name);
				Debug.Log("layer = " + other.gameObject.layer);
			}
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & _layerMask) == 0)
            _colliderList.Remove(other);
    }

	public bool IsBuildable()
    {
        return _colliderList.Count == 0;
    }
}
