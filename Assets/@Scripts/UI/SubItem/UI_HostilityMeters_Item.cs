using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_HostilityMeters_Item : UI_Base
{
	enum Images
	{
		HostilityMeter_Fill,
	}

	Image _fillImage;
	DetectorType _detectorType;
	float _hostilityRatio = 0f;
	float _lerpSpeed = 1f;

	bool _isFlashing = false;
	Color _defaultColor;
	Color _flashColor = new Color(1.0f, 0.2f, 0.2f, 1.0f);

	public override void Init()
	{
		Bind<Image>(typeof(Images));
		_fillImage = GetImage((int)Images.HostilityMeter_Fill);
		_fillImage.fillAmount = 0.0f;
		_defaultColor = _fillImage.color;
	}

	void Update()
	{
		float diff = _hostilityRatio - _fillImage.fillAmount;
		if (Mathf.Abs(diff) > _lerpSpeed * Time.deltaTime)
		{
			diff /= Mathf.Abs(diff);
			_fillImage.fillAmount += diff * Time.deltaTime * _lerpSpeed;
		}
		else
		{
			_fillImage.fillAmount = _hostilityRatio;
			if (_fillImage.fillAmount <= 0.0f || _fillImage.fillAmount >= 1.0f)
			{
				gameObject.SetActive(false);
			}
		}
	}

	public void SetHostilityMeter(HostilityMeter hostilityMeter)
	{
		_detectorType = hostilityMeter.Type;
		_hostilityRatio = hostilityMeter.Value / hostilityMeter.MaxValue;

		if (_hostilityRatio > 0.01f && gameObject.activeSelf == false)
		{
			Debug.Log(_hostilityRatio);
			Show(0.2f);
		}

		// Test
		//if (_hostilityRatio >= 1f && !_isFlashing)
		//	StartCoroutine(FlashRoutine());
	}

	IEnumerator FlashRoutine()
	{
		_isFlashing = true;
		float flashTime = 1.5f;
		float elapsed = 0f;

		while (elapsed < flashTime)
		{
			float t = Mathf.PingPong(elapsed * 5f, 1f); // 깜박 속도 조절
			_fillImage.color = Color.Lerp(_defaultColor, _flashColor, t);
			elapsed += Time.deltaTime;
			yield return null;
		}

		_fillImage.color = _defaultColor;
		_isFlashing = false;
	}
}
