using UnityEngine;


public class MonsterAnimationEventReciever : MonoBehaviour
{

    [SerializeField]
    public MonsterController2 _monster;

    public void StateEnd()
    {
        if (_monster == null)
            return;
        _monster.CurrentState.StateEnd();
    }

    public void LookTargetStart()
    {
        Monster.AttackState lookingState = _monster.CurrentState as Monster.AttackState;
        lookingState.LookTargetStart();
    }
    public void LookTargetEnd()
    {
        Monster.AttackState lookingState = _monster.CurrentState as Monster.AttackState;
        if (lookingState == null)
            return;
        lookingState.LookTargetEnd();
    }


    public void AttackGlowOn()
    {
        _monster.LerpAttackGlowColorCoroutinStart();
    }

    public virtual void Dissolve()
    {
        _monster.LerpDissolveStart();
    }
    public virtual void Appear()
    {
        _monster.LerpAppearDissolveStart();
    }
    public virtual void NightMareDissolve()
    {
        _monster.NightMareLerpDissolveStart();
    }
    public virtual void NightMareAppear()
    {
        _monster.NightMareLerpAppearDissolveStart();
    }
}
