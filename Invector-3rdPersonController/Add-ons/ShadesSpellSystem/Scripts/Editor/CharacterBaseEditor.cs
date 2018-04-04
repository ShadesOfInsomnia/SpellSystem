using UnityEngine;
using UnityEditor;
using System.Collections;
using Invector;
using System;


namespace Shadex
{
    /// <summary>
    /// Custom editor base shared between the spell attack behavior/spell book detail inspector.
    /// </summary>
    public abstract class SpellBookBaseEditor : Editor
    {
        /// <summary>Invector skin.</summary>
        protected GUISkin skin;

        /// <summary>Unity skin.</summary>
        protected GUISkin defaultSkin;

        /// <summary>Logo for the script.</summary>
        protected Texture2D m_Logo;

        /// <summary>
        /// Occurs when the script is initially enabled, loads the skin.
        /// </summary>
        void OnEnable()
        {
            skin = Resources.Load("skin") as GUISkin;
            m_Logo = (Texture2D)Resources.Load("ammoIcon", typeof(Texture2D));
        }

        /// <summary>
        /// Spell header info used to create the animator state.
        /// </summary>
        protected abstract void DisplaySpellHeader(SpellBookEntrySubType SubType);

        /// <summary>
        /// Is the spell expanded, true/false for the spell book but always true for the spell attack behavior.
        /// </summary>
        /// <returns>Whether expanded.</returns>
        protected abstract bool IsExpanded();

        /// <summary>
        /// Information specific to the spell book regarding the spell
        /// </summary>
        protected abstract void DisplaySpellInfo(SpellBookEntrySubType SubType);

