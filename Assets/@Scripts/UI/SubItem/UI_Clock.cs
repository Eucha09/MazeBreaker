using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Clock : UI_Base
{
    enum Images
    { 
        Day_BG,
        Day_Icon,
        Day_Bar_BG,
        Day_Bar_Full,
        Day_Pin,
        Night_BG,
        Night_Icon,
        Night_Bar_BG,
        Night_Bar_Full,
        Night_Pin,
    }

    float _dayRatio;
    float _nightRatio;

    float _radius = 60.0f;
    bool _isNight = false;

    Coroutine _coMoveIcon;

	public override void Init()
    {
        Bind<Image>(typeof(Images));

        _dayRatio = Managers.Time.DayRatio;
        _nightRatio = Managers.Time.NightRatio;
        GetImage((int)Images.Day_Bar_BG).fillAmount = _dayRatio;
        GetImage((int)Images.Night_Bar_BG).fillAmount = _nightRatio;
        GetImage((int)Images.Night_Bar_BG).transform.rotation = Quaternion.Euler(0.0f, 0.0f, 360.0f * _nightRatio);
        GetImage((int)Images.Night_Bar_Full).transform.rotation = Quaternion.Euler(0.0f, 0.0f, 360.0f * _nightRatio);
        Vector3 adjustedDirection = Quaternion.Euler(0, 0, 360.0f * _nightRatio) * Vector3.up;
        Vector3 indicatorPosition = adjustedDirection.normalized * 55;
        GetImage((int)Images.Night_Pin).GetComponent<RectTransform>().anchoredPosition = indicatorPosition;

        _isNight = Managers.Time.IsNight;
        RefreshUI();
	}

    void Update()
    {
        if (Managers.Time.IsNight == true && _isNight == false)
        {
            _isNight = true;
            RectTransform dayIcon = GetImage((int)Images.Day_Icon).GetComponent<RectTransform>();
            RectTransform nightIcon = GetImage((int)Images.Night_Icon).GetComponent<RectTransform>();
            StartCoroutine(CoMoveIcons(dayIcon, nightIcon, GetImage((int)Images.Day_BG), 0.0f, 5.0f));
            StartCoroutine(CoTransitionImages(GetImage((int)Images.Day_Bar_BG), GetImage((int)Images.Night_Bar_BG), 1.0f));
            StartCoroutine(CoTransitionImages(GetImage((int)Images.Day_Bar_Full), GetImage((int)Images.Night_Bar_Full), 1.0f));
            StartCoroutine(CoTransitionImages(GetImage((int)Images.Night_Pin), GetImage((int)Images.Day_Pin), 1.0f));

        }
        else if (Managers.Time.IsNight == false && _isNight == true)
        {
            _isNight = false;
            RectTransform dayIcon = GetImage((int)Images.Day_Icon).GetComponent<RectTransform>();
            RectTransform nightIcon = GetImage((int)Images.Night_Icon).GetComponent<RectTransform>();
            StartCoroutine(CoMoveIcons(nightIcon, dayIcon, GetImage((int)Images.Day_BG), 1.0f, 5.0f));
            StartCoroutine(CoTransitionImages(GetImage((int)Images.Night_Bar_BG), GetImage((int)Images.Day_Bar_BG), 1.0f));
            StartCoroutine(CoTransitionImages(GetImage((int)Images.Night_Bar_Full), GetImage((int)Images.Day_Bar_Full), 1.0f));
            StartCoroutine(CoTransitionImages(GetImage((int)Images.Day_Pin), GetImage((int)Images.Night_Pin), 1.0f));
        }
        else if (_isNight)
        {
            float ratio = Managers.Time.CurTimeRatio - _dayRatio;
            GetImage((int)Images.Night_Bar_Full).fillAmount = ratio;
        }
        else if (!_isNight)
        {
            float ratio = Managers.Time.CurTimeRatio;
            GetImage((int)Images.Day_Bar_Full).fillAmount = ratio;
        }
    }

    void RefreshUI()
    {
        float angle90 = 90.0f * Mathf.Deg2Rad;
        float angle180 = 180.0f * Mathf.Deg2Rad;
        Vector2 pos90 = new Vector2(Mathf.Cos(angle90), Mathf.Sin(angle90)) * _radius + new Vector2(0.0f, -_radius);
        Vector2 pos180 = new Vector2(Mathf.Cos(angle180), Mathf.Sin(angle180)) * _radius + new Vector2(0.0f, -_radius);

        if (!_isNight)
        {
            GetImage((int)Images.Day_Icon).GetComponent<RectTransform>().anchoredPosition = pos90;
            GetImage((int)Images.Night_Icon).GetComponent<RectTransform>().anchoredPosition = pos180;
            SetAlpha(GetImage((int)Images.Day_BG), 1.0f);

            SetAlpha(GetImage((int)Images.Day_Bar_BG), 1.0f);
            SetAlpha(GetImage((int)Images.Day_Bar_Full), 1.0f);
            SetAlpha(GetImage((int)Images.Night_Pin), 1.0f);
            SetAlpha(GetImage((int)Images.Night_Bar_BG), 0.0f);
            SetAlpha(GetImage((int)Images.Night_Bar_Full), 0.0f);
            SetAlpha(GetImage((int)Images.Day_Pin), 0.0f);
        }
        else
        {
            GetImage((int)Images.Day_Icon).GetComponent<RectTransform>().anchoredPosition = pos180;
            GetImage((int)Images.Night_Icon).GetComponent<RectTransform>().anchoredPosition = pos90;
            SetAlpha(GetImage((int)Images.Day_BG), 0.0f);

            SetAlpha(GetImage((int)Images.Day_Bar_BG), 0.0f);
            SetAlpha(GetImage((int)Images.Day_Bar_Full), 0.0f);
            SetAlpha(GetImage((int)Images.Night_Pin), 0.0f);
            SetAlpha(GetImage((int)Images.Night_Bar_BG), 1.0f);
            SetAlpha(GetImage((int)Images.Night_Bar_Full), 1.0f);
            SetAlpha(GetImage((int)Images.Day_Pin), 1.0f);
        }
    }

    void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    IEnumerator CoMoveIcons(RectTransform fromIcon, RectTransform toIcon, Image bgImage, float targetAlpha, float duration)
    {
        float elapsedTime = 0f;
        float startAlpha = targetAlpha > 0.0f ? 0.0f : 1.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            float angle1 = Mathf.Lerp(90.0f, 0.0f, t) * Mathf.Deg2Rad;
            float angle2 = Mathf.Lerp(180.0f, 90.0f, t) * Mathf.Deg2Rad;

            Vector2 newPosition1 = new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * _radius + new Vector2(0.0f, -_radius);
            Vector2 newPosition2 = new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * _radius + new Vector2(0.0f, -_radius);

            fromIcon.anchoredPosition = newPosition1;
            toIcon.anchoredPosition = newPosition2;

            SetAlpha(bgImage, Mathf.Lerp(startAlpha, targetAlpha, t));

            yield return null;
        }
        RefreshUI();
    }

    IEnumerator CoTransitionImages(Image fromImage, Image toImage, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            SetAlpha(fromImage, Mathf.Lerp(1f, 0f, t));
            SetAlpha(toImage, Mathf.Lerp(0f, 1f, t));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
	}
}
