using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

#if !VANILLA
using Invector;
using Invector.CharacterController;
#endif

namespace Shadex
{
    #region "Base character class"

    /// <summary>
    /// Abstract base class that the character instance class (aka leveling system) is derived from.
    /// </summary>
    public abstract class CharacterBase : MonoBehaviour
    {
        /// <summary>Save slot id for the data layer.</summary>
        public int SaveSlotID;                                  

        /// <summary>Save game id for the data layer.</summary>
        public int LastSaveGameID;                              

        /// <summary>Name of character for dialog/display.</summary>
        public string Name;                                    

        /// <summary>Current level of the character.</summary>
        public int CurrentLevel;                               

        /// <summary>XP collected so far.</summary>
        public int CurrentXP;                                   

        /// <summary>Current life level.</summary>
        public float CurrentLife;                            

        /// <summary>Current mana level.</summary>
        public float CurrentMana;                                    

        /// <summary>Current armour value (from class or static armour).</summary>
        public float CurrentArmour;                            

        /// <summary>Life regen per second.</summary>
        public float RegenLife;                                   

        /// <summary>Mana regen per second.</summary>
        public float RegenMana;                                 

        /// <summary>Next level up requirement XP.</summary>
        public int XPToNextLevel;                                     

        /// <summary>Maximum life available.</summary>
        public int MAXLife;                                            

        /// <summary>Maximum stamina available.</summary>
        public int MAXStamina;                                         

        /// <summary>Maximum mana available.</summary>
        public int MAXMana;                                             

        /// <summary>Maximum equipment load out.</summary>
        public int MAXEquipLoad;                               
        

        /// <summary>Current total equipment weight.</summary>
        public float CurrentEquipLoad;                           

        /// <summary>Bonus armour from equipment.</summary>
        public float BonusArmour;                               

        /// <summary>Bonus maximum life from equipment.</summary>
        public int BonusMAXLife;                               

        /// <summary>Bonus life regen per second from equipment.</summary>
        public float BonusRegenLife;                            

        /// <summary>Bonus maximum mana from equipment.</summary>
        public int BonusMAXMana;                              

        /// <summary>Bonus mana regen per second from equipment.</summary>
        public float BonusRegenMana;                            

        /// <summary>Bonus maximum stamina from equipment.</summary>
        public int BonusMAXStamina;                             

        /// <summary>Skill points stack.</summary>
        public List<BaseSkillValue> Skills;                

        /// <summary>Cache of skill modifier totals from equipment.</summary>
        public List<BaseSkillValue> SkillModTotals;             

        /// <summary>Remaining skill points to distribute.</summary>
        public int UnspentSkillPoints;                          

        /// <summary>Stack of resistances.</summary>
        public List<BaseResistPercent> Resist;                 

        /// <summary>Cache of resistance modifier totals.</summary>
        public List<BaseResistPercent> ResistModTotals;         

        /// <summary>Axis of the class (Lawful, Chaotic etc).</summary>
        public BaseAxis CurrentAxis;                           

        /// <summary>Alignment of the class (Good, Evil).</summary>
        public BaseAlignment CurrentAlignment;                 

        /// <summary>Character race.</summary>
        public BaseRace CurrentRace;                           

        /// <summary>Character class.</summary>
        public BaseClass CurrentClass;

        /// <summary>Character military rank.</summary>
        public BaseRank CurrentRank;

        /// <summary>Attribute modifier stack.</summary>
        public List<BaseValue> Modifiers;                       

        /// <summary>2 secs stacks of DoTs.</summary>
        public List<MagicDamageOverTime> DoubleDoTs;           

        /// <summary>.</summary>
        public List<MagicDamageOverTime> DoTs;                 

        /// <summary>0.5 secs stacks of DoTs.</summary>
        public List<MagicDamageOverTime> HalfDoTs;              

        /// <summary>0.25 secs stacks of DoTs.</summary>
        public List<MagicDamageOverTime> QuarterDoTs;           

        /// <summary>Collectables eg gold, gems etc.</summary>
        public List<BaseCollection> Collectables;

        /// <summary>Conditions child game object pointer.</summary>
        public Transform ConditionsRoot;

        /// <summary>Conditions child game object pointers.</summary>
        public List<BaseCondition> Conditions;

        /// <summary>Attribute changes publisher event.</summary>
        public event UpdateHUDHandler NotifyUpdateHUD;          

        /// <summary>Attribute changes publisher delegate.</summary>
        public delegate void UpdateHUDHandler(CharacterBase cb, CharacterUpdated e);

        /// <summary>When enabled, core stats will get updated into the animator.</summary>
        /// <remarks>An int animator parameter must exist for each stat, prefixed by "Core_".</remarks>
        public bool FeedStatsToAnimator;
        
        /// <summary>CoRoutine enabled for per two second operations.</summary>
        protected bool bPerDblSecondRunning;

        /// <summary>CoRoutine enabled for per second operations.</summary>
        protected bool bPerSecondRunning;

        /// <summary>CoRoutine enabled for per half second operations.</summary>
        protected bool bPerHalfSecondRunning;

        /// <summary>CoRoutine enabled for per quarter second operations.</summary>
        protected bool bPerQtrSecondRunning;

        /// <summary>Cache of the animator for the attached character.</summary>
        protected Animator TheAnimator;

        /// <summary>Cache of the base class for the attached character.</summary>
        protected vCharacter TheCharacter;

        /// <summary>Cache the hash to the Core_Level parameter ID.</summary>
        public static readonly int param_CoreLevel = Animator.StringToHash("Core_Level");

        /// <summary>Cache the hash to the Core_Life parameter ID.</summary>
        public static readonly int param_CoreLife = Animator.StringToHash("Core_Life");

