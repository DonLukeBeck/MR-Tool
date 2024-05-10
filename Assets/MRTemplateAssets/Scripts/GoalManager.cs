using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro;
using DG.Tweening;
using LazyFollow = UnityEngine.XR.Interaction.Toolkit.UI.LazyFollow;
using UnityEngine.Video;
using System.IO;

public struct Goal
{
    public GoalManager.OnboardingGoals CurrentGoal;
    public bool Completed;

    public Goal(GoalManager.OnboardingGoals goal)
    {
        CurrentGoal = goal;
        Completed = false;
    }
}

public class GoalManager : MonoBehaviour
{
    public enum OnboardingGoals
    {
        Empty,
        interactiveMenu,
        interaction,
    }

    Queue<Goal> m_OnboardingGoals;
    Goal m_CurrentGoal;
    bool m_AllGoalsFinished;
    int m_CurrentGoalIndex = 0;

    [Serializable]
    class Step
    {
        [SerializeField]
        public GameObject stepObject;

        [SerializeField]
        public string buttonText;

        public bool includeSkipButton;
    }

    [SerializeField]
    List<Step> m_StepList = new List<Step>();

    [SerializeField]
    public TextMeshProUGUI m_StepButtonTextField;

    [SerializeField]
    public GameObject m_SkipButton;

    [SerializeField]
    public TextMeshProUGUI m_NextStepButtonTextField;

    [SerializeField]
    public GameObject m_NextStepButton;

    [SerializeField]
    public GameObject m_PreviousStepButton;

    [SerializeField]
    public GameObject m_AskQuestionButton;

    [SerializeField]
    public GameObject m_SendPictureButton;

    [SerializeField]
    public GameObject m_RestartButton;

    [SerializeField]
    public GameObject m_CoachingUIParent;

    [SerializeField]
    public FadeMaterial m_FadeMaterial;

    [SerializeField]
    public LazyFollow m_GoalPanelLazyFollow;

    [SerializeField]
    public GameObject m_VideoPlayer;

    [SerializeField]
    public TextMeshProUGUI m_NextVideoButtonTextField;

    [SerializeField]
    public GameObject m_AgentResponse;

    [SerializeField]
    public GameObject m_3DModel;

    [SerializeField]
    public GameObject m_3DModelPieces;

    [SerializeField]
    public Scrollbar m_progressBar;

    [SerializeField]
    public GameObject m_InteractiveMenu;

    [SerializeField]
    public GameObject m_LeftHand;

    [SerializeField]
    public GameObject m_RightHand;

    [SerializeField]
    public GameObject m_ModelLocationPointer;

    [SerializeField]
    Toggle m_VideoPlayerToggle;

    [SerializeField]
    Toggle m_ModelLocationPointerToggle;

    [SerializeField]
    Toggle m_PassthroughToggle;

    [SerializeField]
    ARPlaneManager m_ARPlaneManager;

    [SerializeField]
    UdpSocket WebSocket;

    private Vector3 m_TargetOffset = new Vector3(0f, -.25f, 1.5f);
    private int k_step = 0;
    private float k_children = 0;
    private List<GameObject> m_Child = new List<GameObject>();
    private bool m_isCabin = false;
    private string[] m_videoFiles;
    private bool m_isFirstVideo = true;

