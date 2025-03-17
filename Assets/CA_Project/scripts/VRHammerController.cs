using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRHammerController : MonoBehaviour
{
    [Header("Physics Settings")]
    public float hitVelocityThreshold = 1.0f;
    public float hitCooldown = 0.1f;
    
    [Header("References")]
    public Transform hammerHead; // Assign the transform of the hammer's head/impact point
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Vector3 previousPosition;
    private Vector3 velocity;
    private float cooldownTimer = 0f;
    private bool isGrabbed = false;
    
    // Layer mask for moles
    private int moleLayerMask;
    
    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        
        if (hammerHead == null)
        {
            // If no hammer head is assigned, use this object's transform
            hammerHead = transform;
            Debug.LogWarning("No hammer head transform assigned. Using this object's transform.");
        }
        
        // Set up initial position
        previousPosition = hammerHead.position;
        
        // Set up layer mask for moles
        // Set your moles to this layer in the inspector
        moleLayerMask = LayerMask.GetMask("Mole");
        
        // Set up grab detection
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
        
        // Set rigidbody settings for better physics
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }
    
    void Update()
    {
        // Update cooldown timer
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        // Only use custom hit detection when grabbed
        if (isGrabbed)
        {
            // Calculate velocity manually
            velocity = (hammerHead.position - previousPosition) / Time.deltaTime;
            previousPosition = hammerHead.position;
            
            // Check for hits when the hammer is moving fast enough
            if (velocity.magnitude > hitVelocityThreshold && cooldownTimer <= 0)
            {
                CheckForHits();
            }
        }
    }
    
    void CheckForHits()
    {
        // Direction of movement
        Vector3 direction = velocity.normalized;
        
        // Use a line cast in the direction of movement to detect hits
        RaycastHit hit;
        float castDistance = velocity.magnitude * Time.deltaTime * 2; // Cast a bit further than we moved
        
        Debug.DrawRay(hammerHead.position, direction * castDistance, Color.red, 0.5f);
        
        if (Physics.Raycast(hammerHead.position, direction, out hit, castDistance, moleLayerMask))
        {
            // We hit something!
            Debug.Log($"Hit: {hit.collider.gameObject.name} with velocity: {velocity.magnitude}");
            
            // Find the mole component
            MoleCollision mole = hit.collider.GetComponent<MoleCollision>();
            if (mole != null)
            {
                // Call the hit method on the mole
                WhackAMoleController controller = mole.GetComponentInParent<WhackAMoleController>();
                if (controller != null)
                {
                    controller.HitMole(hit.collider.gameObject);
                }
            }
            
            // Set cooldown
            cooldownTimer = hitCooldown;
        }
    }
    
    void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        Debug.Log("Hammer grabbed");
    }
    
    void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        Debug.Log("Hammer released");
    }
}