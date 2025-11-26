using UnityEngine;


namespace SpiderPatternsInfo
{
    public class WalkPatternInfo : PatternInfo
    {

        public WalkPatternInfo(SpiderController patternUser)
        {
            _patternUser = patternUser;
            _patternName = "Walk";
            _lastPatterUsedTime = int.MinValue;    //시작하자마자 쿨타임 0
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            return true;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new SpiderAttackPatterns.WalkPattern.OrbitAroundPlayer());
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class RunAwayPatternInfo : PatternInfo
    {

        public RunAwayPatternInfo(SpiderController patternUser)
        {
            _patternUser = patternUser;
            _patternName = "RunAway";
            _lastPatterUsedTime = int.MinValue;    //시작하자마자 쿨타임 0

        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (Vector3.Distance(_patternUser.MainTarget.position, _patternUser.transform.position) < 8f)
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new SpiderAttackPatterns.RunAwayPattern.RunAwayFromTarget());
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class PoisonPatternInfo : PatternInfo
    {
        float _patternMaxDist;
        float _patternMinDist;

        public PoisonPatternInfo(SpiderController patternUser, float patternCoolTime, float patternMinDistance, float patternMaxDistance)
        {
            _patternUser = patternUser;
            _patternMaxDist = patternMaxDistance;
            _patternMinDist = patternMinDistance;
            _patternCoolTime = patternCoolTime;
            _patternName = "Poison";
            _lastPatterUsedTime = int.MinValue;    //시작하자마자 쿨타임 0

        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (Vector3.Distance(_patternUser.MainTarget.position, _patternUser.transform.position) < _patternMaxDist// 거리 체크
                && _patternMinDist < Vector3.Distance(_patternUser.MainTarget.position, _patternUser.transform.position)
                && RemainingCoolTime() <= 0)// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new SpiderAttackPatterns.PoisonPattern.Poison());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class SlashPatternInfo : PatternInfo
    {
        float _patternMaxDist;
        float _patternMinDist;

        public SlashPatternInfo(SpiderController patternUser, float patternCoolTime, float patternMinDistance, float patternMaxDistance)
        {
            _patternUser = patternUser;
            _patternMaxDist = patternMaxDistance;
            _patternMinDist = patternMinDistance;
            _patternCoolTime = patternCoolTime;
            _patternName = "Slash";
            _lastPatterUsedTime = int.MinValue;    //시작하자마자 쿨타임 0

        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (Vector3.Distance(_patternUser.MainTarget.position, _patternUser.transform.position) < _patternMaxDist// 거리 체크
                && _patternMinDist < Vector3.Distance(_patternUser.MainTarget.position, _patternUser.transform.position)
                && RemainingCoolTime() <= 0)// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new SpiderAttackPatterns.SlashPattern.Slash());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class WebPatternInfo : PatternInfo
    {
        float _patternMaxDist;
        float _patternMinDist;
        public WebPatternInfo(SpiderController patternUser, float patternCoolTime, float patternMinDistance, float patternMaxDistance)
        {
            _patternUser = patternUser;
            _patternMaxDist = patternMaxDistance;
            _patternMinDist = patternMinDistance;
            _patternCoolTime = patternCoolTime;
            _patternName = "Web";
            _lastPatterUsedTime = int.MinValue;    //시작하자마자 쿨타임 0

        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (Vector3.Distance(_patternUser.MainTarget.position, _patternUser.transform.position) < _patternMaxDist// 거리 체크
                && _patternMinDist < Vector3.Distance(_patternUser.MainTarget.position, _patternUser.transform.position)
                && RemainingCoolTime() <= 0)// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new SpiderAttackPatterns.WebPattern.Web());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class RushPatternInfo : PatternInfo
    {
        float _patternMaxDist;
        float _patternMinDist;
        int _rushCount;

        public RushPatternInfo(SpiderController patternUser, float patternCoolTime, float patternMinDistacne, float patternMaxDistance, int rushCount)
        {
            _patternUser = patternUser;
            _patternMaxDist = patternMaxDistance;
            _patternMinDist = patternMinDistacne;
            _patternCoolTime = patternCoolTime;
            _rushCount = rushCount;
            _patternName = "Rush";
            _lastPatterUsedTime = int.MinValue;    //시작하자마자 쿨타임 0

        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (Vector3.Distance(_patternUser.MainTarget.position, _patternUser.transform.position) < _patternMaxDist// 거리 체크
                && _patternMinDist < Vector3.Distance(_patternUser.MainTarget.position, _patternUser.transform.position)
                && RemainingCoolTime() <= 0)// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new SpiderAttackPatterns.RushPattern.Rush(_rushCount));
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class LayEggPatternInfo : PatternInfo
    {

        public LayEggPatternInfo(SpiderController patternUser, float patternCoolTime)
        {
            _patternUser = patternUser;
            _patternCoolTime = patternCoolTime;
            _patternName = "LayEgg";

        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (RemainingCoolTime() <= 0)// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new SpiderAttackPatterns.LayEggPattern.SetEggPos());
            _patternUser.CurrentPatternInfo = this;

        }
    }

    public class Paze2RoarPatternInfo : PatternInfo, ResetPatternData
    {
        float _patternMaxDist;
        public bool IsPatternUsed = false;

        public Paze2RoarPatternInfo(SpiderController patternUser, float patternCoolTime, float patternMaxDistance)
        {
            _patternUser = patternUser;
            _patternCoolTime = patternCoolTime;
            _patternName = "Paze2Roar";
            _patternMaxDist = patternMaxDistance;
            IsPatternUsed = false;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (_patternUser.Hp <= _patternUser.MaxHp / 2 && !IsPatternUsed)// 쿨타임 체크
            {
                Debug.Log("HP절반!");
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            IsPatternUsed = true;
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new SpiderAttackPatterns.Paze2pattern.Paze2());
            _patternUser.CurrentPatternInfo = this;
        }

        public void Reset()
        {
            IsPatternUsed = false;
        }
    }

    public class Paze1RoarPatternInfo : PatternInfo, ResetPatternData
    {
        float _patternMaxDist;
        public bool IsPatternUsed = false;

        public Paze1RoarPatternInfo(SpiderController patternUser, float patternCoolTime, float patternMaxDistance)
        {
            _patternUser = patternUser;
            _patternCoolTime = patternCoolTime;
            _patternName = "Paze1Roar";
            _patternMaxDist = patternMaxDistance;
            IsPatternUsed = false;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)

                if (_patternUser.Hp > _patternUser.MaxHp / 2 && !IsPatternUsed)// 쿨타임 체크
            {
                Debug.Log("Paze1!");
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            IsPatternUsed = true;
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new SpiderAttackPatterns.Paze1Pattern.Paze1());
            _patternUser.CurrentPatternInfo = this;
        }

        public void Reset()
        {
            IsPatternUsed = false;
        }
    }
}