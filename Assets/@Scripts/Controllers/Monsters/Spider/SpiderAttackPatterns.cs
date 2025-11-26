using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.AI;
using static Unity.Burst.Intrinsics.X86;
using System.Collections.Generic;
using Unity.VisualScripting;
using Monster;
using Monster.Attack;

namespace SpiderAttackPatterns
{
    namespace WalkPattern
    {
        public class MoveToTarget : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Walk");
                _monster.Nma.isStopped = false;
            }

            public override void UpdateState()
            {
                TargetInRangeStateCheck();
                UsePattern();
                if (_monster.MainTarget != null)
                {
                    _monster.Nma.SetDestination(_monster.MainTarget.transform.position);
                    if (Vector3.Distance(_monster.MainTarget.transform.position, _monster.transform.position) < 13)
                    {
                        _monster.CurrentState = new WalkPattern.OrbitAroundPlayer();
                        //만약 13이내에 들어올 경우 MainTarget의 주변을 원을 그리며 도는 class를 하나 만들어줄래?
                        //_monster.CurrentState = new Monster.Attack.StandBy();
                    }
                }
            }
        }

        public class OrbitAroundPlayer : Monster.AttackState
        {
            private float defaultOrbitRadius = 6f;
            private float orbitRadius = 8f; // 플레이어 중심 반지름
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

            public override void OnDamaged(DamageCollider dm)
            {
                base.OnDamaged(dm);
                if (dm.DamageCalculate(_monster) == 0)
                    return;
            }
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


            public override void OnDamaged(DamageCollider dm)
            {
                base.OnDamaged(dm);
                if (dm.DamageCalculate(_monster) == 0)
                    return;
            }
        }

    }

    namespace RunAwayPattern
    {
        public class RunAwayFromTarget : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Walk");
                _monster.Nma.isStopped = false;
            }

            public override void UpdateState()
            {
                TargetInRangeStateCheck();
                UsePattern();
                AvoidObstacle();

                if (_monster.MainTarget != null)
                {
                    if (Vector3.Distance(_monster.MainTarget.transform.position, _monster.transform.position) > 10)
                    {
                        _monster.CurrentState = new Monster.Attack.StandBy();
                    }
                }
            }

            private void AvoidObstacle()
            {
                Vector3 fleeDirection = (_monster.transform.position - _monster.MainTarget.position).normalized;

                Vector3 fleeTarget = _monster.transform.position + (fleeDirection) * 3f;

                // 장애물 회피를 위한 NavMesh 위치 보정
                NavMeshHit hit;
                if (NavMesh.SamplePosition(fleeTarget, out hit, 3f, NavMesh.AllAreas))
                {
                    _monster.Nma.speed = 3f; // 속도 증가
                    _monster.Nma.SetDestination(hit.position);
                }
            }

            public override void ExitState()
            {
                _monster.Nma.speed = 3f;
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
                _monster.Ani.Play("SlashAttack");
                _monster.Nma.isStopped = true;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }
    namespace PoisonPattern
    {
        public class Poison : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Poison");
                _monster.Nma.isStopped = true;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new IdlePattern.Idle();
            }
        }
    }   
    namespace SlashPattern
    {
        public class Slash : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Slash");
                _monster.Nma.isStopped = true;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new IdlePattern.Idle();
            }
        }
    }
    namespace WebPattern
    {
        public class Web : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Web");
                _monster.Nma.isStopped = true;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new IdlePattern.Idle();
            }
        }
    }
    namespace RushPattern
    {
        public class Rush : Monster.AttackState
        {
            Vector3 _rushDirection;
            float _rushDistance;
            float _rushDuration;
            public int count;
            public Rush(int cnt)
            {
                count = cnt;
            }
            public override void EnterState(MonsterController2 beast)
            {
                base.EnterState(beast);
                _monster.Ani.Play("Rush",0,0f);
                _monster.Nma.isStopped = true;
                count--;
                rotationSpeed = 100f;
            }

            public void SetRushDirection()
            {
                _rushDirection = _monster.transform.forward;
                _rushDistance = Vector3.Distance(_monster.transform.position,_monster.MainTarget.position);
                _rushDuration = 0.25f;
            }

            public void StartCorMoveInDistance()
            {
                _monster.StartCoroutine(MoveInDistance(_monster.Nma, _rushDirection, _rushDistance, _rushDuration,_monster.obstacleLayer));
            }

            private IEnumerator MoveInDistance(NavHybridAgent agent, Vector3 direction, float distance, float duration, LayerMask obstacleLayer)
            {
                // 초기 위치와 목표 위치 계산
                Vector3 startPosition = agent.transform.position;
                Vector3 targetPosition = startPosition + new Vector3(direction.normalized.x, 0, direction.normalized.z) * distance;

                // 목표 위치로 Raycast를 실행
                RaycastHit hit;
                if (Physics.Raycast(startPosition, direction.normalized, out hit, distance, obstacleLayer))
                {
                    // 장애물이 감지된 경우 충돌 지점을 목표 위치로 설정
                    targetPosition = hit.point;
                    Debug.Log($"Obstacle detected. Adjusting target position to {targetPosition}");
                }

                float elapsedTime = 0f;

                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;

                    // 현재 위치를 Lerp로 계산
                    Vector3 nextPosition = Vector3.Lerp(
                        new Vector3(startPosition.x, agent.transform.position.y, startPosition.z),
                        new Vector3(targetPosition.x, agent.transform.position.y, targetPosition.z),
                        elapsedTime / duration
                    );

                    // NavMeshAgent의 위치를 갱신
                    agent.Warp(nextPosition);

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


                    // 다음 프레임 대기
                    yield return null;
                }

                // 이동 완료 후 목표 위치로 설정
                if(elapsedTime >= duration)
                agent.Warp(new Vector3(targetPosition.x, agent.transform.position.y, targetPosition.z));
            }

            public override void StateEnd()
            {
                if (count > 0)
                    _monster.CurrentState = new Rush(count);
                else
                    _monster.CurrentState = new RushEnd();
            }
        }

        public class RushEnd : Monster.AttackState
        {
            public override void EnterState(MonsterController2 beast)
            {
                base.EnterState(beast);
                _monster.Nma.isStopped = true;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new IdlePattern.Idle();
            }
        }
    }
    namespace Paze2pattern
    {
        public class Paze2 : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {

                base.EnterState(monster);
                _monster.Ani.Play("Roar");
                _monster.Nma.isStopped = true; 
                Debug.Log("Roar 재생!");
                SpiderController spider;
                spider = _monster as SpiderController;
                //_monster.Patterns.Insert(0, new SpiderPatternsInfo.RushPatternInfo(spider, 50, 3, 15, 3));
                _monster.Patterns.Insert(0, new SpiderPatternsInfo.LayEggPatternInfo(spider, 60f));

            }
            public override void StateEnd()
            {
                _monster.CurrentState = new RushPattern.Rush(3);
            }
        }
    }
    namespace LayEggPattern
    {

        public struct EggData
        {
            public Vector3 pos;
            public SpiderMeleeController.AttackType spiderType;
        }

        public class SetEggPos : Monster.AttackState
        {
            public int pointCount = 4; // 생성할 포인트 개수
            public float radius = 15f; // 중심으로부터 거리 제한
            int neededMeleeSpiderCount=0;
            int neededRangerSpiderCount=0;

            private List<EggData> eggDatas = new List<EggData>();

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Nma.isStopped = false;
                _monster.Nma.speed = 12.5f;
                _monster.Ani.Play("Run");
                SpiderController spiderController = _monster as SpiderController;
                neededMeleeSpiderCount = spiderController.MeleeSpiderMaxCount - spiderController.MeleeSpiders.Count;
                neededRangerSpiderCount = spiderController.rangerSpiderMaxCount- spiderController.RangerSpiders.Count;

                GenerateRandomPoints();
                StateEnd();
            }

            // NavMesh 상에서 랜덤 포인트 생성
            void GenerateRandomPoints()
            {
                eggDatas.Clear();

                for (int i = 0; i < pointCount; i++)
                {
                    Vector3 randomPoint = GetRandomPointOnNavMesh();
                    if (randomPoint != Vector3.zero) // 유효한 포인트만 추가
                    {
                        SpiderController spiderController = _monster as SpiderController;
                        EggData data = new EggData();
                        data.pos = randomPoint;

                        if (neededMeleeSpiderCount > 0)
                        {
                            neededMeleeSpiderCount--;
                            data.spiderType = SpiderMeleeController.AttackType.Melee;
                        }
                        else if (neededRangerSpiderCount > 0)
                        {
                            neededRangerSpiderCount--;
                            data.spiderType = SpiderMeleeController.AttackType.Ranger;
                        }
                        else
                            data.spiderType = SpiderMeleeController.AttackType.Bomb;

                        eggDatas.Add(data);
                    }
                    else
                    {
                        Debug.Log("Failed To Generate Point");
                    }
                }
            }

            // NavMesh 상에서 랜덤한 유효 포인트 가져오기
            Vector3 GetRandomPointOnNavMesh()
            {
                Vector3 randomDirection = Random.insideUnitSphere * radius;
                randomDirection += _monster.transform.position; // 자기 자신을 기준으로 랜덤 위치 지정

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
                {
                    // 자기 자신의 위치에서 샘플된 위치로 Raycast
                    Vector3 startPosition = _monster.transform.position + new Vector3(0,5,0);
                    Vector3 endPosition = hit.position + new Vector3(0, 5, 0);
                    Vector3 direction = (endPosition - startPosition).normalized; // 방향 계산

                    Ray ray = new Ray(startPosition, direction); // Ray를 발사
                    RaycastHit rayHit;
                    float distance = Vector3.Distance(startPosition, endPosition);

                    if (Physics.Raycast(ray, out rayHit, distance, _monster.obstacleLayer))
                    {
                        return rayHit.point; 

                    }
                    else
                    {
                        return hit.position; // 장애물이 없을 경우 해당 위치 반환
                    }
                }

                return Vector3.zero; // 실패 시 기본값 반환
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new MoveToTarget(eggDatas,0);//배열과 인덱스를 넘겨준다
            }
        }

        public class MoveToTarget : Monster.AttackState
        {
            int _index;
            List<EggData> _eggDatas;

            public MoveToTarget(List<EggData> eggDatas, int index)
            {
                _eggDatas = eggDatas;
                _index = index;
            }

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Run");
                _monster.Nma.isStopped = false;
                _monster.Nma.SetDestination(_eggDatas[_index].pos);
                _monster.Nma.speed = 12.5f;

            }

            public override void UpdateState()
            {

                _monster.MainTargetInfo.lastDetactiontime = Time.time;
                if (!_monster.Nma.pathPending && _monster.Nma.remainingDistance <= _monster.Nma.stoppingDistance)
                {
                    StateEnd();
                }
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new LayEgg(_eggDatas,_index);
            }
        }

        public class LayEgg : Monster.AttackState
        {
            List<EggData> _eggDatas;
            int _index;

            public LayEgg(List<EggData> eggDatas, int index)
            {
                _eggDatas = eggDatas;
                _index = index;
            }


            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("LayEgg", 0,0f);
                _monster.Nma.isStopped = false;
                _monster.MainTargetInfo.lastDetactiontime = Time.time;
            }

            public override void StateEnd()
            {
                if (_index + 1 > _eggDatas.Count-1)
                {
                    _monster.Nma.speed = 3;
                    _monster.CurrentState = new Monster.Attack.StandBy();
                }
                else
                {
                    Debug.Log(_index);
                    _monster.CurrentState = new MoveToTarget(_eggDatas, _index + 1);
                }
            }

            public EggData GetEggData()
            {
                return _eggDatas[_index];
            }

        }
    }
    namespace Paze1Pattern
    {
        public class Paze1 : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {

                base.EnterState(monster);
                _monster.Ani.Play("Roar");
                _monster.Nma.isStopped = true;
                Debug.Log("Roar 재생!");

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