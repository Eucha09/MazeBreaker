using UnityEngine;
using System.Collections.Generic;
using static Define;
using System;
using System.Linq;
using Player;



public class DamageCollider : MonoBehaviour
{
    //스턴
    public Stun stun;
    //추가효과(상태 이상)
    public List<StatusAffect> statusAffect;

    //제거할 예정
    public List<FxInfo> fxInfo;

    //스킬 배율
    [SerializeField]
    protected float _skillMultiplier = 1f;
    

    //공격 방향
    public enum AttackDirectionType
    {
        SkillDirection,       //스킬이 바라보는 방향대로 밀려남.
        CasterToTarget,      // 플레이어 - 몬스터 벡터
        SkillToTarget       //스킬 - 몬스터 벡터, 팡 터져서 밀려나는 느낌
    }

    public AttackDirectionType AttackDirection;

    //힐
    public enum HealType
    {
        None,       // 상대에게 데미지
        Heal,          // 아군 힐
        HealAndDamage  // 아군 힐 & 적에게 데미지
    }

    public HealType healType;
    public float healAmount;

    //공격 시전자
    protected BaseController _caster;

    //외부참조용
    public BaseController Caster { get { return _caster; } private set { } }


    [Header("Player Properties")]
    //소모할 내구도(플레이어에게만 적용)
    [SerializeField]
    protected int _consomptionDurability = 1;
    //그로기 수치(플레이어에게만 적용)
    [SerializeField]
    protected int _weaknessAmount = 0;
    //해당 스킬을 생성한 무기(플레이어에게만 적용)
    protected Equipment Weapon;
    //해당 공격을 맞출 때 마다 스테미나를 회복할지(플레이어에게만 적용)
    public bool staminaRecover = false;
    public int staminaRecoverAmount = 1;
    [SerializeField]
    protected bool isChargeAttack = false;
    public bool IsChargeAttack { get { return isChargeAttack; } set { } }

    public void Start()
    {
        PlayerController pc = Caster as PlayerController;
        if (pc != null)
            Weapon = pc.CurrentWeapon as Equipment;
    }

    //웨폰타입에 따라 자원 데미지 배율 검사 메서드
    public float DamageCalculate(BaseController target)
    {
        float materialMagnificationValue = 1;
        if (target.MaterialType == MaterialType.Tree && (Weapon != null && Weapon.WeaponType == WeaponType.Axe))
        {
            materialMagnificationValue = 2;
        }
        if (target.MaterialType == MaterialType.Stone && (Weapon != null && Weapon.WeaponType == WeaponType.Mace))
        {
            materialMagnificationValue = 2;
        }
        if (target.MaterialType == MaterialType.Grass && (Weapon != null && Weapon.WeaponType == WeaponType.Sword))
        {
            materialMagnificationValue = 2;
        }

        MonsterController2 mc = target as MonsterController2;



        //(스킬 데미지 퍼센티지 x 시전자 공격력)/(1+방어력/c(방어력 효율 상수))
        //여기서 상수는 방어력 효율을 결정지으며, 상수가 낮을수록 방어력 효율이 올라간다.
        //return Mathf.RoundToInt((_skillMultiplier * _caster.Attack) / (1 + target.Defense / 100f) * materialMagnificationValue);
        float rawValue = (_skillMultiplier * _caster.Attack) / (1 + target.Defense / 100f) * materialMagnificationValue;
        if (mc != null && mc.Weakness >0 && isChargeAttack)
        {
            return Mathf.FloorToInt(rawValue + 0.5f) * (mc.Weakness + 1);
        }
        else
        {
            return Mathf.FloorToInt(rawValue + 0.5f);
        }
        //return (_skillMultiplier * _caster.Attack)/(1+target.Defense / 100)* materialMagnificationValue;
    }

    protected HashSet<BaseController> _damagedTargets = new HashSet<BaseController>(); // 데미지를 준 객체를 추적하는 HashSet
    bool isDurabiliityDecreased = false;//내구도 감소가 일어났는지 체크하는 변수
    bool isStaminaRecovered = false;//스테미나가 회복됐는지 체크하는 변수

