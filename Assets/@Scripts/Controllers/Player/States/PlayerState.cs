using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public enum PlayerStateType
    {
        None,
        Idle,
        Move,
        Dash,
        Hurt,
        NuckBack,
        AirBorne,
        Stun,
        Land,
        Sleep,
        Dead,
        Grabbed,
        WakeUp,
        Attack,
        Parry,
        SprintReady,
        Sprint,
        Interaction,
        Building,
        Charge
    }

    public enum PlayerInteractionObjectType
    {
        Default,
        Tent,
        BonFire,
    }



    public abstract class PlayerState
    {
        protected PlayerController _player;
        public bool CanChangeStateByInput = false;//다음 상태로 전환 가능한 상태인지 체크하는 변수
        //ex) 자신이 공격중일 경우 상태전환이 불가하며, 공격이 끝난 후부터 상태 전환이 가능하다.
        public bool CanBufferInput = false;//버퍼에 인풋이 가능한지 체크하는 변수

        public virtual void EnterState(PlayerController player)
        {
            _player = player;
        }

        public virtual void UpdateState() { }
        public virtual void ExitState() { }

        public virtual PlayerStateType GetState() { return PlayerStateType.None; }

        public virtual void HandleOnTriggerEnter(Collider other) { }

        

        public virtual void OnCancel(InputAction.CallbackContext context)
        {
			_player.CurrentState = new PlayerIdleState();
        }

        public virtual void OnDamaged(DamageCollider dm)
        {

            UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
            ui.ShowHurtOverlay();

            _player.Stats.TakeDamage(dm.DamageCalculate(_player));

            float damage = dm.DamageCalculate(_player);
            dm.ShowDamagePopup(damage, _player.transform.position + Vector3.up * 1.5f, true);
            CinemachineShake.Instance.ShakeCamera(5, .15f);


            if (dm.stun.type != StunType.None)
            {
                dm.stun.RemainStunDuration = dm.stun.stunDuration;
                //_player.stun = dm.stun;
                _player.CurrentState = new PlayerStunnedState(dm);
            }


            for (int i = 0; i < dm.statusAffect.Count; i++)
            {
                switch (dm.statusAffect[i].type)
                {
                    case StatusAffectType.Poison:
                        _player.StartPoisonCoroutin(dm.statusAffect[i]);
                        break;

                    case StatusAffectType.Burn:
                        _player.StartBurnCoroutin(dm.statusAffect[i]);
                        break;

                    case StatusAffectType.Slow:
                        _player.StartSlowCoroutin(dm.statusAffect[i]);
                        break;
                }
            }
        }

        public virtual void AnimationEnd() { }


        protected void PlayWeaponAnimation(string animationName, float interpol)
        {
            if (_player.CurrentWeapon != null)
                animationName = _player.CurrentWeapon.WeaponType.ToString() + animationName;

            _player.Ani.CrossFade(animationName, interpol);
        }

        protected void PlayWeaponAnimation(string animationName, int a, float b)
        {
            if (_player.CurrentWeapon != null)
                animationName = _player.CurrentWeapon.WeaponType.ToString() + animationName;

            _player.Ani.Play(animationName, a, b);
        }

        protected void PlayWeaponAnimation(string animationName)
        {
            if (_player.CurrentWeapon != null)
                animationName = _player.CurrentWeapon.WeaponType.ToString() + animationName;

            _player.Ani.Play(animationName);
        }

    }


    public abstract class PlayerDefaultState : PlayerState
    {

        /*
        public virtual void OnMove(InputAction.CallbackContext context) 
        {
            if (!PossibleActions.Contains(nameof(OnMove)))
                return;  // 등록되어 있지 않다면 return

            if (!IsActionPossible())
                return;

            if (_player.Stats.IsHookReady)
                return;

            Vector2 input = context.ReadValue<Vector2>();

            if (input != Vector2.zero)
            {
                _player.CurrentState = new PlayerMovingState();
            }
        }
        */

        public virtual void MoveCheck()
        {
            if (_player.MoveDir != Vector2.zero && _player.InputBuffer.IsBufferEmpty() && CanChangeStateByInput)
            {
                _player.CurrentState = new PlayerMovingState();
            }
        }

        public virtual void OnDash()
        {
            if (_player.CurrentStaminaCount - 1 < 0)
                return;
            _player.UseStamina(1);

            _player.CurrentState = new PlayerDashingState();
        }

        public virtual void OnSprintCancel()
        {
            if(_player.CurrentState.GetState() == PlayerStateType.SprintReady)
                _player.CurrentState = new PlayerIdleState();
        }

        public virtual void OnSkill(SkillInfo2 skill)
        {
            _player.CurrentState = new ComboAttackState(0);
            /*
            if(skill.SkillConditionCheck())
                skill.UseSkill();
            */
        }
        public virtual void OnSkillRelease(SkillInfo2 skill)
        {
        }

        public virtual void OnHook()
        {
            if (_player.Stats.IsHookReady)
            {
                _player.CurrentState = new PlayerHookShotState();
            }
        }
    }

    public abstract class PlayerHookState : PlayerState
    {
        public virtual void OnHookCancel() { }

        public virtual void OnHook()
        {
            if (_player.Stats.IsHookReady)
            {
                _player.CurrentState = new PlayerHookShotState();
            }
        }
    }

    public abstract class PlayerSprintState : PlayerState
    {
        
        public virtual void OnBrake()
        {
            _player.CurrentState = new PlayerBrakeState(14);
        }
    }
}