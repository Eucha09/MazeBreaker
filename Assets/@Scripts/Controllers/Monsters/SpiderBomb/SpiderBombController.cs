using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class SpiderBombController : MonsterController2
{
    


    protected override void Start()
    {
        base.Start();

        //초기 상태
        Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform).SetInfo(this, 1.2f);

        CurrentState = new Monster.Search.Idle();

        //패턴 데이터
        _patterns.Add(new SpiderBombPatternsInfo.BombPatternInfo(this, 1f));
        _patterns.Add(new SpiderBombPatternsInfo.WalkPatternInfo(this));
    }


    /*
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        //만약 MainTarget이 Null이 아니면서 거리가 일정이하 가까워졌다면 코루틴실행 && 이미 코루틴이 실행됐으면 다시 안실행 되게 설정
        if (MainTarget != null && pulseCoroutine == null)
        {
            if(Vector3.Distance(MainTarget.transform.position, transform.position) < bombDist)
                pulseCoroutine = StartCoroutine(PulseAndExplode());
        }
    }
    */

    private void OnTriggerEnter(Collider other)
    {
        CurrentState.HandleOnTriggerEnter(other);
    }

    [Header("Bomb Setting")]
    public float bombDist = 3f;  //폭발 시작하기까지의 거리
    public float duration = 2f; // 총 지속 시간, 2초뒤에 터진다
    public float maxScale = 2f; // 최대 크기
    public float minScale = 0.5f; // 최소 크기
    public GameObject explosionEffect; // 터질 때 사용할 이펙트 (파티클 등)
    public Color pulseColor = Color.red; // 번쩍거릴 색상
    public Color defaultColor = Color.white; // 기본 색상
    private Coroutine pulseCoroutine = null;

    IEnumerator PulseAndExplode()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 남은 시간 계산
            float remainingTime = duration - elapsedTime;

            // 진동 주기: 시간이 적을수록 빠르게 진동
            float pulseSpeed = Mathf.Lerp(1f, 20f, elapsedTime / duration); // 1~20Hz
            float scaleFactor = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(elapsedTime * pulseSpeed * Mathf.PI) + 1) / 2);

            // 오브젝트 크기 조정
            transform.localScale = Vector3.one * scaleFactor;

            // 색상 변화
                float colorLerp = Mathf.PingPong(elapsedTime * pulseSpeed, 1);
                foreach (Material mat in _baseMat)
                {
                    mat.color = Color.Lerp(defaultColor, pulseColor, colorLerp);
                    // Emission을 번쩍거리게 만들기 (HDR 색상 적용)
                    mat.SetColor("_EmissionColor", Color.Lerp(defaultColor, pulseColor, colorLerp) * 10f);
                }


            // 경과 시간 업데이트
            elapsedTime += Time.deltaTime;

            yield return null; // 다음 프레임까지 대기
        }

        // 터지는 효과
        Explode();
    }

    void Explode()
    {
        // 터질 때 파티클 효과 생성
        if (explosionEffect != null)
        {
            if (Hp <= 0)
                return;
            GameObject a = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            a.GetComponentInChildren<DefaultDamageCollider>().Init(this, 0.1f);
        }

        // 자신을 삭제
        Destroy(gameObject);
    }

}
