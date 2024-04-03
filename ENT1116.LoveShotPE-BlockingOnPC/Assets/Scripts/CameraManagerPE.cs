using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Android;

public class CameraManagerPE : MonoBehaviour//������ƽű� PE�����ֻ��棨pocket edition��,�벻����׺�ĵ��԰����ֿ�
{

    [Header("\nCam & Postprocessing")]
    [SerializeField] Vector3 attitude = Vector3.zero;//�豸�ڿռ��еĳ���
    [SerializeField] GameObject postProcessingVolumn;//������ڵ���Ч�ռ�
    [SerializeField] VolumeProfile m_Profile;//���������ļ�
    [SerializeField] Camera CamView;//����ĵ��Ļ���,�����PC���������һ��
    [SerializeField] Transform t_camera;
    [SerializeField] int photographSerial;//��Ƭ����

    [Header("CamPosition")]
    [SerializeField] Vector3 CamPos;//���С��Χ�ƶ������ֻ����ٶȼ��ṩ��
    [SerializeField] Vector3 CamRot;//���������ת�����ֻ��������ṩ��
    [Header("IK")]
    [SerializeField] Transform rightHandIK;//����IKλ��
    [SerializeField] Transform rightHandIKParentHolder;//��chest�����ɫ�ƶ�����ת
    [SerializeField] Transform rightHandIkParent;//����Holder��λ�õ���������ת�������ֻ������ǵ���ת
    [SerializeField] Transform rightHandIkTarget;//����parent�£�IK����IKtarget

    [Header("CamUI")]
    [SerializeField] CameraUIPE cameraUI;
    public float CamRectPosL = 102;
    public float CamRectPosR = 132;//���ʵʱ�����������UI�ϣ�������Ļ�����Ҳ����ռ��
    [SerializeField] Vector2 CamRectSize = Vector2.zero;//�������Ļ����ʾ��������

    [Header("Lens Component��ͷ����")]
    public float minFov = 60/0.6f;
    public float maxFov = 60/10f;//�佹��Χ,�����ƺ�û�������Ѿ�����
    public float minFocusDistance = 0.35f;//����Խ����루m��
    public float maxFocusDistance = 1000f;
    public float focusLengthMultiplier = 2;
    public float minAperture = 4;
    public float maxAperture = 22;//��Ȧ��Χ

    [Header("Camera Body Components�������")]//PE����ʵû��Ҫ��������Ĺ��ܣ�����ν��
    public float focusSpeed = 2;//�Խ��ٶ�
    public bool faceFocus = false;//�����Խ�����ʱ��û��qwq��
    public int pixelX = 5184;//cmos���ؿ��
    public int pixelY = 3888;//cmos���ظ߶�
    public bool Raw;

    [Header("Exposure Components�ع�Ҫ��")]
    public ExposureMode exposureMode = ExposureMode.Auto;
    public enum ExposureMode { Auto, P, S, A };//�ع�ģʽ
    [SerializeField] float Fov = 60;//��Fov���㽹��
    [SerializeField] float focalLength;
    [SerializeField] float aperture = 4;//ʵ�ʹ�Ȧ
    [SerializeField] float shotSpeed = 100;//�����ٶȣ����ڻ��㶯̬ģ��

    [Header("FocusPoint������Խ�")]
    [SerializeField] float focusDistance = 3;
    public Vector3 focusAngle = Vector3.zero;//�Խ���ѡ��ת����������������ǰ���ĽǶ�
    public bool focusOn;//�Խ��ɹ���ʾ,���Ա�CameraUI�޸�
    [SerializeField] Transform focusOrientation;//�Խ����߳���
    
        // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = true;//��������
        CamRectSize = SetCameraScreenSize();//����CamView�ĳ����
    }

    private void ApertureControll()//�黯����
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

    private Vector2 SetCameraScreenSize()//����Camview�Ĵ�С��������Ļ�ռ�
    {
#if UNITY_EDITOR
        Vector2 ScreenRes = UnityEditor.Handles.GetMainGameViewSize();//�ڱ༭���з���Gameview�Ĵ�С
#else
            Vector2 ScreenRes = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);//���豸�Ϸ�����Ļ�Ĵ�С
