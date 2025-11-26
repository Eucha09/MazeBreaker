using Unity.VisualScripting;
using UnityEngine;

public class PrologueScen : BaseScene
{
	[SerializeField]
	Define.Scene _curScene;
	[SerializeField]
	Define.Scene _nextScene;

	protected override void Init()
	{
		base.Init();

		SceneType = _curScene;
		Screen.SetResolution(1920, 1080, true);
		//Debug.developerConsoleVisible = false;
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Application.Quit();
        }
    }

    public void NextScene()
	{
		Managers.Scene.LoadScene(_nextScene);
	}

	public override void Clear()
	{

	}

}
