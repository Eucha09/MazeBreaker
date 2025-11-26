using Data;
using JetBrains.Annotations;
using Monster;
using Monster.Attack;
using Monster.Search;
using NUnit.Framework;
using Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Define;

public class MonsterController2 : BaseController
{
    public bool isBoss = false;


    [System.Serializable]
    public class TargetInfo
    {
        public float lastDetactiontime;
        public Transform target;
    }


    #region Sight
    [Header("Sight Properties")]
    //시야각 관련 Variables
    public float viewRadius;
    [UnityEngine.Range(0, 360)]
    public float viewAngle;
    public LayerMask targetLayer;
    public LayerMask obstacleLayer;
    protected float targetDetectionTime;
    [SerializeField]
    protected float maxDetectionLostTime = 10f;
    protected float originalViewRadius;

    public float TargetDetectionTime { get { return targetDetectionTime; } set { targetDetectionTime = value; } }
    public float MaxDetectionLostTime { get { return maxDetectionLostTime; } set { maxDetectionLostTime = value; } }
    [SerializeField]
    public List<TargetInfo> targets;

    #endregion



    public enum SearchType
    {
        Default,
        Patrol,
        Chase,
        Alert,
        None
    }

    public SearchType searchType;

    [Header("Chase Properties")]
    public bool ChaseTrigger = false;
	public Vector3 ChasingPos { get; set; } = Vector3.zero;
	//ChaseTrigger가 True일 경우 ChasingPos를 향해 감, False일 경우 다시 자기 스폰 포인트로 감

    public void SetChasingPos(Vector3 pos, bool chaseTrigger = true)
    {
        ChasingPos = pos;
        ChaseTrigger = chaseTrigger;
	}

    public void ChasingEnd()
    {
        ChaseTrigger = false;
	}

	#region RoleSetting
	public enum RoleType
    {
        Solo,
        Leader,
        Member
    }

    public RoleType roleType;

    /*모든 멤버들의 정보를 갖고 있음.
    포메이션 정보를 갖고 있고, 각 멤버들에게 자신이 있어야할 포지션 값을 넘겨준다.  
    적 발견시 모든 멤버들에게 적 정보를 공유한다.
    Leader사망시에 Member중 한명에게 Leader를 위임한다.*/
    [Header("Leader Properties")]
    public List<MonsterController2> members;


    //리더 위임(리더가 죽을떄 호출)
    public void DelegateLeaderToMember()
    {
        if (members.Count == 0)
            return;
        members[0].roleType = RoleType.Leader;
        members[0].members = members;

        //리더가 member[0]로 바꼈음을 다른 대원들에게 알려준다.
        for (int i = 1; i < members.Count; i++)
        {
            members[i].leader = members[0];
        }
        MonsterController2 delegatedLeader = members[0];
        members.Remove(members[0]);
        delegatedLeader.searchType = searchType;
    }

    /*리더의 정보를 갖고있으며, 적 발견시 리더의 타겟에 해당 적을 추가한다.
    비전투시에는 리더로부터 받은 포지션 값으로 이동한다.
    멤버들은 죽을 때 자기자신이 죽었다고 리더에게 보고한다.
    리더로부터 타겟들의 정보를 가지고 있는다..*/
    [Header("Member Properties")]
    public MonsterController2 leader;

    //리더에게 자신이 죽었다고 보고(멤버가 죽을 때)
    public void ReportDeathToLeader()
    {
        //리스트에서 자기자신 제거
        leader.members.Remove(this);
    }

    #endregion

    #region ETC
    protected Transform _mainTarget;
    protected TargetInfo _mainTargetInfo;
    protected MonsterController2 _parent = null;
    protected Animator _ani;
    protected NavHybridAgent _nma;
    protected Rigidbody _rb;
    protected Vector3 _spawnPoint;
    protected Quaternion _spawnRotation;
    bool _isStartedMethodCalled = false;

