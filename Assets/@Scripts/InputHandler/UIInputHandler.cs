using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;
using static Define;

public class UIInputHandler : MonoBehaviour
{
    UI_GameScene _sceneUI;

    void Start()
    {
		_sceneUI = Managers.UI.SceneUI as UI_GameScene;
    }

    public void OnInven(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			if (Managers.UI.PopupUICount == 0)
				_sceneUI.ShowOrCloseInven();
		}
	}

	UI_Map _mapUI;
	public void OnRadar(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			_sceneUI.OnCancelButton();
			if (_mapUI == null)
				_mapUI = Managers.UI.ShowPopupUI<UI_Map>();
			else
			{
				Managers.UI.ClosePopupUI(_mapUI);
				_mapUI = null;
			}
		}
	}

	public void OnQuickSlot1(InputAction.CallbackContext context)
	{
		if (context.performed)
			_sceneUI.GetInputQuickSlot(1);
	}

	public void OnQuickSlot2(InputAction.CallbackContext context)
	{
		if (context.performed)
			_sceneUI.GetInputQuickSlot(2);
	}

	public void OnQuickSlot3(InputAction.CallbackContext context)
	{
		if (context.performed)
			_sceneUI.GetInputQuickSlot(3);
	}

	public void OnQuickSlot4(InputAction.CallbackContext context)
	{
		if (context.performed)
			_sceneUI.GetInputQuickSlot(4);
	}

	public void OnQuickSlot5(InputAction.CallbackContext context)
	{
		if (context.performed)
			_sceneUI.GetInputQuickSlot(5);
	}

	public void OnQuickSlot6(InputAction.CallbackContext context)
	{
		if (context.performed)
			_sceneUI.GetInputQuickSlot(6);
	}

	public void OnQuickSlot7(InputAction.CallbackContext context)
	{
		if (context.performed)
			_sceneUI.GetInputQuickSlot(7);
	}

	public void OnQuickSlot8(InputAction.CallbackContext context)
	{
		if (context.performed)
			_sceneUI.GetInputQuickSlot(8);
	}

	public void RotationL_BuildingMode(InputAction.CallbackContext context)
	{
		if (context.performed)
			GetComponent<BuildingSystem>().BuildingObjectRotation(-90);
	}

	public void RotationR_BuildingMode(InputAction.CallbackContext context)
	{
		if (context.performed)
			GetComponent<BuildingSystem>().BuildingObjectRotation(90);
	}

	public void OnOkButton(InputAction.CallbackContext context)
	{
		UI_Base ui = Managers.UI.PopupUICount > 0 ? Managers.UI.TopPopup() : Managers.UI.SceneUI;
		if (ui == null)
			return;

		if (context.performed)
			ui.OnOkButton();
	}

	public void OnCancelButton(InputAction.CallbackContext context)
	{
		UI_Base ui = Managers.UI.PopupUICount > 0 ? Managers.UI.TopPopup() : Managers.UI.SceneUI;
		if (ui == null)
			return;

		if (context.performed && !_cancelLocked)
			ui.OnCancelButton();
	}

	bool _cancelLocked = false;
	Coroutine _coLockCancel;

	public void LockInputCancelBriefly()
	{
		if (_coLockCancel != null)
			return;
		_coLockCancel = StartCoroutine(CoLockInputCancelBriefly());
	}

	IEnumerator CoLockInputCancelBriefly()
	{
		_cancelLocked = true;
		yield return null;
		_cancelLocked = false;
		_coLockCancel = null;
	}

	public void OnDeleteButton(InputAction.CallbackContext context)
	{
		UI_Base ui = Managers.UI.PopupUICount > 0 ? Managers.UI.TopPopup() : Managers.UI.SceneUI;
		if (ui == null)
			return;

		if (context.performed)
			ui.OnDeleteButton();
	}

	public void OnMoveUICursor(InputAction.CallbackContext context)
	{
		UI_VirtualCursor virtualCursor = Managers.UI.Cursor;
		UI_Base ui = Managers.UI.PopupUICount > 0 ? Managers.UI.TopPopup() : Managers.UI.SceneUI;

		if (context.performed)
		{
			if (virtualCursor != null && virtualCursor.gameObject.activeSelf)
				virtualCursor.MoveDir = context.ReadValue<Vector2>();
			else if (ui != null)
				ui.MoveDir = context.ReadValue<Vector2>();
		} 
		else if (context.canceled)
		{
			if (virtualCursor != null && virtualCursor.gameObject.activeSelf)
				virtualCursor.MoveDir = Vector2.zero;
			if (ui != null)
				ui.MoveDir = Vector2.zero;
		}
	}

	public void OnZoomUICursor(InputAction.CallbackContext context)
	{
		UI_Base ui = Managers.UI.PopupUICount > 0 ? Managers.UI.TopPopup() : Managers.UI.SceneUI;

		if (context.performed)
		{
			if (ui != null)
				ui.ZoomScale = context.ReadValue<Vector2>().y;
		}
		else if (context.canceled)
		{
			if (ui != null)
				ui.ZoomScale = 0.0f;
		}
	}

	public void OnClickLeftBtnUICursor(InputAction.CallbackContext context)
	{
		UI_VirtualCursor virtualCursor = Managers.UI.Cursor;
		UI_Base popupUI = Managers.UI.PopupUICount > 0 ? Managers.UI.TopPopup() : Managers.UI.SceneUI;

		if (context.performed)
		{
			if (virtualCursor != null && virtualCursor.gameObject.activeSelf)
				virtualCursor.OnPointerDown(PointerEventData.InputButton.Left);
			else if (popupUI != null)
				popupUI.OnPointerDown(PointerEventData.InputButton.Left);
		}
		else if (context.canceled)
		{
			if (virtualCursor != null && virtualCursor.gameObject.activeSelf)
				virtualCursor.OnPointerUpAndClick(PointerEventData.InputButton.Left);
			else if (popupUI != null)
				popupUI.OnPointerUpAndClick(PointerEventData.InputButton.Left);

			//TEMP
			GetComponent<BuildingSystem>().OnBuild();
		}
	}

	public void OnClickRightBtnUICursor(InputAction.CallbackContext context)
	{
		UI_VirtualCursor virtualCursor = Managers.UI.Cursor;
		UI_Base popupUI = Managers.UI.PopupUICount > 0 ? Managers.UI.TopPopup() : Managers.UI.SceneUI;

		if (context.performed)
		{
			if (virtualCursor != null && virtualCursor.gameObject.activeSelf)
				virtualCursor.OnPointerDown(PointerEventData.InputButton.Right);
			else if (popupUI != null)
				popupUI.OnPointerDown(PointerEventData.InputButton.Right);
		}
		else if (context.canceled)
		{
			if (virtualCursor != null && virtualCursor.gameObject.activeSelf)
				virtualCursor.OnPointerUpAndClick(PointerEventData.InputButton.Right);
			else if (popupUI != null)
				popupUI.OnPointerUpAndClick(PointerEventData.InputButton.Right);

			//TEMP
			GetComponent<BuildingSystem>().OnDemolition();
		}
	}

	public void OnClickMiddleBtnUICursor(InputAction.CallbackContext context)
	{
		UI_VirtualCursor virtualCursor = Managers.UI.Cursor;
		UI_Base popupUI = Managers.UI.PopupUICount > 0 ? Managers.UI.TopPopup() : Managers.UI.SceneUI;

		if (context.performed)
		{
			if (virtualCursor != null && virtualCursor.gameObject.activeSelf)
				virtualCursor.OnPointerDown(PointerEventData.InputButton.Middle);
			else if (popupUI != null)
				popupUI.OnPointerDown(PointerEventData.InputButton.Middle);
		}
		else if (context.canceled)
		{
			if (virtualCursor != null && virtualCursor.gameObject.activeSelf)
				virtualCursor.OnPointerUpAndClick(PointerEventData.InputButton.Middle);
			else if (popupUI != null)
				popupUI.OnPointerUpAndClick(PointerEventData.InputButton.Middle);
		}
	}

	public void OnControlsChanged(PlayerInput playerInput)
	{
		string curControlScheme = playerInput.currentControlScheme;
		Debug.Log($"입력 디바이스 변경됨: {curControlScheme}");
		if (curControlScheme == "KeyboardMouse")
			Managers.Input.DeviceMode = DeviceMode.KeyboardMouse;
		else if (curControlScheme == "Gamepad")
			Managers.Input.DeviceMode = DeviceMode.Gamepad;
	}
}
