using UnityEngine;


namespace MonkeyMinionPatternsInfo
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
            if ( GetDistanceToTarget(_patternUser.MainTarget) > _dist)
            return true;
            else return false;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new MonkeyMinionAttackPatterns.WalkPattern.MoveToTarget(_dist));
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class OrbitMovePatternInfo : PatternInfo
    {

        public OrbitMovePatternInfo(MonsterController2 patternUser)
        {
            _patternUser = patternUser;
            _patternName = "OrbitMove";
            _lastPatterUsedTime = int.MinValue;    //시작하자마자 쿨타임 0
        }

        public override bool PatternConditionCheck()
        {

            if(_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            else
                return true;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new MonkeyMinionAttackPatterns.OrbitMovePattern.OrbitAroundPlayer());
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
                && RemainingCoolTime() == 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))
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
            _patternUser.CurrentState.ChangeState(new MonkeyMinionAttackPatterns.BitePattern.Bite());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class ThrowPatternInfo : PatternInfo
    {
        float _attackDist;

        public ThrowPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "Throw";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))
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
            _patternUser.CurrentState.ChangeState(new MonkeyMinionAttackPatterns.ThrowPattern.Throw());
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class TwoBitePatternInfo : PatternInfo
    {
        float _attackDist;

        public TwoBitePatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "TwoBite";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))
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
            _patternUser.CurrentState.ChangeState(new MonkeyMinionAttackPatterns.TwoBitePattern.TwoBite());
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class BurrowDownPatternInfo : PatternInfo
    {
        float _attackDist;

        public BurrowDownPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "BurrowDown";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))
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
            _patternUser.CurrentState.ChangeState(new MonkeyMinionAttackPatterns.BurrowDownPattern.BurrowDown());
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class BurrowMovePatternInfo : PatternInfo
    {
        float _attackDist;

        public BurrowMovePatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "BurrowMove";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))
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
            _patternUser.CurrentState.ChangeState(new MonkeyMinionAttackPatterns.BurrowMovePattern.BurrowMove());
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class BurrowUpPatternInfo : PatternInfo
    {
        float _attackDist;

        public BurrowUpPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "BurrowUp";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0
                && !IsObstacleBetween(_patternUser.MainTarget, _patternUser.obstacleLayer))
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
            _patternUser.CurrentState.ChangeState(new MonkeyMinionAttackPatterns.BurrowUpPattern.BurrowUp());
            _patternUser.CurrentPatternInfo = this;
        }
    }



    //평소에는 메인타겟으로 주로 가장 체력이 적은 아군을 따라다닌다.
    public class MoveToAllyPatternInfo : PatternInfo
    {
        float _stopDist;

        public MoveToAllyPatternInfo(MonsterController2 patternUser)
        {
            _patternUser = patternUser;
            _patternName = "MoveToAlly";
            _stopDist = 3;
            _patternCoolTime = 0;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;

            PlantController pc = _patternUser as PlantController;

            if(pc.allies.Count == 0)
                return false;

            if (GetDistanceToTarget(pc.GetLowestHpAlly().transform) > _stopDist
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
            _patternUser.CurrentState.ChangeState(new MonkeyMinionAttackPatterns.MoveToAllyPattern.MoveToAlly(_stopDist));
            _patternUser.CurrentPatternInfo = this;
        }
    }
    //체력이 적은 대상과 가까워졌을 때 힐 패턴을 사용한다.
    public class HealPatternInfo : PatternInfo
    {
        float _patternDist;

        public HealPatternInfo(MonsterController2 patternUser)
        {
            _patternUser = patternUser;
            _patternName = "Heal";
            _patternDist = 3.5f;
            _patternCoolTime = 3f;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;

            PlantController pc = _patternUser as PlantController;

            if (pc.allies.Count == 0)
                return false;

            if (
                GetDistanceToTarget(pc.GetLowestHpAlly().transform) < _patternDist
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
            _patternUser.CurrentState.ChangeState(new MonkeyMinionAttackPatterns.HealPattern.Heal());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    //적이 가까울 때는 공격한다.

    //아군이 없을 경우에는 적을 향해 걸어간다.

    public class RunAwayPatternInfo : PatternInfo
    {
        float _dist;

        public RunAwayPatternInfo(MonsterController2 patternUser)
        {
            _patternUser = patternUser;
            _patternName = "RunAway";
            _dist = 6;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;

            PlantController pc = _patternUser as PlantController;

            if (pc.allies.Count > 0)
                return false;

            if (GetDistanceToTarget(_patternUser.MainTarget) < _dist)
                return true;
            else return false;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new MonkeyMinionAttackPatterns.RunAwayPattern.RunAway(_dist));
            _patternUser.CurrentPatternInfo = this;
        }
    }
}