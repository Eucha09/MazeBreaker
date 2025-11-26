using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Player;
public class PlayerAnimationEventReciever : MonoBehaviour
{
    public AudioClip[] clips;
    public AudioClip[] HurtAudio;



    [SerializeField]
    private PlayerController _playerController;


    public void CameraShake(float intensity)
    {
        CinemachineShake.Instance.ShakeCamera(intensity, .15f);  //5f가 무난
    }

    public void CameraShake_1f(float intensity)
    {
        CinemachineShake.Instance.ShakeCamera(intensity, 1f);  //5f가 무난
    }

    public void ZoomIn8(float time)
    {
        CinemachineShake.Instance.ZoomIn(8,time);
    }

    public void ZoomIn9(float time)
    {
        CinemachineShake.Instance.ZoomIn(9, time);
    }
    public void ZoomIn9_7(float time)
    {
        CinemachineShake.Instance.ZoomIn(9.7f, time);
    }
    public void ZoomIn9_5(float time)
    {
        CinemachineShake.Instance.ZoomIn(9.5f, time);
    }
    public void ZoomOut(float time)
    {
        CinemachineShake.Instance.ZoomOut(12, time);
    }

    public void ResetZoom(float time)
    {
        CinemachineShake.Instance.ResetZoom(time);
    }

    private void Awake()
    {
        playerGlowmat.SetColor("_Color", originalColor);
    }
    public Material playerGlowmat;
    public float glowIntensity = 8f;
    private Coroutine glowCoroutine; // 현재 실행 중인 코루틴 참조
    public Color originalColor;
    public Color HurtColor;
    public Color ChargeColor;
    /*
    public void TurnOnGlow(float duration)
    {
        // 이미 실행 중인 글로우 코루틴이 있다면 중지
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }
        //  playerGlowmat.SetColor("_Color", playerGlowmat.GetColor("_Color") * 10);
        glowCoroutine = StartCoroutine(GlowEffect(duration, true));
    }
    public void TurnOffGlow(float duration)
    {
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }

        // 새 코루틴 시작
        glowCoroutine = StartCoroutine(GlowEffect(duration, false));
    }
    private IEnumerator GlowEffect(float duration, bool isTurningOn)
    {
        float elapsedTime = 0f;
        Color targetColor = isTurningOn
            ? GlowColor * glowIntensity // 글로우 강도 증가
            : originalColor; // 글로우 강도 감소

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // 색상 선형 보간
            Color currentColor = Color.Lerp(originalColor, targetColor, t);
            playerGlowmat.SetColor("_Color", currentColor);

            yield return null; // 한 프레임 대기
        }

        // 정확한 최종 값 보정
        playerGlowmat.SetColor("_Color", targetColor);
    }

    */


    public void SetGlow_Charge()
    {
        // 이미 실행 중인 글로우 코루틴이 있다면 중지
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }

