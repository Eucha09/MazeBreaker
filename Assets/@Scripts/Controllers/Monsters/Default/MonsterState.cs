using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.InputSystem;
using System;
using static MonsterController2;
using Monster.Chase;
using Monster.Attack;
using MonsterPatternsInfo;

namespace Monster
{
    public abstract class State
    {
        protected State _nextState = null;

        public void ChangeState(State nextState)
        {
            if (_nextState != null && _nextState.ToString() != new Monster.Attack.StandBy().ToString())
                return;
            _nextState = nextState;
            _monster.CurrentState = _nextState;
        }


        protected MonsterController2 _monster;

        public virtual void EnterState(MonsterController2 monster)
        {
            _monster = monster;
        }

        public virtual void UpdateState()
        {
            //마지막으로 타겟을 감지한 지점이 
            if (_monster.Hp <= 0)
            {
                ChangeState(new DeadState());
                //_monster.CurrentState = new DeadState();
            }
        }
        public virtual void ExitState() { }


        public virtual void HandleOnTriggerEnter(Collider other) { }

        public virtual void HandleOnColliderEnter(Collision collision) { }

        public virtual void OnDamaged(DamageCollider dm)
        {
            // ✅ 무적 판정
            if (_monster is StoneGolemController golem && golem.IsShielded)
            {
                Debug.Log("돌 골렘 무적 상태! 데미지 무효화됨");
                return;
            }
            CinemachineShake.Instance.ShakeCamera(1.5f, .2f);
            _monster.DamageEffectStart();
            _monster.Hp -= dm.DamageCalculate(_monster);

            // 데미지 계산
            float damageAmount = dm.DamageCalculate(_monster);
            if (dm.IsChargeAttack)
                _monster.Weakness = 0;
            // 팝업 출력
           
            dm.ShowDamagePopup(damageAmount, _monster.transform.position + Vector3.up * 2f,false);  //1.5f


            if (_monster.Hp <= 0)
            {
                ChangeState(new DeadState());
                //_monster.CurrentState = new DeadState();
                return;
            }

            if(dm.Caster != null)
            {
                //만약 기존 타겟에 있을 경우 시간을 늘려주고
                //아닐 경우
                TargetInfo a = _monster.targets.Find(a => a.target == dm.Caster.transform);
                if (a != null)//이미 리스트에 포함되어 있을 경우 마지막 발견시간 갱신
                {
                    a.lastDetactiontime = Time.time;
                }
                else//리스트에 포함되어 있지 않다면 새로 추가
                {
                    a = new TargetInfo
                    {
                        target = dm.Caster.transform,
                        lastDetactiontime = Time.time
                    };
                    _monster.targets.Add(a); // 새로운 타겟 추가
                }
            }

            //만약 공격패턴 쿨타임이 돈게 있거나 공격중일 경우 Hurt 재생 X
            bool attackPatternReady = false;

            foreach (var pattern in _monster.Patterns)
            {
                if (pattern.PatternConditionCheck())
                {
                    attackPatternReady = true; break;
                }
            }

            if ((!attackPatternReady && _monster.CurrentPatternInfo.GetPatternType() != PatternType.Attack)
                && _monster.CurrentPatternInfo.GetPatternType() != PatternType.Groggy)
            {
                ChangeState(new HurtState());
            }

            for (int i = 0; i < dm.statusAffect.Count; i++)
            {
                switch (dm.statusAffect[i].type)
                {
                    case StatusAffectType.Poison:
                        _monster.StartPoisonCoroutin(dm.statusAffect[i]);
                        break;

                    case StatusAffectType.Burn:
                        _monster.StartBurnCoroutin(dm.statusAffect[i]);
                        break;

                    case StatusAffectType.Slow:
                        _monster.StartSlowCoroutin(dm.statusAffect[i]);
                        break;
                }
            }
        }

        public virtual void StateEnd(){ }

    }


    public abstract class SearchState : State
    {
        //ChasingPos가 있을 경우 ChaseState로 넘어간다

        public override void UpdateState()
        {
            base.UpdateState();
        }



        public Vector3 GetRandomPointOnNavMesh(Vector3 center, float range)
        {
            Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, range, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return center; // 실패 시 중심으로 되돌아감
        }
    }

    public abstract class ChaseState : State
    {
        //지정된 위치로 쫓아간다
        //리더나 솔로는 ChasingPos를 향해 쫓아간다. (이 때 리더는 멤버들에게 자신에게 따라오라고 명령한다.)
        //멤버는 리더를 따라간다.
        //만약 ChasingPos를 향해 가던 중 타겟을 발견할 경우 Chase를 중지하고 AttackState로 넘어간다.
        //마지막으로 갱신된 ChasingPos까지 쫓아간 후 타겟이 보이지 않을 경우 추적을 종료하고 SearchState로 전환한다.

        public override void UpdateState()
        {
            base.UpdateState();
        }

        //플레이어를 찾는 로직 작성 필요
        //BoxArea로 Raycast를 쏴서 만약 특정 Layer를 갖고 있는 오브젝트가 감지될경우 가장 가까운 적을 타겟으로 설정


        public Vector3 GetRandomPointOnNavMesh(Vector3 center, float range)
        {
            Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, range, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return center; // 실패 시 중심으로 되돌아감
        }
    }


    public abstract class AttackState : State
    {
        protected PatternInfo _patternInfo;

        public float rotationSpeed = 10;

        public delegate void MyDelegate();
        private MyDelegate _multiDelegate;


        public override void UpdateState()
        {
            base.UpdateState();
            _multiDelegate?.Invoke();// 델릭이트에 등록된 모든 함수 호출
        }

        

