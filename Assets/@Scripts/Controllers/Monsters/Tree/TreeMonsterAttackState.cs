using System.Collections;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;




namespace TreeMonster.Attack
{
    public class Idle : AttackState
    {

        //현재 쿨타임이 돌은 스킬이 있는지 체크
        public override void EnterState(TreeMonsterController monster)
        {
            base.EnterState(monster);
            _monster.Ani.Play("Idle");
        }

        public override void UpdateState()
        {
            base.UpdateState();
            LookPlayer();
            
            TargetInRangeStateCheck();//MainTarget이 없을경우 Search로 전환한다.
            UsePattern();//사용가능한 스킬이 있는지 체크하고 있을 경우 공격
            
        }

        public override void OnDamaged(DamageCollider dm)
        {
            base.OnDamaged(dm);
            _monster.TargetDetectionTime = Time.time;
            if (dm.Caster != null)
                _monster.MainTarget = dm.Caster.transform;
            _monster.CurrentState = new Hurt();
        }


        public override void ExitState()
        {
            base.ExitState();
        }

    }


    public class JumpAttack : AttackState
    {

        public override void EnterState(TreeMonsterController mosnter)
        {
            base.EnterState(mosnter);
            IsAnimationEnd = false;
            _monster.Ani.Play("JumpAttack");
        }

        public override void UpdateState()
        {
            base.UpdateState();
            LookPlayer();

            //애니메이션이 끝났을 경우
            if (IsAnimationEnd)
            {
                _monster.CurrentState = new Idle();
            }
        }

        public override void ExitState()
        {
            base.ExitState();
        }
    }

    public class SpitAttack : AttackState
    {
        public bool _isSpit1CycleEnd;
        int _cnt = 0;
        public SpitAttack(int cnt)
        {
            _cnt = cnt;
        }

        public override void EnterState(TreeMonsterController monster)
        {
            Debug.Log("SpitAttack");
            base.EnterState(monster);
            IsAnimationEnd = false;
            _isSpit1CycleEnd = true;
            //_monster.Ani.Play("SpitAttack");
        }

        public override void UpdateState()
        {
            base.UpdateState();
            LookPlayer();
            //애니메이션이 끝났을 경우
            if (_isSpit1CycleEnd && _cnt > 0)
            {
                _monster.Ani.Play("SpitAttack",-1,0f);
                _isSpit1CycleEnd = false;
                _cnt--;
            }
            if (IsAnimationEnd)
            {
                _monster.CurrentState = new Idle();
            }
        }

        public override void ExitState()
        {
            base.ExitState();
        }
    }

    public class Hurt : AttackState
    {
        //JumpAttackAnimation이 끝났는지 체크

        public override void EnterState(TreeMonsterController monster)
        {
            base.EnterState(monster);
            IsAnimationEnd = false;
            _monster.Ani.Play("Hurt", -1, 0f);
            
        }

        public override void UpdateState()
        {
            base.UpdateState();
            UsePattern();
            //애니메이션이 끝났을 경우
            if (IsAnimationEnd)
            {
                _monster.CurrentState = new Idle();
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
            EnterState(_monster);
        }
    }

}