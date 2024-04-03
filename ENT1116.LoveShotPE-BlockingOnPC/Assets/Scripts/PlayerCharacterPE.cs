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
    public VariableJoystick moveController;//摇杆输入
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
        rb.MovePosition(rb.position + m_animator.deltaPosition);//把刚体移动到玩家网格处
        playerTransform.position = rb.position;
    }
    void viewControll()
    {
        xRot -= viewController.Vertical * Time.deltaTime * 20 * rotSpeed;
        yRot += viewController.Horizontal* Time.deltaTime * 20 * rotSpeed;
        xRot = Mathf.Clamp(xRot, -90, 90);
        FPVHolder.rotation = Quaternion.Euler(xRot, yRot, 0);//这行是转的玩家上的空物体
        FPV_vcam.rotation = Quaternion.Slerp(FPV_vcam.rotation, FPVHolder.rotation, camFollowingSpeed * 0.4f * Time.deltaTime);//真的vcam平滑跟随空物体转
        playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.Euler(0, yRot, 0), camFollowingSpeed * 0.4f * Time.deltaTime);//平滑地转玩家mesh
        FPV_vcam.position += (FPVHolder.position - FPV_vcam.position) * camFollowingSpeed * Time.deltaTime;//相机位置平滑跟随空物体，其实这里用Vector3.MoveTowards也可以但是懒得改了
    }
    void viewControllCam()//shit的一部分 具体内容看CamerManager
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
            viewControllCam();//相机启用时只转
    }
}
