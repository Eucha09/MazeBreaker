using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FantasticAnimal : MonoBehaviour
{
    [SerializeField]
    List<Vector2Int> _path;
    int _idx = 0;
    [SerializeField]
    Vector3 _destPos;

    public Vector3 DestPos { set { _destPos = value; } }

    NavMeshAgent _nma;
    Animator _anim;
    [SerializeField]
    float _speed = 4.0f;

    [SerializeField]
    bool _moving;

    [Space]
    public float erodeOutRate = 0.03f;
    public float erodeRefreshRate = 0.01f;
    public List<SkinnedMeshRenderer> objectsToErode = new List<SkinnedMeshRenderer>();

    void Start()
    {
        _nma = GetComponent<NavMeshAgent>();
        _anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (_moving == false)
            return;

        Vector3 dir = _destPos - transform.position;
        if (dir.magnitude < 1.0f && _idx < _path.Count)
        {
            _idx++;
            if (_idx == _path.Count)
            {
                if (objectsToErode != null)
                    StartCoroutine(ErodeObjects());
                GameObject.Destroy(gameObject, 1.0f);
                return;
            }
            _destPos = Managers.Map.GridToWorld(_path[_idx]);
            _nma.SetDestination(_destPos);
        }
    }

    public void SetDestPosition(Vector3 startPos, Vector3 destPos)
    {
        _nma.speed = _speed;

        Vector2Int startGridPos = Managers.Map.WorldToGrid(startPos);
        Vector2Int destGridPos = Managers.Map.WorldToGrid(destPos);

        _path = Managers.Map.FindPath(startGridPos, destGridPos);
        if (_path == null)
            return;

        _idx = 0;
        _destPos = Managers.Map.GridToWorld(_path[_idx]);
        _nma.SetDestination(_destPos);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && _moving == false)
        {
            SetDestPosition(transform.position, _destPos);
            _moving = true;
            _anim.CrossFade("RUN", 0.1f);
        }
    }

    IEnumerator ErodeObjects()
    {
        for (int i = 0; i < objectsToErode.Count; i++)
        {
            float t = 0;
            while (t < 1)
            {
                t += erodeOutRate;
                for (int j = 0; j < objectsToErode[i].materials.Length; j++)
                {
                    objectsToErode[i].materials[j].SetFloat("_Erode", t);
                }
                yield return new WaitForSeconds(erodeRefreshRate);
            }
        }
    }
}
