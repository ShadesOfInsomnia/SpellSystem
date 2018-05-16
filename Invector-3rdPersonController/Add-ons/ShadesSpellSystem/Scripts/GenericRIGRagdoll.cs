using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Invector;
using Invector.vCharacterController;

namespace Shadex
{
    /// <summary>
    /// A modified version of invector's ragdoll script that doesn't require human body parts
    /// </summary>
    /// <remarks>
    /// Deprecated, invector has since released generic ragdoll via template
    /// </remarks>
    [vClassHeader("Generic Ragdoll System", true, "ragdollIcon", true, "Every gameobject children of the character must have their tag added in the IgnoreTag List.")]
    public class GenericRIGRagdoll : vMonoBehaviour
    {
        #region public variables

        [vButton("Active Ragdoll", "ActivateRagdoll", typeof(vRagdoll))]
        public bool removePhysicsAfterDie;
        [Tooltip("SHOOTER: Keep false to use detection hit on each children collider, don't forget to change the layer to BodyPart from hips to all childrens. MELEE: Keep true to only hit the main Capsule Collider.")]
        public bool disableColliders = false;
        public AudioSource collisionSource;
        public AudioClip collisionClip;
        [Header("Add Tags for Weapons or Items here:")]
        public List<string> ignoreTags = new List<string>() { "Weapon", "Ignore Ragdoll" };
        public AnimatorStateInfo stateInfo;

        public Transform characterChest, characterHips, characterHead;
        public Transform[] characterToes, characterFeet;

        #endregion

        #region private variables
        [HideInInspector]
        public vCharacter iChar;
        Animator animator;


        bool inStabilize, isActive, updateBehaviour;

        bool ragdolled
        {
            get
            {
                return state != RagdollState.animated;
            }
            set
            {
                if (value == true)
                {
                    if (state == RagdollState.animated)
                    {
                        //Transition from animated to ragdolled
                        setKinematic(false); //allow the ragdoll RigidBodies to react to the environment
                        setCollider(false);
                        animator.enabled = false; //disable animation
                        state = RagdollState.ragdolled;
                    }
                }
                else
                {
                    characterHips.parent = hipsParent;
                    isActive = false;
                    if (state == RagdollState.ragdolled)
                    {
                        setKinematic(true); //disable gravity etc.
                        setCollider(true);
                        ragdollingEndTime = Time.time; //store the state change time

                        animator.enabled = true; //enable animation
                        state = RagdollState.blendToAnim;

                        //Store the ragdolled position for blending
                        foreach (BodyPart b in bodyParts)
                        {
                            b.storedRotation = b.transform.rotation;
                            b.storedPosition = b.transform.position;
                        }

                        //Remember some key positions
                        //ragdolledFeetPosition = 0.5f * (animator.GetBoneTransform(HumanBodyBones.LeftToes).position + animator.GetBoneTransform(HumanBodyBones.RightToes).position);
                        ragdolledFeetPosition = Vector3.zero;
                        foreach (Transform t in characterToes)
                        {
                            ragdolledFeetPosition += t.position;
                        }
                        ragdolledFeetPosition *= 0.5f;

                        ragdolledHeadPosition = characterHead.position; // animator.GetBoneTransform(HumanBodyBones.Head).position;
                        ragdolledHipPosition = characterHips.position; // animator.GetBoneTransform(HumanBodyBones.Hips).position;

                        //Initiate the get up animation
                        //hip hips forward vector pointing upwards, initiate the get up from back animation
                        //if (animator.GetBoneTransform(HumanBodyBones.Hips).forward.y > 0)
                        if (characterHips.forward.y > 0)
                            animator.Play("StandUp@FromBack");
                        else
                            animator.Play("StandUp@FromBelly");
                    }
                }
            }
        }

        //Possible states of the ragdoll
        enum RagdollState
        {
            animated,    //Mecanim is fully in control
            ragdolled,   //Mecanim turned off, physics controls the ragdoll
            blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
        }

