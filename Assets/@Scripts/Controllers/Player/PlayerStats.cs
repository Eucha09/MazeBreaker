using JetBrains.Annotations;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    [SerializeField]
    private PlayerController _player;

    #region Variables
    [SerializeField]
    private float _maxHP = 100f;

    [SerializeField]
    private float _currentHP;

    [SerializeField]
    private float _maxStamina = 100f;

    [SerializeField]
    private float _currentStamina;

    [SerializeField]
    private float _maxHunger = 100f;

    [SerializeField]
    private float _currentHunger;

	[SerializeField]
	private float _maxPositiveEnergy = 200f;

	[SerializeField]
	private float _currentPositiveEnergy;

	[SerializeField]
    private float _dashStamina = 25f;

    public float MaxStamina
    {
        get { return _maxStamina; }
        set { _maxStamina = value; }
    }

    public float CurrentStamina
    {
        get { return _currentStamina; }
        set { _currentStamina = value; }
    }

    public float DashStamina
    {
        get { return _dashStamina; }
        set { _dashStamina = value; }
    }

    public float MaxHunger { get { return _maxHunger; } set { _maxHunger = value; } }
    public float CurrentHunger { get { return _currentHunger; } set { _currentHunger = value; } }

	public float MaxPositiveEnergy { get { return _maxPositiveEnergy; } set { _maxPositiveEnergy = value; } }
	public float CurrentPositiveEnergy { get { return _currentPositiveEnergy; } set { _currentPositiveEnergy = value; } }

    [SerializeField]
    private float _attack;

    public float Attack
    {
        get { return _attack; }
        set { _attack = value; }
    }

    [SerializeField]
    private float _defense;

    public float Defense
    {
        get { return _defense; }
        set { _defense = value; }
    }

    [SerializeField]
    private float _maxSpecialGage;

    [SerializeField]
    private float _currentSpecialGage;

    public float CurrnetSpecialGage {  get { return _currentSpecialGage; } set { _currentSpecialGage = value; } }

    public float MaxSpecialGage { get { return _maxSpecialGage; } set { _maxSpecialGage = value; } }

    [SerializeField]
    float _fillSpecialGageRetentionDuration = 0;

    float _fillSpecialGageStartTime = 0;

    [SerializeField]
    private float _moveSpeed = 5f;


    [SerializeField]
    private float _currentMoveSpeed = 5f;

    [SerializeField]
    private float _dashSpeed = 10f;

    [SerializeField]
    private AnimationCurve _velocityCurve;

    // [SerializeField]
    // private float _dashDuration = 0.2f;

    [SerializeField]
    private float _maxMental = 100f;

    [SerializeField]
    private float _currentMental;

    [SerializeField]
    private float _hpAutoRecoveryOffset;

    [SerializeField]
    private float _mentalAutoDeclineOffset;

    [SerializeField]
    private float _staminaAutoRecoveryOffset;
    
    [SerializeField]
    float _specialGageDeclineOffset;

    [SerializeField]
    float _hungerAutoDeclineOffset;

    [SerializeField]
    private bool _isHookReady = false;
    #endregion


    #region GetSet
    public float MaxHP
    {
        get { return _maxHP; }
        set { _maxHP = value; }
    }

    public float CurrentHP
    {
        get { return _currentHP; }
        set { _currentHP = value; }
    }

    

    public float CurrentMoveSpeed
    {
        get { return _currentMoveSpeed; }
        set { _currentMoveSpeed = value; }
    }

    public float MoveSpeed
    {
        get { return _moveSpeed; }
        set { _moveSpeed = value; }
    }

    public float DashSpeed
    {
        get { return _dashSpeed; }
        set { _dashSpeed = value; }
    }

    public AnimationCurve VelocityCurve
    {
        get { return _velocityCurve; }
        set { _velocityCurve = value; }
    }

    /*public float DashDuration
    {
        get { return _dashDuration; }
        set { _dashDuration = value; }
    }*/

    public float MaxMental
    {
        get { return _maxMental; }
        set { _maxMental = value; }
    }

    public float CurrentMental
    {
        get { return _currentMental; }
        set { _currentMental = value; }
    }

    public bool IsHookReady
    {
        get { return _isHookReady; }
        set { _isHookReady = value; }
    }

    #endregion


    public void UpdateStats()
    {
        /*
        if (CurrentHP < MaxHP)
        {
            CurrentHP += Time.deltaTime * _hpAutoRecoveryOffset;
        }
        else
        {
            CurrentHP = MaxHP;
        }

        if (CurrentMental >= 0)
        {
            if (CurrentHP <= 0)
            {
                CurrentMental -= Time.deltaTime * _mentalAutoDeclineOffset * 20;
            }
            else
            {
                CurrentMental -= Time.deltaTime * _mentalAutoDeclineOffset;
            }
        }
        else
        {
            CurrentMental = 0;
        }
        */

        /*
        if (CurrentHunger >= 0)
        {
            CurrentHunger -= Time.deltaTime * _hungerAutoDeclineOffset;
        }
        else
        {
            CurrentHunger = 0;
        }
        */

        /*
        if (CurrentStamina < MaxStamina) 
        {
            CurrentStamina += Time.deltaTime * _staminaAutoRecoveryOffset * (CurrentHunger/MaxHunger);
        }
        else
        {
            CurrentStamina = MaxStamina;
        }

        //특수게이지 관련
        if(Time.time - _fillSpecialGageStartTime > _fillSpecialGageRetentionDuration)
        {
            _currentSpecialGage -= Time.deltaTime * _specialGageDeclineOffset;
            if(_currentSpecialGage < 0)
                _currentSpecialGage = 0;
            ParticleSystem[] ps = _player.Aura.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < ps.Length; i++)
            {
                var main = ps[i].main;
                main.loop = false;
            }
        }
        */
    }



    public PlayerStats()
    {
        CurrentHP = MaxHP;
        CurrentStamina = MaxStamina;
        CurrentMental = MaxMental;
    }

    public void TakeDamage(float amount)
    {

        CurrentHP -= amount;
        if (CurrentHP < 0) CurrentHP = 0;
    }

    public void Heal(float amount)
    {
        CurrentHP += amount;
        if (CurrentHP > MaxHP) CurrentHP = MaxHP;
    }

    public void UseStamina(float amount)
    {
        CurrentStamina -= amount;
        if (CurrentStamina < 0) CurrentStamina = 0;
    }

    public void UseSpecialGage(float amount)
    {
        _currentSpecialGage -= amount;
        ParticleSystem[] ps = _player.Aura.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < ps.Length; i++)
        {
            var main = ps[i].main;
            main.loop = false;
        }
        if (_currentSpecialGage < 0) _currentSpecialGage = 0;
    }

    public void FillSpecialGage(float amount)
    {
        Debug.Log("다채워짐");
        _fillSpecialGageStartTime = Time.time;
        _currentSpecialGage += amount;
        if (_currentSpecialGage >= _maxSpecialGage)
        {
            _currentSpecialGage = _maxSpecialGage;
            ParticleSystem[] ps =  _player.Aura.GetComponentsInChildren<ParticleSystem>();
            for(int i=0; i<ps.Length; i++)
            {
                var main = ps[i].main;
                main.loop = true;
                ps[i].Play();
            }
        }
    }
    
    public void RecoverStamina(float amount)
    {
        CurrentStamina += amount;
        if (CurrentStamina > MaxStamina) CurrentStamina = MaxStamina;
    }

    public void RecoverHunger(float amount)
    {
        CurrentHunger += amount;
        if (CurrentHunger > MaxHunger) CurrentHunger = MaxHunger;
    }

    public void AffectMental(float amount)
    {
        CurrentMental += amount;
        if (CurrentMental > MaxMental) CurrentMental = MaxMental;
        if (CurrentMental < 0) CurrentMental = 0;
    }
}
