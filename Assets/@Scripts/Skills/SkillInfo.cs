using Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public enum SkillKey
{
    Default,
    Q,
    E,
    C
}

public abstract class SkillInfo2
{
    protected int _templateId;
    protected PlayerController _skillUser;
    protected string _skillName;
    protected SkillKey _skillKey;
    public SkillKey SkillKey { get { return _skillKey; } set { _skillKey = value; } }

    protected int _staminaConsumptionCount;


    Vector3 _attackPos;
    public Vector3 AttackPos { get { return _attackPos; } set { _attackPos = value; } }

    protected float _detectRadius = 8f;
    protected float _detectAngle = 90f; 
    public GameObject DetectedTarget;


    public int TemplateId { get { return _templateId; } set { _templateId = value; } }
    public int StaminaConsumptionCount { get { return _staminaConsumptionCount; } set { _staminaConsumptionCount = value; } }

    public virtual bool SkillConditionCheck()
    {
        /*
        if (Time.time - _lastSkillUsedTime > _skillCoolTime)
        {
            return true;
        }
        return false;
        */
        if(_skillUser.CurrentStaminaCount - _staminaConsumptionCount < 0)
        {
            return false;
        }
        return true;
    }

    public virtual void UseSkill() 
    {
        _skillUser.UseStamina(_staminaConsumptionCount);
        //_skillUser.CurrentStaminaCount -= _staminaConsumptionCount;
    }

    //스킬쪽에서도 만약 미리 버퍼에 저장해둬야하는 경우가 생길경우
    public virtual void BufferInput() { }

    public virtual void GetAttackPos()
    {
        //만약 커서가 없는 상태라면, 주변에서 적 서칭
        //부채꼴
        if (_skillUser.GetComponent<PlayerInput>().currentControlScheme == "Gamepad")
        {
            DetectedTarget = DetectTarget();
            if (DetectedTarget == null)
            {
                Vector3 Dir = new Vector3(_skillUser.MoveDir.x, 0, _skillUser.MoveDir.y).normalized;
                Dir = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * Dir;
                Dir.y = 0;
                _attackPos  = _skillUser.transform.position + Dir * 3f;
            }
            else
            {
                _attackPos = DetectedTarget.transform.position;
            }
        }
        else 
        {
        Vector3 mousePosition = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(_skillUser.transform.position.x, _skillUser.transform.position.y, _skillUser.transform.position.z));

        float rayLength;
        if (groundPlane.Raycast(ray, out rayLength))
        {
            _attackPos = ray.GetPoint(rayLength);
        }

        }
    }

    public virtual GameObject DetectTarget()
    {
        Collider[] targets = Physics.OverlapSphere(_skillUser.transform.position, _detectRadius, _skillUser.AttackTargetLayer);
        List<GameObject> detectedTargets = new List<GameObject>();

        foreach (Collider target in targets)
        {
            Vector3 dirToTarget = (target.transform.position - _skillUser.transform.position).normalized;

            Vector3 Dir = new Vector3(_skillUser.MoveDir.x, 0, _skillUser.MoveDir.y).normalized;

            if (Dir == Vector3.zero)
                Dir = _skillUser.transform.forward;

            Dir = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * Dir;
            Dir.y = 0;

            float angle = Vector3.Angle(Dir, dirToTarget);
            if (angle <= _detectAngle / 2)
            {
                detectedTargets.Add(target.gameObject);
            }
        }

        if (detectedTargets.Count == 0)
            return null;

        GameObject closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject target in detectedTargets)
        {
            float distance = Vector3.Distance(_skillUser.transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        if (closestTarget != null)
        {
            return closestTarget;
        }
        else
        {
            return null;
        }


    }

    public static SkillInfo2 MakeSkillInfo(int templateId, PlayerController skillUser)
    {
        SkillInfo2 skillInfo = null;

        SkillData skillData = null;
        Managers.Data.SkillDict.TryGetValue(templateId, out skillData);
        if (skillData == null)
            return null;


		Type type = Type.GetType(skillData.skillType);
		if (type == null)
		{
			Debug.Log("Class not found!!!");
            return null;
		}

		object obj = Activator.CreateInstance(type, new object[] { skillUser });
        skillInfo = obj as SkillInfo2;
        if (skillInfo != null)
        {
            skillInfo.TemplateId = templateId;
            skillInfo.StaminaConsumptionCount = skillData.costCount;
        }

        return skillInfo;
	}
}


public class InteractionSkillInfo : SkillInfo2
{

