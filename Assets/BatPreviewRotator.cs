using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BatPreviewRotator : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform bat;
    public Transform batPivot;
    public Button xAxisButton;
    public Button yAxisButton;
    public TMP_Dropdown trailDropdown;
    public List<TrailRenderer> availableTrails;

    public float cameraRotateSpeed = 20f;
    public float batAutoRotateSpeed = 100f;
    public float hoverHeight = 1.5f;
    public float sensitivity = 0.2f;
    public float returnSpeed = 2f;
    public float smoothTime = 0.2f;

    private bool cameraRotationDone = false;
    private bool batRotationDone = false;
    private float cameraRotationAngle = 0f;
    private float batRotationAngle = 0f;

    private Quaternion initialPivotRotation;
    private Vector3 lastMousePosition;
    private float idleTimer = 0f;
    private float idleThreshold = 3f;

    private Quaternion targetRotation;

    private bool xAxisControl = false;
    private bool yAxisControl = false;
    private bool showControls = false;

    void Start()
    {
        if (bat == null) bat = this.transform;
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        if (batPivot == null)
        {
            Debug.LogError("BatPivot not assigned. Please assign an empty GameObject at the bat's top.");
        }

        initialPivotRotation = batPivot.rotation;
        targetRotation = batPivot.rotation;
        lastMousePosition = Input.mousePosition;

        xAxisButton.gameObject.SetActive(false);
        yAxisButton.gameObject.SetActive(false);
        trailDropdown.gameObject.SetActive(false);

        xAxisButton.onClick.AddListener(() => {
            xAxisControl = true;
            yAxisControl = false;
        });

        yAxisButton.onClick.AddListener(() => {
            yAxisControl = true;
            xAxisControl = false;
        });

        DisableAllTrails();  // Trails are hidden initially
        PopulateTrailDropdown();
        trailDropdown.onValueChanged.AddListener(OnTrailSelected);
    }

    void Update()
    {
        bat.position = new Vector3(bat.position.x, hoverHeight, bat.position.z);

        if (!cameraRotationDone)
        {
            RotateCameraAroundBat();
        }
        else if (!batRotationDone)
        {
            AutoRotateBat();
        }
        else
        {
            if (!showControls)
            {
                showControls = true;
                xAxisButton.gameObject.SetActive(true);
                yAxisButton.gameObject.SetActive(true);
                trailDropdown.gameObject.SetActive(true);
            }

            MouseFollowWithDamping();
        }
    }

    void RotateCameraAroundBat()
    {
        float step = cameraRotateSpeed * Time.deltaTime;
        cameraTransform.RotateAround(bat.position, Vector3.up, step);
        cameraRotationAngle += step;

        if (cameraRotationAngle >= 360f)
        {
            cameraRotationDone = true;
        }
    }

    void AutoRotateBat()
    {
        float step = batAutoRotateSpeed * Time.deltaTime;
        bat.Rotate(Vector3.up, step);
        batRotationAngle += step;

        if (batRotationAngle >= 360f)
        {
            batRotationDone = true;
            initialPivotRotation = batPivot.rotation;
            targetRotation = batPivot.rotation;
        }
    }

    void MouseFollowWithDamping()
    {
        Vector3 mouseDelta = Input.mousePosition - lastMousePosition;

        if (mouseDelta.sqrMagnitude > 1f)
        {
            idleTimer = 0f;

            float yRotation = mouseDelta.x * sensitivity;
            float xRotation = -mouseDelta.y * sensitivity;

            Quaternion rotationChange = Quaternion.identity;

            if (xAxisControl)
            {
                rotationChange = Quaternion.AngleAxis(yRotation, Vector3.up);
            }
            else if (yAxisControl)
            {
                rotationChange = Quaternion.AngleAxis(xRotation, batPivot.right);
            }
            else
            {
                Quaternion yQuat = Quaternion.AngleAxis(yRotation, Vector3.up);
                Quaternion xQuat = Quaternion.AngleAxis(xRotation, batPivot.right);
                rotationChange = yQuat * xQuat;
            }

            targetRotation = rotationChange * batPivot.rotation;
        }
        else
        {
            idleTimer += Time.deltaTime;

            if (idleTimer > idleThreshold)
            {
                targetRotation = Quaternion.Lerp(batPivot.rotation, initialPivotRotation, returnSpeed * Time.deltaTime);
            }
        }

        batPivot.rotation = Quaternion.Slerp(batPivot.rotation, targetRotation, Time.deltaTime / smoothTime);
        lastMousePosition = Input.mousePosition;
    }

    void PopulateTrailDropdown()
    {
        if (trailDropdown == null || availableTrails == null || availableTrails.Count == 0)
        {
            Debug.LogWarning("Dropdown or trail list not set up properly.");
            return;
        }

        trailDropdown.ClearOptions();
        List<string> trailNames = new List<string>();

        foreach (var trail in availableTrails)
        {
            trailNames.Add(trail.name);
        }

        trailDropdown.AddOptions(trailNames);
        // No selection until user picks one manually
    }

    void OnTrailSelected(int index)
    {
        for (int i = 0; i < availableTrails.Count; i++)
        {
            availableTrails[i].gameObject.SetActive(i == index);
            availableTrails[i].Clear();
        }
    }

    void DisableAllTrails()
    {
        foreach (var trail in availableTrails)
        {
            trail.gameObject.SetActive(false);
        }
    }
}
