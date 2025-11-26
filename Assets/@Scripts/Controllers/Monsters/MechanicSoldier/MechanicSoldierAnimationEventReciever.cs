using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using RPG_Indicator;

public class MechanicSoldierAnimationEventReciever : MonsterAnimationEventReciever
{



    /*public void SpawnThuner()
    {
        StartCoroutine(SpawnThunderCoroutine());

    }
    private IEnumerator SpawnThunderCoroutine()
    {
        //Thunder Effect를 생성할지점 List에 저장
        //생성 되는 지점의 범위는 _monster.transform.posi
        //List에 들어있는 각 Position들에 0.1~0.3초 간격으로 Effect 생성
    }*/
    public void SpawnThuner()
    {
        StartCoroutine(SpawnThunderCoroutine());
        Managers.Sound.Play3DSound(gameObject, "CoreKeeper/MechanicSoldier/MechanicSoldierThunderAttack", 0.0f, 54.0f);
    }

    /*private IEnumerator SpawnThunderCoroutine()
    {
        int thunderCount = 10; // 생성 개수 (필요 시 변수화 가능)
        float areaRadius = 10f; // 생성 범위 반지름 (필요 시 변수화 가능)
        float indicatorDuration = .5f; // 인디케이터 유지 시간

        List<Vector3> spawnPositions = new List<Vector3>();

        Vector3 playerPosition = _monster.MainTarget.position;

        // 번개 스폰 지점 선정
        for (int i = 0; i < thunderCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * areaRadius;
            Vector3 position = playerPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
            spawnPositions.Add(position);
        }

        // 번개 생성 루프
        foreach (var pos in spawnPositions)
        {
            // 인디케이터 생성
            GameObject indicator = Managers.Resource.Instantiate("Effects/CoreKeeper/Lightning/Indicator", pos, Quaternion.identity);

            // 인디케이터 유지
           yield return new WaitForSeconds(indicatorDuration);

            // 낙뢰 이펙트 생성
            Managers.Resource.Instantiate("Effects/CoreKeeper/Lightning/Strike", pos, Quaternion.identity);

            // 인디케이터 제거
            //GameObject.Destroy(indicator);

            // 다음 생성까지 랜덤 대기
            yield return new WaitForSeconds(Random.Range(0f, 0.05f));
        }
    }*/

    private IEnumerator SpawnThunderCoroutine()
    {
        int thunderCount = 20;
        float areaRadius = 15f;

        float minDistanceBetween = 3f; // 최소 거리 제한

        if (_monster.MainTarget == null)
            yield break;

        Vector3 playerPosition = _monster.MainTarget.position;
        List<Vector3> spawnPositions = new List<Vector3>();
        List<GameObject> indicators = new List<GameObject>();

        int attempts = 0;
        while (spawnPositions.Count < thunderCount && attempts < thunderCount * 10)
        {
            Vector3 position;

            // 50% 확률로 플레이어 위치
            if (Random.value < 0.5f)
            {
                position = playerPosition;
            }
            else
            {
                Vector2 randomCircle = Random.insideUnitCircle * areaRadius;
                position = playerPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
            }

            // 겹침 방지
            bool tooClose = false;
            foreach (var existing in spawnPositions)
            {
                if (Vector3.Distance(existing, position) < minDistanceBetween)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                spawnPositions.Add(position);
            }

            attempts++;
        }

        foreach (var pos in spawnPositions)
        {
            StartCoroutine(SpawnThunder(pos));
            yield return new WaitForSeconds(Random.Range(0f, 0.2f)); // 랜덤 간격
        }

    }

    IEnumerator SpawnThunder(Vector3 pos)
    {
        float indicatorDuration = 1f;

        GameObject indicator = Managers.Resource.Instantiate("Effects/CoreKeeper/Lightning/Indicator", pos, Quaternion.identity);
        GameObject.Destroy(indicator, 2f);  // 인디케이터 유지 시간만큼

        yield return new WaitForSeconds(indicatorDuration);
        GameObject strike = Managers.Resource.Instantiate("Effects/CoreKeeper/Lightning/Strike", pos, Quaternion.identity);
        strike.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.5f);
        GameObject.Destroy(strike, 2f); // 2초 뒤 제거

    }

    public void DashStartCoroutin()
    {
        _monster.Nma.isStopped = false;
        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        Vector3 dashStartPosition = _monster.transform.position;
        Vector3 dashDirection = (PlayerPos - _monster.transform.position).normalized;
        dashDirection.y = 0;
        float dashDistance = 8f;
        float elapsedTime = 0f; // 경과 시간 체크
        _monster.Nma.velocity = dashDirection * 30f;

        _monster.transform.rotation = Quaternion.LookRotation(dashDirection);

        while (true)
        {
            _monster.Nma.velocity = dashDirection * 30f;
            elapsedTime += Time.deltaTime; // 시간 증가

            RaycastHit hit;
            if (Physics.Raycast(_monster.transform.position + Vector3.up, dashDirection, out hit, 3f, _monster.obstacleLayer))
            {
                break;
            }

            // NavMesh 상에서 이동 가능 여부 확인
            NavMeshHit navHit;
            if (!NavMesh.SamplePosition(_monster.transform.position + dashDirection * 3f * Time.deltaTime, out navHit, 0.5f, NavMesh.AllAreas))
            {
                break;
            }

            if (Physics.Raycast(_monster.transform.position + Vector3.up, dashDirection, out hit, 3f))
            {
                NavMeshObstacle obstacle = hit.collider.GetComponent<NavMeshObstacle>();
                if (obstacle != null)
                {
                    break;
                }
            }

            if (Vector3.Distance(dashStartPosition, _monster.transform.position) > dashDistance)
            {
                break;
            }
            //1초이상 지났는데도 안풀렸을 경우 break;
            if (elapsedTime > 1f)
            {
                break;
            }

            yield return null;
        }
    }

    private Vector3 PlayerPos;
    public void CalculatePlayer()
    {
        PlayerPos = _monster.MainTarget.position;
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
        GameObject a = Managers.Resource.Instantiate("Effects/PlantMonster/SpitAttackReady", _monster.transform.position, _monster.transform.rotation);

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
    public void DashAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/CoreKeeper/Lightning/Smash", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.5f);
        a.transform.SetParent(_monster.transform);
        GameObject.Destroy(a, 2f); // 2초 뒤 제거

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

    public void HealEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/PlantMonster/HealAttack", _monster.transform.position, Quaternion.identity);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.2f);
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
        MechanicSoldierController monster = _monster as MechanicSoldierController;
        Managers.Sound.Stop3DLoop(monster._plasmaLoopSound);
    }
    public RpgIndicator MonsterIndicator;

    public void Indicator()
    {
        if (MonsterIndicator != null)
        {
            MonsterIndicator.ShowCone(45, 6, false, RpgIndicator.IndicatorColor.Enemy, 0);
            MonsterIndicator.Casting(.6f);  // 1.5초 동안 채워지고 끝나면 사라짐        }
        }

    }
    public AudioClip[] footstepSounds; // 인스펙터에서 넣어줄 사운드 배열

    public void PlayRandomFootstepSound(AudioClip clip)
    {
        if (footstepSounds == null || footstepSounds.Length == 0)
        {
            Debug.LogWarning("Footstep sounds not assigned.");
            return;
        }

        int randomIndex = Random.Range(0, footstepSounds.Length);
        AudioClip selectedClip = footstepSounds[randomIndex];
        Managers.Sound.PlayRandomized3DSound(gameObject, clip, 0.0f, 54.0f);
    }



}
