using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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
    public TMPro.TextMeshProUGUI scoreText;
    
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
                
                // Initialize cooldown for each mole
                moleCooldowns[moles[i]] = 0f;
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
        // Update cooldowns
        foreach (var mole in moles)
        {
            if (moleCooldowns[mole] > 0)
            {
                moleCooldowns[mole] -= Time.deltaTime;
            }
        }
    }
    
    // This method should be called when the hammer collides with a mole
    public void HitMole(GameObject hitMole)
    {
        // Check if the mole is in cooldown
        if (moleCooldowns[hitMole] <= 0)
        {
            // Increment score
            score++;
            UpdateScoreDisplay();
            
            // Play hit sound
            if (hitSound != null)
            {
                hitSoundSource.PlayOneShot(hitSound);
            }
            
            // Set cooldown for this mole
            moleCooldowns[hitMole] = hitCooldown;
            
            // Optional: Animate the mole being hit
            StartCoroutine(AnimateMoleHit(hitMole));
        }
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    private IEnumerator AnimateMoleHit(GameObject mole)
    {
        // Save original position
        Vector3 originalPosition = mole.transform.localPosition;
        
        // Move mole down
        mole.transform.localPosition = new Vector3(
            originalPosition.x,
            originalPosition.y - 0.2f,
            originalPosition.z
        );
        
        // Wait
        yield return new WaitForSeconds(0.3f);
        
        // Move mole back up
        mole.transform.localPosition = originalPosition;
    }
}
