using UnityEngine;
using UnityEngine.UI;

public class UI_HpBar : UI_Base
{
    enum Images
    {
        HpBar_BG,
        HpBar_Frame,
        HpBar_Full,
    }

    enum GameObjects
    { 
        HpBar,
    }

    BaseController _target;

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));
    }

    public void SetInfo(BaseController target, float height)
    {
        _target = target;
        transform.localPosition = Vector3.up * height;
    }

    void Update()
    {
        if (_target == null)
            return;

        PlayerController player = Managers.Object.GetPlayer();
        Vector3 dir = _target.transform.position - player.transform.position;
        if (_target.Hp <= 0 || _target.Hp == _target.MaxHp || dir.magnitude >= 36.0f)
        {
            GetObject((int)GameObjects.HpBar).gameObject.SetActive(false);
            return;
        }

        GetObject((int)GameObjects.HpBar).gameObject.SetActive(true);

        float hp_ratio = _target.Hp / _target.MaxHp;
        GetImage((int)Images.HpBar_Full).fillAmount = hp_ratio;

        transform.rotation = Camera.main.transform.rotation;
    }
}
