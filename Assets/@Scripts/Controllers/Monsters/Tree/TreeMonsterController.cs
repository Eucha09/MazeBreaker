using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class TreeMonsterController : MonsterController2
{

    //_currentState는 상세 상태(Idle, move, 등등)
    TreeMonster.State _currentState;

    public TreeMonster.State CurrentState
    {
        get { return _currentState; }
        set
        {
            if (_currentState != null)
            {
                _currentState.ExitState();
            }

            _currentState = value;
            _currentState.EnterState(this);
        }
    }


    protected override void Start()
    {
        base.Start();

        //hpBar 추가
        Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform).SetInfo(this, 3.5f);

        CurrentState = new TreeMonster.Search.Idle(3f);

        //패턴 데이터
        _patterns.Add(new TreeMonsterPattern.JumpAttackPatternInfo(this, 10, 5));
        _patterns.Add(new TreeMonsterPattern.SpitAttackPatternInfo(this, 10, 5));
    }

    protected override void FixedUpdate()
    {
        CurrentState.UpdateState();
    }

    public override void OnDamaged(DamageCollider dm)
    {
        //데미지 받는 로직 자유롭게 작성
        _currentState.OnDamaged(dm);
    }


    private void OnTriggerEnter(Collider other)
    {
        _currentState.HandleOnTriggerEnter(other);
    }
}