        /// <summary>
        /// Display the spell sub detail options, shared between the spell book and attack behavior.
        /// </summary>
        /// <param name="SpellDetail">Spell book entry to build the GUI from.</param>
        /// <param name="DetailType">Type of spell, casting, charge, hold or release.</param>
        protected void DisplaySpellSubDetail(SpellBookEntry SpellDetail)
        {
            // spell entry container title
            switch (SpellDetail.SubType)
            {
                case SpellBookEntrySubType.Charge:
                    GUILayout.BeginVertical(" " + SpellDetail.SpellName + " Charge", "window", GUILayout.ExpandWidth(true));
                    break;
                case SpellBookEntrySubType.Hold:
                    GUILayout.BeginVertical(" " + SpellDetail.SpellName + " Hold", "window", GUILayout.ExpandWidth(true));
                    break;
                case SpellBookEntrySubType.Release:
                    GUILayout.BeginVertical(" " + SpellDetail.SpellName + " Release", "window", GUILayout.ExpandWidth(true));
                    break;
                default:  // casting
                    GUILayout.BeginVertical(" " + SpellDetail.SpellName, "window", GUILayout.ExpandWidth(true));
                    break;
            }
            

            // icon
            GUILayout.BeginHorizontal();
            if (SpellDetail.Icon != null)
            {
                DrawTextureGUI(GUILayoutUtility.GetRect(25, 25), SpellDetail.Icon, new Vector2(25, 25));
            }

            // header display
            DisplaySpellHeader(SpellDetail.SubType);


            // has spell detail
            if (SpellDetail != null)
            {
                // spell detail is expanded
                if (IsExpanded())
                {
                    DisplaySpellInfo(SpellDetail.SubType);

                    // limb particle types
                    EditorGUILayout.LabelField("Limb Particles?", EditorStyles.boldLabel);
                    SpellDetail.attackLimb = (AvatarIKGoal)EditorGUILayout.EnumPopup("1st Attack Limb:", SpellDetail.attackLimb, GUILayout.ExpandWidth(true));
                    SpellDetail.LimbParticleEffect = EditorGUILayout.ObjectField("1st Attack Particle: ", SpellDetail.LimbParticleEffect, typeof(GameObject), false) as GameObject;
                    SpellDetail.attackLimb2 = (AvatarIKGoal)EditorGUILayout.EnumPopup("2nd Attack Limb:", SpellDetail.attackLimb2, GUILayout.ExpandWidth(true));
                    if (SpellDetail.attackLimb != SpellDetail.attackLimb2)
                    {
                        SpellDetail.LimbParticleEffect2 = EditorGUILayout.ObjectField("2nd Attack Particle: ", SpellDetail.LimbParticleEffect2, typeof(GameObject), false) as GameObject;
                    }
                    if (!(SpellDetail.LimbParticleEffect == null && SpellDetail.LimbParticleEffect2 != null)) SpellDetail.DoNotPoolLimbParticles = EditorGUILayout.Toggle("Do Not Pool", SpellDetail.DoNotPoolLimbParticles);

                    // add new spawn button
                    EditorGUILayout.LabelField("Spawn Prefabs", EditorStyles.boldLabel);
                    if (GUILayout.Button("Add New"))
                    {
                        SpellDetail.SpawnOverTime.Add(new SpawnerOptionsOverTime()
                        {
                            SpawnStartTime = 0.5f,
                            SpawnEndTime = 0.5f,
                            NumberToSpawn = 1,
                            DestructionTimeOut = 4f,
                            RandomRotate = new RandomRotateOptions(),
                            RandomSphere = new RandomSphereOptions(),
                            PhysicsForceOptions = new PhysicsOptions()
                            {
                                Radius = 10,
                                Force = 20,
                                UpwardsForce = 3
                            }
                        });
                    }

                    // list of spawn ables                    
                    for (int s = 0; s < SpellDetail.SpawnOverTime.Count; s++)
                    {
                        // container
                        GUILayout.BeginVertical("box");
                        SpawnerOptionsOverTime SpawnDetail = SpellDetail.SpawnOverTime[s];

                        // start/end time
                        GUILayout.BeginHorizontal();
                        SpawnDetail.SpawnStartTime = EditorGUILayout.FloatField("Spawn Start:", SpawnDetail.SpawnStartTime);
                        GUILayout.Label("End:", GUILayout.Width(50));
                        SpawnDetail.SpawnEndTime = EditorGUILayout.FloatField(SpawnDetail.SpawnEndTime);
                        GUILayout.EndHorizontal();

                        // prefab                                                
                        SpawnDetail.Prefab = EditorGUILayout.ObjectField("Prefab: ", SpawnDetail.Prefab, typeof(GameObject), false) as GameObject;
                        if (SpawnDetail.Prefab)
                        {
                            GUILayout.BeginHorizontal();
                            SpawnDetail.NumberToSpawn = EditorGUILayout.IntField("Spawn Quantity:", SpawnDetail.NumberToSpawn);
                            GUILayout.Label("Timeout:", GUILayout.Width(50));
                            SpawnDetail.DestructionTimeOut = EditorGUILayout.FloatField(SpawnDetail.DestructionTimeOut);
                            GUILayout.EndHorizontal();
                        }

                        // audio clip
                        SpawnDetail.PlayOnSpawn = EditorGUILayout.ObjectField("Audio Clip: ", SpawnDetail.PlayOnSpawn, typeof(AudioClip), false) as AudioClip;

                        // audio source
                        if (SpawnDetail.PlayOnSpawn)
                        {
                            SpawnDetail.SourceOfAudio = EditorGUILayout.ObjectField("Audio Source: ", SpawnDetail.SourceOfAudio, typeof(AudioSource), false) as AudioSource;
                        }

                        // advanced options
                        if (SpawnDetail.ShowAdvancedOptions && SpawnDetail.Prefab)
                        {
                            // offset/rotation
                            EditorGUILayout.LabelField("Advanced Spawning", EditorStyles.boldLabel);
                            SpawnDetail.Offset = EditorGUILayout.Vector3Field("Offset: ", SpawnDetail.Offset);
                            SpawnDetail.Angle = EditorGUILayout.Vector3Field("Angle: ", SpawnDetail.Angle);

                            // pooling                            
                            GUILayout.BeginHorizontal();
                            SpawnDetail.DoNotPool = EditorGUILayout.Toggle("Do Not Pool:", SpawnDetail.DoNotPool);
                            if (!SpawnDetail.DoNotPool)
                            {
                                GUILayout.Label("Slot ID:", GUILayout.Width(65));
                                SpawnDetail.PoolSlotId = EditorGUILayout.IntField(SpawnDetail.PoolSlotId);
                            }
                            GUILayout.EndHorizontal();

                            // enable physics / empty space
                            GUILayout.BeginHorizontal();
                            SpawnDetail.PhysicsForceOptions.Enabled = EditorGUILayout.Toggle("Enable Physics:", SpawnDetail.PhysicsForceOptions.Enabled);
                            GUILayout.Label("Min Space:", GUILayout.Width(65));
                            SpawnDetail.EmptySpaceRadius = EditorGUILayout.FloatField(SpawnDetail.EmptySpaceRadius);
                            GUILayout.EndHorizontal();

                            // physics options
                            if (SpawnDetail.PhysicsForceOptions.Enabled)
                            {
                                // explosion radius
                                GUILayout.BeginHorizontal();
                                SpawnDetail.PhysicsForceOptions.Radius = EditorGUILayout.FloatField("Radius/Mode:", SpawnDetail.PhysicsForceOptions.Radius);

                                // type of physics to apply
                                SpawnDetail.PhysicsForceOptions.Mode = (ForceMode)EditorGUILayout.EnumPopup(SpawnDetail.PhysicsForceOptions.Mode, GUILayout.ExpandWidth(true));
                                GUILayout.EndHorizontal();

                                // amount of force
                                GUILayout.BeginHorizontal();
                                SpawnDetail.PhysicsForceOptions.Force = EditorGUILayout.FloatField("Force/Upwards:", SpawnDetail.PhysicsForceOptions.Force);
                                SpawnDetail.PhysicsForceOptions.UpwardsForce = EditorGUILayout.FloatField(SpawnDetail.PhysicsForceOptions.UpwardsForce);
                                GUILayout.EndHorizontal();
                            }

                            // parenting
                            GUILayout.BeginHorizontal();
                            SpawnDetail.KeepParent = EditorGUILayout.Toggle("Keep Parent:", SpawnDetail.KeepParent);

                            // spawn location
                            SpawnDetail.UseRootTransform = EditorGUILayout.Toggle("Use Root Transform:", SpawnDetail.UseRootTransform);
                            GUILayout.EndHorizontal();

                            // random rotate
                            EditorGUILayout.LabelField("Randomization", EditorStyles.boldLabel);
                            GUILayout.BeginHorizontal();
                            SpawnDetail.RandomRotate.IncludeX = EditorGUILayout.Toggle("Rnd Rotate X:", SpawnDetail.RandomRotate.IncludeX);
                            GUILayout.Label("Y:", GUILayout.Width(15));
                            SpawnDetail.RandomRotate.IncludeY = EditorGUILayout.Toggle(SpawnDetail.RandomRotate.IncludeY, GUILayout.Width(20));
                            GUILayout.Label("Z:", GUILayout.Width(15));
                            SpawnDetail.RandomRotate.IncludeZ = EditorGUILayout.Toggle(SpawnDetail.RandomRotate.IncludeZ, GUILayout.Width(20));
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();

                            // random sphere
                            SpawnDetail.RandomSphere.Radius = EditorGUILayout.FloatField("Rnd Sphere Radius: ", SpawnDetail.RandomSphere.Radius);
                            if (SpawnDetail.RandomSphere.Radius < 0) SpawnDetail.RandomSphere.Radius = 0;
                            if (SpawnDetail.RandomSphere.Radius > 0)
                            {
                                GUILayout.BeginHorizontal();
                                SpawnDetail.RandomSphere.IncludeX = EditorGUILayout.Toggle("Rnd Sphere X:", SpawnDetail.RandomSphere.IncludeX);
                                GUILayout.Label("Y:", GUILayout.Width(15));
                                SpawnDetail.RandomSphere.IncludeY = EditorGUILayout.Toggle(SpawnDetail.RandomSphere.IncludeY, GUILayout.Width(20));
                                GUILayout.Label("Z:", GUILayout.Width(15));
                                SpawnDetail.RandomSphere.IncludeZ = EditorGUILayout.Toggle(SpawnDetail.RandomSphere.IncludeZ, GUILayout.Width(20));
                                GUILayout.FlexibleSpace();
                                GUILayout.EndHorizontal();
                            }                                                   
                        }

                        // delete this spawn
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Delete"))
                        {
                            SpellDetail.SpawnOverTime.RemoveAt(s);  // delete this entry
                            break;  // force redisplay of the list
                        }

                        // show expanded options
                        if (SpawnDetail.Prefab)
                        {
                            if (GUILayout.Button((SpawnDetail.ShowAdvancedOptions ? "Less" : "More")))
                            {
                                SpawnDetail.ShowAdvancedOptions = !SpawnDetail.ShowAdvancedOptions;
                            }
                        }
                        GUILayout.EndHorizontal();

                        // end of container
                        GUILayout.EndVertical();
                    }
                }
            }

            // end of container
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draw a sprite on the interface, used to display the icon from the inventory.
        /// </summary>
        /// <param name="position">Position to draw at.</param>
        /// <param name="sprite">Sprite to draw.</param>
        /// <param name="size">Size to draw.</param>
        protected void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;

            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
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
