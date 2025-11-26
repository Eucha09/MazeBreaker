using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraInputHandler : MonoBehaviour
{
    MainCameraController _camera;

    void Start()
    {
        _camera = Camera.main.GetComponent<MainCameraController>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (_camera == null || _camera.FollowCam == null)
            return;

		if (Managers.UI.Cursor != null && Managers.UI.Cursor.gameObject.activeSelf == true)
			_camera.FollowCam.EnableCameraRotation(false);
		else if (context.performed && Managers.UI.PopupUICount == 0)
            _camera.FollowCam.EnableCameraRotation(true);
        else if (context.canceled)
            _camera.FollowCam.EnableCameraRotation(false);
    }
}
