using UnityEngine;
using UnityEngine.AI;



namespace TreeMonster.Search
{
    //Idle상태 이후 Look Around후 3초뒤 새로운 목표지점을 찍는다. 도착할 경우 Idle로 전환
    public class Idle : SearchState
    {
        float _idleStartTime=0f;
        float _idleDuration = 0f;

        public Idle(float idleDuration)
        {
            _idleDuration = idleDuration;  
        }

        public override void EnterState(TreeMonsterController monster)
        {
            base.EnterState(monster);

            //_monster.BaseMat.SetFloat("_UseEmission", 0.0f);
            _idleStartTime = Time.time;
            _monster.Ani.Play("Idle");
        }

        public override void UpdateState()
        {
            base.UpdateState();
            TargetInRangeStateCheck();
            /*
            if (Time.time - _idleStartTime > 3f)
            {
                _monster.CurrentState = new LookAround();
            }
            */
        }

        public override void ExitState()
        {
            base.ExitState();
        }
        public override void OnDamaged(DamageCollider dm)
        {
            base.OnDamaged(dm);
            _monster.TargetDetectionTime = Time.time;
            if (dm.Caster != null)
                _monster.MainTarget = dm.Caster.transform;
            _monster.CurrentState = new TreeMonster.Attack.Hurt();
        }
    }

    public class LookAround : SearchState
    {
        public bool isJumpStart = false;
        public override void EnterState(TreeMonsterController monster)
        {
            IsAnimationEnd = false;
            base.EnterState(monster);
            randomDirection = GetRandomDirection(_monster.transform);
            _monster.Ani.CrossFade("JumpAttack",0.05f);
        }

        public override void UpdateState()
        {
            base.UpdateState();
            TargetInRangeStateCheck();
            if (isLooking)
            {
                LookRandomDirection();
            }
            if(IsAnimationEnd)
                _monster.CurrentState = new Idle(Random.Range(3.0f, 5.0f));
        }

        public override void ExitState()
        {
            base.ExitState();
        }

        public Vector3 GetRandomDirection(Transform transform)
        {
            // ±90도 이상 360도 이하의 무작위 각도를 생성합니다.
            float randomAngle = Random.Range(90f, 270f);

            // 각도를 무작위로 ±로 변환 (90도 혹은 -90도 이상의 각도)
            if (Random.value < 0.5f)
            {
                randomAngle = -randomAngle;
            }

            // 현재 Transform의 forward 방향을 기준으로 Y축을 중심으로 회전합니다.
            Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.up);
            Vector3 newDirection = rotation * transform.forward;

            return newDirection.normalized; // 단위 벡터로 반환
        }

        public override void OnDamaged(DamageCollider dm)
        {
            base.OnDamaged(dm);
            _monster.TargetDetectionTime = Time.time;
            if (dm.Caster != null)
                _monster.MainTarget = dm.Caster.transform;
            _monster.CurrentState = new TreeMonster.Attack.Hurt();
        }
    }

    public class Dead : SearchState
    {
        public override void EnterState(TreeMonsterController monster)
        {
            base.EnterState(monster);
            _monster.Ani.CrossFade("Dead", 0.05f);
        }

        public override void UpdateState()
        {
        }
    }

}