    void Start()
    {
        // Initialize the goals
        m_OnboardingGoals = new Queue<Goal>();
        var welcomeGoal = new Goal(OnboardingGoals.Empty);
        var interactiveMenuGoal = new Goal(OnboardingGoals.interactiveMenu);
        var interactionGoal = new Goal(OnboardingGoals.interaction);
        var endGoal = new Goal(OnboardingGoals.Empty);

        // Add the goals to the queue
        m_OnboardingGoals.Enqueue(welcomeGoal);
        m_OnboardingGoals.Enqueue(interactiveMenuGoal);
        m_OnboardingGoals.Enqueue(interactionGoal);
        m_OnboardingGoals.Enqueue(endGoal);

        m_CurrentGoal = m_OnboardingGoals.Dequeue();

        // Set the first button text in the interactive menu and hide all the rest 
        m_NextStepButtonTextField.text = "Start Assembly";
        m_PreviousStepButton.SetActive(false);
        m_AskQuestionButton.SetActive(false);
        m_SendPictureButton.SetActive(false);
        m_RestartButton.SetActive(false);

        // Set video player
        if (m_VideoPlayer != null)
        {
            m_VideoPlayer.SetActive(false);

            if (m_VideoPlayerToggle != null)
                m_VideoPlayerToggle.isOn = false;
        }

        // Set tutorial video
        m_videoFiles = Directory.GetFiles("Assets/MRTemplateAssets/Videos", "*.mp4");
        m_NextVideoButtonTextField.text = "Next Video";

        // Set agent response
        if (m_AgentResponse != null)
        {
            m_AgentResponse.SetActive(false);
        }

        // Set arrow pointer
        if (m_ModelLocationPointer != null)
        {
            m_ModelLocationPointer.SetActive(false);

            if (m_ModelLocationPointerToggle != null)
                m_ModelLocationPointerToggle.isOn = false;
        }

        // Set passthrough
        if (m_FadeMaterial != null)
        {
            m_FadeMaterial.FadeSkybox(false);

            if (m_PassthroughToggle != null)
                m_PassthroughToggle.isOn = false;
        }

        m_CoachingUIParent.SetActive(false);
    }

    // Next step button functioanlity
    public void NextStep()
    {
        // First step
        if (k_step == 0)
        {
            // Show interactive menu buttons
            m_NextStepButtonTextField.text = "Next Step";
            m_AskQuestionButton.SetActive(true);
            m_SendPictureButton.SetActive(true);
            m_RestartButton.SetActive(true);
            m_PreviousStepButton.SetActive(false);
            m_VideoPlayer.SetActive(false);
            m_VideoPlayerToggle.isOn = false;

            // Hide all pieces
            for (int i = 0; i < m_Child.Count; i++)
            {
                m_Child[i].SetActive(false);
            }

            // Show first piece
            m_Child[0].SetActive(true);
            k_step++;
        }
        // Last step
        else if (k_step == k_children - 1)
        {
            // Show last piece
            m_Child[k_step].SetActive(true);
            k_step++;
            //m_AskQuestionButton.SetActive(false);
            //m_SendPictureButton.SetActive(false);
            m_NextStepButton.SetActive(false);
        }
        // Middle steps
        else
        {
            m_PreviousStepButton.SetActive(true);

            // Color animation for each piece
            Material[] m_Materials = m_Child[k_step].GetComponent<MeshRenderer>().materials;
            foreach (Material material in m_Materials)
            {
                material.DOColor(Color.red, 3f).From();
            }

            // Show next piece
            m_Child[k_step].SetActive(true);
            k_step++;
        }
        print("Current Step" + k_step);
    }

    // Previous step button functionality
    public void PreviousStep()
    {
        // Middle steps
        if (k_step > 0)
        {
            //let user interact with dialogue agent
            m_AskQuestionButton.SetActive(true);
            m_SendPictureButton.SetActive(true);
            m_NextStepButton.SetActive(true);
            k_step--;
            m_Child[k_step].SetActive(false);

        }
        // First step
        else {
            // Restore inital interactive menu
            m_NextStepButtonTextField.text = "Start Assembly";
            m_PreviousStepButton.SetActive(false);
            m_AskQuestionButton.SetActive(false);
            m_SendPictureButton.SetActive(false);
            m_RestartButton.SetActive(false);

            // Display model
            for (int i = 0; i < m_Child.Count; i++)
            {
                m_Child[i].SetActive(true);
            }
        }
        print("Current Step" + k_step);
    }

