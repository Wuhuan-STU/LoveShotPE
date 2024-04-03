using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraUIPE : MonoBehaviour
{
    [SerializeField] ScreenTouchHandler m_screenTouch;
    [SerializeField] MainManagerPE m_mainManager;
    [SerializeField] CameraManagerPE m_cameraManager;
    [Header("BackGround左右黑边&九宫格")]
    [SerializeField] RectTransform t_background;//左右黑边+九宫格线
    [SerializeField] Vector2 backgroundSize = new Vector2(3104,1125);//现在用的这张图左右黑边图是3104*1125的
    public Vector2 ScreenRes = new Vector2(2550, 1440);//屏幕分辨率
    public Vector2 CamRectPos = new Vector2(102,132);//相机画面中心距左边和右边的比值
    public Vector2 SideSize;//左右黑边的宽度

    [Header("FocusArea对焦圈")]
    [SerializeField] GameObject m_focusAreaUI;//对焦圈对象
    public Vector2 focusLocalPos = Vector2.zero;//对焦圈在取景画面中的相对坐标
    [Header("FovMultiples缩放")]
    public float fovMultiplier;//缩放倍数，不是Fov的倍数，真实的Fov是（60/FovMultiplier）
    public float lastFovMultiplier;//"之前的"缩放倍数
    [SerializeField] TMP_Text fovMultiplierText;
    [SerializeField] CanvasGroup c_fovMultiplierText;
    [SerializeField] bool fadeOut;


    // Start is called before the first frame update
    void Start()
    {
        SideSize = SetBackgroundSize();
    }

    private Vector2 SetBackgroundSize()//设置黑边位置
    {
#if UNITY_EDITOR
            ScreenRes = UnityEditor.Handles.GetMainGameViewSize();
#else
            ScreenRes = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
#endif
        t_background.sizeDelta = new Vector2(backgroundSize.x * ( ScreenRes.y/backgroundSize.y), ScreenRes.y);
        t_background.position = new Vector3(ScreenRes.x * CamRectPos.x / (CamRectPos.y + CamRectPos.x), ScreenRes.y/2, 0);//把两侧黑边放在合适的位置上，
        return new Vector2(ScreenRes.x * CamRectPos.x / (CamRectPos.y + CamRectPos.x) - ScreenRes.y * 4 / 3 / 2, ScreenRes.x * CamRectPos.x / (CamRectPos.y + CamRectPos.x) + ScreenRes.y * 4 / 3 / 2);//返回左右黑边的像素数
    }
    // Update is called once per frame

    static public float KeepDecimal(float input, int n)//用于保留n位小数
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
        if(m_screenTouch.m_touchState==TouchState.singleTouch && SideSize.x < m_screenTouch.touchPoint.x  && m_screenTouch.touchPoint.x < SideSize.y)//如果在取景界面上发生了单点
        {
            m_focusAreaUI.GetComponent<CanvasGroup>().alpha = 1;
            m_focusAreaUI.GetComponent<RectTransform>().position = m_screenTouch.touchPoint;
            focusLocalPos.x = (m_screenTouch.touchPoint.x - SideSize.x ) / (ScreenRes.y / 3 * 4 / 2)-1;
            focusLocalPos.y = (m_screenTouch.touchPoint.y - ScreenRes.y/2) / (ScreenRes.y);
            m_cameraManager.focusOn = false;
        }
        if (m_screenTouch.m_touchState == TouchState.Double)//至少在华为的相机里，双指缩放是不限定在取景界面的
        {
            fadeOut = true;
            c_fovMultiplierText.alpha = 0.8f;
            fovMultiplier = lastFovMultiplier * (m_screenTouch.fingersDisatanceMultiplier*0.5f+0.5f);
            fovMultiplier = Mathf.Clamp(fovMultiplier, 0.6f, 15);
            fovMultiplier = KeepDecimal(fovMultiplier, 2);
            fovMultiplierText.text = fovMultiplier.ToString() + "x";//在屏幕上打出缩放倍率
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
