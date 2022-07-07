using UnityEngine;

namespace TerraUnity.Runtime
{
    public class ExtendedFlyCam : MonoBehaviour
    {
        public float cameraSensitivity = 90;
        public float normalMoveSpeed = 0.5f;
        public float climbSpeed = 1f;
        public float slowMoveFactor = 0.25f;
        public float fastMoveFactor = 2f;
        public float motorSpeed = 1f;
        public bool lockRotation = false;
        //public bool mouse3D = false;
        public bool dynamicFOV = false;
        private float rotationX = 0.0f;
        private float rotationY = 0.0f;
        private Camera cam;

        private float motorV = 0f;
        private float motorH = 0f;
        private float motorU = 0f;
        private float motorD = 0f;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cam = GetComponent<Camera>();
        }

        void Update()
        {
            if (!lockRotation)
            {
                rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
                rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
                rotationY = Mathf.Clamp(rotationY, -90, 90);

                transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
                transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left); // * Quaternion.Euler(new Vector3(0, initialRotationY, 0));
            }

            if (dynamicFOV)
            {
                float height = (Mathf.InverseLerp(-500f, 8848f, transform.position.y) * 20f) + 35f;
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, Mathf.Clamp(height, 45f, 60f), Time.deltaTime);
            }

            //if (mouse3D)
            //{
            //    if (transform.localEulerAngles.y > 180)
            //        transform.position += (-transform.right * transform.localEulerAngles.y / 120f) * normalMoveSpeed * Time.deltaTime;
            //    else if (transform.localEulerAngles.y < 180)
            //        transform.position += (transform.right * transform.localEulerAngles.y / 120f) * normalMoveSpeed * Time.deltaTime;
            //
            //    transform.position += (transform.forward * (Mathf.Abs(transform.localEulerAngles.x) / 120f)) * normalMoveSpeed * Time.deltaTime;
            //}

            motorV = Mathf.Lerp(motorV, Input.GetAxis("Vertical"), Time.deltaTime * motorSpeed);
            motorH = Mathf.Lerp(motorH, Input.GetAxis("Horizontal"), Time.deltaTime * motorSpeed);
            if (Input.GetKey(KeyCode.E) || Input.GetMouseButton(0)) motorU = Mathf.Lerp(motorU, 1, Time.deltaTime * motorSpeed);
            else motorU = Mathf.Lerp(motorU, 0, Time.deltaTime * motorSpeed);
            if (Input.GetKey(KeyCode.Q) || Input.GetMouseButton(1)) motorD = Mathf.Lerp(motorD, 1, Time.deltaTime * motorSpeed);
            else motorD = Mathf.Lerp(motorD, 0, Time.deltaTime * motorSpeed);

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * motorV;
                transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * motorH;
            }
            else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * motorV;
                transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * motorH;
            }
            else
            {
                transform.position += transform.forward * normalMoveSpeed * motorV;
                transform.position += transform.right * normalMoveSpeed * motorH;
            }

            transform.position += transform.up * climbSpeed * motorU;
            transform.position -= transform.up * climbSpeed * motorD;
        }
    }
}

