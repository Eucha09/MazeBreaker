using System;
using UnityEngine;

public class ArrowLineDrawer : MonoBehaviour
{
    private LineRenderer lineRenderer;

    void Start()
    {
        // LineRenderer 컴포넌트를 가져옴
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer가 없습니다! LineRenderer 컴포넌트를 추가하세요.");
            return;
        }
    }


    /// <summary>
    /// LineRenderer로 시작 지점과 끝 지점을 연결합니다.
    /// </summary>
    /// <param name="startPoint">시작 지점</param>
    /// <param name="endPoint">끝 지점</param>
    public void DrawLine(Vector3 startPoint, Vector3 direction, float length)
    {
        // LineRenderer의 점 개수 설정
        lineRenderer.positionCount = 2;

        // 시작 지점 설정
        lineRenderer.SetPosition(0, startPoint);

        // RaycastHit 정보 저장 변수
        RaycastHit hit;

        // Raycast를 쏴서 충돌체가 있는지 확인
        if (Physics.Raycast(new Vector3(0,1,0)+startPoint, direction, out hit, length))
        {
            // 충돌 지점까지 선을 그립니다.
            lineRenderer.SetPosition(1, new Vector3(hit.point.x,startPoint.y,hit.point.z));
        }
        else
        {
            // 충돌이 없으면 원래의 끝 지점까지 선을 그립니다.
            Vector3 endPoint = startPoint + direction.normalized * length;
            lineRenderer.SetPosition(1, endPoint);
        }
    }
}
