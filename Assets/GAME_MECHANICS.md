# Game Mechanics

Always wrap using namespace ROTO { }

All classes should be public unless a private method is nested inside a public class and only needs to be accessed by that class


## Core Gameplay

**Player**
- PlayerController.cs
- PlayerCombat.cs
- PlayerHealth.cs

**EnemyAI**
- EnemyAI.cs
- EnemyHealth.cs
- EnemyAttack.cs

**System Management & UI**
- GameManager.cs
- LevelManager.cs
- UIManager.cs

**UX Scripts**
- CutsceneManager.cs
- DamageHandler.cs
- Hitbox.cs
- AbilitySystems.cs
- KnockbackHandler.cs
- DashMechanics.cs
- AnimationController.cs
- AudioManager.cs
- CameraFollow.cs

## Script Leads

**PlayerController.cs**
- Defines character movement. Forward, left, right, back, jump, crouch (?)
- Avoid diagonal speed boost

**PlayerCombat.cs**
- Defines player melee
- Damage count
- Special abilities

**PlayerHealth.cs**
- Defines player starting health
- If hit then how much does health go down
- Health regeneration?
- Death behavior
- Does player have attack cooldown? Maybe for special abilities but melee can spam
- If player hits a heart on the battlefield, then health regenerates by 25%

**EnemyAI.cs**
- Enemy behavior such as when to attack, movement

**EnemyHealth.cs**
- Defines enemy starting health
- Same as player health

**EnemyAttack.cs**
- Attack patterns 
- Damage dealing
- Attack cooldown (don’t spam player)
- Cases for difficulties (easy, moderate, hard) - will be integrated within GameManager.cs

**GameManager.cs**
- Difficulty options (easy, moderate, hard, literally impossible [would be funny])
- Controls game states, such as pause, game over, level progression

**LevelManager.cs**
- Manages checkpoints (if any), respawns
- Calls CustceneManager for scene loading

**CutsceneManager.cs**
- Manages cutscene behavior for each cutscene

**UIManager.cs**
- Health bars
- Menus
- In-game UI (map, etc)
- Loading screens


For Better Gameplay, if we want it

**DamageHandler.cs**
- Special class to handle damage and knockback

**Hitbox.cs**
- Detects hits and applies damage based on collision

**AbilitySystems.cs**
- Manages special abilities of each character

**KnockbackHandler.cs**
- Handles knockback effects on both players and enemies

**DashMechanics.cs**
- Manages speed boosts if dashing is available

**NonplayableCharacters.cs**
- NPCs can be placed in unity in desired locations
- Class manages NPC behavior

**Interactables.cs**
- Manages objects
- Interactable NPC interactions? Or should it be a separate class?

**Collectibles.cs**
- Power ups?
- Collectible items?

**AnimationController.cs**
- Controls player animation during gameplay
- How do sprites move? Reaction when hit? Attack animations

**AudioManager.cs**
- Manages music, gameplay audio, sfx, etc.

**CameraFollow.cs**
- Manages for camera directions
- Might be complex
- Need to account for 2D sprite in 3D environment (if they’re facing left-back, camera shift to the right (view-left) would force character to look back-right (mirrors over axis))