        //The current state
        RagdollState state = RagdollState.animated;
        //How long do we blend when transitioning from ragdolled to animated
        float ragdollToMecanimBlendTime = 0.5f;
        float mecanimToGetUpTransitionTime = 0.05f;
        //A helper variable to store the time when we transitioned from ragdolled to blendToAnim state
        float ragdollingEndTime = -100;
        //Additional vectores for storing the pose the ragdoll ended up in.
        Vector3 ragdolledHipPosition, ragdolledHeadPosition, ragdolledFeetPosition;
        //Declare a list of body parts, initialized in Start()
        List<BodyPart> bodyParts = new List<BodyPart>();
        // used to reset parent of hips
        Transform hipsParent;
        //used to controll damage frequency
        bool inApplyDamage;

        class BodyPart
        {
            public Transform transform;
            public Vector3 storedPosition;
            public Quaternion storedRotation;
        }
        #endregion

        void Start()
        {
            // store the Animator component
            animator = GetComponent<Animator>();
            iChar = GetComponent<GenericRIGController>();
            if (!iChar)
            {
                iChar = GetComponent<vCharacter>();
            }

            if (iChar)
            {
                iChar.onActiveRagdoll.AddListener(ActivateRagdoll);
            }

            // find character chest and hips
            //characterChest = animator.GetBoneTransform(HumanBodyBones.Chest);
            //characterHips = animator.GetBoneTransform(HumanBodyBones.Hips);
            hipsParent = characterHips.parent;
            // set all RigidBodies to kinematic so that they can be controlled with Mecanim
            // and there will be no glitches when transitioning to a ragdoll
            setKinematic(true);
            setCollider(true);

            // find all the transforms in the character, assuming that this script is attached to the root
            Component[] components = GetComponentsInChildren(typeof(Transform));

            // for each of the transforms, create a BodyPart instance and store the transform 
            foreach (Component c in components)
            {
                if (!ignoreTags.Contains(c.tag))
                {
                    BodyPart bodyPart = new BodyPart();
                    bodyPart.transform = c as Transform;
                    if (c.GetComponent<Rigidbody>() != null)
                        c.tag = gameObject.tag;
                    bodyParts.Add(bodyPart);
                }
            }
        }

        void LateUpdate()
        {
            if (animator == null) return;
            if (!updateBehaviour && animator.updateMode == AnimatorUpdateMode.AnimatePhysics) return;
            updateBehaviour = false;
            RagdollBehaviour();
        }

        void FixedUpdate()
        {
            updateBehaviour = true;
            if (!isActive) return;
            if (iChar.currentHealth > 0)
            {
                if (characterHips.parent != null) characterHips.parent = null;
                if (ragdolled && !inStabilize)
                {
                    ragdolled = false;
                    StartCoroutine(ResetPlayer(1f));
                }
                else if (animator != null && !animator.isActiveAndEnabled && ragdolled || (animator == null && ragdolled))
                    transform.position = characterHips.position;
            }
        }

        void OnDestroy()
        {
            if (characterHips && characterHips.parent == null)
                Destroy(characterHips);
        }

        /// <summary>
        /// Reset the inApplyDamage variable. Set to false;
        /// </summary>
        void ResetDamage()
        {
            inApplyDamage = false;
        }

        /// <summary>
        /// Add Damage to vCharacter every 0.1 seconds
        /// </summary>
        /// <param name="damage"></param>
        public void ApplyDamage(vDamage damage)
        {
            if (isActive && ragdolled && !inApplyDamage && iChar)
            {
                inApplyDamage = true;
                iChar.TakeDamage(damage);
                Invoke("ResetDamage", 0.2f);
            }
        }


        // active ragdoll - call this method to turn the ragdoll on      
        public void ActivateRagdoll()
        {
            if (isActive)
                return;

            inApplyDamage = true;
            isActive = true;

            if (transform.parent != null) transform.parent = null;

            iChar.EnableRagdoll();

            var isDead = !(iChar.currentHealth > 0);
            // turn ragdoll on
            inStabilize = true;
            ragdolled = true;

            // start to check if the ragdoll is stable
            StartCoroutine(RagdollStabilizer(2f));

            if (!isDead)
                characterHips.parent = null;
            Invoke("ResetDamage", 0.2f);
        }

