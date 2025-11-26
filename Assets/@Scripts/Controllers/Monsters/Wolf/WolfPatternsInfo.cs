using UnityEngine;


namespace WolfPatternsInfo
{
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

            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            else
                return true;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new WolfAttackPatterns.OrbitMovePattern.OrbitAroundPlayer());
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
            _patternUser.CurrentState.ChangeState(new WolfAttackPatterns.BitePattern.Bite());
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class DashAttackPatternInfo : PatternInfo
    {
        float _attackDist;

        public DashAttackPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
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
            _patternUser.CurrentState.ChangeState(new WolfAttackPatterns.DashAttackPattern.DashAttack());
            _patternUser.CurrentPatternInfo = this;
        }
    }
}