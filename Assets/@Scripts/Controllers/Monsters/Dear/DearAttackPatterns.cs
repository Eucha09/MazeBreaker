using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.AI;
using static Unity.Burst.Intrinsics.X86;
using System.Collections.Generic;
using Unity.VisualScripting;
using Monster;
using Monster.Attack;

namespace DearAttackPatterns
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


            /*public override void OnDamaged(DamageCollider dm)
            {
                base.OnDamaged(dm);
                if (dm.DamageCalculate(_monster) == 0)
                    return;
                _monster.CurrentState = new HurtedState(dm);
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
                _monster.Ani.Play("Bite", -1, 0f);
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
                _monster.Ani.Play("Fire");
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

    namespace MoveToAllyPattern
    {
        public class MoveToAlly : Monster.AttackState
        {
            PlantController _plant;
            float _dist;

            public MoveToAlly(float dist)
            {
                _dist = dist;
            }

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _plant = monster as PlantController;
                _monster.Ani.Play("Walk");
                _monster.Nma.isStopped = false;
                TargetCheckStart();
                PatternCheckStart();
            }

            public override void UpdateState()
            {
                base.UpdateState();
                if (_plant != null)
                {
                    _plant.Nma.SetDestination(_plant.GetClosestAlly().transform.position);
                    if (Vector3.Distance(_monster.MainTarget.transform.position, _plant.GetClosestAlly().transform.position) < _dist)
                    {
                        ChangeState(new Monster.Attack.StandBy());
                    }
                }
            }

            public override void ExitState()
            {
                Debug.Log("ExitState");
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

    namespace HealPattern
    {
        public class Heal : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Heal", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }

    namespace RunAwayPattern
    {
        public class RunAway : Monster.AttackState
        {
            float _dist;

            public RunAway(float dist)
            {
                _dist = dist+2.5f;
            }

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("Run", 0.05f);
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
                    PlayAnimationOneTime("Run", 0.05f);
                    RunAwayFromTarget();
                    _monster.Nma.speed = 5f;
                }
                else
                {
                    PlayAnimationOneTime("Walk",0.05f);
                    RunAwayFromTarget();
                    _monster.Nma.speed = 1.5f;
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
                _monster.Nma.speed = 1.5f;
            }


            /*public override void OnDamaged(DamageCollider dm)
            {
                base.OnDamaged(dm);
                if (dm.DamageCalculate(_monster) == 0)
                    return;
                ChangeState(new HurtedState(dm));
            }*/

            string _animationName;

            private void PlayAnimationOneTime(string animationName, float fadeAmount)
            {
                if (_animationName == animationName)
                    return;

				_animationName = animationName;
                _monster.Ani.CrossFade(_animationName, 0.05f);
            }

        }


    }
}