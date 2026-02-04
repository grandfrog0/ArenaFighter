using UnityEngine;

public class CameraObserver : MonoBehaviour
{
    public Transform Target;
    public Transform RotateTransform;
    public GameObject TargetModel;
    [SerializeField] float sensitivity = 2f; 
    [SerializeField] Vector3 offset;
    [SerializeField] float maxDistance;
    private float _distance = 3;
    private float _xRot, _yRot;

    void Update()
    {
        if (!Target)
        {
            _yRot = _xRot = 0;
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        _xRot -= mouseY;
        _xRot = Mathf.Clamp(_xRot, -80, 80);
        _yRot += mouseX;

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (_distance == 0)
            {
                _distance = 3;
                TargetModel.SetActive(true);
            }
            else
            {
                _distance = 0;
                TargetModel.SetActive(false);
            }
        }

        transform.localRotation = Quaternion.Euler(_xRot, _yRot, 0);
        transform.position = Target.position + transform.rotation * offset * _distance;
        RotateTransform.Rotate(Vector3.up * mouseX);
    }
}
