using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static PlantController;


public class PlantController : MonsterController2
{
    public enum AttackType
    {
        Ranger,
        Melee,
        Healer
    }
    [Header("Plant Properties")]
    public AttackType type;



    [System.Serializable]
    public struct Ally
    {
        public GameObject ally;
        public float lastDetectionTime;
    }
    [Header("Healer Plant Properties")]
    //아군을 담고 있는 리스트
    public List<Ally> allies = new List<Ally>();

    public LayerMask allyLayer; // 아군이 속한 레이어
    public float detectionRange = 10f; // 탐지 범위
    public float allyRemoveTime = 10f; // 마지막 감지 후 삭제 시간

    private IEnumerator DetectAlliesCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // 1초마다 실행

            Collider[] detectedObjects = Physics.OverlapSphere(transform.position, detectionRange, allyLayer);

            foreach (Collider col in detectedObjects)
            {
                GameObject allyObject = col.gameObject;

                // 자신을 탐지하지 않도록 예외 처리
                if (allyObject == gameObject) continue;

                // Raycast로 장애물 확인 (나와 아군 사이에 장애물이 없어야 함)
                Vector3 direction = (allyObject.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, allyObject.transform.position);

                if (Physics.Raycast(transform.position, direction, distance, obstacleLayer))
                {
                    continue; // 장애물이 있으면 감지 안 함
                }

                // 기존 리스트에서 찾기
                int index = allies.FindIndex(a => a.ally == allyObject);

                if (index >= 0)
                {
                    // 기존 아군이면 시간 갱신
                    Ally updatedAlly = allies[index];
                    updatedAlly.lastDetectionTime = Time.time;
                    allies[index] = updatedAlly;
                }
                else
                {
                    // 새로운 아군 추가
                    allies.Add(new Ally { ally = allyObject, lastDetectionTime = Time.time });
                }
            }

            // **10초 지난 아군 & Missing된 오브젝트 제거**
            allies.RemoveAll(a => a.ally == null || Time.time - a.lastDetectionTime > allyRemoveTime);
        }
    }

    public GameObject GetClosestAlly()
    {
        if (allies.Count == 0) return null; // 아군이 없으면 null 반환

        return allies
            .Where(a => a.ally != null) // Missing된 오브젝트 제거
            .OrderBy(a => Vector3.Distance(transform.position, a.ally.transform.position)) // 거리순 정렬
            .FirstOrDefault().ally; // 가장 가까운 아군 반환
    }

    public GameObject GetLowestHpAlly()
    {
        if (allies.Count == 0) return null; // 아군이 없으면 null 반환

        return allies
            .Where(a => a.ally != null) // Missing된 오브젝트 제거
            .OrderBy(a => a.ally.GetComponent<BaseController>().Hp) // HP순 정렬
            .FirstOrDefault().ally; // 가장 가까운 아군 반환
    }



    protected override void Start()
    {
        base.Start();
        //초기 상태
        CurrentState = new Monster.Search.Idle();
        Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform).SetInfo(this, 1.8f);

        //패턴 데이터
        if (type == AttackType.Melee)
        {
            _patterns.Add(new PlantPatternsInfo.BitePatternInfo(this, 3f, 3f));
            _patterns.Add(new PlantPatternsInfo.WalkPatternInfo(this, 3f));
        }else if (type == AttackType.Ranger)
        {
            _patterns.Add(new PlantPatternsInfo.PoisonBulletPatternInfo(this, 8f,8f));
            _patterns.Add(new PlantPatternsInfo.OrbitMovePatternInfo(this));
        }else if (type == AttackType.Healer)
        {
            StartCoroutine(DetectAlliesCoroutine());
            _patterns.Add(new PlantPatternsInfo.MoveToAllyPatternInfo(this));
            _patterns.Add(new PlantPatternsInfo.HealPatternInfo(this));
            _patterns.Add(new PlantPatternsInfo.BitePatternInfo(this, 3f, 5f));
            _patterns.Add(new PlantPatternsInfo.RunAwayPatternInfo(this));

        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        StartCoroutine(DetectAlliesCoroutine());
    }

}
