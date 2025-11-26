using UnityEngine;
using System.Linq;


namespace StoneGolemPatternsInfo
{

    public class Paze1RoarInfo : PatternInfo, ResetPatternData
    {
        float _patternMaxDist;
        public bool IsPatternUsed = false;

        public Paze1RoarInfo(MonsterController2 patternUser, float patternCoolTime, float patternMaxDistance)
        {
            _patternUser = patternUser;
            _patternCoolTime = patternCoolTime;
            _patternName = "Paze1Roar";
            _patternMaxDist = patternMaxDistance;
            IsPatternUsed = false;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (_patternUser.Hp > _patternUser.MaxHp / 2 && !IsPatternUsed)// 쿨타임 체크
            {
                Debug.Log("Paze1!");
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            IsPatternUsed = true;
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.RoarPattern.Roar());
            _patternUser.CurrentPatternInfo = this;
        }

        public void Reset()
        {
            IsPatternUsed = false;
        }
    }
    public class WalkPatternInfo : PatternInfo
    {
        float _dist;

        public WalkPatternInfo(MonsterController2 patternUser, float stopDist)
        {
            _patternUser = patternUser;
            _patternName = "Walk";
            _dist = stopDist;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) > _dist)
                return true;
            else return false;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.WalkPattern.MoveToTarget(_dist));
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class OrbitMovePatternInfo : PatternInfo
    {

        public OrbitMovePatternInfo(MonsterController2 patternUser)
        {
            _patternUser = patternUser;
            _patternName = "OrbitMove";
            _lastPatterUsedTime = int.MinValue;    //시작하자마자 쿨타임 0
        }

        public override bool PatternConditionCheck()
        {

            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            else
                return true;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.OrbitMovePattern.OrbitAroundPlayer());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class SmartOrbitMovePatternInfo : PatternInfo
    {

        public SmartOrbitMovePatternInfo(MonsterController2 patternUser)
        {
            _patternUser = patternUser;
            _patternName = "SmartOrbitMove";
            _lastPatterUsedTime = int.MinValue;    //시작하자마자 쿨타임 0
        }

        public override bool PatternConditionCheck()
        {

            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            else
                return true;
        }

        public override void UsePattern()
        {
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.SmartOrbitMovePattern.SmartOrbitAroundPlayer());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class BitePatternInfo : PatternInfo
    {
        float _attackDist;

        public BitePatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "Bite";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.BitePattern.Bite());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class StoneUpPatternInfo : PatternInfo
    {
        float _attackDist;

        public StoneUpPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "StoneUp";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.StoneUpPattern.StoneUp());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class StoneWallPatternInfo : PatternInfo
    {
        float _attackDist;

        public StoneWallPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "StoneWall";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.StoneWallPattern.StoneWall());
            _patternUser.CurrentPatternInfo = this;
        }
    }

    public class StoneWallPaze2PatternInfo : PatternInfo
    {
        float _attackDist;

        public StoneWallPaze2PatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "StoneWallPaze2";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.StoneWallPaze2Pattern.StoneWallPaze2());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class Ground3HitPatternInfo : PatternInfo
    {
        float _attackDist;

        public Ground3HitPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "Ground3Hit";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.Ground3HitPattern.Ground3Hit());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class StonePillarPatternInfo : PatternInfo
    {
        float _attackDist;

        public StonePillarPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "StonePillar";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            // 💬 패턴 사용자(보스)를 StoneGolemController로 캐스팅
            var controller = _patternUser as StoneGolemController;

            // 💬 현재 패턴이 이미 실행 중이면 다시 실행하지 않음
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;

            // 💬 비석 중 하나라도 살아있으면 절대 재실행 안 함
            if (controller.linkedPillars.Any(p => p != null && !p.IsDead))
                return false;

            // 💬 비석이 모두 부서진 후 아직 쿨타임 60초가 지나지 않았다면 false
            if (Time.time - controller.lastPillarClearTime < 60f)
                return false;


            // 💬 거리 조건 만족하면 실행 가능
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.StonePillarPattern.StonePillar());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class JumpAttackPatternInfo : PatternInfo
    {
        float _attackDist;

        public JumpAttackPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "JumpAttack";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.JumpAttackPattern.JumpAttack());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class ThrowStonePatternInfo : PatternInfo
    {
        float _attackDist;

        public ThrowStonePatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "ThrowStone";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.ThrowStonePattern.ThrowStone());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class WindPatternInfo : PatternInfo
    {
        float _attackDist;

        public WindPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "Wind";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.WindPattern.Wind());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class SpawnMonsterPatternInfo : PatternInfo
    {
        float _attackDist;

        public SpawnMonsterPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "SpawnMonster";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.SpawnMonsterPattern.SpawnMonster());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class TurntoSpherePatternInfo : PatternInfo
    {
        float _attackDist;

        public TurntoSpherePatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "TurntoSphere";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.TurntoSpherePattern.TurntoSphere());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class RollPatternInfo : PatternInfo
    {
        float _attackDist;

        public RollPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "Roll";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.RollPattern.Roll());
            _patternUser.CurrentPatternInfo = this;
        }
    }



    public class DashAttackPatternInfo : PatternInfo
    {
        float _attackDist;

        public DashAttackPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "PoisonBullet";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new WolfAttackPatterns.DashAttackPattern.DashAttack());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class ThunderAttackPatternInfo : PatternInfo
    {
        float _attackDist;

        public ThunderAttackPatternInfo(MonsterController2 patternUser, float attackDist, float coolTime)
        {
            _patternUser = patternUser;
            _patternName = "ThunderAttack";
            _attackDist = attackDist;
            _patternCoolTime = coolTime;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (GetDistanceToTarget(_patternUser.MainTarget) < _attackDist
                && RemainingCoolTime() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void UsePattern()
        {
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.ThunderAttackPattern.ThunderAttack());
            _patternUser.CurrentPatternInfo = this;
        }
    }
    public class Paze2PatternInfo : PatternInfo
    {
        float _patternDist;
        public bool IsPatternUsed = false;

        public Paze2PatternInfo(MonsterController2 patternUser, float patternCoolTime, float patternDistance)
        {
            _patternName = "Paze2";
            _patternUser = patternUser;
            _patternDist = patternDistance;
            _patternCoolTime = patternCoolTime;
            IsPatternUsed = false;
        }

        public override bool PatternConditionCheck()
        {
            if (_patternUser.CurrentPatternInfo.PatternName == _patternName)
                return false;
            if (_patternUser.Hp < _patternUser.MaxHp / 2 && !IsPatternUsed)// 쿨타임 체크
            {
                Debug.Log("HP절반!");
                return true;
            }
            return false;
        }

        public override void UsePattern()
        {
            IsPatternUsed = true;
            _lastPatterUsedTime = Time.time;
            _patternUser.CurrentState.ChangeState(new StoneGolemAttackPatterns.Paze2Pattern.Paze2());
            _patternUser.CurrentPatternInfo = this;
        }

    }



}