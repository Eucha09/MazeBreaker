using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UIManager
{
    int _order = 10;

    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    UI_Scene _sceneUI = null;
    UI_VirtualCursor _cursor = null;

    public UI_Scene SceneUI { get { return _sceneUI; } }
    public int PopupUICount { get { return _popupStack.Count; } }
    public UI_VirtualCursor Cursor { get { return _cursor; } }
    public bool IsActiveVirtualCursor { get { return Cursor != null && Cursor.gameObject.activeSelf; } }

	public GameObject Root
    {
        get
        {
			GameObject root = GameObject.Find("@UI_Root");
			if (root == null)
				root = new GameObject { name = "@UI_Root" };
            return root;
		}
	}

	public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = 0;
        }
    }

	public T MakeWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UI_Base
	{
		if (string.IsNullOrEmpty(name))
			name = typeof(T).Name;

		GameObject go = Managers.Resource.Instantiate($"UI/WorldSpace/{name}", parent);

        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

		return Util.GetOrAddComponent<T>(go);
	}

	public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
	{
		if (string.IsNullOrEmpty(name))
			name = typeof(T).Name;

		GameObject go = Managers.Resource.Instantiate($"UI/SubItem/{name}", parent);

		return Util.GetOrAddComponent<T>(go);
	}

    public UI_VirtualCursor ShowVirtualCursor()
    {
        if (Managers.Input.DeviceMode != DeviceMode.Gamepad)
            return null;
        if (_cursor != null)
        {
			_cursor.gameObject.SetActive(true);
			return _cursor;
        }

        GameObject go = Managers.Resource.Instantiate("UI/Cursor/UI_VirtualCursor");
		_cursor = go.GetComponent<UI_VirtualCursor>();
		go.transform.SetParent(Root.transform);

        return _cursor;
	}

    public void CloseVirtualCursor()
    {
        if (_cursor == null)
            return;

        _cursor.Clear();
		_cursor.gameObject.SetActive(false);
    }

	public T ShowSceneUI<T>(string name = null) where T : UI_Scene
	{
		if (string.IsNullOrEmpty(name))
			name = typeof(T).Name;

		GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");
		T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;

		go.transform.SetParent(Root.transform);

		return sceneUI;
	}

	public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/Popup/{name}");
        T popup = Util.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);

        go.transform.SetParent(Root.transform);

		return popup;
    }

    public UI_Popup TopPopup()
    {
        if (_popupStack.Count == 0)
            return null;
        return _popupStack.Peek();
    }

    public void ClosePopupUI(UI_Popup popup)
    {
		if (_popupStack.Count == 0)
			return;

		Stack<UI_Popup> temp = new Stack<UI_Popup>();
        while (_popupStack.Count > 0 && _popupStack.Peek() != popup)
        {
            UI_Popup ui = _popupStack.Pop();
            temp.Push(ui);
            _order--;
		}

		ClosePopupUI();

        while (temp.Count > 0)
        {
            UI_Popup ui = temp.Pop();
            SetCanvas(ui.gameObject, true);
            _popupStack.Push(ui);
        }
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_Popup popup = _popupStack.Pop();
        popup.Clear();
        Managers.Resource.Destroy(popup.gameObject);
        popup = null;
        _order--;
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    public void Clear()
    {
        CloseAllPopupUI();
        _sceneUI = null;
    }
}
