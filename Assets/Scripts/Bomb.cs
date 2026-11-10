using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{

    // REFERENCES
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    // BOMB SETTINGS

    [Range(0.5f, 10f)]
    [SerializeField] private float fuseTime = 4f;

    [Range(0.5f, 10f)]
    [SerializeField] private float explosionRadius = 3f;

    [Range(1f, 30f)]
    [SerializeField] private float explosionForce = 12f;

    [SerializeField] private AudioClip fuseBeep;
    [SerializeField] private AudioClip explosionSound;

    // BOMB FLASH SETTINGS

    [SerializeField] private Color flashColor = Color.white;
    private Color originalColor;

    [Range(0.05f, 1f)]
    [SerializeField] private float slowFlashInterval = 0.4f;

    [Range(0.05f, 1f)]
    [SerializeField] private float fastFlashInterval = 0.15f;

    [Range(0.01f, 0.5f)]
    [SerializeField] private float imminentFlashInterval = 0.05f;


    // SETUP

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        StartCoroutine(FlashFuse());// Start flashing on a loop

        Invoke(nameof(Explode), fuseTime); // Explode after (fuseTime) seconds
    }


    // EXPLOSION

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            Destructible destructible = hit.GetComponent<Destructible>();

            if (destructible != null)
            {
                destructible.DestroyObject();
            }

            // Find the rigid body to target
            Rigidbody2D targetRB = hit.attachedRigidbody;

            // Skip if there is no rb on the hit
            if (targetRB == null)
            {
                continue;
            }

            // if the rigid body is not "dynamic" then continue
            if (targetRB.bodyType != RigidbodyType2D.Dynamic)
            {
                continue;
            }

            // therefore all thats left is rbs that are dynamic (like the player etc)

            // calculate the explosion direction
            Vector2 direction = (targetRB.worldCenterOfMass - (Vector2)transform.position).normalized;

            // calculate the actual force based on direction and distance
            targetRB.AddForce(direction * explosionForce, ForceMode2D.Impulse);

            Debug.Log("Explosion hit: " + hit.name);
        }

        AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, 0.8f); 
        Destroy(gameObject);
    }

    private IEnumerator FlashFuse()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fuseTime)
        {
            float progress = elapsedTime / fuseTime; // Create a percentage based on time since bomb started
            Debug.Log("progress is: " + progress);

            float flashInterval = slowFlashInterval;
            Debug.Log("flash interval is: " + flashInterval);

            if (progress >= 0.9f)
            {
                flashInterval = imminentFlashInterval;
            }

            else if (progress >= 0.5f)
            {
                flashInterval = fastFlashInterval;
            }

            // Loop the colours
            spriteRenderer.color = flashColor;
            audioSource.PlayOneShot(fuseBeep);
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashInterval);

            // Increment the timer
            elapsedTime += flashInterval * 2f;
        }

    }




    // DEBUG AND GIZMOS

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}