        // 새 코루틴 시작 (onDuration과 offDuration을 각각 사용)
        glowCoroutine = StartCoroutine(GlowEffect(.1f, .5f,ChargeColor));
    }

    public void SetGlow_Hurt()
    {
        // 이미 실행 중인 글로우 코루틴이 있다면 중지
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }

        // 새 코루틴 시작 (onDuration과 offDuration을 각각 사용)
        glowCoroutine = StartCoroutine(GlowEffect(.1f, .5f,HurtColor));
    }

    private IEnumerator GlowEffect(float onDuration, float offDuration, Color glowcolor)
    {
        float elapsedTime = 0f;

        // 1. 글로우를 켜는 효과
        Color targetColorOn = glowcolor * glowIntensity;
        while (elapsedTime < onDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / onDuration;
            Color currentColor = Color.Lerp(originalColor, targetColorOn, t);
            playerGlowmat.SetColor("_Color", currentColor);
            yield return null;
        }
        playerGlowmat.SetColor("_Color", targetColorOn); // 정확히 타겟 색으로 설정

        // 잠시 멈춤, 그 다음에 끄는 효과를 진행
        elapsedTime = 0f;
        Color targetColorOff = originalColor;
        while (elapsedTime < offDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / offDuration;
            Color currentColor = Color.Lerp(targetColorOn, targetColorOff, t);
            playerGlowmat.SetColor("_Color", currentColor);
            yield return null;
        }
        playerGlowmat.SetColor("_Color", targetColorOff); // 정확히 타겟 색으로 설정
    }




    public void AnimationEnd()
    {
        _playerController.CurrentState.AnimationEnd();
    }

    public void ParrySuccessTimeEnd()
    {
        Player.BlockState parryCheck = _playerController.CurrentState as Player.BlockState; ;
        if (parryCheck != null)
        {
            parryCheck.IsParrySuccessTimeOver = true;
        }
        else
        {
            Debug.LogError("Failed to cast PlayerState to PlayerAttackState.");
        }
    }

    public void OnPlayerAttackDelayOver()
    {
        _playerController.CurrentState.CanChangeStateByInput = true;
    }

    public void BufferInputAllow()
    {
        _playerController.CurrentState.CanBufferInput = true;
    }

    public void FullCharge()
    {
        Player.ChargeState state = _playerController.CurrentState as Player.ChargeState;
        if (state != null)
        {
            state.FullCharge();
        }
    }

    public void PointerTrackingEnd()
    {
        if(_playerController.CurrentState is ComboAttackState state)
        {
            state.PointerTrackingEnd();
        }
    }

    public void CleanupBowIndicator()
    {
        if (_playerController.CurrentState is ComboAttackState state)
        {
            state.CleanupBowIndicator();
        }
    }

    public void ChargeCheck()
    {
        Player.ComboAttackState state = _playerController.CurrentState as Player.ComboAttackState;
        if (state != null)
        {
            state.ChargeCheck();
        }
    }

    public void ChargeLaterCheck()
    {
        Player.ComboAttackState state = _playerController.CurrentState as Player.ComboAttackState;
        if (state != null)
        {
            state.ChargeCheck();
        }
    }

    public void DashEffect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "Dash", _playerController.transform.position, _playerController.transform.rotation);
        Destroy(a, 5f);

    }


    public void PunchAttack1Effect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "PunchAttack1", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);

    }
    public void SwordAttack1Effect()
    {
        if (_playerController.CurrentState.GetState() == PlayerStateType.Charge)
            return;
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "SwordAttack1", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);

    }
    public void SwordAttack2Effect()
    {
        if (_playerController.CurrentState.GetState() == PlayerStateType.Charge)
            return;
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "SwordAttack2", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
    }

    public void SwordAttack3Effect()
    {
        if (_playerController.CurrentState.GetState() == PlayerStateType.Charge)
            return;
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "SwordAttack3", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);

    }
    public void MaceAttack1Effect()
    {
        if (_playerController.Ani.IsInTransition(0))
            return;

        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "MaceAttack1", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
    }
    public void MaceAttack2Effect()
    {
        if (_playerController.Ani.IsInTransition(0))
            return;

        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "MaceAttack2", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
        Destroy(a, 1f);

    }
    public void MaceAttack3Effect()
    {
        if (_playerController.Ani.IsInTransition(0))
            return;

        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "MaceAttack3", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
        Destroy(a, 1f);

    }
    public void MaceFullChargedAttack()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "MaceFullChargedAttack", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
    }
    public void MaceChargedAttack()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "MaceChargedAttack", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);

    }

    public void AxeAttack1Effect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "AxeAttack1", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
        Destroy(a, 5f);
    }
    public void AxeAttack2Effect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "AxeAttack2", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
        Destroy(a, 5f);
    }
    public void AxeAttack3Effect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "AxeAttack3", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
        Destroy(a, 5f);
    }
    public void AxeSlashUpAttack()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "AxeChargedAttack", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
    }
    public void AxeFullChargedAttack()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "AxeFullChargedAttack", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
    }

    public void AxeSpinAttack()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "AxeSpinAttack", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DamageOverTimeDamageCollider>().Init(_playerController, 13, 0.1f);

    }

    public void SwordChargedAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "SwordChargedAttack", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
        Destroy(a, 3f);

    }
    public void SwordFullChargedAttackEffect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "SwordFullChargedAttack", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
        Destroy(a, 3f);

    }
    public void SwordGroundSkillEffect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "SwordGroundSkill", _playerController.transform.position, _playerController.transform.rotation);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
        Destroy(a, 3f);

    }
    public void SwordSpecialAttackAddforce(float force)
    {
        if (_playerController.CurrentState.GetState() == PlayerStateType.Charge)
            return;
        if (_playerController.Ani.IsInTransition(0))
            return;
        _playerController.Rb.AddForce(_playerController.transform.forward * force, ForceMode.Impulse);
    }
    public void MoveForward()
    {
        StartCoroutine(SpecialAttackRoutine(15f, 0.5f));
    }
    [SerializeField] private AnimationCurve dashCurve;
    // 0~1 구간, 값은 0~1로 설정 (Inspector)

    // speed는 최고 속도
    public void MoveForward_AxeChargedAttack()
    {
        StartCoroutine(SpecialAttackRoutine(10f, 0.5f));
    }

    public IEnumerator SpecialAttackRoutine(float maxSpeed, float duration)
    {
        Rigidbody rb = _playerController.Rb;
        Vector3 forward = _playerController.transform.forward;

        float elapsed = 0f;

        // 기존 속도 저장
        Vector3 originalVelocity = rb.linearVelocity;

        // FixedUpdate 기반으로 이동
        while (elapsed < duration)
        {
            yield return new WaitForFixedUpdate();

            elapsed += Time.fixedDeltaTime;

            // 0~1 구간 변환
            float t = Mathf.Clamp01(elapsed / duration);

            // Curve 값 (0~1)
            float curveValue = dashCurve.Evaluate(t);

            // 가속/감속된 속도 적용
            rb.linearVelocity = forward * (maxSpeed * curveValue);
        }

        // 종료 후 속도 초기화
        rb.linearVelocity = originalVelocity;
    }



    public void BowSpecialShotEffect()
    {
        //스폰할 이펙트 지정
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "BowSpecialAttackShot", _playerController.transform.position, _playerController.transform.rotation);
        a.transform.SetParent(_playerController.transform);

        //a.GetComponent<SkillCollider>().Init(_playerController);
    }
    public void BowSpecialHitEffect()
    {
        //스폰할 이펙트 지정
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "BowSpecialAttackHit", _playerController.transform.position, _playerController.transform.rotation);
    }

    public void BowFullChargeEffect()
    {
        Debug.Log("BowOneShot이펙트");
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "BowFullChargedAttack", new Vector3(_playerController.transform.position.x, _playerController.transform.position.y+1, _playerController.transform.position.z ), _playerController.transform.rotation);
        a.GetComponentInChildren<ProjectileDamageCollider>().Init(_playerController, true);
        a.GetComponentInChildren<Rigidbody>().AddForce(a.transform.forward * 40f, ForceMode.Impulse);
        a.AddComponent<ProjectileDestory>().Init(14f);
        Destroy(a, 5f);
    }
    public void BowChargeEffect()
    {
        Debug.Log("BowOneShot이펙트");
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "BowChargedAttack", new Vector3(_playerController.transform.position.x, _playerController.transform.position.y + 1, _playerController.transform.position.z), _playerController.transform.rotation);
        a.GetComponentInChildren<ProjectileDamageCollider>().Init(_playerController, true);
        a.GetComponentInChildren<Rigidbody>().AddForce(a.transform.forward * 40f, ForceMode.Impulse);
        a.AddComponent<ProjectileDestory>().Init(10f);
        Destroy(a, 5f);
    }
    public void BowAttack1Effect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "Arrow", new Vector3(_playerController.transform.position.x, _playerController.transform.position.y + .3f, _playerController.transform.position.z), _playerController.transform.rotation);
        a.GetComponentInChildren<ProjectileDamageCollider>().Init(_playerController, false);
        //a.GetComponentInChildren<BowChargeShotSkill>().Init(15f);
        a.GetComponentInChildren<Rigidbody>().AddForce(a.transform.forward * 40f, ForceMode.Impulse);
        Debug.Log("BowAttack1 출력");
        a.AddComponent<ProjectileDestory>().Init(10f);
        Destroy(a, 5f);
    }
    public void ChargeReadyEffect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "ChargeReady", _playerController.transform.position, _playerController.transform.rotation);
        Destroy(a, 3f);

    }
    public void ChargeFullReadyEffect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "ChargeFullReady", _playerController.transform.position, _playerController.transform.rotation);
        Destroy(a, 3f);

    }
    public void BowOneShotReadyEffect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "BowOneShotReady", Util.FindChild(gameObject, "HP_Bow_Main", true).transform);
        a.GetComponentInChildren<DefaultDamageCollider>().Init(_playerController, 0.1f);
        a.transform.localPosition = Vector3.zero;
        Destroy(a, 3f);

    }
    public void BowOneShotAuraEffect()
    {
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + "BowOneShotAura", _playerController.transform.position, _playerController.transform.rotation);
        Destroy(a, 3f);
    }
    public void ArrowChargeEffect(string effect)
    {
        //스폰할 이펙트 지정
        GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + effect, Util.FindChild(gameObject, "HP_Bow_Main", true).transform);
        a.transform.localPosition = Vector3.zero;
        Destroy(a, 3f);
    }


 
    public void SoundAnimation(AudioClip clip)
    {
        Managers.Sound.Play(clip);
    }
    private int currentIndex = 0; // 현재 인덱스를 저장하는 필드

    public void SoundRandom()
    {
        if (clips.Length > 0 && Random.value <= 0.5f)
        {
            // 현재 인덱스의 사운드를 선택
            AudioClip currentClip = clips[currentIndex];

            // 선택된 사운드 재생
            Managers.Sound.Play(currentClip);

            // 인덱스를 다음으로 이동, 끝에 도달하면 0으로 초기화
            currentIndex = (currentIndex + 1) % clips.Length;
        }
    }

    private int HurtcurrentIndex = 0; // 현재 인덱스를 저장하는 필드

    public void HurtSoundRandom()
    {
        if (HurtAudio.Length > 0)
        {
            // 현재 인덱스의 사운드를 선택
            AudioClip currentClip = HurtAudio[HurtcurrentIndex];

            // 선택된 사운드 재생
            Managers.Sound.Play(currentClip);

            // 인덱스를 다음으로 이동, 끝에 도달하면 0으로 초기화
            HurtcurrentIndex = (HurtcurrentIndex + 1) % HurtAudio.Length;
        }
    }
    public void WakeUpEnd()
    {
        _playerController.CurrentState = new Player.PlayerIdleState();
    }

    public void DeadFadeOut()
    {
        UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
        StartCoroutine(ui.CoFadeOut());
        Invoke("SceneLoad",3f);
    }

    public void SceneLoad()
    {
        // 현재 씬 가져오기
        Scene currentScene = SceneManager.GetActiveScene();

        // 현재 씬 다시 로드
        SceneManager.LoadScene(currentScene.name);
    }
}

