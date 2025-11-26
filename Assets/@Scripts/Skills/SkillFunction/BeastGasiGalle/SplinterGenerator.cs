using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SplinterGenerator : MonoBehaviour
{
    List<GameObject> generator = new List<GameObject>(); // 빈 리스트 초기화

    public int sphereCount = 3;              // 발사할 구체의 갯수
    public float angleBetweenSpheres = 15f;  // 구체 사이의 각도
    public float launchForce = 10f;          // 구체 발사 속도
    public float sphereDistance = 5f;        // 구체 제한 거리
    public float generateDuration = 0.4f;    // 생성 주기
    public int effectGenerateCount = 5;          // 이펙트 생성 갯수
    int currentEffectGenerateCount = 0;
    public float currentGenerateTime = 0f;   // 최근 생성 시간
    public float GenerateIntervalBetweenEffectAndIndicator = 0.3f; // 인디케이터 생성 후 실제 이펙트 생성 간격
    public string IndicatorPath;             // 인디케이터 경로
    public string SplinterPath;              // 이펙트 경로

    public BaseController Caster;

    private void Start()
    {
        currentEffectGenerateCount = 0;
        LaunchSpheres();
    }

    private void FixedUpdate()
    {
        if (generator.Count < 1)
            return;

        //보이지 않는 구체 이동
        foreach(GameObject b in generator)
        {
            b.GetComponent<NavMeshAgent>().velocity = b.transform.forward * launchForce;
        }

        //이펙트 생성
        if (Time.time - currentGenerateTime > generateDuration)
        {
            for (int i = generator.Count - 1; i >= 0; i--)
            {
                Debug.Log("코루틴 실행");
                StartCoroutine(GenerateSplinter(generator[i].transform.position));
            }
            currentEffectGenerateCount++;
            currentGenerateTime = Time.time;
        }

        //제거
        if(currentEffectGenerateCount > effectGenerateCount)
        {
            for (int i = generator.Count - 1; i >= 0; i--)
            {
                GameObject a = generator[i];

                GameObject.Destroy(a);
                generator.RemoveAt(i);  // 리스트에서 안전하게 제거
            }
        }
    }

    IEnumerator GenerateSplinter(Vector3 pos)
    {
        int cnt;
        cnt = currentEffectGenerateCount;
        //인디케이터 스케일 배율
        float indicatorScaleMagnification = 0.1f;
        GameObject a = Managers.Resource.Instantiate(IndicatorPath, pos, Quaternion.identity);
        a.transform.localScale = new Vector3(currentEffectGenerateCount * indicatorScaleMagnification + 1, currentEffectGenerateCount * indicatorScaleMagnification + 1, currentEffectGenerateCount * indicatorScaleMagnification + 1);

        yield return new WaitForSeconds(GenerateIntervalBetweenEffectAndIndicator);

        //이펙트 스케일 배율
        float effectScaleMagnification = 0.1f;
        GameObject b = Managers.Resource.Instantiate(SplinterPath, pos, Quaternion.identity);
        b.GetComponentInChildren<DefaultDamageCollider>().Init(Caster,0.1f);
        b.transform.localScale = new Vector3(cnt * effectScaleMagnification + 1, cnt * effectScaleMagnification + 1, cnt * effectScaleMagnification + 1);
    }

    private void LaunchSpheres()
    {
        float startAngle = -((sphereCount - 1) * angleBetweenSpheres) / 2f; // 구체들의 시작 각도 계산

        for (int i = 0; i < sphereCount; i++)
        {
            // 각 구체에 각도와 회전을 적용
            float angle = startAngle + (i * angleBetweenSpheres);
            Quaternion rotation = Quaternion.Euler(0, angle, 0) * transform.rotation;

            // 빈 오브젝트 생성 및 위치 설정
            GameObject sphere = new GameObject("Sphere_" + i);
            sphere.transform.position = transform.position;
            sphere.transform.rotation = rotation;
            generator.Add(sphere);

            // NavMeshAgent 추가
            NavMeshAgent agent = sphere.AddComponent<NavMeshAgent>();
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

            // NavMeshAgent 초기 설정
            agent.speed = launchForce;
            agent.angularSpeed = 0;  // 구체의 회전을 방지하여 직선 이동하게 설정
            agent.acceleration = launchForce;

            Vector3 launchDirection = rotation * Vector3.forward;  // 로컬 Z축 기준 전방
            agent.velocity = launchDirection * launchForce;        // 발사 방향과 힘 설정 
        }
    }

    // Scene 뷰에서 발사 경로를 시각화하는 Gizmos 추가
    private void OnDrawGizmos()
    {
        float startAngle = -((sphereCount - 1) * angleBetweenSpheres) / 2f; // 구체들의 시작 각도 계산

        for (int i = 0; i < sphereCount; i++)
        {
            // 각 구체의 발사 각도 및 방향 설정
            float angle = startAngle + (i * angleBetweenSpheres);
            Quaternion rotation = Quaternion.Euler(0, angle, 0) * transform.rotation;
            Vector3 launchDirection = rotation * Vector3.forward;

            // Gizmos로 발사 경로를 시각적으로 표시
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + launchDirection * sphereDistance);
            Gizmos.DrawSphere(transform.position + launchDirection * sphereDistance, 0.2f);
        }
    }
}