        /// <summary>Cache the hash to the Core_Mana parameter ID.</summary>
        public static readonly int param_CoreMana = Animator.StringToHash("Core_Mana");

        /// <summary>Cache the hash to the Core_Stamina parameter ID.</summary>
        public static readonly int param_CoreStamina = Animator.StringToHash("Core_Stamina");

        /// <summary>Cache the hash to the Core_EquipLoad parameter ID.</summary>
        public static readonly int param_CoreEquipLoad = Animator.StringToHash("Core_EquipLoad");

       


        /// <summary>
        /// Initialise the character on script add.
        /// </summary>
        public CharacterBase()
        {
            ResetToDefaults();
        } 

        /// <summary>
        /// Find components, add on dead listener for the collectables drop.
        /// </summary>
        public virtual void Start()
        {
#if !VANILLA// add the drop all collectables link
            TheCharacter = GetComponent<vCharacter>();
            TheCharacter.onDead.AddListener(DropAllCollectables);
#endif

            // handle collectables
            if (gameObject.tag == "Player")
            {
                GlobalFuncs.Collectables = Collectables;  // pass current prefab setup to global for fast access
            }

            // set initial animator core stats?
            if (FeedStatsToAnimator)
            {
                TheAnimator = GetComponent<Animator>();
                if (TheAnimator)
                {
                    StatsToAnimator(false);
                }
                else  // failsafe, but disable when no animator
                {
                    FeedStatsToAnimator = false;  // should never happen unless legacy character
                }
            }
        }  

        /// <summary>
        /// Enable repeating operations when enabled.
        /// </summary>
        public virtual void OnEnable()
        {
            // reinitialise DoTs
            DoubleDoTs = new List<MagicDamageOverTime>();
            DoTs = new List<MagicDamageOverTime>();
            HalfDoTs = new List<MagicDamageOverTime>();
            QuarterDoTs = new List<MagicDamageOverTime>();

            // handle per second operations
            if (Application.isPlaying && ((RegenLife + BonusRegenLife) > 0 || (RegenMana + BonusRegenMana) > 0))
            {
                bPerSecondRunning = true;
                InvokeRepeating("PerSecondOperations", 1f, 1f);  // 1s delay, repeat every 1s
            }
        }  

        /// <summary>
        /// Disable repeating operations when disabled failsafe.
        /// </summary>
        public virtual void OnDisable()
        {
            // ensure per second operations are disabled
            if (bPerDblSecondRunning)
            {
                CancelInvoke("PerDoubleSecondOperations");
                bPerDblSecondRunning = false;
            }
            if (bPerSecondRunning)
            {
                CancelInvoke("PerSecondOperations");
                bPerSecondRunning = false;
            }
            if (bPerHalfSecondRunning)
            {
                CancelInvoke("PerHalfSecondOperations");
                bPerHalfSecondRunning = false;
            }
            if (bPerQtrSecondRunning)
            {
                CancelInvoke("PerQuarterSecondOperations");
                bPerQtrSecondRunning = false;
            }
        }  

        /// <summary>
        /// Reset the class core to the defaults.
        /// </summary>
        public virtual void ResetToDefaults()
        {
            // core
            CurrentLevel = 1;
            UnspentSkillPoints = CharacterDefaults.LEVELUP_INITIAL_UNSPENT_POINTS;
            CurrentXP = 0;
            XPToNextLevel = CalcXPToNextLevel(CurrentLevel);
            CurrentEquipLoad = 5;  //temp value, needs to calc from armour)

            // build Skills from defaults            
            Skills = new List<BaseSkillValue>();
            SkillModTotals = new List<BaseSkillValue>();
            BaseSkill[] SkillValues = (BaseSkill[])Enum.GetValues(typeof(BaseSkill));
            for (int i = 0; i < SkillValues.Length; i++)
            {
                Skills.Add(new BaseSkillValue() { Skill = SkillValues[i], Value = CharacterDefaults.SKILLS_INITIAL_VALUE });
                SkillModTotals.Add(new BaseSkillValue() { Skill = SkillValues[i], Value = 0 });
            }

            // build resistances and relink condition child game objects                
            Conditions = new List<BaseCondition>();
            Resist = new List<BaseResistPercent>();
            ResistModTotals = new List<BaseResistPercent>();
            BaseDamage[] ResistValues = (BaseDamage[])Enum.GetValues(typeof(BaseDamage));
            for (int i = 0; i < ResistValues.Length; i++)
            {
                // insert resistance
                Resist.Add(new BaseResistPercent() { Resist = ResistValues[i], Value = 0 });
                ResistModTotals.Add(new BaseResistPercent() { Resist = ResistValues[i], Value = 0 });

                // insert condition prefab link
                Conditions.Add(new BaseCondition() { Type = ResistValues[i], Display = FindInActiveChild(ConditionsRoot, ResistValues[i].ToString()) });
            }

            // collectables
            BaseCollectable[] CollectionValues = (BaseCollectable[])Enum.GetValues(typeof(BaseCollectable));
            bool bReApply = true;
            if (Collectables != null)  // attempt to keep values
            {
                try
                {
                    for (int i = 0; i < CollectionValues.Length; i++)
                    {
                        if (Collectables[i].Type == CollectionValues[i])
                        {
                            Collectables[i].Value = 0;  // reset to zero
                        }
                        else
                        {
                            break;  // dropout, values have changed
                        }
                    }
                    bReApply = false;  // all matched, no need to clear
                }
                catch { }                
            }
            if (bReApply)  // recreate collectables from the enum
            {
                Collectables = new List<BaseCollection>();
                for (int i = 0; i < CollectionValues.Length; i++)
                {
                    Collectables.Add(new BaseCollection() { Type = CollectionValues[i], Value = 0, Spawns = new List<CollectablePrefab>() });
                }
            }

            // rebuild the skill/resistance modifiers
            Modifiers = new List<BaseValue>();
            RebuildModifiers();

        } 

