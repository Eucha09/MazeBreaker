using UnityEngine;
using System.Collections;

public class DefaultDamageCollider : DamageCollider
{
    //스킬 콜라이더 유지 시간
    private float _skillMaintenanceTime;
    //스킬 생성 시간
    private float _skillSpawedTime;



    public void Init(BaseController caster, float skillMaintenanceTime)
    {
        _caster = caster;
        _skillMaintenanceTime=skillMaintenanceTime;
        _skillSpawedTime = Time.time;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public void FixedUpdate()
    {
        if (Time.time - _skillSpawedTime > _skillMaintenanceTime)
            gameObject.GetComponent<Collider>().enabled = false;
    }
}
