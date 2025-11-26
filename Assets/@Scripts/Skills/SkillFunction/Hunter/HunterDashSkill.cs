using Unity.VisualScripting;
using UnityEngine;

public class HunterDashSkill : MonoBehaviour
{
    //맞은 오브젝트가 플레이어일시 HunterAttackPattern.Dash.Impact(Vector3)에 값 전달

    private void OnTriggerEnter(Collider other)
    {
        if ((other.GetComponent<BaseController>()!=null
            && other.GetComponent<BaseController>().ObjectType == Define.GameObjectType.Player)
            )//|| other.gameObject.layer == LayerMask.NameToLayer("Block"))
            {
                HunterAttackPatterns.DashPattern.Dash dashPattern = GetComponent<DamageCollider>().Caster.GetComponent<MonsterController2>().CurrentState as HunterAttackPatterns.DashPattern.Dash;
                dashPattern.Impact(other.ClosestPoint(transform.position));
            }
    }
}
