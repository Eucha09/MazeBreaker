using Data;
using Player;
using ShaderCrew.SeeThroughShader;
using System.Collections;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.InputSystem.EnhancedTouch;

public class TutorialScene : BaseScene
{
	[SerializeField]
	string _mapName;

	// TEMP
	[SerializeField]
	float _minutesDay = 0.5f;
	[SerializeField]
	float _minutesNight = 0.5f;

	[SerializeField]
	PlayersPositionManager _playersPositionManager;
	public PlayersPositionManager PlayerPosManager { get { return _playersPositionManager; } }

	protected override void Init()
	{
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
		// TEMP
		Managers.Time.SetDayAndNightLength(_minutesDay, _minutesNight);

		Screen.SetResolution(1920, 1080, true);

		SceneType = Define.Scene.Tutorial;

		Managers.Sound.Play("BGM/DARK FOREST BGM", Define.Sound.Bgm);

		Managers.Map.LoadMap(_mapName);
		Managers.Map.GenerateWorldSpawnData();

		Vector3 pos = Managers.Map.GridToWorld(new Vector2Int(43, 11));
		GameObject player = Managers.Resource.Instantiate("Creature/ExperimentPlayer", pos, Quaternion.identity);
		Managers.Object.Add(player);

		_playersPositionManager.AddPlayerAtRuntime(player);
		Camera.main.GetComponent<MainCameraController>().SetTarget(player, Define.CameraType.Follow);

		GameObject minimapCamera = Managers.Resource.Instantiate("Camera/MinimapCamera");
		minimapCamera.GetComponent<SynchronousRotation>().SetSynchronizeTarget(Camera.main.transform);
		minimapCamera.GetComponent<SynchronousPosition>().SetTarget(player.transform);

		UI_GameScene ui = Managers.UI.ShowSceneUI<UI_GameScene>();

		//ui.ShowDialogue("으... 여긴?", 5);

		Managers.Game.SetStatus(GameStatus.Enable_Interaction, true);
		StartCoroutine(CoTutorialStart());
	}

	IEnumerator CoTutorialStart()
	{
		yield return null;

		Managers.Quest.StartQuest(1001);

		CreateStarPiece(Managers.Map.GridToWorld(new Vector2Int(43, 13)));
		CreateStarPiece(Managers.Map.GridToWorld(new Vector2Int(39, 13)));
		CreateStarPiece(Managers.Map.GridToWorld(new Vector2Int(39, 21)));
		CreateStarPiece(Managers.Map.GridToWorld(new Vector2Int(37, 21)));
		CreateStarPiece(Managers.Map.GridToWorld(new Vector2Int(37, 29)));
		CreateStarPiece(Managers.Map.GridToWorld(new Vector2Int(31, 29)));
	}

	void Update()
	{
		//Vector2Int gridPos = Managers.Map.WorldToGrid(Managers.Object.GetPlayer().gameObject.transform.position);
		//Debug.Log("x : " + gridPos.x + " y : " + gridPos.y);

		if (Managers.Game.CheckStatus(GameStatus.TutorialComplete))
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

		Managers.Scene.LoadScene(Define.Scene.Prologue3);
	}

	void CreateStarPiece(Vector3 pos)
	{
		GameObject go = Managers.Resource.Instantiate("Item/RewardObject", pos, Quaternion.identity);
		go.GetComponent<RewardObject>().Init(600, 1);
		go.GetComponentInChildren<StarPiece>().Connect();
	}

	public override void Clear()
	{

	}
}
