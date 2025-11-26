using Data;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UI_TargetBar : UI_Base
{
    enum Images
    {
        TargetHpBar,
    }

    enum Texts
    {
        TargetName_Text,
        TargetHpBar_Text,
    }

    enum GameObjects
    { 
        TargetBar,
    }

    BaseController _target;

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        GetObject((int)GameObjects.TargetBar).gameObject.SetActive(false);

	}

    public void SetInfo(BaseController target)
    {
        _target = target;

        ObjectData objectData = null;
        Managers.Data.ObjectDict.TryGetValue(target.TemplateId, out objectData);
        if (objectData != null)
        {
            GetText((int)Texts.TargetName_Text).text = objectData.name;
        }
    }

    void Update()
    {
        if (_target == null)
            return;

        PlayerController player = Managers.Object.GetPlayer();
        Vector3 dir = _target.transform.position - player.transform.position;
        if (_target.Hp <= 0 || dir.magnitude >= 36.0f)
        {
            GetObject((int)GameObjects.TargetBar).gameObject.SetActive(false);
            _target = null;
            return;
        }

        GetObject((int)GameObjects.TargetBar).gameObject.SetActive(true);

        float hp_ratio = _target.Hp / _target.MaxHp;
        GetImage((int)Images.TargetHpBar).fillAmount = hp_ratio;
        GetText((int)Texts.TargetHpBar_Text).text = _target.Hp.ToString("0") + " / " + _target.MaxHp.ToString("0");
    }
}
