using Data;
using MonsterPatternsInfo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class WoodBeastController : MonsterController2 
{

    //_currentState는 상세 상태(Idle, move, 등등
    public Slider slider;
    [Header("Dev UI")]
    public Text playerDistance;
    // public TextMeshProUGUI distanceText;  // TMP 쓰면 타입 교체
    Transform _playerCached;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (slider != null)
        {
            slider.maxValue = MaxWeakness;
            slider.value = Weakness;
        }
        // 거리 갱신
        if (playerDistance != null)
        {
            if (_playerCached == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) _playerCached = p.transform;
            }

            if (_playerCached != null)
            {
                float dist = Vector3.Distance(transform.position, _playerCached.position);
                playerDistance.text = $"{dist:F2} m";
            }
            else
            {
                playerDistance.text = "-";
            }
        }
    }


    protected override void Start()
    {
        base.Start();

        //초기 상태
        CurrentState = new Monster.Search.Idle();

        //패턴 데이터
        _patterns.Add(new WoodBeastPattern.Paze1RoarInfo(this, 0, 0));
        _patterns.Add(new WoodBeastPattern.Paze2RoarInfo(this, 0, 0));
        //_patterns.Add(new WoodBeastPattern.ThrowRockPatternInfo(this, 14, 10));

        //RepeatAttackPatternData ThrowRock = new RepeatAttackPatternData();
        AttackPatternData throwRock = new AttackPatternData();
        throwRock.MinDist = 0;
        throwRock.MaxDist = 10;
        throwRock.PatternCoolTime = 14;
        throwRock.AnimationName = "ThrowRockAttack";
        throwRock.PatternName = "ThrowRockAttack";
        throwRock.PatternUser = this;
        _patterns.Add(new WoodBeastPattern.Paze2AttackPatternInfo(throwRock));

        //ThrowRock.RepeatAnimationName = "ThrowRockAttack";
        //ThrowRock.LastAnimationName = "ThrowRockAttack";
        //ThrowRock.RepeatCount = 2;


        //_patterns.Add(new WoodBeastPattern.Paze2JumpAttackPatternInfo(this, 10, 8));

        RepeatAttackPatternData paze2JumpAttack = new RepeatAttackPatternData();
        paze2JumpAttack.PatternUser = this;
        paze2JumpAttack.PatternName = "Paze2JumpAttack";
        paze2JumpAttack.FirstAnimationName = "Paze2JumpAttack_01";
        paze2JumpAttack.RepeatAnimationName= "Paze2JumpAttack_02";
        paze2JumpAttack.LastAnimationName = "JumpAttack";
        paze2JumpAttack.RepeatCount = 3;
        paze2JumpAttack.MinDist= 0;
        paze2JumpAttack.MaxDist = 8;
        paze2JumpAttack.PatternCoolTime = 10;
        _patterns.Add(new WoodBeastPattern.Paze2RepeatAttackPatternInfo(paze2JumpAttack));


        //_patterns.Add(new WoodBeastPattern.JumpAttackPatternInfo(this, 15, 9));

        AttackPatternData JumpAttack = new AttackPatternData();
        JumpAttack.MinDist = 0f;
        JumpAttack.MaxDist = 15f;
        JumpAttack.PatternCoolTime = 12f;
        JumpAttack.AnimationName = "JumpAttack";
        JumpAttack.PatternName = "JumpAttack";
        JumpAttack.PatternUser = this;
        _patterns.Add(new WoodBeastPattern.Paze1AttackPatternInfo(JumpAttack));


        //_patterns.Add(new WoodBeastPattern.LeaveStormPatternInfo(this, 30, 12));
        //_patterns.Add(new WoodBeastPattern.GasiGallePatternInfo(this, 12, 5));
        //_patterns.Add(new WoodBeastPattern.DoubleSwingPatternInfo(this, 5, 4));
        //_patterns.Add(new WoodBeastPattern.WalkPatternInfo(this, 3));


        AttackPatternData leaveStrom = new AttackPatternData();
        leaveStrom.MinDist = 0f;
        leaveStrom.MaxDist = 14f;
        leaveStrom.PatternCoolTime = 25f;
        leaveStrom.AnimationName = "LeaveStormAttack";
        leaveStrom.PatternName = "LeaveStorm";
        leaveStrom.PatternUser = this;
        _patterns.Add(new MonsterPatternsInfo.AttackPatternInfo(leaveStrom));

        AttackPatternData gasigale = new AttackPatternData();
        gasigale.MinDist = 5f;
        gasigale.MaxDist = 10f;
        gasigale.PatternCoolTime = 11f;
        gasigale.AnimationName = "GasiGalleAttack";
        gasigale.PatternName = "GasiGalle";
        gasigale.PatternUser = this;
        _patterns.Add(new MonsterPatternsInfo.AttackPatternInfo(gasigale));


        AttackPatternData doubleSwing = new AttackPatternData();
        doubleSwing.PatternUser = this;
        doubleSwing.PatternName = "DoubleSwing";
        doubleSwing.AnimationName = "DoubleSwing";
        doubleSwing.MinDist = 0f;
        doubleSwing.MaxDist = 4f;
        doubleSwing.PatternCoolTime = 6f;
        _patterns.Add(new MonsterPatternsInfo.AttackPatternInfo(doubleSwing));

        MoveToTargetPatternData moveToTargetPatternData = new MoveToTargetPatternData();
        moveToTargetPatternData.Dist = 3f;
        moveToTargetPatternData.AnimationName = "Walk";
        moveToTargetPatternData.PatternName = "MoveToTarget";
        moveToTargetPatternData.PatternUser = this;
        _patterns.Add(new MonsterPatternsInfo.MoveToTargetPatternInfo(moveToTargetPatternData));
        

    }

    public override void OnDamaged(DamageCollider dm)
    {
        CurrentState.OnDamaged(dm);

        UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
        ui.TargetBar.SetInfo(this);
    }

}
