using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define;

namespace Player
{
    /*
    public class AxeComboAttackState  : PlayerDefaultState
    {

        AxeComboAttackSkillInfo _skillInfo;

        public AxeComboAttackState(AxeComboAttackSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);


            IsAnimationEnd = false;


            LookAtPos();
            _player.Ani.Play("AxeComboAttack" + _skillInfo.CurrentComboCount.ToString(), -1, 0f);
            _player.Rb.linearVelocity = Vector3.zero;
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
    */
    //인디케이터 없는 V1
    /*public class AxeThrowAttackState : PlayerState, AnimationCheck, AttackDelayCheck
    {
        public AxeThrowSkillInfo _skillInfo;
        public bool IsAnimationEnd { get; set; }
        public bool IsAttackDelayOver { get; set; }


        public AxeThrowAttackState(AxeThrowSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            PossibleActions.Add(nameof(OnDash));
            PossibleActions.Add(nameof(OnSkill));
            IsInputBufferUsing = true;
            IsAnimationEnd = false;
            IsAttackDelayOver = false;

            player.AttackPosition = _skillInfo.AttackPos;


            LookAtPos();
            _player.Ani.Play("AxeThrowSkill", -1, 0f);
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
    }*/
    /*

    public class AxeThrowAttackState : PlayerDefaultState, AnimationCheck
    {
        public AxeThrowSkillInfo _skillInfo;
        public bool IsAnimationEnd { get; set; }
        public bool CanRotate { get; set; } = true;

        GameObject _indicator;

        public AxeThrowAttackState(AxeThrowSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            IsAnimationEnd = false;

            // ✅ 첫 위치 보정 & 인디케이터 생성
            Vector3 initialPosition = GetClampedMousePosition();
            _skillInfo.AttackPos = initialPosition;

            _indicator = Managers.Resource.Instantiate(_player.EffectPath + "AxeThrowIndicator", initialPosition, Quaternion.identity);

            _player.Ani.Play("AxeThrowSkill", -1, 0f);
        }

        public override void UpdateState()
        {
            if (CanRotate)
            {
                RotateTowardsMouse();
                if (_indicator != null)
                    _indicator.transform.position = _skillInfo.AttackPos;
            }


            MoveCheck();

            if (IsAnimationEnd)
                _player.CurrentState = new PlayerIdleState();
        }

        private void RotateTowardsMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane ground = new Plane(Vector3.up, _player.transform.position);
            if (ground.Raycast(ray, out float hit))
            {
                Vector3 mouseWorld = ray.GetPoint(hit);
                Vector3 dir = (mouseWorld - _player.transform.position);
                float distance = dir.magnitude;

                if (distance > 10f)
                    dir = dir.normalized * 10f;

                Vector3 clampedPosition = _player.transform.position + dir;
                _skillInfo.AttackPos = clampedPosition;

                if (dir != Vector3.zero)
                {
                    Quaternion rot = Quaternion.LookRotation(dir);
                    _player.Rb.rotation = rot;
                }
            }
        }

        public void LockIndicatorPosition()
        {
            CanRotate = false;
        }

        public void DestroyIndicator()
        {
            if (_indicator != null)
            {
                Managers.Resource.Destroy(_indicator);
                _indicator = null;
            }
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Attack;
        }


        private Vector3 GetClampedMousePosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane ground = new Plane(Vector3.up, _player.transform.position);
            if (ground.Raycast(ray, out float hit))
            {
                Vector3 mouseWorld = ray.GetPoint(hit);
                Vector3 dir = (mouseWorld - _player.transform.position);
                float distance = dir.magnitude;

                if (distance > 10f)
                    dir = dir.normalized * 10f;

                return _player.transform.position + dir;
            }

            return _player.transform.position;
        }
    }
    

    public class AxeSlashUpAttackState : PlayerDefaultState, AnimationCheck
    {
        AxeSlashUpSkillInfo _skillInfo;
        public bool IsAnimationEnd { get; set; }

        public bool isAxeSlashDownAttackReady = false;


        public AxeSlashUpAttackState(AxeSlashUpSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void OnDamaged(DamageCollider dm)
        {

        }
        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            IsAnimationEnd = false;

            player.AttackPosition = _skillInfo.AttackPos;
            isAxeSlashDownAttackReady = false;


            LookAtPos();
            _player.Ani.Play("AxeSlashUpSkill", -1, 0f);
        }

        public override void UpdateState()
        {

            MoveCheck();
            if (IsAnimationEnd)
                _player.CurrentState = new PlayerIdleState();
        }

        public void AxeSlashDownAttackExecute()
        {
            if (isAxeSlashDownAttackReady)
            {
                new AxeSlashDownSkillInfo(_player).UseSkill();
            }
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

    public class AxeSlashDownAttackState : PlayerDefaultState, AnimationCheck
    {
        AxeSlashDownSkillInfo _skillInfo;
        public bool IsAnimationEnd { get; set; }
        public bool IsAttackDelayOver { get; set; }


        public AxeSlashDownAttackState(AxeSlashDownSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void OnDamaged(DamageCollider dm)
        {

        }
        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            IsAnimationEnd = false;
            IsAttackDelayOver = false;

            player.AttackPosition = _skillInfo.AttackPos;


            LookAtPos();
            _player.Ani.Play("AxeSlashDownSkill", -1, 0f);
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

    public class AxeSpinAttackState : PlayerDefaultState
    {
        AxeSpinSkillInfo _skillInfo;
        GameObject _spinSkillEffect;
        float spinStartTime;

        public AxeSpinAttackState(AxeSpinSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);


            spinStartTime= Time.time;
            _player.Ani.Play("AxeSpinSkill", -1, 0f);
            _spinSkillEffect = Managers.Resource.Instantiate(_player.EffectPath + "AxeSpinSkill", _player.transform.position, _player.transform.rotation);
            _spinSkillEffect.GetComponentInChildren<DamageOverTimeDamageCollider>().Init(_player, 5, 0.2f);
            _spinSkillEffect.GetComponentInChildren<DamageOverTimeDamageCollider>().DamageOverTimeStart();
            _spinSkillEffect.transform.SetParent(_player.transform);
            _player.Rb.linearVelocity = Vector3.zero;
        }

        public override void UpdateState()
        {


            if (Time.time - spinStartTime > _skillInfo.SpinDuration)
            {
                _player.CurrentState = new PlayerIdleState();
                //_skillUser.Ani.CrossFade("SpinDone",0.05f);
            }

            if (Time.time - spinStartTime > _skillInfo.SpinDuration - 0.2f)
            {
                CanBufferInput = true;
            }

            Vector3 moveDirection = new Vector3(_player.MoveDir.x, 0f, _player.MoveDir.y).normalized;

            moveDirection = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * moveDirection;
            moveDirection.y = 0;

            _player.Rb.linearVelocity = moveDirection * _skillInfo.SpinMoveSpeed + Vector3.up * _player.Rb.linearVelocity.y;
        }

        public override void ExitState()
        {
            GameObject.Destroy(_spinSkillEffect);
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Attack;
        }

    }
    */


}