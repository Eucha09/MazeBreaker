using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using RPG_Indicator;
using System.Linq;

public class NightMareEventReciever : MonsterAnimationEventReciever
{
    public Vector3 WallCenterPosition { get; private set; }
    
    public GameObject ChargingGroundIndicator;
    public void ChargingGroundAttackIndicator()
    {
        if (_monster.MainTarget == null) return;

        Vector3 center = _monster.transform.position;
        Vector3 dir = (_monster.MainTarget.position - center).normalized;

        var go = Instantiate(ChargingGroundIndicator);
        var indicator = go.GetComponent<SectorIndicatorNoShader>();

        indicator.Setup(center, dir, angle: 120f, inner: 0f, outer: 10f, duration: 2f/3f);
        indicator.OnCharged = () =>
        {
            // 인디케이터가 다 차면 실제 공격 이펙트 & 데미지
            //GameObject atk = Managers.Resource.Instantiate("Effects/NightMare/WindSlash", center, Quaternion.LookRotation(dir));
            //atk.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.2f);
            indicator.ActivateHitCollider(_monster, 0.1f);

            // 🔥 인디케이터 자연스럽게 사라짐
            indicator.FadeAndDestroy(0.4f);
        };
        indicator.Play();
    }
    /*public void PortalOutIndicator()
    {
        if (_monster.MainTarget == null) return;

        Vector3 center = _monster.transform.position - new Vector3(0,1,4);
        Vector3 dir = (_monster.transform.forward).normalized;

        var go = Instantiate(sectorIndicatorPrefab);
        var indicator = go.GetComponent<SectorIndicatorNoShader>();

        indicator.Setup(center, dir, angle: 120f, inner: 0f, outer: 13f, duration: 8f / 30f);
        indicator.OnCharged = () =>
        {
            // 인디케이터가 다 차면 실제 공격 이펙트 & 데미지
            //GameObject atk = Managers.Resource.Instantiate("Effects/NightMare/WindSlash", center, Quaternion.LookRotation(dir));
            //atk.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.2f);
            // 🔥 인디케이터 자연스럽게 사라짐
            indicator.FadeAndDestroy(0.4f);
        };
        indicator.Play();
    }*/
    public void CameraShakeshort(float intensity)
    {
        CinemachineShake.Instance.ShakeCamera(intensity, .15f);
    }

    public void CameraShakelong(float intensity)
    {
        CinemachineShake.Instance.ShakeCamera(intensity, 4f);
    }
    public void ZoomIn8(float time)
    {
        CinemachineShake.Instance.ZoomIn(8, time);
    }

    public void ZoomIn9(float time)
    {
        CinemachineShake.Instance.ZoomIn(9, time);
    }
    public void ZoomOut(float time)
    {
        CinemachineShake.Instance.ZoomOut(12, time);
    }