        // 특정 함수가 델리게이트에 포함되어 있는지 확인하는 메서드
        private bool DelegateContainsMethod(Delegate del, string methodName)
        {
            foreach (Delegate d in del.GetInvocationList())
            {
                if (d.Method.Name == methodName)
                {
                    return true;
                }
            }
            return false;
        }

        public void TargetCheckStart()
        {
            // 이미 등록된 함수인지 확인
            if (_multiDelegate != null && DelegateContainsMethod(_multiDelegate, nameof(TargetInRangeStateCheck)))
            {
                return;
            }
            _multiDelegate += TargetInRangeStateCheck;
        }

        public void TargetCheckEnd()
        {
            _multiDelegate -= TargetInRangeStateCheck;
        }

        public void TargetInRangeStateCheck()
        {
            if (_monster.MainTarget == null)
            {
                //패턴리스트들을 받아와서 리셋
                foreach (var pattern in _monster.Patterns)
                {
                    if(pattern is ResetPatternData resettable)
                    {
                        resettable.Reset();
                    }
                }
                ChangeState(new Search.Idle());
            }
        }

        public void PatternCheckStart()
        {
            // 이미 등록된 함수인지 확인
            if (_multiDelegate != null && DelegateContainsMethod(_multiDelegate, nameof(UsePattern)))
            {
                return;
            }
            _multiDelegate += UsePattern;
        }

        public void PatternCheckEnd()
        {
            _multiDelegate -= UsePattern;
        }

        //현재 사용 가능한 스킬이 있는지
        public void UsePattern()
        {
            foreach (var pattern in _monster.Patterns)
            {
                if (pattern.PatternConditionCheck() && _nextState == null)
                {
                    pattern.UsePattern();
                    _monster.CurrentPatternInfo = pattern;
                    return;
                }
            }
        }

        public void LookTargetStart()
        {
            // 이미 등록된 함수인지 확인
            if (_multiDelegate != null && DelegateContainsMethod(_multiDelegate, nameof(LookTarget)))
            {
                return;
            }
            _multiDelegate += LookTarget;
        }

        public void LookTargetEnd()
        {
            _multiDelegate -= LookTarget;
        }

        //타겟을 바라보게한다.
        public void LookTarget()
        {
            // 목표(MainTarget) 방향 벡터를 계산합니다.
            Vector3 directionToTarget = _monster.MainTarget.position - _monster.transform.position;

            // 방향 벡터의 y값은 0으로 만들어서 평면에서만 회전하도록 합니다.
            directionToTarget.y = 0;

            // 방향 벡터의 크기를 정규화합니다.
            directionToTarget.Normalize();

            // 현재 오브젝트의 Y축을 목표를 바라보도록 회전 값을 생성합니다.
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

            // 회전을 적용합니다 (부드럽게 Lerp).
            _monster.transform.rotation = Quaternion.Lerp(_monster.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        }

        public float GetDistanceToTarget(Transform target)
        {
            return Vector3.Distance(_monster.transform.position, target.GetComponent<Collider>().ClosestPoint(_monster.transform.position));
        }

        public virtual bool IsObstacleBetween(Transform target, LayerMask obstacleMask)
        {
            Vector3 start = _monster.transform.position;
            Vector3 end = target.GetComponent<Collider>().ClosestPoint(start);
            Vector3 direction = end - start;
            float distance = direction.magnitude;

            // Raycast로 장애물 체크
            if (Physics.Raycast(start, direction.normalized, out RaycastHit hit, distance, obstacleMask))
            {
                return true;
            }

            return false;
        }

    }

    public class GroggyState : State
    {

        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.CurrentPatternInfo = new GroggyPatternInfo();
            _monster.Ani.CrossFade("Groggy", 0.05f);
            _monster.Nma.isStopped = true;
            // 창수(스턴 시 인디케이터 제거, 검토필요)
            if (_monster.currentIndicator != null)
            {
                _monster.currentIndicator.Cancel(0.2f);
                _monster.currentIndicator = null;
            }
        }

        public override void UpdateState()
        {
            if (!_monster.IsGroggy)
                ChangeState(new StandBy());
        }

        public override void ExitState()
        {
        }

        public override void OnDamaged(DamageCollider dm)
        {
            base.OnDamaged(dm);
            _monster.Ani.Play("GroggyHurt", -1,0);
        }
    }

    public class DeadState : State
    {
        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.Ani.CrossFade("Dead", 0.05f);
            _monster.Nma.isStopped = true;
            //_monster.GetComponent<Collider>().enabled = false;
            Collider[]  colliders = _monster.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
            // 창수(스턴 시 인디케이터 제거, 검토필요)
            if (_monster.currentIndicator != null)
            {
                _monster.currentIndicator.Cancel(0.2f);
                _monster.currentIndicator = null;
            }
            _monster.Rb.isKinematic = true;
            _monster.DropItem();
            if(_monster.isBoss)
                Managers.Sound.EndBattle(_monster.gameObject);
            if (_monster.roleType == MonsterController2.RoleType.Leader)
            {
                _monster.DelegateLeaderToMember();
            }
            else if (_monster.roleType == MonsterController2.RoleType.Member)
            {
                _monster.ReportDeathToLeader();
            }

            _monster.ChaseTrigger = false;
            _monster.IsDead = true;
        }

        public override void UpdateState()
        {
            
        }

    }

    public class HurtState : State
    {

        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.Nma.isStopped = true;
            if (_monster.IsGroggy)
            {
                ChangeState(new GroggyState());                //그로기 일 땐 Hurt 재생 X
                return;

            }
            else
                _monster.Ani.Play("Hurt", -1, 0f);

            _monster.CurrentPatternName = " ";
        }

        public override void StateEnd()
        {
                ChangeState(new StandBy());
        }
    }
}