using UnityEngine;

public class IdleAnimation : MonoBehaviour
{
    // Settings
    public int idleMax = 1; // number of idle animations: Name "Idle_X" -> X in [0; idleMax[
    private float cooldown = 20;

    // Init
    private Animator animator;
    private int idleOldInt = 0;
    private float idleAccu = 0;
    Vector3 oldPosition = Vector3.zero;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float dist = (transform.position - oldPosition).magnitude;
        oldPosition = transform.position;

        if (dist > .01f * Time.deltaTime){
            idleAccu = 0;
            animator.SetBool("IsIdle", false);
        } else {
            idleAccu += Time.deltaTime;
            animator.SetBool("IsIdle", true);
        }

        if (idleAccu > cooldown){
            idleAccu = -1;

            int ran;
            if (idleMax > 1){
                // not same animations in a row
                ran = Random.Range(0, idleMax - 2);
                if (ran >= idleAccu){
                    ran++;
                }
            } else {
                ran = Random.Range(0, idleMax - 1);
            }
            string anim = "Idle_"+ran;
            animator.CrossFade(anim, .1f, 0, 0f, 0f);
            idleOldInt = ran;
        }
    }
}
