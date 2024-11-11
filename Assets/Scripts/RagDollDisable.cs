using UnityEngine;

public class ToggleRagdollExcludeArms : MonoBehaviour
{
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform[] armBones; // Assign the arm bones in the Inspector
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private bool isRagdollEnabled = false;

    void Start()
    {
        // Gather all Rigidbodies and Colliders in the player, excluding the arms
        ragdollRigidbodies = playerRoot.GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = playerRoot.GetComponentsInChildren<Collider>(true);

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (!IsArmBone(rb.transform))
                rb.isKinematic = true;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (!IsArmBone(col.transform))
                col.enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            ToggleRagdollState();
        }
    }

    private void ToggleRagdollState()
    {
        isRagdollEnabled = !isRagdollEnabled;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (!IsArmBone(rb.transform))
                rb.isKinematic = !isRagdollEnabled;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (!IsArmBone(col.transform))
                col.enabled = isRagdollEnabled;
        }
    }

    private bool IsArmBone(Transform bone)
    {
        foreach (Transform armBone in armBones)
        {
            if (bone == armBone || bone.IsChildOf(armBone))
                return true;
        }
        return false;
    }
}
