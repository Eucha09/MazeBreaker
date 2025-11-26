using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.AI;
using static Unity.Burst.Intrinsics.X86;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace SpiderBombAttackPatterns
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
                if (_monster.MainTarget != null)
                {
                    _monster.Nma.SetDestination(_monster.MainTarget.transform.position);
                    if (Vector3.Distance(_monster.MainTarget.transform.position, _monster.transform.position) < 1f)
                    {
                        _monster.CurrentState = new Monster.Attack.StandBy();
                    }
                }
                TargetInRangeStateCheck();
            }
        }
    }

    namespace BombPattern
    {
        public class Bomb : Monster.AttackState
        {
            public override void EnterState(MonsterController2 monster)
            {
                base.EnterState(monster);
                _monster.Ani.Play("Bomb");
                _monster.Nma.isStopped = true;
            }

            public override void StateEnd()
            {
                GameObject.Destroy(_monster.gameObject);
            }
        }
    }
}