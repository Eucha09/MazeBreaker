using UnityEngine;
using UnityEngine.Audio;
public class SpiderMeleeAnimationEventReciever : MonsterAnimationEventReciever
{

    public AudioClip[] clips;
    private int currentIndex = 0; // 현재 인덱스를 저장하는 필드

    public void SoundRandom()
    {
        if (clips.Length > 0)
        {
            // 현재 인덱스의 사운드를 선택
            AudioClip currentClip = clips[currentIndex];

            // 선택된 사운드 재생
            Managers.Sound.Play3DSound(gameObject, currentClip, 0.0f, 54.0f);

            // 인덱스를 다음으로 이동, 끝에 도달하면 0으로 초기화
            currentIndex = (currentIndex + 1) % clips.Length;
        }
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
    public void HitAttackEffect()
    {
        //GameObject a = Managers.Resource.Instantiate("Effects/BeastAttack", _beastController.transform.position, _beastController.transform.rotation);
        GameObject a = Managers.Resource.Instantiate("Effects/SpiderMelee/HitAttack", _monster.transform.position, _monster.transform.rotation);
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
        SpiderMeleeController a =  _monster as SpiderMeleeController;
        if (a.Parent != null)
        {
            SpiderController b = a.Parent as SpiderController;
            switch (a.type)
            {
                case SpiderMeleeController.AttackType.Melee:
                    b.MeleeSpiders.Remove(_monster.gameObject);
                    break;

                case SpiderMeleeController.AttackType.Ranger:
                    b.RangerSpiders.Remove(_monster.gameObject);
                    break;
            }
          //  b.currentRangerSpiderCount--;
        }
    }

}
