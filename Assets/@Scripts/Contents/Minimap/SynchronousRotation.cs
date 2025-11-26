using UnityEngine;

public class SynchronousRotation : MonoBehaviour
{
    [SerializeField]
    Transform _synchronizeTarget;
    [SerializeField]
    Vector3 _eulerAnglesOffset;
    [SerializeField]
    bool _isLocalRotation = false;

    [SerializeField]
    bool _syncX;
    [SerializeField]
    bool _syncY;
    [SerializeField]
    bool _syncZ;

    public void SetSynchronizeTarget(Transform target)
    {
        _synchronizeTarget = target;
    }

    Vector3 GetRotation()
    {
        return new Vector3(
            _syncX ? _synchronizeTarget.eulerAngles.x : 0f + _eulerAnglesOffset.x,
            _syncY ? _synchronizeTarget.eulerAngles.y : 0f + _eulerAnglesOffset.y,
            _syncZ ? _synchronizeTarget.eulerAngles.z : 0f + _eulerAnglesOffset.z);
    }

    void Update()
    {
        if (_synchronizeTarget == null)
            return;

		if (_isLocalRotation)
            transform.localEulerAngles = GetRotation();
        else
            transform.eulerAngles = GetRotation();
    }
}
