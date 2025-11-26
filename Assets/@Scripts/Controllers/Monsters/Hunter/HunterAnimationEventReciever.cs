using UnityEngine;

public class HunterAnimationEventReciever : MonsterAnimationEventReciever
{
    public AudioClip[] footstepSounds; // 4개의 발소리 배열
    private int currentStepIndex = 0; // 현재 재생 중인 발소리 인덱스

    public void CameraShakeshort(float intensity)
    {
        CinemachineShake.Instance.ShakeCamera(intensity, .15f);
    }

    public void CameraShakelong(float intensity)
    {
        CinemachineShake.Instance.ShakeCamera(intensity, 1.5f);
    }

    public void PlayFootstepSound()
    {
        if (footstepSounds.Length == 0) return;

        // AudioManager를 통해 사운드 재생
        Managers.Sound.Play3DSound(gameObject, footstepSounds[currentStepIndex], 0f, 54f);

        // 다음 인덱스로 이동 (0→1→2→3→0)
        currentStepIndex = (currentStepIndex + 1) % footstepSounds.Length;
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
        a.GetComponentInChildren<DamageOverTimeDamageCollider>().Init(_monster, 7, 0.1f);
        a.GetComponentInChildren<DamageOverTimeDamageCollider>().DamageOverTimeStart();
        Animator meshAnimator = a.GetComponentInChildren<Animator>();
        if (meshAnimator != null)
        {

            meshAnimator.Play("SpitAttackOn");  // 커지는 애니메이션 실행
        }
        else
            Debug.Log("애니메이터 못찾음");
    }
    
    public void DustEffect()
    {
        if (_monster == null)
            return;
        currentStepIndex = 0; // 달리기 시작 시 인덱스 리셋
        GameObject a = Managers.Resource.Instantiate("Effects/Hunter/Dust", _monster.transform.position, _monster.transform.rotation);
    }
    public void Dashtowall()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/Hunter/Dashtowall", _monster.transform.position, _monster.transform.rotation);
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
    }

}
