using Data;
using UnityEngine;

public class StorageObject : Interactable
{
	int _objectId;
	bool _activeUI;
	PlayerController _player;

	void Start()
	{
		_player = Managers.Object.GetPlayer();

		BaseController bc = GetComponent<BaseController>();
		if (bc != null)
		{
			_objectId = bc.TemplateId;
			ObjectData objectData = null;
			Managers.Data.ObjectDict.TryGetValue(_objectId, out objectData);
			_name = objectData.name;
		}
	}

	void Update()
	{
		if (_activeUI && (_player.transform.position - transform.position).magnitude > 3.0f)
		{
			UI_GameScene sceneUI = Managers.UI.SceneUI as UI_GameScene;
			sceneUI.CloseStorageUI();
			_activeUI = false;
		}
	}

	public override void Interact()
	{
		// 창 띄우기
		Storage storage = GetComponent<Storage>();
		UI_GameScene sceneUI = Managers.UI.SceneUI as UI_GameScene;
		sceneUI.ShowStorageUI(storage);
		_activeUI = true;
	}
}
