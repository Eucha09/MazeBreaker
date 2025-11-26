using Data;
using ShaderCrew.SeeThroughShader;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
using static Define;

public class GameScene : BaseScene
{
	// Game initial settings (for testing)
    [SerializeField]
    string _mapName;
	[SerializeField]
	float _minutesDay = 0.5f;
	[SerializeField]
	float _minutesNight = 0.5f;
	[SerializeField]
	DayAndNight _directionalLight;
    [SerializeField]
    PlayersPositionManager _playersPositionManager;
	[SerializeField]
    Text _coolTimeText;

    protected override void Init()
	{
		SceneType = Define.Scene.Game;
		Screen.SetResolution(1920, 1080, true);
		//Debug.developerConsoleVisible = false;


		// Preload resources
		Managers.Resource.LoadAllAsync<UnityEngine.Object>("Preload", (key, count, totalCount) =>
        {
            //Debug.Log($"{key} {count}/{totalCount}");
			if (count == totalCount)
			{
				StartLoaded();
			}
        });
	}

	void StartLoaded()
	{
		Managers.Data.Init();

		// Load Map And Spawn Objects
		Managers.Map.LoadMap(_mapName);
		Managers.Map.GenerateWorldSpawnData();

		// Create Player
		GameObject player = Managers.Resource.Instantiate("Creature/Player");
		Managers.Object.Add(player);
		Managers.Minimap.SetPlayer(player);
		_playersPositionManager.AddPlayerAtRuntime(player);

		// Setting the length of day and night (for testing)
		Managers.Time.SetDayAndNightLength(_minutesDay, _minutesNight);
		_directionalLight.Init();

		// BGM
		Managers.Sound.Play("BGM/DARK FOREST BGM", Define.Sound.Bgm);

		// Camera Setup (Main Camera, Minimap Camera)
		Camera.main.GetComponent<MainCameraController>().SetTarget(player, Define.CameraType.Follow);
		GameObject minimapCamera = Managers.Resource.Instantiate("Camera/MinimapCamera");
		minimapCamera.GetComponent<SynchronousRotation>().SetSynchronizeTarget(Camera.main.transform);
		minimapCamera.GetComponent<SynchronousPosition>().SetTarget(player.transform);

		// Show UI
		UI_GameScene ui = Managers.UI.ShowSceneUI<UI_GameScene>();

		// Quest Reconnection
		Managers.Quest.ReconnectingQuests();

		// Create Detector AI
		GameObject detectorAI = new GameObject { name = $"@DetectorAI" };
		detectorAI.AddComponent<DetectorAI>();

		// Initial Game Status Settings (for testing)
		Managers.Game.SetStatus(GameStatus.Enable_Interaction, true);
		Managers.Game.SetStatus(GameStatus.Enable_Inventory, true);
		Managers.Game.SetStatus(GameStatus.TimePasses, true);
		Managers.Game.SetStatus(GameStatus.ChangingClothes, true);
		Managers.Game.SetStatus(GameStatus.PickUpRadar, true);
		Managers.Game.SetStatus(GameStatus.TutorialComplete, true);
		Managers.Quest.StartQuest(1005);
		Managers.Quest.StartQuest(1006);
		Managers.Quest.StartQuest(1022);
	}

    void Update()
    {
		if (Managers.Game.CheckStatus(GameStatus.DemoComplete))
			StartCoroutine(LoadNextScene());

        if (Input.GetKeyDown(KeyCode.F4))
        {
            Application.Quit();
        }
	}

	IEnumerator LoadNextScene()
	{
		UI_GameScene sceneUI = Managers.UI.SceneUI as UI_GameScene;

		yield return StartCoroutine(sceneUI.CoFadeOut());

		Managers.Scene.LoadScene(Define.Scene.End);
	}

    public override void Clear()
    {

    }
}