    public InteractionSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _staminaConsumptionCount = 0;
    }

    public override void UseSkill()
    {
        _skillUser.CurrentState = new Player.PlayerInteractionSkillReadyState();
    }
}

interface ComboType
{
    int ComboCount { get; set; }
}

/*
public class AxeComboAttackSkillInfo : SkillInfo2, ComboType
{
    int _currentComboCount;
    public int CurrentComboCount { get { return _currentComboCount; } private set { } }
    int _maxComboCount;
    float _lastComboUseTime;
    float _comboDuration;
    public int ComboCount { get { return _currentComboCount;  } set { _currentComboCount = value; } }

    public AxeComboAttackSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _currentComboCount = 1;
        _maxComboCount = 3;
        _comboDuration = 1.25f;
        _staminaConsumptionCount = 1;
    }


    public override bool SkillConditionCheck()
    {
        return true;
    }
    

    public override void UseSkill()         //플레이어가 공격 또는 스킬을 눌렀을 때 호출되는 함수 (q,e, 좌클릭)
    {
        if (_currentComboCount >= _maxComboCount || Time.time - _lastComboUseTime > _comboDuration)
            _currentComboCount = 0;

        _lastComboUseTime = Time.time;
        GetAttackPos();
        _currentComboCount++;
        
        /*
        if (_currentComboCount == _maxComboCount)
            _skillUser.UseStamina(_staminaConsumptionCount);
        
        // _skillUser.UseStamina(0);  스태미나 회복되게

        _skillUser.CurrentState = new Player.AxeComboAttackState(this);
        
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}
*/

/*
public class MaceComboAttackSkillInfo : SkillInfo2, ComboType
{
    int _currentComboCount;
    public int CurrentComboCount { get { return _currentComboCount; } private set { } }
    int _maxComboCount;
    float _lastComboUseTime;
    float _comboDuration;
    public int ComboCount { get { return _currentComboCount; } set { _currentComboCount = value; } }

    public MaceComboAttackSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _currentComboCount = 1;
        _maxComboCount = 3;
        //_skillCoolTime = 0;
        _comboDuration = 1.25f;
        _staminaConsumptionCount = 1;
    }

    public override bool SkillConditionCheck()
    {
        return true;
    }

    /*
    public override bool SkillConditionCheck()
    {
        if (RemainingCoolTime() <= 0)// 쿨타임 체크
        {
            return true;
        }
        return false;
    }
    

    public override void UseSkill()
    {
        //_lastSkillUsedTime = Time.time;
        if (_currentComboCount >= _maxComboCount || Time.time - _lastComboUseTime > _comboDuration)
            _currentComboCount = 0;

        _lastComboUseTime = Time.time;
        GetAttackPos();
        _currentComboCount++;

        /*
        if (_currentComboCount == _maxComboCount)
            _skillUser.UseStamina(_staminaConsumptionCount);
        
        //_skillUser.UseStamina(0);

        _skillUser.CurrentState = new Player.MaceComboAttackState(this);
        
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}

public class SwordComboAttackSkillInfo : SkillInfo2, ComboType
{
    int _currentComboCount;
    public int CurrentComboCount { get { return _currentComboCount; } private set { } }
    int _maxComboCount;
    float _lastComboUseTime;
    float _comboDuration;
    public int ComboCount { get { return _currentComboCount; } set { _currentComboCount = value; } }

    public SwordComboAttackSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _currentComboCount = 1;
        _maxComboCount = 3;
        _comboDuration = 1.25f;
        _staminaConsumptionCount = 1;
    }

    public override void UseSkill()
    {
        if (_currentComboCount >= _maxComboCount || Time.time - _lastComboUseTime > _comboDuration)
            _currentComboCount = 0;

        _lastComboUseTime = Time.time;
        GetAttackPos();
        _currentComboCount++;

        /*
        if (_currentComboCount == _maxComboCount)
            _skillUser.UseStamina(_staminaConsumptionCount);
        
        //_skillUser.UseStamina(0);

        _skillUser.CurrentState = new Player.SwordComboAttackState(this);
        
    }

    public override bool SkillConditionCheck()
    {
        return true;
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}
*/

