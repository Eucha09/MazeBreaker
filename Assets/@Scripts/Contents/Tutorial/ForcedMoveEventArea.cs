using UnityEngine;

public class ForcedMoveEventArea : MonoBehaviour
{
    [SerializeField]
    GameObject _movePoint;

	void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PlayerController>())
        {
            other.GetComponent<PlayerController>().PlayerMoveToEventPosition(_movePoint.transform.position);
            Managers.Resource.Destroy(gameObject);
        }
	}
}
