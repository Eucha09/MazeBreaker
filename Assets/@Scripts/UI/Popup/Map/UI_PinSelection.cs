using Data;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_PinSelection : UI_Base
{
	enum GameObjects
	{ 
		Grid,
	}

	enum Buttons
	{
		OkButton,
		DeleteButton,
	}

	public bool IsActive { get; private set; }
	UI_Map_Node _node;
	bool _isTemp;

	List<UI_PinSelection_Item> _items;
	int _columns;
	int _rows;
	int _curIdx;

	public override void Init()
	{
		Bind<GameObject>(typeof(GameObjects));
		Bind<Button>(typeof(Buttons));

		GetButton((int)Buttons.OkButton).gameObject.BindEvent(OnOkButtonClick);
		GetButton((int)Buttons.DeleteButton).gameObject.BindEvent(OnDeleteButtonClick);
	}

	public void OnOkButtonClick(PointerEventData data)
	{
		_isTemp = false;
		CloseUI();
	}

	public void OnDeleteButtonClick(PointerEventData data)
	{
		_isTemp = true;
		CloseUI();
	}

	public void ShowUI(UI_Map_Node node, bool isTemp = false)
	{
		IsActive = true;
		_node = node;
		_isTemp = isTemp;

		if (_items == null)
		{
			_items = new List<UI_PinSelection_Item>();
			GameObject grid = GetObject((int)GameObjects.Grid);
			foreach (Transform child in grid.transform)
				Destroy(child.gameObject);
			foreach (MinimapMarkerData markerData in Managers.Data.MinimapMarkerDict.Values)
			{
				if (markerData.isPin == false)
					continue;
				UI_PinSelection_Item item = Managers.UI.MakeSubItem<UI_PinSelection_Item>(grid.transform);
				item.SetPinData(markerData);
				_items.Add(item);
			}
			_columns = GetColumnCount();
			_rows = Mathf.CeilToInt((float)_items.Count / _columns);
		}

		for (int i = 0; i < _items.Count; i++)
		{
			_items[i].SetNode(i, _node);
			if (_node.Icon == _items[i].IconSprite)
				_curIdx = i;
		}

		GetComponent<UI_Slide>().ShowUI();
	}

	public void CloseUI()
	{
		IsActive = false;

		if (_isTemp)
			Managers.Minimap.RemoveMarker(_node.MinimapObject);
		else if (_node != null)
			_node.Cancel();

		_node = null;
		_isTemp = false;
		GetComponent<UI_Slide>().HideUI();
	}

	public void SelectPinType(int idx)
	{
		if (idx >= _items.Count)
			return;

		_curIdx = idx;
		_node.SetTemplateId(_items[idx].TemplateId);
	}

	public void MoveCursor(int dx, int dy)
	{
		int curRow = _curIdx / _columns;
		int curCol = _curIdx % _columns;
		int newRow = Mathf.Clamp(curRow - dy, 0, _rows - 1);
		int newCol = Mathf.Clamp(curCol + dx, 0, _columns - 1);
		int newIdx = newRow * _columns + newCol;

		if (newIdx < _items.Count && _curIdx != newIdx)
			SelectPinType(newIdx);
	}

	int GetColumnCount()
	{
		GridLayoutGroup grid = GetObject((int)GameObjects.Grid).GetComponent<GridLayoutGroup>();
		Rect rect = grid.GetComponent<RectTransform>().rect;

		float totalWidth = rect.width;
		float cellWidth = grid.cellSize.x;
		float spacing = grid.spacing.x;
		float padding = grid.padding.left + grid.padding.right;

		int columns = Mathf.FloorToInt((totalWidth + spacing - padding) / (cellWidth + spacing));
		return Mathf.Max(1, columns);
	}
}
