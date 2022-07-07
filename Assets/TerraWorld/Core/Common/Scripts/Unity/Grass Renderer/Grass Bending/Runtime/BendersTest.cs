using UnityEngine;
using GrassBending;

public class BendersTest : MonoBehaviour
{
    public GameObject benderPrefab;
    public float force = 2000;
    public float yOffset = 3f;
    public float destrcutionTime = 10f;
    public float bendRadius = 1f;
    public float bendIntensity = 1.5f;

    void Update ()
    {
        SpawnBenders();
    }

    private void SpawnBenders ()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            if (benderPrefab != null)
            {
                GameObject bender = Instantiate(benderPrefab, transform.position + (transform.transform.up * yOffset), Quaternion.identity);
                bender.name = "Bender";

                BendGrassWhenVisible BGWV = bender.AddComponent<BendGrassWhenVisible>();
                BGWV.BendRadius = bendRadius;
                BGWV.BendIntensity = bendIntensity;

                BendingImpulse bendingImpulse = bender.AddComponent<BendingImpulse>();
                bendingImpulse.destrcutionTime = destrcutionTime;

                if (bender.GetComponent<Rigidbody>() == null) bender.AddComponent<Rigidbody>();
                bender.GetComponent<Rigidbody>().AddForce(transform.transform.forward * force);
            }
        }
    }
}

