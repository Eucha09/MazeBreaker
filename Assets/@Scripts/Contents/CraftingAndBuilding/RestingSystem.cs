using Data;
using Player;
using UnityEngine;

public class RestingSystem : MonoBehaviour
{
	[SerializeField]
	int _objectId;
	public int ObjectId { get { return _objectId; } set { _objectId = value; } }
	public string Name { get; private set; }
	public PlayerInteractionObjectType InteractionObjectType { get; set; }

	double _combustionStartTime;
	double _combustionEndTime;
	public float FuelProgressRatio
	{
		get
		{
			if (_combustionEndTime - _combustionStartTime < 0.001)
				return 0.0f;
			double ratio = (Managers.Time.CurTime - _combustionStartTime) / (_combustionEndTime - _combustionStartTime);
			return Mathf.Clamp01((float)ratio);
		}
	}

	public bool IsActive { get { return _storage == null ? true : Managers.Time.CurTime < _combustionEndTime; } }

	Storage _storage;

	bool _isResting;
	PlayerController _player;

	[SerializeField]
	Transform _interactPoint;
	[SerializeField]
	AudioClip _audioClip;

	#region ObjectInfoBinder
	public BuildingObjectInfo Info { get; set; }

	public void Bind(BuildingObjectInfo info)
	{
		Info = info;

		_combustionStartTime = Info.CombustionStartTime;
		_combustionEndTime = Info.CombustionEndTime;
	}

	public void Refresh()
	{
		if (Info == null)
			return;

	}

	public void Unbind()
	{
		Info.CombustionStartTime = _combustionStartTime;
		Info.CombustionEndTime = _combustionEndTime;
		Info = null;
	}
	#endregion

	void Start()
	{
		_storage = GetComponent<Storage>();
		_objectId = GetComponentInParent<BaseController>().TemplateId;
		ObjectData objectData = null;
		Managers.Data.ObjectDict.TryGetValue(_objectId, out objectData);
		Name = objectData.name;

		Managers.Sound.Play3DLoop(gameObject, _audioClip, 0, 20.0f);
	}

    void Update()
	{
		// 연료 태우기
		if (_storage != null && _combustionEndTime < Managers.Time.CurTime)
		{
			Item item = _storage.GetItemBySlot(_storage.StartSlot);
			CraftingMaterial cm = item as CraftingMaterial;
			if (cm != null && cm.Fuelable)
			{
				_combustionStartTime = Managers.Time.CurTime;
				_combustionEndTime = _combustionStartTime + cm.FuelEfficiency;
				_storage.RemoveFromSlot(_storage.StartSlot, 1);
			}
		}


		if (!IsActive && _isResting)
		{
			_player.PlayerInteractionEnd();
		}
	}

	public void Resting(PlayerController player)
	{
		if (!IsActive)
			return;

		_isResting = true;
		_player = player;

		_player.PlayerInteractionStart(InteractionObjectType,
			_interactPoint.position, _interactPoint.forward, GetComponent<Interactable>());

		UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
		ui.ShowTimeScaleControlUI();
	}

	public void Cancel()
	{
		Managers.Object.GetPlayer().GetComponent<Rigidbody>().MovePosition(_interactPoint.position);

		_isResting = false;
		_player = null;

		UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
		ui.CloseTimeScaleControlUI();
	}
}
