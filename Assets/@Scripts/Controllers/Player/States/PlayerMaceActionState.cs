using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define;
using System.Collections;
using Unity.VisualScripting;

namespace Player
{
    public class ComboAttackState : PlayerDefaultState
    {
        int _comboCount;
        bool _isTrackingPointer;

        // ===== [추가] 인디케이터 관련 =====
        GameObject _bowIndicatorGO;
        BowComboIndicator _bowIndicator;
        const float BOW_INDICATOR_MAX_DISTANCE = 10f;          // 필요시 무기 데이터로 교체
        const string BOW_INDICATOR_PREFAB_PATH = "UI/BowIndicator";

        public ComboAttackState(int comboCount)
        {
            _comboCount = comboCount;
            _isTrackingPointer = true;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            _player.UseStamina(0);
            if (_player.CurrentWeapon == null)
            {
                _player.Ani.Play("Fist" + "ComboAttack" + (_comboCount + 1).ToString(), -1, 0f);
            }
            else if(_player.CurrentWeapon.WeaponType == WeaponType.Bow)
            {
                _player.Ani.Play(_player.CurrentWeapon.WeaponType.ToString() + "ComboAttack" + (_comboCount + 1).ToString(), -1, 0f);
                if (_player.CurrentWeaponModel.GetComponent<Animator>() != null)
                    _player.CurrentWeaponModel.GetComponent<Animator>().Play("BowComboAttack", -1, 0f);

                // ===== [추가] 인디케이터 생성 & 표시 =====
                _bowIndicatorGO = Managers.Resource.Instantiate(BOW_INDICATOR_PREFAB_PATH);
                if (_bowIndicatorGO != null)
                {
                    _bowIndicator = _bowIndicatorGO.GetComponent<BowComboIndicator>();
                    if (_bowIndicator != null)
                    {
                        _bowIndicator.SetMaxDistance(BOW_INDICATOR_MAX_DISTANCE);
                        _bowIndicator.Show(true);
                    }
                }
            }
            else
            {
                _player.Ani.Play(_player.CurrentWeapon.WeaponType.ToString() + "ComboAttack" + (_comboCount + 1).ToString(), -1, 0f);
            }

            
            //MaceComboAttack1
        }

        public override void UpdateState()
        {
            //공격이 나가기 전까지는 마우스를 따라간다.
            if (_isTrackingPointer)
            {
                Vector3 mousePosition = Input.mousePosition;

                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                Plane groundPlane = new Plane(Vector3.up, new Vector3(_player.transform.position.x, _player.transform.position.y, _player.transform.position.z)); // Y異뺤쓣 湲곗??쇰줈 ?섎뒗 ?됰㈃???뺤쓽?⑸땲??

                float rayLength;
                if (groundPlane.Raycast(ray, out rayLength))
                {

                    Vector3 targetPosition = ray.GetPoint(rayLength);

                    Vector3 direction = targetPosition - _player.transform.position;
                    direction.y = 0;


                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        _player.Rb.rotation = targetRotation;
                    }

                    // ===== [추가] 인디케이터 라인 업데이트 =====
                    if (_bowIndicator != null)
                    {
                        _bowIndicator.UpdateLine(_player.transform.position, ray, _player.transform.position.y);
                    }
                }
            }

            if(CanChangeStateByInput && _player.IsDefaultSkillHovering && _player.CurrentStaminaCount >= 2)
            {
                _comboCount++;
                if (_comboCount > 2)
                    _comboCount = 0;
                _player.InputBuffer._bufferedInputData = null;
                _player.CurrentState = new ComboAttackState(_comboCount);
                return;
            }

            if(_player.InputBuffer.IsBufferEmpty())
                MoveCheck();

        }

        //애니메이션 이벤트로 달아줘야함
        public void ChargeCheck()
        {
            if (_player.IsDefaultSkillHovering && _player.DefaultSkillHoveringTime > 0.1f && _player.CurrentStaminaCount >= 2 && _player.CurrentWeapon != null)
            {
                // ✅ 차징 진입 전에 '일반 인디케이터' 정리
                CleanupBowIndicator();
                _player.CurrentState = new ChargeState(_comboCount);
            }
        }

