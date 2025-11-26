using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using static Define;

public class ProjectileDamageCollider : DamageCollider
{
    public bool isPenetrate = false; // ← 추가: 관통 여부 설정 변수

    public void Init(BaseController caster, bool penetrate)
    {
        _caster = caster;
        isPenetrate = penetrate;
    }
    //GPT 관통 스킬 구현


    public void Init(BaseController caster,float damage)
    {
        _caster = caster;
        _skillMultiplier = damage;
    }


    public void Init(BaseController caster)
    {
        _caster = caster;
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (Managers.Layer.IsLayerExcluded(other.gameObject.layer))
            return;

        Debug.Log(other.name);
        base.OnTriggerEnter(other);
        /*
        if (other.GetComponent<BaseController>() == null)
            return;
        */

        /*if ((other.GetComponent<BaseController>() != _caster && other.GetComponent<BaseController>().ObjectType != _caster.ObjectType) && other.GetComponentInChildren<DamageCollider>() == null)
        {
            Destroy(gameObject);
        }*/

        if (!isPenetrate &&
    (other.GetComponent<BaseController>() != _caster &&
     other.GetComponent<BaseController>().ObjectType != _caster.ObjectType) &&
     other.GetComponentInChildren<DamageCollider>() == null)
        {
            Destroy(gameObject);
        }    // 관통옵션아닐 때 파괴

        //other.Layer가 Block일경우 Destoy해줘
        if (other.gameObject.layer == LayerMask.NameToLayer("Block"))
        {
            Destroy(gameObject);
            return;
        }
    }
}
