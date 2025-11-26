using System;
using UnityEngine;

public class FantasyButterfly : Interactable
{
    public float _mentalHealAmount;

    public override void Interact()
    {
        transform.tag = "Untagged";
        GetComponent<Collider>().enabled = false;


        // 현재 오브젝트와 자식들에 있는 모든 ParticleSystem 컴포넌트 가져오기
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();

        // 모든 ParticleSystem의 Loop 옵션을 false로 설정
        foreach (ParticleSystem ps in particleSystems)
        {
            var mainModule = ps.main;
            mainModule.loop = false;
            mainModule.simulationSpeed = 10f;
            
        }

        Managers.Object.GetPlayer().GetComponent<PlayerController>().Stats.AffectMental(_mentalHealAmount / 2);
        Destroy(gameObject,2f);
    }

    void Update()
    {
        // TODO
        //if (Managers.Game.DayTime == Define.DayTimeType.Night)
        //    Destroy(gameObject);
    }
}
