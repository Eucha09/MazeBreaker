using UnityEngine;
using UnityEngine.UI;

public class UI_MinimapIndicator : UI_Base
{
    enum Images
    {
        ArrowImage,
        IconImage,
    }

    enum GameObjects
    { 
        Arrow,
    }

    Sprite _sprite;
    Color _color;

    Transform _target;

    public void SetIndicator(Transform target, Sprite sprite, Color color)
    {
        _target = target;
        _sprite = sprite;
        _color = color;
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        GetImage((int)Images.IconImage).sprite = _sprite;
        GetImage((int)Images.IconImage).color = _color;
        GetImage((int)Images.ArrowImage).color = _color;
    }

    void Update()
    {
        Vector3 dir = _target.position - Managers.Object.GetPlayer().transform.position;
        dir.y = 0;
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0;

        if (dir.magnitude < 45.0f)
        {
            GetImage((int)Images.IconImage).enabled = false;
            GetImage((int)Images.ArrowImage).enabled = false;
            return;
        }
        else
        {
            GetImage((int)Images.IconImage).enabled = true;
            GetImage((int)Images.ArrowImage).enabled = true;

            float angle = Vector3.SignedAngle(dir, cameraForward, Vector3.up);

            GetObject((int)GameObjects.Arrow).transform.rotation = Quaternion.Euler(0, 0, angle);

            Vector3 adjustedDirection = Quaternion.Euler(0, 0, angle) * Vector3.up;
            Vector3 indicatorPosition = adjustedDirection.normalized * 90;
            GetComponent<RectTransform>().anchoredPosition = indicatorPosition;
        }
    }
}
