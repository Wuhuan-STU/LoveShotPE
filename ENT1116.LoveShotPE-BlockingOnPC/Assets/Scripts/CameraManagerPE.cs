using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Android;

public class CameraManagerPE : MonoBehaviour//相机控制脚本 PE代表手机版（pocket edition）,与不带后缀的电脑版区分开
{

    [Header("\nCam & Postprocessing")]
    [SerializeField] Vector3 attitude = Vector3.zero;//设备在空间中的朝向
    [SerializeField] GameObject postProcessingVolumn;//相机所在的特效空间
    [SerializeField] VolumeProfile m_Profile;//后处理配置文件
    [SerializeField] Camera CamView;//相机拍到的画面,这里和PC版的命名不一样
    [SerializeField] Transform t_camera;
    [SerializeField] int photographSerial;//照片计数

    [Header("CamPosition")]
    [SerializeField] Vector3 CamPos;//相机小范围移动（由手机加速度计提供）
    [SerializeField] Vector3 CamRot;//相机三轴旋转（由手机陀螺仪提供）
    [Header("IK")]
    [SerializeField] Transform rightHandIK;//右手IK位置
    [SerializeField] Transform rightHandIKParentHolder;//在chest下随角色移动和旋转
    [SerializeField] Transform rightHandIkParent;//跟随Holder的位置但不跟随旋转，接受手机陀螺仪的旋转
    [SerializeField] Transform rightHandIkTarget;//绑在parent下，IK跟随IKtarget

    [Header("CamUI")]
    [SerializeField] CameraUIPE cameraUI;
    public float CamRectPosL = 102;
    public float CamRectPosR = 132;//相机实时画面的中心在UI上，距离屏幕左侧和右侧距离占比
    [SerializeField] Vector2 CamRectSize = Vector2.zero;//相机在屏幕上显示的像素数

    [Header("Lens Component镜头参数")]
    public float minFov = 60/0.6f;
    public float maxFov = 60/10f;//变焦范围,但是似乎没有用了已经（）
    public float minFocusDistance = 0.35f;//最近对焦距离（m）
    public float maxFocusDistance = 1000f;
    public float focusLengthMultiplier = 2;
    public float minAperture = 4;
    public float maxAperture = 22;//光圈范围

    [Header("Camera Body Components机身参数")]//PE版其实没必要留换机身的功能，无所谓了
    public float focusSpeed = 2;//对焦速度
    public bool faceFocus = false;//人脸对焦（暂时还没做qwq）
    public int pixelX = 5184;//cmos像素宽度
    public int pixelY = 3888;//cmos像素高度
    public bool Raw;

    [Header("Exposure Components曝光要素")]
    public ExposureMode exposureMode = ExposureMode.Auto;
    public enum ExposureMode { Auto, P, S, A };//曝光模式
    [SerializeField] float Fov = 60;//用Fov反算焦距
    [SerializeField] float focalLength;
    [SerializeField] float aperture = 4;//实际光圈
    [SerializeField] float shotSpeed = 100;//快门速度，用于换算动态模糊

    [Header("FocusPoint焦点与对焦")]
    [SerializeField] float focusDistance = 3;
    public Vector3 focusAngle = Vector3.zero;//对焦点选择转换出的相对于相机正前方的角度
    public bool focusOn;//对焦成功提示,可以被CameraUI修改
    [SerializeField] Transform focusOrientation;//对焦射线朝向
    
        // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = true;//打开陀螺仪
        CamRectSize = SetCameraScreenSize();//调整CamView的长宽比
    }

    private void ApertureControll()//虚化控制
    {
        DepthOfField depthOfField;
        if(m_Profile.TryGet<DepthOfField>(out depthOfField))
        {
            depthOfField.aperture.Override(aperture);
            depthOfField.focalLength.Override(focalLength);
        }
    }
    private void FovControll()
    {
        Fov = 60 / cameraUI.fovMultiplier;
        CamView.fieldOfView = Fov;
        focalLength = Mathf.Abs(18 / (Mathf.Tan(0.5f * Fov * 3.141f/180)));
    }

    private Vector2 SetCameraScreenSize()//调整Camview的大小以适配屏幕空间
    {
#if UNITY_EDITOR
        Vector2 ScreenRes = UnityEditor.Handles.GetMainGameViewSize();//在编辑器中返回Gameview的大小
#else
            Vector2 ScreenRes = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);//在设备上返回屏幕的大小