    public Transform MainTarget { get { return _mainTarget; } set { _mainTarget = value;  } }
    public TargetInfo MainTargetInfo { get { return _mainTargetInfo; } set { _mainTargetInfo = value; if(MainTargetInfo != null) MainTarget = _mainTargetInfo.target; } }
    public MonsterController2 Parent { get { return _parent; } set { _parent = value; } }
    public Animator Ani { get { return _ani; } set { _ani = value; } }
    public NavHybridAgent Nma { get { return _nma; } set { _nma = value; } }
    public Rigidbody Rb { get { return _rb; } set { _rb = value; } }
    public Vector3 SpawnPoint { get { return _spawnPoint; } private set { _spawnPoint = value; } }
    public Quaternion SpawnRotation { get { return _spawnRotation; } private set { _spawnRotation = value; } }

    public List<Renderer> Mesh;

    [Header("Attack Glow")]
    protected List<Material> _glowMat;
    protected List<Material> _baseMat;
    public Color targetColor = Color.red; // 목표 컬러
    public float colorChangeDuration = 1.0f; // 컬러 전환에 걸리는 시간
    public Color initialColor;//초기 컬러
    List<Color> _originalBaseColors = new List<Color>();

    public List<Material> GlowMat { get { return _glowMat; } set { _glowMat = value; } }
    public List<Material> BaseMat { get { return _baseMat; } set { _baseMat = value; } }

    #endregion


    #region Skill
    [Header("Pattern Data")]
    [SerializeField]
    //패턴들을 저장하는 변수
    protected List<PatternInfo> _patterns = new List<PatternInfo>();
    public List<PatternInfo> Patterns { get { return _patterns; } private set { } }
    public PatternInfo CurrentPatternInfo { get; set; } = new PatternInfo();

    public string CurrentPatternName = "";

    // ✅ 현재 활성화된 인디케이터 저장용 - 창수(스턴 시 인디케이터 제거, 검토필요)
    public SectorIndicatorNoShader currentIndicator;
    #endregion

    #region Stats
    [Header("Stats")]
    [SerializeField]
    protected float _hp;
    public override float Hp { get { return _hp; } set { if (value > MaxHp) _hp = MaxHp; else if (value < 0) _hp = 0; else _hp = value; } }
    [SerializeField]
    protected float _maxHp;
    public override float MaxHp { get => _maxHp; set => _maxHp = value; }
    [SerializeField]
    protected float _attack = 10;
    public override float Attack { get => _attack; set => _attack = value; }
    [SerializeField]
    protected float _defense = 1;
    public override float Defense { get => _defense; set => _defense = value; }
    [SerializeField]
    private int _weakness = 0;
    public int Weakness
    {
        get => _weakness;
        set
        {
            if (value < 0) _weakness = 0;
            else if (value > MaxWeakness) _weakness = MaxWeakness;
            else _weakness = value;
        }
    }

    [SerializeField]
    private int _maxWeakness = 3;
    public int MaxWeakness { get => _maxWeakness; set => _maxWeakness = value; }

    // 마지막으로 그로기 변화가 있었던 시간
    private float LastWeaknessAffectedTime { get; set; } = 0f;

    private float _originalMaxHp;
    private float _originalAttack;
    private float _originalDefense;

    // 감소 시작까지 대기 시간
    private float WeaknessDisappearDuration { get; set; } = 5f;

    public void AffectWeakness(int amount)
    {
        LastWeaknessAffectedTime = Time.time;
        Weakness += amount;
        if (_weaknessDisappearCoroutine != null)
            StopCoroutine(_weaknessDisappearCoroutine);
        _weaknessDisappearCoroutine = StartCoroutine(WeaknessDiappearRoutine());
    }

