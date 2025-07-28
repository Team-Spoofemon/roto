# Unity Resource Guide v2.0

## Design

[Transforming](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Transform.html)
- Move, action, scale

[Rigidbodies](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Rigidbody.html)
- Motion for 3D characters

[Collider](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Collider.html)
- Base ckass for all colliders
- To be used for interactions and terrain

[Physics](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Physics.html)
- Raycasts, overlap checks, general physics logic

Using Unity's Input System 1.14.1

## Input
[Installation](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Installation.html)

[Quick Start](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/QuickStartGuide.html)

[Overview](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Concepts.html)
- Basic intro to Unity's input system

[Actions](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Actions.html)
- Information on InputSystem.actions, InputActionMap, InputAction, and InputBinding

[PlayerInputComponent](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/PlayerInput.html)
- Ways to use player input components such as actions, control schemes, camera, or behavior.
- What will be used for Zeus prefab

[Interactions](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Interactions.html)
- Different button interactions, like waiting, performed, canceled, etc.
- Can be useful for things like different attackes, ability charging, etc. Can implement light vs heavy attacks, sprint vs walking, etc.

[Processors](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/UsingProcessors.html)
- Modify input values
- Can be used for adjusting sensitivities for customization

[Supported Input Devices](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/SupportedDevices.html)
- Covers keyboard, gamepad, touch, etc. Just gonna be good to think about
- Can be used to plan for controller support

## Scene Management & Flow

[SceneManager](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SceneManagement.EditorSceneManager.html)
- Load and unload scenes

[AsyncOperation](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AsyncOperation.html)
- Used with LoadSceneAsync for smooth transitions

[DontDestroyOnLoad]
- Keep important objects across scenes

## Game State & Saving

[PlayerPrefs](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PlayerPrefs.html)
- Save system for things like settings, quest flags, etc.

[ScriptableObject](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/ScriptableObject.html)
- Reusable data containers

[Time](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Time.html)
- Useful for timers, cooldowns, deltaTime calculations

## UI & Input Helpers

[Canvas](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Canvas.html)
- UI systems

## VFX & Animation

[ParticleSystem](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/ParticleSystem.html)
- Use for things like fire bursts, effects, etc.

[Animator](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Animator.html)
- Used for animations, including transitions and triggers

## Audio

[AudioSource](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioSource.html)
- Play sounds on events

[AudioClip](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioClip.html)
- Reference to actual sound file

## Utility & Scripting Essentials

[Monobehaviour](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.html)
- Scrips inherit from this base class

[GameObject](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html)
- Used to spawn, find and manipulate objects

[Instantiate](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Object.Instantiate.html)
- Spawn enemies, effects, pickups

[Destroy](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Object.Destroy.html)
- Remove objects

[Tag](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject-tag.html)
- Allows us to compare objects in logic

