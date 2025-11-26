using Data;
using Player;
using UnityEngine;

public class FireCamp : Interactable
{
	int _objectId;
	bool _activeUI;
	PlayerController _player;
	RestingSystem _restingSystem;

	[SerializeField]
	GameObject _effect;

	void Start()
	{
		_player = Managers.Object.GetPlayer();

		_objectId = GetComponent<BaseController>().TemplateId;
		ObjectData objectData = null;
		Managers.Data.ObjectDict.TryGetValue(_objectId, out objectData);
		_name = objectData.name;

		_restingSystem = GetComponent<RestingSystem>();
		_restingSystem.InteractionObjectType = PlayerInteractionObjectType.BonFire;
	}

	void Update()
	{
		if (_activeUI && (_player.transform.position - transform.position).magnitude > 5.0f)
		{
			UI_GameScene sceneUI = Managers.UI.SceneUI as UI_GameScene;
			sceneUI.CloseRestingUI();
			_activeUI = false;
		}

		if (_restingSystem.IsActive && _effect.activeSelf == false)
			_effect.SetActive(true);
		else if (_restingSystem.IsActive == false && _effect.activeSelf)
            _effect.SetActive(false);
    }

	public override void Interact()
    {
        GetComponent<RestingSystem>().Resting(_player);
  //      UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
		//ui.ShowRestingUI(_restingSystem, GetComponent<Storage>());
		//_activeUI = true;
	}


	public override void Cancel()
	{
		GetComponent<RestingSystem>().Cancel();
	}
}
