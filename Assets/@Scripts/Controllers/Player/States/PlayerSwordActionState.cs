using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define;
using System.Collections;

namespace Player
{
    /*
    public class SwordComboAttackState : PlayerDefaultState, AnimationCheck
    {
        public bool IsAnimationEnd { get; set; }

        SwordComboAttackSkillInfo _skillInfo;

        public SwordComboAttackState(SwordComboAttackSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);



            IsAnimationEnd = false;


            LookAtPos();
            _player.Ani.Play("SwordComboAttack" + _skillInfo.CurrentComboCount.ToString(), -1, 0f);
        }

        public override void UpdateState()
        {
            MoveCheck();

            if (IsAnimationEnd)
            {
                _player.CurrentState = new PlayerIdleState();
            }
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Attack;
        }

        private void LookAtPos()
        {
            Vector3 direction = _skillInfo.AttackPos - _player.transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _player.Rb.rotation = targetRotation;
            }
        }

    }

    public class SwordPrickAttackState : PlayerDefaultState, AnimationCheck
    {
        SwordPrickSkillInfo _skillInfo;
        public bool IsAnimationEnd { get; set; }

        public SwordPrickAttackState(SwordPrickSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            IsAnimationEnd = false;



            LookAtPos();
            _player.Ani.Play("SwordPrickSkill", -1, 0f);
        }

        public override void UpdateState()
        {
            MoveCheck();
            if (IsAnimationEnd)
                _player.CurrentState = new PlayerIdleState();
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Attack;
        }

        private void LookAtPos()  //마우스 방향대로 공격
        {
            Vector3 direction = _skillInfo.AttackPos - _player.transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _player.Rb.rotation = targetRotation;
            }
        }
    }
    public class SwordGroundAttackState : PlayerDefaultState, AnimationCheck
    {
        SwordGroundSkillInfo _skillInfo;
        public bool IsAnimationEnd { get; set; }

        public SwordGroundAttackState(SwordGroundSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            IsAnimationEnd = false;

            if(_player.PreviousState.GetState() == PlayerStateType.Dash || _player.PreviousState.GetState() == PlayerStateType.Move)
            _player.StartCoroutine(AddForce(.5f));


            LookAtPos();
            _player.Ani.Play("SwordGroundSkill", -1, 0f);
        }

        private IEnumerator AddForce(float duration)
        {
            Debug.Log("됐습니다~~");
            float elapsedTime = 0f;
            Vector3 moveDirection = (_skillInfo.AttackPos-_player.transform.position).normalized * 6f; // 바라보는 방향으로 이동

            while (elapsedTime < duration)
            {
                _player.Rb.linearVelocity = moveDirection; // 지속적으로 velocity 적용
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _player.Rb.linearVelocity = Vector3.zero; // 이동 종료 후 멈추기
        }
        public override void UpdateState()
        {
            MoveCheck();
            if (IsAnimationEnd)
                _player.CurrentState = new PlayerIdleState();
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Attack;
        }


        private void LookAtPos()  //마우스 방향대로 공격
        {
            Vector3 direction = _skillInfo.AttackPos - _player.transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _player.Rb.rotation = targetRotation;
            }
        }
    }
    */

    /* 스킬 커슈텀 이슈 폐기
     public class SwordCounterAttackState : PlayerState, AnimationCheck, AttackDelayCheck
    {
        public bool IsAnimationEnd { get; set; }
        public bool IsAttackDelayOver { get; set; }
        Vector3 _attackPos;

        //
        public SwordCounterAttackState(Vector3 attackPos)
        {
            _attackPos = attackPos;
        }


        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            PossibleActions.Add(nameof(OnDash));
            PossibleActions.Add(nameof(OnSkill));
            IsInputBufferUsing = false;
            IsAnimationEnd = false;
            IsAttackDelayOver = false;



            LookAtPos();
            _player.Ani.Play("SwordPrickSkill", -1, 0f);
        }

        public override void UpdateState()
        {
            if (BufferExecute())
                return;

            OnMove();

            if (IsAnimationEnd)
                _player.CurrentState = new PlayerIdleState();
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Attack;
        }

        public override bool IsActionPossible()
        {
            return IsAttackDelayOver;
        }

        public override void OnDamaged(DamageCollider dm)
        {
            if (IsActionPossible())
            {
                base.OnDamaged(dm);
            }
        }

        private void LookAtPos()  //마우스 방향대로 공격
        {
            Vector3 direction = _attackPos - _player.transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _player.Rb.rotation = targetRotation;
            }
        }

    }*/
}