        // ragdoll collision sound        
        public void OnRagdollCollisionEnter(vRagdollCollision ragdolCollision)
        {
            if (ragdolCollision.ImpactForce > 1)
            {
                collisionSource.clip = collisionClip;
                collisionSource.volume = ragdolCollision.ImpactForce * 0.05f;
                if (!collisionSource.isPlaying)
                    collisionSource.Play();
            }
        }

        // ragdoll stabilizer - wait until the ragdoll became stable based on the chest velocity.magnitude
        IEnumerator RagdollStabilizer(float delay)
        {

            float rdStabilize = Mathf.Infinity;
            yield return new WaitForSeconds(delay);
            while (rdStabilize > (iChar.isDead ? 0.0001f : 0.1f))
            {
                if (animator != null && !animator.isActiveAndEnabled)
                {
                    rdStabilize = characterChest.GetComponent<Rigidbody>().velocity.magnitude;

                }
                else
                    break;
                yield return new WaitForEndOfFrame();
            }

            if (iChar.isDead)
            {
                //Destroy(iChar as Component);
                yield return new WaitForEndOfFrame();
                DestroyComponents();
            }
            inStabilize = false;
        }

        // reset player - restore control to the character	
        IEnumerator ResetPlayer(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            //Debug.Log("Ragdoll OFF");        

            iChar.ResetRagdoll();
        }

        // ragdoll blend - code based on the script by Perttu Hämäläinen with modifications to work with this Controller        
        void RagdollBehaviour()
        {
            var isDead = !(iChar.currentHealth > 0);
            if (isDead) return;
            if (!iChar.ragdolled) return;

            //Blending from ragdoll back to animated
            if (state == RagdollState.blendToAnim)
            {
                if (Time.time <= ragdollingEndTime + mecanimToGetUpTransitionTime)
                {
                    //If we are waiting for Mecanim to start playing the get up animations, update the root of the mecanim
                    //character to the best match with the ragdoll
                    Vector3 animatedToRagdolled = ragdolledHipPosition - characterHips.position; //animator.GetBoneTransform(HumanBodyBones.Hips).position;
                    Vector3 newRootPosition = transform.position + animatedToRagdolled;

                    //Now cast a ray from the computed position downwards and find the highest hit that does not belong to the character 
                    RaycastHit[] hits = Physics.RaycastAll(new Ray(newRootPosition + Vector3.up, Vector3.down));
                    //newRootPosition.y = 0;

                    foreach (RaycastHit hit in hits)
                    {
                        if (!hit.transform.IsChildOf(transform))
                        {
                            newRootPosition.y = Mathf.Max(newRootPosition.y, hit.point.y);
                        }
                    }
                    transform.position = newRootPosition;

                    //Get body orientation in ground plane for both the ragdolled pose and the animated get up pose
                    Vector3 ragdolledDirection = ragdolledHeadPosition - ragdolledFeetPosition;
                    ragdolledDirection.y = 0;

                    Vector3 meanFeetPosition = Vector3.zero;
                    foreach (Transform t in characterFeet)
                    {
                        meanFeetPosition += t.position;
                    }
                    meanFeetPosition *= 0.5f;
                    Vector3 animatedDirection = characterHead.position - meanFeetPosition;

                    //Vector3 meanFeetPosition = 0.5f * (animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                    //Vector3 animatedDirection = animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
                    animatedDirection.y = 0;

                    //Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
                    //hence setting the y components of the vectors to zero. 
                    transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdolledDirection.normalized);
                }
                //compute the ragdoll blend amount in the range 0...1
                float ragdollBlendAmount = 1.0f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime) / ragdollToMecanimBlendTime;
                ragdollBlendAmount = Mathf.Clamp01(ragdollBlendAmount);

