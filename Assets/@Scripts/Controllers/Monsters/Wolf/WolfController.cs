using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public class WolfController : MonsterController2
{

    protected override void Start()
    {
        base.Start();
        //초기 상태
        CurrentState = new Monster.Search.Idle();
        Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform).SetInfo(this, 1.8f);

        //_patterns.Add(new WolfPatternsInfo.BitePatternInfo(this, 3, 5));
        _patterns.Add(new WolfPatternsInfo.DashAttackPatternInfo(this, 6, 5));
        _patterns.Add(new WolfPatternsInfo.OrbitMovePatternInfo(this));
    }

}