        /// <summary>
        /// Notify HUD update subscribers of attribute change.
        /// </summary>
        public virtual void ForceUpdateHUD()
        {
            if (NotifyUpdateHUD != null)
            {  // has subscribers
                CharacterUpdated cu = new CharacterUpdated()
                {
                    XP = CurrentXP,
                    Level = CurrentLevel,
                    Life = CurrentLife,
                    LifeMAX = (MAXLife + BonusMAXLife),
                    Mana = CurrentMana,
                    ManaMAX = (MAXMana + BonusMAXMana),
                    StaminaMAX = (MAXStamina + BonusMAXStamina)
                };  // create character attribute event class
                NotifyUpdateHUD(this, cu);  // notify all subscribers of the update
            }
        }

        /// <summary>
        /// Feeds the animator ALL the current stats from character.
        /// </summary>
        /// <param name="JustSkills">Only update the skill points.</param>
        public virtual void StatsToAnimator(bool JustSkills)
        {
            if (FeedStatsToAnimator)
            {
                if (!JustSkills)
                {
                    StatsToAnimator(param_CoreLevel);
                    StatsToAnimator(param_CoreLife);
                    StatsToAnimator(param_CoreMana);
                    StatsToAnimator(param_CoreStamina);
                    StatsToAnimator(param_CoreEquipLoad);

                    TheAnimator.SetInteger("Core_Axis", (int)CurrentAxis);
                    TheAnimator.SetInteger("Core_Alignment", (int)CurrentAlignment);
                    TheAnimator.SetInteger("Core_Rank", (int)CurrentRank);
                }

                string[] SkillNames = Enum.GetNames(typeof(BaseSkill));
                for (int i = 0; i < SkillNames.Length; i++)
                {
                    TheAnimator.SetInteger("Core_" + SkillNames[i], (int)Skills[i].Value + (int)SkillModTotals[i].Value);
                }
            }
        }

        /// <summary>
        /// Feeds the animator a specific stat from character.
        /// </summary>
        /// <param name="CoreStatHash">Hash of the animator parameter name.</param>
        public virtual void StatsToAnimator(int CoreStatHash)
        {
            if (FeedStatsToAnimator)
            {
                int NewValue = 0;
                if (CoreStatHash == param_CoreStamina)
                {
                    NewValue = (int)((TheCharacter.currentStamina / MAXStamina) * 100);
                }
                else if (CoreStatHash == param_CoreLife)
                {
                    NewValue = (int)((CurrentLife / MAXLife) * 100);
                }
                else if (CoreStatHash == param_CoreMana)
                {
                    NewValue = (int)((CurrentMana / MAXMana) * 100);
                }
                else if (CoreStatHash == param_CoreEquipLoad)
                {
                    NewValue = (int)((CurrentEquipLoad / MAXEquipLoad) * 100);
                }
                else if (CoreStatHash == param_CoreLevel)
                {
                    NewValue = CurrentLevel;
                }

                TheAnimator.SetInteger(CoreStatHash, NewValue);
            }
        }

        /// <summary>
        /// Event hook for when a mana potion is drunk.
        /// </summary>
        /// <param name="ManaIncrease">Amount of mana drunk.</param>
        public virtual void AddMana(int ManaIncrease)
        {
            CurrentMana += ManaIncrease;
            if (CurrentMana > (MAXMana + BonusMAXMana)) CurrentMana = MAXMana;  // limit mana gain to max mana
            StatsToAnimator(param_CoreMana);
            ForceUpdateHUD();
        }

        /// <summary>
        /// Link to event onUseMana to apply the spell mana cost from the vItem attribute . 
        /// </summary>
        /// <param name="ManaCost">Mana cost of spell or ability.</param>
        public virtual void UseMana(int ManaCost)
        {
            CurrentMana -= ManaCost;  // subtract the used mana
            StatsToAnimator(param_CoreMana);
            ForceUpdateHUD();
        }

        /// <summary>
        /// Add XP and potentially level up, linked to AI death event.
        /// </summary>
        /// <param name="Value"></param>
        public virtual void AddXP(int Value)
        {
            CurrentXP += Value;
            if (CurrentXP > XPToNextLevel)
            {  // level up ?
                CurrentLevel += 1;
                XPToNextLevel = CalcXPToNextLevel(CurrentLevel);
                StatsToAnimator(param_CoreLevel);
            }
            ForceUpdateHUD();
        }  


        /// <summary>
        /// Max mana potion drunk.
        /// </summary>
        public virtual void SetManaMAX()
        {
            CurrentMana = (MAXMana + BonusMAXMana);
            StatsToAnimator(param_CoreMana);
            ForceUpdateHUD();
        }

        /// <summary>
        /// Triggered when collider takes a hit on this player/NPC, linked to invector hit event.
        /// </summary>
        /// <param name="Hit">Info about the hit.</param>
        public virtual void OnSendHit(vHitInfo Hit)
        {
            if (Hit.attackObject is vMeleeWeapon)
            {
                MagicObjectDamage mdamage = Hit.attackObject.GetComponent<MagicObjectDamage>();  // attempt grab magic damage
                var levelingAI = Hit.targetCollider.GetComponent<CharacterBase>();
                if (levelingAI) levelingAI.OnRecieveMagicDamage(mdamage, 0);
                StatsToAnimator(param_CoreStamina);
            }
        }

