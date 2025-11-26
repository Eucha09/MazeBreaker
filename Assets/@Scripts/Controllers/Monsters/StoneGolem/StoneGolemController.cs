using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public class StoneGolemController : MonsterController2
{

    public List<NatureController> linkedPillars = new(); // 비석 리스트
    public GameObject shieldAura;                        // 쉴드 이펙트 저장용
    public bool IsShielded => linkedPillars.Any(p => p != null && !p.IsDead); // 하나라도 살아있으면 쉴드 유지
    public float lastPillarClearTime = -999f;
    private bool _prevShielded = false;

    protected override void Start()
    {

        base.Start();
        //초기 상태
        CurrentState = new Monster.Search.Idle();

 
        // Managers.UI.MakeWorldSpaceUI<UI_HpBar>(transform).SetInfo(this, 1.8f);
        _patterns.Add(new StoneGolemPatternsInfo.Paze1RoarInfo(this, 0, 0));
        _patterns.Add(new StoneGolemPatternsInfo.Paze2PatternInfo(this, 0, 0));

        _patterns.Add(new StoneGolemPatternsInfo.BitePatternInfo(this, 3, 15));   //6초
        _patterns.Add(new StoneGolemPatternsInfo.StoneUpPatternInfo(this, 10, 15));
        _patterns.Add(new StoneGolemPatternsInfo.StoneWallPatternInfo(this, 10, 30));

        _patterns.Add(new StoneGolemPatternsInfo.WalkPatternInfo(this, 3));
        //_patterns.Add(new StoneGolemPatternsInfo.SmartOrbitMovePatternInfo(this));

    }
    private void Update()
    {
        // 무적 상태가 변화했을 때만 반응
        if (IsShielded != _prevShielded)
        {
            if (IsShielded)
            {
                // ✅ 보스가 무적 상태가 되었을 때 실행
                Debug.Log("🛡보스 무적 상태 진입!");
                LerpAttackGlowCoroutineStart();
                // 여기에 너의 Glow 활성화 함수 호출
            }
            else
            {
                // ✅ 보스가 무적 상태에서 해제되었을 때 실행
                Debug.Log("❌보스 무적 상태 해제!");
                LerpAttackGlowCoroutineEnd();
                // 여기에 너의 Glow 비활성화 함수 호출
            }

            _prevShielded = IsShielded;
        }

        if (!IsShielded && shieldAura != null)
        {
            Destroy(shieldAura);
            shieldAura = null;
            Debug.Log("보호막 해제됨!");
        }
        // 🕐 비석이 모두 파괴된 순간 → 시간 기록
        if (linkedPillars.Count > 0 && linkedPillars.All(p => p == null || p.IsDead))
        {
            if (lastPillarClearTime < 0f)
            {
                Debug.Log("💥 모든 비석 파괴됨! 재사용 쿨다운 시작");
                lastPillarClearTime = Time.time;
            }
        }

        // 🔄 비석이 다시 살아나면 타이머 초기화
        if (linkedPillars.Any(p => p != null && !p.IsDead))
        {
            lastPillarClearTime = -999f;
        }
    }
    public override void OnDamaged(DamageCollider dm)
    {
        CurrentState.OnDamaged(dm);
    
        UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
        ui.TargetBar.SetInfo(this);
    }
}
