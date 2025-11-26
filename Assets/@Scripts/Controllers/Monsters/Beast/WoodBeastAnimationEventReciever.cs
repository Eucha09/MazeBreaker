using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

//Beast 애니메이션에서 일어나는 모든 Event들을 받는다.
public class WoodBeastAnimationEventReciever : MonsterAnimationEventReciever
{
    public Transform rhandPos;
    public Transform lhandPos;


    public void CameraShakeshort(float intensity)
    {
        CinemachineShake.Instance.ShakeCamera(intensity, .15f);
    }

    public void CameraShakelong(float intensity)
    {
        CinemachineShake.Instance.ShakeCamera(intensity, 1f);
    }

    public void AddForce(float force)
    {
        _monster.Rb.AddForce(_monster.transform.forward * force, ForceMode.Impulse);
    }

    public void JumpStart(float time)
    {

        // 목표 위치와 현재 위치 사이의 거리를 계산합니다.
        StartCoroutine(MoveToTarget(_monster.transform, _monster.MainTarget.position, time));

        Managers.Resource.Instantiate("Effects/JumpIndicate", _monster.MainTarget.position, Quaternion.identity);
        Managers.Resource.Instantiate("Effects/JumpStart", _monster.transform.position, _monster.transform.rotation);


        // Velocity = 거리 / 시간
        //float velocity = distance / time;

        //jumpAttack.JumpVelocity =  velocity;
    }

    public void JumpEnd()
    {
        WoodBeast.Attack.JumpAttack jumpAttack = _monster.CurrentState as WoodBeast.Attack.JumpAttack;
    }

    public IEnumerator MoveToTarget(Transform MainTarget, Vector3 destination, float duration)
    {
        // 시작 시간과 시작 위치를 기록
        float startTime = Time.time;
        Vector3 startPosition = MainTarget.position;

        // 목표 위치까지의 총 거리를 계산
        float distance = Vector3.Distance(startPosition, destination);

        // 일정 시간동안 이동
        while (Time.time < startTime + duration)
        {
            // 경과 시간 비율을 계산 (0에서 1 사이의 값)
            float elapsed = (Time.time - startTime) / duration;

            // 경과 시간 비율에 따라 위치를 선형적으로 보간
            MainTarget.position = Vector3.Lerp(startPosition, destination, elapsed);


            RaycastHit hit;
            if (Physics.Raycast(_monster.transform.position + Vector3.up, _monster.transform.forward, out hit, 3f, _monster.obstacleLayer))
            {
                break;
            }

            // NavMesh 상에서 이동 가능 여부 확인
            NavMeshHit navHit;
            if (!NavMesh.SamplePosition(_monster.transform.position + _monster.transform.forward * 3f * Time.deltaTime, out navHit, 0.5f, NavMesh.AllAreas))
            {
                break;
            }

            if (Physics.Raycast(_monster.transform.position + Vector3.up, _monster.transform.forward, out hit, 3f))
            {
                NavMeshObstacle obstacle = hit.collider.GetComponent<NavMeshObstacle>();
                if (obstacle != null)
                {
                    break;
                }
            }

            // 다음 프레임까지 대기
            yield return null;
        }

        // 정확히 목표 위치에 도착하도록 설정
        if (Time.time - startTime >= duration)
            MainTarget.position = destination;
    }



    public void DoubleSwingEffect()
    {
        //GameObject a = Managers.Resource.Instantiate("Effects/BeastAttack", _beastController.transform.position, _beastController.transform.rotation);
        GameObject a = Managers.Resource.Instantiate("Effects/BeastAttack", _monster.transform.position, _monster.transform.rotation);
        a.transform.SetParent(_monster.transform);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }
    public void LeftSwingEffect()
    {
        //GameObject a = Managers.Resource.Instantiate("Effects/BeastAttack_L", _beastController.transform.position, _beastController.transform.rotation);
        GameObject a = Managers.Resource.Instantiate("Effects/BeastAttack_L", _monster.transform.position, _monster.transform.rotation);
        a.transform.SetParent(_monster.transform);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }

