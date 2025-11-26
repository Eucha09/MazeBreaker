using UnityEngine;


namespace SpiderBombPatternsInfo
{
    public class WalkPatternInfo : PatternInfo
    {
        MonsterController2 _patternUser;

        public WalkPatternInfo(MonsterController2 patternUser)
        {
            _patternUser = patternUser;
            _patternName = "Walk";
        }

        public override bool PatternConditionCheck()
        {
            return true;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState = new SpiderBombAttackPatterns.WalkPattern.MoveToTarget();
        }
    }

    public class BombPatternInfo : PatternInfo
    {
        MonsterController2 _patternUser;
        float _patternDist;

        public BombPatternInfo(MonsterController2 patternUser, float patternDist)
        {
            _patternUser = patternUser;
            _patternName = "Bomb";
            _patternDist = patternDist;
        }

        public override bool PatternConditionCheck()
        {
            if (Vector3.Distance(_patternUser.transform.position, _patternUser.MainTarget.position) < _patternDist)
                return true;
            else
                return false;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState = new SpiderBombAttackPatterns.BombPattern.Bomb();
        }
    }
}