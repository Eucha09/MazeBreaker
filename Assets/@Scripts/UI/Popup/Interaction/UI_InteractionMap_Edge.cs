using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UI_InteractionMap_Edge : UI_Base
{
	UI_InteractionMap _mapUI;
	RectTransform _rectTransform;
	Image _image;

	GameObject _target;
	Vector3 _firstPos;
	Vector3 _secondPos;
	Color _color;

	float _fadeDuration = 0.2f;

	StarPiece _star1;
	StarPiece _star2;

	public bool Active { get; set; } = true;

	public override void Init()
	{
		_mapUI = GetComponentInParent<UI_InteractionMap>();
		_rectTransform = GetComponent<RectTransform>();

		_image = GetComponent<Image>();
		_image.color = _color;

		StarEdge starEdge = _target.GetComponent<StarEdge>();
		if (starEdge != null)
		{
			_star1 = starEdge.FirstObject.GetComponent<StarPiece>();
			_star2 = starEdge.SecondObject.GetComponent<StarPiece>();
		}

		StartCoroutine(CoFadeOut());
	}

	public void SetInfo(StarEdge edge)
	{
		_target = edge.gameObject;
		_firstPos = edge.FirstObject.transform.position;
		_secondPos = edge.SecondObject.transform.position;
		_color = edge.Color;
	}

	void Update()
	{
		if (_target == null)
		{
			Managers.Resource.Destroy(gameObject);
			return;
		}

		Vector3 firstRectPos = WorldToRect(_firstPos);
		Vector3 secondRectPos = WorldToRect(_secondPos);
		float scale = _mapUI.Scale / 5.0f;
		_rectTransform.localPosition = firstRectPos;
		_rectTransform.localScale = new Vector2(Vector3.Distance(secondRectPos, firstRectPos), scale);
		_rectTransform.localRotation = Quaternion.Euler(0, 0, AngleInDeg(firstRectPos, secondRectPos));

		if (_mapUI.OnThePath.ContainsKey(_star1) && _mapUI.OnThePath.ContainsKey(_star2))
		{
			_image.color = Color.blue;
		}
		else if (Active)
		{
			_image.color = _color;
		}
		else
		{
			Color color = _color;
			color.a = 0.2f;
			_image.color = color;
		}
	}

	Vector3 WorldToRect(Vector3 worldPos)
	{
		Vector3 xAxis = Camera.main.transform.right;
		Vector3 zAxis = Camera.main.transform.forward;
		zAxis.y = 0;

		Vector3 transformedPosition = new Vector3(
				Vector3.Dot(worldPos, xAxis.normalized),
				0,
				Vector3.Dot(worldPos, zAxis.normalized)
			);

		Vector3 newRectLocalPos = Vector3.zero;
		newRectLocalPos.x = _mapUI.PivotPos.x + transformedPosition.x * _mapUI.Scale;
		newRectLocalPos.y = _mapUI.PivotPos.y + transformedPosition.z * _mapUI.Scale;
		return newRectLocalPos;
	}

	public float AngleInRad(Vector3 vec1, Vector3 vec2)
	{
		return Mathf.Atan2(vec2.y - vec1.y, vec2.x - vec1.x);
	}

	public float AngleInDeg(Vector3 vec1, Vector3 vec2)
	{
		return AngleInRad(vec1, vec2) * 180 / Mathf.PI;
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
