using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class HunterController : MonsterController2
{

    protected override void Start()
    {
        base.Start();
        //초기 상태
        CurrentState = new Monster.Search.Idle();
        Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform).SetInfo(this, 1.8f);

        _patterns.Add(new HunterPatternsInfo.DashPatternInfo(this, 0f, 5f));
        _patterns.Add(new HunterPatternsInfo.WalkPatternInfo(this, 5f));
        //_patterns.Add(new HunterPatternsInfo.WalkPatternInfo(this, 3f));
    }

}
