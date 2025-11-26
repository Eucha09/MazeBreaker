using UnityEngine;


namespace HunterPatternsInfo
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
            _patternUser.CurrentState.ChangeState(new HunterAttackPatterns.WalkPattern.MoveToTarget(_dist));
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
            if (_patternUser.CurrentState.ToString() == new HunterAttackPatterns.OrbitMovePattern.OrbitAroundPlayer().ToString())
                return;
            Debug.Log("중복검사 통과못함");
            _patternUser.CurrentState.ChangeState(new HunterAttackPatterns.OrbitMovePattern.OrbitAroundPlayer());
        }
    }

    //거리가 일정 거리 이내일 경우 휘두르기
    public class SwingPatternInfo : PatternInfo
    {
        MonsterController2 _patternUser;
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
            //_patternUser.CurrentState.ChangeState(new HunterAttackPatterns.Bite());
        }
    }

    //거리가 일정 거리 이상일 경우 돌진

    public class DashPatternInfo : PatternInfo
    {
        MonsterController2 _patternUser;
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
            Transform monsterTransform = _patternUser.transform;
            Transform targetTransform = _patternUser.MainTarget;

            Vector3 direction = targetTransform.position - monsterTransform.position;
            float distance = direction.magnitude; // 타겟까지의 거리
            direction.Normalize(); // 방향 벡터 정규화

            // Raycast를 사용해 장애물 검사
            if (Physics.Raycast(monsterTransform.position + Vector3.up * 1f, direction, out RaycastHit hit, distance, _patternUser.obstacleLayer))
            {
                // 만약 충돌한 오브젝트가 타겟이 아니라면 (즉, 장애물이 있으면)
                if (hit.transform != targetTransform)
                {
                    return false;
                }
            }

            if (Vector3.Distance(_patternUser.transform.position, _patternUser.MainTarget.position) > _attackDist
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
            _patternUser.CurrentState.ChangeState(new HunterAttackPatterns.DashPattern.DashReady());
        }
    }
}