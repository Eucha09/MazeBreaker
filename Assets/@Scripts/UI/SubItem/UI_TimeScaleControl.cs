using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_TimeScaleControl : UI_Base
{
	[SerializeField]
	GameObject _timeScaleControlButton;
	[SerializeField]
	Text _timeScaleText;

	int _idx;
	float[] _timeScales = { 1.0f, 4.0f, 16.0f};

	public override void Init()
	{
		_timeScaleControlButton.BindEvent(OnClickButton);
	}

	public void OnClickButton(PointerEventData data)
	{
		_idx = (_idx + 1) % _timeScales.Length;
		_timeScaleText.text = "x" + (int)_timeScales[_idx];
		Time.timeScale = _timeScales[_idx];
	}

	public void Show()
	{
		if (gameObject.activeSelf)
			return;

		_idx = 0;
		_timeScaleText.text = "x" + (int)_timeScales[_idx];
		Time.timeScale = _timeScales[_idx];
		gameObject.SetActive(true);
	}

	public void Close()
	{
		if (!gameObject.activeSelf)
			return;

		_idx = 0;
		_timeScaleText.text = "x" + (int)_timeScales[_idx];
		Time.timeScale = _timeScales[_idx];
		gameObject.SetActive(false);
	}
}
