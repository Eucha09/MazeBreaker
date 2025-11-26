using Data;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UI_ItemCollect_Item : UI_Base
{
	[SerializeField]
	Image[] _bgImages;

	[SerializeField]
	Image _icon = null;

	[SerializeField]
	Text _nameText = null;

	public int TemplateId { get; private set; }

	Vector3 _initAnchPos;
	Coroutine _coShow;

	public override void Init()
	{

	}

	public void SetItem(int templateId, int count)
	{
		TemplateId = templateId;

		Data.ItemData itemData = null;
		Managers.Data.ItemDict.TryGetValue(templateId, out itemData);

		Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
		_icon.sprite = icon;
		_nameText.text = itemData.name + " x" + count;

		_initAnchPos = _bgImages[0].GetComponent<RectTransform>().anchoredPosition;
		_coShow = StartCoroutine(CoShow(0.2f));
	}

	public void CloseUI()
	{
		if (_coShow != null)
			StopCoroutine(_coShow);
		StartCoroutine(CoClose(0.2f));
	}

	IEnumerator CoShow(float time)
	{
		Vector3 startPos = _initAnchPos + Vector3.right * 50.0f;
		Vector3 endPos = _initAnchPos;

		for (float i = 0.0f; i < time; i += Time.deltaTime)
		{
			foreach (Image bg in _bgImages)
			{
				SetAlpha(bg, i / time);
				bg.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(startPos, endPos, i / time);
			}
			SetAlpha(_icon, i / time);
			SetAlpha(_nameText, i / time);
			yield return null;
		}

		foreach (Image bg in _bgImages)
		{
			SetAlpha(bg, 1.0f);
			bg.GetComponent<RectTransform>().anchoredPosition = endPos;
		}
		SetAlpha(_icon, 1.0f);
		SetAlpha(_nameText, 1.0f);
		_coShow = null;
	}

	IEnumerator CoClose(float time)
	{
		Vector3 startPos = _initAnchPos + Vector3.right * 50.0f;
		Vector3 endPos = _initAnchPos;

		for (float i = 0.0f; i < time; i += Time.deltaTime)
		{
			foreach (Image bg in _bgImages)
			{
				SetAlpha(bg, 1.0f - i / time);
				bg.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(startPos, endPos, 1.0f - i / time);
			}
			SetAlpha(_icon, 1.0f - i / time);
			SetAlpha(_nameText, 1.0f - i / time);
			yield return null;
		}

		Managers.Resource.Destroy(gameObject);
	}

	void SetAlpha(Image image, float alpha)
	{
		Color color = image.color;
		color.a = alpha;
		image.color = color;
	}

	void SetAlpha(Text text, float alpha)
	{
		Color color = text.color;
		color.a = alpha;
		text.color = color;
	}
}
