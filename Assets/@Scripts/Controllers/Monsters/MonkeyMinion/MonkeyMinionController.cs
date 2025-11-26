using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public class MonkeyMinonController : MonsterController2
{
    public enum AttackType
    {
        Ranger,
        Melee,
        Melee2
    }
    [Header("MonkeyMinion Properties")]
    public AttackType type;


    protected override void Start()
    {
        base.Start();
        //초기 상태
        CurrentState = new Monster.Search.Idle();
        Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform).SetInfo(this, 3f);

        //패턴 데이터
        if (type == AttackType.Melee)
        {
            //_patterns.Add(new MonkeyMinionPatternsInfo.BitePatternInfo(this, 3f, 3f));
            //_patterns.Add(new MonkeyMinionPatternsInfo.TwoBitePatternInfo(this, 3f, 4f));
            _patterns.Add(new MonkeyMinionPatternsInfo.WalkPatternInfo(this, 3f));
        }else if (type == AttackType.Ranger)
        {
            _patterns.Add(new MonkeyMinionPatternsInfo.ThrowPatternInfo(this, 6f,8f));
            _patterns.Add(new MonkeyMinionPatternsInfo.OrbitMovePatternInfo(this));
        }else if (type == AttackType.Melee2)
        {
            _patterns.Add(new MonkeyMinionPatternsInfo.BurrowDownPatternInfo(this, 10f, 4));
            //_patterns.Add(new MonkeyMinionPatternsInfo.BitePatternInfo(this, 3f, 5f));
            _patterns.Add(new MonkeyMinionPatternsInfo.WalkPatternInfo(this, 3f));

        }
    }

}
