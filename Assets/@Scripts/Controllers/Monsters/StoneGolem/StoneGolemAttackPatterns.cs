using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.AI;
using static Unity.Burst.Intrinsics.X86;
using System.Collections.Generic;
using Unity.VisualScripting;
using Monster;
using Monster.Attack;

namespace StoneGolemAttackPatterns
{
    namespace RoarPattern
    {
        public class Roar : Monster.AttackState
        {
            //JumpAttackAnimation이 끝났는지 체크

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("Roar", 0.2f); // 0.15초 동안 자연스럽게 전환
                //_monster.Ani.Play("Roar");
                _monster.Nma.isStopped = true;

            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
    namespace Paze2Pattern
    {
        public class Paze2 : Monster.AttackState
        {
            //JumpAttackAnimation이 끝났는지 체크

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                //_monster.Ani.CrossFade("Roar", 0.2f); // 0.15초 동안 자연스럽게 전환
                //_monster.Ani.Play("Roar");
                //_monster.Nma.isStopped = true;
                _monster.Patterns.Add(new StoneGolemPatternsInfo.StonePillarPatternInfo(_monster, 10, 120));
                //_monster.Patterns.Add(new StoneGolemPatternsInfo.WindPatternInfo(_monster, 8, 25));
                //_monster.Patterns.RemoveAll(p => p is StoneGolemPatternsInfo.StoneWallPatternInfo);
                // ✅ 상태 전이 루틴을 지켜주는 방식
                StateEnd();
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new StonePillarPattern.StonePillar();
                //_monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
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
            private float orbitSpeed = 3f;   // 궤도 이동 속도
            private float returnSpeed = 3f; // 궤도 복귀 속도
            private float navMeshCheckDistance = 2f; // NavMesh 샘플링 거리

            private bool isMovingClockwise = true; // 접선 이동 방향 (true: 시계 방향, false: 반시계 방향)
            private float directionChangeCooldown = 5f; // 방향 전환 쿨타임
            private float lastDirectionChangeTime = 0f;

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
        }
    }

    namespace SmartOrbitMovePattern
    {
        // 플레이어와 거리/패턴 조건에 따라 빠르게 추적하거나, 느리게 궤도 돌며 다음 패턴을 기다리는 상태
        public class SmartOrbitAroundPlayer : Monster.AttackState
        {
            private float chaseSpeed = 6f;      // ▶ 거리가 멀 때 추적 속도
            private float orbitSpeed = 4f;      // ▶ 궤도 돌 때 속도
            private float orbitRadius = 3f;      // 궤도 반경 (단순 참고값)
            private float navSampleRadius = 1.5f;// NavMesh 샘플링 허용 반경

            private float switchDist = 5f;      // ▶ 이 거리보다 가까우면 궤도 진입

            private bool isOrbiting = false;
            private bool isClockwise = true;

            private string _currentAnim = "";

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Nma.isStopped = false;

               // isClockwise = Random.value > 0.5f;  반시계로 돌지 시계로 돌지

            }

            public override void UpdateState()
            {
                TargetInRangeStateCheck();
                UsePattern();

                if (_monster.MainTarget == null) return;

                float dist = Vector3.Distance(_monster.transform.position, _monster.MainTarget.position);

                // ▶ 추적 모드
                if (dist > switchDist)
                {

                    isOrbiting = false;
                    _monster.Nma.speed = chaseSpeed;
                    PlayAnim("WalkFast");
                    _monster.Nma.SetDestination(_monster.MainTarget.position);
                    
                }
                // ▶ 궤도 돌기 모드
                else
                {
                    if (!isOrbiting)
                    {
                        isOrbiting = true;
                        _monster.Nma.speed = orbitSpeed;
                        PlayAnim("WalkSlow");
                    }

                    OrbitMove(); // 접선 방향으로 회전 이동
                }
            }

