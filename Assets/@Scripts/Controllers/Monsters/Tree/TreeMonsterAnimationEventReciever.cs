using TMPro;
using UnityEngine;


//Beast 애니메이션에서 일어나는 모든 Event들을 받는다.
public class TreeMonsterAnimationEventReciever : MonoBehaviour
{
    [SerializeField]
    private TreeMonsterController _monsterController;

    public void AnimationEnd()
    {
        TreeMonster.State currentState = _monsterController.CurrentState;
        currentState.IsAnimationEnd = true;
        if (currentState != null)
        {
            currentState.IsAnimationEnd = true;
        }
        else
        {
            Debug.LogError("Failed");
        }
    }

    public void TargetLooking()
    {
        TreeMonster.State lookingState = _monsterController.CurrentState;
        lookingState.isLooking = true;
    }

    public void TargetNotLooking()
    {
        TreeMonster.State lookingState = _monsterController.CurrentState;
        lookingState.isLooking = false;
    }

    public void SpitOneCycleEnd()
    {
        TreeMonster.Attack.SpitAttack spitAttackState = _monsterController.CurrentState as TreeMonster.Attack.SpitAttack;
        spitAttackState._isSpit1CycleEnd = true;

    }
    public void JumpAttackEffect()
    {
        Managers.Resource.Instantiate("Effects/TreeMonster/Jump", _monsterController.transform.position, Quaternion.Euler( new Vector3(-90,0,0)));
    }

    public void SpitProjectileSpawn()
    {
        // 현재 몬스터 컨트롤러의 Euler 각도 값을 가져옴
        Vector3 currentRotation = _monsterController.transform.eulerAngles;

        // y축 값을 -30도 회전시킴
        Quaternion newRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentRotation.z);

        GameObject a = Managers.Resource.Instantiate("Effects/TreeMonster/SpitProjectile1", _monsterController.transform.position, newRotation);
        //a.GetComponentInChildren<SkillCollider>().ArrowInit(_monsterController, 15f);
        a.GetComponentInChildren<Rigidbody>().AddForce(_monsterController.transform.forward * 10f,ForceMode.Impulse);

        newRotation = Quaternion.Euler(currentRotation.x, currentRotation.y + 30, currentRotation.z);

        GameObject b = Managers.Resource.Instantiate("Effects/TreeMonster/SpitProjectile1", _monsterController.transform.position, newRotation);
        //b.GetComponentInChildren<SkillCollider>().ArrowInit(_monsterController, 15f);
        b.GetComponentInChildren<Rigidbody>().AddForce(b.transform.forward * 10f, ForceMode.Impulse);

        newRotation = Quaternion.Euler(currentRotation.x, currentRotation.y - 30, currentRotation.z);

        GameObject c = Managers.Resource.Instantiate("Effects/TreeMonster/SpitProjectile1", _monsterController.transform.position, newRotation);
        //c.GetComponentInChildren<SkillCollider>().ArrowInit(_monsterController, 15f);
        c.GetComponentInChildren<Rigidbody>().AddForce(c.transform.forward * 10f, ForceMode.Impulse);
    }

    public void AttackGlowOn()
    {
        _monsterController.LerpAttackGlowColorCoroutinStart();
    }

    public void Dissolve()
    {
        _monsterController.LerpDissolveStart();
    }

    public AudioClip[] clips;
    private int currentIndex = 0; // 현재 인덱스를 저장하는 필드

    public void SoundRandom()
    {
        
            if (clips.Length > 0)
            {
                // 배열에서 무작위로 하나의 사운드를 선택
                int randomIndex = Random.Range(0, clips.Length);
                AudioClip randomClip = clips[randomIndex];

                // 선택된 사운드 재생
                Managers.Sound.Play(randomClip);
            }
        
    }
    public void PlaySound(AudioClip clip)
    {

     
            Managers.Sound.Play(clip);
        

    }
}
