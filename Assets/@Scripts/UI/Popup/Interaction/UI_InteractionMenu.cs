using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_InteractionMenu : UI_Base
{
	enum Buttons
	{ 
		InteractButton,
		PushButton,
		UpButton,
		DownButton,
		FallButton,
		TeleportButton,
	}

	UI_InteractionMap _parent;
	GameObject _target;

	float _animationDuration = 0.2f;
	Coroutine _coScale;

	public override void Init()
	{
		Bind<Button>(typeof(Buttons));

		_parent = GetComponentInParent<UI_InteractionMap>();

		PlayerController player = Managers.Object.GetPlayer();

		GetButton((int)Buttons.InteractButton).gameObject.BindEvent((eventData) => { _parent.SelectInteractionType(InteractionType.Interact); });
		GetButton((int)Buttons.PushButton).gameObject.BindEvent((eventData) =>
		{
			if (player.Stats.CurrentPositiveEnergy >= 150.0f)
				_parent.SelectInteractionType(InteractionType.Push);
		});
		GetButton((int)Buttons.UpButton).gameObject.BindEvent((eventData) =>
		{
			if (player.Stats.CurrentPositiveEnergy >= 150.0f)
				_parent.SelectInteractionType(InteractionType.Up);
		});
		GetButton((int)Buttons.DownButton).gameObject.BindEvent((eventData) =>
		{
			if (player.Stats.CurrentPositiveEnergy >= 150.0f)
				_parent.SelectInteractionType(InteractionType.Down);
		});
		GetButton((int)Buttons.FallButton).gameObject.BindEvent((eventData) =>
		{
			if (player.Stats.CurrentPositiveEnergy >= 150.0f)
				_parent.SelectInteractionType(InteractionType.Fall);
		});
		GetButton((int)Buttons.TeleportButton).gameObject.BindEvent((eventData) =>
		{
			if (player.Stats.CurrentPositiveEnergy >= 150.0f)
				_parent.SelectInteractionType(InteractionType.Teleport);
		});

		gameObject.SetActive(false);
	}

	void RefreshUI()
	{
		PlayerController player = Managers.Object.GetPlayer();

		GetButton((int)Buttons.PushButton).interactable = player.Stats.CurrentPositiveEnergy >= 150.0f;
		GetButton((int)Buttons.UpButton).interactable = player.Stats.CurrentPositiveEnergy >= 150.0f;
		GetButton((int)Buttons.DownButton).interactable = player.Stats.CurrentPositiveEnergy >= 150.0f;
		GetButton((int)Buttons.FallButton).interactable = player.Stats.CurrentPositiveEnergy >= 150.0f;
		GetButton((int)Buttons.TeleportButton).interactable = player.Stats.CurrentPositiveEnergy >= 150.0f;

		GetButton((int)Buttons.InteractButton).gameObject.SetActive(_target.GetComponent<StarPiece>());
		GetButton((int)Buttons.TeleportButton).gameObject.SetActive(_target.GetComponent<StarPiece>());
		//GetButton((int)Buttons.PushButton).gameObject.SetActive(_target.GetComponent<PatternedWall>() && _target.GetComponent<MazeCell>().Type == TileType.Wall);
		GetButton((int)Buttons.PushButton).gameObject.SetActive(false);
		GetButton((int)Buttons.UpButton).gameObject.SetActive(_target.GetComponent<PatternedWall>() && _target.GetComponent<MazeCell>().Type == TileType.Empty);
		GetButton((int)Buttons.DownButton).gameObject.SetActive(_target.GetComponent<PatternedWall>() && _target.GetComponent<MazeCell>().Type == TileType.Wall);
		//GetButton((int)Buttons.FallButton).gameObject.SetActive(_target.GetComponent<PatternedWall>() && _target.GetComponent<MazeCell>().Type == TileType.Wall);
		GetButton((int)Buttons.FallButton).gameObject.SetActive(false);

	}

	public void SetTarget(GameObject go)
	{
		_target = go;

		RefreshUI();

		if (_coScale != null)
		{
			StopCoroutine(_coScale);
			_coScale = null;
		}
		_coScale = StartCoroutine(CoScale());
	}

	IEnumerator CoScale()
	{
		Vector3 startScale = Vector3.zero;
		Vector3 targetScale = Vector3.one;

		float elapsedTime = 0f;

		while (elapsedTime < _animationDuration)
		{
			transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / _animationDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		transform.localScale = targetScale;
		_coScale = null;
	}
}
