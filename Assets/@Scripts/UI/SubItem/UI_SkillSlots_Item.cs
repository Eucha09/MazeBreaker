using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_SkillSlots_Item : UI_Base
{
	[SerializeField]
	Image _icon = null;

	[SerializeField]
	Text _keyGuideText = null;

	[SerializeField]
	GameObject _costIcons = null;

	public int TemplateId { get; private set; }
	public int CostCount { get; private set; }

	UI_SkillSlots _skillSlotsUI;

	public override void Init()
	{
		gameObject.BindEvent(OnPointerEnter, UIEvent.PointerEnter);
		gameObject.BindEvent(OnPointerExit, UIEvent.PointerExit);
		_skillSlotsUI = GetComponentInParent<UI_SkillSlots>();
	}

	public void SetSkill(SkillInfo2 skillInfo)
	{
		if (skillInfo == null)
		{
			TemplateId = 0;

			gameObject.SetActive(false);
		}
		else
		{
			TemplateId = skillInfo.TemplateId;
			CostCount = skillInfo.StaminaConsumptionCount;

			SkillData skillData = null;
			Managers.Data.SkillDict.TryGetValue(TemplateId, out skillData);

			Sprite icon = Managers.Resource.Load<Sprite>(skillData.iconPath);
			_icon.sprite = icon;

			int i = 0;
			foreach (Transform child in _costIcons.transform)
			{
				child.gameObject.SetActive(i < CostCount);
				i++;
			}

			gameObject.SetActive(true);
		}
	}

	public void OnPointerEnter(PointerEventData data)
	{
		_skillSlotsUI.ShowSkillInfo(TemplateId);
	}

	public void OnPointerExit(PointerEventData data)
	{
		_skillSlotsUI.CloseSkillInfo(TemplateId);
	}
}