    public void RightSwingEffect()
    {
        //GameObject a = Managers.Resource.Instantiate("Effects/BeastAttack_R", _beastController.transform.position, _beastController.transform.rotation);
        GameObject a = Managers.Resource.Instantiate("Effects/BeastAttack_R", _monster.transform.position, _monster.transform.rotation);
        a.transform.SetParent(_monster.transform);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }
    public void JumpAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/BeastJumpAttack", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }
    public void ThrowRockEffect()
    {
        Vector2 randomPos = Random.insideUnitCircle * 1.5f;
        Vector3 spawnPosition = _monster.MainTarget.position + new Vector3(randomPos.x, 0, randomPos.y);

        GameObject a = Managers.Resource.Instantiate("Effects/ThrowRockAttack", new Vector3(_monster.transform.position.x, _monster.transform.position.y + 1, _monster.transform.position.z) , _monster.transform.rotation);
        a.GetComponentInChildren<ProjectileDamageCollider>().Init(_monster);
        Vector3 direction = spawnPosition - a.transform.position;
        direction.y = 0;
        a.GetComponentInChildren<Rigidbody>().AddForce(direction.normalized * 30f, ForceMode.Impulse);
    }
    public void ThrowRockReadyEffect_R(float offset)
    {
        GameObject a = Managers.Resource.Instantiate("Effects/ThrowRockReady", new Vector3(rhandPos.position.x,
            rhandPos.position.y + offset, rhandPos.position.z), _monster.transform.rotation);
    }
    public void ThrowRockReadyEffect_L(float offset)
    {
        GameObject a = Managers.Resource.Instantiate("Effects/ThrowRockReady", new Vector3(lhandPos.position.x,
            lhandPos.position.y + offset, lhandPos.position.z), _monster.transform.rotation);
    }
    public void LeaveStormAttackEffect()
    {
        // 플레이어 주변 반경 2 이내의 랜덤 위치 계산
        Vector2 randomPos = Random.insideUnitCircle * 3f;
        Vector3 spawnPosition = _monster.MainTarget.position + new Vector3(randomPos.x,0, randomPos.y);
        // 목표 위치와 현재 위치 사이의 거리를 계산합니다.
        Managers.Resource.Instantiate("Effects/LeaveIndicate", spawnPosition, Quaternion.identity);

        // 1초 뒤에 LeaveStormAttack 효과를 생성하는 코루틴 시작
        StartCoroutine(DelayedLeaveStormAttack(spawnPosition));
    }

    private IEnumerator DelayedLeaveStormAttack(Vector3 position)
    {
        // 1초 대기
        yield return new WaitForSeconds(1.0f);

        // LeaveStormAttack 효과를 소환하고 초기화
        GameObject a = Managers.Resource.Instantiate("Effects/LeaveStormAttack", position, Quaternion.identity);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 1000f);
        //a.GetComponentInChildren<DamageOverTimeDamageCollider>().Init(_beastController, 10,0.3f);
       // a.GetComponentInChildren<DamageOverTimeDamageCollider>().DamageOverTimeStart();
    }


    public void GasiGalleAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/SplinterGenerator", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<SplinterGenerator>().Caster = _monster;
    }
    public void HandChargeEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/HandCharge", _monster.transform.position, _monster.transform.rotation);
        //a.GetComponentInChildren<SkillCollider>().Init(_beastController);
    }

    public void BeastSound (AudioClip clip)
    {
        Managers.Sound.Play3DSound(gameObject, clip, 0.0f, 54.0f);
    }
    public void MoveForward()
    {
        StartCoroutine(MoveForwardRoutine());
    }
    public IEnumerator MoveForwardRoutine()
    {
        Vector3 hitDirection = transform.forward; ;
        float power;
        float duration;

        _monster.Nma.isStopped = true;        // 경로 이동 잠시 정지
        _monster.Nma.ResetPath();             // 현재 경로 취소 (SetDestination 멈춤)

        power = 15;
        duration = 0.25f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 방향으로 이동 (NavMeshAgent가 직접 위치 갱신)
            _monster.Nma.Move(hitDirection.normalized * power * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _monster.Nma.isStopped = false;       // 다시 경로 따라가기 재개
    }

    public IEnumerator SpecialAttackRoutine(float speed, float duration)
    {
        _monster.Rb.isKinematic = false;

        // 현재 속도 저장
        Vector3 originalVelocity = _monster.Rb.linearVelocity;

        // 전진 속도 설정
        _monster.Rb.linearVelocity = _monster.transform.forward * speed;

        // 지속 시간 대기
        yield return new WaitForSeconds(duration);

        // 속도 초기화
        _monster.Rb.linearVelocity = originalVelocity;

        _monster.Rb.isKinematic = true;
    }
}