        /// <summary>
        /// Triggered when invector vObjectDamage strikes this player/NPC, linked to invector damage event.
        /// </summary>
        /// <param name="damage">Info about the damage received.</param>
        public virtual void OnRecieveDamage(vDamage Damage)
        {
            MagicObjectDamage mdamage = Damage.sender.GetComponent<MagicObjectDamage>();  // attempt grab magic damage
            OnRecieveMagicDamage(mdamage, Damage.damageValue);
        }  

        /// <summary>
        /// Damage mitigation by elemental type.
        /// </summary>
        /// <param name="MagicDamage">Magic damage data.</param>
        /// <param name="Damage">Physical damage amount.</param>
        public virtual void OnRecieveMagicDamage(MagicObjectDamage MagicDamage, float Damage)
        {
            StatsToAnimator(param_CoreLife);
            if (Damage > 0)
            {
                CurrentLife -= MitigateDamge(BaseDamage.Physical, Damage);  // apply physical damage
            }
            if (MagicDamage)
            {  // found?
                bool FoundDblDoT = false;
                bool FoundDoT = false;
                bool FoundHalfDoT = false;
                bool FoundQtrDoT = false;
                foreach (MagicDamageOverTime md in MagicDamage.Damage)
                {  // process all magic damage types                    
                    if (md.Value > 0)  // zero failsafe
                    {   // apply instant damage
                        CurrentLife -= MitigateDamge(md.Type, md.Value);  // apply magic damage

                        // apply damage over time?
                        float ConditionLength = 1f;
                        if (md.DOTLength > 0)
                        {
                            switch (md.DOTFrequency)
                            {
                                case UpdateFrequency.TwoSeconds:
                                    FoundDblDoT = true;
                                    ConditionLength += (2f * md.DOTLength); 
                                    DoubleDoTs.Add(md);
                                    break;
                                case UpdateFrequency.HalfSecond:
                                    FoundHalfDoT = true;
                                    ConditionLength += (0.5f * md.DOTLength);
                                    HalfDoTs.Add(md);
                                    break;
                                case UpdateFrequency.QuarterSecond:
                                    FoundQtrDoT = true;
                                    ConditionLength += (0.25f * md.DOTLength);
                                    QuarterDoTs.Add(md);
                                    break;
                                default:
                                    FoundDoT = true;
                                    ConditionLength += md.DOTLength;
                                    DoTs.Add(md);
                                    break;
                            } 
                        }

                        // attempt enable condition                        
                        if (Conditions[(int)md.Type].Display != null)  // has a condition
                        {
                            Conditions[(int)md.Type].Display.SetActive(true);  // enable
                            Conditions[(int)md.Type].Length += ConditionLength;  // increase the length of the condition
                            FoundDoT = true;  // enable per second operations to count down the condition length
                        }
                        else
                        {
                            if (GlobalFuncs.DEBUGGING_MESSAGES)
                            {
                                Debug.Log("No Condition for " + transform.name + " with " + md.Type.ToString() + " damage");
                            }
                        }
                    }                    
                }

                // area of effect damage
                if (MagicDamage.AOERadius > 0) // AOE enabled?
                {   // find targets of the correct type                    
                    List<Transform> listTargetsInRange;
                    switch (MagicDamage.AOETarget)
                    {
                        case SpawnTarget.Friend:
                            listTargetsInRange = GlobalFuncs.FindAllTargetsWithinRange(
                                transform.localPosition, MagicDamage.AOERadius,
                                GlobalFuncs.targetingLayerMaskFriend,
                                GlobalFuncs.targetingTagsFriend,
                                false, 0f, true);
                            break;
                        case SpawnTarget.Enemy:
                            listTargetsInRange = GlobalFuncs.FindAllTargetsWithinRange(
                                transform.localPosition, MagicDamage.AOERadius,
                                GlobalFuncs.targetingLayerMaskEnemy,
                                GlobalFuncs.targetingTagsEnemy,
                                false, 0f, true);
                            break;
                        default:
                            listTargetsInRange = GlobalFuncs.FindAllTargetsWithinRange(
                                transform.localPosition, MagicDamage.AOERadius,
                                GlobalFuncs.targetingLayerMaskAll,
                                GlobalFuncs.targetingTagsAll,
                                false, 0f, true);
                            break;
                    }

                    // clear AOE radius preventing endless loop
                    MagicDamage.AOERadius = 0;

                    // apply damage to all targets in range
                    if (listTargetsInRange != null)  // fail safe
                    {  
                        if (listTargetsInRange.Count > 0)
                        {  // targets found
                            foreach (Transform tTarget in listTargetsInRange)
                            {  // process all targets
                                if (tTarget != transform)
                                {  // ensure no endless loop by reapplying to self
                                    CharacterBase cb = tTarget.GetComponent<CharacterBase>();  // grab leveling system
                                    if (cb)  
                                    {   // found
                                        cb.OnRecieveMagicDamage(MagicDamage, Damage);  // pass across the damage to target
                                    }
                                }
                            }
                        }
                    }
                }

                // start per quarter second operations if DoTs added to the stack
                if (FoundDblDoT && !bPerDblSecondRunning)
                {
                    bPerDblSecondRunning = true;
                    InvokeRepeating("PerDoubleSecondOperations", 2f, 2f);  // 1/4s delay, repeat every 1/4s
                }

                // start per second operations if DoTs added to the stack
                if (FoundDoT && !bPerSecondRunning)
                {
                    bPerSecondRunning = true;
                    InvokeRepeating("PerSecondOperations", 1f, 1f);  // 1s delay, repeat every 1s
                }

                // start per quarter second operations if DoTs added to the stack
                if (FoundHalfDoT && !bPerHalfSecondRunning)
                {
                    bPerHalfSecondRunning = true;
                    InvokeRepeating("PerHalfSecondOperations", .5f, .5f);  // 1/4s delay, repeat every 1/4s
                }

                // start per quarter second operations if DoTs added to the stack
                if (FoundQtrDoT && !bPerQtrSecondRunning)
                {
                    bPerQtrSecondRunning = true;
                    InvokeRepeating("PerQuarterSecondOperations", .25f, .25f);  // 1/4s delay, repeat every 1/4s
                }
            }

            // update the HUB display subscriber objects
            ForceUpdateHUD();
        }  // add damage including elemental and update the HUD

