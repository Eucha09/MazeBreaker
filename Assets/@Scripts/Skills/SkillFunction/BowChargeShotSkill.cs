using UnityEngine;

public class BowChargeShotSkill : MonoBehaviour
{
    float _distance;
    Vector3 _startPos;
    Vector3 _currentPos;

    public void Init(float dist)
    {
        _distance = dist;
        _startPos = transform.position;
    }

    public void FixedUpdate()
    {
        _currentPos = transform.position;
        //실수 값은 오차 값
        if (Vector3.Distance(_startPos, _currentPos) >= _distance)
        {
            GameObject a = Managers.Resource.Instantiate("Effects/Player/BowDestroy",transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
