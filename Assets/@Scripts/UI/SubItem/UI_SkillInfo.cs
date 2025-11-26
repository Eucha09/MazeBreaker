using UnityEngine;
using UnityEngine.UI;

public class UI_SkillInfo : UI_Base
{
	[SerializeField]
	Image _skillIcon = null;

	[SerializeField]
	Text _skillName = null;

	[SerializeField]
	Text _skillDescription = null;

	public int TemplateId { get; private set; }

	public override void Init()
	{

	}

	public void SetSkillInfo(int templateId)
	{
		if (templateId == 0)
			return;

		TemplateId = templateId;

		Data.SkillData skillData = null;
		Managers.Data.SkillDict.TryGetValue(templateId, out skillData);

		Sprite icon = Managers.Resource.Load<Sprite>(skillData.iconPath);
		_skillIcon.sprite = icon;
		_skillName.text = skillData.name;
		_skillDescription.text = skillData.description;
	}
}
