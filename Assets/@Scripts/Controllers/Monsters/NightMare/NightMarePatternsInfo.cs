using UnityEngine;
using System.Linq;


namespace NightMarePatternsInfo
{

    public class Paze1RoarInfo : PatternInfo, ResetPatternData
    {
        float _patternMaxDist;
        public bool IsPatternUsed = false;

        public Paze1RoarInfo(MonsterController2 patternUser, float patternCoolTime, float patternMaxDistance)
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
                return false;
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
            _patternUser.CurrentState.ChangeState(new NightMareAttackPatterns.RoarPattern.Roar());
            _patternUser.CurrentPatternInfo = this;
        }

        public void Reset()
        {
            IsPatternUsed = false;
        }
    }
    public class WalkPatternInfo : PatternInfo
    {
        float _dist;

        public WalkPatternInfo(MonsterController2 patternUser, float stopDist)
        {
            _patternUser = patternUser;
            _patternName = "Walk";
            _dist = stopDist;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) > _dist)
                return true;
            else return false;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new NightMareAttackPatterns.WalkPattern.MoveToTarget(_dist));
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class BitePatternInfo : PatternInfo
    {
        float _attackDist;

        public BitePatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "Bite";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new NightMareAttackPatterns.BitePattern.Bite());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class CirclePatternInfo : PatternInfo
    {
        float _attackDist;

        public CirclePatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "Circle";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new NightMareAttackPatterns.CirclePattern.Circle());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class PortalInPatternInfo : PatternInfo
    {
        float _attackDist;

        public PortalInPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "PortalIn";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new NightMareAttackPatterns.PortalInPattern.PortalIn());
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class LeatherPatternInfo : PatternInfo
    {
        float _attackDist;

        public LeatherPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "Leather";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new NightMareAttackPatterns.LeatherPattern.Leather());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class DarkRoarPatternInfo : PatternInfo
    {
        float _attackDist;

        public DarkRoarPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "DarkRoar";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new NightMareAttackPatterns.DarkRoarPattern.DarkRoar());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class JumpAttackPatternInfo : PatternInfo
    {
        float _attackDist;

        public JumpAttackPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "JumpAttack";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new NightMareAttackPatterns.JumpAttackPattern.JumpAttack());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class Ground4HitPatternInfo : PatternInfo
    {
        float _attackDist;

        public Ground4HitPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "Ground4Hit";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new NightMareAttackPatterns.Ground4HitPattern.Ground4Hit());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    
    public class Paze2PatternInfo : PatternInfo
    {
        float _patternDist;
        public bool IsPatternUsed = false;

        public Paze2PatternInfo(MonsterController2 patternUser, float patternCoolTime, float patternDistance)
        {
            _patternName = "Paze2";
            _patternUser = patternUser;
            _patternDist = patternDistance;
            _patternCoolTime = patternCoolTime;
            IsPatternUsed = false;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (_patternUser.Hp < _patternUser.MaxHp / 2 && !IsPatternUsed)// 쿨타임 체크
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
            _patternUser.CurrentState.ChangeState(new NightMareAttackPatterns.Paze2Pattern.Paze2());
            _patternUser.CurrentPatternInfo = this;
        }

    }
    //거리가 일정 거리 이내일 경우 휘두르기
    public class SwingPatternInfo : PatternInfo
    {
        float _attackDist;

        public SwingPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "Swing";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (Vector3.Distance(_patternUser.transform.position, _patternUser.MainTarget.position) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentPatternInfo = this;
            //_patternUser.CurrentState.ChangeState(new HunterAttackPatterns.Bite());
        }
    }

    //거리가 일정 거리 이상일 경우 돌진

    public class DashPatternInfo : PatternInfo
    {
        float _attackDist;

        public DashPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "Dash";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new NightMareAttackPatterns.DashPattern.Roar());
            _patternUser.CurrentPatternInfo = this;
        }

    }



}