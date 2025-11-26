using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static Define;

public class InputManager
{
    // Delegate
    public Action KeyAction = null;
    //public Action<int, Define.MouseEvent> MouseAction = null;
    public Action<Define.MouseEvent> MouseActionL = null;
    public Action<Define.MouseEvent> MouseActionR = null;

    public Action<DeviceMode> ControlsChangedHandler = null;
    DeviceMode _deviceMode = DeviceMode.KeyboardMouse;
	public DeviceMode DeviceMode 
    { 
        get { return _deviceMode; } 
        set
        {
            _deviceMode = value;
            if (_deviceMode == DeviceMode.KeyboardMouse)
            {
                UnityEngine.Cursor.visible = true;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                Managers.UI.CloseVirtualCursor();
            }
            else if (_deviceMode == DeviceMode.Gamepad)
            {
                UnityEngine.Cursor.visible = false;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            }
            if (ControlsChangedHandler != null)
                ControlsChangedHandler(_deviceMode);
		}
	}

	// Input Mouse
	bool _pressedL = false;
    float _pressedTimeL = 0;
    bool _pressedR = false;
    float _pressedTimeR = 0;

    public void OnUpdate()
    {
        if (KeyAction != null)
            KeyAction.Invoke();

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // If there's no input keys, Stop. 
        //if (Input.anyKey == false) return;
        //Debug.Assert(Input.anyKey == true, "DIDN'T STOP ; ALTHO we couldn't detect inputs");


        #region MouseAction
        if (MouseActionL != null)
        {
            if (Input.GetMouseButton(0))
            {
                if (!_pressedL)
                {
                    MouseActionL.Invoke(Define.MouseEvent.PointerDown);
                    _pressedTimeL = Time.time;
                }
                MouseActionL.Invoke(Define.MouseEvent.Press);
                _pressedL = true;
            }
            else
            {
                if (_pressedL)
                {
                    if (Time.time < _pressedTimeL + 0.2f)
                        MouseActionL.Invoke(Define.MouseEvent.Click);
                    MouseActionL.Invoke(Define.MouseEvent.PointerUp);
                }
                _pressedL = false;
                _pressedTimeL = 0;
            }
        }
        if (MouseActionR != null)
        {
            if (Input.GetMouseButton(1))
            {
                if (!_pressedR)
                {
                    MouseActionR.Invoke(Define.MouseEvent.PointerDown);
                    _pressedTimeR = Time.time;
                }
                MouseActionR.Invoke(Define.MouseEvent.Press);
                _pressedR = true;
            }
            else
            {
                if (_pressedR)
                {
                    if (Time.time < _pressedTimeR + 0.2f)
                        MouseActionR.Invoke(Define.MouseEvent.Click);
                    MouseActionR.Invoke(Define.MouseEvent.PointerUp);
                }
                _pressedR = false;
                _pressedTimeR = 0;
            }
        }
        #endregion 
    }

    public Vector3 GetMousePosition()
    {
        if (Managers.UI.Cursor != null && Managers.UI.Cursor.gameObject.activeSelf == true)
            return Managers.UI.Cursor.GetCursorScreenPosition();
        else
            return Input.mousePosition;
    }

    public bool IsPointerOverGameObject()
    {
        if (DeviceMode == DeviceMode.KeyboardMouse)
            return EventSystem.current.IsPointerOverGameObject();
        else if (DeviceMode == DeviceMode.Gamepad)
        {
            if (Managers.UI.Cursor != null && Managers.UI.Cursor.gameObject.activeSelf == true)
                return Managers.UI.Cursor.RaycastUI() != null;
            else
                return false;
		}
        return false;
	}

	public void Clear()
    {
        KeyAction = null;
        //MouseAction = null;
        MouseActionL = null;
        MouseActionR = null;
        ControlsChangedHandler = null;
	}
}
