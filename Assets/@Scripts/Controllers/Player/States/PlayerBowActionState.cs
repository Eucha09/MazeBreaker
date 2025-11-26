using StylizedGrassDemo;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define;

namespace Player
{
   
    /*
    public class PlayerBowChargeState : PlayerDefaultState
    {
        BowChargeShotSkillInfo _skillInfo;

        public PlayerBowChargeState(BowChargeShotSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }


        public override void EnterState(PlayerController player)
        {
            //Camera.main.GetComponent<CameraController>().ZoomOutActionCoroutin(1.0f, 1f);


            base.EnterState(player);
            _player.Rb.linearVelocity = Vector3.zero;
            if (_skillInfo.SlidingVector != Vector3.zero)
                _player.Rb.AddForce(_skillInfo.SlidingVector * _skillInfo.SlidingForce, ForceMode.Impulse);
            
            _player.Ani.Play("BowCharge");
            if (_player.CurrentWeaponModel.GetComponent<Animator>() != null)
                _player.CurrentWeaponModel.GetComponent<Animator>().Play("BowCharge");
            
            if (_player.ArrowIndicator == null)
            {
                _player.ArrowIndicator = Managers.Resource.Instantiate(_player.EffectPath + "ArrowIndicator", _player.transform);
                _player.ArrowIndicator.transform.localPosition = Vector3.zero;
            }
        }

        public override void UpdateState()
        {
            _skillInfo.ArrowDistCalculate();


            if (_player.GetComponent<PlayerInput>().currentControlScheme == "Gamepad")
            {
                _skillInfo.GetAttackPos();

                Vector3 targetPosition = _skillInfo.AttackPos;

                Vector3 direction = targetPosition - _player.transform.position;
                direction.y = 0;

                _skillInfo.ArrowDirection = direction;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    _player.Rb.rotation = targetRotation;
                }
                _player.ArrowIndicator.GetComponent<SpriteRenderer>().size = new Vector2(_player.ArrowIndicator.GetComponent<SpriteRenderer>().size.x, _skillInfo.CurrentArrowDist);
                _player.ArrowIndicator.SetActive(true);
            }
            else
            {

                // 留덉슦???꾩튂瑜?媛?몄샃?덈떎.
                Vector3 mousePosition = Input.mousePosition;

                // 留덉슦???꾩튂瑜??붾뱶 醫뚰몴濡?蹂?섑빀?덈떎.
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                Plane groundPlane = new Plane(Vector3.up, new Vector3(_player.transform.position.x, _player.transform.position.y, _player.transform.position.z)); // Y異뺤쓣 湲곗??쇰줈 ?섎뒗 ?됰㈃???뺤쓽?⑸땲??

                float rayLength;
                if (groundPlane.Raycast(ray, out rayLength))
                {

                    Vector3 targetPosition = ray.GetPoint(rayLength);

                    // ?ㅻ툕?앺듃媛 targetPosition???ν븯?꾨줉 ?뚯쟾?⑸땲??
                    Vector3 direction = targetPosition - _player.transform.position;
                    direction.y = 0; // Y異뺤? 怨좎젙?⑸땲??

                    _skillInfo.ArrowDirection = direction;

                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        _player.Rb.rotation = targetRotation;
                        //_player.transform.rotation = Quaternion.Lerp(_player.transform.rotation, targetRotation, Time.deltaTime * 10f); // 遺?쒕읇寃??뚯쟾?⑸땲??
                    }
                    //_player.ArrowIndicator.GetComponent<ArrowLineDrawer>().DrawLine(_player.transform.position,direction, _arrowDist);
                    _player.ArrowIndicator.GetComponent<SpriteRenderer>().size = new Vector2(_player.ArrowIndicator.GetComponent<SpriteRenderer>().size.x, _skillInfo.CurrentArrowDist);
                    _player.ArrowIndicator.SetActive(true);
                }
            }
        }

        public override void ExitState()
        {
            //Camera.main.GetComponent<CameraController>().ZoomInActionCoroutin(0.05f);
            if (_player.ArrowIndicator != null)
            {
                _player.ArrowIndicator.SetActive(false);
            }
            _player.CurrentWeaponModel.GetComponent<Animator>().Play("BowRun");
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Attack;
        }

        public override void OnSkillRelease(SkillInfo2 skill)
        {
            if (skill as BowChargeShotSkillInfo != null)
            {
                _player.CurrentState = new PlayerBowShotState(_skillInfo);
            }
        }

    }
    */
    