        /// <summary>
        /// Abstract formula for damage mitigation.
        /// </summary>
        /// <param name="Type">Type of elemental damage.</param>
        /// <param name="Amount">Amount of damage.</param>
        /// <returns></returns>
        public abstract float MitigateDamge(BaseDamage Type, float Amount);  

        /// <summary>
        /// Drop all collectables on death.
        /// </summary>
        /// <param name="target">Failsafe ensuring actual death.</param>
        public virtual void DropAllCollectables(GameObject target = null)
        {
            if (target != null && target != gameObject) return;
            List<SpawnerOptionsDelayedSequence> PrefabsToSpawn = new List<SpawnerOptionsDelayedSequence>();
            for (int i = 0; i < Collectables.Count; i++)
            {
                if (Collectables[i].Value > 0)
                {
                    for (int p = GlobalFuncs.Collectables[i].Spawns.Count - 1; p >= 0; p--)
                    {
                        int TotalUsed = 0;
                        while (Collectables[i].Value - GlobalFuncs.Collectables[i].Spawns[p].Amount > 0)
                        {
                            TotalUsed += 1;
                            Collectables[i].Value -= GlobalFuncs.Collectables[i].Spawns[p].Amount;
                        }
                        if (TotalUsed > 0)
                        {
                            PrefabsToSpawn.Add(new SpawnerOptionsDelayedSequence()
                            {
                                RandomSphere = new RandomSphereOptions() { Radius = GlobalFuncs.OnDeathDropRadius, IncludeX = true, IncludeZ = true },
                                PhysicsForceOptions = new PhysicsOptions(),
                                RandomRotate = new RandomRotateOptions(),
                                NumberToSpawn = TotalUsed,
                                Prefab = GlobalFuncs.Collectables[i].Spawns[p].Prefab
                            });
                        }
                    }
                }
            }
            StartCoroutine(GlobalFuncs.SpawnAllDelayed(PrefabsToSpawn, 0f, false, transform, gameObject, -1, false, SpawnTarget.Any));
        }

        /// <summary>
        /// Repeating coroutine applying DoT's every 2 seconds.
        /// </summary>
        public virtual void PerDoubleSecondOperations()
        {
            if (!ApplyDoT(ref DoubleDoTs))
            {
                CancelInvoke("PerDoubleSecondOperations");
                bPerDblSecondRunning = false;
            }
            else
            {
                ForceUpdateHUD();
            }
        }

        /// <summary>
        /// Repeating coroutine applying DoT's and life/mana regen every second.
        /// </summary>
        public virtual void PerSecondOperations()
        {
            bool bChangesApplied = false;

            // apply life if needed
            if ((RegenLife + BonusRegenLife) > 0)
            {
                if (CurrentLife < (MAXLife + BonusMAXLife))
                {
                    CurrentLife += (RegenLife + BonusRegenLife);
                    if (CurrentLife > (MAXLife + BonusMAXLife))
                    {
                        CurrentLife = (MAXLife + BonusMAXLife);
                    }
                    bChangesApplied = true;
                }
            }

            // and mana
            if ((RegenMana + BonusRegenMana) > 0)
            {
                if (CurrentMana < (MAXMana + BonusMAXMana))
                {
                    CurrentMana += (RegenMana + BonusRegenMana);
                    if (CurrentMana > (MAXMana + BonusMAXMana))
                    {
                        CurrentMana = (MAXMana + BonusMAXMana);
                    }
                    bChangesApplied = true;
                }
            }

            // damage over time
            if (!ApplyDoT(ref DoTs) && !ApplyConditions() && (RegenMana + BonusRegenMana + RegenLife + BonusRegenLife) == 0)
            {
                CancelInvoke("PerSecondOperations");  // cancel if no DoTs or regen or active conditions
                bPerSecondRunning = false;
            }
            else
            {
                bChangesApplied = true;
            }
            
            // update the on screen display (if any)
            if (bChangesApplied)
            {
                ForceUpdateHUD();
            }
        }

        /// <summary>
        /// Repeating coroutine applying DoT's every half second.
        /// </summary>
        public virtual void PerHalfSecondOperations()
        {
            if (!ApplyDoT(ref HalfDoTs))
            {
                CancelInvoke("PerHalfSecondOperations");
                bPerHalfSecondRunning = false;
            }
            else
            {
                ForceUpdateHUD();
            }
        }

        /// <summary>
        /// Repeating coroutine applying DoT's every quarter second.
        /// </summary>
        public virtual void PerQuarterSecondOperations()
        {
            if (!ApplyDoT(ref QuarterDoTs))
            {
                CancelInvoke("PerQuarterSecondOperations");
                bPerQtrSecondRunning = false;
            }
            else
            {
                ForceUpdateHUD();
            }
        }

