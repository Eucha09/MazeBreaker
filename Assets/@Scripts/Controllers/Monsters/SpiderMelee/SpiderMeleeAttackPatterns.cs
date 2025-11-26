using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.AI;
using static Unity.Burst.Intrinsics.X86;
using System.Collections.Generic;
using Unity.VisualScripting;
using Monster;

namespace SpiderMeleeAttackPatterns
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
                    if (!_monster.Nma.pathPending)
                    {
                        if (GetDistanceToTarget(_monster.MainTarget) < _dist)
                        {
                            ChangeState(new Monster.Attack.StandBy());
                            return;
                        }

                        if (!_monster.Nma.hasPath || _monster.Nma.velocity.sqrMagnitude == 0f)
                        {
                            ChangeState(new Monster.Attack.StandBy());
                            return;
                        }
                    }
                }
            }
            /*public override void OnDamaged(DamageCollider dm)
            {
                base.OnDamaged(dm);
                ChangeState(new HurtedState(dm));
            }*/
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
            private float lastDirectionChangeTime = 0f;

            private bool isMovingClockwise = true; // 접선 이동 방향 (true: 시계 방향, false: 반시계 방향)

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Walk");
                _monster.Nma.isStopped = false;

                if (_monster.MainTarget != null)
                {
                    Vector3 direction = _monster.transform.position - _monster.MainTarget.transform.position;

                    // 🛑 몬스터와 플레이어가 같은 위치라면 NaN 방지
                    if (direction == Vector3.zero)
                    {
                        direction = Vector3.forward; // 기본 방향 지정
                    }
                    else
                    {
                        direction.Normalize();
                    }

                    Vector3 initialTarget = _monster.MainTarget.transform.position + direction * orbitRadius;
                    SetValidNavMeshPosition(initialTarget);
                }

                // 시계 / 반시계 방향 랜덤 결정
                isMovingClockwise = Random.Range(0, 2) == 0 ? false : true;

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

                float distanceToOrbit = Vector3.Distance(_monster.transform.position, playerPosition + toMonster * orbitRadius);
                Vector3 directTarget = playerPosition + toMonster * orbitRadius;


                // 🛑 전방 장애물 감지 로직 추가 (회피용 Raycast)
                Vector3 forwardDirection = _monster.transform.forward;
                float checkDistance = 2f; // 감지 거리

                if (Physics.Raycast(_monster.transform.position + Vector3.up, forwardDirection, checkDistance, _monster.obstacleLayer))
                {
                    Debug.Log("⚠️ 앞에 장애물 감지! 회피 행동 실행");

                    // 플레이어와 너무 가까운 경우, RunAway 상태로 전환
                    if (Vector3.Distance(_monster.transform.position, playerPosition) < orbitRadius)
                    {
                        ChangeState(new RunAway(orbitRadius * 1.5f)); // 도망 거리 증가
                        return;
                    }
                    else if (Time.time - lastDirectionChangeTime > 1f)
                    {
                        isMovingClockwise = !isMovingClockwise;
                        lastDirectionChangeTime = Time.time;
                    }
                }

                // 🛑 장애물 감지 로직 개선
                RaycastHit hit;
                Vector3 directionToTarget = directTarget - _monster.transform.position;

                if (Physics.Raycast(_monster.transform.position + Vector3.up, -toMonster, out hit, Vector3.Distance(_monster.transform.position, _monster.MainTarget.position), _monster.obstacleLayer))
                {
                    directTarget = playerPosition;
                }

                //플레이어와의 거리가 orbitRadius보다 작고, 전방이 NavMesh상으로 가지 못하는 지역일 경우 RunAway Class로 State 전환

                // Tangent 방향 목표 위치 계산
                Vector3 tangent = isMovingClockwise ? Vector3.Cross(toMonster, Vector3.up).normalized : -Vector3.Cross(toMonster, Vector3.up).normalized;
                Vector3 tangentTarget = directTarget + tangent * (orbitRadius - distanceToOrbit) * 0.5f;

                // 거리 비율 계산 (0: 가장 멀리, 1: 가장 가까이)
                float distanceRatio = Mathf.InverseLerp(orbitRadius, 0f, distanceToOrbit);

                // 목표 위치 보간
                Vector3 targetPosition = Vector3.Lerp(directTarget, tangentTarget, distanceRatio);

                // 🛑 NavMesh 샘플링 거리 활용 개선
                SetValidNavMeshPosition(targetPosition, true);
            }

            private bool SetValidNavMeshPosition(Vector3 targetPosition, bool isReturning = false)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetPosition, out hit, navMeshCheckDistance, NavMesh.AllAreas))
                {
                    _monster.Nma.speed = isReturning ? returnSpeed : orbitSpeed;
                    _monster.Nma.SetDestination(hit.position);
                    return true;
                }

                return false; // 유효한 위치를 찾지 못함
            }

            public override void ExitState()
            {
                _monster.Nma.speed = _monster.MoveSpeed;
            }

            /*public override void OnDamaged(DamageCollider dm)
            {
                base.OnDamaged(dm);
                if (dm.DamageCalculate(_monster) == 0)
                    return;
                ChangeState(new HurtedState(dm));
            }*/
        }

        public class RunAway : Monster.AttackState
        {
            float _dist;

            public RunAway(float dist)
            {
                _dist = dist;
            }

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Walk");
                _monster.Nma.isStopped = false;
                _monster.Nma.speed = 5f;
                TargetCheckStart();
                PatternCheckStart();
            }

            public override void UpdateState()
            {
                base.UpdateState();

                if (_monster.MainTarget == null)
                    return;

                float distance = Vector3.Distance(_monster.transform.position, _monster.MainTarget.position);

                if (distance < _dist)
                {
                    RunAwayFromTarget();
                }
                else
                {
                    _monster.CurrentState = new OrbitAroundPlayer();
                }
            }

            void RunAwayFromTarget()
            {
                Vector3 fleeDirection = (_monster.transform.position - _monster.MainTarget.position).normalized;
                Vector3 newPos = _monster.transform.position + fleeDirection * _dist;

                // 장애물 감지 (레이캐스트)
                if (Physics.Raycast(_monster.transform.position, fleeDirection, out RaycastHit hit, _dist))
                {
                    // 충돌한 경우 옆 방향으로 도망
                    fleeDirection = Vector3.Cross(fleeDirection, Vector3.up);
                    newPos = _monster.transform.position + fleeDirection * _dist;
                }

                NavMeshHit navHit;
                if (NavMesh.SamplePosition(newPos, out navHit, 5.0f, NavMesh.AllAreas))
                {
                    _monster.Nma.SetDestination(navHit.position);
                }
            }

            public override void ExitState()
            {
                _monster.Nma.speed = 3.5f;
            }


            /*public override void OnDamaged(DamageCollider dm)
            {
                base.OnDamaged(dm);
                if (dm.DamageCalculate(_monster) == 0)
                    return;
                ChangeState(new HurtedState(dm));
            }*/
        }
    }

    namespace BitePattern
    {
        public class Bite : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Bite");
                _monster.Nma.isStopped = true;
                
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }

    namespace PoisonBulletPattern
    {
        public class PoisonBullet : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("PoisonBullet");
                _monster.Nma.isStopped = true;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new IdlePattern.Idle();
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