/* 활 특수 공격

public void BowSpecialAttackIndicator(string effect)
{
    Player.BowArrowRainAttackState bowState = _playerController.CurrentState as Player.BowArrowRainAttackState;
    Vector3 attackPos = bowState._skillInfo.AttackPos;
    Vector3 targetPos;
    float distanceToMouse = Vector3.Distance(_playerController.transform.position, attackPos);

    if (distanceToMouse > 10f)
    {
        Vector3 direction = (attackPos - _playerController.transform.position).normalized;
        targetPos = _playerController.transform.position + direction * 10f;

    }
    else
    {
        targetPos = attackPos;

    }
    GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + effect, targetPos, _playerController.transform.rotation);
    Managers.Resource.Instantiate(_playerController.EffectPath + effect, targetPos, _playerController.transform.rotation);
}

public void BowSpecialAttackEffect(string effect)
{
    Player.BowArrowRainAttackState bowState = _playerController.CurrentState as Player.BowArrowRainAttackState;
    Vector3 attackPos = bowState._skillInfo.AttackPos;
    Vector3 targetPos;
    float distanceToMouse = Vector3.Distance(_playerController.transform.position, attackPos);

    if (distanceToMouse > 10f)
    {
        Vector3 direction = (attackPos - _playerController.transform.position).normalized;
        targetPos = _playerController.transform.position + direction * 10f;

    }
    else
    {
        targetPos = attackPos;

    }
    GameObject a = Managers.Resource.Instantiate(_playerController.EffectPath + effect, targetPos, _playerController.transform.rotation);
    a.GetComponentInChildren<DamageOverTimeDamageCollider>().Init(_playerController, 6, 0.1f);
    Destroy(a, 5f);

}
*/
/*
public void DisableRotation()
{
    var rotatable = _playerController.CurrentState as Player.BowArrowRainAttackState;
    if (rotatable != null)
    {
        rotatable.CanRotate = false;

    }
}
public void DisableArrowOneShotRotation()
{
    var rotatable = _playerController.CurrentState as Player.BowOneShotAttackState;
    if (rotatable != null)
    {
        rotatable.CanRotate = false;

    }
}
public void DisableAxeRotation()
{
    var rotatable = _playerController.CurrentState as Player.AxeThrowAttackState;
    if (rotatable != null)
    {
        rotatable.CanRotate = false;

    }
}
public void DisableMacePulseRotation()
{
    var rotatable = _playerController.CurrentState as Player.MacePulseAttackState;
    if (rotatable != null)
    {
        rotatable.CanRotate = false;
    }
}


public void LockArrowRainIndicator()
{
    var state = _playerController.CurrentState as Player.BowArrowRainAttackState;
    if (state != null)
        state.LockIndicatorPosition();
}
public void LockArrowOneShotIndicator()
{
    var state = _playerController.CurrentState as Player.BowOneShotAttackState;
    if (state != null)
        state.LockIndicatorPosition();
}
public void LockAxeThrowIndicator()
{
    var state = _playerController.CurrentState as Player.BowArrowRainAttackState;
    if (state != null)
        state.LockIndicatorPosition();
}
public void LockMacePulseIndicator()
{
    var state = _playerController.CurrentState as Player.MacePulseAttackState;
    if (state != null)
        state.LockIndicatorPosition();
}
public void ArrowRainIndicatorHide()
{
    var state = _playerController.CurrentState as Player.BowArrowRainAttackState;
    if (state != null)
    {
        state.DestroyIndicator();
    }
}
public void ArrowOneShotIndicatorHide()
{
    var state = _playerController.CurrentState as Player.BowOneShotAttackState;
    if (state != null)
    {
        state.DestroyIndicator();
    }
}
public void AxeThrowIndicatorHide()
{
    var state = _playerController.CurrentState as Player.AxeThrowAttackState;
    if (state != null)
    {
        state.DestroyIndicator();
    }
}

public void MacePulseIndicatorHide()
{
    var state = _playerController.CurrentState as Player.MacePulseAttackState;
    if (state != null)
    {
        state.DestroyIndicator();
    }
}
*/