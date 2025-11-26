using UnityEngine;

public class GameStatusEventArea : MonoBehaviour
{
	[SerializeField]
	GameStatus _gameStatus;

	void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PlayerController>())
		{
			Managers.Game.SetStatus(_gameStatus, true);
			Managers.Resource.Destroy(gameObject);
		}
	}
}
