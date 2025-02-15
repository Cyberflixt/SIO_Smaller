using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleExplosionRadius : MonoBehaviour
{
    // Start is called before the first frame update
    public float radius = 5;
    public float damage = 200;

    [Button("Kaboom")]
    public void Explode(){
        // Empty list of objets destroyed
        List<DestructibleWall> done = new List<DestructibleWall>();
        
        // Find objects in radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius*5);
        foreach (Collider collider in colliders){
            // Get parent's parent
            Transform parent = collider.transform.parent;
            if (parent){
                parent = parent.parent;
                if (parent){
                    // Is object destructible?
                    DestructibleWall destroy = parent.GetComponent<DestructibleWall>();
                    //DebugPlus.Log(parent, destroy);
                    if (destroy && !done.Contains(destroy)){ // not already done?
                        done.Add(destroy);

                        // Apply explosion
                        destroy.Explosion(transform.position, radius);
                    }
                }
            }
        }
    }

    private IEnumerator Timer(int seconds){
        yield return new WaitForSeconds(seconds);
        DestructibleUtilities.ExplosionSphere(transform.position, radius, damage);
    }

    void Start(){
        StartCoroutine(Timer(5));
    }
}
