using Monster.Search;
using UnityEngine;
using UnityEngine.AI;


namespace Monster.Chase
{
    public class ChaseToTarget : ChaseState
    {
        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.Ani.CrossFade("Walk", 0.05f);
            _monster.Nma.isStopped = false;
            _monster.MoveSpeed =_monster.Nma.speed;
        }

        public override void UpdateState()
        {
            base.UpdateState();
            if (_monster.MainTarget != null)
            {
                ChangeState(new Attack.StandBy());
                return;
            }
            if (!_monster.ChaseTrigger)
            {
                ChangeState(new Search.Idle());
                return;
            }
            if (_monster.roleType == MonsterController2.RoleType.Leader || _monster.roleType == MonsterController2.RoleType.Solo)
            {
                _monster.Nma.SetDestination(_monster.ChasingPos);
                if (!_monster.Nma.pathPending)
                {
                    if (_monster.Nma.remainingDistance <= _monster.Nma.stoppingDistance)
                    {
                        if (!_monster.Nma.hasPath || _monster.Nma.velocity.sqrMagnitude == 0f)
                        {
                            ChangeState(new Idle());
                            return;
                        }
                    }
                }
            }
            else
            {
                //만약 
                float maintainDist = 3;//유지해야하는 거리
                _monster.Nma.SetDestination(_monster.leader.transform.position);
                _monster.Nma.speed = _monster.leader.Nma.speed
                    * (Vector3.Distance(_monster.leader.transform.position, _monster.transform.position) / maintainDist);
            }
        }

        public override void ExitState()
        {
            _monster.Nma.speed = _monster.MoveSpeed;
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

    public class Idle : SearchState
    {
        float _idleStartTime = 0f;
        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _idleStartTime = Time.time;
            if (_monster.Ani != null)
                _monster.Ani.CrossFade("Idle", 0.25f);

            if (_monster.Nma != null)
                _monster.Nma.isStopped = true;
        }

        public override void UpdateState()
        {
            base.UpdateState();
            if (_monster.ChaseTrigger)
            {
                ChangeState(new ChaseToTarget());
                return;
            }

            if (Time.time - _idleStartTime > 3f)
            {
                //마지막 감지지점으로 도착하고나서 3초 이상 지났을 경우 SearchState로 변환
                _monster.ChaseTrigger = false;
                ChangeState(new Search.Idle());
            }
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


}