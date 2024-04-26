using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro;
using DG.Tweening;
using LazyFollow = UnityEngine.XR.Interaction.Toolkit.UI.LazyFollow;

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
        FindSurfaces,
        TapSurface,
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
    public GameObject m_PreviousStepButton;

    [SerializeField]
    public GameObject m_AskQuestionButton;

    [SerializeField]
    public GameObject m_SendPictureButton;

    [SerializeField]
    public GameObject m_RestartButton;

    [SerializeField]
    GameObject m_CoachingUIParent;

    [SerializeField]
    FadeMaterial m_FadeMaterial;

    [SerializeField]
    LazyFollow m_GoalPanelLazyFollow;

    [SerializeField]
    GameObject m_VideoPlayer;

    [SerializeField]
    GameObject m_3DModel;

    [SerializeField]
    GameObject m_3DModelPieces;

    [SerializeField]
    Scrollbar m_progressBar;

    [SerializeField]
    GameObject m_InteractiveMenu;

    [SerializeField]
    GameObject m_LeftHand;

    [SerializeField]
    GameObject m_RightHand;

    [SerializeField]
    GameObject m_ModelLocationPointer;

    [SerializeField]
    Toggle m_VideoPlayerToggle;

    [SerializeField]
    Toggle m_ModelLocationPointerToggle;

    [SerializeField]
    Toggle m_PassthroughToggle;

    [SerializeField]
    ARPlaneManager m_ARPlaneManager;

    private Vector3 m_TargetOffset = new Vector3(0f, -.25f, 1.5f);

    private int k_step = 0;
    private float k_children = 0;

    private List<GameObject> m_Child = new List<GameObject>();

    void Start()
    {
        // Initialize the goals
        m_OnboardingGoals = new Queue<Goal>();
        var welcomeGoal = new Goal(OnboardingGoals.Empty);
        var findSurfaceGoal = new Goal(OnboardingGoals.FindSurfaces);
        var tapSurfaceGoal = new Goal(OnboardingGoals.TapSurface);
        var endGoal = new Goal(OnboardingGoals.Empty);

        // Add the goals to the queue
        m_OnboardingGoals.Enqueue(welcomeGoal);
        m_OnboardingGoals.Enqueue(findSurfaceGoal);
        m_OnboardingGoals.Enqueue(tapSurfaceGoal);
        m_OnboardingGoals.Enqueue(endGoal);

        m_CurrentGoal = m_OnboardingGoals.Dequeue();

        // Set the first button text in the interactive menu and hide all the rest 
        m_NextStepButtonTextField.text = "Start Assembly";
        m_PreviousStepButton.SetActive(false);
        m_AskQuestionButton.SetActive(false);
        m_SendPictureButton.SetActive(false);
        m_RestartButton.SetActive(false);

        // Add children to the list
        foreach (Transform child in m_3DModelPieces.transform)
        {
            m_Child.Add(child.gameObject);
        }

        k_children = m_Child.Count;

        if (m_VideoPlayer != null)
        {
            m_VideoPlayer.SetActive(false);

            if (m_VideoPlayerToggle != null)
                m_VideoPlayerToggle.isOn = false;
        }

        if (m_ModelLocationPointer != null)
        {
            m_ModelLocationPointer.SetActive(false);

            if (m_ModelLocationPointerToggle != null)
                m_ModelLocationPointerToggle.isOn = false;
        }

        if (m_FadeMaterial != null)
        {
            m_FadeMaterial.FadeSkybox(false);

            if (m_PassthroughToggle != null)
                m_PassthroughToggle.isOn = false;
        }

    }

    // Next step button functioanlity
    public void NextStep()
    {
        //first step
        if (k_step == 0)
        {
            // show interactive menu buttons
            m_NextStepButtonTextField.text = "Next Step";
            m_PreviousStepButton.SetActive(true);
            m_AskQuestionButton.SetActive(true);
            m_SendPictureButton.SetActive(true);
            m_RestartButton.SetActive(true);

            // hide all pieces
            for (int i = 0; i < m_Child.Count; i++)
            {
                m_Child[i].SetActive(false);
            }

            // show first piece
            m_Child[0].SetActive(true);
            k_step++;
        }
        //last step
        else if (k_step == k_children - 1)
        {
            // show last piece
            m_Child[k_step].SetActive(true);
        }
        //middle steps
        else
        {
            // color animation for each piece
            Material[] m_Materials = m_Child[k_step].GetComponent<MeshRenderer>().materials;
            foreach (Material m in m_Materials)
            {
                m.DOColor(Color.red, 3f).From();
            }

            // show next piece
            m_Child[k_step].SetActive(true);
            k_step++;
        }
    }

    // Previous step button functionality
    public void PreviousStep()
    {
        //middle steps
        if (k_step > 0)
        {
            m_Child[k_step].SetActive(false);
            k_step--;
        }
        // first step
        else {
            // restore inital interactive menu
            m_NextStepButtonTextField.text = "Start Assembly";
            m_PreviousStepButton.SetActive(false);
            m_AskQuestionButton.SetActive(false);
            m_SendPictureButton.SetActive(false);
            m_RestartButton.SetActive(false);
            // display model
            m_3DModelPieces.SetActive(true);
        }
    }


    // Send picture button functionality
    public void SendPicture()
    {

        
    }


    // Ask question button functionality
    public void AskQuestion()
    {

        
    }

    // Restart button functionality
    public void Restart()
    {
        k_step = 0;

        // restore inital interactive menu
        m_NextStepButtonTextField.text = "Start Assembly";
        m_PreviousStepButton.SetActive(false);
        m_AskQuestionButton.SetActive(false);
        m_SendPictureButton.SetActive(false);
        m_RestartButton.SetActive(false);
        // display model
        m_3DModelPieces.SetActive(true);
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
                case OnboardingGoals.FindSurfaces:
                    m_GoalPanelLazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.Follow;
                    break;
                case OnboardingGoals.TapSurface:
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

        // turn on video instructions
        TurnOnVideoPlayer();

        //show 3D Model
        if (m_3DModel != null)
            m_3DModel.SetActive(true);

        // show Interactive Menu  
        if (m_InteractiveMenu != null)
            m_InteractiveMenu.SetActive(true);

        if (m_VideoPlayerToggle != null)
            m_VideoPlayerToggle.isOn = true;

        //toggle passthrough off
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
        var findSurfaceGoal = new Goal(OnboardingGoals.FindSurfaces);
        var tapSurfaceGoal = new Goal(OnboardingGoals.TapSurface);
        var endGoal = new Goal(OnboardingGoals.Empty);

        m_OnboardingGoals.Enqueue(welcomeGoal);
        m_OnboardingGoals.Enqueue(findSurfaceGoal);
        m_OnboardingGoals.Enqueue(tapSurfaceGoal);
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
}