                //In LateUpdate(), Mecanim has already updated the body pose according to the animations. 
                //To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips 
                //and slerp all the rotations towards the ones stored when ending the ragdolling
                foreach (BodyPart b in bodyParts)
                {
                    if (b.transform != transform)
                    { //this if is to prevent us from modifying the root of the character, only the actual body parts
                      //position is only interpolated for the hips
                        if (b.transform == characterHips) // animator.GetBoneTransform(HumanBodyBones.Hips))
                            b.transform.position = Vector3.Lerp(b.transform.position, b.storedPosition, ragdollBlendAmount);
                        //rotation is interpolated for all body parts
                        b.transform.rotation = Quaternion.Slerp(b.transform.rotation, b.storedRotation, ragdollBlendAmount);
                    }
                }

                //if the ragdoll blend amount has decreased to zero, move to animated state
                if (ragdollBlendAmount == 0)
                {
                    state = RagdollState.animated;
                    return;
                }
            }
        }

        // set all rigidbodies to kinematic
        void setKinematic(bool newValue)
        {
            var _hips = characterHips.GetComponent<Rigidbody>();
            _hips.isKinematic = newValue;
            Component[] components = _hips.transform.GetComponentsInChildren(typeof(Rigidbody));

            foreach (Component c in components)
            {
                if (!ignoreTags.Contains(c.transform.tag))
                    (c as Rigidbody).isKinematic = newValue;
            }
        }

        // set all colliders to trigger
        void setCollider(bool newValue)
        {
            if (!disableColliders) return;

            var _hips = characterHips.GetComponent<Collider>();
            _hips.enabled = !newValue;
            Component[] components = _hips.transform.GetComponentsInChildren(typeof(Collider));

            foreach (Component c in components)
            {
                if (!ignoreTags.Contains(c.transform.tag))
                    if (!c.transform.Equals(transform)) (c as Collider).enabled = !newValue;
            }
        }

        // destroy the components if the character is dead
        void DestroyComponents()
        {
            if (removePhysicsAfterDie)
            {
                var collider = GetComponent<Collider>();
                var rigidbody = GetComponent<Rigidbody>();
                Destroy(rigidbody);
                Destroy(collider);
                var joints = GetComponentsInChildren<CharacterJoint>();
                if (joints != null)
                {
                    foreach (CharacterJoint comp in joints)
                        if (!ignoreTags.Contains(comp.gameObject.tag))
                            Destroy(comp);
                }

                var rigidbodys = GetComponentsInChildren<Rigidbody>();
                if (rigidbodys != null)
                {
                    foreach (Rigidbody comp in rigidbodys)
                        if (!ignoreTags.Contains(comp.gameObject.tag))
                            Destroy(comp);
                }

                var colliders = GetComponentsInChildren<Collider>();
                if (colliders != null)
                {
                    foreach (Collider comp in colliders)
                        if (!ignoreTags.Contains(comp.gameObject.tag))
                            Destroy(comp);
                }
            }

            //var scripts = GetComponentsInChildren<MonoBehaviour>();
            //if (scripts != null)
            //{
            //    foreach (MonoBehaviour comp in scripts)
            //        if (!ignoreTags.Contains(comp.gameObject.tag))
            //            DestroyObject(comp);
            //}
        }
    }
}

/* *****************************************************************************************************************************
 * Copyright        : 2017 Shades of Insomnia
 * Founding Members : Charles Page (Shade)
 *                  : Rob Alexander (Insomnia)
 * License          : Attribution-ShareAlike 4.0 International (CC BY-SA 4.0) https://creativecommons.org/licenses/by-sa/4.0/
 * *****************************************************************************************************************************
 * You are free to:
 *     Share        : copy and redistribute the material in any medium or format.
 *     Adapt        : remix, transform, and build upon the material for any purpose, even commercially. 
 *     
 * The licensor cannot revoke these freedoms as long as you follow the license terms.
 * 
 * Under the following terms:
 *     Attribution  : You must give appropriate credit, provide a link to the license, and indicate if changes were made. You may 
 *                    do so in any reasonable manner, but not in any way that suggests the licensor endorses you or your use.
 *     ShareAlike   : If you remix, transform, or build upon the material, you must distribute your contributions under the same 
 *                    license as the original. 
 *                  
 * You may not apply legal terms or technological measures that legally restrict others from doing anything the license permits. 
 * *****************************************************************************************************************************/
