using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.AI;
using static Unity.Burst.Intrinsics.X86;
using System.Collections.Generic;
using Unity.VisualScripting;
using Monster;

namespace MonkeyMinionAttackPatterns
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
                _monster.Ani.CrossFade("Walk", 0.15f); // 0.15초 동안 자연스럽게 전환
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
            private float orbitRadius = 5f; // 플레이어 중심 반지름
            private float orbitSpeed = 5f;   // 궤도 이동 속도
            private float returnSpeed = 5f; // 궤도 복귀 속도
            private float navMeshCheckDistance = 2f; // NavMesh 샘플링 거리

            private bool isMovingClockwise = true; // 접선 이동 방향 (true: 시계 방향, false: 반시계 방향)
            private float directionChangeCooldown = 5f; // 방향 전환 쿨타임
            private float lastDirectionChangeTime = 0f;

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("Walk", 0.15f); // 0.15초 동안 자연스럽게 전환
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
                lastDirectionChangeTime = Time.time; // 초기 방향 변경 시간 설정

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

                // 🛑 장애물 감지 로직 개선
                RaycastHit hit;
                Vector3 directionToTarget = directTarget - _monster.transform.position;

                if (Physics.Raycast(_monster.transform.position + Vector3.up, -toMonster, out hit, Vector3.Distance(_monster.transform.position, _monster.MainTarget.position), _monster.obstacleLayer))
                {
                    directTarget = playerPosition;
                }

                // Tangent 방향 목표 위치 계산
                Vector3 tangent = isMovingClockwise ? Vector3.Cross(toMonster, Vector3.up).normalized : -Vector3.Cross(toMonster, Vector3.up).normalized;
                Vector3 tangentTarget = directTarget + tangent * (orbitRadius - distanceToOrbit) * 0.5f;

                // 거리 비율 계산 (0: 가장 멀리, 1: 가장 가까이)
                float distanceRatio = Mathf.InverseLerp(orbitRadius, 0f, distanceToOrbit);

                // 목표 위치 보간
                Vector3 targetPosition = Vector3.Lerp(directTarget, tangentTarget, distanceRatio);

                // 🛑 일정 시간마다 방향 전환 추가
                if (Time.time - lastDirectionChangeTime > directionChangeCooldown)
                {
                    isMovingClockwise = !isMovingClockwise;
                    lastDirectionChangeTime = Time.time;
                }

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

                if (distance < _dist + 1)
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
                _monster.Ani.CrossFade("Bite", 0.15f); // 0.15초 동안 자연스럽게 전환
                //_monster.Ani.Play("Bite", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
    namespace ThrowPattern
    {
        public class Throw : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("Throw", 0.15f); // 0.15초 동안 자연스럽게 전환
                //_monster.Ani.Play("Bite", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
    namespace TwoBitePattern
    {
        public class TwoBite : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("TwoBite");
                _monster.Nma.isStopped = true;
                _monster.Nma.updateRotation = false;
            }

            public override void ExitState()
            {
                _monster.Nma.updateRotation = true;
                _monster.Nma.isStopped = false;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
    namespace BurrowDownPattern
    {
        public class BurrowDown : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("BurrowDown", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void StateEnd()
            {
                _monster.CurrentState = new BurrowMovePattern.BurrowMove();
            }
        }
    }
    namespace BurrowMovePattern
    {
        public class BurrowMove : Monster.AttackState
        {
            private GameObject _stoneLoopFX;   // 돌진 중 따라다니는 이펙트
            private Vector3 _targetPos;        // 돌진 목표 위치
            private float _moveSpeed = 16f;     // 돌진 속도
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("BurrowMove", -1, 0f);
                _monster.Nma.isStopped = true;
                //_stoneLoopFX = Managers.Resource.Instantiate("Effects/StoneMinion/BurrowMove", _monster.transform.position + new Vector3(0,1,0), _monster.transform.rotation);
                //_stoneLoopFX.transform.SetParent(_monster.transform);

                _monster.StartCoroutine(BurrowMoveRoutine());
            }
            private IEnumerator BurrowMoveRoutine()
            {
                Debug.Log("BurrowMoveRoutine 시작 (직접 돌진)");

                // 1. 타겟 위치 저장
                _targetPos = _monster.MainTarget.position;

                // 2. 인디케이터 생성 + 잠깐 대기
                //Managers.Resource.Instantiate("Effects/StoneMinion/BurrowAttackIndicator", _targetPos, Quaternion.identity);
                //yield return new WaitForSeconds(1.0f); // 경고용 지연

                // 3. 거리 기반 돌진
                float minDistance = 0.5f;
                int safety = 0;

                while (Vector3.Distance(_monster.transform.position, _targetPos) > minDistance && safety < 300)
                {
                    Vector3 dir = (_targetPos - _monster.transform.position).normalized;
                    _monster.transform.position += dir * _moveSpeed * Time.deltaTime;

                    // 회전도 자연스럽게
                    if (dir != Vector3.zero)
                    {
                        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
                        _monster.transform.rotation = Quaternion.Slerp(_monster.transform.rotation, rot, 10f * Time.deltaTime);
                    }

                    safety++;
                    yield return null;
                }

                // 4. StoneLoop 제거
                if (_stoneLoopFX != null)
                    GameObject.Destroy(_stoneLoopFX);

                Debug.Log("[BurrowMove] 돌진 완료 → BurrowUp 전환");
                StateEnd();
            }
            public override void StateEnd()
            {
                _monster.CurrentState = new BurrowUpPattern.BurrowUp();
            }
        }
    }
    namespace BurrowUpPattern
    {
        public class BurrowUp : Monster.AttackState
        {
      
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("BurrowUp", -1, 0f);
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
                _monster.Ani.CrossFade("Standby", 0.15f); // 0.15초 동안 자연스럽게 전환
                //_monster.Ani.Play("Standby");
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
                _monster.Ani.CrossFade("Walk", 0.15f); // 0.15초 동안 자연스럽게 전환
                _monster.Nma.isStopped = false;
                TargetCheckStart();
                PatternCheckStart();
            }

            public override void UpdateState()
            {
                base.UpdateState();
                if (_plant != null)
                {
                    _plant.Nma.SetDestination(_plant.GetLowestHpAlly().transform.position);
                    if (Vector3.Distance(_monster.MainTarget.transform.position, _plant.GetLowestHpAlly().transform.position) < _dist)
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

                if (_monster.MainTarget == null)
                    return;

                float distance = Vector3.Distance(_monster.transform.position, _monster.MainTarget.position);

                if (distance < _dist)
                {
                    RunAwayFromTarget();
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
}