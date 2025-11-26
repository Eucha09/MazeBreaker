using UnityEngine;
using UnityEngine.UI;

public class UI_SpecialGauge : UI_Base
{
    enum GameObjects
    {
        SpecialGauge,
		SpecialGauge1_BG,
		SpecialGauge2_BG,
		SpecialGauge3_BG,
		SpecialGauge4_BG,
		SpecialGauge5_BG,
		SpecialGauge1_Full,
        SpecialGauge2_Full,
        SpecialGauge3_Full,
        SpecialGauge4_Full,
        SpecialGauge5_Full,
    }

    PlayerController _player;

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));

        _player = Managers.Object.GetPlayer();
    }

    void Update()
    {
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;

        Vector3 cameraEulerAngles = Camera.main.transform.rotation.eulerAngles;
        Vector3 eulerAngles = new Vector3(90, cameraEulerAngles.y, cameraEulerAngles.z);

        transform.rotation = Quaternion.Euler(eulerAngles);

        for (int i = 0; i < 5; i++)
            GetObject((int)GameObjects.SpecialGauge1_BG + i).SetActive(i < _player.MaxStaminaCount);

		for (int i = 0; i < 5; i++)
			GetObject((int)GameObjects.SpecialGauge1_Full + i).SetActive(i < _player.CurrentStaminaCount);

        GetObject((int)GameObjects.SpecialGauge).transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 12.5f * (5 - _player.MaxStaminaCount));
    }
}
