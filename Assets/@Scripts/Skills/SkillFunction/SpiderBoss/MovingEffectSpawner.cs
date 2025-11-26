using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MovingEffectSpawner : MonoBehaviour
{
    [Header("NavMeshAgent 이동 설정")]
    public float moveSpeed = 5f;             // 이동 속도
    public float lifeTime = 10f;             // 오브젝트 수명
    public float effectInterval = 2f;        // 이펙트 소환 간격

    [Header("이펙트 프리팹")]
    public GameObject effectPrefab;          // 소환할 이펙트 프리팹

    private NavMeshAgent _nma;               // NavMeshAgent 참조

    private void Start()
    {
        // NavMeshAgent 초기화
        _nma = GetComponent<NavMeshAgent>();

        // NavMeshAgent 설정 (경로 탐색 비활성화)
        _nma.updateRotation = false;
        _nma.updatePosition = true; // 직접 Position을 제어하기 위함
        _nma.speed = moveSpeed;
        StartCoroutine(EffectSpawningRoutine());

        // LifeTime이 지나면 오브젝트 삭제
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 오브젝트 이동: NavMeshAgent의 velocity를 앞 방향으로 설정
        Vector3 forwardVelocity = transform.forward * moveSpeed;
        _nma.velocity = forwardVelocity;

        // 자기 자신 위치의 바로 0.5f 앞을 검사
        Vector3 checkPosition = transform.position + transform.forward * 0.5f;

        // NavMesh 상에서 해당 지점이 유효한지 확인
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(checkPosition, out hit, 0.1f, NavMesh.AllAreas))
        {
            Debug.LogWarning("앞쪽 위치가 NavMesh에 없습니다! 오브젝트를 파괴합니다.");
            Destroy(gameObject);
        }
    }

    private IEnumerator EffectSpawningRoutine()
    {
        WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate(); // FixedUpdate 주기
        float elapsedTime = 0f; // 경과 시간 초기화

        while (true)
        {
            yield return fixedUpdate; // FixedUpdate 이후 실행
            elapsedTime += Time.fixedDeltaTime; // 고정 시간 누적

            if (elapsedTime >= effectInterval) // 지정된 시간 간격에 도달하면
            {
                SpawnEffect(); // 이펙트 생성
                elapsedTime = 0f; // 타이머 초기화
            }
        }
    }
    public MonsterController2 _monster;

    private void SpawnEffect()
    {
        if (effectPrefab != null)
        {
            GameObject a =Instantiate(effectPrefab, transform.position, transform.rotation); // 현재 위치에 이펙트 소환
            a.GetComponentInChildren<DamageOverTimeDamageCollider>().Init(_monster,60,0.1f); //0.1초마다 1
            a.GetComponentInChildren<DamageOverTimeDamageCollider>().DamageOverTimeStart();
            Managers.Sound.Play3DSound(a, "SkillSound/PoisonArea", 0.0f, 40f);
        }
    }
}