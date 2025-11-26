using UnityEngine;

public class UI_SkillSlots : UI_Base
{
	[SerializeField]
	UI_SkillSlots_Item _qSkillSlot;
	[SerializeField]
	UI_SkillSlots_Item _eSkillSlot;
	[SerializeField]
	UI_SkillInfo _skillInfoUI;

	PlayerController _player;

	public override void Init()
	{
		_player = Managers.Object.GetPlayer();
		_qSkillSlot.SetSkill(_player.qSkill);
		_eSkillSlot.SetSkill(_player.eSkill);

		Managers.Event.ItemEvents.EquipItemAction += HandleEquipItem;
	}

	public void RefrechUI()
	{
		if (_player == null)
			return;

		_qSkillSlot.SetSkill(_player.qSkill);
		_eSkillSlot.SetSkill(_player.eSkill);
	}

	public void HandleEquipItem(int TemplateId, Define.EquipmentType equipmentType, bool equipped)
	{
		RefrechUI();
	}

	public void ShowSkillInfo(int templateId)
	{
		if (templateId == 0)
		{
			_skillInfoUI.gameObject.SetActive(false);
			return;
		}

		_skillInfoUI.gameObject.SetActive(true);
		_skillInfoUI.SetSkillInfo(templateId);
	}

	public void CloseSkillInfo(int templateId)
	{
		_skillInfoUI.SetSkillInfo(0);
		_skillInfoUI.gameObject.SetActive(false);
	}

	void OnEnable()
	{
		_skillInfoUI.SetSkillInfo(0);
		_skillInfoUI.gameObject.SetActive(false);
	}

	void OnDisable()
	{
		_skillInfoUI.SetSkillInfo(0);
		_skillInfoUI.gameObject.SetActive(false);
	}
}
