using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define;

namespace Player
{

    public class PlayerSprintReadyState : PlayerDefaultState
    {
        float _currentSpeed;
        public override void EnterState(PlayerController player)
        {

            base.EnterState(player);
            CanChangeStateByInput = true;
            PlayWeaponAnimation("Run", 0.05f);
            _currentSpeed = _player.Stats.MoveSpeed;
            TrailRenderer trailSystem = _player.SprintTrail.GetComponentInChildren<TrailRenderer>();
            trailSystem.time = 1f;
        }

        public override void UpdateState()
        {
            if (!_player.IsSprintPossible)
            {
                _player.CurrentState = new PlayerIdleState();
                return;
            }




            _currentSpeed += Time.deltaTime * 5f;
            _player.Ani.speed = _currentSpeed / _player.Stats.MoveSpeed;

            if (_currentSpeed >= 14f)
                _player.CurrentState = new PlayerSprintingState();


            Vector3 moveDirection = new Vector3(_player.MoveDir.x, 0f, _player.MoveDir.y).normalized;

            moveDirection = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * moveDirection;
            moveDirection.y = 0;

            _player.Rb.linearVelocity = _player.transform.forward * _currentSpeed + Vector3.up * _player.Rb.linearVelocity.y;

            if (moveDirection != Vector3.zero)
            {
                // ?꾩옱 ?뚯쟾 媛믨낵 紐⑺몴 ?뚯쟾 媛??ъ씠瑜?泥쒖쿇???뚯쟾
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                _player.Rb.rotation = Quaternion.Lerp(_player.Rb.rotation, targetRotation, Time.deltaTime * 2f);
            }
        }

        public override void ExitState()
        {
            _player.Ani.speed = 1;
            _player.Rb.linearVelocity = Vector3.zero;
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.SprintReady;
        }


        /*
        public override void OnSprintCancel(InputAction.CallbackContext context)
        {
            TrailRenderer trailSystem = _player.SprintTrail.GetComponentInChildren<TrailRenderer>();
            trailSystem.time = 0f;
            _player.CurrentState = new PlayerIdleState();
            //_player.CurrentState = new PlayerBrakeState(_player.Stats.MoveSpeed);
        }

        public override void OnSwordAttack(InputAction.CallbackContext context)
        {
            if (_player.Stats.IsHookReady)
                return;
            _player.CurrentState = new PlayerSwordAttackState();
        }
        public override void OnBlock(InputAction.CallbackContext context)
        {
            _player.CurrentState = new PlayerBlockingState();

        }

        public override void OnPickAttack(InputAction.CallbackContext context)
        {
            if (_player.Stats.IsHookReady)
                return;
            _player.CurrentState = new PlayerPickAttackState();
        }

        public override void OnAxeAttack(InputAction.CallbackContext context)
        {
            if (_player.Stats.IsHookReady)
                return;
            _player.CurrentState = new PlayerAxeAttackState();
        }

        public override void OnBowCharge(InputAction.CallbackContext context)
        {
            if (_player.Stats.IsHookReady)
                return;

            _player.CurrentState = new PlayerBowChargeState(_player.transform.forward, 5f);
        }

        */



    }

    public class PlayerSprintingState : PlayerSprintState
    {

        public override void EnterState(PlayerController player)
        {

            base.EnterState(player);
            CanChangeStateByInput = true;
            _player.Ani.CrossFade("FastRun", 0.05f);
            Managers.Event.PlayerEvents.OnPlayerSprint();
		}

        public override void UpdateState()
        {
            if (!_player.IsSprintPossible)
            {
                _player.CurrentState = new PlayerBrakeState(14);
                return;
            }



            Vector3 moveDirection = new Vector3(_player.MoveDir.x, 0f, _player.MoveDir.y).normalized;

            moveDirection = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * moveDirection;
            moveDirection.y = 0;

            _player.Rb.linearVelocity = _player.transform.forward * 14f + Vector3.up * _player.Rb.linearVelocity.y;

            if (moveDirection != Vector3.zero)
            {
                // ?꾩옱 ?뚯쟾 媛믨낵 紐⑺몴 ?뚯쟾 媛??ъ씠瑜?泥쒖쿇???뚯쟾
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                _player.Rb.rotation = Quaternion.Lerp(_player.Rb.rotation, targetRotation, Time.deltaTime * 2f);
            }
        }

        public override void ExitState()
        {
            //_player.Rb.linearVelocity = Vector3.zero;
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Sprint;
        }



        /*
        public override void OnBrake(InputAction.CallbackContext context)
        {
            _player.CurrentState = new PlayerBrakeState(14);
            //_player.CurrentState = new PlayerBrakeState(_player.Stats.MoveSpeed);
        }

        public override void OnSwordAttack(InputAction.CallbackContext context)
        {
            if (_player.Stats.IsHookReady)
                return;
            _player.CurrentState = new PlayerSwordAttackState();
        }

        public override void OnPickAttack(InputAction.CallbackContext context)
        {
            if (_player.Stats.IsHookReady)
                return;
            _player.CurrentState = new PlayerPickAttackState();
        }

        public override void OnAxeAttack(InputAction.CallbackContext context)
        {
            if (_player.Stats.IsHookReady)
                return;
            _player.CurrentState = new PlayerAxeAttackState();
        }

        public override void OnBowCharge(InputAction.CallbackContext context)
        {
            if (_player.Stats.IsHookReady)
                return;

            _player.CurrentState = new PlayerBowChargeState(_player.transform.forward, 10f);
        }

        public override void OnBlock(InputAction.CallbackContext context)
        {
            _player.CurrentState = new PlayerBlockingState();

        }
        */


    }

    public class PlayerBrakeState : PlayerState
    {
        float _playerSprintSpeed;
        public PlayerBrakeState(float playerSprintSpeed)
        {
            _playerSprintSpeed = playerSprintSpeed;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            _player.Ani.Play("Brake");
            TrailRenderer trailSystem = _player.SprintTrail.GetComponentInChildren<TrailRenderer>();
            trailSystem.time = 0f;
        }

        public override void UpdateState()
        {
            _playerSprintSpeed -= 20f * Time.deltaTime;
            _player.Rb.linearVelocity = _player.transform.forward * _playerSprintSpeed + Vector3.up * _player.Rb.linearVelocity.y;
            if (_playerSprintSpeed <= 0.2f)
            {
                _player.CurrentState = new PlayerIdleState();
            }
        }
    }

}