/*
public class BowChargeShotSkillInfo : SkillInfo2
{
    Vector3 _arrowDirection;
    public Vector3 ArrowDirection { get { return _arrowDirection; } set { _arrowDirection = value; } }
    Vector3 _slidingVector;
    public Vector3 SlidingVector { get { return _slidingVector; } set { _slidingVector = value; } }
    float _slidingForce;
    public float SlidingForce { get { return _slidingForce; } set { _slidingForce = value; } }
    float _currentArrowDist;
    public float CurrentArrowDist { get { return _currentArrowDist; } set { _currentArrowDist = value; } }
    float _arrowMaxDist;
    public float ArrowMaxDist { get { return _arrowMaxDist; } set { _arrowMaxDist = value; } }
    float _maxChargeTime;
    public float MaxChargeTime { get { return _maxChargeTime; } set { _maxChargeTime = value; } }
    float _chargeStartTime;
    public float ChargeStartTime { get { return _chargeStartTime; } set { _chargeStartTime = value; } }
    float _maxDamage;
    public float MaxDamage { get { return _maxDamage; } set { _maxDamage = value; } }

    public BowChargeShotSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _currentArrowDist = 0;
        _arrowMaxDist = 10;
        _maxChargeTime = 0.8f;
        _maxDamage = 20f;
        _staminaConsumptionCount = 0;
        _detectAngle = 90;
        _detectRadius = 10;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        _currentArrowDist = 0;
        _chargeStartTime = Time.time;

        if (_skillUser.CurrentState.GetState() == Player.PlayerStateType.Dash)
        {
            _slidingVector = _skillUser.transform.forward;
            _slidingForce = 10f;
        }
        else
        {
            _slidingVector = _skillUser.transform.forward;
            _slidingForce = 0f;
        }
        _skillUser.CurrentState = new Player.PlayerBowChargeState(this);
    }

    public void ArrowDistCalculate()
    {
        _currentArrowDist = _arrowMaxDist * ((Time.time - _chargeStartTime) / (_maxChargeTime)) > _arrowMaxDist
                ? _arrowMaxDist : _arrowMaxDist * ((Time.time - _chargeStartTime) / (_maxChargeTime));
    }

}

public class AxeThrowSkillInfo : SkillInfo2
{

    public AxeThrowSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _staminaConsumptionCount = 2;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();
        _skillUser.CurrentState = new Player.AxeThrowAttackState(this);
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}


public class AxeSlashUpSkillInfo : SkillInfo2
{

    public AxeSlashUpSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _staminaConsumptionCount = 4;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();
        _skillUser.CurrentState = new Player.AxeSlashUpAttackState(this);
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}


public class AxeSlashDownSkillInfo : SkillInfo2
{

    public AxeSlashDownSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _staminaConsumptionCount = 1;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();
        _skillUser.CurrentState = new Player.AxeSlashDownAttackState(this);
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}

public class AxeSpinSkillInfo : SkillInfo2
{
    float _spinDuration;
    float _spinMoveSpeed;

    public float SpinDuration { get { return _spinDuration; } set { _spinDuration = value; } }
    public float SpinMoveSpeed { get { return _spinMoveSpeed; } set { _spinMoveSpeed = value; } }

    public AxeSpinSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _spinDuration = 1f;
        _spinMoveSpeed = 5f;
        _staminaConsumptionCount = 2;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        _skillUser.CurrentState = new Player.AxeSpinAttackState(this);
    }

}


public class BowArrowRainSkillInfo : SkillInfo2
{
    Vector3 _slidingVector;
    public Vector3 SlidingVector { get { return _slidingVector; } set { _slidingVector = value; } }
    float _slidingForce;
    public float SlidingForce { get { return _slidingForce; } set { _slidingForce = value; } }

    public BowArrowRainSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _staminaConsumptionCount = 3;
        _detectRadius = 15f;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();

        if (_skillUser.CurrentState.GetState() == Player.PlayerStateType.Dash)
        {
            _slidingVector = _skillUser.transform.forward;
            _slidingForce = 10f;
        }
        else
        {
            _slidingVector = Vector3.zero;
            _slidingForce = 0f;
        }

        _skillUser.CurrentState = new Player.BowArrowRainAttackState(this);
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}
public class BowOneShotSkillInfo : SkillInfo2
{
    Vector3 _slidingVector;
    public Vector3 SlidingVector { get { return _slidingVector; } set { _slidingVector = value; } }
    float _slidingForce;
    public float SlidingForce { get { return _slidingForce; } set { _slidingForce = value; } }
    public BowOneShotSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _staminaConsumptionCount = 2;
        _detectRadius = 15f;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();

        if (_skillUser.CurrentState.GetState() == Player.PlayerStateType.Dash)
        {
            _slidingVector = _skillUser.transform.forward;
            _slidingForce = 10f;
        }
        else
        {
            _slidingVector = Vector3.zero;
            _slidingForce = 0f;
        }

        _skillUser.CurrentState = new Player.BowOneShotAttackState(this);
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}


public class MaceSmashSkillInfo : SkillInfo2
{

    public MaceSmashSkillInfo(PlayerController skillUser)
    {
        _skillUser=skillUser;
        _staminaConsumptionCount = 2;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();
        _skillUser.CurrentState = new Player.MaceSmashAttackState(this);
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }

}
public class MaceCircleSkillInfo : SkillInfo2
{

    public MaceCircleSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _staminaConsumptionCount = 2;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();
        _skillUser.CurrentState = new Player.MaceCircleAttackState(this);
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }

}
public class MacePulseSkillInfo : SkillInfo2
{

    public MacePulseSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _staminaConsumptionCount = 3;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();
        _skillUser.CurrentState = new Player.MacePulseAttackState(this);
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }

}
public class SwordPrickSkillInfo : SkillInfo2
{

    public SwordPrickSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _staminaConsumptionCount = 2;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();
        _skillUser.CurrentState = new Player.SwordPrickAttackState(this);
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}
public class SwordGroundSkillInfo : SkillInfo2
{

    public SwordGroundSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _staminaConsumptionCount = 3;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();
        _skillUser.CurrentState = new Player.SwordGroundAttackState(this);
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}
*/

