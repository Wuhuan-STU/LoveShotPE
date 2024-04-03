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
    [SerializeField] Camera MainCamera;//���������
    [SerializeField] Camera CamView;//�ֳ����
    [SerializeField] Canvas BasicPlayCanvas;//������UI
    [SerializeField] GameObject CamViewUI;//���UI
    [SerializeField] Rig TwoHandsIK;//˫��IK�����λ��


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
            cameraManager.SetActive(true);//�����������
            MainCamera.enabled = false;//�ص��������
            CamView.enabled = true;//���ֳ����
            BasicPlayCanvas.enabled = false;//�ص�����UI
            CamViewUI.SetActive(true);//�����UI
            TwoHandsIK.weight = 1;//��IK׷��
        }
        if (playState == PlayState.basic)
        {
            cameraManager.SetActive(false);
            MainCamera.enabled = transform;
            CamView.enabled = false;
            BasicPlayCanvas.enabled = true;
            CamViewUI.SetActive(false);
            TwoHandsIK.weight = 0;//�����ȫ��������
        }
        return playState;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
