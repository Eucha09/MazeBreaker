using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public class NightMareController : MonsterController2
{
    public LayerMask blockLayer;

    protected override void Start()
    {

        base.Start();
        //초기 상태
        CurrentState = new Monster.Search.Idle();


        // Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform).SetInfo(this, 1.8f);
        //_patterns.Add(new NightMarePatternsInfo.Paze1RoarInfo(this, 0, 0));
        //_patterns.Add(new NightMarePatternsInfo.Paze2PatternInfo(this, 0, 0));
        
        _patterns.Add(new NightMarePatternsInfo.WalkPatternInfo(this, 3));
        _patterns.Add(new NightMarePatternsInfo.PortalInPatternInfo(this, 8, 15f));
        _patterns.Add(new NightMarePatternsInfo.Ground4HitPatternInfo(this, 5, 13f));
        _patterns.Add(new NightMarePatternsInfo.BitePatternInfo(this, 3, 3f));
        //_patterns.Add(new NightMarePatternsInfo.DashPatternInfo(this, 10f, 15f));
        //_patterns.Add(new NightMarePatternsInfo.CrowPatternInfo(this, 10f, 15f));
        //_patterns.Add(new NightMarePatternsInfo.LeatherPatternInfo(this, 8f, 10));



    }

    public override void OnDamaged(DamageCollider dm)
    {
        CurrentState.OnDamaged(dm);
    
        UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
        ui.TargetBar.SetInfo(this);
    }
}
