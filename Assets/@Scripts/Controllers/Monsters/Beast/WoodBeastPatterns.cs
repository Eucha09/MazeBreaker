using MonsterPatternsInfo;
using UnityEngine;


namespace WoodBeastPattern
{

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
            _patternUser.CurrentState.ChangeState(new WoodBeast.Attack.MoveToTarget(_dist));
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class SingleSwingPatternInfo : PatternInfo
    {
        float _patternDist;


        public SingleSwingPatternInfo(MonsterController2 patternUser, float patternCoolTime, float patternDistance)
        {
            _patternUser = patternUser;
            _patternDist = patternDistance;
            _patternCoolTime = patternCoolTime;
            _patternName = "SingleSwing";
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _patternDist// 거리 체크
                && RemainingCoolTime() <= 0 
                && !IsObstacleBetween(_patternUser.MainTarget,_patternUser.obstacleLayer))// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState( new WoodBeast.Attack.SingleSwing());
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class DoubleSwingPatternInfo : PatternInfo
    {
        float _patternDist;


        public DoubleSwingPatternInfo(MonsterController2 patternUser, float patternCoolTime, float patternDistance)
        {
            _patternUser = patternUser;
            _patternDist = patternDistance;
            _patternCoolTime = patternCoolTime;
            _patternName = "DoubleSwing";
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _patternDist// 거리 체크
                && RemainingCoolTime() <= 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState( new WoodBeast.Attack.DoubleSwing());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class GasiGallePatternInfo : PatternInfo
    {
        float _patternDist;


        public GasiGallePatternInfo(MonsterController2 patternUser, float patternCoolTime, float patternDistance)
        {
            _patternUser = patternUser;
            _patternDist = patternDistance;
            _patternCoolTime = patternCoolTime;
            _patternName = "GasiGalle";
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _patternDist// 거리 체크
                && RemainingCoolTime() <= 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new WoodBeast.Attack.GasiGalle());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class LeaveStormPatternInfo : PatternInfo
    {
        float _patternDist;


        public LeaveStormPatternInfo(MonsterController2 patternUser, float patternCoolTime, float patternDistance)
        {
            _patternUser = patternUser;
            _patternDist = patternDistance;
            _patternCoolTime = patternCoolTime;
            _patternName = "LeaveStorm";
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _patternDist// 거리 체크
                && RemainingCoolTime() <= 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState( new WoodBeast.Attack.LeaveStorm());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class JumpAttackPatternInfo : PatternInfo
    {
        float _patternDist;


        public JumpAttackPatternInfo(MonsterController2 patternUser, float patternCoolTime, float patternDistance)
        {
            _patternUser = patternUser;
            _patternDist = patternDistance;
            _patternCoolTime = patternCoolTime;
            _patternName = "JumpAttack";
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _patternDist// 거리 체크
                && RemainingCoolTime() <= 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new WoodBeast.Attack.JumpAttack(0));
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class Paze2RepeatAttackPatternInfo : RepeatAttackPatternInfo
    {
        public Paze2RepeatAttackPatternInfo(RepeatAttackPatternData pd) : base(pd) { }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) >= _patternData.MinDist
                && GetDistanceToTarget(_patternUser.MainTarget) <= _patternData.MaxDist
                && _patternUser.Hp <= _patternUser.MaxHp / 2
                && RemainingCoolTime() <= 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))// 쿨타임 체크
            {
                return true;
            }
            return false;
        }
    }

    public class Paze2AttackPatternInfo : AttackPatternInfo
    {
        public Paze2AttackPatternInfo(AttackPatternData pd) : base(pd) { }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) >= _patternData.MinDist
                && GetDistanceToTarget(_patternUser.MainTarget) <= _patternData.MaxDist
                && _patternUser.Hp <= _patternUser.MaxHp / 2
                && RemainingCoolTime() <= 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))// 쿨타임 체크
            {
                return true;
            }
            return false;
        }
    }

    public class Paze1AttackPatternInfo : AttackPatternInfo
    {
        public Paze1AttackPatternInfo(AttackPatternData pd) : base(pd) { }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) >= _patternData.MinDist
                && GetDistanceToTarget(_patternUser.MainTarget) <= _patternData.MaxDist
                && _patternUser.Hp >= _patternUser.MaxHp / 2
                && RemainingCoolTime() <= 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))// 쿨타임 체크
            {
                return true;
            }
            return false;
        }
    }

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
            _patternUser.CurrentState.ChangeState( new WoodBeast.Attack.Roar());
            _patternUser.CurrentPatternInfo = this;
        }

        public void Reset()
        {
            IsPatternUsed = false;
        }
    }

    public class Paze2RoarInfo : PatternInfo, ResetPatternData
    {
        float _patternDist;
        public bool IsPatternUsed = false;

        public Paze2RoarInfo(MonsterController2 patternUser, float patternCoolTime, float patternDistance)
        {
            _patternName = "Paze2Roar";
            _patternUser = patternUser;
            _patternDist = patternDistance;
            _patternCoolTime = patternCoolTime;
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
            _patternUser.CurrentState.ChangeState(new WoodBeast.Attack.Paze2());
            _patternUser.CurrentPatternInfo = this;
        }
        public void Reset()
        {
            IsPatternUsed = false;
        }
    }
    public class ThrowRockPatternInfo : PatternInfo
    {
        float _patternDist;


        public ThrowRockPatternInfo(MonsterController2 patternUser, float patternCoolTime, float patternDistance)
        {
            _patternUser = patternUser;
            _patternDist = patternDistance;
            _patternCoolTime = patternCoolTime;
            _patternName = "ThrowRock";
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _patternDist// 거리 체크
                && _patternUser.Hp <= _patternUser.MaxHp / 2
                && RemainingCoolTime() <= 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            // _patternUser.CurrentState.ChangeState( new WoodBeast.Attack.ThrowRock(2)); 돌 두 번 던질 때
            _patternUser.CurrentState.ChangeState( new WoodBeast.Attack.ThrowRock());
            _patternUser.CurrentPatternInfo = this;
        }
    }
}