using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_DetailedDescription : UI_Base
{
	enum Images
	{
		Icon,
	}

	enum TextMeshProUGUIs
	{
		TitleText,
		DescriptionText,
	}

	public int TemplateId { get; private set; }

	Vector3 _initAnchPos;
	Coroutine _coShow;
	bool _init = false;

	public override void Init()
	{
		if (_init)
			return;

		Bind<Image>(typeof(Images));
		Bind<TextMeshProUGUI>(typeof(TextMeshProUGUIs));
		_initAnchPos = GetComponent<RectTransform>().anchoredPosition;
		_init = true;
	}

	public void SetItem(int templateId)
	{
		TemplateId = templateId;

		if (_init == false)
			Init();

		Data.ItemData itemData = null;
		Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);
		if (itemData != null)
		{
			Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
			GetImage((int)Images.Icon).sprite = icon;
			GetTMP((int)TextMeshProUGUIs.TitleText).text = itemData.name;
			GetTMP((int)TextMeshProUGUIs.DescriptionText).text = itemData.detailedDescription;
		}

		if (_coShow != null)
			StopCoroutine(_coShow);
		_coShow = StartCoroutine(CoShow(0.2f));
	}

	IEnumerator CoShow(float time)
	{
		Vector3 startPos = _initAnchPos + Vector3.down * 50.0f;
		Vector3 endPos = _initAnchPos;

		for (float i = 0.0f; i < time; i += Time.deltaTime)
		{
			GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(startPos, endPos, i / time);
			yield return null;
		}

		GetComponent<RectTransform>().anchoredPosition = endPos;

		_coShow = null;
	}
}
