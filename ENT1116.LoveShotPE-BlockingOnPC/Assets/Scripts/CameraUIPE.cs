using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraUIPE : MonoBehaviour
{
    [SerializeField] ScreenTouchHandler m_screenTouch;
    [SerializeField] MainManagerPE m_mainManager;
    [SerializeField] CameraManagerPE m_cameraManager;
    [Header("BackGround���Һڱ�&�Ź���")]
    [SerializeField] RectTransform t_background;//���Һڱ�+�Ź�����
    [SerializeField] Vector2 backgroundSize = new Vector2(3104,1125);//�����õ�����ͼ���Һڱ�ͼ��3104*1125��
    public Vector2 ScreenRes = new Vector2(2550, 1440);//��Ļ�ֱ���
    public Vector2 CamRectPos = new Vector2(102,132);//����������ľ���ߺ��ұߵı�ֵ
    public Vector2 SideSize;//���ҺڱߵĿ��

    [Header("FocusArea�Խ�Ȧ")]
    [SerializeField] GameObject m_focusAreaUI;//�Խ�Ȧ����
    public Vector2 focusLocalPos = Vector2.zero;//�Խ�Ȧ��ȡ�������е��������
    [Header("FovMultiples����")]
    public float fovMultiplier;//���ű���������Fov�ı�������ʵ��Fov�ǣ�60/FovMultiplier��
    public float lastFovMultiplier;//"֮ǰ��"���ű���
    [SerializeField] TMP_Text fovMultiplierText;
    [SerializeField] CanvasGroup c_fovMultiplierText;
    [SerializeField] bool fadeOut;


    // Start is called before the first frame update
    void Start()
    {
        SideSize = SetBackgroundSize();
    }

    private Vector2 SetBackgroundSize()//���úڱ�λ��
    {
#if UNITY_EDITOR
            ScreenRes = UnityEditor.Handles.GetMainGameViewSize();
#else
            ScreenRes = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
#endif
        t_background.sizeDelta = new Vector2(backgroundSize.x * ( ScreenRes.y/backgroundSize.y), ScreenRes.y);
        t_background.position = new Vector3(ScreenRes.x * CamRectPos.x / (CamRectPos.y + CamRectPos.x), ScreenRes.y/2, 0);//������ڱ߷��ں��ʵ�λ���ϣ�
        return new Vector2(ScreenRes.x * CamRectPos.x / (CamRectPos.y + CamRectPos.x) - ScreenRes.y * 4 / 3 / 2, ScreenRes.x * CamRectPos.x / (CamRectPos.y + CamRectPos.x) + ScreenRes.y * 4 / 3 / 2);//�������Һڱߵ�������
    }
    // Update is called once per frame

    static public float KeepDecimal(float input, int n)//���ڱ���nλС��
    {
        string fn = "F" + n.ToString();
        float a = float.Parse(input.ToString(fn));
        return a;
    }
    static public bool FadeIn(CanvasGroup m_canvasGroup,float fadeTime)
    {
        m_canvasGroup.alpha += Time.deltaTime / fadeTime;
        if (m_canvasGroup.alpha >= 1)
        {
            m_canvasGroup.interactable = true;
            return true;
        }
        else return false;
    }
    static public bool FadeOut(CanvasGroup m_canvasGroup, float fadeTime)
    {
        m_canvasGroup.alpha -= Time.deltaTime / fadeTime;
        if (m_canvasGroup.alpha <= 0)
        {
            m_canvasGroup.interactable = false;
            return true;
        }
        else return false;
    }


    private void OnEnable()
    {
        m_focusAreaUI.GetComponent<CanvasGroup>().alpha = 0;
    }
    void Update()
    {
        if(m_screenTouch.m_touchState==TouchState.singleTouch && SideSize.x < m_screenTouch.touchPoint.x  && m_screenTouch.touchPoint.x < SideSize.y)//�����ȡ�������Ϸ����˵���
        {
            m_focusAreaUI.GetComponent<CanvasGroup>().alpha = 1;
            m_focusAreaUI.GetComponent<RectTransform>().position = m_screenTouch.touchPoint;
            focusLocalPos.x = (m_screenTouch.touchPoint.x - SideSize.x ) / (ScreenRes.y / 3 * 4 / 2)-1;
            focusLocalPos.y = (m_screenTouch.touchPoint.y - ScreenRes.y/2) / (ScreenRes.y);
            m_cameraManager.focusOn = false;
        }
        if (m_screenTouch.m_touchState == TouchState.Double)//�����ڻ�Ϊ������˫ָ�����ǲ��޶���ȡ�������
        {
            fadeOut = true;
            c_fovMultiplierText.alpha = 0.8f;
            fovMultiplier = lastFovMultiplier * (m_screenTouch.fingersDisatanceMultiplier*0.5f+0.5f);
            fovMultiplier = Mathf.Clamp(fovMultiplier, 0.6f, 15);
            fovMultiplier = KeepDecimal(fovMultiplier, 2);
            fovMultiplierText.text = fovMultiplier.ToString() + "x";//����Ļ�ϴ�����ű���
        }
        if(fovMultiplier != lastFovMultiplier && m_screenTouch.m_touchState != TouchState.Double)
        {
            lastFovMultiplier = fovMultiplier;
            fadeOut = false;
        }
        if (!fadeOut)
        {
            fadeOut = FadeOut(c_fovMultiplierText, 0.3f);
        }
    }
}
