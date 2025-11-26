using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.AI;
using static Unity.Burst.Intrinsics.X86;
using System.Collections.Generic;
using Unity.VisualScripting;
using Monster;
using Monster.Attack;

namespace NightMareAttackPatterns
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
                //_monster.Patterns.Add(new NightMarePatternsInfo.StonePillarPatternInfo(_monster, 10, 120));
                //_monster.Patterns.Add(new NightMarePatternsInfo.WindPatternInfo(_monster, 8, 25));
                //_monster.Patterns.RemoveAll(p => p is NightMarePatternsInfo.StoneWallPatternInfo);
                // ✅ 상태 전이 루틴을 지켜주는 방식
                StateEnd();
            }

            public override void StateEnd()
            {
                //_monster.CurrentState = new StonePillarPattern.StonePillar();
                //_monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
    namespace WalkPattern
    {
        public class MoveToTarget : Monster.AttackState
        {
            float _dist;
            public float BaseSpeed = 4f;
            public float RunSpeed = 8f;

            public MoveToTarget(float dist)
            {
                _dist = dist;
            }

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Nma.speed = RunSpeed;
                _monster.Ani.CrossFade("Run", 0.15f); // 0.15초 동안 자연스럽게 전환
                _monster.Nma.isStopped = false;
                TargetCheckStart();
                //DistanceFromSpawnPointCheckStart();
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
                _monster.Nma.speed = BaseSpeed;
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
    namespace CirclePattern
    {
        public class Circle : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("Circle", 0.15f); // 0.15초 동안 자연스럽게 전환
                //_monster.Ani.Play("Bite", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
    namespace PortalInPattern
    {
        public class PortalIn : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                //GameObject a = Managers.Resource.Instantiate("Effects/NightMare/PortalEnter", _monster.transform.position, Quaternion.identity);
                _monster.Ani.CrossFade("PortalIn", 0.05f); // 0.15초 동안 자연스럽게 전환
                _monster.Nma.isStopped = true;
            }
            public override void StateEnd()
            {
                _monster.CurrentState = new NightMareAttackPatterns.PortalOutPattern.PortalOut();
            }
        }
    }
    namespace PortalOutPattern
    {
        public class PortalOut : Monster.AttackState
        {
            private float searchRadius = 10f;
            private float angleStep = 10f; // 36개 Ray
            float startRadius = 8f;
            float maxRadius = 20f;   // 안전장치
            float radiusStep = 1f;

            // ★ 추가: 포탈 FX/스폰 높이 보정 (필요시 0.1~0.3 사이로 튜닝)
            float yOffset = 2.5f;
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.StartCoroutine(PortalOutRoutine());
                //_monster.Nma.isStopped = true;
            }
            private IEnumerator PortalOutRoutine()
            {
                if (_monster.MainTarget == null)
                {
                    ChangeState(new Monster.Attack.StandBy());
                    yield break;
                }

                Transform player = _monster.MainTarget;
                Vector3 origin = player.position + Vector3.up * 1.0f;

                RaycastHit closestHit = new RaycastHit();
                bool found = false;

                // 반지름을 1씩 늘려가며 360도 스캔
                for (float r = startRadius; r <= maxRadius && !found; r += radiusStep)
                {
                    for (float angle = 0f; angle < 360f; angle += angleStep)
                    {
                        Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                        if (Physics.Raycast(origin, dir, out var hit, r, _monster.obstacleLayer))
                        {
                            if (!hit.collider.CompareTag("Wall")) continue; // 태그로 벽만

                            if (!found || hit.distance < closestHit.distance)
                            {
                                closestHit = hit;
                                found = true;
                            }
                        }
                    }

                    // 프레임 스파이크 방지(선택): 한 반지름 레벨 끝날 때 한 프레임 쉬기
                    if (!found) yield return null;
                }


                if (!found)
                {
                    _monster.NightMareLerpAppearDissolveStart();
                    ChangeState(new Monster.Attack.StandBy());
                    yield break;
                }

                // 1) 포탈 출구 이펙트
                Managers.Resource.Instantiate("Effects/NightMare/PortalExit", closestHit.point + closestHit.normal * 0.02f + Vector3.up * yOffset, Quaternion.LookRotation(closestHit.normal, Vector3.up) * Quaternion.Euler(90, 0, 0));

                // 2) 악몽 위치 이동 (벽 뒤쪽)
                Vector3 exitPos = closestHit.point - closestHit.normal * 4f;
                _monster.Nma.enabled = false; // 순간이동 위해
                _monster.transform.position = exitPos;
                _monster.transform.rotation = Quaternion.LookRotation((player.position - exitPos).normalized, Vector3.up);
                //_monster.LerpDissolveStart();
                _monster.Ani.Play("Idle"); // 혹은 대기용 애니 이름
                yield return new WaitForSeconds(1f); // 살짝 딜레이

                //공격 범위 알려주는 부분

                Vector3 center = _monster.transform.position + (_monster.transform.forward).normalized * 4.5f ;
                Vector3 direct = (_monster.transform.forward).normalized;

                var go = Managers.Resource.Instantiate("Effects/Indicators/Fan/SectorIndicator");
                var indicator = go.GetComponent<SectorIndicatorNoShader>();

                indicator.Setup(new Vector3(center.x,.5f,center.z), direct, angle: 180f, inner: 0f, outer: 13f, duration: .5f);
                indicator.OnCharged = () =>
                {
                    // 인디케이터가 다 차면 실제 공격 이펙트 & 데미지
                    // 실제 공격 판정 (부채꼴 MeshCollider Trigger ON → DefaultDamageCollider가 lifeTime 후 자동 OFF)
                    indicator.ActivateHitCollider(_monster, 0.1f);

                    //GameObject atk = Managers.Resource.Instantiate("Effects/NightMare/WindSlash", center, Quaternion.LookRotation(dir));
                    //atk.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.2f);
                    // 🔥 인디케이터 자연스럽게 사라짐
                    indicator.FadeAndDestroy(0.4f);
                };
                indicator.Play();

                yield return new WaitForSeconds(7f/30f); // 살짝 딜레이

                // 3) 등장 애니
                _monster.NightMareLerpAppearDissolveStart();
                _monster.Ani.Play("PortalOut");

               /* // 4) 앞으로 튀어나오기
                var rb = _monster.GetComponent<Rigidbody>();
                if (rb) rb.isKinematic = true; // 물리 힘 사용 안 함

                float dashDuration = 0.25f;  // 대시 시간
                float dashSpeed = 40f;    // m/s
                float homing = 0.35f;  // 0=직선, 0.3~0.5=살짝 추적

                float t = 0f;
                while (t < dashDuration)
                {
                    t += Time.deltaTime;

                    // 현재 전방과 플레이어 방향을 혼합해 조향
                    Vector3 toPlayer = (player.position - _monster.transform.position);
                    toPlayer.y = 0f;

                    Vector3 curFwd = _monster.transform.forward;
                    Vector3 aimDir = Vector3.Slerp(
                        curFwd,
                        toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : curFwd,
                        homing
                    ).normalized;

                    // 프레임 이동량
                    Vector3 step = aimDir * dashSpeed * Time.deltaTime;

                    // 높이 고정(스폰 높이 유지)
                    Vector3 newPos = _monster.transform.position + step;
                    newPos.y = exitPos.y;

                    _monster.transform.position = newPos;
                    _monster.transform.rotation = Quaternion.LookRotation(aimDir, Vector3.up);

                    yield return null;
                }
               */

                // 5) NavMesh 복구
               //_monster.Nma.enabled = true;
               // _monster.Nma.Warp(_monster.transform.position); // 위치 동기화
               // _monster.Nma.isStopped = false;

                yield break;
            }
            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
    namespace DarkRoarPattern
    {
        public class  DarkRoar: Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("DarkRoar", 0.15f); // 0.15초 동안 자연스럽게 전환
                //_monster.Ani.Play("Bite", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
    namespace LeatherPattern
    {
        public class Leather : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("Leather", 0.15f); // 0.15초 동안 자연스럽게 전환
                //_monster.Ani.Play("Bite", -1, 0f);
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
    
    namespace Ground4HitPattern
    {
        public class Ground4Hit : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Ground4Hit", -1, 0f);
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

    namespace DashPattern
    {
        public class Roar : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Roar", -1, 0f);
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

            }

            Vector3 targetDirection;

            public override void UpdateState()
            {
                _monster.Nma.SetDestination(_monster.MainTarget.position);
                //NavMesh Path Corner[0] 지점을 향해 몸을 튼다.
                //NavMesh의 LinearVelodity를 _monster의 Forward 방향으로  설정한다
                // ✅ 플레이어와의 거리 체크 → 일정 거리 도달 시 Slash로 전환
                float distanceToPlayer = Vector3.Distance(_monster.transform.position, _monster.MainTarget.position);
                if (distanceToPlayer <= 6f)
                {
                    ChangeState(new Slash()); // ✅ 바로 슬래시 상태로 넘어감
                    return;
                }
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
                       // ImpactWall(hit1.point);


                    }
                }

                /*만약 Destination에 도착했을 경우 standby로 전환
                if (!_monster.Nma.pathPending && _monster.Nma.remainingDistance <= _monster.Nma.stoppingDistance)
                {
                    if (!_monster.Nma.hasPath || _monster.Nma.velocity.sqrMagnitude == 0f)
                    {
                        _monster.CurrentState = new StandBy();
                    }
                }*/
            }

            public override void ExitState()
            {
                _monster.Nma.angularSpeed = originalAngularSpeed;
                _monster.Nma.speed = originalSpeed;
            }

            public override void StateEnd()
            {
            }
        }
        public class Slash : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade("Slash", 0.15f); // 0.15초 동안 자연스럽게 전환
                //_monster.Ani.Play("Bite", -1, 0f);
                _monster.Nma.isStopped = true;
            }
            public override void StateEnd()
            {
                _monster.CurrentState = new CirclePattern.Circle();
            }
        }

    }


}