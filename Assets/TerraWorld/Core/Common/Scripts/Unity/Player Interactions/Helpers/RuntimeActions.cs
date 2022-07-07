using UnityEngine;

namespace TerraUnity.Runtime
{
    public class RuntimeActions : MonoBehaviour
    {
        private Rigidbody rigidBody;
        private GameObject parent;

        private void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Respawn")
            {
                //Destroy(gameObject);

                rigidBody = GetComponent<Rigidbody>();
                if (rigidBody == null) gameObject.AddComponent<Rigidbody>();
            }
        }

        private void GetRootParent()
        {
            parent = gameObject;

            while (parent.transform.parent != null)
            {
                parent = parent.transform.parent.gameObject;
            }
        }
    }
}

