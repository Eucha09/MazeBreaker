using UnityEngine;

public class SpiderEggAnimationEventReciever:MonsterAnimationEventReciever
{
    public void SpawnSpider2()
    {
        SpiderEggController controller = _monster as SpiderEggController;
        Instantiate(controller._spawnSpiderEffect, transform.position, Quaternion.identity);

        SpiderController spiderQueen =  controller.Parent as SpiderController;
        GameObject a = Instantiate(controller._spider, transform.position, Quaternion.identity);
        a.GetComponent<MonsterController2>().Parent = controller.Parent;
        a.GetComponent<MonsterController2>().Hp = a.GetComponent<MonsterController2>().MaxHp * (_monster.Hp / _monster.MaxHp);
        switch (controller._spider.GetComponent<SpiderMeleeController>().type)
        {
            case SpiderMeleeController.AttackType.Ranger:
                spiderQueen.RangerSpiders.Add(a);
                break;

            case SpiderMeleeController.AttackType.Melee:
                spiderQueen.MeleeSpiders.Add(a);
                break;

        }
    }

    public void DestroyThis()
    {
        GameObject.Destroy(gameObject);
    }
    public void EggSound(AudioClip clip)
    {
        Managers.Sound.Play3DSound(gameObject, clip, 0.0f, 54.0f);
    }
}