        public void PointerTrackingEnd()
        {
            _isTrackingPointer = false;

        }
        // ===== [추가] 공통 정리 함수 =====
        public void CleanupBowIndicator()
        {
            if (_bowIndicator != null)
            {
                _bowIndicator.Show(false);
                _bowIndicator = null;
            }
            if (_bowIndicatorGO != null)
            {
                Managers.Resource.Destroy(_bowIndicatorGO);
                _bowIndicatorGO = null;
            }
        }
        public override PlayerStateType GetState()
        {
            return PlayerStateType.Attack;
        }

        public override void OnSkill(SkillInfo2 skill)
        {
                _comboCount++;
                if (_comboCount > 2)
                    _comboCount = 0;
            _player.CurrentState = new ComboAttackState(_comboCount);
        }

        public override void AnimationEnd()
        {
            _player.CurrentState = new Player.PlayerIdleState();
        }
        public override void ExitState()  // 중간에 대쉬를 쓸 때에 대한 대비로 내가 추가해봄
        {
            base.ExitState();
            // ✅ 다른 상태로 바뀌는 모든 경우에 대한 세이프가드
            CleanupBowIndicator();
        }
    }



    public class ChargeState : PlayerDefaultState
    {
        int _chargeCount;
        int _comboCount;

        // ===== 인디케이터 관련 =====
        GameObject _chargeIndicatorGO;
        ChargeIndicator _chargeIndicator;
        WeaponIndicatorProfile _indicatorProfile; // 🔹 무기별 프로파일 캐시

        public ChargeState(int comboCount)
        {
            _chargeCount = 1;
            _comboCount = comboCount;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            _player.ChargingAura.SetActive(true);
            _indicatorProfile = _player.IndicatorProfile;     //무기 차징 값들 받아오기

            if (_player.CurrentWeapon.WeaponType == WeaponType.Bow)
            {
                //스테미나 2칸 소모
                _player.UseStamina(2);
                _player.Ani.Play(_player.CurrentWeapon.WeaponType.ToString() + "ChargeReady" + (_comboCount + 1).ToString(), -1, 0f);
                if (_player.CurrentWeaponModel.GetComponent<Animator>() != null)
                    _player.CurrentWeaponModel.GetComponent<Animator>().Play("ChargeReady", -1, 0f);
                GameObject a = Managers.Resource.Instantiate(_player.EffectPath + "ChargeReady", _player.transform.position, _player.transform.rotation);
                Managers.Sound.Play(_player.ChargeReadyClip);


            }
            else
            {
                //스테미나 2칸 소모
                _player.UseStamina(2);
                _player.Ani.Play(_player.CurrentWeapon.WeaponType.ToString() + "ChargeReady" + (_comboCount + 1).ToString(), -1, 0);
                GameObject a = Managers.Resource.Instantiate(_player.EffectPath + "ChargeReady", _player.transform.position, _player.transform.rotation);
                Managers.Sound.Play(_player.ChargeReadyClip);

            }
            // 무기 Profile 가져와서 인디케이터 생성
            _chargeIndicatorGO = Managers.Resource.Instantiate(_player.EffectPath + "ChargeIndicator", _player.transform.position, _player.transform.rotation);
            _chargeIndicator = _chargeIndicatorGO.GetComponent<ChargeIndicator>();
                if (_chargeIndicator != null && _indicatorProfile != null)
                {
                    float width = _indicatorProfile.stage1Width;
                    float length = _indicatorProfile.stage1Length;

                    _chargeIndicator.Init(_player.transform, width, length);
                }
        }

        public override void UpdateState()
        {
            _player.UseStamina(0);
            Vector3 mousePosition = Input.mousePosition;
            
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(_player.transform.position.x, _player.transform.position.y, _player.transform.position.z)); // Y異뺤쓣 湲곗??쇰줈 ?섎뒗 ?됰㈃???뺤쓽?⑸땲??

            float rayLength;
            if (groundPlane.Raycast(ray, out rayLength))
            {

                Vector3 targetPosition = ray.GetPoint(rayLength);

                Vector3 direction = targetPosition - _player.transform.position;
                direction.y = 0;


                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    _player.Rb.rotation = targetRotation;
                }
            }

            if (!_player.IsDefaultSkillHovering)
            {
                _player.InputBuffer._bufferedInputData = null;
                _player.CurrentState = new ChargedAttackState(_chargeCount, _comboCount, _chargeIndicator);
                _chargeIndicator = null; // 소유권 넘김

            }
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Charge;
        }

        public override void OnSkillRelease(SkillInfo2 skill)
        {
            _player.CurrentState = new ChargedAttackState(_chargeCount, _comboCount, _chargeIndicator);
        }

        //애니메이션 이벤트로 달아줘야함
        public void FullCharge()
        {
            Debug.Log("FullCharge 이벤트 실행");
            if (_player.CurrentStaminaCount >= 2)
            {

                _chargeCount++;
                //스테미나 4칸 소모
                _player.UseStamina(2);
                GameObject a = Managers.Resource.Instantiate(_player.EffectPath + "ChargeFullReady", _player.transform.position, _player.transform.rotation);
                Managers.Sound.Play(_player.FullChargeReadyClip);
                _player.Ani.Play(_player.CurrentWeapon.WeaponType.ToString() + "FullChargeReady" + (_comboCount + 1).ToString(), -1, 0f);
                
                // 인디케이터 2단 길이/너비로 확장
                if (_chargeIndicator != null && _indicatorProfile != null)
                {
                    float widthStage2 = _indicatorProfile.stage2Width;
                    float lengthStage2 = _indicatorProfile.stage2Length;

                    // 길이는 Lerp되게 목표값만 설정
                    _chargeIndicator.SetTargetLength(lengthStage2);
                }
            }
            else
            {
                Debug.Log("Stamina 4 미만");
            }
        }
        public override void ExitState()
        {
            base.ExitState();
            _player.ChargingAura.SetActive(false);
            // 인디케이터 삭제
            if (_chargeIndicator != null)
            {
                _chargeIndicator.DestroyIndicator();
                _chargeIndicator = null;
            }
        }


    }

    public class ChargedAttackState : PlayerDefaultState
    {
        int _chargeCount;
        int _comboCount;
        ChargeIndicator _chargeIndicator;   //추가

        public ChargedAttackState(int chargeCount, int comboCount, ChargeIndicator chargeIndicator)
        {
            //chargeCount가 몇이냐에 따라 해당 스킬 애니메이션 시전
            _chargeCount = chargeCount;
            _comboCount = comboCount;
            _chargeIndicator = chargeIndicator;

        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            _player.ChargingAura.SetActive(false);

            // 🔹 여기서 인디케이터 정리 (부드럽게 사라지게)
            if (_chargeIndicator != null)
            {
                _chargeIndicator.DestroyIndicator();
                _chargeIndicator = null;
            }
            switch (_chargeCount)
            {
                case 1:
                    //스테미나 2칸 소모
                    //_player.UseStamina(2);
                    if (_player.CurrentWeapon.WeaponType == WeaponType.Bow)
                    {
                        _player.Ani.Play(_player.CurrentWeapon.WeaponType.ToString() + "ChargedAttack" + (_comboCount + 1).ToString(), -1, 0f);
                        if (_player.CurrentWeaponModel.GetComponent<Animator>() != null)
                            _player.CurrentWeaponModel.GetComponent<Animator>().Play("ChargedAttack", -1, 0f);
                    }
                    else
                    {
                        _player.Ani.Play(_player.CurrentWeapon.WeaponType.ToString() + "ChargedAttack" + (_comboCount + 1).ToString(), -1, 0);
                    }



                    //MaceChargedAttack1
                    break;

                case 2:
                    //스테미나 4칸 소모
                    //_player.UseStamina(4);
                    if (_player.CurrentWeapon.WeaponType == WeaponType.Bow)
                    {
                        _player.Ani.Play(_player.CurrentWeapon.WeaponType.ToString() + "FullChargedAttack" + (_comboCount + 1).ToString(), -1, 0f);
                        if (_player.CurrentWeaponModel.GetComponent<Animator>() != null)
                            _player.CurrentWeaponModel.GetComponent<Animator>().Play("ChargedAttack", -1, 0f);
                    }
                    else
                    {
                        _player.Ani.Play(_player.CurrentWeapon.WeaponType.ToString() + "FullChargedAttack" + (_comboCount + 1).ToString(), -1, 0);
                    }                    //MaceFullChargedAttack1
                    break;

                default:
                    break;
            }
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Attack;
        }

        public override void UpdateState()
        {
            MoveCheck();
        }

        public override void OnSkill(SkillInfo2 skill)
        {
            _player.CurrentState = new ComboAttackState(0);
        }

        public override void AnimationEnd()
        {
            _player.CurrentState = new Player.PlayerIdleState();
        }
    }

    /*
    public class MaceComboAttackState : PlayerDefaultState, AnimationCheck
    {
        public bool IsAnimationEnd { get; set; }

        MaceComboAttackSkillInfo _skillInfo;

        public MaceComboAttackState(MaceComboAttackSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);



            IsAnimationEnd = false;


            LookAtPos();
            _player.Ani.Play("MaceComboAttack" + _skillInfo.CurrentComboCount.ToString(), -1, 0f);
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

    public class MaceSmashAttackState : PlayerDefaultState, AnimationCheck
    {
        MaceSmashSkillInfo _skillInfo;
        public bool IsAnimationEnd { get; set; }
        public MaceSmashAttackState(MaceSmashSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            // PossibleActions.Add(nameof(OnDash));
            IsAnimationEnd = false;



            //LookAtPos();
            _player.Ani.Play("MaceSmashSkill", -1, 0f);
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

    /*
    public class MaceCircleAttackState : PlayerDefaultState, AnimationCheck
    {
        MaceCircleSkillInfo _skillInfo;
        public bool IsAnimationEnd { get; set; }
        public bool IsAttackDelayOver { get; set; }

        public MaceCircleAttackState(MaceCircleSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            // PossibleActions.Add(nameof(OnDash));
            IsAnimationEnd = false;
            IsAttackDelayOver = false;



            LookAtPos();
            _player.Ani.Play("MaceCircleSkill", -1, 0f);
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




    /*public class MacePulseAttackState : PlayerState, AnimationCheck, AttackDelayCheck
    {
        public MacePulseSkillInfo _skillInfo;
        public bool IsAnimationEnd { get; set; }
        public bool IsAttackDelayOver { get; set; }

        public MacePulseAttackState(MacePulseSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            // PossibleActions.Add(nameof(OnDash));
            PossibleActions.Add(nameof(OnSkill));
            IsInputBufferUsing = true;     //선입력 가능하게 할건지
            IsAnimationEnd = false;
            IsAttackDelayOver = false;



            LookAtPos();
            _player.Ani.Play("MacePulseSkill", -1, 0f);
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
    public class MacePulseAttackState : PlayerDefaultState, AnimationCheck
    {
        public MacePulseSkillInfo _skillInfo;
        public bool IsAnimationEnd { get; set; }
        public bool CanRotate { get; set; } = true;

        GameObject _indicator;

        public MacePulseAttackState(MacePulseSkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            IsAnimationEnd = false;

            Vector3 initialPosition = GetClampedMousePosition();
            _skillInfo.AttackPos = initialPosition;
            _indicator = Managers.Resource.Instantiate(_player.EffectPath + "MacePulseIndicator", initialPosition, Quaternion.identity);

            _player.Ani.Play("MacePulseSkill", -1, 0f);
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
            DestroyIndicator();
        }

        public void DestroyIndicator()
        {
            if (_indicator != null)
            {
                Managers.Resource.Destroy(_indicator);
                _indicator = null;
            }
        }

        public override PlayerStateType GetState() => PlayerStateType.Attack;

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
    */
    

    /* 스킬 커슈텀 폐기
    public class MaceCounterAttackState : PlayerState, AnimationCheck, AttackDelayCheck
    {
        public bool IsAnimationEnd { get; set; }
        public bool IsAttackDelayOver { get; set; }
        Vector3 _attackPos;

        //
        public MaceCounterAttackState(Vector3 attackPos)
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
            _player.Ani.Play("MaceCounterAttackSkill", -1, 0f);
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

    }
    */
}