using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TouchState {none,singleTouch,singleStay,singleMove,Double}
public class ScreenTouchHandler : MonoBehaviour
{
    [SerializeField] MainManagerPE m_MainManager;
    public TouchState m_touchState;//放开后清零
    public TouchState lastTouchState;//放开后不清零
    public float touchTime;//放开后清零
    public float lastTouchTime;//放开后不清零
    [Header("One Finger Touch")]
    public Vector2 touchMove;
    public Vector2 lastTouchMove;
    public Vector2 touchPoint;
    [Header("Two Fingers Touch")]
    public Vector2 entireMove;
    public Vector2 lastEntireMove;//两个手指中点的移动
    public Vector2 originalRelativePos;//第二指接触屏幕时相对第一指的坐标
    public Vector2 relativePos;//第二指相对第一指的坐标，动态
    public float fingersDisatanceMultiplier;//整个过程中两指缩放的比率
    public Vector2 touchPoint1;//第二个手指的触点

    float i;

    // Start is called before the first frame update
    void Start()
    {
        m_MainManager = GetComponent<MainManagerPE>();
    }
    
    void OneFingerTouch()//单指输入
    {
        Touch touch = Input.GetTouch(0);
        touchTime += Time.deltaTime;
        lastTouchTime = touchTime;
        touchMove += touch.deltaPosition;
        lastTouchMove = touchMove;
        if (touchMove.magnitude > 20)
        {
            m_touchState = TouchState.singleMove;
        }
        else
        {
            if (touchTime < 0.2)
            {
                m_touchState = TouchState.none;
                touchPoint = touch.position;
            }
            else
            {
                m_touchState = TouchState.singleStay;
                touchPoint = touch.position;
            }
        }
    }
    void TwoFingerTouch()//双指输入
    {
        m_touchState = TouchState.Double;
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);
        if (originalRelativePos == Vector2.zero)
            originalRelativePos = touch1.position - touch0.position;
        relativePos = touch1.position - touch0.position;
        fingersDisatanceMultiplier = relativePos.magnitude / originalRelativePos.magnitude;
        entireMove += touch0.deltaPosition / 2 + touch0.deltaPosition / 2;//两指中点的位移量
    }
    
    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount == 2)
        {
            TwoFingerTouch();
        }
        if(m_touchState!=TouchState.Double && Input.touchCount == 1)//双指操作后放开一指不会被认为是单指操作
        {
            OneFingerTouch();
        }
        if(Input.touchCount == 0)
        {
            if (m_touchState == TouchState.none && touchTime < 0.2 && touchTime >0)//只有在单指迅速点的时候，才会在touchstate为none的同时存在Touchtime>0的可能性
            {
                m_touchState = TouchState.singleTouch;
                lastTouchState = m_touchState;
            }
            else { m_touchState = TouchState.none; }//这样可以造出一帧的TouchState.SingleTouch
            touchTime = 0;
            touchMove = Vector2.zero;
            originalRelativePos = Vector2.zero;
            //relativePos = Vector2.zero;
            entireMove = Vector2.zero;
        }
    }
}
