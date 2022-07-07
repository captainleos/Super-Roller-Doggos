using UnityEngine;

namespace TerraUnity.Runtime
{
    [RequireComponent(typeof(Camera))]
    public class SceneDirector : MonoBehaviour
    {
        public float speed = 2f;
        public Vector2 panAngles = new Vector2(185f, 25f);
        public bool rotateFromCenter = false;
        public bool rotateAroundCenter = false;
        public float fRadius = 3f;
        public bool slideOnSurface = false;
        public float heightOffset = 2f;
        public float adoptionSpeed = 1f;
        public LayerMask layerMask = ~0;
        public enum WaterDetection
        {
            onWater,
            underWater
        }
        public WaterDetection waterDetection = WaterDetection.onWater;
        private bool isUnderwater;

        private Vector2 _centre;
        private float _angle;

        private void OnValidate()
        {
            if (rotateFromCenter) rotateAroundCenter = false;
            if (rotateAroundCenter) rotateFromCenter = false;
        }

        private void Start()
        {
            _centre = transform.position;
        }

        void Update()
        {
            if (rotateFromCenter)
                transform.RotateAround(transform.position, Vector3.up, speed * Time.deltaTime);
            else if (rotateAroundCenter)
            {
                _angle += speed * Time.deltaTime;

                var offset = new Vector2(Mathf.Sin(_angle), Mathf.Cos(_angle)) * fRadius;
                transform.position = _centre + offset;
                transform.LookAt(_centre);
            }
            else
            {
                Vector3 dir = Quaternion.Euler(panAngles) * transform.forward;
                transform.position += dir * speed * Time.deltaTime;
            }

            Vector3 origin = transform.position;
            origin.y = 100000f;
            Ray ray = new Ray(origin, Vector3.down);
            RaycastHit hit;
            if (waterDetection == WaterDetection.underWater) isUnderwater = true;
            else isUnderwater = false;

            if (!Raycasts.RaycastNonAllocSorted(ray, false, isUnderwater, out hit, layerMask))
                return;

            origin = hit.point;
            origin.y += heightOffset;

            if (slideOnSurface || transform.position.y < origin.y)
                transform.position = Vector3.Lerp(transform.position, origin, Time.deltaTime * adoptionSpeed);
        }

        Vector2 VectorFromAngle(float theta)
        {
            return new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
        }
    }
}

