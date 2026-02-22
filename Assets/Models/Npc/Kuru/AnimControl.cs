using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LookAtController : MonoBehaviour
{
    public Transform objectToLookAt;
    public float headWeight;
    public float bodyWeight;
    private Animator animator;
    public float distanceWithPlayer;
    public float lookDistance;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

    }

    private void Update()
    {
        distanceWithPlayer = Vector3.Distance(transform.position, objectToLookAt.position);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (distanceWithPlayer < lookDistance)
        {
            animator.SetLookAtPosition(objectToLookAt.position);
            animator.SetLookAtWeight(1, bodyWeight, headWeight);
        }      
    }
}