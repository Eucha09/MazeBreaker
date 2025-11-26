using Data;
using SpiderAttackPatterns.PoisonPattern;
using System.Collections;
using UnityEngine;
using static Define;

public class BaseController : MonoBehaviour
{
    StatInfo _stat = new StatInfo();
    public virtual StatInfo Stat
    {
        get { return _stat; }
        set
        {
            if (_stat.Equals(value))
                return;

            _stat.MergeFrom(value);
        }
    }

    public virtual float Hp { get { return _stat.hp; } set { if(value > MaxHp) _stat.hp = MaxHp; else if (value < 0) _stat.hp = 0; else _stat.hp = value; } }
    public virtual float MaxHp { get { return _stat.maxHp; } set { _stat.maxHp = value; } }
    public virtual float Attack { get { return _stat.attack; } set { _stat.attack = value; } }
    public virtual float Defense { get { return _stat.defense; } set { _stat.defense = value; } }
    public virtual float MoveSpeed { get { return _stat.speed; } set { _stat.speed = value; } }
    public virtual float CurrentMoveSpeed { get { return _stat.currentSpeed; } set { _stat.currentSpeed = value; } }

    // player stat
    public virtual float Mental { get { return _stat.mental; } set { _stat.mental = value; } }
    public virtual float MaxMental { get { return _stat.maxMental; } set { _stat.maxMental = value; } }
    public virtual float Hunger { get { return _stat.hunger; } set { _stat.hunger = value; } }
    public virtual float MaxHunger { get { return _stat.maxHunger; } set { _stat.maxHunger = value; } }
    public virtual float Stamina { get { return _stat.stamina; } set { _stat.stamina = value; } }
    public virtual float MaxStamina { get { return _stat.maxStamina; } set { _stat.maxStamina = value; } }
    public virtual float SpecialValue { get { return _stat.specialValue; } set { _stat.specialValue = value; } }

    [SerializeField]
    int _templateId;
    public int TemplateId { get { return _templateId; } set { _templateId = value; } }

    [SerializeField]
    GameObjectType _objectType;
    public GameObjectType ObjectType { get { return _objectType; } protected set { _objectType = value; } }

    // TEMP
    [SerializeField]
    MaterialType _materialType;
    public MaterialType MaterialType { get { return _materialType; } protected set { _materialType = value; } }

    public Vector2Int GridPos
    {
        get { return Managers.Map.WorldToGrid(transform.position); }
        set { transform.position = Managers.Map.GridToWorld(value); }
    }

    bool _isDead;
	public virtual bool IsDead 
    {
        get { return _isDead; } 
        set
        {
            _isDead = value;
            if (Info != null)
				Info.IsDead = value;
        }
    }

    public CreatureInfo Info { get; protected set; }

	public virtual void Bind(CreatureInfo info)
	{

	}

    public virtual void Refresh()
    {

    }

	public virtual void Unbind()
	{

	}

	public virtual void Revive()
    {    

	}

    public virtual void OnDamaged(DamageCollider dm) { }

    public virtual void OnMentalDamaged(float damage)
    {
        damage = Mathf.Max(damage, 0.0f);
        Mental = Mathf.Max(Mental - damage, 0.0f);
        if (Mental <= 0.0f)
        {
            OnDead();
            return;
        }
    }

    protected virtual void OnDead() { }


    //StatusEffect
    //만약 OnDamaged에 추가 효과가 있을 경우 추가효과에 해당하는 StatusEffect 코루틴 실행

    protected Coroutine poison = null;
    protected Coroutine burn = null;
    protected Coroutine slow = null;

    public void StartPoisonCoroutin(StatusAffect statusAffect)
    {
        if (poison != null)
            return;
        poison = StartCoroutine(PoisonAffectCoroutine(statusAffect.affectDuration, 0.5f, statusAffect.affectValuePercentage));
    }

    public void StartBurnCoroutin(StatusAffect statusAffect)
    {
        if (burn != null)
            return;
        burn = StartCoroutine(BurnAffectCoroutine(statusAffect.affectDuration, 0.3f, statusAffect.affectValuePercentage));
    }

    public void StartSlowCoroutin(StatusAffect statusAffect)
    {
        if (slow != null)
            return;
        slow = StartCoroutine(SlowAffectCoroutine(statusAffect.affectDuration, statusAffect.affectValuePercentage));
    }

    public virtual IEnumerator PoisonAffectCoroutine(float duration, float damageInterval, float damagePercentage)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Hp -=  MaxHp * damagePercentage;

            // 0.5초 간격으로 대기
            yield return new WaitForSecondsRealtime(damageInterval);
            ShowDamagePopup(MaxHp * damagePercentage, transform.position + Vector3.up * 1.5f, true);

            // 경과 시간 증가
            elapsedTime += damageInterval;
        }

        poison = null;
        Debug.Log("독지속시간 종료");
    }

    public virtual IEnumerator BurnAffectCoroutine(float duration, float damageInterval, float damagePercentage)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Hp -= MaxHp * damagePercentage;

            // 0.3초 간격으로 대기
            yield return new WaitForSecondsRealtime(damageInterval);

            // 경과 시간 증가
            elapsedTime += damageInterval;
        }

        burn = null;
    }

    public virtual IEnumerator SlowAffectCoroutine(float duration, float affectValuePercentage)
    {
        //슬로우에 걸릴경우 일정시간이후 해당 상태 해제

        float elapsedTime = 0f;
        CurrentMoveSpeed = MoveSpeed * affectValuePercentage;
        GetComponentInChildren<Animator>().SetFloat("AnimationSpeed", affectValuePercentage);

        while (elapsedTime < duration)
        {

            // 0.3초 간격으로 대기
            yield return new WaitForFixedUpdate();

            // 경과 시간 증가
            elapsedTime += Time.fixedDeltaTime;
        }
        Debug.Log("슬로우 끝");
        CurrentMoveSpeed = MoveSpeed;
        GetComponentInChildren<Animator>().SetFloat("AnimationSpeed", 1f);

        slow = null;
    }

    public void ShowDamagePopup(float amount, Vector3 position, bool isPlayerHit = false)
    {

		UI_DamagePopup popup = Managers.UI.MakeWorldSpaceUI<UI_DamagePopup>();
		popup.transform.position = position;
        if (popup != null)
        {
            Color popupColor = isPlayerHit ? Color.green : Color.white;
			popup.Setup(Mathf.RoundToInt(amount), popupColor, isPlayerHit);
        }
    }

}
