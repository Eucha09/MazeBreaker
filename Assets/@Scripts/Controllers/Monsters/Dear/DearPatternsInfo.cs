using UnityEngine;


namespace DearPatternsInfo
{
    public class WalkPatternInfo : PatternInfo
    {
        MonsterController2 _patternUser;
        float _dist;

        public WalkPatternInfo(MonsterController2 patternUser, float stopDist)
        {
            _patternUser = patternUser;
            _patternName = "Walk";
            _dist = stopDist;
        }

        public override bool PatternConditionCheck()
        {
            if(Vector3.Distance(_patternUser.transform.position,_patternUser.MainTarget.position) > _dist)
            return true;
            else return false;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new PlantAttackPatterns.WalkPattern.MoveToTarget(_dist));
        }
    }

    public class OrbitMovePatternInfo : PatternInfo
    {
        MonsterController2 _patternUser;

        public OrbitMovePatternInfo(MonsterController2 patternUser)
        {
            _patternUser = patternUser;
            _patternName = "Walk";
            _lastPatterUsedTime = int.MinValue;    //시작하자마자 쿨타임 0
        }

        public override bool PatternConditionCheck()
        {

            return true;
        }

        public override void UsePattern()
        {
            if (_patternUser.CurrentState.ToString() == new PlantAttackPatterns.OrbitMovePattern.OrbitAroundPlayer().ToString())
                return;
            Debug.Log("중복검사 통과못함");
            _patternUser.CurrentState.ChangeState(new PlantAttackPatterns.OrbitMovePattern.OrbitAroundPlayer());
        }
    }

    public class BitePatternInfo : PatternInfo
    {
        MonsterController2 _patternUser;
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
            _patternUser.CurrentState.ChangeState(new PlantAttackPatterns.BitePattern.Bite());
        }
    }

    public class PoisonBulletPatternInfo : PatternInfo
    {
        MonsterController2 _patternUser;
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
            _patternUser.CurrentState.ChangeState(new PlantAttackPatterns.PoisonBulletPattern.PoisonBullet());
        }
    }

    //평소에는 메인타겟으로 주로 가장 체력이 적은 아군을 따라다닌다.
    public class MoveToAllyPatternInfo : PatternInfo
    {
        MonsterController2 _patternUser;
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
            PlantController pc = _patternUser as PlantController;

            if(pc.allies.Count == 0)
                return false;

            if (Vector3.Distance(_patternUser.transform.position, pc.GetClosestAlly().transform.position) > _stopDist
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
            _patternUser.CurrentState.ChangeState(new PlantAttackPatterns.MoveToAllyPattern.MoveToAlly(_stopDist));
        }
    }
    //체력이 적은 대상과 가까워졌을 때 힐 패턴을 사용한다.
    public class HealPatternInfo : PatternInfo
    {
        MonsterController2 _patternUser;
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
            PlantController pc = _patternUser as PlantController;

            if (pc.allies.Count == 0)
                return false;

            if (
                Vector3.Distance(_patternUser.transform.position, pc.GetClosestAlly().transform.position) < _patternDist
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
            _patternUser.CurrentState.ChangeState(new PlantAttackPatterns.HealPattern.Heal());
        }
    }
    //적이 가까울 때는 공격한다.

    //아군이 없을 경우에는 적을 향해 걸어간다.

    public class RunAwayPatternInfo : PatternInfo
    {
        MonsterController2 _patternUser;
        float _dist;

        public RunAwayPatternInfo(MonsterController2 patternUser)
        {
            _patternUser = patternUser;
            _patternName = "RunAway";
            _dist = 10;
        }

        public override bool PatternConditionCheck()
		{
			if (_patternUser.MainTarget == null)
				return false;

			if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
				return false;
			if (Vector3.Distance(_patternUser.transform.position, _patternUser.MainTarget.position) < _dist)
                return true;
            else return false;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new DearAttackPatterns.RunAwayPattern.RunAway(_dist));
			_patternUser.CurrentPatternInfo = this;
		}
    }
}