    // Ask question button functionality
    public void AskQuestion()
    {
        // Send question to server
        WebSocket.SendData("Question " + k_step.ToString());
        TurnOnAgentResponse();
    }


    // Send picture button functionality
    public void SendPicture()
    {
        // Capture image
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

        // Convert to byte array
        byte[] bytes = screenshot.EncodeToPNG();

        // Define chunk size
        int chunkSize = 8192;

        // Calculate number of chunks
        int totalChunks = Mathf.CeilToInt((float)bytes.Length / chunkSize);

        // Send total number of chunks to server
        WebSocket.SendData("ImageChunks " + totalChunks.ToString());

        // Send chunks to server
        for (int i = 0; i < totalChunks; i++)
        {
            // wait for previous packet to arrive
            System.Threading.Thread.Sleep(250);

            int offset = i * chunkSize;
            int length = Mathf.Min(chunkSize, bytes.Length - offset);
            byte[] chunk = new byte[length];
            Array.Copy(bytes, offset, chunk, 0, length);
            string chunkString = Convert.ToBase64String(chunk);
            //print("Chunk " + i + " of " + totalChunks + " sent");
            //print("Chunk string" + chunkString);
            WebSocket.SendData("Base64EncodedChunk " + chunkString);
        }

        // Send step and model to server (remove lines if dialogue agent has a Vision Language Model and can recognize the step from the image)
        if (m_isCabin)
        {
            WebSocket.SendData("Step " + k_step.ToString() + " Model 2");
            print("Step sent " + k_step);
        } else {
            WebSocket.SendData("Step " + k_step.ToString() + " Model 1");
            print("Step sent " + k_step);
        }

        // Release memory
        Destroy(screenshot);

        // Show agent response
        TurnOnAgentResponse();
    }

    // Restart button functionality
    public void Restart()
    {
        k_step = 0;

        // Restore inital interactive menu
        m_AgentResponse.SetActive(false);
        m_NextStepButtonTextField.text = "Start Assembly";
        m_PreviousStepButton.SetActive(false);
        m_AskQuestionButton.SetActive(false);
        m_SendPictureButton.SetActive(false);
        m_RestartButton.SetActive(false);
        // Display model
        for (int i = 0; i < m_Child.Count; i++)
        {
            m_Child[i].SetActive(true);
        }
    }

    // Next Video button functionality
    public void NextVideo()
    {
        VideoPlayer videoPlayer = GameObject.Find("Video Player").GetComponent<VideoPlayer>(); ;


        // Load and play the video
        if (m_isFirstVideo)
        {
            m_NextVideoButtonTextField.text = "Previous Video";
            videoPlayer.url = m_videoFiles[1];
            videoPlayer.Play();
            m_isFirstVideo = false;
        }else {
            m_NextVideoButtonTextField.text = "Next Video";
            videoPlayer.url = m_videoFiles[0];
            videoPlayer.Play();
            m_isFirstVideo = true;
        }

    }

    public void SetModel1()
    {
        m_3DModel = GameObject.Find("Lego Dump Truck");
        m_3DModelPieces = GameObject.Find("Lego Dump Truck Bricks");
        GameObject.Find("Model Selection").SetActive(false);
        GameObject.Find("Lego Truck Cabin").SetActive(false);
        m_CoachingUIParent.SetActive(true);
    }

    public void SetModel2()
    {
        m_3DModel = GameObject.Find("Lego Truck Cabin");
        m_3DModelPieces = GameObject.Find("Lego Truck Cabin Bricks");
        GameObject.Find("Lego Dump Truck").SetActive(false);
        GameObject.Find("Model Selection").SetActive(false);
        m_isCabin = true;
        m_CoachingUIParent.SetActive(true);
    }

