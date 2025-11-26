using Data;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_TitleScene : UI_Scene
{
	enum Buttons
	{
		GameStartButton,
	}

	[SerializeField]
	RectTransform _panel;
	[SerializeField]
	float _slideDistance = 500.0f;
	[SerializeField]
	Image _fadeImage;

	Vector2 _originalPos;
	Coroutine _coSlide;

	public override void Init()
	{
		base.Init();

		Bind<Button>(typeof(Buttons));

		GetButton((int)Buttons.GameStartButton).gameObject.BindEvent(OnClickGameStartButton);

		_panel.gameObject.SetActive(false);
		_originalPos = _panel.anchoredPosition;

		_fadeImage.gameObject.SetActive(true);
		Color color = _fadeImage.color;
		color.a = 1.0f;
		_fadeImage.color = color;
	}

	public void OnClickGameStartButton(PointerEventData data)
	{
		StartCoroutine(CoSceneTransition(Define.Scene.Prologue1));
	}

	public IEnumerator CoFadeIn(float fadeDuration)
	{
		_fadeImage.gameObject.SetActive(true);
		Color color = _fadeImage.color;
		color.a = 1.0f;
		_fadeImage.color = color;

		for (float i = 0.0f; i < fadeDuration; i += Time.deltaTime)
		{
			color.a = 1.0f - Mathf.Clamp01(i / fadeDuration);
			_fadeImage.color = color;
			yield return null;
		}

		_fadeImage.gameObject.SetActive(false);
	}

	public IEnumerator CoFadeOut(float fadeDuration)
	{
		_fadeImage.gameObject.SetActive(true);
		Color color = _fadeImage.color;
		color.a = 0.0f;
		_fadeImage.color = color;

		for (float i = 0.0f; i < fadeDuration; i += Time.deltaTime)
		{
			color.a = Mathf.Clamp01(i / fadeDuration);
			_fadeImage.color = color;
			yield return null;
		}

		color.a = 1.0f;
		_fadeImage.color = color;
	}

	public IEnumerator CoShow(float duration)
	{
		Vector2 startPos = _originalPos + new Vector2(_slideDistance, 0.0f);
		Vector2 endPos = _originalPos;
		float startAlpha = 0.0f;
		float endAlpha = 1.0f;

		CanvasGroup canvasGroup = _panel.GetOrAddComponent<CanvasGroup>();
		_panel.anchoredPosition = startPos;
		canvasGroup.alpha = startAlpha;
		canvasGroup.interactable = endAlpha > 0.5f;
		canvasGroup.blocksRaycasts = endAlpha > 0.5f;
		_panel.gameObject.SetActive(true);

		for (float i = 0.0f; i < duration; i += Time.deltaTime)
		{
			float t = i / duration;
			_panel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
			canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
			yield return null;
		}


		_panel.anchoredPosition = endPos;
		canvasGroup.alpha = endAlpha;
	}

	public IEnumerator CoSceneTransition(Define.Scene nextScene)
	{
		yield return StartCoroutine(CoFadeOut(1.0f));

		Managers.Scene.LoadScene(Define.Scene.Prologue1);
	}

	#region Gamepad
	public override void OnOkButton()
	{
		if (_panel.gameObject.activeSelf)
			OnClickGameStartButton(new PointerEventData(EventSystem.current));
	}
	#endregion
}