    public virtual void OnTriggerEnter(Collider other)
    {
        //힐 처리
        if (healType == HealType.HealAndDamage || healType == HealType.Heal)
        {
            if (other.GetComponent<BaseController>() != null
                && other.GetComponent<BaseController>().ObjectType == _caster.ObjectType
                && !_damagedTargets.Contains(other.GetComponent<BaseController>()))
            {
                other.GetComponent<BaseController>().Hp += healAmount;
                _damagedTargets.Add(other.GetComponent<BaseController>());
                if (Weapon != null)
                    Weapon.DecreaseDurability(_consomptionDurability);
            }

            if (staminaRecover && Caster is PlayerController pc)
            {
                pc.CurrentStaminaCount += staminaRecoverAmount;
            }
        }

        //데미지 처리
        if (healType == HealType.None || healType == HealType.HealAndDamage)
        {
            //데미지를 받을 수 없는 객체거나 시전자가 없는 경우 Return
            if (other.GetComponent<BaseController>() == null || _caster == null)
                return;

            BaseController target = other.GetComponent<BaseController>();


            // 데미지를 주는 대상이 자기 자신이거나 같은 아군이면 리턴
            if (target == _caster || target.ObjectType == _caster.ObjectType)
                return;

            // 이미 데미지를 준 객체라면 리턴
            if (_damagedTargets.Contains(target))
                return;

            //스테미나 회복 가능한 공격일 경우 회복
            if (staminaRecover && Caster is PlayerController pc && !isStaminaRecovered)
            {
                pc.CurrentStaminaCount+=staminaRecoverAmount;
                isStaminaRecovered = true;
            }

            //기절 수치를 더해줄 수 있는 공격일 경우 기절 수치 증가
            if (_weaknessAmount > 0f && target is MonsterController2 monster)
            {
                monster.AffectWeakness(_weaknessAmount);
            }


            //넉백 실험
            if (target is MonsterController2 monster2)
            {
                monster2.StartCoroutine(monster2.KnockbackRoutine(this));
            }

            // 맞은 객체가 데미지를 받을 수 있는 객체라면 데미지 처리
            target.OnDamaged(this);

            //무기 내구도 감소
            if (Weapon != null && !isDurabiliityDecreased)
            {
                Weapon.DecreaseDurability(_consomptionDurability);
                isDurabiliityDecreased = true;
            }

            //데미지를 준 객체로 기록
            _damagedTargets.Add(target);


            #region SFX, VFX
            if (target is PlayerController pc2 && pc2.CurrentState.GetState() == PlayerStateType.Dash)
            {

            }
            else 
            {
            Collider currentCollider = GetComponent<Collider>();
            Vector3 contactPoint = currentCollider.ClosestPoint(other.transform.position);
            InstantiateFX(contactPoint, target);
            }
            #endregion
        }

        //상태 이상 처리

    }
    public GameObject damageTextPrefab;

    /*V1DamageUI
      public void ShowDamagePopup(float amount, Vector3 position)
    {
        // 프리팹이 연결되지 않은 경우 자동으로 Resources에서 불러오기
        if (damageTextPrefab == null)
        {
            damageTextPrefab = Resources.Load<GameObject>("Prefabs/DamagePopupRoot");
            if (damageTextPrefab == null)
            {
                Debug.LogWarning("❌ DamageTextPrefab을 Resources 폴더에서 찾을 수 없습니다!");
                return;
            }
        }

        GameObject popup = Instantiate(damageTextPrefab, position, Quaternion.identity);
        popup.SetActive(true);

        var popupScript = popup.GetComponent<DamagePopup>();
        if (popupScript != null)
            popupScript.Setup(Mathf.RoundToInt(amount));
    }*/

    /*V2DamaeUI
    public void ShowDamagePopup(float amount, Vector3 position, bool isPlayerHit = false)
    {
        if (damageTextPrefab == null)
        {
            damageTextPrefab = Resources.Load<GameObject>("Prefabs/DamagePopupRoot");
            if (damageTextPrefab == null)
            {
                Debug.LogWarning("❌ DamageTextPrefab을 Resources 폴더에서 찾을 수 없습니다!");　
                return;
            }
        }

        GameObject popup = Instantiate(damageTextPrefab, position, Quaternion.identity);
        popup.SetActive(true);

        var popupScript = popup.GetComponent<DamagePopup>();
        if (popupScript != null)
        {
            Color popupColor = isPlayerHit ? Color.red : Color.white;
            popupScript.Setup(Mathf.RoundToInt(amount), popupColor, isPlayerHit);
        }
    }
    */

