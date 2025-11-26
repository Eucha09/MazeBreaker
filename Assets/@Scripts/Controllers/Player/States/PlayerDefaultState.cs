using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using TreeMonster.Search;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define;

namespace Player
{

    public class PlayerIdleState : PlayerDefaultState
    {
        Equipment _currentWeapon;

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            CanChangeStateByInput = true;
            if (_player.PreviousState != null && _player.PreviousState.GetState() == PlayerStateType.Move)
            {
                _player.Ani.SetBool("IsMoveStop", true);
            }
            else if ((_player.PreviousState != null && _player.PreviousState.GetState() == PlayerStateType.Attack) ||
                (_player.PreviousState != null && _player.PreviousState.GetState() == PlayerStateType.Hurt) ||
                    (_player.PreviousState != null && _player.PreviousState.GetState() == PlayerStateType.Stun))
            {
                PlayWeaponAnimation("BattleIdle", .5f);
            }
           
            else
            {

                PlayWeaponAnimation("Idle", 0.05f);
            }
            _currentWeapon = _player.CurrentWeapon;
            Managers.Event.PlayerEvents.OnDisablePlayerMovement();
        }

        public override void UpdateState()
        {
            if (!_player.IsGrounded())
            {
                _player.CurrentState = new PlayerAirBorneState();
            }
            if (_currentWeapon != _player.CurrentWeapon)
            {
                PlayWeaponAnimation("Idle", 0.05f);   // 여기를 Change로만 바꿔주자.
                _currentWeapon = _player.CurrentWeapon;
            }
            if (!_player.IsGrounded())
            {
                _player.CurrentState = new PlayerAirBorneState();
            }
            MoveCheck();
        }