    public void ResetZoom(float time)
    {
        CinemachineShake.Instance.ResetZoom(time);
    }
    public void MoveForward()
    {
        StartCoroutine(SpecialAttackRoutine(25f, 0.35f));
    }
    public void Addforce(float force)
    {
        _monster.Rb.AddForce(_monster.transform.forward * force, ForceMode.Impulse);
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
    public void IndicatorEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/WindIndicator", _monster.transform.position, Quaternion.identity);
    }
    public void WindAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/Wind", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.2f);
    }
    // 스킬1  아래서 위로 소환 
    public int count = 5;
    public float interval = .5f;
    public float spawnRadius = 3f;
    public float minDistanceBetween = 1.5f;

    // ▶ N번째마다 플레이어 현재 위치에 생성(스냅샷). 예: 2 → 0,2,4번째
    public int centralEveryN = 2;

    public void StartStoneUp()
    {
        StartCoroutine(StartStoneUpCoroutine());
    }

    private IEnumerator StartStoneUpCoroutine()
    {
        List<Vector3> spawnPositions = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            if (_monster.MainTarget == null)
                yield break;

            Vector3 targetPos;

            // ✅ N번째마다 "그 순간의" 플레이어 위치를 그대로 사용 (인디케이터 추적 X)
            if (centralEveryN > 0 && i % centralEveryN == 0)
            {
                targetPos = _monster.MainTarget.position;  // 스냅샷
            }
            else
            {
                // 🎯 랜덤(겹침 방지)
                Vector3 center = _monster.MainTarget.position;
                Vector3 candidate;
                int attempts = 0;

                do
                {
                    Vector2 offset = Random.insideUnitCircle * spawnRadius;
                    candidate = new Vector3(center.x + offset.x, center.y, center.z + offset.y);

                    bool tooClose = spawnPositions.Any(pos => Vector3.Distance(pos, candidate) < minDistanceBetween);
                    if (!tooClose) break;

                    attempts++;
                } while (attempts < 10);

                targetPos = candidate;
            }

            spawnPositions.Add(targetPos);
            SpawnStoneAt(targetPos);          // 인디케이터 고정 → 딜레이 후 스톤
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnStoneAt(Vector3 pos)
    {
        GameObject ind = Managers.Resource.Instantiate("Effects/NightMare/Indicator", pos, Quaternion.identity);
        GameObject.Destroy(ind, 1.5f);

        StartCoroutine(SpawnStoneStrikeAfterDelay(pos, .4f));
    }

    private IEnumerator SpawnStoneStrikeAfterDelay(Vector3 pos, float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject a = Managers.Resource.Instantiate("Effects/NightMare/StoneUp", pos, Quaternion.identity);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.5f);
        GameObject.Destroy(a, 2f);
    }





    // 스킬 동시에 펑 터지는 원 여러 개 생성
    public void SpawnAreaEffectsWithIndicator()
    {
        if (_monster.MainTarget == null)
            return;

        int count = 6;
        float radius = 7f;
        float yOffset = 0.1f;
        float navSampleRadius = 1.0f;
        int spawnAttempts = 10;

        int spawned = 0;

        while (spawned < count && spawnAttempts > 0)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 candidate = _monster.MainTarget.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, navSampleRadius, NavMesh.AllAreas))
            {
                Vector3 spawnPos = hit.position + Vector3.up * yOffset;

                // ▶ 1. 인디케이터 먼저 생성
                GameObject indicator = Managers.Resource.Instantiate("Effects/NightMare/AreaIndicator", spawnPos, Quaternion.identity);

                // ▶ 2. 코루틴으로 0.8초 후 데미지 이펙트 생성
                StartCoroutine(SpawnDelayedAreaEffect(spawnPos, 0.6f));

                spawned++;
            }

            spawnAttempts--;
        }
    }

    private IEnumerator SpawnDelayedAreaEffect(Vector3 spawnPos, float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject effect = Managers.Resource.Instantiate("Effects/NightMare/AreaAttack", spawnPos, Quaternion.identity);
        effect.GetComponentInChildren<DefaultDamageCollider>()?.Init(_monster, 0.1f); // 데미지 주려면 여기에
    }
    // Leather SKill
    public void LeatherProjectiles()
    {
        if (_monster.MainTarget == null)
            return;

        int projectileCount = 5;                // 총 발사 개수
        float spreadAngle = 45f;                // 전체 부채꼴 각도
        float shootForce = 30f;                 // 발사 속도

        Vector3 startPos = _monster.transform.position + Vector3.up * 1.5f;
        Vector3 forward = (_monster.MainTarget.position - _monster.transform.position).normalized;
        forward.y = 0;
        forward.Normalize();

        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadAngle / (projectileCount - 1);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 direction = rotation * forward;

            // ✅ 투사체 생성
            GameObject proj = Managers.Resource.Instantiate("Effects/NightMare/Leather", _monster.transform.position, Quaternion.LookRotation(direction));
            proj.GetComponentInChildren<ProjectileDamageCollider>().Init(_monster);
            proj.GetComponentInChildren<Rigidbody>().AddForce(direction * shootForce, ForceMode.Impulse);
        }

    }
    public GameObject lineIndicatorPrefab;
    public float mergeDuration = 0.5f;       // 선 수렴 시간
    public float sideOffset = 1f;            // 좌우 거리
    public float lineLength = 20f;           // 선 길이

    public void SpawnMergingIndicators(Vector3 leftStart, Vector3 rightStart, Vector3 forward)
    {
        StartCoroutine(MergeIndicatorsRoutine(leftStart, rightStart, forward));
    }

    private IEnumerator MergeIndicatorsRoutine(Vector3 leftStart, Vector3 rightStart, Vector3 forward)
    {
        Vector3 centerStart = (leftStart + rightStart) * 0.5f;

        GameObject lineL = Instantiate(lineIndicatorPrefab, leftStart, Quaternion.identity);
        GameObject lineR = Instantiate(lineIndicatorPrefab, rightStart, Quaternion.identity);

        LineRenderer lrL = lineL.GetComponent<LineRenderer>();
        LineRenderer lrR = lineR.GetComponent<LineRenderer>();

        float elapsed = 0f;

        while (elapsed < mergeDuration)
        {
            float t = elapsed / mergeDuration;
            Vector3 currentLeft = Vector3.Lerp(leftStart, centerStart, t);
            Vector3 currentRight = Vector3.Lerp(rightStart, centerStart, t);

            lrL.SetPosition(0, currentLeft);
            lrL.SetPosition(1, currentLeft + forward * lineLength);

            lrR.SetPosition(0, currentRight);
            lrR.SetPosition(1, currentRight + forward * lineLength);

            elapsed += Time.deltaTime;
            yield return null;
        }

        lrL.SetPosition(0, centerStart);
        lrL.SetPosition(1, centerStart + forward * lineLength);

        lrR.SetPosition(0, centerStart);
        lrR.SetPosition(1, centerStart + forward * lineLength);

        yield return new WaitForSeconds(0.2f);
        Destroy(lineL);
        Destroy(lineR);
    }



    public void SummonCrowPairVolley()
    {
        StartCoroutine(CrowPairVolleyRoutine());
    }

    private IEnumerator CrowPairVolleyRoutine()
    {
        if (_monster.MainTarget == null)
            yield break;

        Vector3 center = _monster.transform.position;
        Vector3 forward = (_monster.MainTarget.position - center).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        float sideOffset = 2.5f;
        float yOffset = 1.0f;

        // ▶ 1. 좌/우 위치 계산
        Vector3 leftSpawnPos = center - right * sideOffset + Vector3.up * yOffset;
        Vector3 rightSpawnPos = center + right * sideOffset + Vector3.up * yOffset;

        // ▶ 2. 까마귀 생성 (임시 Sphere or 추후 Prefab)
        GameObject crowL = Managers.Resource.Instantiate("Effects/NightMare/Crow", leftSpawnPos, Quaternion.LookRotation(forward));
        GameObject crowR = Managers.Resource.Instantiate("Effects/NightMare/Crow", rightSpawnPos, Quaternion.LookRotation(forward));
        crowL.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 10f);
        crowR.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 10f);

        crowL.transform.SetParent(_monster.transform);
        crowR.transform.SetParent(_monster.transform);
        Rigidbody rbL = crowL.GetComponent<Rigidbody>();
        Rigidbody rbR = crowR.GetComponent<Rigidbody>();

        // ▶ 3. 첫 번째 까마귀 돌진
        yield return new WaitForSeconds(3f);
        if (_monster.MainTarget != null)
        {
            // 기준 방향 계산 (까마귀 → 플레이어)
            Vector3 dirL = (_monster.MainTarget.position - crowL.transform.position).normalized;
            Vector3 rightL = Vector3.Cross(Vector3.up, dirL);
            Vector3 leftStartL = crowL.transform.position - rightL * sideOffset + Vector3.up * 0.1f;
            Vector3 rightStartL = crowL.transform.position + rightL * sideOffset + Vector3.up * 0.1f;

            SpawnMergingIndicators(leftStartL, rightStartL, dirL);
            yield return new WaitForSeconds(mergeDuration);
            crowL.transform.SetParent(null);      // 부모에서 분리
            rbL.AddForce(dirL * 40f, ForceMode.Impulse);
        }

        // ▶ 4. 두 번째 까마귀 돌진
        yield return new WaitForSeconds(1f);
        if (_monster.MainTarget != null)
        {
            Vector3 dirR = (_monster.MainTarget.position - crowR.transform.position).normalized;
            Vector3 rightR = Vector3.Cross(Vector3.up, dirR);
            Vector3 leftStartR = crowR.transform.position - rightR * sideOffset + Vector3.up * 0.1f;
            Vector3 rightStartR = crowR.transform.position + rightR * sideOffset + Vector3.up * 0.1f;

            SpawnMergingIndicators(leftStartR, rightStartR, dirR);
            yield return new WaitForSeconds(mergeDuration);

            crowR.transform.SetParent(null);      // 부모에서 분리
            rbR.AddForce(dirR * 40f, ForceMode.Impulse);
        }

        // ▶ 5. 일정 시간 후 제거
        Destroy(crowL, 2f);
        Destroy(crowR, 2f);
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
        NightMareAttackPatterns.JumpAttackPattern.JumpAttack jumpAttack = _monster.CurrentState as NightMareAttackPatterns.JumpAttackPattern.JumpAttack;
        // 목표 위치와 현재 위치 사이의 거리를 계산합니다.
        _monster.Nma.enabled = false;   // 점프 중 강제 위치이동
        StartCoroutine(MoveToTarget(_monster.transform, _monster.transform.position + _monster.transform.forward * 10f, time));
        //Managers.Resource.Instantiate("Effects/JumpIndicate", _monster.transform.position + _monster.transform.forward * 10f, Quaternion.identity);

        //Managers.Resource.Instantiate("Effects/StoneGolem/JumpIndicate", WallCenterPosition, Quaternion.identity);
        //Managers.Resource.Instantiate("Effects/JumpStart", _monster.transform.position, _monster.transform.rotation);

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


  


            // 다음 프레임까지 대기
            yield return null;
        }

        // 정확히 목표 위치에 도착하도록 설정
        if (Time.time - startTime >= duration)
            MainTarget.position = destination;
    }
    public void JumpEnd()
    {
        _monster.Nma.enabled = true;
        NightMareAttackPatterns.JumpAttackPattern.JumpAttack jumpAttack = _monster.CurrentState as NightMareAttackPatterns.JumpAttackPattern.JumpAttack;
    }
    public void JumpAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/StoneGolem/JumpAttack", WallCenterPosition, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }

    public void ChargingGroundEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/NightMare/ChargingGround", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
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
    public void SlashEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/NightMare/Slash", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.5f);
    }

    Vector3 targetPos;

    public void Gound4HitIndicatorEffect()
    {
        targetPos = _monster.MainTarget.position;  // 스냅샷
        GameObject a = Managers.Resource.Instantiate("Effects/NightMare/Ground4HitIndicator", targetPos+new Vector3(0,.5f,0), Quaternion.identity);
    }
    public void Gound4HitEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/NightMare/Ground4Hit", targetPos, Quaternion.identity);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.1f);
    }
    public void Gound4HitPortalEffect()
    {
        GameObject a = Managers.Resource.Instantiate("Effects/NightMare/Ground4HitPortal", _monster.transform.position + (_monster.transform.forward).normalized * 3f, _monster.transform.rotation);
    }
    public void PortalInEffect()
    {
        Managers.Resource.Instantiate("Effects/NightMare/PortalIn", _monster.transform.position + (_monster.transform.forward).normalized * 3f, _monster.transform.rotation);
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

    public void PoisonProjectileAttackEffect()
    {

        Debug.Log("이펙트 호출~~");
        GameObject a = Managers.Resource.Instantiate("Effects/SpiderMelee/PoisonProjectileAttack", new Vector3(_monster.transform.position.x, _monster.transform.position.y + 2, _monster.transform.position.z), Quaternion.LookRotation(PlayerPos - _monster.transform.position));
        a.GetComponentInChildren<ProjectileDamageCollider>().Init(_monster);
        Vector3 direction = PlayerPos - a.transform.position;
        direction.y = 0;
        a.GetComponentInChildren<Rigidbody>().AddForce(direction.normalized * 20f, ForceMode.Impulse);
    }

    public void PlaySound(AudioClip clip)
    {
        Managers.Sound.Play3DSound(gameObject, clip, 0.0f, 54.0f);
    }

    public override void Dissolve()
    {
        base.NightMareDissolve();
        //NightMareController monster = _monster as NightMareController;
    }
    public override void Appear()
    {
        base.NightMareAppear();
        //NightMareController monster = _monster as NightMareController;
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
