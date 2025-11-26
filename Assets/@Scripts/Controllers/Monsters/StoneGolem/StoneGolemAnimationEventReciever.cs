using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using RPG_Indicator;
using System.Linq;

public class StoneGolemAnimationEventReciever : MonsterAnimationEventReciever
{
    public Vector3 WallCenterPosition { get; private set; }

    public void CameraShakeshort(float intensity)
    {
        CinemachineShake.Instance.ShakeCamera(intensity, .15f);
    }

    public void CameraShakelong(float intensity)
    {
        CinemachineShake.Instance.ShakeCamera(intensity, 1f);
    }
    public void MoveForward()
    {
        StartCoroutine(SpecialAttackRoutine(25f, 0.35f));
    }

    public IEnumerator SpecialAttackRoutine(float speed, float duration)
    {
        // 현재 속도 저장
        Vector3 originalVelocity = _monster.Rb.linearVelocity;

        // 전진 속도 설정
        _monster.Rb.linearVelocity = _monster.transform.forward * speed;

        // 지속 시간 대기
        yield return new WaitForSeconds(duration);

        // 속도 초기화
        _monster.Rb.linearVelocity = originalVelocity;
    }
    /* 스킬1 돌 아래서 위로 소환 (현재 폐기)--------------------------------------------------------
    public void StartStoneUp()// 스킬1 돌 아래서 위로 소환 (현재 폐기)--------------------------------------------------------
    {
        StartCoroutine(StartStoneUpCoroutine());
    }

    private IEnumerator StartStoneUpCoroutine()
    {
        int count = 5;
        float interval = .5f;
        float spawnRadius = 3f;
        float minDistanceBetween = 1.5f;

        List<Vector3> spawnPositions = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            if (_monster.MainTarget == null)
                yield break;

            Vector3 targetPos;

            if (i == 0)
            {
                // 첫 번째는 정중앙 타격
                targetPos = _monster.MainTarget.position;
            }
            else
            {
                Vector3 center = _monster.MainTarget.position;
                Vector3 candidate;
                int attempts = 0;

                do
                {
                    Vector2 offset = Random.insideUnitCircle * spawnRadius;
                    candidate = new Vector3(center.x + offset.x, center.y, center.z + offset.y);

                    // 겹침 방지
                    bool tooClose = spawnPositions.Any(pos => Vector3.Distance(pos, candidate) < minDistanceBetween);
                    if (!tooClose)
                        break;

                    attempts++;
                } while (attempts < 10);

                targetPos = candidate;
            }

            spawnPositions.Add(targetPos);
            SpawnStoneAt(targetPos);
            yield return new WaitForSeconds(interval);
        }
    }
    private void SpawnStoneAt(Vector3 pos)
    {
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/Indicator", pos, Quaternion.identity);
        GameObject.Destroy(a, 1.5f);

        StartCoroutine(SpawnStoneStrikeAfterDelay(pos, .8f));
    }

    private IEnumerator SpawnStoneStrikeAfterDelay(Vector3 pos, float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/StoneUp", pos, Quaternion.identity);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.5f);
        GameObject.Destroy(a, 2f);
    }
    */

    //스킬2 돌 케이지--------------------------------------------------------------------

    /*public void SpawnStoneWall()  이펙트로 제어
        {
            if (_monster.MainTarget == null)
                return;

            // ✅ 당신이 조절할 변수들
            int rockCount = 13;
            float playerToWallDistance = .1f; // 플레이어 뒤로 얼마나 떨어질지
            float spacing = 2.2f;              // 돌 간격
            float arcAngle = 240f;             // 부채꼴 각도

            // 1. 플레이어 → 보스 방향
            Vector3 dirToBoss = (_monster.transform.position - _monster.MainTarget.position).normalized;
            Vector3 dirToWall = -dirToBoss;

            // 2. 벽 중심 위치: 플레이어 뒤쪽
            Vector3 wallCenter = _monster.MainTarget.position + dirToWall * playerToWallDistance;

            // 3. 돌 배치 반지름 계산
            float totalArcLength = spacing * (rockCount - 1);
            float arcRadians = arcAngle * Mathf.Deg2Rad;
            float actualRadius = totalArcLength / arcRadians;

            float startAngle = -arcAngle * 0.5f;

            for (int i = 0; i < rockCount; i++)
            {
                float t = rockCount == 1 ? 0.5f : (float)i / (rockCount - 1);
                float angle = startAngle + arcAngle * t;
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
                Vector3 offsetDir = rot * dirToWall;

                Vector3 spawnPos = wallCenter + offsetDir * actualRadius;

                GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/StoneWall", spawnPos, Quaternion.LookRotation(-dirToWall));

                float randomScale = Random.Range(0.9f, 1.2f);
                a.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
                a.transform.Rotate(0, Random.Range(-30f, 30f), 0);

                GameObject.Destroy(a, 3f);
            }
        }*/

