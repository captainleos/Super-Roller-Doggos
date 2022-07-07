using UnityEngine;

namespace TerraUnity.Runtime
{
    public class Shooter : MonoBehaviour
    {
        public GameObject prefab;
        public float power = 2000f;
        private bool isShooting = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
                isShooting = true;
        }

        private void FixedUpdate ()
        {
            if (isShooting)
                Shoot();
        }

        private void Shoot ()
        {
            if (prefab == null) return;
            GameObject bullet = Instantiate(prefab, transform.position + (transform.up * 1f) + (transform.forward * 1f), Quaternion.identity);
            bullet.name = "Bullet";
            bullet.GetComponent<Rigidbody>().AddForce(transform.forward * power);
            isShooting = false;
        }
    }
}

