using UnityEngine;

public class Detector : MonoBehaviour
{
    Collider _collider;

    void Start()
    {
        _collider = GetComponent<Collider>();
        _collider.enabled = true;
    }

    void Update()
    {
        if (_collider.enabled)
            return;
        //if (Managers.Quest.FinishedQuest.Contains(1001) && Managers.Quest.FinishedQuest.Contains(1002))
        //    _collider.enabled = true;
    }
}