    void Update()
    {
        if (!m_AllGoalsFinished)
        {
            ProcessGoals();
        }

        // Debug Input
#if UNITY_EDITOR
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CompleteGoal();
        }
#endif
    }

    void ProcessGoals()
    {
        if (!m_CurrentGoal.Completed)
        {
            switch (m_CurrentGoal.CurrentGoal)
            {
                case OnboardingGoals.Empty:
                    m_GoalPanelLazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.Follow;
                    break;
                case OnboardingGoals.interactiveMenu:
                    m_GoalPanelLazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.Follow;
                    break;
                case OnboardingGoals.interaction:
                    m_GoalPanelLazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.None;
                    break;
            }
        }
    }

    void CompleteGoal()
    {
        m_CurrentGoal.Completed = true;
        m_CurrentGoalIndex++;
        if (m_OnboardingGoals.Count > 0)
        {
            m_CurrentGoal = m_OnboardingGoals.Dequeue();
            m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false);
            m_StepList[m_CurrentGoalIndex].stepObject.SetActive(true);
            m_StepButtonTextField.text = m_StepList[m_CurrentGoalIndex].buttonText;
            m_SkipButton.SetActive(m_StepList[m_CurrentGoalIndex].includeSkipButton);
        }
        else
        {
            m_AllGoalsFinished = true;
            ForceEndAllGoals();
        }
    }

    public void ForceCompleteGoal()
    {
        CompleteGoal();
    }

    public void ForceEndAllGoals()
    {
        m_CoachingUIParent.transform.localScale = Vector3.zero;

        // Turn on video instructions
        TurnOnVideoPlayer();

        // Show 3D Model
        if (m_3DModel != null)
            m_3DModel.SetActive(true);

        // Add children to the list
        foreach (Transform child in m_3DModelPieces.transform)
        {
            m_Child.Add(child.gameObject);
        }

        k_children = m_Child.Count;

        // Show Interactive Menu  
        if (m_InteractiveMenu != null)
            m_InteractiveMenu.SetActive(true);

        if (m_VideoPlayerToggle != null)
            m_VideoPlayerToggle.isOn = true;

        // Toggle passthrough off
        if (m_FadeMaterial != null)
        {
            if(m_LeftHand != null)
                m_LeftHand.SetActive(false);

            if (m_RightHand != null)
                m_RightHand.SetActive(false);

            m_FadeMaterial.FadeSkybox(true);

            if (m_PassthroughToggle != null)
                m_PassthroughToggle.isOn = true;
        }
    }

    public void ResetCoaching()
    {
        m_CoachingUIParent.transform.localScale = Vector3.one;

        m_OnboardingGoals.Clear();
        m_OnboardingGoals = new Queue<Goal>();
        var welcomeGoal = new Goal(OnboardingGoals.Empty);
        var interactiveMenuGoal = new Goal(OnboardingGoals.interactiveMenu);
        var interactionGoal = new Goal(OnboardingGoals.interaction);
        var endGoal = new Goal(OnboardingGoals.Empty);

        m_OnboardingGoals.Enqueue(welcomeGoal);
        m_OnboardingGoals.Enqueue(interactiveMenuGoal);
        m_OnboardingGoals.Enqueue(interactionGoal);
        m_OnboardingGoals.Enqueue(endGoal);

        for (int i = 0; i < m_StepList.Count; i++)
        {
            if (i == 0)
            {
                m_StepList[i].stepObject.SetActive(true);
                m_SkipButton.SetActive(m_StepList[i].includeSkipButton);
                m_StepButtonTextField.text = m_StepList[i].buttonText;
            }
            else
            {
                m_StepList[i].stepObject.SetActive(false);
            }
        }

        m_CurrentGoal = m_OnboardingGoals.Dequeue();
        m_AllGoalsFinished = false;

        m_CurrentGoalIndex = 0;
    }

    public void TogglePlayer(bool visibility)
    {
        if (visibility)
        {
            TurnOnVideoPlayer();
        }
        else
        {
            m_VideoPlayer.SetActive(false);
        }
    }

    public void ToggleModelLocationPointer(bool visibility)
    {
        if (visibility)
        {
            m_ModelLocationPointer.SetActive(true);
        }
        else
        {
            m_ModelLocationPointer.SetActive(false);
        }
    }

    public void TogglePassthrough(bool visibility)
    {
        if (visibility)
        {
            m_FadeMaterial.FadeSkybox(true);
            m_LeftHand.SetActive(false);
            m_RightHand.SetActive(false);
        }
        else
        {
            m_FadeMaterial.FadeSkybox(false);
            m_LeftHand.SetActive(true);
            m_RightHand.SetActive(true);
        }
    }

    void TurnOnVideoPlayer()
    {
        if (m_VideoPlayer.activeSelf)
            return;

        var follow = m_VideoPlayer.GetComponent<LazyFollow>();
        if (follow != null)
            follow.rotationFollowMode = LazyFollow.RotationFollowMode.None;

        m_VideoPlayer.SetActive(false);
        var target = Camera.main.transform;
        var targetRotation = target.rotation;
        var newTransform = target;
        var targetEuler = targetRotation.eulerAngles;
        targetRotation = Quaternion.Euler
        (
            0f,
            targetEuler.y,
            targetEuler.z
        );

        newTransform.rotation = targetRotation;
        var targetPosition = target.position + newTransform.TransformVector(m_TargetOffset);
        m_VideoPlayer.transform.position = targetPosition;


        var forward = target.position - m_VideoPlayer.transform.position;
        var targetPlayerRotation = forward.sqrMagnitude > float.Epsilon ? Quaternion.LookRotation(forward, Vector3.up) : Quaternion.identity;
        targetPlayerRotation *= Quaternion.Euler(new Vector3(0f, 180f, 0f));
        var targetPlayerEuler = targetPlayerRotation.eulerAngles;
        var currentEuler = m_VideoPlayer.transform.rotation.eulerAngles;
        targetPlayerRotation = Quaternion.Euler
        (
            currentEuler.x,
            targetPlayerEuler.y,
            currentEuler.z
        );

        m_VideoPlayer.transform.rotation = targetPlayerRotation;
        m_VideoPlayer.SetActive(true);
        if (follow != null)
            follow.rotationFollowMode = LazyFollow.RotationFollowMode.LookAtWithWorldUp;
    }

    void TurnOnAgentResponse()
    {
        if (m_AgentResponse.activeSelf)
            return;

        var follow = m_AgentResponse.GetComponent<LazyFollow>();
        if (follow != null)
            follow.rotationFollowMode = LazyFollow.RotationFollowMode.None;

        m_AgentResponse.SetActive(false);
        var target = Camera.main.transform;
        var targetRotation = target.rotation;
        var newTransform = target;
        var targetEuler = targetRotation.eulerAngles;
        targetRotation = Quaternion.Euler
        (
            0f,
            targetEuler.y,
            targetEuler.z
        );

        newTransform.rotation = targetRotation;
        var targetPosition = target.position + newTransform.TransformVector(m_TargetOffset);
        m_AgentResponse.transform.position = targetPosition;


        var forward = target.position - m_AgentResponse.transform.position;
        var targetPlayerRotation = forward.sqrMagnitude > float.Epsilon ? Quaternion.LookRotation(forward, Vector3.up) : Quaternion.identity;
        targetPlayerRotation *= Quaternion.Euler(new Vector3(0f, 180f, 0f));
        var targetPlayerEuler = targetPlayerRotation.eulerAngles;
        var currentEuler = m_AgentResponse.transform.rotation.eulerAngles;
        targetPlayerRotation = Quaternion.Euler
        (
            currentEuler.x,
            targetPlayerEuler.y,
            currentEuler.z
        );

        m_AgentResponse.transform.rotation = targetPlayerRotation;

        m_AgentResponse.SetActive(true);
        if (follow != null)
            follow.rotationFollowMode = LazyFollow.RotationFollowMode.LookAtWithWorldUp;
    }
}
