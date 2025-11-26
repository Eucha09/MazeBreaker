using UnityEngine;

public class SpiderBombAnimationEventReciever : MonsterAnimationEventReciever
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
    public void Bomb()
    {
        SpiderBombController controller = _monster as SpiderBombController;
        GameObject a = Instantiate(controller.explosionEffect, transform.position, Quaternion.identity);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster,0.1f);
    }

    public void BombIndicatorEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/SpiderBomb/BombIndicator", _monster.transform.position, _monster.transform.rotation);
        a.transform.SetParent(_monster.transform);
    }
    public void SpiderQueenSound(AudioClip clip)
    {
        Managers.Sound.Play3DSound(gameObject, clip, 0.0f, 54.0f);
    }

}
