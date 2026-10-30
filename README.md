# Ashvale

A 3D stealth-action game built in Unity 6 with hand-coded guard AI.
Sneak through a walled village, find the hidden key, and escape the guards.

![Gameplay](Screenshots/gameplay.png)

## Play it

**[Download for Windows]( https://github.com/krishnaborseGamedev/Ashvale/releases/download/V1.0/Ashvale_Buildzip.zip )**
Unzip the file and run `Ashvale.exe`.
Windows may show "Windows protected your PC". Click "More info", then "Run anyway".

## The game

Patrolling guards watch a walled village. Find the key hidden under the blue flowers to win.
If a guard hits you until your health reaches 0, you lose.
You can fight back with a sword (combat mode), or stay out of sight.

| Win | Lose |
|---|---|
| Pick up the key | Health reaches 0 |

## Controls

| Key | Action |
|---|---|
| W A S D | Move |
| Shift | Run |
| Space | Jump |
| C | Crouch (move slowly) |
| K | Combat mode on/off |
| YOUR ATTACK KEY | Sword attack (only in combat mode) |
| Mouse | Look around |

## Screenshots

![Village](Screenshots/village.png)
![Guard chase](Screenshots/chase.png)
![Win screen](Screenshots/win.png)

## What I built

**Guard AI (EnemyController.cs)**
- A finite state machine with 6 states: Patrol, Alert, Chase, Search, Return, Attack.
- Vision uses three checks: a distance check, a view-angle check, and a line-of-sight raycast aimed at chest height, so walls and houses block sight.
- Guards use Unity NavMesh, walk waypoint routes, wait at each point, search the last place they saw you, and return to patrol.

**Player**
- Third-person movement built on Unity Starter Assets (left untouched; my scripts talk to it from outside).
- Crouch (PlayerCrouch.cs) changes the character's height and speed.
- Combat mode (PlayerCombat.cs): sword attack with a root-motion lunge, hit detection with a sphere check at the blade, and a set that stops one swing from hitting the same guard twice.
- Health (PlayerHealth.cs) with an on-screen health bar (HealthBarUI.cs).

**Game flow and polish**
- Key pickup (KeyPickup.cs) triggers the Win screen. Player death triggers the Lose screen. Both have Restart and Quit (GameEndUI.cs).
- On-screen guide text for objective and controls.
- Sound (SfxManager.cs): background ambience, sword swing, and key pickup, with small random pitch changes so sounds don't feel repeated.
- Performance work: GPU instancing on foliage materials, LOD groups, and fog.

## How the code is organized

| Script | One job |
|---|---|
| EnemyController | Decides the guard's state and handles vision |
| EnemyAnimator | Turns the guard's state into animation |
| EnemyHealth | Guard health, hit effect, death |
| PlayerCombat | Combat mode, attack, hit detection |
| PlayerCrouch | Crouch |
| PlayerHealth | Player health and death |
| KeyPickup | Key trigger and win |
| GameEndUI | Win/Lose screens, restart, quit |
| HealthBarUI | Health bar |
| SfxManager | Plays sound effects |

Each script does one job. Scripts talk to each other through simple public flags (for example `IsCrouching`, `IsInCombat`) instead of reaching into each other's code.

## Problems I solved

1. **Root motion froze the player.** Once a script defines `OnAnimatorMove()`, Unity calls it every frame, even when Apply Root Motion is off. That made `CharacterController.Move()` run twice per frame and froze movement. Fix: turn `applyRootMotion` on only during the attack, and add a guard line at the top of `OnAnimatorMove()` so it returns when root motion is off.
2. **Guard death animation looped and the dead guard attacked again.** Any State transitions are checked every frame. Fix: make Die a bool that stays true, add `Die == false` to every other Any State transition, and turn off "Can Transition To Self" on the death transition.
3. **Guards fell through the ruins.** The NavMeshSurface was on the guard prefab. Fix: use one NavMeshSurface on a shared environment object.
4. **Gaps in walkways and guards walking through houses.** Seams between separate mesh pieces and props that weren't cut out of the NavMesh. Fix: adjust the bake and add NavMeshModifier (Not Walkable) to houses, gates, and trees.
5. **Guards stopped just outside attack range.** The chase stopping distance was set equal to the attack range. Fix: stop at `attackRange - 0.5`.
6. **Guards failed to see the player.** The vision raycast aimed at the feet. Fix: aim at chest height.
7. **Swapping the character model.** Changing the Animator Avatar alone does not change the body. Fix: move the new bone hierarchy and mesh into the player object and delete the old one.

## Known limitations

- Guards sometimes stand still while chasing (they turn to face you but don't walk), mostly right after losing and finding you again. I investigated with path debugging and the NavMesh route is valid, so the cause is still open.
- Crouching slows you down but does not yet reduce how far guards can see.
- The hit spark appears at the sword tip instead of on the guard's body.
- The sword-hit and guard-alert sounds are not working yet.
- Third-party art and character files are not in this repository (see below).

## Not included (cut to finish the project)

Boss fight, lock-on targeting, extra combos, and a second level.

## About this repository

This repo contains my scripts, scenes, and project settings. Purchased and free third-party packs, Mixamo characters and animations, and weapon/audio files are not included because I can't re-share them. The playable game is in the Release above.

## Built with

Unity 6 (6000.3), C#, URP, NavMesh, Cinemachine, Input System.

## Credits

- Environment: Polytope Studio (Lowpoly Environments), Fantasy Environments (Town pack), LUMINATE
- VFX: Vefects (Flipbook VFX Bundle Lite)
- Characters and animations: Mixamo (Kachujin G Rosales, Paladin J Nordstrom and animation clips)
- Movement base: Unity Starter Assets
- Sound effects: SOUND SITE NAME HERE (author and license)

## Author

Krishna Borse, game developer.
Portfolio: https://krishnaborse.framer.website
GitHub: https://github.com/krishnaborseGamedev
LinkedIn: ADD YOUR LINKEDIN LINK HERE

## License

My code is under the MIT License. Third-party assets keep their own licenses.
