using RPG_Indicator;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonkeyMinionAnimationEventReciever : MonsterAnimationEventReciever
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
    public void SpitReadyEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/PlantMonster/SpitAttackReady", _monster.transform.position , _monster.transform.rotation);

    }
    public void SpitAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/PlantMonster/SpitAttack", _monster.transform.position, _monster.transform.rotation);
        //a.GetComponentInChildren<DamageOverTimeDamageCollider>().Init(_monster, 7, 0.1f);
        //a.GetComponentInChildren<DamageOverTimeDamageCollider>().DamageOverTimeStart();
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 1f);

        Animator meshAnimator = a.GetComponentInChildren<Animator>();
        if (meshAnimator != null)
        {

            meshAnimator.Play("SpitAttackOn");  // 커지는 애니메이션 실행
        }
        else
            Debug.Log("애니메이터 못찾음");
    }


    public void GroundAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/BearMinion/GroundAttack", _monster.transform.position + _monster.transform.forward * 1.7f, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }
    public void LineAttackEffect()
    {
        //GameObject a = Managers.Resource.Instantiate("Effects/BeastAttack", _beastController.transform.position, _beastController.transform.rotation);
        GameObject a = Managers.Resource.Instantiate("Effects/StoneMinion/LineAttack", _monster.transform.position, _monster.transform.rotation);
        //a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }
    public void SpawnLineAttack()
    {
        StartCoroutine(SpawnLineAttackCoroutine());
    }

    private IEnumerator SpawnLineAttackCoroutine()
    {
        if (_monster == null)
            yield break;

        Vector3 origin = _monster.transform.position + _monster.transform.forward * 2.5f;
        Vector3 forward = _monster.transform.forward.normalized;

        int count = 5;
        float spacing = 3f;
        float interval = 0.15f;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = origin + forward * spacing * i;

            // 인디케이터 + 딜레이 후 돌 생성은 별도 코루틴에서 처리
            StartCoroutine(SpawnSingleLineAttack(spawnPos, _monster.transform.rotation, i * interval));

            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator SpawnSingleLineAttack(Vector3 pos, Quaternion rot, float delay)
    {
        // 인디케이터 먼저 생성
        Managers.Resource.Instantiate("Effects/StoneMinion/LineAttackIndicator", pos, rot);

        // 0.7초 후 돌 생성
        yield return new WaitForSeconds(0.7f);

        GameObject a = Managers.Resource.Instantiate("Effects/StoneMinion/LineAttack", pos, rot);
        // AudioSource가 있다면 pitch를 랜덤하게 설정
        AudioSource audio = a.GetComponentInChildren<AudioSource>();
        if (audio != null)
        {
            audio.pitch = Random.Range(0.9f, 1.2f);  // 예시로 0.9~1.2 사이의 피치 변화
        }
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }




    public void SlashEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/BearMinion/Slash", _monster.transform.position, Quaternion.identity);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster,0.1f);
    }

    public void BiteAttackEffect()
    {
        //GameObject a = Managers.Resource.Instantiate("Effects/BeastAttack", _beastController.transform.position, _beastController.transform.rotation);
        GameObject a = Managers.Resource.Instantiate("Effects/BearMinion/Slash", _monster.transform.position + _monster.transform.forward * 1.5f, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }
    public void BurrowDownEffect()
    {
        Managers.Resource.Instantiate("Effects/StoneMinion/BurrowDown", _monster.transform.position, _monster.transform.rotation);
    }
    public void BurrowMoveCrackEffect()
    {
        Managers.Resource.Instantiate("Effects/StoneMinion/BurrowMoveCrack", _monster.transform.position + new Vector3(0,1,0), _monster.transform.rotation);
    }
    private Vector3 PlayerPos;
    public void CalculatePlayer()
    {
        PlayerPos = _monster.MainTarget.position;

    }
    public void PoisonProjectileAttackEffect()
    {

        Debug.Log("이펙트 호출~~");
        GameObject a = Managers.Resource.Instantiate("Effects/MonkeyMinion/ThrowAttack", _monster.transform.position, Quaternion.LookRotation(PlayerPos - _monster.transform.position));
        a.GetComponentInChildren<ProjectileDamageCollider>().Init(_monster);
        Vector3 direction = PlayerPos - a.transform.position;
        direction.y = 0;
        a.GetComponentInChildren<Rigidbody>().AddForce(direction.normalized * 25f, ForceMode.Impulse);
    }

    public void SpiderQueenSound(AudioClip clip)
    {
        Managers.Sound.Play3DSound(gameObject, clip, 0.0f, 54.0f);
    }
    public AudioClip[] DeadSound; // 유니티에서 두 개 이상 넣을 수 있음
    public AudioClip[] HurtSound;
    [Range(0f, 1f)]
    public float playChance = 0.5f; // 0.5 = 50% 확률로 재생
    public void DeadRandomSound()
    {
        if (DeadSound == null || DeadSound.Length == 0)
            return;

        int randomIndex = Random.Range(0, DeadSound.Length);
        AudioClip selectedClip = DeadSound[randomIndex];
        Managers.Sound.Play3DSound(gameObject, selectedClip, 0.0f, 54.0f);
    }
    public void HurtRandomSound()
    {
        // 확률 체크 먼저
        if (Random.value > playChance)
        {
            // 확률에 따라 사운드 재생하지 않음
            return;
        }

        // 사운드가 없으면 탈출
        if (HurtSound == null || HurtSound.Length == 0)
            return;

        // 사운드 하나 랜덤 선택
        int index = Random.Range(0, HurtSound.Length);
        AudioClip selectedClip = HurtSound[index];

        Managers.Sound.Play3DSound(gameObject, selectedClip, 0.0f, 54.0f);
    }


}
