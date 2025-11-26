using Data;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Refinement_Item : UI_Base
{
	[SerializeField]
	Image _toggleImage = null;

	[SerializeField]
	GameObject _inputItems = null;

	[SerializeField]
	GameObject _outputItems = null;

	[SerializeField]
	Image _progressBar;

	public int RefinementId { get; private set; }
	public List<RequiredItem> RequiredItem { get; private set; } = new List<RequiredItem>();

	UI_Refinement _refinementUI;

	public override void Init()
	{
		gameObject.BindEvent(OnClick);
		_refinementUI = GetComponentInParent<UI_Refinement>(true);


	}

	void Update()
	{
		if (_refinementUI.RefinementSystem.CurRefinementId == RefinementId)
			_progressBar.fillAmount = _refinementUI.RefinementSystem.ProgressRatio;
		else
			_progressBar.fillAmount = 0.0f;

		if (_refinementUI.RefinementSystem.NextRefinementId == RefinementId)
			_toggleImage.gameObject.SetActive(true);
		else
			_toggleImage.gameObject.SetActive(false);
	}

	public void SetItem(int refinementId)
	{
		if (refinementId == 0)
		{
			RefinementId = 0;
			return;
		}

		RefinementId = refinementId;

		RefinementData refinementData = null;
		Managers.Data.RefinementDict.TryGetValue(RefinementId, out refinementData);

		foreach (Transform child in _inputItems.transform)
			Destroy(child.gameObject);
		foreach (RequiredItem requiredItem in refinementData.inputItems)
			Managers.UI.MakeSubItem<UI_Required_Item2>(_inputItems.transform).SetItem(requiredItem);

		foreach (Transform child in _outputItems.transform)
			Destroy(child.gameObject);
		foreach (RequiredItem requiredItem in refinementData.outputItems)
			Managers.UI.MakeSubItem<UI_Required_Item2>(_outputItems.transform).SetItem(requiredItem);
	}

	public void OnClick(PointerEventData data)
	{
		if (RefinementId == 0)
			return;

		if (data.button == PointerEventData.InputButton.Left)
		{
			if (_refinementUI.RefinementSystem.ReserveCrafting(RefinementId))
				Managers.Sound.Play("UISOUND/CraftSucceed");
			else
				Managers.Sound.Play("UISOUND/CraftFail");
		}
		if (data.button == PointerEventData.InputButton.Right)
			_refinementUI.RefinementSystem.Cancel(RefinementId);
	}
}
