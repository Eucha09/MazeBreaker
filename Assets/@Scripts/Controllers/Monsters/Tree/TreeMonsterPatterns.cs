using UnityEngine;

namespace TreeMonsterPattern
{
    public class JumpAttackPatternInfo : PatternInfo
    {
        TreeMonsterController _patternUser;
        float _patternDist;


        public JumpAttackPatternInfo(TreeMonsterController patternUser, float patternCoolTime, float patternDistance)
        {
            _patternUser = patternUser;
            _patternDist = patternDistance;
            _patternCoolTime = patternCoolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.MainTarget != null
                && Vector3.Distance(_patternUser.MainTarget.position, _patternUser.transform.position) < _patternDist// 거리 체크
                && RemainingCoolTime() <= 0)// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState = new TreeMonster.Attack.JumpAttack();
        }
    }

    public class SpitAttackPatternInfo : PatternInfo
    {
        TreeMonsterController _patternUser;
        float _patternDist;


        public SpitAttackPatternInfo(TreeMonsterController patternUser, float patternCoolTime, float patternDistance)
        {
            _patternUser = patternUser;
            _patternDist = patternDistance;
            _patternCoolTime = patternCoolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.MainTarget != null
                && Vector3.Distance(_patternUser.MainTarget.position, _patternUser.transform.position) < _patternDist// 거리 체크
                && RemainingCoolTime() <= 0)// 쿨타임 체크
            {
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState = new TreeMonster.Attack.SpitAttack(Random.Range(3, 5));
        }
    }
}