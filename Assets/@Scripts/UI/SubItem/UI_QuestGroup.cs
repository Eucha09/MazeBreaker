using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_QuestGroup : UI_Base
{
    [SerializeField]
	TextMeshProUGUI _title;
	[SerializeField]
	Transform _questItems;
	[SerializeField]
	UI_ToolTip _tooltip;

	List<UI_Quest_Item> items = new List<UI_Quest_Item>();
    QuestGroup _questGroup;

	public override void Init()
    {
        _tooltip.gameObject.SetActive(false);

		Show(1.0f, 1.5f);
	}

    public void SetInfo(QuestGroup questGroup)
    {
        _questGroup = questGroup;
        _title.text = questGroup.Title;
    }

    void Update()
    {
        if (_questGroup == null)
            return;

        if (!_isHide && _questGroup.IsAllFinished())
        {
            Hide(1.0f, 0.0f, true);
        }

        while (items.Count < _questGroup.Quests.Count)
        {
            UI_Quest_Item item = Managers.UI.MakeSubItem<UI_Quest_Item>(_questItems);
            item.SetInfo(_questGroup.Quests[items.Count]);
            items.Add(item);
        }
    }

    public void ShowTooltipUI(string text, Vector3 position)
    {
        _tooltip.SetToolTip(text);
        _tooltip.transform.position = position;
        _tooltip.gameObject.SetActive(true);
	}

	public void CloseTooltipUI()
	{
		_tooltip.gameObject.SetActive(false);
	}
}
