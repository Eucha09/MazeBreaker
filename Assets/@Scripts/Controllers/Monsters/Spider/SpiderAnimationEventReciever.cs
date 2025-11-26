using System.IO;
using UnityEngine;
using RPG_Indicator;

public class SpiderAnimationEventReciever : MonsterAnimationEventReciever
{
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
    public void StebAttackEffect()
    {
        //GameObject a = Managers.Resource.Instantiate("Effects/BeastAttack", _beastController.transform.position, _beastController.transform.rotation);
        GameObject a = Managers.Resource.Instantiate("Effects/SpiderQueen/StebAttack", _monster.transform.position, _monster.transform.rotation);
        a.transform.SetParent(_monster.transform);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }
    public void SlashAttackEffect()
    {
        //GameObject a = Managers.Resource.Instantiate("Effects/BeastAttack", _beastController.transform.position, _beastController.transform.rotation);
        GameObject a = Managers.Resource.Instantiate("Effects/SpiderQueen/SlashAttack", _monster.transform.position, _monster.transform.rotation);
        a.transform.SetParent(_monster.transform);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }

    private Vector3 PlayerPos;

    public void CalculatePlayer()
    {
        PlayerPos = _monster.MainTarget.position;

    }
    public void WebAttackEffect()
    {

        GameObject a = Managers.Resource.Instantiate("Effects/SpiderQueen/WebAttack", _monster.transform.position, Quaternion.LookRotation(PlayerPos - _monster.transform.position));
        a.GetComponentInChildren<ProjectileDamageCollider>().Init(_monster);
        Vector3 direction = PlayerPos - a.transform.position;
        direction.y = 0;
        a.GetComponentInChildren<Rigidbody>().AddForce(direction.normalized * 25f, ForceMode.Impulse);
    }
    public void PoisonWaveAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/SpiderQueen/PoisonWaveAttack", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 1000f);
        a.GetComponentInChildren<MovingEffectSpawner>()._monster = _monster;
    }



    public GameObject EggPrefab;

    public void LayEgg()
    {
        SpiderAttackPatterns.LayEggPattern.LayEgg layEggState = _monster.CurrentState as SpiderAttackPatterns.LayEggPattern.LayEgg;
        GameObject a = Instantiate(EggPrefab, _monster.transform.position, Quaternion.identity);
        a.GetComponent<MonsterController2>().Parent = _monster;
        switch (layEggState.GetEggData().spiderType)
        {
            case SpiderMeleeController.AttackType.Ranger:
                a.GetComponent<SpiderEggController>()._spider = Managers.Resource.Load<GameObject>($"Prefabs/Creature/Monster/SpiderRanger");
                break;

            case SpiderMeleeController.AttackType.Melee:
                a.GetComponent<SpiderEggController>()._spider = Managers.Resource.Load<GameObject>($"Prefabs/Creature/Monster/SpiderMelee");
                break;

            case SpiderMeleeController.AttackType.Bomb:
                a.GetComponent<SpiderEggController>()._spider = Managers.Resource.Load<GameObject>($"Prefabs/Creature/Monster/SpiderBomb");
                break;
        }
        
    }
    public void SpiderQueenSound(AudioClip clip)
    {
        Managers.Sound.Play3DSound(gameObject, clip, 0.0f, 54.0f);
    }

}
