using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class WhackAMoleController : MonoBehaviour
{
    [Header("Game Settings")]
    public int score = 0;
    public float hitCooldown = 0.5f; // Time before a mole can be hit again
    
    [Header("Audio")]
    public AudioSource hitSoundSource;
    public AudioClip hitSound;
    
    [Header("References")]
    public GameObject hammer;
    public GameObject[] moles;
    public TextMeshPro scoreText; // 3D TextMeshPro
    
    // Dictionary to track cooldowns for each mole
    private Dictionary<GameObject, float> moleCooldowns = new Dictionary<GameObject, float>();
    
    void Start()
    {
        // Initialize the moles array if not set in inspector
        if (moles == null || moles.Length == 0)
        {
            moles = new GameObject[8];
            for (int i = 0; i < 8; i++)
            {
                moles[i] = transform.Find($"Mole{i + 1}")?.gameObject;
                if (moles[i] == null)
                {
                    Debug.LogError($"Mole{i + 1} not found as a child of WhacAMole");
                }
                else
                {
                    // Initialize cooldown for each mole
                    moleCooldowns[moles[i]] = 0f;
                }
            }
        }
        else
        {
            // Initialize cooldowns for moles that were set in the inspector
            foreach (GameObject mole in moles)
            {
                if (mole != null && !moleCooldowns.ContainsKey(mole))
                {
                    moleCooldowns[mole] = 0f;
                }
            }
        }
        
        // Initialize audio source if not set
        if (hitSoundSource == null)
        {
            hitSoundSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize score display
        UpdateScoreDisplay();
    }
    
    void Update()
    {
        // Update cooldowns - with null check and key check
        if (moles != null)
        {
            foreach (GameObject mole in moles)
            {
                if (mole != null && moleCooldowns.ContainsKey(mole))
                {
                    if (moleCooldowns[mole] > 0)
                    {
                        moleCooldowns[mole] -= Time.deltaTime;
                    }
                }
            }
        }
    }
    
    // This method should be called when the hammer collides with a mole
    public void HitMole(GameObject hitMole)
    {
        // Check if the mole exists in our dictionary
        if (hitMole != null && moleCooldowns.ContainsKey(hitMole))
        {
            // Check if the mole is in cooldown
            if (moleCooldowns[hitMole] <= 0)
            {
                // Increment score
                score++;
                UpdateScoreDisplay();
                
                // Play hit sound
                if (hitSound != null && hitSoundSource != null)
                {
                    hitSoundSource.PlayOneShot(hitSound);
                }
                
                // Set cooldown for this mole
                moleCooldowns[hitMole] = hitCooldown;
            }
        }
        else
        {
            Debug.LogWarning("Tried to hit a mole that isn't in our dictionary: " + 
                            (hitMole != null ? hitMole.name : "null"));
            
            // Add the mole to our dictionary if it exists but isn't tracked
            if (hitMole != null && !moleCooldowns.ContainsKey(hitMole))
            {
                moleCooldowns.Add(hitMole, hitCooldown);
                
                // Still count this hit
                score++;
                UpdateScoreDisplay();
                
                // Play hit sound
                if (hitSound != null && hitSoundSource != null)
                {
                    hitSoundSource.PlayOneShot(hitSound);
                }
            }
        }
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
}