    public IEnumerator WeaknessDiappearRoutine()
    {
        float elapsed = 0f;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            elapsed += Time.fixedDeltaTime;

            if (elapsed >= WeaknessDisappearDuration)
            {
                Weakness = 0;
                yield break; // 코루틴 종료
            }
        }
    }

    public Coroutine _weaknessDisappearCoroutine;

    public bool IsGroggy { get; set; } = false;
    #endregion

    //_currentState는 상세 상태(Idle, move, 등등)
    Monster.State _currentState;

    public Monster.State CurrentState
    {
        get { return _currentState; }
        set
        {

            if (_currentState != null)
            {
                _currentState.ExitState();
            }

            _currentState = value;
            _currentState.EnterState(this);
        }
    }

    #region ObjectInfoBinder

	public override void Bind(CreatureInfo info)
	{
        Info = info;

        TemplateId = Info.ObjectId;
        IsDead = Info.IsDead;
        ChaseTrigger = Info.ChaseTrigger;
        ChasingPos = Info.ChasingPos;

		if (_isStartedMethodCalled)
		{
			SpawnPoint = Info.SpawnPos;
			SpawnRotation = Info.Rotation;
			Hp = IsDead ? 0.0f : MaxHp;
			gameObject.SetActive(!IsDead);
		}
	}

	public override void Refresh()
	{
		if (Info == null)
			return;

		if (IsDead && Info.IsDead == false)
		{
			IsDead = false;
			transform.position = Info.SpawnPos; // TODO
			Hp = MaxHp;
			gameObject.SetActive(true);
			LerpAppearDissolveStart();
		}
	}

	public override void Unbind()
	{
        Info.ChaseTrigger = ChaseTrigger;
        Info.ChasingPos = ChasingPos;
		Info = null;
	}
    #endregion

	protected virtual void Start()
	{
		_isStartedMethodCalled = true;

		Data.ObjectData objectData = null;
        Managers.Data.ObjectDict.TryGetValue(TemplateId, out objectData);
        if (objectData != null)
        {
            ObjectType = objectData.objectType;
            MaterialType = objectData.materialType;
            MaxHp = objectData.stat.maxHp;
            Hp = MaxHp;
            Attack = objectData.stat.attack;
            Defense = objectData.stat.defense;
            _originalMaxHp = objectData.stat.maxHp;
            _originalAttack = objectData.stat.attack;
            _originalDefense = objectData.stat.defense;
        }

		Nma = gameObject.GetOrAddComponent<NavHybridAgent>();
        Ani = GetComponent<Animator>();
        Rb = GetComponent<Rigidbody>();
        originalViewRadius = viewRadius;
        SpawnPoint = transform.position;
        SpawnRotation = transform.rotation;
        if (Nma != null)
        {
            MoveSpeed = Nma.speed;
            CurrentMoveSpeed = Nma.speed;
        }

        // MeshRenderer 리스트 가져오기
        List<Renderer> meshRenderer = Mesh ?? new List<Renderer>(); // Mesh가 null일 경우 초기화
        if (meshRenderer.Count == 0) // 리스트가 비어 있다면 GetComponentInChildren로 추가
        {
            Renderer childRenderer = GetComponentInChildren<Renderer>();
            if (childRenderer != null)
                meshRenderer.Add(childRenderer);
        }

        // BaseMat과 GlowMat 리스트 초기화
        if (_baseMat == null) _baseMat = new List<Material>();
        if (_glowMat == null) _glowMat = new List<Material>();

        // 각 Renderer에 대해 처리
        foreach (Renderer renderer in meshRenderer)
        {
            Material[] materials = renderer.materials; // Material 배열 복사본
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = Instantiate(materials[i]); // Material 인스턴스화
            }
            renderer.materials = materials; // 변경된 배열을 다시 할당

            // BaseMat과 GlowMat 추가
            if (materials.Length > 0)
            {
                _baseMat.Add(materials[0]);
                materials[0].SetFloat("_DissolveAmount", 0.0f); // Dissolve 초기화
            }
            if (materials.Length > 1)
            {
                _glowMat.Add(materials[1]);
            }
        }

        //오리지널 BaseMat Color 저장
        foreach (Material baseMat in _baseMat)
        {
            if (baseMat.HasProperty("_BaseColor")) // ✅ 안전 체크
            {
                _originalBaseColors.Add(baseMat.GetColor("_BaseColor"));
            }
            else
            {
                _originalBaseColors.Add(Color.white); // 기본값 추가 (예외 방지)
            }
        }

        //시야각
        StartCoroutine(DetectTargetsRoutine());

        if (Info != null)
		{
            SpawnPoint = Info.SpawnPos;
            SpawnRotation = Info.Rotation;
			Hp = IsDead ? 0.0f : MaxHp;
			gameObject.SetActive(!IsDead);
		}
	}

	protected virtual void OnEnable()
    {
        if (!_isStartedMethodCalled)
            return;

        CurrentState = new Monster.Search.Idle();
        StartCoroutine(DetectTargetsRoutine());
        for (int i = 0; i < _baseMat.Count; i++)
        {
            if (i < _originalBaseColors.Count) // ✅ 인덱스 초과 방지
            {
                if (_baseMat[i].HasProperty("_BaseColor")) // ✅ 안전 체크
                {
                    _baseMat[i].SetColor("_BaseColor", _originalBaseColors[i]);
                }
            }
        }
        foreach (Material glowMat in _glowMat)
        {
            glowMat.SetColor("_Color", initialColor);
        }

        Collider[] colliders = GetComponents<Collider>();
		foreach (Collider col in colliders)
			col.enabled = true;

        foreach (Material baseMat in _baseMat)
            baseMat.SetFloat("_DissolveAmount", 0.0f);

        MainTarget = null;
        MainTargetInfo = null;
        targets.Clear();

  //      if (_coDissolve != null)
  //      {
  //          StopCoroutine(_coDissolve);
  //          LerpDissolveStart();
		//}
  //      if (_coAppearDissolve != null)
  //      {
  //          StopCoroutine(_coAppearDissolve);
  //          LerpAppearDissolveStart();
		//}
	}

    protected virtual void OnDisable()
	{
		//NavMeshAgent nma = GetComponent<NavMeshAgent>();
		//if (nma != null)
		//	nma.Warp(SpawnPoint);
		//else
		//	transform.position = SpawnPoint;

		if (isBoss && isPlayerExist)
		{
			isPlayerExist = false;
			Managers.Sound.EndBattle(this.gameObject);
		}
	}


    protected void Update()
    {
        foreach(PatternInfo p in _patterns)
        {
            p.UpdateRemainingCoolTime();
        }
        if (roleType == RoleType.Member)
        {
            ChaseTrigger = leader.ChaseTrigger;
            ChasingPos = leader.ChasingPos;
        }
    }

    protected virtual void FixedUpdate()
    {
        CurrentState.UpdateState();
        //자신을 소환한 부모가 있을 경우 타겟을 공유한다.
        if (Parent != null)
        {
            targets = Parent.targets;
            MainTargetInfo = Parent.MainTargetInfo;
            MainTarget = Parent.MainTarget;
        }
        if (Info != null)
            Managers.Map.ApplyMove(Info);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //CurrentState.HandleOnColliderEnter(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        CurrentState.HandleOnTriggerEnter(other);
    }

    public override void OnDamaged(DamageCollider dm)
    {
        //데미지 받는 로직 자유롭게 작성
        CurrentState.OnDamaged(dm);
    }

    #region Shader Effect

    protected Coroutine colorLerpCoroutine = null;
    IEnumerator LerpAttackGlowColor()
    {
        float elapsedTime = 0f;

        while (elapsedTime < colorChangeDuration)
        {
            // 컬러를 Lerp로 전환
            foreach (Material glowMat in _glowMat)
            {
                glowMat.SetColor("_Color", Color.Lerp(initialColor, targetColor, elapsedTime / (colorChangeDuration)));
            }
            

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < colorChangeDuration)
        {
            // 컬러를 Lerp로 전환
            foreach (Material glowMat in _glowMat)
            {
                glowMat.SetColor("_Color", Color.Lerp(targetColor, initialColor, elapsedTime / (colorChangeDuration)));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 마지막으로 목표 컬러로 확실히 설정
        foreach (Material glowMat in _glowMat)
        {
            glowMat.SetColor("_Color", initialColor);
        }
        colorLerpCoroutine = null;
    }

    public void LerpAttackGlowColorCoroutinStart()
    {
        if (colorLerpCoroutine != null)
            StopCoroutine(colorLerpCoroutine);
        colorLerpCoroutine = StartCoroutine(LerpAttackGlowColor());
    }

    // 돌 골렘 Glow 함수
    protected Coroutine attackGlowCoroutine = null;

    IEnumerator LerpAttackGlowColorStart()
    {
        float elapsedTime = 0f;

        Debug.Log("빨개진다2");
        while (elapsedTime < colorChangeDuration)
        {
            // 컬러를 Lerp로 전환
            foreach (Material glowMat in _glowMat)
            {
                glowMat.SetColor("_Color", Color.Lerp(initialColor, targetColor * 1.1f, elapsedTime / (colorChangeDuration)));
            }


            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void LerpAttackGlowCoroutineStart()
    {

        Debug.Log("빨개진다1");
        if (attackGlowCoroutine != null)
            StopCoroutine(attackGlowCoroutine);
        attackGlowCoroutine = StartCoroutine(LerpAttackGlowColorStart());
    }
    
    IEnumerator LerpAttackGlowColorEnd()
    {
        float elapsedTime = 0f;
        

        while (elapsedTime < colorChangeDuration)
        {
            // 컬러를 Lerp로 전환
            foreach (Material glowMat in _glowMat)
            {
                glowMat.SetColor("_Color", Color.Lerp(targetColor * 1.1f, initialColor, elapsedTime / (colorChangeDuration)));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 마지막으로 목표 컬러로 확실히 설정
        foreach (Material glowMat in _glowMat)
        {
            glowMat.SetColor("_Color", initialColor);
        }
        colorLerpCoroutine = null;
    }
    public void LerpAttackGlowCoroutineEnd()
    {
        if (attackGlowCoroutine != null)
            StopCoroutine(attackGlowCoroutine);
        attackGlowCoroutine = StartCoroutine(LerpAttackGlowColorEnd());
    }


    Coroutine _coDissolve;
	IEnumerator LerpDissolve()
    {
        float elapsedTime = 0f;

        while (elapsedTime < 2.0f)
        {
            // 컬러를 Lerp로 전환
            foreach (Material baseMat in _baseMat)
            {
                baseMat.SetFloat("_DissolveAmount", Mathf.Lerp(0, 1f, elapsedTime / 2.0f));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

		foreach (Material baseMat in _baseMat)
		{
			baseMat.SetFloat("_DissolveAmount", 1.0f);
		}
		gameObject.SetActive(false);

        // 죽었을 때만 스폰 위치로 리턴
		if (IsDead && Info != null)
			Info.ReturnSpawnPos();

		_coDissolve = null;
	}

    public void LerpDissolveStart()
    {
		_coDissolve = StartCoroutine(LerpDissolve());
    }

	Coroutine _coAppearDissolve;
	IEnumerator LerpAppearDissolve()
    {
        float elapsedTime = 0f;

		while (elapsedTime < 2.0f)
        {
            // 컬러를 Lerp로 전환
            foreach (Material baseMat in _baseMat)
            {
                baseMat.SetFloat("_DissolveAmount", Mathf.Lerp(1, 0f, elapsedTime / 2.0f));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
		}

		foreach (Material baseMat in _baseMat)
			baseMat.SetFloat("_DissolveAmount", 0.0f);

		_coAppearDissolve = null;
	}

    public void LerpAppearDissolveStart()
    {
		_coAppearDissolve = StartCoroutine(LerpAppearDissolve());
    }
    //NightMare전용 - GameObject 활성화제거 안하기 위해서 / 나타나는 속도 Custom하게
    public void NightMareLerpDissolveStart()
    {
        _coDissolve = StartCoroutine(NightMareLerpDissolve());
    }

    IEnumerator NightMareLerpDissolve()
    {
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            // 컬러를 Lerp로 전환
            foreach (Material baseMat in _baseMat)
            {
                baseMat.SetFloat("_DissolveAmount", Mathf.Lerp(0, 1f, elapsedTime));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (Material baseMat in _baseMat)
        {
            baseMat.SetFloat("_DissolveAmount", 1.0f);
        }

        _coDissolve = null;
    }
    public void NightMareLerpAppearDissolveStart()
    {
        foreach (Material baseMat in _baseMat)
        {
            baseMat.SetFloat("_DissolveAmount", 0.0f);
        }        //_coAppearDissolve = StartCoroutine(NightMareLerpAppearDissolve());
    }
    IEnumerator NightMareLerpAppearDissolve()
    {
        float elapsedTime = 0f;

        while (elapsedTime < .1f)
        {
            // 컬러를 Lerp로 전환
            foreach (Material baseMat in _baseMat)
            {
                baseMat.SetFloat("_DissolveAmount", Mathf.Lerp(1, 0f, elapsedTime));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (Material baseMat in _baseMat)
        {
            baseMat.SetFloat("_DissolveAmount", 0.0f);
        }

        _coAppearDissolve = null;
    }

    Coroutine _damageEffectCoroutin = null;
    IEnumerator DamagedEffect()
    {

        // ✅ 색상 변경
        foreach (Material baseMat in _baseMat)
        {
            if (baseMat.HasProperty("_BaseColor")) // ✅ 안전 체크
            {
                baseMat.SetColor("_BaseColor", new Color(50 / 255f, 0, 0, 1) * 150f);  //40f
            }
        }

        yield return new WaitForSeconds(0.1f);   //.35f

        // ✅ 원본 색상 복원
        for (int i = 0; i < _baseMat.Count; i++)
        {
            if (i < _originalBaseColors.Count) // ✅ 인덱스 초과 방지
            {
                if (_baseMat[i].HasProperty("_BaseColor")) // ✅ 안전 체크
                {
                    _baseMat[i].SetColor("_BaseColor", _originalBaseColors[i]);
                }
            }
        }
        _damageEffectCoroutin = null;
    }

    public void DamageEffectStart()
    {

        if (_damageEffectCoroutin != null)
        {
            StopCoroutine(_damageEffectCoroutin);
            _damageEffectCoroutin = StartCoroutine(DamagedEffect());
        }
        else
            _damageEffectCoroutin = StartCoroutine(DamagedEffect());
    }

    #endregion


    #region Field Of View
    public IEnumerator DetectTargetsRoutine()
    {
        while (true)
        {
            DetectTargetsInView();
            if (MainTarget == null)
            {
                viewRadius = originalViewRadius; // 감지 범위를 원래대로 설정
            }
            else
            {
                viewRadius = originalViewRadius * 1.5f; // 감지 범위를 1.5배로 설정
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private bool isPlayerExist = false;

    public virtual void DetectTargetsInView()
    {
        List<TargetInfo> allTargets;
        if (roleType == RoleType.Leader || roleType == RoleType.Solo)
            allTargets = targets;
        else
            allTargets = leader.targets;

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetLayer);

        //시야각에 있는 오브젝트들의 정보를 리스트에 담는다,
        foreach (Collider target in targetsInViewRadius)
        {
            if (target.transform == transform)
                continue;
            Transform targetTransform = target.transform;
            Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer))
                {
                    //멤버일 경우와 리더일 경우 구분
                    //멤버일 경우 leader.targets 접근
                    TargetInfo a = allTargets.Find(a => a.target == targetTransform);
                    if (a != null)//이미 리스트에 포함되어 있을 경우 마지막 발견시간 갱신
                    {
                        a.lastDetactiontime = Time.time;
                    }
                    else//리스트에 포함되어 있지 않다면 새로 추가
                    {
                        a = new TargetInfo
                        {
                            target = targetTransform,
                            lastDetactiontime = Time.time
                        };
                        allTargets.Add(a); // 새로운 타겟 추가
                    }
                }
            }
        }
        if (allTargets.Count == 0)
        {
			MainTarget = null;
			if (isBoss && isPlayerExist)
			{
				isPlayerExist = false;
				Managers.Sound.EndBattle(this.gameObject);
			}
			return;
        }

		// 리스트에 있는 타겟 중 가장 가까운 타겟을 ClosestTarget으로 설정
		MainTarget = allTargets
            .OrderBy(t => Vector3.Distance(transform.position, t.target.position))
            .FirstOrDefault().target; // 리스트가 비어있을 경우 null 방지
        _mainTargetInfo = allTargets
            .OrderBy(t => Vector3.Distance(transform.position, t.target.position))
            .FirstOrDefault(); // 리스트가 비어있을 경우 null 방지

        // 5초 이상 지난 타겟 제거
        allTargets.RemoveAll(t => Time.time - t.lastDetactiontime > maxDetectionLostTime);
        //오브젝트가 꺼졌거나 없어진경우, 혹은 Missing(연결이 끊어진)상태일 경우 삭제
        allTargets.RemoveAll(t => !t.target.gameObject.activeSelf || t.target == null || (t.target != null && t.target.Equals(null)));

        if (isBoss && allTargets.Find(t => t.target.gameObject.layer == LayerMask.NameToLayer("Player")) != null && !isPlayerExist)
        {
            //Battle BGM On
            isPlayerExist = true;
            Managers.Sound.PlayBattleBGM(this.gameObject, "BGM/BeastBGM");
        }

        if (isBoss && allTargets.Find(t => t.target.gameObject.layer == LayerMask.NameToLayer("Player")) == null && isPlayerExist)
        {
            isPlayerExist = false;
            Managers.Sound.EndBattle(this.gameObject);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (_mainTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _mainTarget.position);
        }
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
    }

    Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
    #endregion


    public override IEnumerator SlowAffectCoroutine(float duration, float affectValuePercentage)
    {
        //슬로우에 걸릴경우 일정시간이후 해당 상태 해제

        float elapsedTime = 0f;
        Nma.speed = MoveSpeed * affectValuePercentage;
        GetComponentInChildren<Animator>().SetFloat("AnimationSpeed", affectValuePercentage);

        while (elapsedTime < duration)
        {

            // 0.3초 간격으로 대기
            yield return new WaitForFixedUpdate();

            // 경과 시간 증가
            elapsedTime += Time.fixedDeltaTime;
        }
        Debug.Log("슬로우 끝");
        Nma.speed = MoveSpeed;
        GetComponentInChildren<Animator>().SetFloat("AnimationSpeed", 1f);

        slow = null;
	}


    //맞은 지점
    public IEnumerator KnockbackRoutine(DamageCollider dm)
    {
        Vector3 hitDirection = new Vector3(0,0,0);
        float power;
        float duration;

        _nma.isStopped = true;        // 경로 이동 잠시 정지
        _nma.ResetPath();             // 현재 경로 취소 (SetDestination 멈춤)
        if(dm.AttackDirection == DamageCollider.AttackDirectionType.SkillDirection)
        {
            hitDirection = dm.transform.forward;
            hitDirection.y = 0;
        }
        if (dm.AttackDirection == DamageCollider.AttackDirectionType.SkillToTarget)
        {
            hitDirection = (transform.position - dm.transform.position);
            hitDirection.y = 0;
        }
        if (dm.AttackDirection == DamageCollider.AttackDirectionType.CasterToTarget)
        {
            hitDirection = (transform.position - dm.Caster.transform.position);
            hitDirection.y = 0;
        }
        power = 3;
        duration = 0.25f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 방향으로 이동 (NavMeshAgent가 직접 위치 갱신)
            _nma.Move(hitDirection.normalized * power * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _nma.isStopped = false;       // 다시 경로 따라가기 재개
    }


    public void DropItem()
	{
		Data.ObjectData objectData = null;
		Managers.Data.ObjectDict.TryGetValue(TemplateId, out objectData);

		foreach (RewardData rewardData in objectData.rewards)
		{
			if (rewardData.dayTimeType != DayTimeType.None && rewardData.dayTimeType != Managers.Time.DayTimeType)
				continue;
			int rand = Random.Range(0, 100);
			if (rand < rewardData.probability)
			{
				int count = Random.Range(rewardData.minCount, rewardData.maxCount + 1);
				for (int i = 0; i < count; i++)
				{
					Vector3 pos = GetRandomPosOfDropItem(transform.position, 1.5f);

					RewardObjectInfo rewardObject = new RewardObjectInfo();
					rewardObject.SpawnPos = pos;
					rewardObject.Init(rewardData.itemId, 1, transform.position);
					Managers.Map.ApplyEnter(rewardObject);
				}
			}
		}
	}

	Vector3 GetRandomPosOfDropItem(Vector3 center, float radius)
	{
		Vector3 pos = center;
		for (int i = 0; i < 10; i++)
		{
			Vector2 randWithinCircle = Random.insideUnitCircle * radius;
			pos.x = center.x + randWithinCircle.x;
			pos.z = center.z + randWithinCircle.y;

			Vector3 dir = pos - center;
			RaycastHit hit;
			if (!Physics.SphereCast(center, 0.5f, dir, out hit, dir.magnitude, 1 << (int)Layer.Block))
				return pos;
		}
		return center;
	}
}
