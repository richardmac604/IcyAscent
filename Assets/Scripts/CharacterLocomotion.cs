using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Based on code from TheKiwiCoder: https://www.youtube.com/watch?v=_I8HsTfKep8&t=1s
//Standard assets for animation
//Mixamo for Pistol idle animation
//POLYGON starter pack https://assetstore.unity.com/packages/3d/props/polygon-starter-pack-low-poly-3d-art-by-synty-156819?aid=1011ljjCh&utm_campaign=unity_affiliate&utm_medium=affiliate&utm_source=partnerize-linkmaker
public class CharacterLocomotion : MonoBehaviour {
    Animator animator;
    Vector2 input;

    // Start is called before the first frame update
    void Start() {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        //get horizontal and vertical inputs, pass to animator to drive movement
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");

        animator.SetFloat("InputX", input.x);
        animator.SetFloat("InputY", input.y);

    }
}
