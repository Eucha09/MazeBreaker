using UnityEngine;
using System.Collections;

public class DamageOverTimeDamageCollider : DamageCollider
{

    int _tickCount;
    float _interval;

    public void Init(BaseController caster, int tickCount, float interval)
    {
        _caster = caster;
        _tickCount = tickCount;
        _interval = interval;
    }

    public void DamageOverTimeStart()
    {
        StartCoroutine(ToggleCollider(_tickCount, _interval));
    }


    private IEnumerator ToggleCollider(int tickCount, float interval)
    {
        int cnt = 0;

        while (cnt < tickCount)
        {
            // 콜라이더 활성화
            GetComponent<Collider>().enabled = true;

            // 0.1초 동안 활성화
            yield return new WaitForFixedUpdate();

            // 콜라이더 비활성화
            GetComponent<Collider>().enabled = false;

            // b초 간격 대기
            yield return new WaitForSeconds(interval);

            cnt++;
            _damagedTargets.Clear();
            _playedSound.Clear();
        }
    }
}
