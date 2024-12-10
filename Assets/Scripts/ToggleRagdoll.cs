using UnityEngine;

public class ToggleRagdoll : MonoBehaviour
{
    [SerializeField] private Transform playerRoot;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private bool isRagdollEnabled = false;

    void Start()
    {
        // Gather all Rigidbodies and Colliders in the player
        ragdollRigidbodies = playerRoot.GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = playerRoot.GetComponentsInChildren<Collider>(true);

        // Disable ragdoll at start
        SetRagdollState(false);
    }

    public void EnableRagdoll()
    {
        SetRagdollState(true);
    }

    public void DisableRagdoll()
    {
        SetRagdollState(false);
    }

    private void SetRagdollState(bool state)
    {
        isRagdollEnabled = state;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !state;
        }

        foreach (Collider col in ragdollColliders)
        {
            col.enabled = state;
        }
    }
}