            private void OrbitMove()
            {
                Transform player = _monster.MainTarget;
                Vector3 toMonster = (_monster.transform.position - player.position).normalized;

                // 시계 방향 접선 벡터 계산
                Vector3 tangent = Vector3.Cross(toMonster, Vector3.up).normalized;

                // 목표 위치 = 지금 위치에서 접선 방향으로 이동
                Vector3 offset = tangent * orbitRadius; // 약간만 움직이게 보정
                Vector3 orbitTarget = _monster.transform.position + offset;

                // NavMesh 위의 위치 보정
                NavMeshHit hit;
                if (NavMesh.SamplePosition(orbitTarget, out hit, navSampleRadius, NavMesh.AllAreas))
                {
                    float distance = Vector3.Distance(_monster.transform.position, hit.position);

                    if (distance > 0.3f) // 너무 가까운 위치면 생략
                    {
                        _monster.Nma.SetDestination(hit.position); // ✅ 목적지 설정!
                    }
                }
            }


            private void PlayAnim(string animName)
            {
                if (_currentAnim == animName) return;

                _monster.Ani.CrossFade(animName, 0.1f);
                _currentAnim = animName;
            }

            public override void ExitState()
            {
                _monster.Nma.speed = _monster.MoveSpeed;
            }
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

    namespace DashAttackPattern
    {
        public class DashAttack : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("DashAttack");
                _monster.Nma.ResetPath();
                _monster.Nma.isStopped = true;
                _monster.Nma.updateRotation = false;

                //플레이어쪽을 향해 바라보고 돌진
            }



            public override void ExitState()
            {
                _monster.Nma.updateRotation = true;
            }

            public override void StateEnd()
            {
                _monster.CurrentState.ChangeState(new Monster.Attack.StandBy());
            }
        }
    }
    namespace ThunderAttackPattern
    {
        public class ThunderAttack : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("ThunderAttack");
                _monster.Nma.ResetPath();
                _monster.Nma.isStopped = true;
                _monster.Nma.updateRotation = false;

                //플레이어쪽을 향해 바라보고 돌진
            }



            public override void ExitState()
            {
                _monster.Nma.updateRotation = true;
            }

            public override void StateEnd()
            {
                _monster.CurrentState.ChangeState(new Monster.Attack.StandBy());
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


            public override void OnDamaged(DamageCollider dm)
            {
                base.OnDamaged(dm);
                if (dm.DamageCalculate(_monster) == 0)
                    return;
            }

        }



    }

    namespace StoneUpPattern
    {
        public class StoneUp : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("StoneUp", 0.15f); // 0.15초 동안 자연스럽게 전환
                //_monster.Ani.Play("StoneUp", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void ExitState()
            {
                _monster.Nma.isStopped = false;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }

