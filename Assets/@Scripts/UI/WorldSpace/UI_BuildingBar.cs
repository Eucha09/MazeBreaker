using UnityEngine;
using UnityEngine.UI;

public class UI_BuildingBar : UI_Base
{
	enum Images
	{
		BuildingBar_BG,
		BuildingBar_Frame,
		BuildingBar_Full,
	}

	enum GameObjects
	{
		BuildingBar,
	}

	PreviewObject _target;

	public override void Init()
	{
		Bind<Image>(typeof(Images));
		Bind<GameObject>(typeof(GameObjects));
	}

	public void SetInfo(PreviewObject target, float height)
	{
		_target = target;
		transform.localPosition = Vector3.up * height;
	}

	void Update()
	{
		if (_target == null)
			return;

		if (_target.ProgressRatio <= 0 || _target.ProgressRatio >= 1.0f)
		{
			GetObject((int)GameObjects.BuildingBar).gameObject.SetActive(false);
			return;
		}

		GetObject((int)GameObjects.BuildingBar).gameObject.SetActive(true);

		GetImage((int)Images.BuildingBar_Full).fillAmount = _target.ProgressRatio;

		transform.rotation = Camera.main.transform.rotation;
	}
}
