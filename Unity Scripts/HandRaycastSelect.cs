using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Import this namespace for UI elements
using TMPro; // Use TextMeshPro for the UI

public class HandRaycastSelect : MonoBehaviour
{
    public OVRHand hand;
    public TextMeshProUGUI positionText; // Use TextMeshProUGUI for UI text
    public LayerMask planeLayerMask;
    public GameObject virtualObject; // Reference to the virtual object you want to move
    public GameObject cursor; // Cursor to indicate where the ray points at
    public float moveSpeed = 1f; // Adjusted speed at which the object moves to the target location
    public Canvas confirmationWindowCanvas; // Reference to the parent Canvas GameObject for the confirmation window

    private Vector3 targetPosition; // The target position to move the object to
    private bool isLocationSelected = false; // Flag to track if a location is selected
    private float confirmationWindowTimer = 0f; // Timer to keep track of the confirmation window display duration

    private void Start()
    {
        // Initialize targetPosition to the current position of the object
        if (virtualObject != null)
        {
            targetPosition = virtualObject.transform.position;
        }

        // Deactivate the confirmation window at the start
        if (confirmationWindowCanvas != null)
        {
            confirmationWindowCanvas.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        Ray ray = new Ray(hand.transform.position, hand.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, planeLayerMask))
        {
            // Move the cursor to the hit point
            if (cursor != null)
            {
                cursor.SetActive(true);
                cursor.transform.position = hit.point;
            }

            if (hand.GetFingerIsPinching(OVRHand.HandFinger.Index) && !isLocationSelected)
            {
                Vector3 hitPosition = hit.point;
                // Update the TextMeshPro text with the position
                positionText.text = $"Position: {hitPosition.x:F2}, {hitPosition.y:F2}, {hitPosition.z:F2}";
                // Update the target position
                targetPosition = hitPosition;
                // Set the location as selected
                isLocationSelected = true;
                // Change the cursor color to blue for 1 second
                if (cursor != null)
                {
                    Renderer cursorRenderer = cursor.GetComponent<Renderer>();
                    if (cursorRenderer != null)
                    {
                        cursorRenderer.material.color = Color.blue;
                    }
                }
                // Show the confirmation window
                if (confirmationWindowCanvas != null)
                {
                    confirmationWindowCanvas.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            // Hide the cursor when not pointing at the plane
            if (cursor != null)
            {
                cursor.SetActive(false);
            }
        }

        // Move the virtual object towards the target position if a location is selected and confirmed
        if (virtualObject != null && targetPosition != Vector3.zero && !confirmationWindowCanvas.gameObject.activeSelf)
        {
            virtualObject.transform.position = Vector3.MoveTowards(virtualObject.transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }

        // Reset the cursor color to green after 1 second
        if (isLocationSelected && cursor != null)
        {
            confirmationWindowTimer += Time.deltaTime;
            if (confirmationWindowTimer >= 1f)
            {
                Renderer cursorRenderer = cursor.GetComponent<Renderer>();
                if (cursorRenderer != null)
                {
                    cursorRenderer.material.color = Color.green;
                }
                confirmationWindowTimer = 0f;
                isLocationSelected = false;
            }
        }
    }

    // This method will be called when the "Yes" button is clicked
    public void OnYesButtonClicked()
    {
        // Send the target position via UDP
        string message = $"Target Position: {targetPosition.x}, {targetPosition.y}, {targetPosition.z}";
        UDPClient.Instance.Send(message);

        // Hide the confirmation window
        confirmationWindowCanvas.gameObject.SetActive(false);
    }

    // This method will be called when the "No" button is clicked
    public void OnNoButtonClicked()
    {
        // Reset the target position
        targetPosition = Vector3.zero;

        // Hide the confirmation window
        confirmationWindowCanvas.gameObject.SetActive(false);
    }
}