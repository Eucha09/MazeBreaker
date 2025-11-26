using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class PatternedWall : MazeWall
{
    public void Push(Vector3 dir)
    {
        if (_coMove != null)
            return;

        dir.y = 0.0f;
        if (transform.rotation.y == 0.0f)
            dir.x = 0.0f;
        else
            dir.z = 0.0f;

        _coMove = StartCoroutine(CoPushAndRestore(dir));
    }

    public void Up()
    {
        if (_coMove != null)
            return;

        _coMove = StartCoroutine(CoUpAndRestore());
    }

    public void Down()
    {
        if (_coMove != null)
            return;

        _coMove = StartCoroutine(CoDownAndRestore());
    }

    public void Fall(Vector3 dir)
    {
        if (_coMove != null)
            return;

        Vector3 localDir = _wallObject.transform.InverseTransformDirection(dir);
        localDir.y = 0;
        localDir.x = 0;

        _coMove = StartCoroutine(CoFallAndRestore(localDir));
    }

    IEnumerator CoPushAndRestore(Vector3 dir)
    {
        while (true)
        {
            //// Shake
            //float shakeX = Random.Range(-_shakeAmount, _shakeAmount);
            //float shakeZ = Random.Range(-_shakeAmount, _shakeAmount);
            //_movingObjects[i].transform.localPosition += new Vector3(shakeX, 0, shakeZ) * Time.deltaTime;

            _wallObject.transform.position += dir.normalized * 3.0f * Time.deltaTime;


            BoxCollider boxCollider = _wallObject.GetComponent<BoxCollider>();
            Vector3 center = _wallObject.transform.position + _wallObject.transform.up * boxCollider.center.y;
            Vector3 halfExtents = boxCollider.size * 0.49f;
            int layerMask = (1 << (int)Define.Layer.Block) | (1 << (int)Define.Layer.Ground);
            Collider[] hits = Physics.OverlapBox(center, halfExtents, _wallObject.transform.rotation);
            bool collision = false;
            foreach (Collider hit in hits)
            {
                if (hit.gameObject == _wallObject)
                    continue;
                //if (hit.GetComponent<BaseController>())
                //    hit.GetComponent<BaseController>().OnDamaged(100.0f, null);
                if (((1 << (int)hit.gameObject.layer) & layerMask) > 0)
                    collision = true;
            }
            if (collision)
                break;

            yield return null;
        }
        yield return new WaitForSeconds(5.0f);
        yield return StartCoroutine(CoRestore());
        _coMove = null;
    }

    IEnumerator CoUpAndRestore()
    {
        Managers.Sound.Play3DSound(gameObject, "WallChange", 0.0f, 20.0f);
        while (true)
        {
            // Shake
            float shakeX = Random.Range(-_shakeAmount, _shakeAmount);
            float shakeZ = Random.Range(-_shakeAmount, _shakeAmount);
            _wallObject.transform.localPosition += new Vector3(shakeX, 0, shakeZ) * 0.02f;

            Vector3 dir = _upPos - _wallObject.transform.localPosition;
            _wallObject.transform.localPosition += dir.normalized * _speed * Time.deltaTime;
            if (Vector3.Distance(_wallObject.transform.localPosition, _upPos) < _speed * Time.deltaTime)
            {
                _wallObject.transform.localPosition = _upPos;
                break;
            }

            yield return null;
        }
        yield return new WaitForSeconds(5.0f);
        yield return StartCoroutine(CoRestore());
        _coMove = null;
    }

    IEnumerator CoDownAndRestore()
    {
        Managers.Sound.Play3DSound(gameObject, "WallChange", 0.0f, 20.0f);
        while (true)
        {
            // Shake
            float shakeX = Random.Range(-_shakeAmount, _shakeAmount);
            float shakeZ = Random.Range(-_shakeAmount, _shakeAmount);
            _wallObject.transform.localPosition += new Vector3(shakeX, 0, shakeZ) * 0.02f;

            Vector3 dir = _downPos - _wallObject.transform.localPosition;
            _wallObject.transform.localPosition += dir.normalized * _speed * Time.deltaTime;
            if (Vector3.Distance(_wallObject.transform.localPosition, _downPos) < _speed * Time.deltaTime)
            {
                _wallObject.transform.localPosition = _downPos;
                break;
            }

            yield return null;
        }
        yield return new WaitForSeconds(5.0f);
        yield return StartCoroutine(CoRestore());
        _coMove = null;
    }

    IEnumerator CoFallAndRestore(Vector3 localDir)
    {
        float speed = 0.0f;
        float angle = 0.0f;
        float radius = _wallObject.GetComponent<BoxCollider>().size.z / 2;
        Vector3 pivot = _wallObject.transform.localPosition + localDir.normalized * radius;

        while (angle < 120)
        {
            angle += speed * Time.deltaTime;
            speed += 9.81f * Time.deltaTime;
            float radian = angle * Mathf.Deg2Rad;

            Vector3 newPos = pivot - localDir.normalized * Mathf.Cos(radian) * radius;
            newPos.y = pivot.y + Mathf.Sin(radian) * radius;
            _wallObject.transform.localPosition = newPos;

            Vector3 rightVector = Vector3.Cross(localDir, Vector3.up);
            _wallObject.transform.localRotation = Quaternion.AngleAxis(-angle, rightVector);

            BoxCollider boxCollider = _wallObject.GetComponent<BoxCollider>();
            Vector3 center = _wallObject.transform.position + _wallObject.transform.up * boxCollider.center.y;
            Vector3 halfExtents = boxCollider.size * 0.49f;
            int layerMask = (1 << (int)Define.Layer.Block) | (1 << (int)Define.Layer.Ground);
            Collider[] hits = Physics.OverlapBox(center, halfExtents, _wallObject.transform.rotation);
            bool collision = false;
            foreach (Collider hit in hits)
            {
                if (hit.gameObject == _wallObject)
                    continue;
                //if (hit.GetComponent<BaseController>())
                //    hit.GetComponent<BaseController>().OnDamaged(100.0f, null);
                if (((1 << (int)hit.gameObject.layer) & layerMask) > 0)
                    collision = true;
            }
            if (collision)
                break;
            yield return null;
        }
        yield return new WaitForSeconds(5.0f);
        yield return StartCoroutine(CoRestore());
        _coMove = null;
    }
}
