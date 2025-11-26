using UnityEngine;


namespace MonsterPatternsInfo
{
    public class StandByPatternInfo : PatternInfo
    {
        public override PatternType GetPatternType()
        {
            return PatternType.StandBy;
        }
    }


    public class GroggyPatternInfo : PatternInfo
    {
        public override PatternType GetPatternType()
        {
            return PatternType.Groggy;
        }
    }

    public struct MoveToTargetPatternData
    {
        public MonsterController2 PatternUser;
        public string PatternName;
        public string AnimationName;
        public float Dist;
    }

    public class MoveToTargetPatternInfo : PatternInfo
    {
        MoveToTargetPatternData _patternData;
        public MoveToTargetPatternInfo(MoveToTargetPatternData pd)
        {
            _patternUser = pd.PatternUser;
            _patternName = pd.PatternName;
            _patternData = pd;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) > _patternData.Dist)
                return true;
            else return false;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState = new Monster.Attack.MoveToTargetPattern.MoveToTarget(_patternData);
            //_patternUser.CurrentPatternInfo = this;
            _patternUser.CurrentPatternName = _patternName;
        }

        public override PatternType GetPatternType()
        {
            return PatternType.Move;
        }
    }



    public struct OrbitMovePatternData
    {
        public MonsterController2 PatternUser;
        public string PatternName;
        public string OrbitMoveAnimationName;
        public string RunAnimationName;
        public float OrbitRadius;
        public float OrbitSpeed;
        public float ReturnSpeed;
        public float RunDist;
    }
    public class OrbitMovePatternInfo : PatternInfo
    {
        OrbitMovePatternData _patternData;

        public OrbitMovePatternInfo(OrbitMovePatternData pd)
        {
            _patternUser = pd.PatternUser;
            _patternName = pd.PatternName;
            _lastPatterUsedTime = int.MinValue;    //시작하자마자 쿨타임 0
            _patternData = pd;
        }

        public override bool PatternConditionCheck()
        {

            if (_patternUser.CurrentPatternName == _patternName)
                return false;
            else
                return true;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new Monster.Attack.OrbitMovePattern.OrbitAroundTarget(
                _patternData
                ));
            _patternUser.CurrentPatternInfo = this;
        }

        public override PatternType GetPatternType()
        {
            return PatternType.Move;
        }
    }

    public struct AttackPatternData
    {
        public MonsterController2 PatternUser;
        public string PatternName;
        public string AnimationName;
        public float MinDist;
        public float MaxDist;
        public float PatternCoolTime;
    }

    public class AttackPatternInfo : PatternInfo
    {
        protected AttackPatternData _patternData;

        public AttackPatternInfo(AttackPatternData pd)
        {
            _patternUser = pd.PatternUser;
            _patternName = pd.PatternName;
            _patternCoolTime = pd.PatternCoolTime;
            _patternData = pd;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) >= _patternData.MinDist
                && GetDistanceToTarget(_patternUser.MainTarget) <= _patternData.MaxDist
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
            _patternUser.CurrentState = new Monster.Attack.AttackPattern.Attack(
                _patternData
                );
            //_patternUser.CurrentPatternInfo = this;
            _patternUser.CurrentPatternName = _patternName;
        }

        public override PatternType GetPatternType()
        {
            return PatternType.Attack;
        }
    }


    public struct RepeatAttackPatternData
    {
        public MonsterController2 PatternUser;
        public string PatternName;
        public string FirstAnimationName;
        public string RepeatAnimationName;
        public string LastAnimationName;
        public int RepeatCount;
        public float MinDist;
        public float MaxDist;
        public float PatternCoolTime;
    }

    public class RepeatAttackPatternInfo : PatternInfo
    {
        protected RepeatAttackPatternData _patternData;

        public RepeatAttackPatternInfo(RepeatAttackPatternData pd)
        {
            _patternUser = pd.PatternUser;
            _patternName = pd.PatternName;
            _patternCoolTime = pd.PatternCoolTime;
            _patternData = pd;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) >= _patternData.MinDist
                && GetDistanceToTarget(_patternUser.MainTarget) <= _patternData.MaxDist
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
            _patternUser.CurrentState = new Monster.Attack.RepeatAttackPattern.RepeatAttack(
                _patternData, 0
                );
            //_patternUser.CurrentPatternInfo = this;
            _patternUser.CurrentPatternName = _patternName;
        }

        public override PatternType GetPatternType()
        {
            return PatternType.Attack;
        }
    }


    public struct RunAwayPatternData
    {
        public MonsterController2 PatternUser;
        public string PatternName;
        public string AnimationName;
        public float RunStartDist;
        public float RunEndDist;
        public string RunAnimationName;
        public float RunSpeed;
        public string WalkAnimationName;
        public float WalkSpeed;
    }

    public class RunAwayPatternInfo : PatternInfo
    {
        RunAwayPatternData _patternData;

        public RunAwayPatternInfo(RunAwayPatternData pd)
        {
            _patternUser = pd.PatternUser;
            _patternName = pd.PatternName;
            _patternData = pd;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternName == _patternName)
                return false;
            if (Vector3.Distance(_patternUser.transform.position, _patternUser.MainTarget.position) < _patternData.RunStartDist)
                return true;
            else return false;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new Monster.Attack.RunAwayPattern.RunAway(_patternData));
            _patternUser.CurrentPatternInfo = this;
        }

        public override PatternType GetPatternType()
        {
            return PatternType.Move;
        }
    }
}