using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class RotTest : MonoBehaviour
{
    [SerializeField] Transform m_transform;
    [SerializeField] Rigidbody m_rb;
    [SerializeField] float g = 0;
    [SerializeField] bool b_;
    [SerializeField] float averageg;
    int i;
    // Start is called before the first frame update
    void Start()
    {
        m_transform = GetComponent<Transform>();
        m_rb = GetComponent<Rigidbody>();
        Input.gyro.enabled = true;//´ò¿ªÍÓÂÝÒÇ
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        g = Input.acceleration.magnitude;
        
        if (b_)
        {
            i++;
            if (i >= 1200)
            {
                averageg += g;
            }
        }

        else
        { m_rb.AddForce(0, g-averageg/ (i - 1200), 0, ForceMode.Acceleration); }
        Debug.Log(averageg / (i-1200));
    }
}
