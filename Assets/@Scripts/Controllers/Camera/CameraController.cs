using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    Define.CameraMode _mode = Define.CameraMode.QuarterView;
    public Define.CameraMode Mode { get { return _mode; } }

    GameObject _cameraPivot;

    [SerializeField]
    LayerMask _wallMask;

    [SerializeField]
    float _quarterViewDistance = 8.0f;
    [SerializeField]
    float _quarterViewHeight = 16.0f;

    [SerializeField]
    float _currentQuarterViewDistance = 8.0f;
    [SerializeField]
    float _currentQuarterViewHeight = 16.0f;

    float _smoothTime = 0.1f;
    Vector3 _velocity = Vector3.zero;

    public float CurrentRotationAngle { get; private set; } = 45.0f;
    float _rotationSmoothTime = 0.12f;
    float _rotationVelocity;

    GameObject _player = null;
    GameObject _forward;

    // 추가된 변수
    bool _isShaking = false;
    float _shakeDuration = 0.0f;
    float _shakeMagnitude = 0.1f;

    public float value = 0f;          // 조절할 float 값
    public float scrollSpeed = 1f;    // 마우스 휠의 스크롤 속도


    public void SetPlayer(GameObject player)
    {
        _player = player;
        _forward = Util.FindChild(player, "RunCameraPos");
    }

    void Start()
    {
        Managers.Input.KeyAction -= OnKeyboard;
        Managers.Input.KeyAction += OnKeyboard;


        _player = Managers.Object.GetPlayer().gameObject;
        _cameraPivot = new GameObject { name = "CameraPivot" };
        SetCameraMode(Define.CameraMode.QuarterView);
    }
    float currentValue;
    Vector3 offset;

    private List<Renderer> _previouslyHitObjects = new List<Renderer>();
    private HashSet<Renderer> _currentFrameHitObjects = new HashSet<Renderer>();
    void LateUpdate()
    {
        Vector2 cutoutPos = Camera.main.WorldToViewportPoint(_player.transform.position);
        cutoutPos.y /= (Screen.width / Screen.height);
        Vector3 offset = _player.transform.position + new Vector3(0,0.5f,0) - transform.position;
        RaycastHit[] hitObjects = Physics.RaycastAll(transform.position, offset, offset.magnitude, _wallMask);

        _currentFrameHitObjects.Clear();

        for (int i = 0; i < hitObjects.Length; i++)
        {
            Renderer[] renderers = hitObjects[i].transform.GetComponentsInChildren<Renderer>();
            if (renderers != null)
            {
                for (int j = 0; j < renderers.Length; j++)
                {
                    Renderer currentRenderer = renderers[j];

                    // previouslyHitObjects에 포함되어 있는지 확인
                    if (!_previouslyHitObjects.Contains(currentRenderer))
                    {
                        Material[] materials = currentRenderer.materials;
                        for (int m = 0; m < materials.Length; ++m)
                        {
                            if (materials[m].HasProperty("_CutOutPosition"))
                            {
                                Debug.Log("으아아아아아앙아");
                                materials[m].SetVector("_CutOutPosition", cutoutPos);
                                materials[m].SetFloat("_CutOutSize", 0.3f);
                                materials[m].SetFloat("_FallOffSize", 0.15f);
                            }
                        }
                    }

                    // 현재 프레임에서 히트된 오브젝트를 추가
                    _currentFrameHitObjects.Add(currentRenderer);
                }
            }
        }

        // 이전 프레임에서 가리고 있었지만, 이번 프레임에서 가리지 않는 오브젝트들 처리
        foreach (Renderer renderer in _previouslyHitObjects)
        {
            if (!_currentFrameHitObjects.Contains(renderer))
            {
                Material[] materials = renderer.materials;
                for (int m = 0; m < materials.Length; ++m)
                {
                    materials[m].SetFloat("_CutOutSize", 0.0f);
                    materials[m].SetFloat("_FallOffSize", 0.0f);
                }
            }
        }

        // 현재 프레임의 가린 오브젝트들을 이전 프레임 기록으로 업데이트
        _previouslyHitObjects.Clear();
        _previouslyHitObjects.AddRange(_currentFrameHitObjects);


        if (_mode == Define.CameraMode.QuarterView)
        {
            // 마우스 휠 입력 감지
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            // 스크롤 입력에 따라 value 값을 조정
            value += scrollInput * scrollSpeed;

            // value 값을 특정 범위로 제한하고 싶다면, 아래 주석을 해제하세요.
            value = Mathf.Clamp(value, 0, 8);

            currentValue = Mathf.Lerp(currentValue, value, Time.deltaTime * 5f);



            float desiredAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, CurrentRotationAngle, ref _rotationVelocity, _rotationSmoothTime);
            Quaternion currentRotation = Quaternion.Euler(0, desiredAngle, 0);

            offset = (Vector3.up * (_currentQuarterViewHeight + currentValue)) - (currentRotation * Vector3.forward * _currentQuarterViewDistance);


            if (_player.GetComponent<PlayerController>().CurrentState.GetType() == typeof(Player.PlayerSprintState))
            {
                // 플레이어의 전방 10 지점 계산
                Vector3 sprintCameraPivot = _player.transform.position + _player.transform.forward * 5f;

                // SmoothDamp를 사용하여 카메라 피벗을 목표 위치로 이동
                _cameraPivot.transform.position = Vector3.SmoothDamp(_cameraPivot.transform.position, sprintCameraPivot, ref _velocity, _smoothTime);
            }
            else
            {
                _cameraPivot.transform.position = Vector3.SmoothDamp(_cameraPivot.transform.position, _player.transform.position, ref _velocity, _smoothTime);
            }
                //if (_pc.currentState == StrategyState.DefaultState)
                //{
                
            //}
            //else if (_pc.currentState == StrategyState.HookMoveState)
            //{
            //    _cameraPivot.transform.position = Vector3.SmoothDamp(_quarterViewFollow.transform.position, _forward.transform.position, ref _velocity, _smoothTime * (_phm.defaultSpeed / _phm.speed));
            //}
            // 카메라 위치 계산
            Vector3 targetPosition = _cameraPivot.transform.position + offset;

            if (_isShaking)
            {
                targetPosition += Random.insideUnitSphere * _shakeMagnitude;
            }

            transform.position = targetPosition;
            //transform.LookAt(_cameraPivot.transform);

            // 카메라 회전 설정 (LookAt 대신 수동으로)
            Vector3 lookDirection = (_cameraPivot.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _rotationSmoothTime*3);
            transform.rotation = lookRotation;
        }
    }

    public void SetCameraMode(Define.CameraMode mode)
    {
        _mode = mode;
        if (_mode == Define.CameraMode.QuarterView)
        {
            _cameraPivot.transform.position = _player.transform.position;
           
        }
    }

    public void OnKeyboard()
    {

        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    CurrentRotationAngle += 45.0f; // 오른쪽 화살표 입력 시 45도 회전
        //}
        //else if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    CurrentRotationAngle -= 45.0f; // 왼쪽 화살표 입력 시 -45도 회전
        //}


    }


    Coroutine _zoomCoroutin = null;

    public void ZoomOutActionCoroutin(float duration, float dist)
    {
        if (_zoomCoroutin != null)
            StopCoroutine(_zoomCoroutin);
        _zoomCoroutin = StartCoroutine(ZoomOutAction(duration,dist));
        
    }

    IEnumerator ZoomOutAction(float maxChargeTime, float dist)
    {
        float startDistance = _currentQuarterViewDistance;
        float startHeight = _currentQuarterViewHeight;
        float endDistance = startDistance + dist;
        float endHeight = startHeight + dist*(startHeight/ startDistance);
        float elapsedTime = 0f;

        while (elapsedTime < maxChargeTime)
        {
            _currentQuarterViewDistance = Mathf.Lerp(startDistance, endDistance, elapsedTime / maxChargeTime);
            _currentQuarterViewHeight = Mathf.Lerp(startHeight, endHeight, elapsedTime / maxChargeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _currentQuarterViewDistance = endDistance;
        _currentQuarterViewHeight = endHeight;
    }

    public void ZoomInActionCoroutin(float duration)
    {
        if (_zoomCoroutin != null)
            StopCoroutine(_zoomCoroutin);
        _zoomCoroutin = StartCoroutine(ZoomInAction(duration));
    }

    IEnumerator ZoomInAction(float duration)
    {
        float startDistance = _currentQuarterViewDistance;
        float startHeight = _currentQuarterViewHeight;
        float endDistance = _quarterViewDistance; // 원래 거리로 돌아오게 설정
        float endHeight = _quarterViewHeight;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            _currentQuarterViewDistance = Mathf.Lerp(startDistance, endDistance, elapsedTime / duration);
            _currentQuarterViewHeight = Mathf .Lerp(startHeight, endHeight,elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _currentQuarterViewDistance = endDistance;
        _currentQuarterViewHeight = endHeight;
    }

    Coroutine _shakeCoroutin = null;

    public void CameraShakingCoroutin(float duration, float magnitude)
    {
        if(_shakeCoroutin != null)
            StopCoroutine(_shakeCoroutin);
        _shakeCoroutin = StartCoroutine(CameraShake(duration, magnitude));
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        _isShaking = true;
        _shakeDuration = duration;
        _shakeMagnitude = magnitude;

        yield return new WaitForSeconds(duration);

        _isShaking = false;
    }
    // 창수부분
    public void QuickZoomAndReturn(float zoomDuration, float zoomAmount)
    {
        if (_zoomCoroutin != null)
            StopCoroutine(_zoomCoroutin);
        _zoomCoroutin = StartCoroutine(QuickZoomRoutine(zoomDuration, zoomAmount));
    }

    IEnumerator QuickZoomRoutine(float duration, float zoomAmount)
    {
        // 즉시 줌 아웃
        _currentQuarterViewDistance -= zoomAmount;
        _currentQuarterViewHeight -= zoomAmount * (_quarterViewHeight / _quarterViewDistance);

        // 지정된 시간이 지난 후 서서히 원래 거리로 돌아옴
        yield return new WaitForSeconds(0.1f); // 즉시 확대 후 약간의 딜레이를 준 뒤 원래 상태로 복구

        yield return StartCoroutine(ZoomInAction(duration)); // ZoomInAction을 사용해 원래 거리로 돌아가도록 함
    }
}
