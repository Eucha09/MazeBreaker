using Player;
using ShaderCrew.SeeThroughShader;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class DieSoldier : Interactable
{
	[SerializeField]
	GameObject _dieSoldier;
	[SerializeField]
	GameObject _dieSoldier_Naked;

	void Start()
    {
		_name = "쓰러져있는 군인";
	}

	public override void Interact()
	{
		if (_dieSoldier_Naked.activeSelf)
			return;

		StartCoroutine(CoClothChange());
	}

	IEnumerator CoClothChange()
    {
        UI_GameScene sceneUI = Managers.UI.SceneUI as UI_GameScene;
		Managers.Object.GetPlayer().BlockInput();
		
		yield return StartCoroutine(sceneUI.CoFadeOut());


		GameObject player = Managers.Object.GetPlayer().gameObject;
		PlayerInput oldInput = player.GetComponent<PlayerInput>();
		oldInput.DeactivateInput();
		oldInput.enabled = false;

		Managers.Event.UpdateAction += ChangePlayerObject;

        Managers.Game.SetStatus(GameStatus.ChangingClothes, true);
		_dieSoldier.SetActive(false);
		_dieSoldier_Naked.SetActive(true);

		yield return new WaitForSeconds(3.0f);

		yield return StartCoroutine(sceneUI.CoFadeIn());
    }

	void ChangePlayerObject()
    {
        GameObject player = Managers.Object.GetPlayer().gameObject;
        GameObject newPlayer = Managers.Resource.Instantiate("Creature/Player", player.transform.position, player.transform.rotation);
        TutorialScene scene = Managers.Scene.CurrentScene as TutorialScene;

        Managers.Object.Add(newPlayer);
		Managers.Minimap.SetPlayer(newPlayer);
        scene.PlayerPosManager.AddPlayerAtRuntime(newPlayer);
        Camera.main.GetComponent<MainCameraController>().SetTarget(newPlayer, Define.CameraType.Follow);
        GameObject minimapCamera = GameObject.Find("MinimapCamera");
        minimapCamera.GetComponent<SynchronousPosition>().SetTarget(newPlayer.transform);
        GameObject light = GameObject.Find("Directional Light");
        light.GetComponent<DayAndNight>().Fog.GetComponent<SynchronousPosition>().SetTarget(newPlayer.transform);

		//PlayerInput oldInput = player.GetComponent<PlayerInput>();
		//PlayerInput newInput = newPlayer.GetComponent<PlayerInput>();
		//oldInput.DeactivateInput();
		//foreach (var device in oldInput.user.pairedDevices)
		//{
		//	newInput.user.UnpairDevice(device);
		//	InputUser.PerformPairingWithDevice(device, newInput.user);
		//}
		//newInput.user.ActivateControlScheme(oldInput.currentControlScheme);
		scene.PlayerPosManager.RemovePlayerAtRuntime(player);
        Managers.Resource.Destroy(player);

		//newInput.ActivateInput();

		Managers.Event.UpdateAction -= ChangePlayerObject;

    }
}
