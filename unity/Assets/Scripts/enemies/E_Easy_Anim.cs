using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

    // TO DO 2: set animation controller script
    // TO DO 3: use animator.stringtohash to increase performance
    // TO DO 4: health system changes => delete enemy type 
// animation state controller
public class E_Easy_Anim : MonoBehaviour, IEnemyAnimation
{
    private string ANIM_IDLE_OR_TRACK = "idleOrTrack";
    private string ANIM_SHOULD_ATTACK = "shouldAttack";
    private string ANIM_SHOULD_JUMP = "shouldJump";
    private IEnemyBehavior.enemyState enemyState;
    private Animator animator; // Animator has parameters in it, which set in Unity Editor
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        updateState();
        UpdateAnimation();
    }

    private void updateState()
    {
        enemyState = E_Easy.current;
    }
    private void UpdateAnimation()
    {
        switch(enemyState) {
            case IEnemyBehavior.enemyState.Idle :
                IdleAnim();
                break;
            case IEnemyBehavior.enemyState.Track :
                TrackAnim();
                break;
            case IEnemyBehavior.enemyState.Fire :
                AttackAnim();
                break;
            case IEnemyBehavior.enemyState.Die :
                print("die animation not set yet");
                break;
        }
    }

    // ============== IEnemyAnimation implementation ============== // 
    public void IdleAnim() {
    // TO DO: add logic here
    }
    public void TrackAnim() {
    // TO DO: add logic here
    }
    public void AttackAnim() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            AnimationSetter(ANIM_SHOULD_ATTACK, true);
        } 

        if (!Input.GetKeyDown(KeyCode.Space)) {
            AnimationSetter(ANIM_SHOULD_ATTACK, false);
        }
    }
    public void DieAnim() {}
    // ============== IEnemyAnimation implementation ============== // 


    public void AnimationSetter<T>(string _name, T _condition)
    {
        if (_condition.GetType() == typeof(int))
        {
            animator.SetInteger(_name, (int.Parse(_condition.ToString())));
        }
    }
}