        /// <summary>
        /// Apply DoT's, called from per second operations repeating coroutines.
        /// </summary>
        /// <param name="whichDoT">Type of damage over time (and frequency) to apply via the mitigation function.</param>
        /// <returns></returns>
        public virtual bool ApplyDoT(ref List<MagicDamageOverTime> whichDoT)
        {
            if (whichDoT != null)
            {  // fail safe
                if (whichDoT.Count > 0)
                {  // DoTs still in stack
                    foreach (MagicDamageOverTime md in whichDoT)
                    {  // process DoT stack
                        CurrentLife -= MitigateDamge(md.Type, (md.DOTValue > 0 ? md.DOTValue : md.Value));  // apply magic damage

                        // reduce the remaining seconds for the DoT
                        switch (md.DOTFrequency)
                        {
                            case UpdateFrequency.TwoSeconds:
                                md.DOTLength -= 2f;
                                break;
                            case UpdateFrequency.HalfSecond:
                                md.DOTLength -= 0.5f;
                                break;
                            case UpdateFrequency.QuarterSecond:
                                md.DOTLength -= 0.25f;
                                break;
                            default:
                                md.DOTLength -= 1f;
                                break;
                        }
                    }
                    whichDoT.RemoveAll(md => md.DOTLength <= 0);  // clear all with zero secs remaining
                    ForceUpdateHUD();   // update HUD
                    return whichDoT.Count > 0;  // only keep running if more DoTs in the stack
                }
            }
            return false;  // return DoTs all run, cancel invoke
        }

        /// <summary>
        /// Enable condition particle effects (eg burning, bleeding) on the parent character.
        /// </summary>
        /// <returns>Whether any conditions are still active.</returns>
        public virtual bool ApplyConditions()
        {
            if (Conditions == null) return false;
            bool StillActive = false;
            foreach (BaseCondition bc in Conditions)
            {
                if (bc.Display)  // ensure condition exists
                {
                    if (bc.Length >= 1)  // found an active condition
                    {
                        StillActive = true;  // keep per second operations running
                        bc.Length -= 1;  // subtract a second for next run
                        bc.Display.SetActive(true);  // ensure effect is enabled
                    }
                    else if (bc.Length < 1)  // condition has expired
                    {
                        bc.Length = 0;  // reset length
                        bc.Display.SetActive(false);  // ensure effect is disabled
                    }
                }
            }
            return StillActive;  // all done
        }

        /// <summary>
        /// Rebuild the modifier list after changes
        /// </summary>
        public abstract void RebuildModifiers();

        /// <summary>
        /// Formula to determine next level up.
        /// </summary>
        /// <param name="CurrentLevel">Current character level.</param>
        /// <returns>XP required to level up.</returns>
        public abstract int CalcXPToNextLevel(int CurrentLevel);

        /// <summary>
        /// Abstract formula to determine max life from stats.
        /// </summary>
        /// <param name="UpdateToMAX">Option to MAX out the life attribute on upgrade.</param>
        public abstract void CalcMaxLife(bool UpdateToMAX);

        /// <summary>
        /// Abstract formula to determine bonus resistance per level.
        /// </summary>
        public abstract void CalcResistLevelBonus();

        /// <summary>
        /// Abstract formula to determine max stamina from stats.
        /// </summary>
        public abstract void CalcMaxStamina();

        /// <summary>
        /// Abstract formula to determine max mana from stats.
        /// </summary>
        /// <param name="UpdateToMAX">Option to MAX out the mana attribute on upgrade.</param>
        public abstract void CalcMaxMana(bool UpdateToMAX);

        /// <summary>
        /// Abstract formula to determine max equip weight from stats.
        /// </summary>
        public abstract void CalcMaxEquipLoad(); 

        /// <summary>
        /// Abstract formula to link to the spell size by level script.
        /// </summary>
        /// <returns>Spell scale modifier.</returns>
        public abstract float CalcSpellScale();

        /// <summary>
        /// Cache the skill point/resistance bonuses for faster calc.
        /// </summary>
        /// <param name="UpdateToMAX">Update core stats to the MAX after recalc.</param>
        protected virtual void rebuildModifierTotals(bool UpdateToMAX)
        {
            // zero the skill cache totals
            foreach (BaseSkillValue sv in SkillModTotals)
            {
                sv.Value = 0;
            }

            // recalc the skill totals
            foreach (BaseSkillValue Mod in Modifiers.Where(t => t is BaseSkillValue))
            {
                SkillModTotals[(int)Mod.Skill].Value += Mod.Value;
            }

            // recalc core stats
            reCalcCore(UpdateToMAX);
        }

        /// <summary>
        /// Recalc base resistance from items and character.
        /// </summary>
        /// <param name="LevelBonus">Bonus amount per level.</param>
        protected virtual void reCalcBaseResist(float LevelBonus)
        {
            // zero the resist cache totals
            foreach (BaseResistPercent rp in ResistModTotals)
            {
                rp.Value = 0;
            }

            // recalc the resist totals
            foreach (BaseResistPercent Mod in Modifiers.Where(t => t is BaseResistPercent))
            {
                ResistModTotals[(int)Mod.Resist].Value += Mod.Value + (LevelBonus * (CurrentLevel - 1));
            }
        }

