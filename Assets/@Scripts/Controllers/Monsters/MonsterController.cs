using Data;
using System.Collections;
using System.Data.Common;
using UnityEngine;
using UnityEngine.AI;
using static Define;

public class MonsterController : CreatureController
{
    [SerializeField]
    float _hp;

    protected Coroutine _coSkill;
    protected Coroutine _coPatrol;
    protected Coroutine _coSearch;

    protected Vector3 _centerPos;
    protected Vector3 _destPos;
    protected GameObject _target;
    protected float _patrolRange = 9.0f;
    protected float _searchRange = 10.0f;
    protected float _chaseRange = 27.0f;
    protected int _layerObstacle;

    [SerializeField]
    bool _rangedSkill = false;
    protected float _skillRange;

    protected NavMeshAgent _nma;

    bool _init;

    public override CreatureState State
    {
        get { return _state; }
        set
        {
            base.State = value;

            if (_coPatrol != null)
            {
                StopCoroutine(_coPatrol);
                _coPatrol = null;
            }
            if (_coSearch != null)
            {
                StopCoroutine(_coSearch);
                _coSearch = null;
            }
        }
    }

    protected override void UpdateAnimation()
    {
        switch (_state)
        {
            case CreatureState.Idle:
                _animator.CrossFade("IDLE", 0.1f);
                break;
            case CreatureState.Moving:
                _animator.CrossFade("WALK", 0.1f);
                break;
            case CreatureState.Run:
                _animator.CrossFade("RUN", 0.1f);
                break;
            case CreatureState.Skill:
                _animator.CrossFade("SKILL", 0.1f, -1, 0.0f);
                break;
            case CreatureState.Stunned:
                _animator.CrossFade("STUN", 0.1f);
                break;
            case CreatureState.Damaged:
                _animator.Play("HURT", 0, 0);
                break;
            case CreatureState.Dead:
                _animator.CrossFade("DEAD", 0.1f);
                break;
        }
    }

    protected override void Init()
    {
        _init = true;
        base.Init();

        ObjectData objectData = null;
        Managers.Data.ObjectDict.TryGetValue(TemplateId, out objectData);
        ObjectType = objectData.objectType;
        MaterialType = objectData.materialType;
        Stat = objectData.stat;
        Hp = objectData.stat.maxHp;
        State = CreatureState.Idle;

        _nma = GetComponent<NavMeshAgent>();
        _centerPos = transform.position;
        _destPos = transform.position;

        // TODO
        Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform).SetInfo(this, 3.5f);

        // TODO
        if (_rangedSkill)
            _skillRange = 10.0f;
        else
            _skillRange = 2.0f;


        Renderer meshRenderer = GetComponentInChildren<Renderer>();
        Material[] materials = meshRenderer.materials;

        for (int i = 0; i < materials.Length; i++)
        {
            meshRenderer.materials[i] = Instantiate(materials[i]);
        }

