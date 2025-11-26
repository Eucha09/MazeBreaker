using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerBar : UI_Base
{
    enum Images
    {
        HpBar_Fill,
        //MentalBar_Fill,
        HungerBar_Fill,
    }

    enum Texts
    {
        HpBar_Text,
        //MentalBar_Text,
        HungerBar_Text,
    }

    enum GameObjects
    {
        HpBar,
        HungerBar,
    }

    [SerializeField]
    UI_StatInfo _statInfo;

    BaseController _player;

	public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
		Bind<GameObject>(typeof(GameObjects));

        GetObject((int)GameObjects.HpBar).BindEvent((data) => { _statInfo.gameObject.SetActive(true); _statInfo.SetStatInfo(1); }, Define.UIEvent.PointerEnter);
		GetObject((int)GameObjects.HpBar).BindEvent((data) => { _statInfo.gameObject.SetActive(false); }, Define.UIEvent.PointerExit);
		GetObject((int)GameObjects.HungerBar).BindEvent((data) => { _statInfo.gameObject.SetActive(true); _statInfo.SetStatInfo(2); }, Define.UIEvent.PointerEnter);
		GetObject((int)GameObjects.HungerBar).BindEvent((data) => { _statInfo.gameObject.SetActive(false); }, Define.UIEvent.PointerExit);

        _statInfo.gameObject.SetActive(false);
	}

    public void RefreshUI()
	{
		_player = Managers.Object.GetPlayer();
		float hp_ratio = _player.Hp / _player.MaxHp;
        GetImage((int)Images.HpBar_Fill).fillAmount = hp_ratio;
        GetText((int)Texts.HpBar_Text).text = _player.Hp.ToString("0") + "/" + _player.MaxHp.ToString("0");
        //float mental_ratio = _player.Mental / _player.MaxMental;
        //GetText((int)Texts.MentalBar_Text).text = _player.Mental.ToString("0") + "/" + _player.MaxMental.ToString("0");
        //GetImage((int)Images.MentalBar_Fill).fillAmount = mental_ratio;
        if (_player.MaxHunger > 0.0f)
        {
            float hunger_ratio = _player.Hunger / _player.MaxHunger;
            GetText((int)Texts.HungerBar_Text).text = _player.Hunger.ToString("0") + "/" + _player.MaxHunger.ToString("0");
            GetImage((int)Images.HungerBar_Fill).fillAmount = hunger_ratio;
        }
	}
}