        /// <summary>
        /// Link to item equipped/unequipped to rebuild the equipment modifiers.
        /// </summary>
        /// <param name="Source">Source item causing the stat change.</param>
        /// <param name="Equipped">Whether the source item is being equipped.</param>
        public virtual void reCalcEquipmentBonuses(CharacterEquipAttributes Source, bool Equipped)
        {
            // ensure per second operations are disabled
            if (bPerSecondRunning)
            {
                CancelInvoke("PerSecondOperations");
                bPerSecondRunning = false;
            }

            // remove all previous (non character) equipment bonuses
            Modifiers.RemoveAll(s => s.src != ModifierSource.Character);

            // zero previous core totals
            CurrentEquipLoad = 0f;
            BonusArmour = 0f;
            BonusMAXLife = 0;
            BonusRegenLife = 0f;
            BonusMAXMana = 0;
            BonusRegenMana = 0f;
            BonusMAXStamina = 0;

            // process children with equipment that have attributes
            CharacterEquipAttributes[] allEquiped = GetComponentsInChildren<CharacterEquipAttributes>();  // find all magic equipment
            if (!Equipped && allEquiped != null)
            {  // remove source item that has just been unEquiped?
                allEquiped = allEquiped.Where(s => s != Source).ToArray();  // remove the source item
            }
            if (allEquiped != null)
            {    // found some?
                foreach (CharacterEquipAttributes equip in allEquiped)
                {  // process all
                    // recalc core modifiers
                    foreach (CoreBonus cb in equip.Core)
                    {
                        switch (cb.Type)
                        {
                            case CharacterCore.Weight:
                                CurrentEquipLoad += cb.Value;
                                break;
                            case CharacterCore.Armour:
                                BonusArmour += cb.Value;
                                break;
                            case CharacterCore.Life:
                                BonusMAXLife += (int)cb.Value;
                                break;
                            case CharacterCore.LifeRegen:
                                BonusRegenLife += (int)cb.Value;
                                break;
                            case CharacterCore.Mana:
                                BonusMAXMana += (int)cb.Value;
                                break;
                            case CharacterCore.ManaRegen:
                                BonusRegenMana += cb.Value;
                                break;
                            case CharacterCore.Stamina:
                                BonusMAXStamina += (int)cb.Value;
                                break;
                        }
                    }

                    // recalc skill modifier stack
                    foreach (SkillBonus sb in equip.SkillPoints)
                    {
                        Modifiers.Add(new BaseSkillValue() { src = ModifierSource.Armour, Skill = sb.Type, Value = sb.Value });
                    }

                    // recalc resist modifier stack
                    foreach (MagicDamage md in equip.Resistance)
                    {
                        Modifiers.Add(new BaseResistPercent() { src = ModifierSource.Armour, Resist = md.Type, Value = md.Value });
                    }
                }
            }

            // check bounds
            if (CurrentLife > (MAXLife + BonusMAXLife)) CurrentLife = (MAXLife + BonusMAXLife);
            if (CurrentMana > (MAXMana + BonusMAXMana)) CurrentMana = (MAXMana + BonusMAXMana);

            // update animator
            StatsToAnimator(param_CoreLife);
            StatsToAnimator(param_CoreMana);
            StatsToAnimator(param_CoreStamina);
            StatsToAnimator(param_CoreEquipLoad);

            // re enable per second operations, if have DoTs or regen attributes
            if (Application.isPlaying && ((DoTs.Count + RegenLife + BonusRegenLife + RegenMana + BonusRegenMana) > 0))
            {
                bPerSecondRunning = true;
                InvokeRepeating("PerSecondOperations", 1f, 1f);  // 1s delay, repeat every 1s
            }

            // recalc the totals
            rebuildModifierTotals(false);
        }

        /// <summary>
        /// Recalc all core stats after state change.
        /// </summary>
        /// <param name="UpdateToMAX">Force mana/health stats to the max on recalc.</param>
        public virtual void reCalcCore(bool UpdateToMAX)
        {
            CalcMaxLife(UpdateToMAX);
            CalcResistLevelBonus();
            CalcMaxEquipLoad();
            CalcMaxStamina();// bUpdateToMAX);
            CalcMaxMana(UpdateToMAX);
        }

        /// <summary>
        /// Auto skill point distribution.
        /// </summary>
        /// <param name="Randomise">Random or sequential auto distribution.</param>
        public virtual void DistributePoints(bool Randomise)
        {
            if (Randomise)
            {
                for (int i = 0; i < UnspentSkillPoints; i++)
                {
                    Skills[UnityEngine.Random.Range(0, Skills.Count - 1)].Value += 1;
                }
            }
            else  // sequential
            {  
                int iNextSkill = 0;
                for (int i = 0; i < UnspentSkillPoints; i++)
                {
                    Skills[iNextSkill].Value += 1;
                    iNextSkill += 1;
                    if (iNextSkill == Skills.Count)
                    {
                        iNextSkill = 0;
                    }
                }
            }
            UnspentSkillPoints = 0;
        }  

        /// <summary>
        /// Add skill modifiers onto the stack.
        /// </summary>
        /// <param name="SkillName">Which skill.</param>
        /// <param name="ApplyValue">Value to apply.</param>
        public virtual void AddModifier(BaseSkill SkillName, int ApplyValue)
        {
            Modifiers.Add(new BaseSkillValue() { Skill = SkillName, Value = ApplyValue, src = ModifierSource.Character });
        }

        /// <summary>
        /// Add resistance modifiers onto the stack.
        /// </summary>
        /// <param name="SkillName">Which resistance type.</param>
        /// <param name="ApplyValue">Value to apply.</param>
        public virtual void AddModifier(BaseDamage ResistName, int ApplyValue)
        {
            Modifiers.Add(new BaseResistPercent() { Resist = ResistName, Value = ApplyValue, src = ModifierSource.Character });
        }

