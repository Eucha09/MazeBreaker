using UnityEngine;
using System.Collections.Generic;

public class SpiderEggController : MonsterController2
{

    [Header("SpiderEggProperties")]
    public float _spawnDuration=10f;
    public GameObject _spawnedEffect;
    public GameObject _spawnSpiderEffect;
    public GameObject _spider = null;


    protected override void Start()
    {
        base.Start();
        Hp = 40;
        MaxHp = 40;
        //hpBar 추가
        Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform).SetInfo(this, 1.2f);
        //5초뒤에 알 부화 메서드 실행
        Invoke("SpawnSpider", _spawnDuration);
        Instantiate(_spawnedEffect,transform.position,Quaternion.identity);
        _ani.Play("Idle");
    }


    protected override void FixedUpdate()
    {
        if (Hp <= 0)
        {
            Instantiate(_spawnSpiderEffect, transform.position, Quaternion.identity);
            GameObject.Destroy(gameObject);
        }
    }

    public override void OnDamaged(DamageCollider dm)
    {
        DamageEffectStart();
        Hp -= dm.DamageCalculate(this);

    }

    private void SpawnSpider()
    {
        Ani.Play("Spawn");
    }
}