public class HammerBuildingModeSkillInfo : SkillInfo2
{

    public HammerBuildingModeSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _staminaConsumptionCount = 0;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();
        _skillUser.CurrentState = new Player.BuildingModeState();
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}

public class HammerDemolitionModeSkillInfo : SkillInfo2
{

    public HammerDemolitionModeSkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;
        _staminaConsumptionCount = 0;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();
        _skillUser.CurrentState = new Player.DemolitionModeState();
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}

public class ParrySkillInfo : SkillInfo2
{

    public ParrySkillInfo(PlayerController skillUser)
    {
        _skillUser = skillUser;

    }

    public override void UseSkill()
    {
        base.UseSkill();
        GetAttackPos();
        _skillUser.CurrentState = new Player.BlockState(this);
    }

    public override void BufferInput()
    {
        GetAttackPos();
    }
}



/*
public class ChargeSkillInfo
{
    public GameObject _skillUser;//스킬 사용하는 사람

    public string skillName;     // 스킬 이름
    public float damage;         // 스킬 데미지
    public float cooldown;       // 스킬 쿨타임
    public Vector3 attackPos;//공격할 위치
    public Sprite skillIcon;     // 스킬 아이콘 (UI에 표시하기 위한 이미지)

    public Vector3 _slidingVector;
    public float _slidingForce;

    Vector3 _arrowDirection;
    float _arrowDist = 0;
    float _arrowMaxDist = 15;
    float _maxChargeTime = 0.8f;//?李⑥쭠源뚯? ?꾨떖?섎뒗??嫄몃━???쒓컙
    float _chargeStartTime;

    // 스킬 효과 설명
    public string description;   // 스킬에 대한 설명

    public void SkillStart(GameObject skillUser)
    {
        _skillUser = skillUser;

        Camera.main.GetComponent<CameraController>().ZoomOutActionCoroutin(1.0f, 1f);

        skillUser.GetComponentInChildren<Rigidbody>().linearVelocity = Vector3.zero;
        if (_slidingVector != Vector3.zero)
            skillUser.GetComponentInChildren<Rigidbody>().AddForce(_slidingVector * _slidingForce, ForceMode.Impulse);
        _chargeStartTime = Time.time;//李⑥? ?쒖옉?쒓컙 湲곕줉
                                     //李⑥쭠 ?좊땲硫붿씠???ъ깮
        Debug.Log("?붿궡 ?μ쟾");
        skillUser.GetComponentInChildren<Animator>().Play("BowCharge");
        if (skillUser.GetComponentInChildren<PlayerController>().CurrentWeaponModel.GetComponent<Animator>() != null)
            _player.CurrentWeaponModel.GetComponent<Animator>().Play("BowCharge");
        //ArrowIndicator?앹꽦
        if (_player.ArrowIndicator == null)
        {
            _player.ArrowIndicator = Managers.Resource.Instantiate(_player.EffectPath + "ArrowIndicator", _player.transform);
            // ArrowIndicator??濡쒖뺄 醫뚰몴瑜?(0, 0, 0)?쇰줈 珥덇린??
            _player.ArrowIndicator.transform.localPosition = Vector3.zero;
        }

    }

    public void SkillUpdate()
    {

    }

    private void LookAtPos()
    {
        Vector3 direction = attackPos - _skillUser.transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _skillUser.GetComponentInChildren<Rigidbody>().rotation = targetRotation;
        }
    }
}
*/