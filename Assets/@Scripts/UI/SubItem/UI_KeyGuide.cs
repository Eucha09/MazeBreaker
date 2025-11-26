using UnityEngine;
using static Define;

public class UI_KeyGuide : UI_Base
{
	enum GameObjects
	{
		KeyboardMouse,
		Gamepad,
	}

	public override void Init()
	{
		Bind<GameObject>(typeof(GameObjects));

		OnControlsChanged(Managers.Input.DeviceMode);
		Managers.Input.ControlsChangedHandler -= OnControlsChanged;
		Managers.Input.ControlsChangedHandler += OnControlsChanged;
	}

	public void OnControlsChanged(DeviceMode deviceMode)
	{
		if (deviceMode == DeviceMode.KeyboardMouse)
		{
			GetObject((int)GameObjects.KeyboardMouse).SetActive(true);
			GetObject((int)GameObjects.Gamepad).SetActive(false);
		}
		else if (deviceMode == DeviceMode.Gamepad)
		{
			GetObject((int)GameObjects.KeyboardMouse).SetActive(false);
			GetObject((int)GameObjects.Gamepad).SetActive(true);
		}
	}

	void OnDestroy()
	{
		Managers.Input.ControlsChangedHandler -= OnControlsChanged;
	}
}