    //V3DamageUI_Pooling
    public void ShowDamagePopup(float amount, Vector3 position, bool isPlayerHit = false)
    {

		UI_DamagePopup popup = Managers.UI.MakeWorldSpaceUI<UI_DamagePopup>();
		popup.transform.position = position;
        if (popup != null)
        {
            Color popupColor = isPlayerHit ? Color.red : Color.white;
			popup.Setup(Mathf.RoundToInt(amount), popupColor, isPlayerHit);
        }
    }


    //사운드 재생한 객체 저장
    //이미 한번 재생한적 있는 유형의 사운드일 경우 
    protected List<GameObjectType> _playedSound = new List<GameObjectType>();


    public virtual void InstantiateFX(Vector3 contactPoint, BaseController target)
    {
        var matchingFxInfos = fxInfo.Where(x => x.type == target.ObjectType).ToList();
        if (matchingFxInfos.Count > 0)   //인스펙터 상에 Fx Info를 추가해주면 실행
        {
            int randomIndex = UnityEngine.Random.Range(0, matchingFxInfos.Count);

            // 랜덤 요소 선택
            var randomFx = matchingFxInfos[randomIndex];
            //막타시
            if (target.Hp <= 0)
            {
                Managers.Resource.Instantiate(randomFx.FinalHitVFX, contactPoint, Quaternion.identity);
                if (_playedSound.Contains(target.ObjectType))
                    return;
                Managers.Sound.Play(randomFx.HitSFX, Define.Sound.Effect, 1);
                _playedSound.Add(target.ObjectType);
            }
            else
            {
                if(randomFx.HitVFX != null)
                Managers.Resource.Instantiate(randomFx.HitVFX, contactPoint, Quaternion.identity);

                //리스트 검사결과 이미 한번 사운드를 재생한 적 있는 객체일 경우 Return
                if (_playedSound.Contains(target.ObjectType))
                    return;
    
                else
                {
                    Managers.Sound.Play(randomFx.HitSFX, Define.Sound.Effect, 1);
                }
                //Managers.Sound.Play(randomFx.HitSFX, Define.Sound.Effect, 1);  //일반 사운드 재생
                //Managers.Sound.PlayRandomized3DSound(target.gameObject, randomFx.HitSFX, 0f, 54f);
                //사운드 재생하고 재생한 객체 정보 리스트에 저장
                _playedSound.Add(target.ObjectType);
            }
        }
        else //인스펙터 상에 Fx Info없으면 디폴트 값 실행
        {
            //막타시
            if (target.Hp <= 0)
            {

                // NatureTreeHitSFX1, NatureTreeHitSFX2, NatureTreeHitSFX3
                if (target.ObjectType == GameObjectType.Nature)
                {
                    //주의! 만약 NatureType일경우 "ObjectType + MaterialType +  이름"으로 저장해놔야함 
                    //ex) ObjectType이 만약 Nature일 경우, 해당 경로에 저장되는 오브젝트의 이름은 "NatrueStoneFinalHitVFX"
                    int randomValue = UnityEngine.Random.Range(1, 4); // 1부터 3까지의 값을 생성
                    //ex) 이때 오브젝트의 이름은 "NatrueStoneFinalHitVFX"+ "1~3"중에 하나의 숫자를 붙여줘야 랜덤하게재생됨
                    Managers.Resource.Instantiate("Effects/" + target.ObjectType.ToString() + target.MaterialType.ToString() + "HitVFX" + randomValue.ToString(), contactPoint, Quaternion.identity);
                    //Sound도 마찬가지
                    if (_playedSound.Contains(target.ObjectType))
                        return;
                    Managers.Sound.Play("Sounds/FarmingSound/" + target.ObjectType.ToString() + target.MaterialType.ToString() + "HitSFX" + randomValue.ToString(), Define.Sound.Effect, 1);
                    _playedSound.Add(target.ObjectType);
                }
                else if (target.ObjectType == GameObjectType.Monster)
                {
                    //주의! 해당 경로에는 반드시 ObjectType + 프리팹 이름으로 저장해놔야함 
                    //ex) ObjectType이 만약 Player일 경우, 해당 경로에 저장되는 오브젝트의 이름은 "PlayerHitVFX"
                    Managers.Resource.Instantiate("Effects/" + target.ObjectType.ToString() + "", contactPoint, Quaternion.identity);
                    //Sound도 마찬가지
                    //이미 한번 몬스터 사운드가 재생됐으면 return
                    if (_playedSound.Contains(target.ObjectType))
                        return;
                    Managers.Sound.Play("Sounds/" + target.ObjectType.ToString() + "", Define.Sound.Effect, 1);
                    _playedSound.Add(target.ObjectType);
                }
                else if (target.ObjectType == GameObjectType.Structure)
                {
                    //주의! 만약 NatureType일경우 "ObjectType + MaterialType +  이름"으로 저장해놔야함 
                    //ex) ObjectType이 만약 Nature일 경우, 해당 경로에 저장되는 오브젝트의 이름은 "NatrueStoneFinalHitVFX"
                    int randomValue = UnityEngine.Random.Range(1, 4); // 1부터 3까지의 값을 생성
                    //ex) 이때 오브젝트의 이름은 "NatrueStoneFinalHitVFX"+ "1~3"중에 하나의 숫자를 붙여줘야 랜덤하게재생됨
                    Managers.Resource.Instantiate("Effects/" + target.ObjectType.ToString() + target.MaterialType.ToString() + "HitVFX" + randomValue.ToString(), contactPoint, Quaternion.identity);
                    //Sound도 마찬가지
                    if (_playedSound.Contains(target.ObjectType))
                        return;
                    Managers.Sound.Play("Sounds/FarmingSound/" + target.ObjectType.ToString() + target.MaterialType.ToString() + "HitSFX" + randomValue.ToString(), Define.Sound.Effect, 1);
                    _playedSound.Add(target.ObjectType);
                }
                else
                {
                    //주의! 해당 경로에는 반드시 ObjectType + 프리팹 이름으로 저장해놔야함 
                    //ex) ObjectType이 만약 Player일 경우, 해당 경로에 저장되는 오브젝트의 이름은 "PlayerHitVFX"
                    Managers.Resource.Instantiate("Effects/" + target.ObjectType.ToString() + "", contactPoint, Quaternion.identity);
                    //Sound도 마찬가지
                    if (_playedSound.Contains(target.ObjectType))
                        return;
                    Managers.Sound.Play("Sounds/" + target.ObjectType.ToString() + "", Define.Sound.Effect, 1);
                    _playedSound.Add(target.ObjectType);
                }
            }
            else //막타가 아닌 경우에
            {
                if (target.ObjectType == GameObjectType.Nature)
                {
                    //주의! 만약 NatureType일경우 "ObjectType + MaterialType +  이름"으로 저장해놔야함 
                    //ex) ObjectType이 만약 Nature일 경우, 해당 경로에 저장되는 오브젝트의 이름은 "NatrueStoneHitVFX"
                    int randomValue = UnityEngine.Random.Range(1, 4); // 1부터 3까지의 값을 생성
                    Managers.Resource.Instantiate("Effects/" + target.ObjectType.ToString() + target.MaterialType.ToString() + "HitVFX" + randomValue.ToString(), contactPoint, Quaternion.identity);
                    //Sound도 마찬가지
                    if (_playedSound.Contains(target.ObjectType))
                        return;
                    Managers.Sound.Play("Sounds/FarmingSound/" + target.ObjectType.ToString() + target.MaterialType.ToString() + "HitSFX" + randomValue.ToString(), Define.Sound.Effect, 1);
                    _playedSound.Add(target.ObjectType);
                }
                else if (target.ObjectType == GameObjectType.Structure)
                {
                    //주의! 만약 NatureType일경우 "ObjectType + MaterialType +  이름"으로 저장해놔야함 
                    //ex) ObjectType이 만약 Nature일 경우, 해당 경로에 저장되는 오브젝트의 이름은 "NatrueStoneFinalHitVFX"
                    int randomValue = UnityEngine.Random.Range(1, 4); // 1부터 3까지의 값을 생성
                    //ex) 이때 오브젝트의 이름은 "NatrueStoneFinalHitVFX"+ "1~3"중에 하나의 숫자를 붙여줘야 랜덤하게재생됨
                    Managers.Resource.Instantiate("Effects/" + target.ObjectType.ToString() + target.MaterialType.ToString() + "HitVFX" + randomValue.ToString(), contactPoint, Quaternion.identity);
                    //Sound도 마찬가지
                    if (_playedSound.Contains(target.ObjectType))
                        return;
                    Managers.Sound.Play("Sounds/FarmingSound/" + target.ObjectType.ToString() + target.MaterialType.ToString() + "HitSFX" + randomValue.ToString(), Define.Sound.Effect, 1);
                    _playedSound.Add(target.ObjectType);
                }
                else if (target.ObjectType == GameObjectType.Monster)
                {
                    //주의! 만약 NatureType일경우 "ObjectType + MaterialType +  이름"으로 저장해놔야함 
                    //ex) ObjectType이 만약 Nature일 경우, 해당 경로에 저장되는 오브젝트의 이름은 "NatrueStoneFinalHitVFX"
                    int randomValue = UnityEngine.Random.Range(1, 4); // 1부터 3까지의 값을 생성
                    //ex) 이때 오브젝트의 이름은 "NatrueStoneFinalHitVFX"+ "1~3"중에 하나의 숫자를 붙여줘야 랜덤하게재생됨
                    Managers.Resource.Instantiate("Effects/Player/MonsterHit", contactPoint, Quaternion.identity);
                    //Sound도 마찬가지
                    if (_playedSound.Contains(target.ObjectType))
                        return;
                    //Managers.Sound.Play("Sounds/MonsterHitSound/" + WeaponType.ToString() + "HitSFX" + randomValue.ToString(), Define.Sound.Effect, 1);
                    _playedSound.Add(target.ObjectType);
                }
                else
                {
                    Managers.Resource.Instantiate("Effects/" + target.ObjectType.ToString() + "HitVFX", contactPoint, Quaternion.identity);
                    if (_playedSound.Contains(target.ObjectType))
                        return;
                    Managers.Sound.Play("Sounds/" + target.ObjectType.ToString() + "HitSFX", Define.Sound.Effect, 1);
                    _playedSound.Add(target.ObjectType);
                }
            }
        }
    }

