using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_Quest_Item : UI_Base
{
    [SerializeField]
	TextMeshProUGUI _context;
	[SerializeField]
    Toggle _toggle;
	[SerializeField]
	GameObject _tooltipIcon;
	[SerializeField]
	Transform _tooltipPos;
	[SerializeField]
	GameObject _tooltipEffect;

	string _contextText;
	string _tootipText;

    Quest _quest;
	UI_QuestGroup _parent;

    public override void Init()
    {
		_tooltipIcon.gameObject.BindEvent(OnPointerEnter, Define.UIEvent.PointerEnter);
		_tooltipIcon.gameObject.BindEvent(OnPointerExit, Define.UIEvent.PointerExit);
		_parent = GetComponentInParent<UI_QuestGroup>(true);

		OnControlsChanged(Managers.Input.DeviceMode);
		Managers.Input.ControlsChangedHandler -= OnControlsChanged;
		Managers.Input.ControlsChangedHandler += OnControlsChanged;
	}

    public void SetInfo(Quest quest)
    {
        _quest = quest;
        _tootipText = quest.ToolTip;

		if (quest.ToolTip == null || quest.ToolTip == "")
			_tooltipIcon.gameObject.SetActive(false);
		else
			_tooltipIcon.gameObject.SetActive(true);
	}

    void Update()
    {
        _context.text = $"{_contextText}";
        if (_quest.TargetValue > 1)
            _context.text += $" [{_quest.CurValue}/{_quest.TargetValue}]";
        _toggle.isOn = _quest.IsFinished;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_parent.ShowTooltipUI(_tootipText, _tooltipPos.position);
		_tooltipEffect.SetActive(false);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_parent.CloseTooltipUI();
	}

	public void OnControlsChanged(DeviceMode deviceMode)
	{
		if (deviceMode == DeviceMode.KeyboardMouse)
		{
			_contextText = _quest.Context;

			if (_quest.ToolTip != null && _quest.ToolTip.Length > 0)
				_tooltipIcon.gameObject.SetActive(true);
		}
		else if (deviceMode == DeviceMode.Gamepad)
		{
			if (_quest.Context_Xbox == null || _quest.Context_Xbox.Length == 0)
				_contextText = _quest.Context;
			else
				_contextText = _quest.Context_Xbox;

			_tooltipIcon.SetActive(false);
		}
	}

	void OnDestroy()
	{
		Managers.Input.ControlsChangedHandler -= OnControlsChanged;
	}
}
