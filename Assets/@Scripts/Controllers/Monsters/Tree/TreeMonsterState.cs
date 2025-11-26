using UnityEngine;
using UnityEngine.InputSystem;


namespace TreeMonster
{
    public abstract class State
    {
        public bool isLooking = false;
        public bool IsAnimationEnd { get; set; } = false;


        protected TreeMonsterController _monster;

        public virtual void EnterState(TreeMonsterController monster)
        {
            _monster = monster;
        }

        public virtual void UpdateState()
        {
            if (_monster.Hp <= 0)
            {
                _monster.CurrentState = new TreeMonster.Search.Dead();
            }
        }
        public virtual void ExitState() { }

        public virtual void HandleOnTriggerEnter(Collider other) { }
        public virtual void OnDamaged(DamageCollider dm)
        {
            _monster.Hp -= dm.DamageCalculate(_monster);
        }

    }

    public abstract class AttackState : State
    {
        public override void EnterState(TreeMonsterController monster)
        {
            base.EnterState(monster);
            //_monster.BaseMat.SetFloat("_UseEmission", 1.0f);
        }

        public void TargetInRangeStateCheck()
        {
            if (_monster.MainTarget == null)
                _monster.CurrentState = new TreeMonster.Search.LookAround();
        }

        //현재 사용 가능한 스킬이 있는지
        public void UsePattern()
        {
            foreach (var pattern in _monster.Patterns)
            {
                if (pattern.PatternConditionCheck())
                {
                    pattern.UsePattern();
                    break;
                }
            }
        }



        //타겟을 바라보게한다.
        public void LookPlayer()
        {


            if (!isLooking || _monster.MainTarget == null)
                return;
            // 목표(MainTarget) 방향 벡터를 계산합니다.
            Vector3 directionToTarget = _monster.MainTarget.position - _monster.transform.position;

            // 방향 벡터의 y값은 0으로 만들어서 평면에서만 회전하도록 합니다.
            directionToTarget.y = 0;

            // 방향 벡터의 크기를 정규화합니다.
            directionToTarget.Normalize();

            // 현재 오브젝트의 Y축을 목표를 바라보도록 회전 값을 생성합니다.
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

            // 회전을 적용합니다 (부드럽게 Lerp).
            _monster.transform.rotation = Quaternion.Lerp(_monster.transform.rotation, lookRotation, Time.deltaTime * 10f);

        }

    }

    public abstract class SearchState : State
    {
        public Vector3 randomDirection;


        //플레이어를 찾는 로직 작성 필요
        //BoxArea로 Raycast를 쏴서 만약 특정 Layer를 갖고 있는 오브젝트가 감지될경우 가장 가까운 적을 타겟으로 설정
        public void TargetInRangeStateCheck()
        {
            if (_monster.MainTarget != null)
                _monster.CurrentState = new TreeMonster.Attack.Idle();
        }

        public void LookRandomDirection()
        {
            // 현재 오브젝트의 Y축을 랜덤한 방향으로 바라보도록 회전 값을 생성합니다.
            Quaternion lookRotation = Quaternion.LookRotation(randomDirection);

            // 회전을 적용합니다 (부드럽게 Lerp).
            _monster.transform.rotation = Quaternion.Lerp(_monster.transform.rotation, lookRotation, Time.deltaTime * 10f);

            // 현재 회전과 목표 회전 사이의 차이를 계산합니다.
            float angleDifference = Quaternion.Angle(_monster.transform.rotation, lookRotation);
        }
    }
}