using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WolfAnimationEventReciever : MonsterAnimationEventReciever
{

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
        GameObject a = Managers.Resource.Instantiate("Effects/PlantMonster/SpitAttackReady", _monster.transform.position , _monster.transform.rotation);

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
        GameObject a = Managers.Resource.Instantiate("Effects/Wolf/Dash", _monster.transform.position, _monster.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster, 0.5f);
        a.transform.SetParent(_monster.transform);

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
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_monster,0.2f);
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
    }

}
