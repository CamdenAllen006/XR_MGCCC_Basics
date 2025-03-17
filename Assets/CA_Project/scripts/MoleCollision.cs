using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleCollision : MonoBehaviour
{
    private WhackAMoleController gameController;
    
    void Start()
    {
        // Find the parent WhackAMole game controller
        gameController = GetComponentInParent<WhackAMoleController>();
        
        if (gameController == null)
        {
            Debug.LogError("WhackAMoleController not found in parent hierarchy");
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with the hammer
        if (collision.gameObject.name == "Hammer")
        {
            // Check if the hammer is moving fast enough (optional)
            if (collision.relativeVelocity.magnitude > 1.0f)
            {
                // Register the hit with the game controller
                gameController.HitMole(gameObject);
            }
        }
    }
    
    // Alternative method using triggers if you prefer
    void OnTriggerEnter(Collider other)
    {
        // Check if the trigger is with the hammer
        if (other.gameObject.name == "Hammer")
        {
            // Register the hit with the game controller
            gameController.HitMole(gameObject);
        }
    }
}