using System.Collections;
using UnityEngine;

public class TitleScene : BaseScene
{
	[SerializeField]
	Texture _skybox;
	[SerializeField]
	Light _globalLight;

	[SerializeField]
	GameObject _camera1;
	[SerializeField]
	GameObject _camera2;

	UI_TitleScene _sceneUI;

	protected override void Init()
	{
		base.Init();

		SceneType = Define.Scene.Title;
		Screen.SetResolution(1920, 1080, true);
		//Debug.developerConsoleVisible = false;

		RenderSettings.skybox.SetTexture("_Texture1", _skybox);
		RenderSettings.skybox.SetFloat("_Blend", 0);
		_globalLight.color = Color.white;
		RenderSettings.fogColor = _globalLight.color;
		RenderSettings.fogEndDistance = 300.0f;

		_sceneUI = Managers.UI.ShowSceneUI<UI_TitleScene>();

		StartCoroutine(CoStart());

		Managers.Sound.Play("BGM/DARK FOREST BGM", Define.Sound.Bgm);
	}

	IEnumerator CoStart()
	{
		yield return new WaitForSeconds(1.0f);
	
		yield return StartCoroutine(_sceneUI.CoFadeIn(1.0f));

		yield return new WaitForSeconds(1.0f);

		_camera2.SetActive(true);
		_camera1.SetActive(false);

		yield return new WaitForSeconds(1.5f);

		yield return StartCoroutine(_sceneUI.CoShow(0.5f));
	}

	public override void Clear()
	{

	}
}