        BaseMat = materials[0];
        GlowMat = materials[1];
    }

    protected override void UpdateController()
    {
        base.UpdateController();
        // Debug
        _hp = Hp;
        if(_hp <= 0 && State != CreatureState.Dead)
        {
            State = CreatureState.Dead;
        }
    }

    protected override void UpdateIdle()
    {
        if (_target != null)
        {
            if (_coSkill == null)
                State = CreatureState.Run;
            return;
        }

        float distance = (_destPos - transform.position).magnitude;
        if (distance > 1.0f)
        {
            State = CreatureState.Moving;   
            return;
        }

        if (_coPatrol == null)
            _coPatrol = StartCoroutine("CoPatrol");
        if (_coSearch == null)
            _coSearch = StartCoroutine("CoSearch");
    }

    protected override void UpdateMoving()
    {
        if (_target != null)
        {
            State = CreatureState.Run;
            return;
        }

        Vector3 dir = _destPos - transform.position;
        if (dir.magnitude < 0.5f)
        {
            _nma.SetDestination(transform.position);
            State = CreatureState.Idle;
            return;
        }

        if (_coSearch == null)
            _coSearch = StartCoroutine("CoSearch");

        _nma.SetDestination(_destPos);
        _nma.speed = MoveSpeed;
    }

    protected override void UpdateRun()
    {
        if (CanUseSkill())
        {
            _nma.SetDestination(transform.position);
            State = CreatureState.Skill;
            UseSkill();
            return;
        }

        _destPos = _target.transform.position;
        Vector3 dir = _centerPos - transform.position;
        if (dir.magnitude > _chaseRange)
        {
            _nma.SetDestination(transform.position);
            _target = null;
            _destPos = _centerPos;
            State = CreatureState.Moving;
            return;
        }

        _nma.SetDestination(_destPos);
        _nma.speed = MoveSpeed * 1.5f;
    }

    protected override void UpdateSkill()
    {
        Vector3 dir = _target.transform.position - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
    }

    protected override void UpdateDamaged()
    {
        //if (CanUseSkill())
        //{
        //    _nma.SetDestination(transform.position);
        //    State = CreatureState.Skill;
        //    UseSkill();
        //    return;
        //}

        _nma.SetDestination(transform.position);
    }

    protected override void UpdateDead()
    {
        _nma.SetDestination(transform.position);
    }

    protected virtual bool CanUseSkill()
    {
        if (_target == null || _coSkill != null)
            return false;

        Vector3 dir = _target.transform.position - transform.position;
        if (dir.magnitude <= _skillRange)
        {
            return true;
        }
        return false;
    }

    protected virtual void UseSkill()
    {
        if (_target == null || _coSkill != null)
            return;

        if (_rangedSkill)
            _coSkill = StartCoroutine("CoStartShootArrow");
        else
            _coSkill = StartCoroutine("CoStartPunch");
    }

    protected virtual IEnumerator CoPatrol()
    {
        int waitSeconds = Random.Range(1, 4);
        yield return new WaitForSeconds(waitSeconds);

        for (int i = 0; i < 10; i++)
        {
            Vector3 randDir = Random.insideUnitSphere * _patrolRange;
            randDir.y = 0;
            Vector3 randPos = _centerPos + randDir;

            Vector3 dir = (randPos - transform.position);
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, dir.magnitude, LayerMask.GetMask("Block")))
                continue;

            NavMeshPath path = new NavMeshPath();
            _nma.CalculatePath(randPos, path);
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                _destPos = randPos;
                _coPatrol = null;
                yield break;
            }
        }

        _coPatrol = null;
    }

    protected virtual IEnumerator CoSearch()
    {
        BaseController player = Managers.Object.GetPlayer();

        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (_target != null)
                continue;

            if (player == null)
                continue;

            Vector3 dir = (player.transform.position - transform.position);
            if (dir.magnitude > _searchRange)
                continue;

            Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir, Color.green);
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, dir.magnitude, LayerMask.GetMask("Block")))
            {
                _target = player.gameObject;
            }
        }
    }

    IEnumerator CoStartPunch()
    {
        // 쿨타임
        yield return new WaitForSeconds(2.5f);
        _coSkill = null;
    }

    IEnumerator CoStartShootArrow()
    {
        // 쿨타임
        yield return new WaitForSeconds(3.0f);
        _coSkill = null;
    }

    //public override void OnDamaged(float damage, BaseController attacker)
    //{
    //    base.OnDamaged(damage, attacker);

    //    if (State == CreatureState.Dead)
    //        return;
    //    //if (State == CreatureState.Skill)
    //    //    return;

    //    State = CreatureState.Damaged;
    //    UpdateAnimation();
    //}

    protected override void OnDead()
    {
        if (State == CreatureState.Dead)
            return;

        State = CreatureState.Dead;

        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        DropItem();
        //RewardData rewardData = GetRandomReward();
        //if (rewardData != null)
        //{
        //    // 자원 생성
        //    GameObject go = Managers.Resource.Instantiate("Item/RewardObject");
        //    go.transform.position = transform.position;
        //    go.GetComponent<RewardObject>().Init(rewardData);
        //}
	}

	void DropItem()
	{
		ObjectData objectData = null;
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
					GameObject go = Managers.Resource.Instantiate("Item/RewardObject");
					go.GetComponent<RewardObject>().Init(rewardData.itemId, 1, transform.position, pos);
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

			RaycastHit hit;
			if (!Physics.SphereCast(center, 0.5f, pos - center, out hit, radius, 1 << (int)Layer.Block))
				return pos;
		}
		return center;
	}

    // Old
	RewardData GetRandomReward()
    {
        ObjectData objectData = null;
        Managers.Data.ObjectDict.TryGetValue(TemplateId, out objectData);

        int rand = Random.Range(0, 100);

        int sum = 0;
        foreach (RewardData rewardData in objectData.rewards)
        {
            sum += rewardData.probability;
            if (rand < sum)
            {
                return rewardData;
            }
        }

        return null;
    }

    void OnDisable()
    {
        if (_coPatrol != null)
        {
            StopCoroutine(_coPatrol);
            _coPatrol = null;
        }

        if (_coSearch != null)
        {
            StopCoroutine(_coSearch);
            _coSearch = null;
        }

        if (_coSkill != null)
        {
            StopCoroutine(_coSkill);
            _coSkill = null;
        }
    }

    void OnEnable()
    {
        if (_init)
            Init();
    }

    public void OnAttack()
    {
        //if (_rangedSkill)
        //{
        //    Vector3 dir = _target.transform.position - transform.position;
        //    GameObject go = Managers.Resource.Instantiate("Objects/TreeSpit");
        //    go.transform.position = transform.position;
        //    go.GetComponent<TreeSpit>().Dir = dir.normalized;
        //}
        //else
        //{
        //    Vector3 dir = _target.transform.position - (transform.position + transform.forward * _skillRange);
        //    if (dir.magnitude <= _skillRange)
        //    {
        //        PlayerControllerV1 pc = _target.GetComponent<PlayerControllerV1>();
        //        if (pc != null)
        //            pc.HandleDamage(transform.position, _stat.Attack);
        //    }
        //}
    }

    public void SetState(CreatureState state)
    {
        State = state;
    }

    public void Destroy()
    {
        Managers.Object.Remove(gameObject);
    }

    public void PlaySound(AudioClip audioClip)
    {
        if (audioClip == null)
            return;
        Managers.Sound.Play3DSound(gameObject, audioClip, 0.0f, 54.0f);
    }
    public void Play3DSound(AudioClip audioClip)
    {
        if (audioClip == null)
            return;
        Managers.Sound.Play3DSound(gameObject, audioClip, 0.0f, 54.0f);
    }
    public void Effect(string effectPath)
    {
        if (effectPath == null)
            return;
        GameObject a=Managers.Resource.Instantiate(effectPath, transform.position, transform.rotation);
        a.GetComponent<DefaultDamageCollider>().Init(this, 0.1f);
    }

    [Header("Attack Glow")]
    [SerializeField]
    Material _glowMat;
    [SerializeField]
    Material _baseMat;
    public Color targetColor = Color.red; // 목표 컬러
    public float colorChangeDuration = .4f; // 컬러 전환에 걸리는 시간
    public Color initialColor;//초기 컬러
    private Coroutine colorLerpCoroutine = null;

    public Material GlowMat { get { return _glowMat; } set { _glowMat = value; } }
    public Material BaseMat { get { return _baseMat; } set { _baseMat = value; } }

    IEnumerator LerpAttackGlowColor()
    {
        float elapsedTime = 0f;

        while (elapsedTime < colorChangeDuration)
        {
            // 컬러를 Lerp로 전환
            GlowMat.SetColor("_Color", Color.Lerp(initialColor, targetColor, elapsedTime / (colorChangeDuration)));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < colorChangeDuration)
        {
            // 컬러를 Lerp로 전환
            GlowMat.SetColor("_Color", Color.Lerp(targetColor, initialColor, elapsedTime / (colorChangeDuration)));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 마지막으로 목표 컬러로 확실히 설정
        GlowMat.SetColor("_Color", initialColor);
        colorLerpCoroutine = null;
    }

    public void LerpAttackGlowColorCoroutinStart()
    {
        if (colorLerpCoroutine != null)
            StopCoroutine(colorLerpCoroutine);
        colorLerpCoroutine = StartCoroutine(LerpAttackGlowColor());
    }

    IEnumerator LerpDissolve()
    {
        float elapsedTime = 0f;

        while (elapsedTime < 2.0f)
        {
            // 컬러를 Lerp로 전환
            _baseMat.SetFloat("_DissolveAmount", Mathf.Lerp(0, 1f, elapsedTime / 2.0f));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        gameObject.SetActive(false);
    }
    public void LerpDissolveStart()
    {
        StartCoroutine(LerpDissolve());
    }

}