    public void SpawnStoneWall()
    {
        if (_monster.MainTarget == null)
            return;

        // ✅ 당신이 조절할 변수들
        int rockCount = 13;
        float playerToWallDistance = 0.1f;     // 플레이어 뒤로 얼마나 떨어질지
        float spacing = 1.7f;                  // 돌 간격 2,2
        float arcAngle = 240f;                 // 부채꼴 각도

        // 1. 플레이어 → 보스 방향
        Vector3 dirToBoss = (_monster.transform.position - _monster.MainTarget.position).normalized;
        Vector3 dirToWall = -dirToBoss;

        // 2. 벽 중심 위치: 플레이어 뒤쪽
        Vector3 wallCenter = _monster.MainTarget.position + dirToWall * playerToWallDistance;
        WallCenterPosition = wallCenter; // ✅ 저장!

        // 3. 돌 배치 반지름 계산
        float totalArcLength = spacing * (rockCount - 1);
        float arcRadians = arcAngle * Mathf.Deg2Rad;
        float actualRadius = totalArcLength / arcRadians;

        float startAngle = -arcAngle * 0.5f;

        for (int i = 0; i < rockCount; i++)
        {
            float t = rockCount == 1 ? 0.5f : (float)i / (rockCount - 1);
            float angle = startAngle + arcAngle * t;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 offsetDir = rot * dirToWall;

            Vector3 spawnPos = wallCenter + offsetDir * actualRadius;

            GameObject a = Managers.Resource.Instantiate("StoneWall", spawnPos, Quaternion.LookRotation(-dirToWall));
            GameObject b = Managers.Resource.Instantiate("Effects/StoneGolem/StoneUp", spawnPos, Quaternion.LookRotation(-dirToWall));
            float randomScale = Random.Range(0.9f, 1.2f);
            a.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            a.transform.Rotate(0, Random.Range(-360f, 360f), 0);

            // ✅ 애니메이션 재생 (Rise)
            Animator anim = a.GetComponent<Animator>();
            if (anim != null)
                anim.Play("StoneUp");

            // ✅ Fall 재생 후 제거 (2.5초 뒤 Fall → 0.7초 뒤 제거)
            StartCoroutine(PlayFallAndDestroy(a, 5f, 0.7f));
        }
    }

    private IEnumerator PlayFallAndDestroy(GameObject wall, float delayBeforeFall, float fallAnimDuration)
    {
        yield return new WaitForSeconds(delayBeforeFall);

        Animator anim = wall.GetComponent<Animator>();
        if (anim != null)
            anim.Play("StoneDown");

        yield return new WaitForSeconds(fallAnimDuration);

        Destroy(wall);
    }

    // 돌 벽 스킬 변형 둘 다 가두기

    public void SpawnStonePrison()
    {
        if (_monster.MainTarget == null)
            return;

        Vector3 playerPos = _monster.MainTarget.position;
        Vector3 bossPos = _monster.transform.position;

        // 1. 중심점: 보스와 플레이어 중간
        Vector3 center = (playerPos + bossPos) * 0.5f;

        // 2. 반지름 계산
        float baseDistance = Vector3.Distance(playerPos, bossPos);
        float margin = 2.5f; // 추가 여유 거리
        float radius = (baseDistance * 0.5f) + margin;

        float minRadius = 10f; // 최소 반지름
        if (radius < minRadius)
            radius = minRadius;

        // 3. 돌 개수 계산
        float spacing = 2.2f;
        float circumference = 2 * Mathf.PI * radius;
        int rockCount = Mathf.CeilToInt(circumference / spacing);

        for (int i = 0; i < rockCount; i++)
        {
            float angle = (360f / rockCount) * i;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            Vector3 dir = rot * Vector3.forward;

            Vector3 spawnPos = center + dir * radius;

            GameObject a = Managers.Resource.Instantiate("StoneWall", spawnPos, Quaternion.LookRotation(-dir));
            GameObject b = Managers.Resource.Instantiate("Effects/StoneGolem/StoneUp", spawnPos, Quaternion.LookRotation(-dir));
            float randomScale = Random.Range(0.9f, 1.2f);
            a.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            a.transform.Rotate(0, Random.Range(-360f, 360f), 0);

            Animator anim = a.GetComponent<Animator>();
            if (anim != null)
                anim.Play("StoneUp");

            StartCoroutine(PlayFallAndDestroy(a, 15f, 0.7f));
        }
    }








