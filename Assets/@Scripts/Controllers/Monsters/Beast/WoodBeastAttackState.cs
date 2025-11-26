using Monster;
using Monster.Attack;
using System.Collections;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

namespace WoodBeast.Attack
{
    public class Roar : Monster.AttackState
    {
        //JumpAttackAnimation이 끝났는지 체크

        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.Ani.Play("Roar");
            _monster.Nma.isStopped = true;

        }

        public override void StateEnd()
        {
            _monster.CurrentState = new GasiGalle();
        }
    }

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

    }

    public class JumpAttack : Monster.AttackState
    {
        public int count;
        public JumpAttack(int cnt) 
        {

            count = cnt;        
        }
        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            if (count == 2)
            {
                _monster.Ani.Play("Paze2JumpAttack_01");
                count--;
            }
            else if (count == 1)
            {
                _monster.Ani.Play("Paze2JumpAttack_02");
                count--;

            }
            else
            {
                _monster.Ani.Play("JumpAttack");
            }

            _monster.Nma.isStopped = true;
        }

        public override void ExitState()
        {
            base.ExitState();
        }

        public override void StateEnd()
        {
            if (count > 0)
                _monster.CurrentState = new JumpAttack(count);
            else
                _monster.CurrentState = new Monster.Attack.StandBy();
        }
    }
    
    public class DoubleSwing : Monster.AttackState
    {

        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.Ani.Play("DoubleSwing");   
            _monster.Nma.isStopped = true;
            _monster.Rb.isKinematic = false;
        }

        public override void ExitState()
        {
            base.ExitState();
            _monster.Rb.isKinematic = true;
        }

        public override void StateEnd()
        {
            _monster.CurrentState = new Monster.Attack.StandBy();
        }


    }

    public class SingleSwing : Monster.AttackState
    {
        //JumpAttackAnimation이 끝났는지 체크

        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.Ani.Play("SingleSwing");
            _monster.Nma.isStopped = true;

        }

        public override void StateEnd()
        {
            _monster.CurrentState = new Monster.Attack.StandBy();
        }
    }
    public class GasiGalle : Monster.AttackState
    {

        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.Ani.Play("GasiGalleAttack");
            _monster.Nma.isStopped = true;
        }
        public override void StateEnd()
        {
            _monster.CurrentState = new Monster.Attack.StandBy();
        }
    }

    public class LeaveStorm : Monster.AttackState
    {

        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.Ani.Play("LeaveStormAttack");
            _monster.Nma.isStopped = true;
        }

        public override void StateEnd()
        {
            _monster.CurrentState = new Monster.Attack.StandBy();
        }
    }
    public class ThrowRock : Monster.AttackState
    {

        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.Ani.Play("ThrowRock");
            _monster.Nma.isStopped = true;
        }

        public override void StateEnd()
        {
            _monster.CurrentState = new Monster.Attack.StandBy();
        }
    }
    /*public class ThrowRock : Monster.AttackState    돌 두 번 던지기 패턴
    {
        public int count;
        public ThrowRock(int cnt)
        {

            count = cnt;
        }
        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            _monster.Ani.Play("ThrowRockAttack", -1, 0f);
            count--;
            _monster.Nma.isStopped = true;
        }

        public override void StateEnd()
        {
            if (count > 0)
            {
                Debug.Log("한 번더 던져~");
                _monster.CurrentState = new ThrowRock(count);

            }
            else
            {
                Debug.Log("Idle로 가~");

                _monster.CurrentState = new Monster.Attack.StandBy();
            }
        }
    }*/

    public class Paze2 : Monster.AttackState
    {
        public override void EnterState(MonsterController2 monster)
        {
            base.EnterState(monster);
            Debug.Log("Roar 재생!");
            _monster.Ani.Play("Roar");
            _monster.Nma.isStopped = true;


        }

        public override void StateEnd()
        {
            //_monster.CurrentState = new ThrowRock(2);
            _monster.CurrentState = new StandBy();

        }

    }

}