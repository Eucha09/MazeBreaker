using UnityEngine;

public class TemporaryStorage : Storage
{
	float _delay;
	void Update()
	{
		if (_delay > 0.0f)
		{
			_delay -= Time.unscaledDeltaTime;
			return;
		}

		int itemCount = Managers.Inven.GetCountSlotsFilled(StartSlot, EndSlot);
		if (itemCount == 0)
		{
			UI_GameScene sceneUI = Managers.UI.SceneUI as UI_GameScene;
			sceneUI.CloseStorageUI();
			if (Info != null)
				Managers.Map.ApplyLeave(Info);
			else
				Managers.Resource.Destroy(gameObject);
		}
		else
			_delay = 0.25f;
	}
}
