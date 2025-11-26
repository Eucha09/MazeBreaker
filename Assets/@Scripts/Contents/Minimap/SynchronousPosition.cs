using UnityEngine;

public class SynchronousPosition : MonoBehaviour
{
    [SerializeField]
    Transform _target;
    [SerializeField]
    Vector3 _offset;
    [SerializeField]
    bool _isLocalPosition = false;

    [SerializeField]
    bool _syncX;
    [SerializeField]
    bool _syncY;
    [SerializeField]
    bool _syncZ;

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    Vector3 GetPosion()
    {
        return new Vector3(
            _syncX ? _target.position.x : 0f + _offset.x,
            _syncY ? _target.position.y : 0f + _offset.y,
            _syncZ ? _target.position.z : 0f + _offset.z);
    }

    void Update()
    {
		if (_target == null)
            return;

		if (_isLocalPosition)
            transform.localPosition= GetPosion();
        else
            transform.position = GetPosion();
    }
}
