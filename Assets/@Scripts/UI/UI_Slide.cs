using System.Collections;
using UnityEngine;

public class UI_Slide : MonoBehaviour
{
	[SerializeField]
	Vector2 _offScreenPostion;
	[SerializeField]
	float _moveDuration = 0.5f;
	[SerializeField]
	bool _hideOnStart;

	Vector2 _initPos;
	Vector2 _hidePos;
	Coroutine _coMove;

	void Start()
	{
		_initPos = gameObject.GetComponent<RectTransform>().anchoredPosition;
		_hidePos = _initPos + _offScreenPostion;

		if (_hideOnStart)
			gameObject.GetComponent<RectTransform>().anchoredPosition = _hidePos;
	}

	public void HideUI()
	{
		if (gameObject.activeInHierarchy == false)
			return;
		if (_coMove != null)
		{
			StopCoroutine(_coMove);
			_coMove = null;
		}

		RectTransform element = gameObject.GetComponent<RectTransform>();
		_coMove = StartCoroutine(CoMoveUI(element, _hidePos));
	}

	public void ShowUI()
	{
		if (gameObject.activeInHierarchy == false)
			return;
		if (_coMove != null)
		{
			StopCoroutine(_coMove);
			_coMove = null;
		}

		RectTransform element = gameObject.GetComponent<RectTransform>();
		_coMove = StartCoroutine(CoMoveUI(element, _initPos));
	}

	private IEnumerator CoMoveUI(RectTransform element, Vector2 targetPosition)
	{
		Vector2 startPosition = element.anchoredPosition;
		float elapsedTime = 0f;

		while (elapsedTime < _moveDuration)
		{
			element.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, (elapsedTime / _moveDuration));
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		element.anchoredPosition = targetPosition;
		_coMove = null;
	}
}