    // 스킬3 MeteorStone ---------------------------------------------------

    public void StartMeteorStoneRain()
    {
        StartCoroutine(MeteorStoneRainCoroutine());
    }

    private IEnumerator MeteorStoneRainCoroutine()
    {
        int waveCount = 3;
        float waveInterval = 1f;

        int clusterCountPerWave = 6;   // 한 wave(1초 타이밍)당 생성할 중심 지점 개수 (각각 클러스터 역할)
        int stonesPerCluster = 4;      // 각 클러스터에서 생성할 돌의 개수
        float waveRadius = 20f;         // 플레이어를 중심으로 낙하할 전체 범위 반지름 (8m 안에서 중심 지점 선택됨)
        float clusterRadius = 3f;      // 각 클러스터 내부에서 돌이 퍼질 수 있는 반경 (중심에서 최대 2m까지 퍼짐)
        float minDistanceBetween = 1.5f; // 돌들 간 최소 거리 (너무 붙어 있지 않도록 하기 위함)
        float minClusterDistance = 7f; // ✅ 클러스터끼리 최소 간격


        for (int wave = 0; wave < waveCount; wave++)
        {
            if (_monster.MainTarget == null)
                yield break;

            List<Vector3> clusterCenters = new List<Vector3>();

            // ✅ 첫 번째 클러스터는 무조건 플레이어 중심
            clusterCenters.Add(_monster.MainTarget.position);
            // 1. 중심 지점 4개 생성
            int centerAttempts = 0;
            while (clusterCenters.Count < clusterCountPerWave && centerAttempts < 30)
            {
                Vector2 offset = Random.insideUnitCircle * waveRadius;
                Vector3 candidate = _monster.MainTarget.position + new Vector3(offset.x, 0f, offset.y);

                // ✅ 클러스터 간 거리 체크
                bool tooClose = clusterCenters.Any(pos => Vector3.Distance(pos, candidate) < minClusterDistance);
                if (!tooClose)
                    clusterCenters.Add(candidate);

                centerAttempts++;
            }

            // 2. 각 중심에서 4개씩 낙하 위치 생성
            foreach (var center in clusterCenters)
            {
                List<Vector3> clusterPositions = new List<Vector3>();
                int spawnAttempts = 0;

                while (clusterPositions.Count < stonesPerCluster && spawnAttempts < 20)
                {
                    Vector2 offset = Random.insideUnitCircle * clusterRadius;
                    Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);

                    bool tooClose = clusterPositions.Any(p => Vector3.Distance(p, candidate) < minDistanceBetween);
                    if (!tooClose)
                        clusterPositions.Add(candidate);

                    spawnAttempts++;
                }

                // 3. 인디케이터 및 낙하 실행
                foreach (var pos in clusterPositions)
                {
                    MeteorStoneIndicator(pos, wave);
                }
            }

            yield return new WaitForSeconds(waveInterval);
        }
    }

    private void MeteorStoneIndicator(Vector3 pos, int wave)
    {
        GameObject indicator = Managers.Resource.Instantiate("Effects/StoneGolem/MeteorStoneIndicator", pos, Quaternion.identity);
        GameObject.Destroy(indicator, 1.5f);
        StartCoroutine(MeteorStoneAfterDelay(pos, 0.7f, wave));
    }
    private int _lastSoundPlayedWave = -1;         // 마지막으로 사운드 재생된 웨이브
    public AudioClip clip;
    private IEnumerator MeteorStoneAfterDelay(Vector3 pos, float delay, int wave)
    {
        yield return new WaitForSeconds(delay);
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/MeteorStone", pos, Quaternion.identity);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
        CinemachineShake.Instance.ShakeCamera(7, .15f);
        GameObject.Destroy(a, 2f);

        // ✅ Wave 당 사운드 한 번만 재생
        if (_lastSoundPlayedWave != wave)
        {
            _lastSoundPlayedWave = wave;
            //Managers.Sound.Play3DSound(a, clip, 0.0f, 54.0f);
            Managers.Sound.PlayRandomized3DSound(a, clip, 0.0f, 54.0f);
        }
    }

    // 스킬4 잔몹 스폰-------------------------------------------------
    public void SummonMinions()
    {
        int minionCount = 3;
        float spawnRadius = 4f;
        Vector3 bossPosition = _monster.transform.position;

        for (int i = 0; i < minionCount; i++)
        {
            float angle = (360f / minionCount) * i;
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * spawnRadius;
            Vector3 spawnPos = bossPosition + offset;

            GameObject minion = Managers.Resource.Instantiate("Creature/Monster/SpiderMelee", spawnPos, Quaternion.identity);
        }
    }
    // 스킬5 점프공격-------------------------------------------------
    public void JumpStart(float time)
    {
        StoneGolemAttackPatterns.JumpAttackPattern.JumpAttack jumpAttack = _monster.CurrentState as StoneGolemAttackPatterns.JumpAttackPattern.JumpAttack;
        // 목표 위치와 현재 위치 사이의 거리를 계산합니다.
        StartCoroutine(jumpAttack.MoveToTarget(_monster.transform, WallCenterPosition, time));

        //Managers.Resource.Instantiate("Effects/StoneGolem/JumpIndicate", WallCenterPosition, Quaternion.identity);
        //Managers.Resource.Instantiate("Effects/JumpStart", _monster.transform.position, _monster.transform.rotation);

    }

    public void JumpEnd()
    {
        StoneGolemAttackPatterns.JumpAttackPattern.JumpAttack jumpAttack = _monster.CurrentState as StoneGolemAttackPatterns.JumpAttackPattern.JumpAttack;
    }
    public void JumpAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/JumpAttack", WallCenterPosition, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }

    // 스킬 - 돌 정령 소환 후 돌 projetile---------------------------------
    public void SummonStoneSpiritVolley()
    {
        StartCoroutine(StoneSpiritVolleyRoutine());
    }

    private IEnumerator StoneSpiritVolleyRoutine()
    {
        if (_monster.MainTarget == null)
            yield break;

        int spawnCount = 5;
        float spawnDistance = 7f;
        float delayBetweenShots = 0.3f;
        int maxAttempts = 10;

        Transform player = _monster.MainTarget;
        Vector3 playerPos = player.position;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 validSpawnPos = Vector3.zero;
            bool found = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2 offset2D = Random.insideUnitCircle.normalized;
                Vector3 testPos = playerPos + new Vector3(offset2D.x, 0, offset2D.y) * spawnDistance;

                // 1. NavMesh 위에 있는가?
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(testPos, out navHit, 1.0f, NavMesh.AllAreas))
                {
                    // 2. 플레이어와의 라인에 장애물이 없는가?
                    if (!Physics.Linecast(navHit.position + Vector3.up, playerPos + Vector3.up, LayerMask.GetMask("Block")))
                    {
                        validSpawnPos = navHit.position;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                Debug.LogWarning("Valid spawn position not found for Stone Spirit.");
                continue;
            }
            // ✅ 최신 위치 반영해서 발사 방향 재계산
            Vector3 latestPlayerPos = _monster.MainTarget.position;
            Vector3 shootDir = (latestPlayerPos - validSpawnPos).normalized;

            Managers.Resource.Instantiate("Effects/StoneGolem/StoneSpirit", validSpawnPos, Quaternion.LookRotation(shootDir));

            yield return new WaitForSeconds(delayBetweenShots);

            // 투사체 발사
            GameObject proj = Managers.Resource.Instantiate("Effects/StoneGolem/ThrowRockAttack", validSpawnPos + Vector3.up, Quaternion.LookRotation(shootDir));
            proj.GetComponentInChildren<ProjectileDamageCollider>().Init(_monster);
            proj.GetComponentInChildren<Rigidbody>().AddForce(shootDir * 30f, ForceMode.Impulse);

            yield return new WaitForSeconds(delayBetweenShots);
        }
    }


    // 스킬 - 돌 비석 3개
    public void SpawnStonePillars()
    {
        Debug.Log("🔥 [EVENT] SpawnStonePillars() 호출됨");

        if (_monster.MainTarget == null)
        {
            Debug.LogWarning("⚠️ MainTarget이 null입니다. 비석 소환 중단!");
        }
        StartCoroutine(SpawnStonePillarsRoutine());
    }
    private IEnumerator SpawnStonePillarsRoutine()
    {
        // ✅ 여기! 리스트 초기화 먼저
        (_monster as StoneGolemController).linkedPillars.Clear();

        int pillarCount = 3;
        float spawnRadius = 14f;
        float minDistance = 8f;

        List<Vector3> spawnPositions = new List<Vector3>();

        for (int i = 0; i < pillarCount; i++)
        {
            Vector3 candidate;
            int attempt = 0;

            do
            {
                Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
                candidate = _monster.transform.position + new Vector3(offset.x, 0, offset.y);

                bool tooClose = spawnPositions.Any(pos => Vector3.Distance(pos, candidate) < minDistance);
                if (!tooClose)
                    break;

                attempt++;
            }
            while (attempt < 10);

            spawnPositions.Add(candidate);

            // 1. StonePillar 본체
            GameObject pillar = Managers.Resource.Instantiate("Effects/StoneGolem/StonePillar", candidate, Quaternion.identity);
            // 이 줄 추가!
            (_monster as StoneGolemController).linkedPillars.Add(pillar.GetComponent<NatureController>());

            GameObject b = Managers.Resource.Instantiate("Effects/StoneGolem/StoneUp", candidate, Quaternion.identity);

            // 2. 아우라 이펙트
            //GameObject aura = Managers.Resource.Instantiate("Effects/StoneGolem/StoneUp", candidate, Quaternion.identity);

            // 3. 크기/회전 랜덤
            float randomScale = Random.Range(0.9f, 1.2f);
            pillar.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            pillar.transform.Rotate(0, Random.Range(0f, 360f), 0);

            // 4. 애니메이션 재생
            Animator anim = pillar.GetComponent<Animator>();
            if (anim != null)
                anim.Play("StoneUp");
            // 5. 연결선 생성: 여기에 넣기!
            GameObject link = Managers.Resource.Instantiate("Effects/StoneGolem/LinkBeam");
            link.GetComponent<PillarLink>().boss = _monster.transform;
            link.GetComponent<PillarLink>().pillar = pillar.transform;
            // ✅ 이 줄 추가! 비석이 사라질 때 같이 파괴되게 부모 설정
            link.transform.SetParent(pillar.transform);

        }
        // ✅ 보호막 이펙트 생성 (여기!)
        GameObject aura = Managers.Resource.Instantiate("Effects/StoneGolem/PillarAura", _monster.transform.position, Quaternion.identity, _monster.transform); //생성을 부모 자식으로 한 번에 하게
        //aura.transform.SetParent(_monster.transform); // 씬에 스폰을 하고 그 다음 부모로 감. 따라서 스케일 값이 변하게 됨.
        (_monster as StoneGolemController).shieldAura = aura;


        yield return null;
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
    public void Ground3HitEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/Ground3Hit", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.5f);
    }
    public void DeadEffect()
    {
        Managers.Resource.Instantiate("Effects/StoneGolem/Dead", _monster.transform.position, _monster.transform.rotation);
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

    public void IndicatorEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/WindIndicator", _monster.transform.position, Quaternion.identity);
    }
    public void WindChargeEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/WindCharge", _monster.transform.position, Quaternion.identity);
    }
    public void WindAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/Wind", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.2f);
    }
    public void ComboAttack_1Effect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/Slash_R", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.2f);
    }

    public void ComboAttack_2Effect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/Slash_L", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.2f);
    }
    public void ComboAttack_3Effect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/3Hit", _monster.transform.position, _monster.transform.rotation);
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
            //MonsterIndicator.ShowLine(13,5, false, RpgIndicator.IndicatorColor.Enemy, 0);
            MonsterIndicator.ShowRadius(4,false, RpgIndicator.IndicatorColor.Enemy, 0);
            MonsterIndicator.Casting(1f);  // 1.5초 동안 채워지고 끝나면 사라짐        }
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
        Managers.Sound.Play3DSound(gameObject, selectedClip, 0.0f, 54.0f);
    }



}