#endif
        CamView.rect = new Rect(CamRectPosL/(CamRectPosL+CamRectPosR)-ScreenRes.y*4/3/2/ScreenRes.x, 0, ScreenRes.y * 4 / 3 / ScreenRes.x, 1);
        return new Vector2(ScreenRes.y * 4 / 3, ScreenRes.y);
    }

    private Quaternion CameraTransform()//陀螺仪控制相机（其实是手IK和玩家mesh）转动
    {
        //我承认这个是shit 但是考虑到手机陀螺仪的奇怪输入就先放在这吧(能用就行，绷)
        attitude = Input.gyro.attitude.eulerAngles;//获取手机在空间中的朝向
        rightHandIkParent.localRotation = Quaternion.Euler(attitude.x,attitude.y,attitude.z);
        rightHandIkParent.position = rightHandIKParentHolder.position;
        rightHandIK.position += (rightHandIkTarget.position-rightHandIK.position) * 4f * Time.deltaTime;
        rightHandIK.rotation = Quaternion.Slerp(rightHandIK.rotation, rightHandIkTarget.rotation, 5* Time.fixedDeltaTime);
        return rightHandIK.rotation;
        /*
         * 简单地说一下这坨屎山是什么
         * 主要原因是我搞不懂手机陀螺仪输入的角度的一些复杂旋转……
         * 
         * 现在的写法是：Player的骨骼上有一个right Hand IK Parent Holder,仅跟随玩家动画进行变换
         * 玩家prefab下放right Hand IK Parent,因为prefab不转所以这个parent不会跟随玩家旋转
         * Parent的父级带有一个（90，0，0）的旋转。 将陀螺仪输入值应用为parent的localRot
         * Parent下有IKTarget和CamManager
         * 其中，IK Target带有一个相对parent的恒定旋转角，真正的右手IK会在角度和位置上平滑跟随这个IK Target
         * CamManager就是手持相机，也带有一个恒定旋转角（180，180，0），来使相机的旋转正确
         * 沿着Cam视线20m处放了一个PlayerCamviewTarget（跟随相机旋转），PlayerCharacter脚本里会在这个空物体投影到玩家所在xz平面处放置PlayerCamviewTargetHorizontal
         * 然后让玩家lookAt这个PlayerCamviewTargetHorizontal，来实现玩家的旋转……
         * 后面应该得把陀螺仪输入值搞明白之后，通过某个变换转成x俯仰y航向z横滚，这样就可以简单很多了
         * 但是在这之前，先让这坨屎山顶一顶
         * （反正好像也没多几行代码？）
         */
    }

    public Texture2D CameraCapture(Camera camera, Rect rect, string fileName)//渲染照片主函数
    {
        photographSerial += 1;//照片计数+1
        //m_postProcessProfile.GetSetting<DepthOfField>().aperture.value /= (pixelY / CamRectSize.y);//更新虚化值,PE景深还没做所以先注释掉了

        RenderTexture render = new RenderTexture((int)rect.width, (int)rect.height, -1);//创建一个RenderTexture对象 
        camera.targetTexture = render;//设置截图相机的targetTexture为render
        CamView.rect = new Rect(0, 0, 1, 1);//相机铺满整个RenderTexture
        camera.Render();//手动开启截图相机的渲染

        RenderTexture.active = render;//激活RenderTexture
        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24/*位深度*/, false);//新建一个Texture2D对象
        tex.ReadPixels(rect, 0, 0);//读取像素
        tex.Apply();//保存像素信息

        RenderTexture.active = null;//关闭RenderTexture的激活状态
        camera.targetTexture = null;//将相机从RenderTexture上剥离，回到屏幕空间
        SetCameraScreenSize();//刷新相机在屏幕空间的左右裁切
        Object.Destroy(render);//删除RenderTexture对象
        //m_postProcessProfile.GetSetting<DepthOfField>().aperture.value *= (pixelY / CamRectSize.y);//恢复虚化值
        byte[] bytes = tex.EncodeToPNG();//将纹理数据，转化成一个png图片
        System.IO.File.WriteAllBytes("Assets/DCIM/" + fileName + ".jpg", bytes);//写入数据
        Debug.Log(string.Format("截取了一张图片: {0}", fileName));

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();//刷新Unity的资产目录
#endif

        return tex;//返回Texture2D对象，方便游戏内展示和使用
    }
    /// <summary>
    ///版权声明：本文为博主原创文章，遵循 CC 4.0 BY-SA 版权协议，转载请附上原文出处链接和本声明。                    
    ///原文链接：https://blog.csdn.net/c651088720/article/details/94621145
    /// </summary>

    private void AF()//自动对焦
    {
        focusAngle = new Vector3((Mathf.Atan(cameraUI.focusLocalPos.x) * 4 / 3.14159f/*弧度制的45°*/)/*距离中心的角度比例*/, Mathf.Atan(cameraUI.focusLocalPos.y) * 4 / 3.14159f, 0);
        focusAngle *= CamView.fieldOfView;
        focusOrientation.localRotation = Quaternion.Euler(focusAngle.y*0.8f, -focusAngle.x*0.65f, 0);//0.8和0.65是实测出来的纠正系数，测试发现和屏幕分辨率无关
        Ray ray = new Ray(CamView.GetComponent<Transform>().position, focusOrientation.forward);//发出测距线
        RaycastHit hit;
        Physics.Raycast(ray, out hit);//用raycast探测对焦距离
        if (focusDistance == 0)
        {
            focusDistance = 10000;
        }
        else
        {
            focusDistance += (hit.distance - focusDistance) * focusSpeed * Time.deltaTime;//对焦
            if (Mathf.Abs(focusDistance - hit.distance) < 0.05)
                focusOn = true;
            else
                focusOn = false;
        }
        focusDistance = Mathf.Clamp(focusDistance, minFocusDistance, maxFocusDistance);//对焦距离限制
        DepthOfField depthOfField;//将对焦数据赋予Postprocess
        if (m_Profile.TryGet<DepthOfField>(out depthOfField))
        {
            depthOfField.focusDistance.Override(focusDistance);
        }
    }

    public void TakePhoto()//这个函数由UI上的快门按钮唤起
    {
        CameraCapture(CamView, new Rect(0, 0, pixelX, pixelY),"P"+ photographSerial.ToString());
        /*
         * 这里补充高模渲染、异步加载等
         */
    }

    // Update is called once per frame
    void Update()
    {
        CameraTransform();
        FovControll();
        ApertureControll();
        if (!focusOn)
        {
            AF();
        }
    }
}
