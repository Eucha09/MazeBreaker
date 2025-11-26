using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class SpiderController : MonsterController2
{
    [Header("SpiderCount")]
    public int rangerSpiderMaxCount = 2;
    public int MeleeSpiderMaxCount = 2;

    //각 종류별로 List로 자식 스파이더들을 갖고있음
    //알에서 무사히 부화했을때 List에 추가되며,죽었을때 List에서 해제됨
    public List<GameObject> RangerSpiders;
    public List<GameObject> MeleeSpiders;
    public List<GameObject> BombSpiders;

    protected override void Start()
    {
        base.Start();

        Hp = 1000;
        MaxHp = 1000;
        //초기 상태
        CurrentState = new Monster.Search.Idle();

        //패턴 데이터
        _patterns.Add(new SpiderPatternsInfo.Paze1RoarPatternInfo(this, 0, 0));
        _patterns.Add(new SpiderPatternsInfo.Paze2RoarPatternInfo(this, 0, 0));

        // 플레이어와 거리 12만큼 항상 유지한다.
        //_patterns.Add(new SpiderPatternsInfo.LayEggPatternInfo(this,40f));
        _patterns.Add(new SpiderPatternsInfo.RushPatternInfo(this, 10, 3, 15, 3));
        //_patterns.Add(new SpiderPatternsInfo.PoisonPatternInfo(this, 10,1, 40));
        _patterns.Add(new SpiderPatternsInfo.SlashPatternInfo(this, 15,1, 5));
       // _patterns.Add(new SpiderPatternsInfo.LayEggPatternInfo(this,50f));
        _patterns.Add(new SpiderPatternsInfo.WebPatternInfo(this, 10,5, 20));

        _patterns.Add(new SpiderPatternsInfo.WalkPatternInfo(this));
        

    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        CurrentState.HandleOnTriggerEnter(other);
    }
    public override void OnDamaged(DamageCollider dm)
    {
        CurrentState.OnDamaged(dm);

        UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
        ui.TargetBar.SetInfo(this);
    }
}
