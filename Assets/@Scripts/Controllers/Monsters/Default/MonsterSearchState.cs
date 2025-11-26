using Monster.Chase;
using UnityEngine;
using UnityEngine.AI;


namespace Monster.Search
{
    //Idle상태 이후 Look Around후 3초뒤 새로운 목표지점을 찍는다. 도착할 경우 Idle로 전환
    public class Idle : SearchState
    {
        float _idleStartTime=0f;
        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _idleStartTime = Time.time;
            if(_monster.Ani != null)
            _monster.Ani.CrossFade("Idle", 0.25f);
           
            if(_monster.Nma != null)
                _monster.Nma.isStopped = true;

        }

        public override void UpdateState()
        {
            base.UpdateState();
            if (_monster.MainTarget != null)
            {
                ChangeState(new Attack.StandBy());
                return;
            }
            if (_monster.ChaseTrigger)
            {
                ChangeState(new ChaseToTarget());
                return;
            }


            switch (_monster.searchType)
                {
                    case MonsterController2.SearchType.Default:
                        DefaultUpdate();
                        break;

                    case MonsterController2.SearchType.Patrol:
                        PatrolUpdate();
                        break;

                    case MonsterController2.SearchType.Chase:
                        ChaseUpdate();
                        break;

                    case MonsterController2.SearchType.Alert:
                        AlertUpdate();
                        break;

                    case MonsterController2.SearchType.None:
                        NoneUpdate();
                        break;
                }
        }

        void DefaultUpdate()
        {
            if (Time.time - _idleStartTime > 3f)
            {
                Vector3 targetPosition = GetRandomPointOnNavMesh(_monster.SpawnPoint, 10f);

                // 장애물 감지를 위한 Raycast 실행
                Vector3 destination = GetValidDestination(_monster.SpawnPoint, targetPosition);

                // 최종 목적지 설정
                _monster.Nma.SetDestination(destination);
                _monster.CurrentState = new Move();
            }
        }

        // 장애물 감지를 수행하고 적절한 목적지를 반환하는 함수
        Vector3 GetValidDestination(Vector3 start, Vector3 end)
        {
            RaycastHit hit;

            // Ray를 쏴서 장애물 레이어에 충돌하는지 확인
            if (Physics.Raycast(start, (end - start).normalized, out hit, Vector3.Distance(start, end), _monster.obstacleLayer))
            {
                return hit.point; // 장애물에 부딪힌 위치 반환
            }

            return end; // 장애물이 없으면 원래 목적지 반환
        }

        void PatrolUpdate()
        {
            _monster.CurrentState = new Move();

        }

        void ChaseUpdate()
        {
            _monster.CurrentState = new Move();
        }

        void AlertUpdate()
        {
            Vector3 targetDirection = _monster.SpawnRotation * Vector3.forward; // 스폰 방향 기준 전방
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // 현재 회전에서 목표 회전으로 일정 속도로 회전
            _monster.transform.rotation = Quaternion.RotateTowards(
                _monster.transform.rotation,
                targetRotation,
                90f * Time.fixedDeltaTime // 회전 속도 (90도/초)
            );
        }

        void MemberUpdate()
        {
        }

        void NoneUpdate()
        {

        }

        public override void ExitState()
        {
            base.ExitState();
        }

        public override void OnDamaged(DamageCollider dm)
        {
            base.OnDamaged(dm);
            _monster.TargetDetectionTime = Time.time;
            if (dm.Caster != null)
                _monster.MainTarget = dm.Caster.transform;
        }
    }

    public class Move : SearchState
    {
        public override void EnterState(MonsterController2 beast)
        {
            base.EnterState(beast);
            _monster.Ani.CrossFade("Walk", 0.05f);
            _monster.Nma.isStopped = false;
        }

        public override void UpdateState()
        {
            base.UpdateState();
            if (_monster.MainTarget != null)
            {
                ChangeState(new Attack.StandBy());
                return;
            }
            if (_monster.ChaseTrigger)
            {
                ChangeState(new ChaseToTarget());
                return;
            }
            if (_monster.roleType == MonsterController2.RoleType.Leader || _monster.roleType == MonsterController2.RoleType.Solo)
            {
                switch (_monster.searchType)
                {
                    case MonsterController2.SearchType.Default:
                        DefaultUpdate();
                        break;

                    case MonsterController2.SearchType.Patrol:
                        break;

                    case MonsterController2.SearchType.Chase:
                        ChaseUpdate();
                        break;
                }
            }
            else
            {
                DefaultUpdate();
            }
        }

        void DefaultUpdate()
        {
            if (!_monster.Nma.pathPending)
            {
                if (_monster.Nma.remainingDistance <= _monster.Nma.stoppingDistance)
                {
                    ChangeState(new Idle());
                }
            }
        }

        void ChaseUpdate()
        {
            if (_monster.Nma.remainingDistance <= _monster.Nma.stoppingDistance)
            {
                if(_monster.targets.Find(x=>x.target.gameObject.layer == LayerMask.NameToLayer("Star")) != null)
                {
                    //만약 별조각을 발견하였을 경우 별조각 순회
                    //_monster.CurrentState = new Chase();
                    return;
                }

                //다음 순회 장소 지정(으차햄이 지정해 줘야함)
                //_monster.Nma.SetDestination();
                
                
            }
        }

        void MemberUpdate()
        {
        }

        public override void ExitState()
        {
            base.ExitState();
        }

        public override void OnDamaged(DamageCollider dm)
        {
            base.OnDamaged(dm);
            _monster.TargetDetectionTime = Time.time;
            if (dm.Caster != null)
                _monster.MainTarget = dm.Caster.transform;
        }
    }

    public class ReturnToSpawnPoint : SearchState
    {
        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.Ani.CrossFade("Walk", 0.05f);
            _monster.Nma.isStopped = false;
            _monster.Nma.SetDestination(_monster.SpawnPoint);
        }

        public override void UpdateState()
        {
            if (_monster.ChaseTrigger)
            {
                ChangeState(new ChaseToTarget());
                return;
            }
            if (_monster.Hp <= 0)
            {
                ChangeState(new DeadState());
                //_monster.CurrentState = new DeadState();
            }
            if (_monster.Nma.remainingDistance <= _monster.Nma.stoppingDistance)
            {
                _monster.CurrentState = new Idle();
            }
        }

        public override void ExitState()
        {
            _monster.MainTarget = null;
            _monster.targets.Clear();
            //_monster.MainTargetInfo = null;
        }

        public override void OnDamaged(DamageCollider dm)
        {
            base.OnDamaged(dm);
        }
    }

}