        /// <summary>
        /// Finds child transform by name even if inactive.
        /// </summary>
        /// <param name="Parent">Transform to search.</param>
        /// <param name="Name">Name of transform to find.</param>
        /// <returns>Game object searched for or null if not found.</returns>
        public virtual GameObject FindInActiveChild(Transform Parent, string Name)
        {
            if (Parent)  // failsafe
            {
                Transform[] trs = Parent.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in trs)
                {
                    if (t.name == Name)
                    {
                        return t.gameObject;
                    }
                }
            }
            return null;
        }
    }  

    #endregion

    #region "Modifier Stack & Other Lists"
    
    /// <summary>
    /// Modifier stack source for filtering.
    /// </summary>
    public enum ModifierSource
    {
        Character, Armour, Weapon
    }

    /// <summary>
    /// Base modifier stack.
    /// </summary>
    [Serializable]
    public class BaseValue
    {
        /// <summary>Source of the skill/resist modifier.</summary>
        public ModifierSource src;

        /// <summary>Value of the skill/resist in the stack.</summary>
        public float Value;
    }  

    /// <summary>
    /// Skill variant of the modifier stack.
    /// </summary>
    [Serializable]
    public class BaseSkillValue : BaseValue
    {
        /// <summary>Skill type.</summary>
        public BaseSkill Skill;
    }

    /// <summary>
    /// Resistance variant of the modifier stack.
    /// </summary>
    [Serializable]
    public class BaseResistPercent : BaseValue
    {
        /// <summary>Resistance type.</summary>
        public BaseDamage Resist;
    }

    /// <summary>
    /// Condition reaction to elemental damage.
    /// </summary>
    [Serializable]
    public class BaseCondition
    {
        /// <summary>Condition damage type.</summary>
        public BaseDamage Type;

        /// <summary>Child game object to enable when the condition is active.</summary>
        public GameObject Display;

        /// <summary>Remaining seconds to keep the condition active.</summary>
        public float Length;
    }

    /// <summary>
    /// Collectable non inventory items, eg gold
    /// </summary>
    [Serializable]
    public class BaseCollection
    {
        /// <summary>Type of collectable.</summary>
        public BaseCollectable Type;

        /// <summary>Amount held.</summary>
        public float Value;

        /// <summary>Prefab list for this collectable, only present on player.</summary>
        public List<CollectablePrefab> Spawns;
    }     

    /// <summary>
    /// Prefabs vs value the prefab represents
    /// </summary>
    [Serializable]
    public class CollectablePrefab
    {
        /// <summary>Game object to spawn when dropping this amount of the collectable.</summary>
        public GameObject Prefab;

        /// <summary>Amount of collectables this prefab represents.</summary>
        public float Amount;  
    }

    /// <summary>
    /// Editor only GUI modifier
    /// </summary>
#if UNITY_EDITOR
    public enum BaseIncrease
    {
        Fives = 5, Tens = 10, Twentyfives = 25, Fifties = 50, Hundreds = 100
    }  
#endif

    /// <summary>
    /// Core attribute list for GUI.
    /// </summary>
    public enum CharacterCore
    {
        Weight, Armour, Life, LifeRegen, Mana, ManaRegen, Stamina
    }

    /// <summary>
    /// Core bonus attributes for equipment to increase.
    /// </summary>
    [Serializable]
    public class CoreBonus
    {
        /// <summary>Type of core attribute to increase whilst wearing/holding this item.</summary>
        [Tooltip("Type of core attribute to increase whilst wearing/holding this item")]
        public CharacterCore Type;

        /// <summary>Amount of core attribute of the specified type to apply.</summary>
        [Tooltip("Amount of core attribute of the specified type to apply")]
        public float Value;
    }

    /// <summary>
    /// Magic damage to apply when a weapon/spell strikes.
    /// </summary>
    [Serializable]
    public class MagicDamage
    {
        /// <summary>Type of magic damage, note that the normal vObjectDamage is effectively applying physical and is required.</summary>
        [Tooltip("Type of magic damage, note that the normal vObjectDamage is effectively applying physical and is required")]
        public BaseDamage Type;

        /// <summary>Amount of magic damage/resist of the specified type to apply.</summary>
        [Tooltip("Amount of magic damage/resist of the specified type to apply")]
        public float Value;
    }

    /// <summary>
    /// Update frequency choice.
    /// </summary>
    public enum UpdateFrequency
    {
        QuarterSecond, HalfSecond, WholeSecond, TwoSeconds
    }

    /// <summary>
    /// Damage over time.
    /// </summary>
    [Serializable]
    public class MagicDamageOverTime : MagicDamage
    {
        /// <summary>Number of seconds to apply this damage, per second, enable by setting to greater than zero.</summary>
        [Tooltip("Number of seconds to apply this damage, per second, enable by setting to greater than zero")]
        public float DOTLength = 0;

        /// <summary>Amount of magic damage of the specified type to apply per tick of the frequency, if zero then the main damage value will be applied instead.</summary>
        [Tooltip("Amount of magic damage of the specified type to apply per tick of the frequency, if zero then the main damage value will be applied instead")]
        public float DOTValue = 0;

        /// <summary>Update frequency of the magic damage over time, eg if length = 2 secs, damage value = 2 and frequency = Quarter Second then 16 total damage will be applied.</summary>
        [Tooltip("Update frequency of the magic damage over time, eg if length = 2 secs, damage value = 2 and frequency = Quarter Second then 16 total damage will be applied")]
        public UpdateFrequency DOTFrequency;
    }


    /// <summary>
    /// Skill bonus for equipment.
    /// </summary>
    [Serializable]
    public class SkillBonus
    {
        /// <summary>Type of skill to increase whilst wearing/holding this item.</summary>
        [Tooltip("Type of skill to increase whilst wearing/holding this item")]
        public BaseSkill Type;

        /// <summary>Amount of skill attribute of the specified type to apply.</summary>
        [Tooltip("Amount of skill attribute of the specified type to apply")]
        public float Value;
    }

    #endregion
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
