using Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using static Define;

public class NightMare : MonsterController
{
    GameObject _player;

    float _groggyTime;

    [SerializeField]
    Transform _rightHand;
    bool _isHoldOn;

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();

        if (State == CreatureState.Skill)
        {
            _animator.CrossFade("SKILL", 0.1f, -1, 0);
        }
    }

    protected override void Init()
    {
        ObjectData objectData = null;
        Managers.Data.ObjectDict.TryGetValue(TemplateId, out objectData);
        ObjectType = objectData.objectType;
        MaterialType = objectData.materialType;
        Stat = objectData.stat;
        Hp = objectData.stat.maxHp;
        State = CreatureState.Idle;
        MoveSpeed = 4.0f;

		_animator = GetComponent<Animator>();
        _nma = GetComponent<NavMeshAgent>();
        _destPos = transform.position;

        Managers.Event.TimeEvents.DayAction += OnDay;
        _player = Managers.Object.GetPlayer().gameObject;


		Renderer meshRenderer = GetComponentInChildren<Renderer>();
		Material[] materials = meshRenderer.materials;

		for (int i = 0; i < materials.Length; i++)
		{
			meshRenderer.materials[i] = Instantiate(materials[i]);
		}

		BaseMat = materials[0];
	}

    protected override void UpdateController()
    {
        base.UpdateController();

    }

    protected override void UpdateIdle()
    {
        if (_groggyTime > 0.0f)
        {
            _groggyTime -= Time.deltaTime;
            return;
        }

        if (_target != null)
        {
            MoveToNextPos();
            State = CreatureState.Run;          // chase the Player
            return;
        }

        float distance = (_destPos - transform.position).magnitude;
        if (distance > 2.0f)
        {
            MoveToNextPos();
            State = CreatureState.Moving;         // Walk around random Area
            return;
        }

        if (_coPatrol == null)
            _coPatrol = StartCoroutine("CoPatrol");
        if (_coSearch == null)
            _coSearch = StartCoroutine("CoSearch");
    }

    protected override void UpdateMoving()
    {
        if (_target != null)
        {
            MoveToNextPos();
            State = CreatureState.Run;
            return;
        }

        if (_nma.remainingDistance < 2.0f || _nma.remainingDistance > 25.0f)
        {
            MoveToNextPos();
            return;
        }

        if (_coSearch == null)
            _coSearch = StartCoroutine("CoSearch");
    }

    float _undetectedTime = 0.0f;

    protected override void UpdateRun()
    {
        if (_target == null || _undetectedTime > 10.0f)
        {
            _undetectedTime = 0.0f;
            _nma.SetDestination(transform.position);
            _target = null;
            _destPos = transform.position;
            State = CreatureState.Idle;
            return;
        }

        if (CanUseSkill())
        {
            _attackDir = _target.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(_attackDir.normalized);
            State = CreatureState.Skill;
            _coSkill = StartCoroutine("CoStartAttack");
            return;
        }

        Vector3 targetDir = (_target.transform.position - transform.position);
        Debug.DrawRay(transform.position + Vector3.up * 0.5f, targetDir, Color.green);
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, targetDir, targetDir.magnitude, LayerMask.GetMask("Block")))
            _undetectedTime += Time.deltaTime;
        else
            _undetectedTime = 0.0f;

        if (targetDir.magnitude <= 11.0f)
        {
            _nma.SetDestination(_target.transform.position);
        }

        if (_nma.remainingDistance < 2.0f || _nma.remainingDistance > 25.0f)
        {
            MoveToNextPos();
            return;
        }
    }

    protected void MoveToNextPos()
    {
        Vector2Int gridPos = Managers.Map.WorldToGrid(transform.position);
        Vector2Int destGridPos = Managers.Map.WorldToGrid(_destPos);
        if (_target != null)
        {
            destGridPos = Managers.Map.WorldToGrid(_target.transform.position);
        }

        List<Vector2Int> path = Managers.Map.FindPath(gridPos, destGridPos, false, 20);
        if (path == null) // TODO
            _target = null;
        if (path == null || path.Count < 2)
        {
            _nma.SetDestination(Managers.Map.GridToWorld(gridPos));
            _destPos = Managers.Map.GridToWorld(gridPos);
            if (_target == null)
                State = CreatureState.Idle;
            return;
        }

        Vector2Int nextPos = path[2];

        _nma.SetDestination(Managers.Map.GridToWorld(nextPos));
        if (_target == null)
            _nma.speed = MoveSpeed;
        else
            _nma.speed = MoveSpeed * 2;
    }

    protected override void UpdateStunned()
    {
        if (_groggyTime <= 0.0f)
        {
            State = CreatureState.Idle;
            return;
        }

        _groggyTime -= Time.deltaTime;
    }

    Vector3 _attackDir;

    protected override void UpdateSkill()
    {
        if (_coSkill == null)
        {
            if (_isHoldOn)
            {
                _isHoldOn = false;
                // player die
                // TEMP
                _target.GetComponent<PlayerController>().OnRelease();
            }
            else
            {
                _groggyTime = 1.0f;
                _nma.updateRotation = true;
                _target = null;
                State = CreatureState.Idle;
            }
            return;
        }

        //if (_isHoldOn && _target.GetComponent<PlayerControllerV1>().isGrabbed == false)
        //{
        //    _isHoldOn = false;
        //    if (_coSkill != null)
        //    {
        //        StopCoroutine(_coSkill);
        //        _coSkill = null;
        //    }
        //    _groggyTime = 2.0f;
        //    _nma.updateRotation = true;
        //    State = CreatureState.Stunned;
        //    return;
        //}

        if (_isHoldOn)
        {
            _target.transform.position = _rightHand.position;
            _target.transform.rotation = Quaternion.LookRotation(transform.position - _target.transform.position);
        }
    }

    protected override bool CanUseSkill()
    {
        if (_target == null)
            return false;

        Vector3 dir = _target.transform.position - transform.position;
        if (dir.magnitude <= 6.0f)
        {
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir, Color.green);
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, dir.magnitude, LayerMask.GetMask("Block")))
            {
                return true;
            }
        }

        return false;
    }

    IEnumerator CoStartAttack()
    {
        _nma.ResetPath();
        _nma.updateRotation = false;
        yield return new WaitForSeconds(0.3f);

        Player.PlayerStateType state = _target.GetComponent<PlayerController>().CurrentState.GetState();
        Vector3 dir = _target.transform.position - transform.position;
        if (dir.magnitude <= 7.0f && state != Player.PlayerStateType.Grabbed && state != Player.PlayerStateType.Dead)
        {
            _isHoldOn = true;
            //_target.GetComponent<BaseController>().OnDamaged(10000.0f, this);
            //_target.GetComponent<BaseController>().OnMentalDamaged(10000.0f);
            _target.GetComponent<PlayerController>().OnGrabbed();
            //if (_target.GetComponent<PlayerStat>().Mental <= 0.0f)
            //  Managers.Sound.Play("MonsterSound/QuaJik");
            yield return new WaitForSeconds(3.5f);

            //_target.GetComponent<PlayerController>().OnDamaged(10000.0f, this);


        }

        _coSkill = null;
    }

    public void OnDay()
    {
		Managers.Event.TimeEvents.DayAction -= OnDay;
		StartCoroutine(CoLerpDissolve());
    }

    protected override IEnumerator CoPatrol()
    {
        while (true)
        {
            Vector2Int gridPos = Managers.Map.WorldToGrid(transform.position);
            List<Vector2Int> list = Managers.Map.BFS(gridPos, 5);

            if (list.Count > 0)
            {
                int rand = Random.Range(0, list.Count);
                Vector3 pos = Managers.Map.GridToWorld(list[rand]);
                _destPos = pos;
                yield break;
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    protected override IEnumerator CoSearch()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (_target != null)
                continue;

            if (_player.GetComponent<PlayerController>().CurrentState.ToString() == "Player.PlayerDeadState")
                continue;

            Vector3 dir = (_player.transform.position - transform.position);
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir, Color.green);
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, dir.magnitude, LayerMask.GetMask("Block")))
            {
                _target = _player;
            }
            else if (dir.magnitude <= 18.0f * 3)
                PlayCrySound();
        }
    }

    float _lastTimeCrySound;
    void PlayCrySound()
    {
        if (Time.time - _lastTimeCrySound > 6.0f)
        {
            Managers.Sound.Play3DSound(gameObject, "NightSound/Crow_Breathe", 0.0f, 54.0f);
            _lastTimeCrySound = Time.time;
        }
    }

    public bool IsChasingPlayer()
    {
        return _target != null;
    }

	IEnumerator CoLerpDissolve()
	{
		float elapsedTime = 0f;

		while (elapsedTime < 2.0f)
		{
			BaseMat.SetFloat("_DissolveAmount", Mathf.Lerp(0, 1f, elapsedTime / 2.0f));
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		Managers.Object.Remove(gameObject);
	}
}
