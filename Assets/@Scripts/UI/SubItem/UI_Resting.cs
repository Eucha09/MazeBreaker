using UnityEngine;
using UnityEngine.UI;

public class UI_Resting : UI_Base
{
	[SerializeField]
	Text _titleText;
	[SerializeField]
	GameObject _restButton;
	[SerializeField]
	UI_Storage_Item _fuelSlot;

	RestingSystem _restingSystem;
	UI_Storage _storageUI;
	bool _init;

	public override void Init()
	{
		_storageUI = GetComponent<UI_Storage>();

		_restButton.BindEvent((data) => { _restingSystem.Resting(Managers.Object.GetPlayer()); });

		RefreshUI();
		_init = true;
	}

	void Update()
	{
		if (_fuelSlot != null)
			_fuelSlot.SetProgressRatio(_restingSystem.FuelProgressRatio);
	}

	public void SetRestingSystem(RestingSystem restingSystem)
	{
		_restingSystem = restingSystem;
		if (_restingSystem == null)
			return;

		_titleText.text = _restingSystem.name;
	}

	public void RefreshUI()
	{
		if (gameObject.activeInHierarchy == false)
			return;
		if (_init == false)
			return;

		if (_storageUI != null)
			_storageUI.RefreshUI();
	}
}