    /*

    public class PlayerBowShotState : PlayerDefaultState, AnimationCheck
    {
        BowChargeShotSkillInfo _skillInfo;

        public bool IsAnimationEnd { get; set; }


        public PlayerBowShotState(){}

        public PlayerBowShotState(BowChargeShotSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            


            base.EnterState(player);

            IsAnimationEnd = false;


            GameObject a = Managers.Resource.Instantiate(_player.EffectPath + "Arrow", new Vector3(_player.transform.position.x, _player.transform.position.y + .5f, _player.transform.position.z), Quaternion.LookRotation(_skillInfo.ArrowDirection));
            float damagePercentage = Mathf.Lerp(0.5f, 2f, _skillInfo.CurrentArrowDist / _skillInfo.ArrowMaxDist);
            a.GetComponentInChildren<ProjectileDamageCollider>().Init(_player, damagePercentage);
            a.GetComponentInChildren<BowChargeShotSkill>().Init(_skillInfo.CurrentArrowDist);
            a.GetComponentInChildren<Rigidbody>().AddForce(a.transform.forward * 40f, ForceMode.Impulse);

            _player.Ani.Play("BowShot",-1,0f);
            if (_player.CurrentWeaponModel.GetComponent<Animator>() != null)
                _player.CurrentWeaponModel.GetComponent<Animator>().Play("BowShot",-1,0f);
        }

        public override void UpdateState()
        {
            MoveCheck();
            if (IsAnimationEnd)
                _player.CurrentState = new PlayerIdleState();
        }

        public override void ExitState()
        {
            if (_player.CurrentWeaponModel == null)
                return;
            if (_player.CurrentWeaponModel.GetComponent<Animator>() != null && _player.CurrentWeaponModel != null)
                _player.CurrentWeaponModel.GetComponent<Animator>().Play("BowRun");
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Attack;
        }


    }


    public class BowOneShotAttackState : PlayerDefaultState, AnimationCheck
    {
        GameObject _indicator;

        BowOneShotSkillInfo _skillInfo;
        public BowOneShotSkillInfo SkillInfo {  get { return _skillInfo; } private set { } }
        public bool IsAnimationEnd { get; set; }
        public bool CanRotate { get; set; } = true;

        public BowOneShotAttackState(BowOneShotSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            IsAnimationEnd = false;

            if (_skillInfo.SlidingVector != Vector3.zero)
                _player.Rb.AddForce(_skillInfo.SlidingVector * _skillInfo.SlidingForce, ForceMode.Impulse);
            Vector3 initialPos = GetClampedMousePosition(); // [추가]
            _skillInfo.AttackPos = initialPos;              // [추가]

            _indicator = Managers.Resource.Instantiate(_player.EffectPath + "BowOneShotIndicator", initialPos, Quaternion.identity); // [추가]
            //LookAtPos();
            _player.Ani.Play("BowOneShotSkill", -1, 0f);
            if (_player.CurrentWeaponModel.GetComponent<Animator>() != null)
                _player.CurrentWeaponModel.GetComponent<Animator>().Play("BowOneShotSkill", -1, 0f);
        }

        public override void UpdateState()
        {
            if (CanRotate) // [추가]
            {
                if (_player.GetComponent<PlayerInput>().currentControlScheme == "Gamepad")
                {
                    _skillInfo.GetAttackPos();
                    Vector3 targetPosition = _skillInfo.AttackPos;

                    Vector3 direction = targetPosition - _player.transform.position;
                    direction.y = 0;

                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        _player.Rb.rotation = targetRotation;
                    }
                }
                else
                    RotateTowardsMouse();
                if (_indicator != null)
                    _indicator.transform.position = _skillInfo.AttackPos;
            }
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
        // [수정] 마우스 위치 기준으로 방향 회전
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

        // [추가] 마우스 위치 최대 거리 제한 보정 함수
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

        // [추가] 잠금 시 인디케이터 삭제
        public void LockIndicatorPosition()
        {
            CanRotate = false;
            DestroyIndicator();
        }

        // [추가] 인디케이터 제거 함수
        public void DestroyIndicator()
        {
            if (_indicator != null)
            {
                Managers.Resource.Destroy(_indicator);
                _indicator = null;
            }
        }
    }
    public class BowArrowRainAttackState : PlayerDefaultState, AnimationCheck
    {
        public BowArrowRainSkillInfo _skillInfo;
        public BowArrowRainSkillInfo SkillInfo { get { return _skillInfo; } private set { } }
        public bool IsAnimationEnd { get; set; }
        public bool CanRotate { get; set; } = true; //브이라이징식 조절
        GameObject _indicator;

        public bool CanUpdateIndicator { get; set; } = true;


        public BowArrowRainAttackState(BowArrowRainSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            IsAnimationEnd = false;

            if (_skillInfo.SlidingVector != Vector3.zero)
                _player.Rb.AddForce(_skillInfo.SlidingVector * _skillInfo.SlidingForce, ForceMode.Impulse);


            LookAtPos();
            _player.Ani.Play("BowArrowRainSkill", -1, 0f);
            //브이라이징식 조절
            _indicator = Managers.Resource.Instantiate(_player.EffectPath + "ArrowRainIndicator", _player.transform.position, Quaternion.identity);

            if (_player.CurrentWeaponModel.GetComponent<Animator>() != null)
                _player.CurrentWeaponModel.GetComponent<Animator>().Play("BowArrowRainSkill", -1, 0f);
        }

        public override void UpdateState()
        {
            if (CanRotate)
            {
                if (_player.GetComponent<PlayerInput>().currentControlScheme == "Gamepad")
                    _skillInfo.GetAttackPos();
                else
                    RotateTowardsMouse(); // 회전 & 공격 방향 갱신
                // 인디케이터도 따라 움직임
                if (_indicator != null)
                {
                    _indicator.transform.position = _skillInfo.AttackPos;
                }
            }
            if (CanUpdateIndicator && _indicator != null)
                _indicator.transform.position = _skillInfo.AttackPos;

            MoveCheck();
            if (IsAnimationEnd)
            {
                _player.CurrentState = new PlayerIdleState();

            }
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

                // 회전 처리
                if (dir != Vector3.zero)
                {
                    Quaternion rot = Quaternion.LookRotation(dir);
                    _player.Rb.rotation = rot;
                }

                // 스킬 위치 및 인디케이터 위치 갱신
                _skillInfo.AttackPos = clampedPosition;
                if (_indicator != null)
                {
                    _indicator.transform.position = clampedPosition;
                }
            }
        }
        public void DestroyIndicator()
        {
            if (_indicator != null)
            {
                Managers.Resource.Destroy(_indicator);
                _indicator = null;
            }
        }
        public void LockIndicatorPosition()
        {
            CanUpdateIndicator = false;
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
   

    /* 스킬 커스텀 이슈로 폐기
    public class BowCounterAttackState : PlayerState, AnimationCheck, AttackDelayCheck
    {
        public bool IsAnimationEnd { get; set; }
        public bool IsAttackDelayOver { get; set; }
        Vector3 _attackPos;

        //
        public BowCounterAttackState(Vector3 attackPos)
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
            _player.Ani.Play("BowCounterAttackSkill", -1, 0f);
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

    } */
    }