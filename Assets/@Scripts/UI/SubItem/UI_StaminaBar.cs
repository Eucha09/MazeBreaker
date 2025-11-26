using UnityEngine;
using UnityEngine.UI;

public class UI_StaminaBar : UI_Base
{
    enum Images
    {
        StaminaBar,
    }

    enum Texts
    {
        StaminaBar_Text,
    }

    BaseController _player;

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));

        _player = Managers.Object.GetPlayer();
    }

    public void RefreshUI()
    {
        float stamina_ratio = _player.Stamina / _player.MaxStamina;
        GetImage((int)Images.StaminaBar).fillAmount = stamina_ratio;
        GetText((int)Texts.StaminaBar_Text).text = _player.Stamina.ToString("0") + " / " + _player.MaxStamina.ToString("0");
    }
}
