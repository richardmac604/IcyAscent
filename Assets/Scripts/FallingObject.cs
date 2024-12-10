using UnityEngine;

public class FallingObject : MonoBehaviour
{
    public float knockbackForce = 10.0f;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object hit the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get ToggleRagdoll component
            ToggleRagdoll ragdollController = collision.gameObject.GetComponent<ToggleRagdoll>();

            if (ragdollController != null)
            {
                // Activate the ragdoll
                ragdollController.EnableRagdoll();
            }

            // Apply knockback
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Calculate the direction to knock the player back
                Vector3 knockbackDirection = (collision.transform.position - transform.position).normalized;
                knockbackDirection.y = 0; // Set Y to 0 if you want horizontal knockback only

                // Apply the knockback force to the player
                playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
        }
    }
}
