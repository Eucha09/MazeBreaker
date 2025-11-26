using UnityEngine;
using UnityEngine.InputSystem;
using static Define;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Player;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]

public class PlayerController : BaseController
{
    public BloodScreenEffect bloodScreenEffect;
    public TrailRenderer trailRenderer; // 연결된 TrailRenderer
    public GameObject SprintTrail;
    public GameObject Aura;
    public GameObject ChargingAura;
    public GameObject ChargeReady;
    public AudioClip ChargeReadyClip;
    public AudioClip FullChargeReadyClip;


    //-------------------Skills----------------------------------------------------
    public SkillInfo2 defaultSkill; // 일반공격 3타
    public SkillInfo2 qSkill;
    public SkillInfo2 eSkill;
    public SkillInfo2 cSkill;
    public SkillInfo2 rSkill;

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    #region Variables
    Player.PlayerState _currentState;//현재 State
    Player.PlayerState _previousState;//이전 State
    Player.PlayerState _nextState;

    //------------------무기-----------------------------------------------------------------------------------------------------------------
    //public bool IsEquipPossible { get; set; } = true;//무기 착용이 가능한 상태인지 체크하는 변수
    GameObject _currentWeaponModel; // 모델에 대한 데이터는 Item Class에서 가지고 있을 예정
    [SerializeField]
    Transform _rightHandPos;//무기를 들고 있을 오른쪽손 위치
    [SerializeField]
    Transform _leftHandPos;//무기를 들고 있을 왼쪽손 위치
	Equipment _currentWeapon;
	Equipment _currentArmor;

    public GameObject CurrentWeaponModel { get { return _currentWeaponModel; } set { _currentWeaponModel = value; } }


    public GameObject ArrowIndicator { get; set; } = null;

    //-----------------ETC--------------------------------------------------------------------------------------------------------------
    public string EffectPath { get; set; } = "Effects/Player/";
    Rigidbody _rigidbody;

    [SerializeField]
    Animator _animator;

    [SerializeField]
    GrapplingGun _grapplingGun;

    [SerializeField]
    PlayerStats _stats;

    [SerializeField]
    PlayerInputBufferManager _inputBuffer;

    public LayerMask AttackTargetLayer;
    #endregion
    //-----------Stats--------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    #region Stat
    //public Stun stun;
    //영향 퍼센티지, 남은 시간,

