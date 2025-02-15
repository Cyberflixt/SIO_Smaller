using UnityEngine;

public class BouncePad : MonoBehaviour
{
    // Force to push object
    [SerializeField] private Vector3 force = Vector3.zero;

    private float cooldown = 0;

    private void PropagateForce(Transform transform){
        // Apply force recusively
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.AddForce(force);
        
        // Apply to parent
        if (transform.parent)
            PropagateForce(transform.parent);
    }
    
    private void OnCollisionEnter(Collision collision){
        if (Time.time > cooldown){
            cooldown = Time.time + .05f;
            PropagateForce(collision.transform);
        }
    }
}
