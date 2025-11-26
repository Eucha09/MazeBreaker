using Data;
using Player;
using System.Collections;
using UnityEngine;

public class Tent : Interactable
{
	int _objectId;
	PlayerController _player;
	RestingSystem _restingSystem;

	//Coroutine buffer;
	//public float _mentalHealAmount;

	void Start()
	{
		_player = Managers.Object.GetPlayer();

		_objectId = GetComponentInParent<BaseController>().TemplateId;
		ObjectData objectData = null;
		Managers.Data.ObjectDict.TryGetValue(_objectId, out objectData);
		_name = objectData.name;

		_restingSystem = GetComponent<RestingSystem>();
		_restingSystem.InteractionObjectType = PlayerInteractionObjectType.Tent;
	}

    public override void Interact()
	{

		//Managers.Object.GetPlayer().PlayerInteractionStart(PlayerInteractionObjectType.Tent,
		//    transform.position, gameObject.transform.forward, this);


		GetComponent<RestingSystem>().Resting(_player);

        //UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
        //ui.ShowTimeScaleControlUI();

        //if (buffer != null)
        //    return;
        //buffer = StartCoroutine(CoRollBack());
        
    }

    //IEnumerator CoRollBack()
    //{
    //    Managers.Object.GetPlayer().GetComponent<PlayerController>().CurrentState = new Player.PlayerSleepState();
    //    UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
    //    yield return StartCoroutine(ui.CoFadeOut());
    //    Managers.Object.GetPlayer().GetComponent<PlayerController>().Stats.AffectMental(_mentalHealAmount / 2);
    //    Managers.Object.GetPlayer().GetComponent<Rigidbody>().MovePosition(transform.position);
    //    Managers.Time.Seconds += 180f;

    //    yield return new WaitForSeconds(5f);
    //    Managers.Object.GetPlayer().GetComponent<PlayerController>().Stats.AffectMental(_mentalHealAmount/2);




    //    yield return StartCoroutine(ui.CoFadeIn());
    //    Managers.Object.GetPlayer().GetComponent<PlayerController>().CurrentState = new Player.PlayerIdleState();
    //    buffer = null;
    //}

	public override void Cancel()
	{
        GetComponent<RestingSystem>().Cancel();
	}
}
