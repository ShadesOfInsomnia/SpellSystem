A FREE [Invector](https://assetstore.unity.com/publishers/13943) addon brought to you by 
# Shades of Insomnia
the undisputed kings of feature creep…

***
* Copyright        : 2017 Shades of Insomnia
* Founding Members : Charles Page (Shade) & Rob Alexander (Insomnia)
* License          : Attribution-ShareAlike 4.0 International (CC BY-SA 4.0) https://creativecommons.org/licenses/by-sa/4.0/

### Notice
The spell system requires the [Invector](https://assetstore.unity.com/publishers/13943) 3rd person [Melee](https://assetstore.unity.com/packages/templates/systems/third-person-controller-melee-combat-template-44227) or [Shooter](https://assetstore.unity.com/packages/templates/systems/third-person-controller-shooter-template-84583) controller and Unity3D to be used.  This package is not endorsed or made by [Invector](https://assetstore.unity.com/publishers/13943) and can be found on the forum [thread](http://invector.proboards.com/thread/480/shades-spell-system).

### Usage Guidelines
You are free to:
* Share - copy and redistribute the material in any medium or format.
* Adapt - remix, transform, and build upon the material for any purpose, even commercially. 

The licensor cannot revoke these freedoms as long as you follow the license terms.
Under the following terms:
* Attribution - You must give appropriate credit, provide a link to the license, and indicate if changes were made. You may do so in any reasonable manner, but not in any way that suggests the licensor endorses you or your use.
* ShareAlike - If you remix, transform, or build upon the material, you must distribute your contributions under the same license as the original.  

You may not apply legal terms or technological measures that legally restrict others from doing anything the license permits.

### About the Spell System
In the new system you will find a massive amount of new tools, systems and options to not only bring to life a functional Spell System for your game, but one that has a fast setup, workflow and can easily be extended to do just about anything. In addition to just the spells, we are including Ranged AI capable of casting a number of spells, A Main Menu / Load Level System based off of SQL Lite or EasySave2.

We included a resist system that mitigates damage based on damage types. We included Magic Weapons, We included Teleports, and Creature Summons, Scripts to increase spell size by level. We even created a leveling system with point allocation upon level up. Oh and an entire attribute system that can hook into other existing systems. Oh yea, and fire breathing Iguana's! Like i said, Kings of Feature Creep.

Please have a read under the documentation for instructions. Even at the time of writing this, i'm sure there are cool features we forgot to document. From the Shades of Insomnia Team, Enjoy. Below is the link to the unity package containing a completely setup Project (Requires Invector Melee)

### Download Links
[Shades Spell System v2.0a Melee Package](https://www.dropbox.com/s/u9v2dk2cm932qn7/ShadesSpellSystem2.0aMelee.unitypackage)

[Shades Spell System v2.0a Shooter Package](https://www.dropbox.com/s/m5jcxen9byjblzm/ShadesSpellSystem2.0aShooter.unitypackage)

### Installation Requirements / Order
* Ensure using Unity 5.6+ (ideally 2017.1+)
* Install Invector Melee (or Shooter) Controller 2.x
* Check Edit->Project Settings->Player->Other Settings.....to view the demos as intended set colour space to linear
* Install easysave2 if owned (if not don’t import CharacterDataEasySave2.cs), SQLLite is included in the package as a free alternative
* Install the ShadesSpellSystem2.0aMelee.unitypackage or ShadesSpellSystem2.0aShooter.unitypackage as appropriate
* To run the mainmenu_lobby scene, add the level1 and level2 scenes to the build settings.
* If you’re running a non-windows OS then you will need to download the appropriate SQLLite dll or not import it and use easy save2, see www.sqlite.org/download.html


### Feature List
* Centralized spawn class linked to the pooler, including spawns on birth, death, body removal, spell hits & explosions, trapped objects, proximity triggers, and more
* Player and AI magic enabled character creation wizards for easy setup
* vInventory integration for spells, including custom AI item manager
* Leveling system with data abstraction layer, modifiable attributes, resistances & formula’s
* Main menu system linked to the data abstraction layer
* SQLLite/EasySave2 abstract data layer implementation (easy to clone for other data formats)
* Character information screen
* Attribute System with Point Spending
* Magic weapons and armour with damage and resist types linked to the levelling system
* New Damage System with Damage Over Time, and various Damage Types
* Damage Mitigation System including resist types
* Weapon trails effects
* Custom animation on AI instantiation
* Status effects aka burning, poisoned, frozen etc
* Generic AI without modifying the Invector core (Experimental)
* Ranged AI for Invector Core AI capable of spell casting or arrow shooting.
* Feed vWaypoints into the animator and trigger actions
* 600 free gems unique to this asset (uses the Unity Gem Shader)
* Merge LOD levels from simplygon output onto singular bone structure
* Character equipment material changer (across multiple LOD levels
* Generic ragdoll builder which works from bone chains
* Archery and Thrown weapon System
* Complete custom-made spells and Skills Examples, free for commercial use
* Custom Emissions Shader
* Shield Bash System
* Custom inputs for spells
* Magic Projectile Script with homing and advanced targeting systems
* Custom hand effect’s
* Animator based multi-layered spell casting system
* Custom Spell Sounds
* Physics Based Spells
* Gold and Object Collection System that shows in the Character Screen
* Custom Scripts to do everything from Raise the Dead to Heal.
* Basic Character Creation System (Name, Axis, Class, Race and Alignment)
* Full pooling system, automatic or pre-warmed



### About the Invector 3rd Person Controller
Developing a 3rd Person Controller is really hard and takes too much time, so invector developed this awesome template so you can set up a character controller or AI in less then 10 seconds and melee combat within minutes, leaving space for you to focus on making your game!

With the [Melee Combat Template](https://assetstore.unity.com/packages/templates/systems/third-person-controller-melee-combat-template-44227), you have a starting point to make any type of 3rd Person Game, RPG, Action-Adventure, 2.5D Platform, Isometric, Topdown, etc. Your call, your game. 
* Set up any model in less then 10s
* Rigidbody, root motion and non-root motion controller
* Advanced Enemy AI
* Enemy VS Enemy & Companion AI
* Waypoint System
* Lock-on Target
* Mecanim & Humanoid
* 3rd Person Camera with CameraStates
* Works great on mobile devices
* 360 controller compatible with vibration
* Auto detected input type real time
* FootStep audio system on mesh or terrain
* Fully commented C# code
* Documentation and video tutorials
* Play with ragdoll physics

Invector's [Shooter Template](https://assetstore.unity.com/packages/templates/systems/third-person-controller-shooter-template-84583)  is inspired by AAA shooters and include all the features of the Basic & Melee template, plus: 
* ThirdPerson, TopDown or 2.5D Shooter
* Fire Weapons
* Projectile bullets with trail renderer
* Throwing objects with Trajectory system (granade, bottles, etc..)
* Melee attacks for fire weapons
* Advanced damage based on distance & velocity
* Decal for projectiles based on tags (different materials)
* Advanced Scope View
* Aiming System with dispersion, range, shot frequency, recoil, etc...
* Particles to emitt on attack
