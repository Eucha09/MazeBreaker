using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class SpiderMeleeController : MonsterController2
{
    [Header("Spider Melee Properties")]
    public AttackType type;
    public enum AttackType
    {
        Ranger,
        Melee,
        Bomb
    }



    protected override void Start()
    {
        base.Start();
        //초기 상태
        CurrentState = new Monster.Search.Idle();
        Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform).SetInfo(this, 1.8f);

        //패턴 데이터
        if (type == AttackType.Melee)
        {
            _patterns.Add(new SpiderMeleePatternsInfo.BitePatternInfo(this, 3f, 3f));
            _patterns.Add(new SpiderMeleePatternsInfo.WalkPatternInfo(this, 2f));
        }else if (type == AttackType.Ranger)
        {
            _patterns.Add(new SpiderMeleePatternsInfo.PoisonBulletPatternInfo(this, 15f,8f));
            _patterns.Add(new SpiderMeleePatternsInfo.OrbitMovePatternInfo(this));
        }
    }

}