    public override float Hp { get { return _stats.CurrentHP; } set { if (value > MaxHp) _stats.CurrentHP = MaxHp; else if (value < 0) _stats.CurrentHP = 0f; else _stats.CurrentHP = value; }}
    public override float MaxHp { get { return _stats.MaxHP; } set { _stats.MaxHP = value; } }
    public Coroutine HPCoroutin;
    public override float Mental { get { return _stats.CurrentMental; } set { if (value > MaxMental) _stats.CurrentMental = MaxMental; else if (value < 0) _stats.CurrentMental = 0f; else _stats.CurrentMental = value; } }
    public override float MaxMental { get { return _stats.MaxMental; } set { _stats.MaxMental = value; } }
    public Coroutine MentalCoroutin;
    public IEnumerator MentalReduceCoroutin()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);
            //Mental -= 0f;
            if(Mental == 0)
            {
                CurrentState = new Player.PlayerFaintedState();
            }
            //정신력이 0이 될 경우 쓰러지는 State로 넘기고, 자연적으로 10이 회복될 때 까지 누워있는다.
        }
    }
    public override float Hunger { get => _stats.CurrentHunger; set { if (value > MaxHunger) _stats.CurrentHunger = MaxHunger; else if (value < 0) _stats.CurrentHunger = 0f; else _stats.CurrentHunger = value; } }
    public override float MaxHunger { get => _stats.MaxHunger; set => _stats.MaxHunger = value; }
    public Coroutine HungerCoroutin;
    //배고픔 감소 코루틴
    public IEnumerator HungerReduceCoroutin()
    {
        //N초마다 N의 배고픔이 감소한다.
        while (true)
        {
            yield return new WaitForSeconds(15f);
            Hunger -= 1f;
            //배고픔이 Max에서 25 떨어질 때마다 스테미나 상한 감소
            int a = (int)(Hunger / 25);
            MaxStaminaCount = a + 1;
        }
    }

    public override float CurrentMoveSpeed { get => _stats.CurrentMoveSpeed; set => _stats.CurrentMoveSpeed = value; }
    public override float MoveSpeed { get => _stats.MoveSpeed; set => _stats.MoveSpeed = value; }
    //나중에 버프계열 아이템이 있다면 Multiplier변수를 통해서 데미지 스피드 등등에 곱하기 연산
    public override float Attack { get { return _currentWeapon != null ? _currentWeapon.Damage : 5.0f; } set {} }
    public override float Defense { get { if (_currentArmor == null) return 5f; else return _currentArmor.Defence + 5f; } set {} }

    [SerializeField]
    private int _currentStaminaCount = 4;
    public int MaxStaminaCount { get; set; } = 4;
    public int CurrentStaminaCount { get => _currentStaminaCount; set { if (value > MaxStaminaCount) _currentStaminaCount = MaxStaminaCount; else if (value < 0) _currentStaminaCount = 0; else _currentStaminaCount = value; } }
    float LastStaminaUsedTime { get; set; } = 0f;
    float StaminaRecoveryStartDuration { get; set; } = 2f;
    float StaminaRecoveryDuration { get; set; } = 0.5f;
    public void UseStamina(int staminaConsumptionCount)
    {
        LastStaminaUsedTime = Time.time;
        CurrentStaminaCount -= staminaConsumptionCount;
    }
    public Coroutine StaminaCoroutin;

    IEnumerator StaminaRecoveryCoroutin()
    {
        while (true)
        {
            // 스태미너 회복 시작 대기
            if (Time.time - LastStaminaUsedTime >= StaminaRecoveryStartDuration)
            {
                CurrentStaminaCount++;
                yield return new WaitForSeconds(StaminaRecoveryDuration);
            }
            else
            {
                yield return null;
            }
        }
    }


    public void StartPlayerCoroutine(ref Coroutine coroutine, IEnumerator routine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(routine);
    }

    public void StopPlayerCoroutine(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = null;
    }

    #endregion
    //------------------GetSet---------------------------------------------------------------------------------------------------------------------------------------------------------------
    #region GetSetVariables
    public Player.PlayerState CurrentState
    {
        get { return _currentState; }
        set {
            _nextState = value;

            if (_currentState != null)
            {
                _currentState.ExitState();
                _previousState = _currentState;
            }

            _currentState = value;
            _currentState.EnterState(this);
        }
    }

    public Player.PlayerState PreviousState { get { return _previousState; } set { _previousState = value; } }
    public Player.PlayerState NextState { get { return _nextState; } set { _nextState = value; } }

    public PlayerStats Stats
    {
        get { return _stats; }
        set { _stats = value; }
    }

    public PlayerInputBufferManager InputBuffer
    {
        get { return _inputBuffer; }
        set { _inputBuffer = value; }
    }

    public GrapplingGun GrapplingGun
    {
        get { return _grapplingGun; }
        set { _grapplingGun = value; }
    }

    public Rigidbody Rb {
        get { return _rigidbody; }
        set { _rigidbody = value; }
    }
    public Animator Ani
    {
        get { return _animator; }
        set { _animator = value; }
    }

    public Equipment CurrentWeapon
    {
        get { return _currentWeapon; }
        set { _currentWeapon = value; }
    }

    #endregion
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public Vector3 AttackPosition { get; set; } // 공격 목표 지점 저장

    private void Awake()
    {
        _currentWeapon = null;
        Rb = GetComponent<Rigidbody>();
        Rb.freezeRotation = true;
        StartPlayerCoroutine(ref StaminaCoroutin, StaminaRecoveryCoroutin());
        StartPlayerCoroutine(ref MentalCoroutin, MentalReduceCoroutin());
        StartPlayerCoroutine(ref HungerCoroutin, HungerReduceCoroutin());
        StartPlayerCoroutine(ref _sprintPossibleCheckCoroutine, SprintPossibleCheckCoroutine());
        GetComponent<PlayerInput>().enabled = true;
        GetComponent<PlayerInput>().ActivateInput();
	}

    private void OnEnable()
    {
        CurrentState = new Player.PlayerWakeUpState();
    }


    private void FixedUpdate()
    {
        //Debug.Log(_currentState);

        _currentState.UpdateState();
        Stats.UpdateStats();
    }

    private void Update()
    {
        if (Stats.CurrentHP <= 0
            && CurrentState.GetState() != Player.PlayerStateType.Dead 
            && CurrentState.ToString() != "Player.PlayerGrabbedState")
            CurrentState = new Player.PlayerDeadState();
        ProcessBuffer();
    }

    private void ProcessBuffer()
    {
        if(CurrentState.CanChangeStateByInput)
            InputBuffer.ProcessBufferedInput();
    }



    bool IsGamepadAndShowingPopupUI()
    {
        return Managers.Input.DeviceMode == DeviceMode.Gamepad && (Managers.UI.PopupUICount > 0 || Managers.UI.SceneUI.IsAnyWindowOpen() || Managers.UI.IsActiveVirtualCursor); // TODO
    }

    #region InputMethods

    public Vector2 MoveDir = Vector3.zero;

    public void OnMove(InputAction.CallbackContext context)
    {
		if (context.performed && !IsGamepadAndShowingPopupUI())
			MoveDir = context.ReadValue<Vector2>();
		else if (context.canceled)
            MoveDir = Vector2.zero;
	}

    public void OnDash(InputAction.CallbackContext context)
	{
		if (context.started)
        {

        }
        else if (context.performed && !IsGamepadAndShowingPopupUI())
        {
            if (!(_currentState is PlayerDefaultState state))
                return;

            if (CurrentState.CanBufferInput && !CurrentState.CanChangeStateByInput)
            {
                InputBuffer.BufferInput(new InputData(DashExcecute));
            }


            if (CurrentState.CanChangeStateByInput || CurrentState.GetState() == PlayerStateType.Attack)
            {
                DashExcecute();
            }

        }
        else if (context.canceled)
		{
                PlayerDefaultState state = _currentState as PlayerDefaultState;
                state?.OnSprintCancel();

		}
    }

    public void DashExcecute()
    {
        if (_currentState is PlayerDefaultState state)
        {
            state.OnDash();
            InputBuffer.ClearBuffer();
        }
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        if (context.started)
        {

        }
        else if (context.performed)
        {
            if (!(_currentState is PlayerSprintState state))
                return;

            if (CurrentState.CanBufferInput && !CurrentState.CanChangeStateByInput)
                InputBuffer.BufferInput(new InputData(BrakeExcecute));

            if (CurrentState.CanChangeStateByInput)
            {
                BrakeExcecute();
            }
            
        }
        else if (context.canceled)
        {

        }
    }

    public void BrakeExcecute()
    {
        PlayerSprintState state = _currentState as PlayerSprintState;
        if (state != null)
        {
            state.OnBrake();
            InputBuffer.ClearBuffer();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
	{
		if (context.started)
        {

        }
        else if (context.performed && !IsGamepadAndShowingPopupUI())
        {
            if (Managers.Game.CheckStatus(GameStatus.Enable_Interaction) == false)
                return;
            Interact();
            GetComponent<UIInputHandler>().LockInputCancelBriefly();
        }
        else if (context.canceled)
        {

        }

	}
    public void Interact()
	{
		GameObject go = Managers.Object.FindNearestObjectWithTag("Interactable", transform.position, 3);
		if (go != null)
		{
			go.GetComponent<Interactable>().Interact();
		}
	}

    public void OnHook(InputAction.CallbackContext context)
    {
        if (context.started)
        {

        }
        else if (context.performed)
        {

            if (CurrentState.CanBufferInput && !CurrentState.CanChangeStateByInput)
                InputBuffer.BufferInput(new InputData(HookExcecute));
            if (CurrentState.CanChangeStateByInput)
            {
                HookExcecute();
            }
        }
        else if (context.canceled)
		{

            PlayerHookState state = _currentState as PlayerHookState;
            state?.OnHookCancel();
		}
    }

    public void HookExcecute()
    {
        //PlayerDefaultState state = _currentState as PlayerDefaultState;
        if (_currentState is PlayerDefaultState state)
        {
            state.OnHook();
            InputBuffer.ClearBuffer();
            return;
        }
        
        if (_currentState is PlayerHookState state2)
        {
            state2.OnHook();
            InputBuffer.ClearBuffer();
        }
        
    }

    public void OnHookReady(InputAction.CallbackContext context)
    {
        if (context.started)
        {

        }
        else if (context.performed)
        {
            Stats.IsHookReady = true;
        }
        else if (context.canceled)
		{
			Stats.IsHookReady = false;
		}
        
    }

    public bool IsDefaultSkillHovering = false;
    public float DefaultSkillHoveringTime { get { return Time.time -_defaultSkillHoveringStartTime; } set { } }
    private float _defaultSkillHoveringStartTime;
    public void OnDefaultSkill(InputAction.CallbackContext context)
	{
		if (context.started)
        {

        }
        else if (context.performed && !IsGamepadAndShowingPopupUI())
        {
            IsDefaultSkillHovering = true;
            _defaultSkillHoveringStartTime = Time.time;

            if (EventSystem.current.IsPointerOverGameObject())
                return;


            if (CurrentState.CanBufferInput && !CurrentState.CanChangeStateByInput)
                InputBuffer.BufferInput(new InputData(DefaultSkillExcecute));

            if (CurrentState.CanChangeStateByInput)
            {
                DefaultSkillExcecute();
            }

        }
        else if (context.canceled)
        {
            IsDefaultSkillHovering = false;


                DefaultSkillReleaseExcecute();
            
        }
    }

    public void DefaultSkillExcecute()
    {
        PlayerDefaultState state = _currentState as PlayerDefaultState;
        if (state != null)
        {
            state.OnSkill(defaultSkill);
            InputBuffer.ClearBuffer();
        }
    }

    public void DefaultSkillReleaseExcecute()
    {
        PlayerDefaultState state = _currentState as PlayerDefaultState;
        if (state != null)
        {
            state.OnSkillRelease(defaultSkill);
        }
    }

    public void OnQSkill(InputAction.CallbackContext context)
	{
		if (context.started)
        {

        }
        else if (context.performed && !IsGamepadAndShowingPopupUI())
        {
            if (CurrentState.CanBufferInput && !CurrentState.CanChangeStateByInput)
                InputBuffer.BufferInput(new InputData(QSkillExcecute));

            if (CurrentState.CanChangeStateByInput)
            {
                QSkillExcecute();
            }
        }
        else if (context.canceled)
        {
            PlayerDefaultState state = _currentState as PlayerDefaultState;
            state.OnSkillRelease(qSkill);
        }
    }

    public void QSkillExcecute()
    {
        PlayerDefaultState state = _currentState as PlayerDefaultState;
        if (state != null)
        {
            state.OnSkill(qSkill);
            InputBuffer.ClearBuffer();
        }
    }

    public void OnESkill(InputAction.CallbackContext context)
	{
		if (context.started)
        {

        }
        else if (context.performed && !IsGamepadAndShowingPopupUI())
        {

            if (CurrentState.CanBufferInput && !CurrentState.CanChangeStateByInput)
                InputBuffer.BufferInput(new InputData(ESkillExcecute));


            if (CurrentState.CanChangeStateByInput)
            {
                ESkillExcecute();
            }
        }
        else if (context.canceled)
        {
            PlayerDefaultState state = _currentState as PlayerDefaultState;
            state.OnSkillRelease(eSkill);
        }
    }

    public void ESkillExcecute()
    {
        PlayerDefaultState state = _currentState as PlayerDefaultState;
        if (state != null)
        {
            state.OnSkill(eSkill);
            InputBuffer.ClearBuffer();
        }
    }

    public void OnCSkill(InputAction.CallbackContext context)
	{
		if (context.started)
        {

        }
        else if (context.performed && !IsGamepadAndShowingPopupUI())
        {
            if (CurrentState.CanChangeStateByInput)
            {
                cSkill.UseSkill();
            }
        }
        else if (context.canceled)
        {
        }
    }

    public void CSkillExcecute()
    {
        PlayerDefaultState state = _currentState as PlayerDefaultState;
        if (state != null)
        {
            state.OnSkill(cSkill);
            InputBuffer.ClearBuffer();
        }
    }

    public void OnRSkill(InputAction.CallbackContext context)
	{
		if (context.started)
        {

        }
        else if (context.performed && !IsGamepadAndShowingPopupUI())
        {
        }
        else if (context.canceled)
        {

        }
    }
    #endregion

    #region 현재 사용안하는 코드
    private void OnInteractionSkill(InputAction.CallbackContext context) { }
    private void OnInteractionSkillRelease(InputAction.CallbackContext context) { }
    #endregion

    public void OnCancel(InputAction.CallbackContext context)
	{
        if (context.performed)
		{
			_currentState.OnCancel(context);
		}
	}

	#region 외부 참조용 Method

	public void OnGrabbed()
    {
        if(CurrentState.ToString() != "Player.PlayerDeadState")
        CurrentState = new Player.PlayerGrabbedState();
    }
    public void OnRelease()
    {
        if (Stats.CurrentHP <= 0 && Stats.CurrentMental <= 0)
        {
            Debug.Log("죽음");
            CurrentState = new Player.PlayerDeadState();

        }
        else
        {
            Debug.Log("풀어짐");
            CurrentState = new Player.PlayerIdleState();
        }
    }

    
    public void BlockInput()
    {
        //_inputActions.Player.Disable();
    }

    public void AllowInput()
    {
        //_inputActions.Player.Enable();
    }
    

    //Building 관련 외부 메서드 : Building Pos로 플레이어 이동
    public void MoveToBuildPosition(PreviewObject previewObject)
    {
        CurrentState = new Player.MoveToBuildingPosState(previewObject);
    }

    //완료시 플레이어 알림
    public void BuildingDone()
    {
        CurrentState = new Player.PlayerIdleState();
    }

    //플레이어 인터랙션 시작 (PlayerInteractionType은 현재 어떤 오브젝트와 인터랙션을 하고 있는가,
    //playerPos는 플레이어가 이동할 포지션,
    //lookDirection는 플레이어의 로테이션값)
    public void PlayerInteractionStart(PlayerInteractionObjectType interactionType, Vector3 playerPos, Vector3 lookDirection, Interactable interactable)
    {
        CurrentState = new Player.PlayerInteractionState(interactionType, playerPos, lookDirection, interactable);
    }

    public void PlayerInteractionEnd()
    {
        CurrentState = new Player.PlayerIdleState();
    }

    public void PlayerMoveToEventPosition(Vector3 targetPosition)
    {
        CurrentState = new Player.MoveToEventPosState(targetPosition);
    }

    public void PlayerEventEnd()
    {
        CurrentState = new Player.PlayerIdleState();
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        CurrentState.HandleOnTriggerEnter(other);
    }

    #region WeaponChanging


    
    public void UnEquipWeapon()
    {
        //현재 스킬 시전중일 경우 Idle상태로 돌아온다.
        if (CurrentState.GetState() == PlayerStateType.Attack)
            CurrentState = new PlayerIdleState();

        DisalbeWeaponBindings();
        //주먹 바인딩 해준다.
        if (gameObject.name != "ExperimentPlayer(Clone)")
        {
            //_inputActions.Player.DefaultSkill.started += OnDefaultSkill;
            //defaultSkill = new FistComboAttackSkillInfo(this);
        }
        if (_currentWeaponModel != null)
        {
            Destroy(_currentWeaponModel);
            _currentWeaponModel = null;
        }
        _currentWeapon = null;
	}
    public WeaponIndicatorProfile IndicatorProfile { get; private set; }       //창수 - 무기별 차징 스킬 너비, 길이 받아오기

    //무기 바꿀 때 호출해야하는 함수
    public void ChangeWeapon(Equipment weapon)
    {
        IndicatorProfile = Resources.Load<WeaponIndicatorProfile>($"IndicatorProfiles/{weapon.WeaponType}IndicatorProfile");  //창수 - 무기별 차징 스킬 너비, 길이 받아오기

        //현재 스킬 시전중일 경우 Idle상태로 돌아온다.
        if (CurrentState.GetState() == PlayerStateType.Attack)
            CurrentState = new PlayerIdleState();

        //무기 바꿀때는 주먹 바인딩 해제
        //_inputActions.Player.DefaultSkill.started -= OnDefaultSkill;

        if (weapon == null)
            return;

        if(_currentWeapon != null)
            DisalbeWeaponBindings();
        if (_currentWeaponModel != null)
        {
            Destroy(_currentWeaponModel);
        }
        if(weapon.WeaponType == WeaponType.Bow)
            _currentWeaponModel = Managers.Resource.Instantiate(weapon.PrefabPath, _leftHandPos);
        else
            _currentWeaponModel =Managers.Resource.Instantiate(weapon.PrefabPath, _rightHandPos);
        _currentWeapon = weapon;

        EnableWeaponBindings();
	}

    private void EnableWeaponBindings()
    {
        cSkill = new ParrySkillInfo(this);
        switch (_currentWeapon.WeaponType)
        {
            case WeaponType.Sword:
                /*
                _inputActions.Player.DefaultSkill.started += OnDefaultSkill;
                _inputActions.Player.QSkill.started += OnQSkill;
                _inputActions.Player.ESkill.started += OnESkill;
                _inputActions.Player.CSkill.started += OnCSkill;
                */
                /*
                defaultSkill = new SwordComboAttackSkillInfo(this);
                qSkill = SkillInfo2.MakeSkillInfo(_currentWeapon.QSkillId, this);
                eSkill = SkillInfo2.MakeSkillInfo(_currentWeapon.ESkillId, this);
                */
                //qSkill = new SwordGroundSkillInfo(this);
                //eSkill = new SwordPrickSkillInfo(this);
                //cSkill = new SwordCounterSkillInfo(this);
                /*
                defaultSkill = new ComboSkillInfo("SwordAttack",0,3);
                qSkill = new ArrowRainSkillInfo("ArrowRainSkill");
                eSkill = new SwordPrickSkillInfo("SwordPrickSkill");
                */
                break;

            case WeaponType.Bow:
                
                /*
                defaultSkill = new BowChargeShotSkillInfo(this);
				qSkill = SkillInfo2.MakeSkillInfo(_currentWeapon.QSkillId, this);
				eSkill = SkillInfo2.MakeSkillInfo(_currentWeapon.ESkillId, this);
                */

                break;

            case WeaponType.Mace:
                /*
                defaultSkill = new MaceComboAttackSkillInfo(this);
				qSkill = SkillInfo2.MakeSkillInfo(_currentWeapon.QSkillId, this);
				eSkill = SkillInfo2.MakeSkillInfo(_currentWeapon.ESkillId, this);
                */
                break;

            case WeaponType.Axe:
                /*
                defaultSkill = new AxeComboAttackSkillInfo(this);
				qSkill = SkillInfo2.MakeSkillInfo(_currentWeapon.QSkillId, this);
				eSkill = SkillInfo2.MakeSkillInfo(_currentWeapon.ESkillId, this);
                */


                break;

            case WeaponType.Hammer:
                /*
                defaultSkill = null ;
                qSkill = SkillInfo2.MakeSkillInfo(_currentWeapon.QSkillId, this);
                eSkill = SkillInfo2.MakeSkillInfo(_currentWeapon.ESkillId, this);
                */

                break;

            default:
                break;

        }
        if(defaultSkill!= null)
            defaultSkill.SkillKey = SkillKey.Default;
        if (qSkill != null)
            qSkill.SkillKey = SkillKey.Q;
        if (eSkill != null)
            eSkill.SkillKey = SkillKey.E;
        if (cSkill != null)
            cSkill.SkillKey = SkillKey.C;
    }

    private void DisalbeWeaponBindings()
    {
        /*
        switch (_currentWeapon.WeaponType)
        {
            case WeaponType.Sword:
                _inputActions.Player.DefaultSkill.started -= OnDefaultSkill;
                _inputActions.Player.QSkill.started -= OnQSkill;
                _inputActions.Player.ESkill.started -= OnESkill;
                _inputActions.Player.CSkill.started -= OnCSkill;
                break;

            case WeaponType.Bow:
                _inputActions.Player.DefaultSkill.started -= OnDefaultSkill;
                _inputActions.Player.DefaultSkill.canceled -= OnDefaultSkillRelease;
                _inputActions.Player.QSkill.started -= OnQSkill;
                _inputActions.Player.ESkill.started -= OnESkill;
                _inputActions.Player.CSkill.started -= OnCSkill;
                break;

            case WeaponType.Mace:
                _inputActions.Player.DefaultSkill.started -= OnDefaultSkill;
                _inputActions.Player.QSkill.started -= OnQSkill;
                _inputActions.Player.ESkill.started -= OnESkill;
                _inputActions.Player.CSkill.started -= OnCSkill;
                break;

            case WeaponType.Axe:
                _inputActions.Player.DefaultSkill.started -= OnDefaultSkill;
                _inputActions.Player.QSkill.started -= OnQSkill;
                _inputActions.Player.ESkill.started -= OnESkill;
                _inputActions.Player.CSkill.started -= OnCSkill;
                _inputActions.Player.RSkill.started -= OnRSkill;
                break;

            case WeaponType.Hammer:
                //_inputActions.Player.DefaultSkill.started -= OnDefaultSkill;
                _inputActions.Player.QSkill.started -= OnQSkill;
                _inputActions.Player.ESkill.started -= OnESkill;
                _inputActions.Player.CSkill.started -= OnCSkill;
                _inputActions.Player.RSkill.started -= OnRSkill;
                break;

            default:
                break;
        }
        */
        defaultSkill = null;
        qSkill = null;
        eSkill = null;
        cSkill = null;
    }
	#endregion

	#region Revive
	public override void Revive()
    {
        StartCoroutine(CoRevive());
	}

	IEnumerator CoRevive()
	{
		UI_GameScene sceneUI = Managers.UI.SceneUI as UI_GameScene;

		yield return StartCoroutine(sceneUI.CoFadeOut());

        Rb.MovePosition(Vector3.zero);
        Rb.rotation = Quaternion.identity;
        Rb.linearVelocity = Vector3.zero;
        Hp = MaxHp;
        Mental = MaxMental;
        Hunger = MaxHunger;

		yield return StartCoroutine(sceneUI.CoFadeIn());

        CurrentState = new PlayerWakeUpState();
	}
	#endregion

	#region GroundCheck
	[Header("Ground Check")]
    [SerializeField]
    Vector3 rayStartOffset;
    [SerializeField]
    float sphereRadius = 1f;
    [SerializeField]
    float maxDistance = 10f;
    [SerializeField]
    Color sphereColor = Color.green;

    void OnDrawGizmos()
    {
        Vector3 rayStartPos = transform.position + rayStartOffset;

        Gizmos.color = sphereColor;
        Vector3 direction = -transform.up;
        Gizmos.DrawWireSphere(rayStartPos, sphereRadius);
        Gizmos.DrawWireSphere(rayStartPos + direction * maxDistance, sphereRadius);
        Gizmos.DrawLine(rayStartPos + Vector3.up * sphereRadius, rayStartPos + direction * maxDistance + Vector3.up * sphereRadius);
        Gizmos.DrawLine(rayStartPos - Vector3.up * sphereRadius, rayStartPos + direction * maxDistance - Vector3.up * sphereRadius);
        Gizmos.DrawLine(rayStartPos + Vector3.right * sphereRadius, rayStartPos + direction * maxDistance + Vector3.right * sphereRadius);
        Gizmos.DrawLine(rayStartPos - Vector3.right * sphereRadius, rayStartPos + direction * maxDistance - Vector3.right * sphereRadius);
    }

    public bool IsGrounded()
    {
        Vector3 rayStartPos = transform.position + rayStartOffset;

        // 실제 SphereCast 로직
        RaycastHit hit;
        if (Physics.SphereCast(rayStartPos, sphereRadius, -transform.up, out hit, maxDistance))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    #region SprintPossibleCheck

    public bool IsSprintPossible = false;
    private Coroutine _sprintPossibleCheckCoroutine;

    IEnumerator SprintPossibleCheckCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            IsSprintPossible = Managers.Object.FindNearestObjectWithTag("StarEdge", transform.position, 9f);
        }
    }
    #endregion

    public override void OnDamaged(DamageCollider dm)
    {
        CurrentState.OnDamaged(dm); // TODO
        //데미지 받는 로직 자유롭게 작성
    }
}
