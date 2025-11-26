using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static Unity.Cinemachine.CinemachineTargetGroup;
using static UnityEngine.GraphicsBuffer;

public class UI_InteractionDir : UI_Base
{
	UI_InteractionMap _mapUI;
	RectTransform _rectTransform;

	Vector3 _firstPos;
	Vector3 _secondPos;
	GameObject _target;

	float _fadeDuration = 0.2f;

	public override void Init()
	{
		_mapUI = GetComponentInParent<UI_InteractionMap>();
		_rectTransform = GetComponent<RectTransform>();

		_firstPos = Vector3.zero;

		gameObject.SetActive(false);
		//StartCoroutine(CoFadeOut());
	}

	void Update()
	{
		
		if (Input.GetMouseButtonDown(0))
		{
			Vector3 dir = GetWorldDirection(_secondPos - _firstPos);
			if (Vector3.Angle(dir, _target.transform.forward) < Vector3.Angle(dir, -_target.transform.forward))
				_mapUI.SelectDirection(_target.transform.forward);
			else
				_mapUI.SelectDirection(-_target.transform.forward);
		}


		_secondPos = (Vector3)_mapUI.ScreenPointToRectPoint(Input.mousePosition);

		_rectTransform.localPosition = _firstPos;
		_rectTransform.sizeDelta = new Vector2(Vector3.Distance(_secondPos, _firstPos), 23.0f);
		_rectTransform.localRotation = Quaternion.Euler(0, 0, AngleInDeg(_firstPos, _secondPos));

	}

	public void SetTarget(GameObject target)
	{
		_target = target;
	}

	public float AngleInRad(Vector3 vec1, Vector3 vec2)
	{
		return Mathf.Atan2(vec2.y - vec1.y, vec2.x - vec1.x);
	}

	public float AngleInDeg(Vector3 vec1, Vector3 vec2)
	{
		return AngleInRad(vec1, vec2) * 180 / Mathf.PI;
	}

	Vector3 GetWorldDirection(Vector3 screenDirection)
	{
		Vector3 cameraPosition = Camera.main.transform.position;
		Vector3 forward = Camera.main.transform.up;
		Vector3 right = Camera.main.transform.right;

		Vector3 worldDirection = (right * screenDirection.x + forward * screenDirection.y).normalized;

		return worldDirection;
	}

	IEnumerator CoFadeOut()
	{
		float elapsedTime = 0f;
		Image fadeImage = GetComponent<Image>();
		fadeImage.gameObject.SetActive(true);
		Color color = fadeImage.color;

		while (elapsedTime < _fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			color.a = Mathf.Clamp01(elapsedTime / _fadeDuration * 0.95f);
			fadeImage.color = color;
			yield return null;
		}
	}
}
