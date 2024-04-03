using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Animations.Rigging;//IK
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public enum PlayState {basic,camera,cutscene,dialogue};
public enum GameState {play,enterScene,hold};
public class MainManagerPE : MonoBehaviour
{
    public PlayState playState;
    public GameState gameState;

    [SerializeField] GameObject cameraManager;
    [SerializeField] Camera MainCamera;//场景主相机
    [SerializeField] Camera CamView;//手持相机
    [SerializeField] Canvas BasicPlayCanvas;//主界面UI
    [SerializeField] GameObject CamViewUI;//相机UI
    [SerializeField] Rig TwoHandsIK;//双手IK到相机位置


    // Start is called before the first frame update
    void Start()
    {
        SwitchState(PlayState.basic);
    }
    public void BasicPlayToCamButton()
    {
        SwitchState(PlayState.camera);
    }
    public void CamToBasicPlayButton()
    {
        SwitchState(PlayState.basic);
    }
    public PlayState SwitchState(PlayState m_playState)
    {
        playState = m_playState;
        if (playState == PlayState.camera)
        {
            cameraManager.SetActive(true);//启用相机管理
            MainCamera.enabled = false;//关掉场景相机
            CamView.enabled = true;//打开手持相机
            BasicPlayCanvas.enabled = false;//关掉场景UI
            CamViewUI.SetActive(true);//打开相机UI
            TwoHandsIK.weight = 1;//打开IK追踪
        }
        if (playState == PlayState.basic)
        {
            cameraManager.SetActive(false);
            MainCamera.enabled = transform;
            CamView.enabled = false;
            BasicPlayCanvas.enabled = true;
            CamViewUI.SetActive(false);
            TwoHandsIK.weight = 0;//和相机全部反操作
        }
        return playState;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
