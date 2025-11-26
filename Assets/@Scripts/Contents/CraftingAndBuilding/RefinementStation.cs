using Data;
using UnityEngine;

public class RefinementStation : Interactable
{
	int _objectId;
	bool _activeUI;
	PlayerController _player;

	void Start()
	{
		_player = Managers.Object.GetPlayer();

		_objectId = GetComponent<BaseController>().TemplateId;
		ObjectData objectData = null;
		Managers.Data.ObjectDict.TryGetValue(_objectId, out objectData);
		_name = objectData.name;
	}

	void Update()
	{
		if (_activeUI && (_player.transform.position - transform.position).magnitude > 5.0f)
		{
			UI_GameScene sceneUI = Managers.UI.SceneUI as UI_GameScene;
			sceneUI.CloseRefinementStationUI();
			_activeUI = false;
		}
	}

	public override void Interact()
	{
		// 창 띄우기
		RefinementSystem refinementSystem = GetComponent<RefinementSystem>();
		Storage storage = GetComponent<Storage>();
		UI_GameScene sceneUI = Managers.UI.SceneUI as UI_GameScene;
		sceneUI.ShowRefinementStationUI(refinementSystem, storage);
		_activeUI = true;
	}
}
