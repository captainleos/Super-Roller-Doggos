using UnityEngine;
using System;

namespace TerraUnity.Runtime
{
    public class WindManager : MonoBehaviour
    {
        public bool isWindEnabled = true;
        [Range(0f, 10f)] public float windTime = 0.85f;
        [Range(0f, 10f)] public float windSpeed = 1.25f;
        [Range(0f, 10f)] public float windBending = 1.33f;
        [MinValue(0)] public float m_Speed = 1.0f;

        public bool WindState
        {
            get { return isWindEnabled; }
            set
            {
                isWindEnabled = value;
                ApplySettings();
            }
        }

        public float WindTime
        {
            get { return windTime; }
            set
            {
                windTime = value;
                ApplySettings();
            }
        }

        public float WindSpeed
        {
            get { return windSpeed; }
            set
            {
                windSpeed = value;
                ApplySettings();
            }
        }

        public float WindBending
        {
            get { return windBending; }
            set
            {
                windBending = value;
                ApplySettings();
            }
        }

        public void OnValidate()
        {
            ApplySettings();
        }

        public void ApplyWind (bool _windState, float _windTime, float _windSpeed, float _windBending)
        {
            isWindEnabled = _windState;
            windTime = _windTime;
            windSpeed = _windSpeed;
            windBending = _windBending;
            ApplySettings();
        }

        private void ApplySettings()
        {
            foreach (Material m in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[])
            {
                if (m.hideFlags == HideFlags.NotEditable || m.hideFlags == HideFlags.HideAndDontSave) continue;

                if (m.IsKeywordEnabled("_WIND"))
                {
                    if (m.HasProperty("_WindState"))
                    {
                        m.SetFloat("_WindState", Convert.ToInt32(isWindEnabled));

                        if (WindState)
                        {
                            if (m.HasProperty("_ShakeTime")) m.SetFloat("_ShakeTime", windTime);
                            if (m.HasProperty("_ShakeWindspeed")) m.SetFloat("_ShakeWindspeed", windSpeed);
                            if (m.HasProperty("_ShakeBending")) m.SetFloat("_ShakeBending", windBending);
                        }
                    }
                }
            }
        }

        public void RemoveWind()
        {
            foreach (Material m in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[])
            {
                if (m.hideFlags == HideFlags.NotEditable || m.hideFlags == HideFlags.HideAndDontSave) continue;

                if (m.IsKeywordEnabled("_WIND"))
                    if (m.HasProperty("_WindState")) m.SetFloat("_WindState", 0);
            }
        }

        public void Init()
        {
            isWindEnabled = true;
            windTime = 0.85f;
            windSpeed = 1.25f;
            windBending = 1.33f;

            foreach (Material m in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[])
            {
                if (m.hideFlags == HideFlags.NotEditable || m.hideFlags == HideFlags.HideAndDontSave) continue;
            
                if (m.IsKeywordEnabled("_WIND"))
                    if (m.HasProperty("_WindState"))
                    {
                        if (m.GetFloat("_WindState") == 0) isWindEnabled = false;
                        else isWindEnabled = true;

                        windTime = m.GetFloat("_ShakeTime");
                        windSpeed = m.GetFloat("_ShakeWindspeed");
                        windBending = m.GetFloat("_ShakeBending");

                        break;
                    }
            }

            ApplySettings();
        }


        ////private static Vector4 _windParams1 = new Vector4(20f, 2f, 0.01f, 0f); // x, y (0~20) - z (0~1)
        ////private static Vector4 _windParams2 = new Vector4(12f, 10f, 0.03f, 0f); // x, y (0~20) - z (0~1)
        ////private float freqHeight1;
        ////private float freqHeight2;
        ////private float freqTime1;
        ////private float freqTime2;
        ////private float freqU1;
        ////private float freqU2;
        ////private Vector4 windParams1;
        ////private Vector4 windParams2;
        //
        //void OnDrawGizmosSelected()
        //{
        //    Vector3[] arrow =
        //    {
        //    new Vector3(0,0,1.5f),
        //    new Vector3( 1.0f,0.0f, 0.5f), new Vector3( 0.5f,0.0f,0.5f), new Vector3( 0.5f,0.0f,-1.0f),
        //    new Vector3(-0.5f,0.0f,-1.0f), new Vector3(-0.5f,0.0f,0.5f), new Vector3(-1.0f,0.0f, 0.5f),
        //    new Vector3(0,0,1.5f)
        //};
        //
        //    Gizmos.matrix = transform.localToWorldMatrix;
        //    int count = arrow.Length;
        //
        //    for (int i = 0; i < count; i++)
        //        Gizmos.DrawLine(arrow[i], arrow[(i + 1) % count]);
        //
        //    Gizmos.matrix = Gizmos.matrix * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 90), Vector3.one);
        //
        //    for (int i = 0; i < count; i++)
        //        Gizmos.DrawLine(arrow[i], arrow[(i + 1) % count]);
        //}
        //
        ////void Update ()
        ////{
        ////    freqTime1 = Mathf.Clamp(_windParams1.x * m_Speed / 100f, 0f, 10f);
        ////    freqTime2 = Mathf.Clamp(_windParams2.x * m_Speed / 100f, 0f, 10f);
        ////    freqU1 = Mathf.Clamp(_windParams1.y * m_Speed / 0.4f, 0f, 10f);
        ////    freqU2 = Mathf.Clamp(_windParams2.y * m_Speed / 0.4f, 0f, 10f);
        ////    freqHeight1 = Mathf.Clamp01(_windParams1.z * m_Speed / 40f);
        ////    freqHeight2 = Mathf.Clamp01(_windParams2.z * m_Speed / 40f);
        ////    windParams1 = new Vector4(freqTime1, freqU1, freqHeight1, 0);
        ////    windParams2 = new Vector4(freqTime2, freqU2, freqHeight2, 0);
        ////    Shader.SetGlobalVector("_WindParam1", windParams1);
        ////    Shader.SetGlobalVector("_WindParam2", windParams2);
        ////}
    }
}

