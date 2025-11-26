using MonsterPatternsInfo;
using System.Collections;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

namespace Monster.Attack
{
    public class StandBy : AttackState
    {
        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.CurrentPatternInfo = new PatternInfo();
            _monster.Ani.CrossFade("Standby", 0.25f);
            _monster.Nma.isStopped = true;
            TargetCheckStart();
            LookTargetStart();
            PatternCheckStart();
            _monster.CurrentPatternInfo = new MonsterPatternsInfo.StandByPatternInfo();
            //DistanceFromSpawnPointCheckStart();   //현이가 주석하라고 함. 복귀 안하게 하는거.

        }

        public override void OnDamaged(DamageCollider dm)
        {
            base.OnDamaged(dm);
            if (dm.DamageCalculate(_monster) == 0)
                return;
        }
    }

    namespace MoveToTargetPattern
    {
        
        public class MoveToTarget : AttackState
        {
            MonsterPatternsInfo.MoveToTargetPatternData _patternData;

            public MoveToTarget(MonsterPatternsInfo.MoveToTargetPatternData pd)
            {
                _patternData = pd;
            }

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                Debug.Log(_patternData.AnimationName);
                _monster.Ani.Play(_patternData.AnimationName);
                _monster.Ani.CrossFade(_patternData.AnimationName, .25f);
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
                        if (GetDistanceToTarget(_monster.MainTarget) < _patternData.Dist-1f)
                        {
                            ChangeState(new Monster.Attack.StandBy());
                            return;
                        }

                        /*
                        if (!_monster.Nma.hasPath || _monster.Nma.velocity.sqrMagnitude == 0f)
                        {
                            ChangeState(new Monster.Attack.StandBy());
                            return;
                        }
                        */
                    }
                }
            }
        }
    }

    namespace OrbitMovePattern
    {
        public class OrbitAroundTarget : AttackState
        {
            OrbitMovePatternData _patternData;
            private float navMeshCheckDistance = 2f; // NavMesh 샘플링 거리
            private float lastDirectionChangeTime = 0f;

            private bool isMovingClockwise = true; // 접선 이동 방향 (true: 시계 방향, false: 반시계 방향)

            public OrbitAroundTarget(OrbitMovePatternData pd)
            {
                _patternData = pd;
            }


            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play(_patternData.OrbitMoveAnimationName);
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

                    Vector3 initialTarget = _monster.MainTarget.transform.position + direction * _patternData.OrbitRadius;
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

                float distanceToOrbit = Vector3.Distance(_monster.transform.position, playerPosition + toMonster * _patternData.OrbitRadius);
                Vector3 directTarget = playerPosition + toMonster * _patternData.OrbitRadius;


                // 🛑 전방 장애물 감지 로직 추가 (회피용 Raycast)
                Vector3 forwardDirection = _monster.transform.forward;
                float checkDistance = 2f; // 감지 거리

                if (Physics.Raycast(_monster.transform.position + Vector3.up, forwardDirection, checkDistance, _monster.obstacleLayer))
                {
                    Debug.Log("⚠️ 앞에 장애물 감지! 회피 행동 실행");

                    // 플레이어와 너무 가까운 경우, RunAway 상태로 전환
                    if (Vector3.Distance(_monster.transform.position, playerPosition) < _patternData.OrbitRadius)
                    {
                        ChangeState(new RunAway(_patternData)); // 도망 거리 증가
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
                Vector3 tangentTarget = directTarget + tangent * (_patternData.OrbitRadius - distanceToOrbit) * 0.5f;

                // 거리 비율 계산 (0: 가장 멀리, 1: 가장 가까이)
                float distanceRatio = Mathf.InverseLerp(_patternData.OrbitRadius, 0f, distanceToOrbit);

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
                    _monster.Nma.speed = isReturning ? _patternData.ReturnSpeed : _patternData.OrbitSpeed;
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
            OrbitMovePatternData _patternData;

            public RunAway(OrbitMovePatternData pd)
            {
                _patternData = pd;
            }

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play(_patternData.RunAnimationName);
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

                if (distance < _patternData.RunDist)
                {
                    RunAwayFromTarget();
                }
                else
                {
                    ChangeState( new OrbitAroundTarget(_patternData));
                }
            }

            void RunAwayFromTarget()
            {
                Vector3 fleeDirection = (_monster.transform.position - _monster.MainTarget.position).normalized;
                Vector3 newPos = _monster.transform.position + fleeDirection * _patternData.RunDist;

                // 장애물 감지 (레이캐스트)
                if (Physics.Raycast(_monster.transform.position, fleeDirection, out RaycastHit hit, _patternData.RunDist))
                {
                    // 충돌한 경우 옆 방향으로 도망
                    fleeDirection = Vector3.Cross(fleeDirection, Vector3.up);
                    newPos = _monster.transform.position + fleeDirection * _patternData.RunDist;
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
        }
    }

    namespace AttackPattern
    {


        public class Attack : Monster.AttackState
        {
            AttackPatternData _patternData;
            
            public Attack(AttackPatternData pd)
            {
                _patternData = pd;
            }

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade(_patternData.AnimationName, 0.05f);
                //_monster.Ani.Play(_patternData.AnimationName);
                _monster.Nma.isStopped = true;
            }

            public override void StateEnd()
            {
                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }

    namespace RepeatAttackPattern
    {


        public class RepeatAttack : Monster.AttackState
        {
            int _repeatCount;

            RepeatAttackPatternData _patternData;

            public RepeatAttack(RepeatAttackPatternData pd, int repeatCount)
            {
                _patternData = pd;
                _repeatCount = repeatCount;
            }

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _repeatCount++;
                if (_repeatCount == 1)
                {
                    _monster.Ani.Play(_patternData.FirstAnimationName, -1, 0f);
                }
                else if (_repeatCount == _patternData.RepeatCount)
                {
                    _monster.Ani.Play(_patternData.LastAnimationName, -1, 0f);
                }
                else
                {
                    _monster.Ani.Play(_patternData.RepeatAnimationName, -1, 0f);
                }
                _monster.Nma.isStopped = true;
            }

            public override void StateEnd()
            {
                if(_repeatCount == _patternData.RepeatCount)
                    _monster.CurrentState = new Monster.Attack.StandBy();
                else
                {
                    _monster.CurrentState = new RepeatAttack(_patternData, _repeatCount);
                }
            }
        }
    }

    namespace RunAwayPattern
    {
        public class RunAway : Monster.AttackState
        {
            RunAwayPatternData _patternData;

            public RunAway(RunAwayPatternData patternData)
            {
                _patternData = patternData;
            }

            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.CrossFade(_patternData.RunAnimationName, 0.05f);
                _monster.Nma.isStopped = false;
                _monster.Nma.speed = 4f;
                TargetCheckStart();
                PatternCheckStart();
            }

            public override void UpdateState()
            {

                base.UpdateState();

                if (_monster.MainTarget == null)
                    return;

                float distance = Vector3.Distance(_monster.transform.position, _monster.MainTarget.position);

                if (distance < _patternData.RunEndDist)
                {
                    PlayAnimationOneTime(_patternData.RunAnimationName, 0.05f);
                    RunAwayFromTarget();
                    _monster.Nma.speed = _patternData.RunSpeed;
                }
                else
                {
                    PlayAnimationOneTime(_patternData.WalkAnimationName, 0.05f);
                    RunAwayFromTarget();
                    _monster.Nma.speed = _patternData.WalkSpeed;
                }
            }

            void RunAwayFromTarget()
            {
                Vector3 fleeDirection = (_monster.transform.position - _monster.MainTarget.position).normalized;
                Vector3 newPos = _monster.transform.position + fleeDirection * 3f;

                // 장애물 감지 (레이캐스트)
                if (Physics.Raycast(_monster.transform.position, fleeDirection, out RaycastHit hit, 3f))
                {
                    // 충돌한 경우 옆 방향으로 도망
                    fleeDirection = Vector3.Cross(fleeDirection, Vector3.up);
                    newPos = _monster.transform.position + fleeDirection * 3f;
                }

                NavMeshHit navHit;
                if (NavMesh.SamplePosition(newPos, out navHit, 5.0f, NavMesh.AllAreas))
                {
                    _monster.Nma.SetDestination(navHit.position);
                }
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