#endif
        CamView.rect = new Rect(CamRectPosL/(CamRectPosL+CamRectPosR)-ScreenRes.y*4/3/2/ScreenRes.x, 0, ScreenRes.y * 4 / 3 / ScreenRes.x, 1);
        return new Vector2(ScreenRes.y * 4 / 3, ScreenRes.y);
    }

    private Quaternion CameraTransform()//�����ǿ����������ʵ����IK�����mesh��ת��
    {
        //�ҳ��������shit ���ǿ��ǵ��ֻ������ǵ����������ȷ������(���þ��У���)
        attitude = Input.gyro.attitude.eulerAngles;//��ȡ�ֻ��ڿռ��еĳ���
        rightHandIkParent.localRotation = Quaternion.Euler(attitude.x,attitude.y,attitude.z);
        rightHandIkParent.position = rightHandIKParentHolder.position;
        rightHandIK.position += (rightHandIkTarget.position-rightHandIK.position) * 4f * Time.deltaTime;
        rightHandIK.rotation = Quaternion.Slerp(rightHandIK.rotation, rightHandIkTarget.rotation, 5* Time.fixedDeltaTime);
        return rightHandIK.rotation;
        /*
         * �򵥵�˵һ������ʺɽ��ʲô
         * ��Ҫԭ�����Ҹ㲻���ֻ�����������ĽǶȵ�һЩ������ת����
         * 
         * ���ڵ�д���ǣ�Player�Ĺ�������һ��right Hand IK Parent Holder,��������Ҷ������б任
         * ���prefab�·�right Hand IK Parent,��Ϊprefab��ת�������parent������������ת
         * Parent�ĸ�������һ����90��0��0������ת�� ������������ֵӦ��Ϊparent��localRot
         * Parent����IKTarget��CamManager
         * ���У�IK Target����һ�����parent�ĺ㶨��ת�ǣ�����������IK���ڽǶȺ�λ����ƽ���������IK Target
         * CamManager�����ֳ������Ҳ����һ���㶨��ת�ǣ�180��180��0������ʹ�������ת��ȷ
         * ����Cam����20m������һ��PlayerCamviewTarget�����������ת����PlayerCharacter�ű���������������ͶӰ���������xzƽ�洦����PlayerCamviewTargetHorizontal
         * Ȼ�������lookAt���PlayerCamviewTargetHorizontal����ʵ����ҵ���ת����
         * ����Ӧ�õð�����������ֵ������֮��ͨ��ĳ���任ת��x����y����z����������Ϳ��Լ򵥺ܶ���
         * ��������֮ǰ����������ʺɽ��һ��
         * ����������Ҳû�༸�д��룿��
         */
    }

    public Texture2D CameraCapture(Camera camera, Rect rect, string fileName)//��Ⱦ��Ƭ������
    {
        photographSerial += 1;//��Ƭ����+1
        //m_postProcessProfile.GetSetting<DepthOfField>().aperture.value /= (pixelY / CamRectSize.y);//�����黯ֵ,PE���û��������ע�͵���

        RenderTexture render = new RenderTexture((int)rect.width, (int)rect.height, -1);//����һ��RenderTexture���� 
        camera.targetTexture = render;//���ý�ͼ�����targetTextureΪrender
        CamView.rect = new Rect(0, 0, 1, 1);//�����������RenderTexture
        camera.Render();//�ֶ�������ͼ�������Ⱦ

        RenderTexture.active = render;//����RenderTexture
        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24/*λ���*/, false);//�½�һ��Texture2D����
        tex.ReadPixels(rect, 0, 0);//��ȡ����
        tex.Apply();//����������Ϣ

        RenderTexture.active = null;//�ر�RenderTexture�ļ���״̬
        camera.targetTexture = null;//�������RenderTexture�ϰ��룬�ص���Ļ�ռ�
        SetCameraScreenSize();//ˢ���������Ļ�ռ�����Ҳ���
        Object.Destroy(render);//ɾ��RenderTexture����
        //m_postProcessProfile.GetSetting<DepthOfField>().aperture.value *= (pixelY / CamRectSize.y);//�ָ��黯ֵ
        byte[] bytes = tex.EncodeToPNG();//���������ݣ�ת����һ��pngͼƬ
        System.IO.File.WriteAllBytes("Assets/DCIM/" + fileName + ".jpg", bytes);//д������
        Debug.Log(string.Format("��ȡ��һ��ͼƬ: {0}", fileName));

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();//ˢ��Unity���ʲ�Ŀ¼
#endif

        return tex;//����Texture2D���󣬷�����Ϸ��չʾ��ʹ��
    }
    /// <summary>
    ///��Ȩ����������Ϊ����ԭ�����£���ѭ CC 4.0 BY-SA ��ȨЭ�飬ת���븽��ԭ�ĳ������Ӻͱ�������                    
    ///ԭ�����ӣ�https://blog.csdn.net/c651088720/article/details/94621145
    /// </summary>

    private void AF()//�Զ��Խ�
    {
        focusAngle = new Vector3((Mathf.Atan(cameraUI.focusLocalPos.x) * 4 / 3.14159f/*�����Ƶ�45��*/)/*�������ĵĽǶȱ���*/, Mathf.Atan(cameraUI.focusLocalPos.y) * 4 / 3.14159f, 0);
        focusAngle *= CamView.fieldOfView;
        focusOrientation.localRotation = Quaternion.Euler(focusAngle.y*0.8f, -focusAngle.x*0.65f, 0);//0.8��0.65��ʵ������ľ���ϵ�������Է��ֺ���Ļ�ֱ����޹�
        Ray ray = new Ray(CamView.GetComponent<Transform>().position, focusOrientation.forward);//���������
        RaycastHit hit;
        Physics.Raycast(ray, out hit);//��raycast̽��Խ�����
        if (focusDistance == 0)
        {
            focusDistance = 10000;
        }
        else
        {
            focusDistance += (hit.distance - focusDistance) * focusSpeed * Time.deltaTime;//�Խ�
            if (Mathf.Abs(focusDistance - hit.distance) < 0.05)
                focusOn = true;
            else
                focusOn = false;
        }
        focusDistance = Mathf.Clamp(focusDistance, minFocusDistance, maxFocusDistance);//�Խ���������
        DepthOfField depthOfField;//���Խ����ݸ���Postprocess
        if (m_Profile.TryGet<DepthOfField>(out depthOfField))
        {
            depthOfField.focusDistance.Override(focusDistance);
        }
    }

    public void TakePhoto()//���������UI�ϵĿ��Ű�ť����
    {
        CameraCapture(CamView, new Rect(0, 0, pixelX, pixelY),"P"+ photographSerial.ToString());
        /*
         * ���ﲹ���ģ��Ⱦ���첽���ص�
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
