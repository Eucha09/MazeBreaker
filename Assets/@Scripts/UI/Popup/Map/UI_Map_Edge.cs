using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Map_Edge : UI_Base
{
    UI_Map _mapUI;
    RectTransform _rectTransform;

    MinimapObject_Edge _obj;
    Vector3 _firstPos;
    Vector3 _secondPos;
    Color _color;

    float _fadeDuration = 0.2f;

    public override void Init()
    {
        _mapUI = GetComponentInParent<UI_Map>();
        _rectTransform = GetComponent<RectTransform>();

        Image image = GetComponent<Image>();
        image.color = _color;

        StartCoroutine(CoFadeOut());
    }

    public void SetInfo(MinimapObject_Edge obj)
    {
        _obj = obj;
        _firstPos = obj.FirstPos;
        _secondPos = obj.SecondPos;
        _color = obj.Color;
    }

    void Update()
    {
        if (_obj == null)
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
    }

    Vector3 WorldToRect(Vector3 worldPos)
    {
        //Vector3 xAxis = Camera.main.transform.right;
        //Vector3 zAxis = Camera.main.transform.forward;
        //zAxis.y = 0;

        //Vector3 transformedPosition = new Vector3(
        //        Vector3.Dot(worldPos, xAxis.normalized),
        //        0,
        //        Vector3.Dot(worldPos, zAxis.normalized)
        //    );
        
        Vector3 newRectLocalPos = Vector3.zero;
        newRectLocalPos.x = _mapUI.PivotPos.x + worldPos.x * _mapUI.Scale;
        newRectLocalPos.y = _mapUI.PivotPos.y + worldPos.z * _mapUI.Scale;
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