    namespace StoneWallPattern
    {
        public class StoneWall : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("StoneWall", .15f);
                _monster.Nma.isStopped = true;
            }
            public override void ExitState()
            {
                _monster.Nma.isStopped = false;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new JumpAttackPattern.JumpAttack();
            }
        }
    }

    namespace StoneWallPaze2Pattern
    {
        public class StoneWallPaze2 : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("StoneWallPaze2", .15f);
                _monster.Nma.isStopped = true;
            }
            public override void ExitState()
            {
                _monster.Nma.isStopped = false;
            }

            public override void StateEnd()
            {
                // _monster.CurrentState = new Monster.Attack.StandBy();
                _monster.CurrentState = new Ground3HitPattern.Ground3Hit();

            }
        }
    }
    namespace Ground3HitPattern
    {
        public class Ground3Hit : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("Ground3Hit", .15f);
                _monster.Nma.isStopped = true;
            }
            public override void ExitState()
            {
                _monster.Nma.isStopped = false;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
                //_monster.CurrentState = new StoneUpPattern.StoneUp();

            }
        }
    }
    namespace StonePillarPattern
    {
        public class StonePillar : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("StonePillar", .15f);
                _monster.Nma.isStopped = true;
            }
            public override void ExitState()
            {
                _monster.Nma.isStopped = false;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
                //_monster.CurrentState = new StoneUpPattern.StoneUp();

            }
        }
    }
    namespace JumpAttackPattern
    {
        public class JumpAttack : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("JumpAttack", .15f);
                _monster.Nma.isStopped = true;
            }
            public override void ExitState()
            {
                _monster.Nma.isStopped = false;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
            public IEnumerator MoveToTarget(Transform MainTarget, Vector3 destination, float duration)
            {
                // 시작 시간과 시작 위치를 기록
                float startTime = Time.time;
                Vector3 startPosition = MainTarget.position;

                // 목표 위치까지의 총 거리를 계산
                float distance = Vector3.Distance(startPosition, destination);

                // 일정 시간동안 이동
                while (Time.time < startTime + duration)
                {
                    // 경과 시간 비율을 계산 (0에서 1 사이의 값)
                    float elapsed = (Time.time - startTime) / duration;

                    // 경과 시간 비율에 따라 위치를 선형적으로 보간
                    MainTarget.position = Vector3.Lerp(startPosition, destination, elapsed);


                    RaycastHit hit;
                    if (Physics.Raycast(_monster.transform.position + Vector3.up, _monster.transform.forward, out hit, 3f, _monster.obstacleLayer))
                    {
                        break;
                    }

                    // NavMesh 상에서 이동 가능 여부 확인
                    NavMeshHit navHit;
                    if (!NavMesh.SamplePosition(_monster.transform.position + _monster.transform.forward * 3f * Time.deltaTime, out navHit, 0.5f, NavMesh.AllAreas))
                    {
                        break;
                    }

                    if (Physics.Raycast(_monster.transform.position + Vector3.up, _monster.transform.forward, out hit, 3f))
                    {
                        NavMeshObstacle obstacle = hit.collider.GetComponent<NavMeshObstacle>();
                        if (obstacle != null)
                        {
                            break;
                        }
                    }

                    // 다음 프레임까지 대기
                    yield return null;
                }

                // 정확히 목표 위치에 도착하도록 설정
                if (Time.time - startTime >= duration)
                    MainTarget.position = destination;
            }
        }
    }
    namespace WindPattern
    {
        public class Wind : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Wind", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void ExitState()
            {
                _monster.Nma.isStopped = false;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
    namespace ThrowStonePattern
    {
        public class ThrowStone : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("ThrowStone", .15f);
                //_monster.Ani.Play("ThrowStone", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void ExitState()
            {
                _monster.Nma.isStopped = false;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
    namespace SpawnMonsterPattern
    {
        public class SpawnMonster : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("SpawnMonster", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void ExitState()
            {
                _monster.Nma.isStopped = false;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
    namespace TurntoSpherePattern
    {
        public class TurntoSphere : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("TurntoSphere", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void ExitState()
            {
                _monster.Nma.isStopped = false;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new RollPattern.Roll();
            }
        }
    }

    namespace RollPattern
    {
        public class Roll : Monster.AttackState
        {
            float originalSpeed;
            float originalAngularSpeed;

            GameObject dashEffect = null;

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);


                _monster.Nma.isStopped = true;
                _monster.Ani.Play("Roll", -1, 0f);
                _monster.Nma.enabled = true;

                //NavMesh의스피드를 올린다.
                originalSpeed = _monster.Nma.speed;
                originalAngularSpeed = _monster.Nma.angularSpeed;

                //충돌용 DamageCollider 생성
                dashEffect = Managers.Resource.Instantiate("Effects/Hunter/DashEffect", _monster.transform.position, _monster.transform.rotation, _monster.transform);
                dashEffect.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 100f);
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
            }


            public void Impact(Vector3 HittedPoint)    //플레이어 충돌 시
            {
                Managers.Sound.Play3DSound(_monster.gameObject, "Sounds/Hunter/HunterHitPlayer", 0f, 54f);

                //맞은 지점에 충돌 이펙트 생성
                Managers.Resource.Instantiate("Effects/Hunter/Dashtoplayer", _monster.transform.position, _monster.transform.rotation);
                //Bump로 State 전환
                ChangeState(new StandBy());
            }

            public void ImpactWall(Vector3 HittedPoint)    //벽 충돌 시
            {
                Managers.Sound.Play3DSound(_monster.gameObject, "Sounds/Hunter/HunterHitWall", 0f, 54f);
                //맞은 지점에 충돌 이펙트 생성
                Managers.Resource.Instantiate("Effects/Hunter/Dashtowall", HittedPoint, Quaternion.identity);
                //Bump로 State 전환
                ChangeState(new StandBy());
            }

            public override void ExitState()
            {
                _monster.Nma.isStopped = false;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
}