        public override void ExitState()
        {
            _player.Ani.SetBool("IsMoveStop", false);
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Idle;
        }

    }

    public class PlayerMovingState : PlayerDefaultState
    {
		Equipment _currentWeapon;

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            CanChangeStateByInput = true;
            if(_player.PreviousState.GetState() == PlayerStateType.Dash)
            PlayWeaponAnimation("Run", 0.2f);
            else
            PlayWeaponAnimation("Run", 0.1f);

            _currentWeapon = _player.CurrentWeapon;
            Managers.Event.PlayerEvents.OnEnablePlayerMovement();
        }

        public override void UpdateState()
        {
            if (_currentWeapon != _player.CurrentWeapon)
            {
                PlayWeaponAnimation("Run", 0.05f);
                _currentWeapon = _player.CurrentWeapon;
            }

            if (!_player.IsGrounded())
            {
                _player.CurrentState = new PlayerAirBorneState();
            }


            Vector3 moveDirection = new Vector3(_player.MoveDir.x, 0f, _player.MoveDir.y).normalized; // 占싱듸옙 占쏙옙占쏙옙 占쏙옙占쏙옙

            // 占쏙옙占쏙옙占쏙옙 占쏙옙占싶몌옙 카占쌨띰옙占쏙옙 占쏙옙占쏙옙占쏙옙占쏙옙 占쏙옙환占싹울옙 占쏙옙占쏙옙 占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙
            moveDirection = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * moveDirection;
            moveDirection.y = 0;

            // Rigidbody占쏙옙 占쌈듸옙 占쏙옙占쏙옙 占쏙옙占쏙옙
            _player.Rb.linearVelocity = moveDirection * _player.Stats.CurrentMoveSpeed + Vector3.up * _player.Rb.linearVelocity.y;

            // 캐占쏙옙占싶곤옙 占싱듸옙占싹댐옙 占쏙옙占쏙옙占쏙옙占쏙옙 회占쏙옙
            if (moveDirection != Vector3.zero)
            {
                // _player.Rb.rotation = Quaternion.LookRotation(moveDirection); 
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection); 
                //_player.Rb.rotation = Quaternion.Slerp(_player.Rb.rotation, targetRotation, Time.deltaTime * 10f);
                _player.Rb.MoveRotation(Quaternion.Slerp(_player.Rb.rotation, targetRotation, Time.deltaTime * 10f)); //李쎌닔
            }
            else
            {
                _player.Rb.linearVelocity = Vector3.zero;
                _player.CurrentState = new PlayerIdleState();
            }
        }

        public override void ExitState()
        {
            base.ExitState();
            _player.Rb.linearVelocity = Vector3.zero;
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Move;
        }
    }

    public class PlayerDashingState : PlayerDefaultState
    {
        bool isDashing = true;
        Vector3 moveDirection;
        float _dashStartTime;

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            CanBufferInput = true;


            _player.Stats.UseStamina(_player.Stats.DashStamina);
            PlayWeaponAnimation("Dash",-1,0f);
            Managers.Event.PlayerEvents.OnEnablePlayerDash();

            moveDirection = new Vector3(_player.MoveDir.x, 0f, _player.MoveDir.y).normalized;
            moveDirection = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * moveDirection;
            _dashStartTime= Time.time;
            //Managers.Resource.Instantiate(_player.EffectPath + "Dash", _player.transform.position, _player.transform.rotation);
        }

        public override void UpdateState()
        {
            

            //??ш? ?앸궇 寃쎌슦
            float elapsedTime = Time.time - _dashStartTime;

            float speedFactor = _player.Stats.VelocityCurve.Evaluate(elapsedTime);
            if (speedFactor <= 0)
            {
                _player.trailRenderer.emitting = false;
                if (isDashing && _player.IsSprintPossible && _player.gameObject.name != "ExperimentPlayer")
                {
                        Debug.Log("달려");
                        _player.CurrentState = new PlayerSprintReadyState();
                        return;
                    
                }

                if (_player.MoveDir != Vector2.zero)
                {
                    _player.CurrentState = new PlayerMovingState();
                }
                else
                {
                }
            }





            if (moveDirection == Vector3.zero)
                moveDirection = _player.transform.forward;

            Vector3 velocity = moveDirection * _player.Stats.DashSpeed * speedFactor;

            // Rigidbody占쏙옙 占쌈듸옙 占쏙옙占쏙옙
            _player.Rb.linearVelocity = velocity;

            // 캐占쏙옙占싶곤옙 占싱듸옙占싹댐옙 占쏙옙占쏙옙占쏙옙占쏙옙 회占쏙옙
            if (moveDirection != Vector3.zero)
            {
                _player.Rb.rotation = Quaternion.LookRotation(moveDirection);
            }

        }

        public override void ExitState()
        {
            _player.trailRenderer.emitting = false;
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Dash;
        }

		public override void OnSprintCancel()
		{
			isDashing = false;
		}

        public override void AnimationEnd()
        {
            _player.CurrentState = new PlayerIdleState();
        }


        public override void OnDamaged(DamageCollider dm)        //주석 하면 무적 아님
        {

        }
        

	}

    public class PlayerHookShotState : PlayerHookState
    {

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            //???쒖옉
            _player.GrapplingGun.StartGrapple();

            //???ㅽ뙣????Idle濡??꾪솚
            if (!_player.GrapplingGun.IsGrappleSuccess)
            {
                _player.CurrentState = new PlayerIdleState();
                return;
            }


            //?뚮젅?댁뼱瑜????꾩튂瑜??ν빐 諛붾씪蹂닿쾶 留뚮뱺??
            Vector3 direction = _player.GrapplingGun.GetGrapplePoint() - _player.transform.position;
            Vector3 playerDirection = direction;
            playerDirection.y = 0;
            _player.Rb.rotation = Quaternion.LookRotation(playerDirection, Vector3.up);


            //?좊땲硫붿씠???ъ깮
            if (_player.Ani.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Fall")
            {
                _player.Ani.Play("HookAtAir");
            }
            else
            {
                _player.Ani.Play("HookAtGround");
            }
        }

        public override void UpdateState()
        {
            _player.Rb.linearVelocity = Vector3.zero;
            //洹몃옒?뚮쭅???꾨즺?먯쓣 寃쎌슦 ?뚮젮媛꾨떎.
            if (_player.GrapplingGun.IsGrappleDone)
            {
                _player.CurrentState = new PlayerHookingState();
            }
        }

        public override void ExitState()
        {
        }


    }

    public class PlayerHookingState : PlayerHookState
    {
        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            if (_player.Ani.GetCurrentAnimatorClipInfo(0)[0].clip.name == "HookAtAir")
            {
                _player.Ani.Play("HookFromAir");
            }
            else
            {
                _player.Ani.Play("HookFromGround");
            }
            Managers.Resource.Instantiate(_player.EffectPath + "Hooked", _player.GrapplingGun.GetGrapplePoint(), Quaternion.identity);
        }

        public override void UpdateState()
        {

            Vector3 targetPoint = _player.GrapplingGun.GetGrapplePoint() + new Vector3(0, 3, 0);
            Vector3 direction = targetPoint - _player.transform.position;

            //紐⑺몴吏?먭낵 ?꾩옱 ?뚮젅?댁뼱???꾩튂媛 硫 寃쎌슦
            if (Vector3.Distance(_player.transform.position, targetPoint) > 1f)
            {
                Vector3 playerDirection = direction;
                playerDirection.y = 0;
                _player.Rb.rotation = Quaternion.LookRotation(playerDirection, Vector3.up);
            }

            //占신뱄옙占쏙옙占쏙옙占쏙옙 占싱듸옙
            _player.Rb.linearVelocity = direction.normalized * 20f; // ?대?遺?rb.velocity 二쇱쓽


            //?뚮젅?댁뼱媛 紐⑺몴 吏?먯뿉 ?꾨떖?덉쓣 寃쎌슦
            if (Vector3.Distance(_player.transform.position, targetPoint) < 0.3f)
            {

                _player.GrapplingGun.StopGrapple();
                _player.Rb.linearVelocity = Vector3.zero;
                _player.Rb.AddForce(_player.transform.forward * 3f, ForceMode.Impulse);
                _player.CurrentState = new PlayerAirBorneState();

            }
        }

        public override void ExitState()
        {
            _player.GrapplingGun.StopGrapple();
        }


        public override void OnHookCancel()
        {
            if (_player.GrapplingGun.IsGrappleDone)
            {
                _player.GrapplingGun.StopGrapple();
                _player.CurrentState = new PlayerAirBorneState();
            }

        }
    }


    /*
    public class PlayerHurtState : PlayerState, AnimationCheck
    {

        private float hurtDuration = 0.5f;
        private float hurtStartTimer;

        public bool IsAnimationEnd { get; set; }

        public PlayerHurtState()
        {
            IsAnimationEnd = false;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            hurtStartTimer = Time.time;
            player.Rb.linearVelocity = Vector3.zero;
            PlayWeaponAnimation("Hurt", -1, 0f);
            UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
            ui.ShowHurtOverlay();

        }

        public override void UpdateState()
        {

            Vector2 input = _player.InputActions.Player.Move.ReadValue<Vector2>();
            if (input != Vector2.zero && Time.time - hurtStartTimer > hurtDuration)
            {
                _player.CurrentState = new PlayerMovingState();
            }
            if (IsAnimationEnd)
            {
                _player.CurrentState = new PlayerIdleState();
            }
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Hurt;
        }

        public override void OnDash(InputAction.CallbackContext context)
        {
            if (_player.Stats.CurrentStamina - _player.Stats.DashStamina < 0)
            {
                return;
            }
            if (Time.time - hurtStartTimer < hurtDuration)
                return;
            _player.CurrentState = new PlayerDashingState();
        }

        public override void OnHook(InputAction.CallbackContext context)
        {
            if (Time.time - hurtStartTimer < hurtDuration)
                return;
            if (_player.Stats.IsHookReady)
            {
                _player.CurrentState = new PlayerHookShotState();
            }
        }

    }

    */

    public class PlayerAirBorneState : PlayerHookState
    {

        DamageCollider _damageCollider;
        float _airBorneValue;

        public PlayerAirBorneState() 
        {
            _damageCollider = null;
        }

        public PlayerAirBorneState(DamageCollider dm) 
        {
            _damageCollider = dm;
            _airBorneValue= dm.stun.force;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            CanChangeStateByInput = true;

            if (_damageCollider != null)
            {
                Vector3 targetPosition = _damageCollider.Caster.transform.position;
                Vector3 direction = targetPosition - _player.transform.position;
                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    _player.Rb.rotation = targetRotation;
                }

                _player.Rb.linearVelocity = Vector3.zero;
                _player.Rb.linearVelocity = _player.transform.up * _airBorneValue;
                //_player.Rb.AddForce(_player.transform.up * _airBorneValue, ForceMode.Impulse);
            }
            _player.Ani.CrossFade("Fall", 0.05f);
        }

        public override void UpdateState()
        {
            if(_damageCollider != null)
            {
                if(_player.Rb.linearVelocity.y<=0.1f)
                    if (_player.IsGrounded())
                    {
                        _player.CurrentState = new PlayerLandingState();
                    }
                return;
            }
            if (_player.IsGrounded())
            {
                _player.CurrentState = new PlayerLandingState();
            }
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.AirBorne;
        }

        public override void OnHook()
        {
            if (_player.Stats.IsHookReady)
            {
                _player.CurrentState = new PlayerHookShotState();
            }
        }
    }

    public class PlayerLandingState : PlayerDefaultState
    {

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            _player.Ani.CrossFade("Land", 0.05f);
        }


        public override PlayerStateType GetState()
        {
            return PlayerStateType.Land;
        }

        public override void AnimationEnd()
        {
            _player.CurrentState = new PlayerIdleState();
        }

    }

    /*
    public class PlayerSleepState : PlayerState
    {
        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            Managers.Sound.Play("FemaleVoice/FemaleSleep");

        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Sleep;
        }
    }
    */
    public class PlayerDeadState : PlayerDefaultState
    {
        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            _player.Ani.CrossFade("Dead", 0.05f);
            //_player.Invoke("ScreenFadeOut", 1f);
            //Managers.Sound.Play("FemaleVoice/FemaleSleep");

            Vector3 pos = _player.transform.position;
            pos.y = 0;
            BuildingObjectInfo bundleItems = new BuildingObjectInfo();
			bundleItems.SpawnPos = pos;
			bundleItems.Init(421);
			bundleItems.SetSlotCount(Managers.Inven.GetCountSlotsFilled());
			bundleItems.PushAllItems();
            Managers.Map.ApplyEnter(bundleItems);

            _player.Revive();
		}

        public override void OnDamaged(DamageCollider dm)
        {

        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Dead;
        }
    }

    public class PlayerGrabbedState : PlayerDefaultState
    {
        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            _player.Ani.CrossFade("Grabbed", 0.05f);
            //Managers.Sound.Play("FemaleVoice/FemaleSleep");

        }

        public override void OnDamaged(DamageCollider dm)
        {
            _player.Stats.TakeDamage(dm.DamageCalculate(_player));
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Grabbed;
        }
    }
    public class PlayerWakeUpState : PlayerDefaultState
    {
        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            _player.Ani.Play("WakeUp");
        }


        public override PlayerStateType GetState()
        {
            return PlayerStateType.WakeUp;
        }

        public override void AnimationEnd()
        {
            _player.CurrentState = new PlayerIdleState();
        }
    }

    public class PlayerStunnedState : PlayerDefaultState
    {
        DamageCollider _damageCollider;



        float drag = 0f;
        float angularDrag = 0f;

        Coroutine coroutin;

        public PlayerStunnedState(DamageCollider dm)
        {
            _damageCollider = dm;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            _player.Rb.useGravity = false;
            drag = _player.Rb.linearDamping;
            angularDrag = _player.Rb.angularDamping;
            _player.Rb.linearDamping = 0f;
            _player.Rb.angularDamping = 0f;

            Vector3 targetPosition = _damageCollider.Caster == null ? Vector3.zero : _damageCollider.Caster.transform.position;
            Vector3 direction = targetPosition - _player.transform.position;
            direction.y = 0;

            _player.Rb.linearVelocity = Vector3.zero;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _player.Rb.rotation = targetRotation;
            }

            switch (_damageCollider.stun.type)
            {
                case StunType.Airborne:
                    coroutin = _player.StartCoroutine(HandleAirborne());
                    break;

                case StunType.NuckBack:
                    coroutin = _player.StartCoroutine(HandleNuckBack(direction));
                    break;
                case StunType.Pluck:
                    coroutin = _player.StartCoroutine(HandleNuckBack(-direction));
                    break;

            }
        }

        private IEnumerator HandleAirborne()
        {
            float _initialUpwardVelocity = 0f; // 최초 위로 튕겨진 속도
            _initialUpwardVelocity = _damageCollider.stun.force;

            Vector3 Velocity;
            Velocity = Vector3.up * _initialUpwardVelocity;
            PlayWeaponAnimation("Hurt", -1, 0f);

            while (true)
            {
                yield return new WaitForFixedUpdate();
                Debug.Log("Airborne State");

                Velocity += Physics.gravity * Time.fixedDeltaTime;
                _player.Rb.linearVelocity = Velocity;
                // 땅에 닿았는지 확인 (최고점 이후부터 체크)
                if (Velocity.y <= -_initialUpwardVelocity)
                {
                    Debug.Log("종료");
                    break;
                }
            }
            _player.CurrentState = new PlayerIdleState();
            ExitState();
        }

        private IEnumerator HandleNuckBack(Vector3 direction)
        {
            float _frictionCoefficient = 4f;
            Vector3 Velocity = -direction.normalized * _damageCollider.stun.force;
            PlayWeaponAnimation("Hurt", -1, 0f);
            CinemachineShake.Instance.ShakeCamera(1.5f, .2f);

            while (true)
            {
                yield return new WaitForFixedUpdate();

                // 마찰력 계산 (속도가 0에 가까워지면 마찰력도 적당히 감소하도록)
                Vector3 frictionForce = -_frictionCoefficient * Velocity.normalized * Velocity.magnitude;
                Velocity += frictionForce * Time.fixedDeltaTime;
                _player.Rb.linearVelocity = Velocity;


                // 넉백이 끝나는 조건 (속도가 너무 작아지면 멈추기)
                if (Velocity.magnitude < 0.1f)
                {
                    break;
                }
            }

            _player.CurrentState = new PlayerIdleState();
            ExitState();
        }

        public override void UpdateState()
        {
        }


        public override void ExitState()
        {
            base.ExitState();

            _player.Rb.useGravity = true;
            _player.Rb.linearDamping = drag;
            _player.Rb.angularDamping = angularDrag;
            if (coroutin != null)
            {
                _player.StopCoroutine(coroutin);
            }
        }
        public override PlayerStateType GetState()
        {
            return PlayerStateType.Stun;
        }
    }

    #region InteractionSkill
    public class PlayerInteractionSkillReadyState : PlayerState
	{
		int _mask = (1 << (int)Define.Layer.Block) | (1 << (int)Define.Layer.Default) | (1 << (int)Define.Layer.Monster);
		GameObject _hoveringObject;
		UI_InteractionMap _interactUI;

		public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            //상호작용 애니메이션 틀기
            PlayWeaponAnimation("Idle", 0.05f);

			_interactUI = Managers.UI.ShowPopupUI<UI_InteractionMap>();
		}
        
        public override void UpdateState()
        {
            //마우스 방향을 바라본다.
            LookAtMousePos();
            GetInputMouse();
			//만약 상호작용 가능한 오브젝트를 눌렀을 경우 InteractionSkillUsingState로 넘긴다 <- 으차햄이 작성해주셔야함요
			//_player.CurrentState = new PlayerInteractionSkillUsingState(); //<- State 넘기는 방법
		}

		void GetInputMouse()
		{
            // hovering
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			bool raycastHit = Physics.Raycast(ray, out hit, 100.0f, _mask);
			if (raycastHit && hit.collider.GetComponentInParent<PatternedWall>())
			{
				PatternedWall wall = hit.collider.GetComponentInParent<PatternedWall>();
				if ((wall.GridPos - _player.GridPos).sqrMagnitude <= 1 && wall.IsMoving == false)
				{
					if (_hoveringObject != null)
						_hoveringObject.GetComponent<SelectedOutline>().ClearOutline();
					_hoveringObject = wall.gameObject;
					_hoveringObject.GetComponent<SelectedOutline>().DrawOutline();
				}

			}
			else if (raycastHit && hit.collider.CompareTag("Star"))
			{
				Vector3 dir = hit.collider.transform.position - _player.transform.position;
                float distance = dir.magnitude;
				bool hit2 = Physics.Raycast(_player.transform.position, dir, distance, 1 << (int)Define.Layer.Block);
                if (hit2 == false && distance <= 18.0f)
				{
					if (_hoveringObject != null)
						_hoveringObject.GetComponent<SelectedOutline>().ClearOutline();
					_hoveringObject = hit.collider.gameObject;
					_hoveringObject.GetComponent<SelectedOutline>().DrawOutline();
				}
			}
			else if (_hoveringObject != null)
			{
				_hoveringObject.GetComponent<SelectedOutline>().ClearOutline();
				_hoveringObject = null;
			}


            // Click
			if (Input.GetMouseButtonUp(0))
			{
				if (_hoveringObject != null)
				{
                    if (_hoveringObject.GetComponent<PatternedWall>())
                        _interactUI.SelectObject(_hoveringObject);
                    else if (_hoveringObject.GetComponent<StarPiece>())
                    {
						_hoveringObject.GetComponent<SelectedOutline>().ClearOutline();
						_interactUI.ShowRadar();
                    }
					_hoveringObject = null;
					_player.CurrentState = new PlayerInteractionSkillUsingState();
				}
			}
		}

        public override void ExitState()
        {
			if (_hoveringObject != null)
			{
				_hoveringObject.GetComponent<SelectedOutline>().ClearOutline();
				_hoveringObject = null;
			}
		}

        /*
        public override void OnSkillRelease(SkillInfo2 skill)
        {
            if (skill as InteractionSkillInfo != null) 
            {
                _player.CurrentState = new PlayerIdleState();
            }
        }
        */

        public override void OnDamaged(DamageCollider dm)
        {
            base.OnDamaged(dm);
            if (_player.CurrentState == this)
            {
                _player.CurrentState = new PlayerIdleState();
            }
        }

        public virtual void LookAtMousePos()
        {
            Vector3 worldPos = _player.transform.position;
            Vector3 mousePosition = Input.mousePosition;

            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(_player.transform.position.x, _player.transform.position.y, _player.transform.position.z));

            float rayLength;
            if (groundPlane.Raycast(ray, out rayLength))
            {
                worldPos = ray.GetPoint(rayLength);
            }
            Vector3 direction = worldPos - _player.transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _player.Rb.rotation = targetRotation;
            }
        }
    }

    public class PlayerInteractionSkillUsingState : PlayerState
    {

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            //상호작용 애니메이션 틀기
            PlayWeaponAnimation("Idle", 0.05f);

        }

        //상호작용 대상 오브젝트를 바라본다.
        

        public override void ExitState()
        { 
        }


        /*
        //Ctrl을 한번 더 누를 시 상호작용 모드를 종료하고 Idle로 돌아간다.
        public override void OnSkill(InputAction.CallbackContext context, SkillInfo2 skill)
        {
            if (skill as InteractionSkillInfo != null)
            {
                _player.CurrentState = new PlayerIdleState();
            }
        }
        */

        public override void OnDamaged(DamageCollider dm)
        {
            base.OnDamaged(dm);
            if (_player.CurrentState == this)
            {
                _player.CurrentState = new PlayerIdleState();
            }
        }
    }
    #endregion

    public class PlayerFaintedState : PlayerState
    {
        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            _player.Ani.Play("Dead");
            _player.StartPlayerCoroutine(ref _player.MentalCoroutin, MentalRecoveryCoroutin());
            Managers.Event.PlayerEvents.OnDisablePlayerMovement();
        }

        public override void ExitState()
        {
            _player.StartPlayerCoroutine(ref _player.MentalCoroutin, _player.MentalReduceCoroutin());
        }

        IEnumerator MentalRecoveryCoroutin()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);
                _player.Mental += 1f;
                if (_player.Mental >= 10)
                    _player.CurrentState = new PlayerIdleState();
            }
        }
    }

    #region building
    public class MoveToBuildingPosState : PlayerDefaultState
    {
        Equipment _currentWeapon;
        PreviewObject _previewObject;

        public MoveToBuildingPosState(PreviewObject previewObject)
        {
            _previewObject = previewObject;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
			PlayWeaponAnimation("Run", 0.05f);
            _currentWeapon = _player.CurrentWeapon;
        }

        public override void UpdateState()
        {
            //PreviewObject를 향해 바라보고 앞으로 이동
            Vector3 direction = (_previewObject.transform.position - _player.transform.position).normalized;
            direction.y = 0; // Y축 회전만 허용
            _player.transform.forward = direction;

            // 앞으로 이동
            _player.Rb.linearVelocity = direction * _player.MoveSpeed;

            //PreviewObject와 가까워졌을 경우 BuildingState로 넘기기
            if (Vector3.Distance(_player.transform.position, _previewObject.GetComponent<Collider>().ClosestPoint(_player.transform.position)) < 1f)
                _player.CurrentState = new BuildingState(_previewObject);

            //땅으로 떨어질 경우 취소
            if (!_player.IsGrounded())
            {
                _player.CurrentState = new PlayerAirBorneState();
            }

            //중간에 무기를 바꿀 경우 취소
            if (_currentWeapon != _player.CurrentWeapon)
            {
                _player.CurrentState = new PlayerIdleState();
            }
        }

        public override void ExitState()
		{
			UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;

            //다음 스테이트가 BuildingState가 아닌 경우 취소
            if (_player.NextState.GetState() != PlayerStateType.Building)
            {
                _player.GetComponent<BuildingSystem>().Cancel();
                ui.CloseKeyGuide();
            }
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Building;
		}

		public override void OnDash()
		{
			_player.CurrentState = new PlayerIdleState();
		}
	}

    public class BuildingState : PlayerDefaultState
    {
        Equipment _currentWeapon;
        PreviewObject _previewObject;
        public BuildingState(PreviewObject previewObject)
        {
            _previewObject = previewObject;
        }

        public override void EnterState(PlayerController player)
        {
			base.EnterState(player);
            //Building 애니메이션 추가 필요
            _player.Ani.CrossFade("Building", 0.05f);
            _currentWeapon = _player.CurrentWeapon;
            _previewObject.StartBuilding(_currentWeapon.Damage);
        }

        public override void UpdateState()
        {
            //땅으로 떨어질 경우 취소
            if (!_player.IsGrounded())
            {
                _player.CurrentState = new PlayerAirBorneState();
            }

            //중간에 무기를 바꿀 경우 취소
            if (_currentWeapon != _player.CurrentWeapon)
            {
                _player.CurrentState = new PlayerIdleState();
            }

            //움직임을 받을 경우 Building 취소
            if (_player.MoveDir != Vector2.zero)
            {
                _player.CurrentState = new PlayerMovingState();
            }
        }

        public override void ExitState()
        {
            _player.GetComponent<BuildingSystem>().Cancel();
			UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
			ui.CloseKeyGuide();
		}

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Building;
		}

		public override void OnDash()
		{
			_player.CurrentState = new PlayerIdleState();
		}


    }
    #endregion

    #region Building Mode, Demolition Mode
    public class BuildingModeState : PlayerDefaultState
    {
        Equipment _currentWeapon;

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
			//Building 애니메이션 추가 필요
			PlayWeaponAnimation("Idle", 0.05f);
            _currentWeapon = _player.CurrentWeapon;

            _player.GetComponent<BuildingSystem>().IsDemolitionMode = true;
            UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
            ui.ShowBuildingUI(_player.GetComponent<BuildingSystem>());
            ui.ShowKeyGuide(3);
		}

        public override void UpdateState()
        {

            /*
            Vector2 input = _player.InputActions.Player.Move.ReadValue<Vector2>();

            Vector3 moveDirection = new Vector3(input.x, 0f, input.y).normalized; // 占싱듸옙 占쏙옙占쏙옙 占쏙옙占쏙옙

            // 占쏙옙占쏙옙占쏙옙 占쏙옙占싶몌옙 카占쌨띰옙占쏙옙 占쏙옙占쏙옙占쏙옙占쏙옙 占쏙옙환占싹울옙 占쏙옙占쏙옙 占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙
            moveDirection = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * moveDirection;
            moveDirection.y = 0;

            // Rigidbody占쏙옙 占쌈듸옙 占쏙옙占쏙옙 占쏙옙占쏙옙
            _player.Rb.linearVelocity = moveDirection * _player.Stats.CurrentMoveSpeed + Vector3.up * _player.Rb.linearVelocity.y;

            // 캐占쏙옙占싶곤옙 占싱듸옙占싹댐옙 占쏙옙占쏙옙占쏙옙占쏙옙 회占쏙옙
            if (moveDirection != Vector3.zero)
            {
                PlayWeaponAnimation("Run",0.05f);
                // _player.Rb.rotation = Quaternion.LookRotation(moveDirection); 
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                //_player.Rb.rotation = Quaternion.Slerp(_player.Rb.rotation, targetRotation, Time.deltaTime * 10f);
                _player.Rb.MoveRotation(Quaternion.Slerp(_player.Rb.rotation, targetRotation, Time.deltaTime * 10f)); //李쎌닔
            }
            else
            {
                _player.Rb.linearVelocity = Vector3.zero;
                _player.Ani.SetBool("IsMoveStop", true);
            }
            */

            //땅으로 떨어질 경우 취소
            if (!_player.IsGrounded())
            {
                _player.CurrentState = new PlayerAirBorneState();
            }

            //중간에 무기를 바꿀 경우 취소
            if (_currentWeapon != _player.CurrentWeapon)
            {
                _player.CurrentState = new PlayerIdleState();
            }
        }

        public override void ExitState()
        {
			UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
            ui.CloseBuildingUI();

            _player.GetComponent<BuildingSystem>().IsDemolitionMode = false;
            //다음 스테이트가 BuildingState가 아닌 경우 취소
            if (_player.NextState.GetState() != PlayerStateType.Building)
            {
                _player.GetComponent<BuildingSystem>().Cancel();
                ui.CloseKeyGuide();
            }
		}

        public override void OnDash()
        {
            _player.CurrentState = new PlayerIdleState();
        }
    }

    public class DemolitionModeState : PlayerDefaultState
    {
        Equipment _currentWeapon;

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
			//Building 애니메이션 추가 필요
			PlayWeaponAnimation("Idle", 0.05f);
            _currentWeapon = _player.CurrentWeapon;

            _player.GetComponent<BuildingSystem>().IsDemolitionMode = true;
			UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
			ui.ShowKeyGuide(3);
		}

        public override void UpdateState()
        {
            /*
            Vector2 input = _player.InputActions.Player.Move.ReadValue<Vector2>();

            Vector3 moveDirection = new Vector3(input.x, 0f, input.y).normalized; // 占싱듸옙 占쏙옙占쏙옙 占쏙옙占쏙옙

            // 占쏙옙占쏙옙占쏙옙 占쏙옙占싶몌옙 카占쌨띰옙占쏙옙 占쏙옙占쏙옙占쏙옙占쏙옙 占쏙옙환占싹울옙 占쏙옙占쏙옙 占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙
            moveDirection = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * moveDirection;
            moveDirection.y = 0;

            // Rigidbody占쏙옙 占쌈듸옙 占쏙옙占쏙옙 占쏙옙占쏙옙
            _player.Rb.linearVelocity = moveDirection * _player.Stats.CurrentMoveSpeed + Vector3.up * _player.Rb.linearVelocity.y;

            // 캐占쏙옙占싶곤옙 占싱듸옙占싹댐옙 占쏙옙占쏙옙占쏙옙占쏙옙 회占쏙옙
            if (moveDirection != Vector3.zero)
            {
                PlayWeaponAnimation("Run", 0.05f);
                // _player.Rb.rotation = Quaternion.LookRotation(moveDirection); 
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                //_player.Rb.rotation = Quaternion.Slerp(_player.Rb.rotation, targetRotation, Time.deltaTime * 10f);
                _player.Rb.MoveRotation(Quaternion.Slerp(_player.Rb.rotation, targetRotation, Time.deltaTime * 10f)); //李쎌닔
            }
            else
            {
                _player.Rb.linearVelocity = Vector3.zero;
                _player.Ani.SetBool("IsMoveStop", true);
            }

            */

            //땅으로 떨어질 경우 취소
            if (!_player.IsGrounded())
            {
                _player.CurrentState = new PlayerAirBorneState();
            }

            //중간에 무기를 바꿀 경우 취소
            if (_currentWeapon != _player.CurrentWeapon)
            {
                _player.CurrentState = new PlayerIdleState();
            }
        }

        public override void ExitState()
		{
			_player.GetComponent<BuildingSystem>().IsDemolitionMode = false;
			UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
			ui.CloseKeyGuide();
		}

		public override void OnDash()
        {
            _player.CurrentState = new PlayerIdleState();
        }
    }

    #endregion

    #region Event
    public class MoveToEventPosState : PlayerState
    {
        Vector3 _targetPosition;

        public MoveToEventPosState(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            PlayWeaponAnimation("Run", 0.05f);

            UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
            if (ui != null)
                ui.HideUI();
        }

        public override void UpdateState()
        {
            //PreviewObject를 향해 바라보고 앞으로 이동
            Vector3 direction = (_targetPosition - _player.transform.position).normalized;
            direction.y = 0; // Y축 회전만 허용
            _player.transform.forward = direction;

            // 앞으로 이동
            _player.Rb.linearVelocity = direction * _player.MoveSpeed;

            if (Vector3.Distance(_player.transform.position, _targetPosition) < 0.2f)
                _player.CurrentState = new EventAnimationPlayState();
        }

        public override void ExitState()
		{
            _player.Rb.linearVelocity = Vector3.zero;
			UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
			if (ui != null)
				ui.ShowUI();
		}
    }

    public class EventAnimationPlayState : PlayerState
    {
        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            _player.Ani.CrossFade("PickUp", 0.05f);
            _player.Interact();
        }

        public override void AnimationEnd()
        {
            _player.CurrentState = new PlayerIdleState();
        }
    }

    #endregion


    //플레이어 특정 오브젝트와 상호작용할 때를 처리하는 State
    public class PlayerInteractionState : PlayerDefaultState
    {
        PlayerInteractionObjectType _interactionType;
        Interactable _interactable;
        Vector3 _playerPos;
        Vector3 _playerLookDirection;
        public PlayerInteractionState(PlayerInteractionObjectType type, Vector3 playerPos, Vector3 lookDirection, Interactable interactable)
        {
            //넘겨받은 상호작용 enum Type에 따라서 애니메이션을 다르게 틀어줌
            _interactionType = type;
            _interactable = interactable;
            _playerPos = playerPos;
            _playerLookDirection = lookDirection;

        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
			_player.Rb.isKinematic = true;
			_player.Rb.MovePosition(_playerPos);
            _player.Rb.MoveRotation(Quaternion.LookRotation(_playerLookDirection));
			switch (_interactionType)
            {
                case PlayerInteractionObjectType.Tent:
                    _player.StartPlayerCoroutine(ref _player.HPCoroutin, HPRecoveryCoroutin(2));
                    _player.Ani.CrossFade("SitIdle", 0.05f);
                    break;

                case PlayerInteractionObjectType.BonFire:
                    _player.StartPlayerCoroutine(ref _player.HPCoroutin, HPRecoveryCoroutin(1));
                    _player.Ani.CrossFade("SitIdle",0.05f);
                    break;

                case PlayerInteractionObjectType.Default:
                    PlayWeaponAnimation("Idle",0.05f);
                    break;
            }

            UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
            ui.ShowKeyGuide(5);
        }

        public override void UpdateState()
        {
        }

        public override void ExitState()
        {
            switch (_interactionType)
            {
                case PlayerInteractionObjectType.Tent:
                    _player.StopPlayerCoroutine(ref _player.HPCoroutin);
                    break;

                case PlayerInteractionObjectType.BonFire:
                    _player.StopPlayerCoroutine(ref _player.HPCoroutin);
                    break;

                case PlayerInteractionObjectType.Default:
                    break;
            }
            
            

            _player.Rb.isKinematic = false;
            //인터랙션 종료 
            if(_interactable != null)
                _interactable.Cancel();

            UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
            ui.CloseKeyGuide();
        }

        public override void OnDash()
        {
            _player.CurrentState = new PlayerIdleState();
        }

        IEnumerator MentalRecoveryCoroutin()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);
                _player.Mental += 1f;
            }
        }

        IEnumerator HPRecoveryCoroutin(float amount)
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);
                _player.Hp += amount;
            }
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Interaction;
        }


    }


    #region ParrySkill
    public class BlockState : PlayerDefaultState
    {
        ParrySkillInfo _skillInfo;
        public bool IsParrySuccessTimeOver { get; set; }
        public BlockState(ParrySkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }

        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);

            IsParrySuccessTimeOver = false;


            LookAtPos();
            PlayWeaponAnimation("BlockSkill", -1, 0f);
        }


        public override PlayerStateType GetState()
        {
            return PlayerStateType.Attack;
        }

        public override void OnDamaged(DamageCollider dm)
        {
            if (IsParrySuccessTimeOver)
            {
                base.OnDamaged(dm);
            }
            else
            {
                if (dm.DamageCalculate(_player) == 0)
                    return;
                _player.CurrentState = new ParrySuccessState(dm);
            }
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

        public override void AnimationEnd()
        {
            _player.CurrentState = new PlayerIdleState();
        }
    }

    public class ParrySuccessState : PlayerDefaultState  //성공하면 열로 넘어옴
    {

        public DamageCollider damageCollider;

        Coroutine nuckBackCoroutin;

        public ParrySuccessState(DamageCollider dm)
        {
            damageCollider = dm;
        }

        //시간이 느려지고 그안에 입력(마우스 왼클릭)이 있을 경우 스킬 즉발
        public override void EnterState(PlayerController player)
        {
            base.EnterState(player);
            //Time.timeScale = 0.2f;

            Debug.Log($"[패링 전] 스태미나: {_player.CurrentStaminaCount} / {_player.MaxStaminaCount}");
            //_player.CurrentStaminaCount = _player.MaxStaminaCount; 패링 성공 시 5개 회복
            _player.CurrentStaminaCount = Mathf.Min(_player.CurrentStaminaCount + 3, _player.MaxStaminaCount);
            //_player.StartCoroutine(DoHitStop(0.05f));       //HitStop 0.05초간 정지

            Debug.Log($"[패링 후] 스태미나: {_player.CurrentStaminaCount} / {_player.MaxStaminaCount}"); LookAtPos();
            PlayWeaponAnimation("CounterReady", -1, 0f);
            _player.StartCoroutine(InputDelay());
            nuckBackCoroutin = _player.StartCoroutine(HandleNuckBack(damageCollider.Caster.transform.position - _player.transform.position));
            Managers.Resource.Instantiate("Effects/Player/Parry", _player.transform.position, Quaternion.identity);
            Managers.Sound.Play("Sounds/Parrysucceed", Sound.Effect, 1);
            CinemachineShake.Instance.ShakeCamera(3, .5f);
        }


        private IEnumerator HandleNuckBack(Vector3 direction)
        {
            float _frictionCoefficient = 4f;
            Vector3 Velocity = -direction.normalized * 7f;

            while (true)
            {
                yield return new WaitForFixedUpdate();
                Debug.Log("넉백중");

                // 마찰력 계산 (속도가 0에 가까워지면 마찰력도 적당히 감소하도록)
                Vector3 frictionForce = -_frictionCoefficient * Velocity.normalized * Velocity.magnitude;
                Velocity += frictionForce * Time.fixedDeltaTime;
                _player.Rb.linearVelocity = Velocity;


                // 넉백이 끝나는 조건 (속도가 너무 작아지면 멈추기)
                if (Velocity.magnitude < 0.1f)
                {
                    break;
                }
            }
            _player.CurrentState = new PlayerIdleState();
        }

        public override PlayerStateType GetState()
        {
            return PlayerStateType.Attack;
        }

        IEnumerator InputDelay()
        {
            yield return new WaitForSeconds(0.2f);  //인풋딜레이
        }

        public override void OnDamaged(DamageCollider dm)
        {

        }

        public override void ExitState()
        {
            _player.Rb.linearVelocity = Vector3.zero;
            _player.StopCoroutine(nuckBackCoroutin);
        }

        private void LookAtPos()
        {
            Vector3 direction = damageCollider.Caster.transform.position - _player.transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _player.Rb.rotation = targetRotation;
            }
        }
        public IEnumerator DoHitStop(float duration)      //HitStop 타격감을 위한 약간 정지효과 by 창수
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }

        public override void AnimationEnd()
        {
            _player.CurrentState = new PlayerIdleState();
        }
    }
    #endregion

}
