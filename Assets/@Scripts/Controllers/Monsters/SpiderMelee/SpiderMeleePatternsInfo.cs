using UnityEngine;


namespace SpiderMeleePatternsInfo
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
            if (Vector3.Distance(_patternUser.transform.position,_patternUser.MainTarget.position) > _dist)
            return true;
            else return false;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new SpiderMeleeAttackPatterns.WalkPattern.MoveToTarget(_dist));
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class OrbitMovePatternInfo : PatternInfo
    {

        public OrbitMovePatternInfo(MonsterController2 patternUser)
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
            _patternUser.CurrentState.ChangeState(new SpiderMeleeAttackPatterns.OrbitMovePattern.OrbitAroundPlayer());
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
            _patternUser.CurrentState.ChangeState(new SpiderMeleeAttackPatterns.BitePattern.Bite());
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class PoisonBulletPatternInfo : PatternInfo
    {
        float _attackDist;

        public PoisonBulletPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "PoisonBullet";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
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
            _patternUser.CurrentState.ChangeState(new SpiderMeleeAttackPatterns.PoisonBulletPattern.PoisonBullet());
            _patternUser.CurrentPatternInfo = this;
        }
    }
}