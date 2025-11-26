using RPG_Indicator;
using UnityEngine;

public class PlantAnimationEventReciever : MonsterAnimationEventReciever
{
    public GameObject sectorIndicatorPrefab;
    public void ConeAttackIndicator()
    {

        if (_monster.MainTarget == null) return;

        Vector3 center = _monster.transform.position;
        Vector3 dir = (_monster.MainTarget.position - center).normalized;

        var go = Instantiate(sectorIndicatorPrefab);
        var indicator = go.GetComponent<SectorIndicatorNoShader>();
        _monster.currentIndicator = indicator; //창수(스턴 시 인디케이터 제거, 검토필요)

        indicator.Setup(center, dir, angle: 45f, inner: 0f, outer: 7.75f, duration: 1.0f);
        indicator.OnCharged = () =>
        {
            // 인디케이터가 다 차면 실제 공격 이펙트 & 데미지
            //GameObject atk = Managers.Resource.Instantiate("Effects/NightMare/WindSlash", center, Quaternion.LookRotation(dir));
            //atk.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.2f);
            // 🔥 인디케이터 자연스럽게 사라짐
            indicator.ActivateHitCollider(_monster, 0.1f);

            indicator.FadeAndDestroy(0.4f);
        };
        indicator.Play();
    }
    public void SetRushDirection()
    {
        SpiderAttackPatterns.RushPattern.Rush rush = _monster.CurrentState as SpiderAttackPatterns.RushPattern.Rush;
        rush.SetRushDirection();
    }

    public void RushStart()
    {
        SpiderAttackPatterns.RushPattern.Rush rush = _monster.CurrentState as SpiderAttackPatterns.RushPattern.Rush;
        rush.StartCorMoveInDistance();
    }
    public void SpitReadyEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/PlantMonster/SpitAttackReady", _monster.transform.position , _monster.transform.rotation);

    }
    public void SpitAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/PlantMonster/SpitAttack", _monster.transform.position, _monster.transform.rotation);
        //a.GetComponentInChildren<DamageOverTimeDamageCollider>().Init(_monster, 7, 0.1f);
        //a.GetComponentInChildren<DamageOverTimeDamageCollider>().DamageOverTimeStart();
        //a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 1f);

        Animator meshAnimator = a.GetComponentInChildren<Animator>();
        if (meshAnimator != null)
        {

            meshAnimator.Play("SpitAttackOn");  // 커지는 애니메이션 실행
        }
        else
            Debug.Log("애니메이터 못찾음");
    }


    public void BiteAttackEffect()
    {
        //GameObject a = Managers.Resource.Instantiate("Effects/BeastAttack", _beastController.transform.position, _beastController.transform.rotation);
        GameObject a = Managers.Resource.Instantiate("Effects/PlantMonster/BiteAttack", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }
    public void WebAttackEffect()
    {
        Vector3 spawnPosition = _monster.MainTarget.position;

        Debug.Log("이펙트 호출~~");
        GameObject a = Managers.Resource.Instantiate("Effects/SpiderQueen/WebAttack", _monster.transform.position, Quaternion.LookRotation(spawnPosition - _monster.transform.position));
        a.GetComponentInChildren<ProjectileDamageCollider>().Init(_monster);
        Vector3 direction = spawnPosition - a.transform.position;
        direction.y = 0;
        a.GetComponentInChildren<Rigidbody>().AddForce(direction.normalized * 25f, ForceMode.Impulse);
    }

    public void HealEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/PlantMonster/HealAttack", _monster.transform.position, Quaternion.identity);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster,0.2f);
    }

    private Vector3 PlayerPos;
    public void CalculatePlayer()
    {
        PlayerPos = _monster.MainTarget.position;

    }
    public void PoisonProjectileAttackEffect()
    {

        Debug.Log("이펙트 호출~~");
        GameObject a = Managers.Resource.Instantiate("Effects/SpiderMelee/PoisonProjectileAttack", new Vector3(_monster.transform.position.x, _monster.transform.position.y + 2, _monster.transform.position.z), Quaternion.LookRotation(PlayerPos - _monster.transform.position));
        a.GetComponentInChildren<ProjectileDamageCollider>().Init(_monster);
        Vector3 direction = PlayerPos - a.transform.position;
        direction.y = 0;
        a.GetComponentInChildren<Rigidbody>().AddForce(direction.normalized * 20f, ForceMode.Impulse);
    }

    public void SpiderQueenSound(AudioClip clip)
    {
        Managers.Sound.Play3DSound(gameObject, clip, 0.0f, 54.0f);
    }

    public override void Dissolve()
    {
        base.Dissolve();
    }

}
