using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterPE : MonoBehaviour
{
    [SerializeField] float SpeedX;
    [SerializeField] float SpeedZ;
    [SerializeField] float xRot;
    [SerializeField] float yRot;

    [SerializeField] MainManagerPE mainManagerPE;
    [SerializeField] GameObject playerMesh;
    [SerializeField] Transform playerTransform;
    [SerializeField] Transform FPVHolder;
    [SerializeField] Transform FPV_vcam;
    [SerializeField] Animator m_animator;
    [SerializeField] Transform target;
    [SerializeField] Transform targetHorizonal;
    public VariableJoystick moveController;//ҡ������
    public VariableJoystick viewController;
    public float rotSpeed = 5;
    Rigidbody rb;

    [Header("Sens")]
    [SerializeField] float camFollowingSpeed = 20;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = playerMesh.GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
    }
    void MoveControll()
    {
        SpeedX = Mathf.Lerp(SpeedX, moveController.Vertical * 2f, Time.deltaTime * 20);
        SpeedZ = Mathf.Lerp(SpeedZ, moveController.Horizontal * 1.5f, Time.deltaTime * 20);
        m_animator.SetFloat("SpeedX", SpeedZ);
        m_animator.SetFloat("SpeedZ", SpeedX);
        rb.MovePosition(rb.position + m_animator.deltaPosition);//�Ѹ����ƶ����������
        playerTransform.position = rb.position;
    }
    void viewControll()
    {
        xRot -= viewController.Vertical * Time.deltaTime * 20 * rotSpeed;
        yRot += viewController.Horizontal* Time.deltaTime * 20 * rotSpeed;
        xRot = Mathf.Clamp(xRot, -90, 90);
        FPVHolder.rotation = Quaternion.Euler(xRot, yRot, 0);//������ת������ϵĿ�����
        FPV_vcam.rotation = Quaternion.Slerp(FPV_vcam.rotation, FPVHolder.rotation, camFollowingSpeed * 0.4f * Time.deltaTime);//���vcamƽ�����������ת
        playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.Euler(0, yRot, 0), camFollowingSpeed * 0.4f * Time.deltaTime);//ƽ����ת���mesh
        FPV_vcam.position += (FPVHolder.position - FPV_vcam.position) * camFollowingSpeed * Time.deltaTime;//���λ��ƽ����������壬��ʵ������Vector3.MoveTowardsҲ���Ե������ø���
    }
    void viewControllCam()//shit��һ���� �������ݿ�CamerManager
    {
        targetHorizonal.position = new Vector3(target.position.x, playerTransform.position.y, target.position.z);
        playerTransform.LookAt(targetHorizonal);
    }
    // Update is called once per frame
    void Update()
    {
        MoveControll();
        if(mainManagerPE.playState==PlayState.basic)
            viewControll();
        if (mainManagerPE.playState == PlayState.camera)
            viewControllCam();//�������ʱֻת
    }
}
