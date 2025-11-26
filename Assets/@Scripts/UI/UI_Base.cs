using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UI_Base : MonoBehaviour
{
	protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();
	public abstract void Init();
	Coroutine _coFade;
	protected bool _isHide;

	private void Start()
	{
		Init();
	}

	protected void Bind<T>(Type type) where T : UnityEngine.Object
	{
		string[] names = Enum.GetNames(type);
		UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
		_objects.Add(typeof(T), objects);

		for (int i = 0; i < names.Length; i++)
		{
			if (typeof(T) == typeof(GameObject))
				objects[i] = Util.FindChild(gameObject, names[i], true);
			else
				objects[i] = Util.FindChild<T>(gameObject, names[i], true);

			if (objects[i] == null)
				Debug.Log($"Failed to bind({names[i]})");
		}
	}

	protected T Get<T>(int idx) where T : UnityEngine.Object
	{
		UnityEngine.Object[] objects = null;
		if (_objects.TryGetValue(typeof(T), out objects) == false)
			return null;

		return objects[idx] as T;
	}

	protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
	protected Text GetText(int idx) { return Get<Text>(idx); }
	protected Button GetButton(int idx) { return Get<Button>(idx); }
	protected Image GetImage(int idx) { return Get<Image>(idx); }
	protected TextMeshProUGUI GetTMP(int idx) { return Get<TextMeshProUGUI>(idx); }

	public static void BindEvent(GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
	{
		UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

		switch (type)
		{
			case Define.UIEvent.Click:
				evt.OnClickHandler -= action;
				evt.OnClickHandler += action;
				break;
			case Define.UIEvent.Drag:
				evt.OnDragHandler -= action;
				evt.OnDragHandler += action;
				break;
			case Define.UIEvent.PointerDown:
				evt.OnPointerDownHandler -= action;
				evt.OnPointerDownHandler += action;
				break;
			case Define.UIEvent.PointerUp:
				evt.OnPointerUpHandler -= action;
				evt.OnPointerUpHandler += action;
				break;
            case Define.UIEvent.PointerEnter:
                evt.OnPointerEnterHandler -= action;
                evt.OnPointerEnterHandler += action;
                break;
            case Define.UIEvent.PointerExit:
                evt.OnPointerExitHandler -= action;
                evt.OnPointerExitHandler += action;
                break;
        }
	}

	public void Show(float fadeDuration, float delay = 0.0f)
	{
		CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
		if (_coFade != null)
			StopCoroutine(_coFade);
		gameObject.SetActive(true);
		_coFade = StartCoroutine(CoFadeCanvas(canvasGroup, 1.0f, fadeDuration, delay));
	}

	public void Hide(float fadeDuration, float delay = 0.0f, bool stopDestroy = false)
	{
		CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
		if (_coFade != null)
			StopCoroutine(_coFade);
		_isHide = true;
		gameObject.SetActive(true);
		_coFade = StartCoroutine(CoFadeCanvas(canvasGroup, 0.0f, fadeDuration, delay, stopDestroy));
	}

	IEnumerator CoFadeCanvas(CanvasGroup canvasGroup, float targetAlpha, float fadeDuration, float delay = 0.0f, bool stopDestroy = false)
	{
		float startAlpha = targetAlpha > 0.5f ? 0.0f : 1.0f;
		float time = 0f;

		// Raycast / Interaction 설정
		canvasGroup.alpha = startAlpha;
		canvasGroup.interactable = targetAlpha > 0.5f;
		canvasGroup.blocksRaycasts = targetAlpha > 0.5f;

		yield return new WaitForSecondsRealtime(delay);
		while (time < fadeDuration)
		{
			time += Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
			yield return null;
		}

		canvasGroup.alpha = targetAlpha;

		if (canvasGroup.alpha < 0.5f)
		{
			if (stopDestroy)
				Managers.Resource.Destroy(gameObject);
			else
				gameObject.SetActive(false);
		}
	}

	public virtual bool IsAnyWindowOpen() { return false; }

	public virtual Vector2 MoveDir { get; set; }
	public virtual float ZoomScale { get; set; }

	public virtual void OnPointerDown(PointerEventData.InputButton inputButton) { }

	public virtual void OnPointerUpAndClick(PointerEventData.InputButton inputButton) { }

	public virtual void OnOkButton() { }

	public virtual void OnCancelButton() { }

	public virtual void OnDeleteButton() { }
}
