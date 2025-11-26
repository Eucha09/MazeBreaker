using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public class MechanicSoldierController : MonsterController2
{
    public GameObject _LoopSoundObject;
    public AudioSource _plasmaLoopSound;

    protected override void Start()
    {
        AudioClip plasmaClip = Managers.Resource.Load<AudioClip>("Sounds/Monster/MechanicSoldier/PlasmaLoop");
        _plasmaLoopSound = Managers.Sound.Play3DLoop(_LoopSoundObject, plasmaClip);

        base.Start();
        //초기 상태
        CurrentState = new Monster.Search.Idle();
       // Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform).SetInfo(this, 1.8f);

        //_patterns.Add(new WolfPatternsInfo.BitePatternInfo(this, 3, 5));
        _patterns.Add(new MechanicSoldierPatternsInfo.BitePatternInfo(this, 3, 8));
        _patterns.Add(new MechanicSoldierPatternsInfo.ThunderAttackPatternInfo(this, 10, 15));
        _patterns.Add(new MechanicSoldierPatternsInfo.OrbitMovePatternInfo(this));
    }

    public override void OnDamaged(DamageCollider dm)
    {
        CurrentState.OnDamaged(dm);
    
        UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
        ui.TargetBar.SetInfo(this);
    }
}