    /*public virtual void InstantiateHitVFX(Vector3 contactPoint, BaseController target)
    {
        GameObject hitVFX = null;
        //막타 칠 때
        if (target.Hp <= 0)
        {
            switch (target.ObjectType)
            {
                case GameObjectType.Monster:
                    Debug.Log("막타이펙트");
                    hitVFX = monsterFinalHitVFX;
                    Debug.Log(hitVFX);
                    break;

                case GameObjectType.Nature:
                    switch (target.MaterialType)
                    {
                        case MaterialType.Stone:
                            hitVFX = stoneFinalHitVFX;
                            break;

                        case MaterialType.Tree:
                            hitVFX = treeFinalHitVFX;
                            break;

                        default:
                            hitVFX = monsterFinalHitVFX;
                            break;
                    }
                    break;

                default:
                    hitVFX = monsterFinalHitVFX;
                    break;
            }
        }
        //평상시
        else
        {
            switch (target.ObjectType)
            {
                case GameObjectType.Monster:
                    hitVFX= monsterHitVFX;
                    break;

                case GameObjectType.Nature:
                    switch (target.MaterialType)
                    {
                        case MaterialType.Stone:
                            hitVFX = stoneHitVFX;
                            break;

                        case MaterialType.Tree:
                            hitVFX = treeHitVFX;
                            break;

                        default:
                            hitVFX = monsterHitVFX;
                            break;
                    }
                    break;

                default:
                    hitVFX = monsterHitVFX;
                    break;
            }
        }

        if(hitVFX == null)
        {
            switch (target.ObjectType)
            {
                case GameObjectType.Monster:
                    hitVFX = Managers.Resource.Load<GameObject>($"Prefabs/Effects/Player/MonsterSpecialHit");
                    Debug.Log(hitVFX);
                    break;

                case GameObjectType.Nature:
                    switch (target.MaterialType)
                    {
                        case MaterialType.Stone:
                            hitVFX = Managers.Resource.Load<GameObject>($"Prefabs/");
                            break;

                        case MaterialType.Tree:
                            hitVFX = Managers.Resource.Load<GameObject>($"Prefabs/");
                            break;

                        default:
                            hitVFX = Managers.Resource.Load<GameObject>($"Prefabs/");
                            break;
                    }
                    break;

                default:
                    hitVFX = Managers.Resource.Load<GameObject>($"Prefabs/");
                    break;
            }
        }
        Managers.Resource.Instantiate(hitVFX, contactPoint, Quaternion.identity);
    }

    public virtual void InstantiateHitSFX(BaseController target)
    {
        AudioClip audioClip = null;
        if (target.Hp <= 0)
        {
            switch (target.ObjectType)
            {
                case GameObjectType.Monster:
                    audioClip = monsterFinalHitSFX;
                    break;

                case GameObjectType.Nature:
                    switch (target.MaterialType)
                    {
                        case MaterialType.Stone:
                            audioClip = stoneFinalHitSFX;
                            break;

                        case MaterialType.Tree:
                            audioClip = treeFinalHitSFX;
                            break;

                        default:
                            audioClip = monsterFinalHitSFX;
                            break;
                    }
                    break;

                default:
                    audioClip = monsterFinalHitSFX;
                    break;
            }
            return;
        }
        //평상시
        else
        {
            switch (target.ObjectType)
            {
                case GameObjectType.Monster:
                    audioClip=monsterHitSFX;
                    break;

                case GameObjectType.Nature:
                    switch (target.MaterialType)
                    {
                        case MaterialType.Stone:
                            audioClip = stoneHitSFX;
                            break;

                        case MaterialType.Tree:
                            audioClip = treeHitSFX;
                            break;

                        default:
                            audioClip = monsterHitSFX;
                            break;
                    }
                    break;

                default:
                    audioClip = monsterHitSFX;
                    break;
            }
        }
        if(audioClip == null)
        {
            switch (target.ObjectType)
            {
                case GameObjectType.Monster:
                    System.Random random = new System.Random();
                    int randomValue = random.Next(1, 4); // 1부터 3까지의 값을 생성
                    if(randomValue == 1)
                        audioClip = Managers.Resource.Load<AudioClip>($"Prefabs/Effects/Player/MonsterSpecialHit");
                    else if (randomValue == 2)
                        audioClip = Managers.Resource.Load<AudioClip>($"Prefabs/Effects/Player/MonsterSpecialHit");
                    else if (randomValue == 3)
                        audioClip = Managers.Resource.Load<AudioClip>($"Prefabs/Effects/Player/MonsterSpecialHit");
                    break;

                case GameObjectType.Nature:
                    switch (target.MaterialType)
                    {
                        case MaterialType.Stone:
                            audioClip = Managers.Resource.Load<AudioClip>($"Prefabs/Effects/Player/MonsterSpecialHit");
                            break;

                        case MaterialType.Tree:
                            audioClip = Managers.Resource.Load<AudioClip>($"Prefabs/Effects/Player/MonsterSpecialHit");
                            break;

                        default:
                            audioClip = Managers.Resource.Load<AudioClip>($"Prefabs/Effects/Player/MonsterSpecialHit");
                            break;
                    }
                    break;

                default:
                    audioClip = Managers.Resource.Load<AudioClip>($"Prefabs/Effects/Player/MonsterSpecialHit");
                    break;
            }
        }
        
        Managers.Sound.Play(audioClip, Define.Sound.Effect, 1);
        //GameObject emptyObject = new GameObject("SkillSFX");
        //Managers.Sound.Play(emptyObject, audioClip, 0, 100);

    }
    */  //이전 SFX, VFX
}

[System.Serializable]
public class FxInfo
{
    public GameObjectType type;
    public MaterialType material;
    public GameObject HitVFX;
    public GameObject FinalHitVFX;
    public AudioClip HitSFX;
    public AudioClip BossShieldHitSFX;
}


[System.Serializable]
public class Stun
{
    public StunType type;
    public float stunDuration;
    public float force;

    float remainStunDuration;
    public float RemainStunDuration { get { return remainStunDuration; } set { remainStunDuration = value; } }
}

[System.Serializable]
public class StatusAffect
{
    public StatusAffectType type;
    public float affectDuration;
    [Range(0, 1)]
    public float affectValuePercentage;

    GameObject affectVFX;
    public GameObject AffectVFX { get { return affectVFX; } set { affectVFX = value; } }
    float remainAffectDuration;
    public float RemainAffectDuration { get { return remainAffectDuration; } set { remainAffectDuration = value; } }
    float lastDamagedTime;
    public float LastDamagedTime { get { return lastDamagedTime; } set { lastDamagedTime = value; } }
}