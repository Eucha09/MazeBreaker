using UnityEngine;

public enum PatternType
{
    None,
    Groggy,
    StandBy,
    Attack,
    Move
}

[System.Serializable]
public class PatternInfo
{
    [SerializeField]
    protected string _patternName;
    public string PatternName { get { return _patternName; } set { _patternName = value; } }
    protected float _lastPatterUsedTime;
    protected float _patternCoolTime;
    [SerializeField] private float _remainingCooltime;

    public MonsterController2 _patternUser;

    public virtual bool PatternConditionCheck()
    {
        if(Time.time - _lastPatterUsedTime > _patternCoolTime)
        {
            return true;
        }
        return false;
    }

    public virtual void UsePattern() { }

    public virtual PatternType GetPatternType() { return PatternType.None; }

    public virtual float RemainingCoolTime() 
    {
        return _patternCoolTime - (Time.time - _lastPatterUsedTime) > 0 ? _patternCoolTime - (Time.time - _lastPatterUsedTime) : 0;
    }

    public void UpdateRemainingCoolTime()
    {
        _remainingCooltime = RemainingCoolTime();
    }

    public virtual float GetDistanceToTarget(Transform target)
    {
        return Vector3.Distance(_patternUser.transform.position, target.GetComponent<Collider>().ClosestPoint(_patternUser.transform.position));
    }

    public virtual bool IsObstacleBetween(Transform target, LayerMask obstacleMask)
    {
        Vector3 start = _patternUser.transform.position;
        Vector3 end = target.GetComponent<Collider>().ClosestPoint(start);
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        // Raycast로 장애물 체크
        if (Physics.Raycast(start, direction.normalized, out RaycastHit hit, distance, obstacleMask))
        {
            return true;
        }

        return false;
    }
}


public interface ResetPatternData
{
    public void Reset();
}