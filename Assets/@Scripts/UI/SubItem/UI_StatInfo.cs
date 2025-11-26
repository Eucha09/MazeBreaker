using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatInfo : UI_Base
{
	[SerializeField]
	TextMeshProUGUI _statName = null;

	[SerializeField]
	TextMeshProUGUI _statDescription = null;

	public int TemplateId { get; private set; }

	public override void Init()
	{

	}

	public void SetStatInfo(int templateId)
	{
		if (templateId == 0)
			return;

		TemplateId = templateId;

		Data.StatDescriptionData statData = null;
		Managers.Data.StatDict.TryGetValue(templateId, out statData);

		_statName.text = statData.name;
		_statDescription.text = statData.description;
	}
}