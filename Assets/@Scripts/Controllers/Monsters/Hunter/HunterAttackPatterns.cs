using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.AI;
using Monster;
using Monster.Attack;

namespace HunterAttackPatterns
{
    namespace WalkPattern
    {
        public class MoveToTarget : Monster.AttackState
        {
            float _dist;

            public MoveToTarget(float dist) 
            {
                _dist = dist;
            }
            
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Walk");
                _monster.Nma.isStopped = false;
                TargetCheckStart();
                PatternCheckStart();
            }

            public override void UpdateState()
            {
                base.UpdateState();
                if (_monster.MainTarget != null)
                {
                    _monster.Nma.SetDestination(_monster.MainTarget.transform.position);
                    if (Vector3.Distance(_monster.MainTarget.transform.position, _monster.transform.position) < _dist)
                    {
                        ChangeState(new Monster.Attack.StandBy());
                    }
                }
            }

            public override void ExitState()
            {
                Debug.Log("ExitState");
            }

            
            public override void OnDamaged(DamageCollider dm)
            {
                base.OnDamaged(dm);
                if (dm.DamageCalculate(_monster) == 0)
                    return;
            }
            
        }
    }

    namespace OrbitMovePattern
    {
        public class OrbitAroundPlayer : Monster.AttackState
        {
            private float defaultOrbitRadius = 6f;
            private float orbitRadius = 6f; // 플레이어 중심 반지름
            private float orbitSpeed = 3f;   // 궤도 이동 속도
            private float returnSpeed = 3f; // 궤도 복귀 속도
            private float navMeshCheckDistance = 2f; // NavMesh 샘플링 거리

            private bool isMovingClockwise = true; // 접선 이동 방향 (true: 시계 방향, false: 반시계 방향)

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Walk");
                _monster.Nma.isStopped = false;

                // 초기 목표 위치 설정
                if (_monster.MainTarget != null)
                {
                    Vector3 direction = (_monster.transform.position - _monster.MainTarget.transform.position).normalized;
                    Vector3 initialTarget = _monster.MainTarget.transform.position + direction * orbitRadius;
                    SetValidNavMeshPosition(initialTarget);
                }
                int randomValue = Random.Range(0, 2); // 0 이상 2 미만의 정수
                if (randomValue == 0)
                {
                    isMovingClockwise = false;
                }

            }

            public override void UpdateState()
            {

                if (_monster.MainTarget != null)
                {

                    ReturnToOrbit();
                }
                TargetInRangeStateCheck();
                UsePattern();
            }

            private void ReturnToOrbit()
            {
                Vector3 playerPosition = _monster.MainTarget.transform.position;
                Vector3 toMonster = (_monster.transform.position - playerPosition).normalized;

                // 현재 _monster와 궤도 간의 거리 계산
                float distanceToOrbit = Vector3.Distance(_monster.transform.position, playerPosition + toMonster * orbitRadius);

                // 궤도를 향해 가는 방향
                Vector3 targetPosition;

                // 거리 비율 계산 (0: 가장 멀리, 1: 가장 가까이)
                float distanceRatio = Mathf.InverseLerp(orbitRadius, 0f, distanceToOrbit);

                // 정방향 목표 위치 계산
                Vector3 directTarget = playerPosition + toMonster * orbitRadius;

                // Tangent 방향 목표 위치 계산
                Vector3 tangent = isMovingClockwise ? Vector3.Cross(toMonster, Vector3.up).normalized : -Vector3.Cross(toMonster, Vector3.up).normalized;
                Vector3 tangentTarget = playerPosition + toMonster * orbitRadius + tangent * (orbitRadius - distanceToOrbit) * 0.5f;

                // 두 목표 위치를 보간 (distanceRatio에 따라 비율 변경)
                targetPosition = Vector3.Lerp(directTarget, tangentTarget, distanceRatio);
                // Raycast로 장애물 감지
                RaycastHit hit;
                Vector3 directionToTarget = targetPosition - _monster.MainTarget.transform.position;
                if (Physics.Raycast(new Vector3(_monster.MainTarget.transform.position.x, _monster.MainTarget.transform.position.y + 1f, _monster.MainTarget.transform.position.z), directionToTarget, out hit, directionToTarget.magnitude, _monster.obstacleLayer))
                {
                    // 장애물이 감지되면 Raycast hit 위치를 targetPosition으로 설정
                    targetPosition = hit.point;
                }

                Vector3 directionToTarget2 = directTarget - _monster.MainTarget.transform.position;
                if (Physics.Raycast(new Vector3(_monster.transform.position.x, _monster.transform.position.y + 1f, _monster.transform.position.z), directionToTarget2, out hit, defaultOrbitRadius, _monster.obstacleLayer))
                {
                    orbitRadius = Mathf.Lerp(orbitRadius, Vector3.Distance(playerPosition, hit.point) - 2f, Time.deltaTime * 2f);
                }
                else
                {
                    orbitRadius = Mathf.Lerp(orbitRadius, defaultOrbitRadius, Time.deltaTime * 2f); // 기본 반경 12
                }


                // 유효한 NavMesh 위치로 보정
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(targetPosition, out navHit, 0.1f, NavMesh.AllAreas))
                {
                    targetPosition = navHit.position;
                }
                else
                {
                    // NavMesh 유효한 위치를 찾지 못한 경우, 회전 방향 전환
                    //isMovingClockwise = !isMovingClockwise;
                }

                SetValidNavMeshPosition(targetPosition, true);
            }

            private bool SetValidNavMeshPosition(Vector3 targetPosition, bool isReturning = false)
            {
                // NavMesh의 유효한 위치를 찾아 이동
                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetPosition, out hit, navMeshCheckDistance, NavMesh.AllAreas))
                {
                    _monster.Nma.speed = isReturning ? returnSpeed : orbitSpeed;
                    _monster.Nma.SetDestination(hit.position);
                    return true;
                }

                return false; // 유효한 위치를 찾지 못함
            }
            public override void OnDamaged(DamageCollider dm)
            {
                base.OnDamaged(dm);
                if (dm.DamageCalculate(_monster) == 0)
                    return;
            }

        }
    }

    namespace DashPattern
    {
        public class DashReady : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("DashReady",-1,0f);
                _monster.Nma.isStopped = true;
                LookTargetStart();
            }

            public override void UpdateState()
            {
                base.UpdateState();
                Transform monsterTransform = _monster.transform;
                Transform targetTransform = _monster.MainTarget;

                Vector3 targetDirection = targetTransform.position - monsterTransform.position;
                float distance = targetDirection.magnitude; // 타겟까지의 거리
                targetDirection.Normalize(); // 방향 벡터 정규화

                if (Physics.Raycast(monsterTransform.position + Vector3.up * 1f, targetDirection, out RaycastHit hit, distance, _monster.obstacleLayer))
                {
                    // 만약 충돌한 오브젝트가 타겟이 아니라면 (즉, 장애물이 있으면)
                    if (hit.transform != targetTransform)
                    {
                        LookTargetEnd();
                    }
                    else
                    {
                        LookTargetStart();
                    }
                }
            }

            public override void StateEnd()
            {
                ChangeState(new Dash());
                //_monster.CurrentState = new Dash();
            }
        }

        public class Dash : Monster.AttackState
        {
            //대쉬 로직을 바꾼다.
            //NavMesh를 활용해서 돌진 이동 구현

            float originalSpeed;
            float originalAngularSpeed;

            GameObject dashEffect = null;

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                Managers.Sound.Play3DSound(monster.gameObject, "Sounds/Hunter/HunterRunStart", 0f, 54f);
                _monster.Ani.Play("Dash", -1, 0f);
                _monster.Nma.isStopped = false;
                _monster.Nma.enabled = true;

                //NavMesh의스피드를 올린다.
                originalSpeed = _monster.Nma.speed;
                originalAngularSpeed = _monster.Nma.angularSpeed;
                //_monster.Nma.angularSpeed = 250;       // 회전 - 높을수록 빠르게 회전
                //_monster.Nma.speed = 70f;             // 최대속도

                //충돌용 DamageCollider 생성
                dashEffect =  Managers.Resource.Instantiate("Effects/Hunter/DashEffect", _monster.transform.position, _monster.transform.rotation, _monster.transform);
                dashEffect.GetComponentInChildren<DefaultDamageCollider>().Init(_monster,100f);
                
            }

            Vector3 targetDirection;

            public override void UpdateState()
            {
                _monster.Nma.SetDestination(_monster.MainTarget.position);
                //NavMesh Path Corner[0] 지점을 향해 몸을 튼다.
                //NavMesh의 LinearVelodity를 _monster의 Forward 방향으로  설정한다
                if (_monster.Nma.path.corners.Length > 1)
                {
                    // 1. 다음 목표 지점 설정 (Corner[1]은 현재 위치 바로 다음 목표 지점)
                    targetDirection = (_monster.Nma.path.corners[1] - _monster.transform.position).normalized;
                }
                else
                {
                    targetDirection = (_monster.MainTarget.position - _monster.transform.position).normalized;
                }

                // 2. 회전 방향 설정 (몸을 튼다)
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                _monster.transform.rotation = Quaternion.Slerp(_monster.transform.rotation, targetRotation, Time.deltaTime * 5f);

                // 3. NavMeshAgent의 velocity를 forward 방향으로 설정
                _monster.Nma.velocity = _monster.transform.forward * 30f;

                //플레이어가 보이지 않으면 앞으로만 돌진하고, 타겟이 보일경우 다시 그쪽으로 몸을 튼다.
                //레이캐스트 검사결과 장애물이 중간에 있을경우 or 반대
                // Raycast를 사용해 장애물 검사
                base.UpdateState();
                Vector3 origin = _monster.transform.position + new Vector3(0, 1, 0);
                Vector3 direction1 = _monster.transform.forward;
                float maxDistance = 3f; // 최대 탐색 거리

                // SphereCast 실행
                if (Physics.Raycast(origin, direction1, out RaycastHit hit1, maxDistance, _monster.obstacleLayer))
                {
                    // 충돌한 표면의 법선 벡터
                    Vector3 normal = hit1.normal;

                    // Ray와 충돌 표면의 각도 계산 (법선 벡터와 Ray 방향의 각도)
                    float angle = Vector3.Angle(-direction1, normal); // 법선과의 각도

                    if (angle < 50)
                    {
                        ImpactWall(hit1.point);


                    }
                }

                //만약 Destination에 도착했을 경우 standby로 전환
                if (!_monster.Nma.pathPending && _monster.Nma.remainingDistance <= _monster.Nma.stoppingDistance)
                {
                    if (!_monster.Nma.hasPath || _monster.Nma.velocity.sqrMagnitude == 0f)
                    {
                        _monster.CurrentState = new StandBy();
                    }
                }

                /*
                Transform monsterTransform = _monster.transform;
                Transform targetTransform = _monster.MainTarget;

                Vector3 targetDirection = targetTransform.position - monsterTransform.position;
                float distance = targetDirection.magnitude; // 타겟까지의 거리
                targetDirection.Normalize(); // 방향 벡터 정규화

                if (Physics.Raycast(monsterTransform.position + Vector3.up * 1f, targetDirection, out RaycastHit hit, distance, _monster.obstacleLayer))
                {
                    // 만약 충돌한 오브젝트가 타겟이 아니라면 (즉, 장애물이 있으면)
                    if (hit.transform != targetTransform)
                    {
                        isTargetLook = false;
                    }
                    else
                    {
                        isTargetLook = true;
                    }
                }

                if(Vector3.Distance(_monster.transform.position, _monster.MainTarget.position) < 5f && !isClosedToTarget)
                {
                    isTargetLook = false;
                    isClosedToTarget = true;
                }

                if (isClosedToTarget)
                {
                    isTargetLook = false;
                }

                //타겟과 일정거리 이상 가까워지면 회전 정지

                if (isTargetLook)
                {
                    //슬금슬급 타겟쪽으로 바라보면서 Velocity를 올려줘야함
                    Vector3 direction = _monster.MainTarget.position - _monster.transform.position;
                    direction.y = 0; // Y축 회전만 고려하므로 Y값을 0으로 설정

                    if (direction != Vector3.zero)
                    {
                        // 목표 방향으로의 회전 계산
                        Quaternion targetRotation = Quaternion.LookRotation(direction);

                        // 현재 회전을 목표 회전으로 Lerp 보간
                        _monster.transform.rotation = Quaternion.Lerp(_monster.transform.rotation, targetRotation, Time.fixedDeltaTime * 6f);
                    }
                }

                _monster.Rb.linearVelocity = _monster.transform.forward * 30f;


                Vector3 origin = _monster.transform.position + new Vector3(0,1,0);
                Vector3 direction1 = _monster.transform.forward;
                float maxDistance = 3f; // 최대 탐색 거리
                float radius = 0.7f; // SphereCast 반지름 (원하는 크기로 조절)

                // SphereCast 실행
                if (Physics.SphereCast(origin, radius, direction1, out RaycastHit hit1, maxDistance, _monster.obstacleLayer))
                {
                    // 충돌한 표면의 법선 벡터
                    Vector3 normal = hit1.normal;

                    // Ray와 충돌 표면의 각도 계산 (법선 벡터와 Ray 방향의 각도)
                    float angle = Vector3.Angle(-direction1, normal); // 법선과의 각도

                    if (angle < 50)
                    {
                        Impact(hit1.point);
                    }
                }
                */
            }

            public override void ExitState()
            {
                _monster.Nma.angularSpeed = originalAngularSpeed;
                _monster.Nma.speed = originalSpeed;
                //충돌용 DamageCollider 파괴
                GameObject.Destroy(dashEffect);
            }

            //충돌용 데미지콜라이더에 플레이어 or 벽이 부딪혔을 경우 현재 State 종료
            public void Impact(Vector3 HittedPoint)    //플레이어 충돌 시
            {
                Managers.Sound.Play3DSound(_monster.gameObject, "Sounds/Hunter/HunterHitPlayer", 0f, 54f);

                //맞은 지점에 충돌 이펙트 생성
                Managers.Resource.Instantiate("Effects/Hunter/Dashtoplayer", _monster.transform.position, _monster.transform.rotation);
                //Bump로 State 전환
                ChangeState(new Bump());
            }

            public void ImpactWall(Vector3 HittedPoint)    //벽 충돌 시
            {
                Managers.Sound.Play3DSound(_monster.gameObject, "Sounds/Hunter/HunterHitWall", 0f, 54f); 
                //맞은 지점에 충돌 이펙트 생성
                Managers.Resource.Instantiate("Effects/Hunter/Dashtowall", HittedPoint, Quaternion.identity);
                //Bump로 State 전환
                ChangeState(new Bump());
            }

            /*
            //OnColliderEnter
            public override void HandleOnColliderEnter(Collision collision)
            {
                if(collision.gameObject.layer == LayerMask.NameToLayer("Block"))
                {
                    //벽에 부딪혔을 경우
                    ChangeState(new Bump());
                    //_monster.CurrentState = new Bump();
                }
            }
            */
            public override void StateEnd()
            {
            }
        }

        public class Bump : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Bump", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }

    namespace IdlePattern
    {
        public class Idle : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Standby");
                _monster.Nma.isStopped = true;
                _monster.StartCoroutine(Wait());
            }

            IEnumerator Wait()
            {
                yield return new WaitForSeconds(0.5f);
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
}