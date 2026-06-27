You are a senior Unity game developer.

Create a COMPLETE clone of Flappy Bird in Unity (latest LTS version).

IMPORTANT:
- Do NOT use copyrighted assets.
- Use simple placeholder graphics only.
- Every sprite should be generated with Unity primitives or simple colored rectangles/circles.
- Every sound effect can be empty or generated later.
- The entire game must be playable without importing external assets.

=====================
PROJECT REQUIREMENTS
=====================

Create a clean project structure.

Assets/
    Scripts/
    Prefabs/
    Scenes/
    Materials/
    Sprites/
    Audio/
    UI/

=====================
GAMEPLAY
=====================

Implement the original Flappy Bird gameplay.

Bird:
- Gravity based movement
- Tap/click/Space to flap
- Smooth physics
- Rotation follows vertical velocity
- Death when touching pipe or ground

Pipes:
- Spawn endlessly
- Random gap height
- Constant movement speed
- Destroy when leaving screen

Camera:
- Fixed camera

Ground:
- Infinite scrolling

Background:
- Infinite scrolling

=====================
GAME STATES
=====================

Implement:

Main Menu

Playing

Paused

Game Over

Restart

=====================
UI
=====================

Main menu:
- Play button

HUD:
- Current score

Game Over:
- Final score
- Best score
- Restart button

=====================
SCORING
=====================

Increase score after passing each pipe.

Store Best Score using PlayerPrefs.

=====================
PLACEHOLDER GRAPHICS
=====================

Bird:
Yellow circle

Pipe:
Green rectangles

Ground:
Brown rectangle

Background:
Blue rectangle

Clouds:
Simple white circles

Everything should be replaceable later by sprites.

=====================
ANIMATIONS
=====================

Bird:
- Slight idle animation
- Rotation while flying

Ground:
- Continuous scrolling

Background:
- Slow parallax scrolling

=====================
EFFECTS
=====================

Simple particle effect on death using Unity Particle System.

=====================
AUDIO
=====================

Create AudioManager.

Prepare references:

Flap

Point

Hit

Die

No actual audio files required.

=====================
CODE QUALITY
=====================

Use C#.

Split responsibilities:

GameManager

BirdController

PipeSpawner

Pipe

ScrollingObject

ScoreManager

AudioManager

UIManager

ObjectPool

Use object pooling instead of Instantiate/Destroy for pipes.

Avoid Update() when unnecessary.

Comment important code.

=====================
EDITOR SETUP
=====================

Automatically create:

Main Camera

Canvas

EventSystem

Prefabs

Physics Layers

Tags

Bird prefab

Pipe prefab

Ground prefab

Background prefab

=====================
GAME FEEL
=====================

Bird flap should feel responsive.

Pipe spacing similar to original Flappy Bird.

Gap randomization should remain fair.

Difficulty remains constant.

=====================
FINAL RESULT
=====================

The finished project should be playable immediately after opening Unity.

No missing references.

No compile errors.

No external assets.

No Asset Store dependencies.

Everything uses placeholder graphics.

The project should be easy to replace with real sprites later.
=====================
SCREEN CONFIGURATION
=====================

Target platform:
- Android
- WebGL

Screen orientation:
- Portrait only
- Fixed orientation
- Disable Landscape Left
- Disable Landscape Right
- Disable Auto Rotation

Reference Resolution:
1080 x 1920

Canvas:
- Canvas Scaler
- Scale With Screen Size
- Reference Resolution: 1080 x 1920
- Match: 0.5

Camera:
- Orthographic
- Everything visible inside a 1080x1920 portrait frame.

The game MUST look identical on:

1080x1920
720x1280
1440x2560
1080x2400
1080x2340

No horizontal scrolling.
No vertical scrolling.
No page overflow on WebGL.

The entire game must always fit inside the viewport.

=====================
SAFE AREA
=====================

Support mobile safe areas.

UI must never overlap:
- notch
- dynamic island
- navigation bar
- home indicator

=====================
WEBGL
=====================

The generated WebGL build must work without modifying index.html.

The Unity canvas should automatically fit the browser viewport.

No browser page scrolling.

The game should always remain centered.

Resize correctly when the browser window changes size.

=====================
CAMERA FRAMING
=====================

The gameplay area is designed around a fixed portrait frame.

Never extend gameplay outside the visible screen.

Ground, pipes and UI must always remain visible.

=====================
RESPONSIVE RULES
=====================

Do not stretch sprites.

Keep aspect ratio.

Expand only empty background if necessary.

Gameplay dimensions remain constant across devices.

The gameplay should replicate the original Flappy Bird as closely as possible.

Bird jump force, gravity, pipe spacing, pipe speed, gap size and collision feel should closely match the original game.

Do not invent new mechanics or extra features unless explicitly requested.

=====================
SCREEN CONFIGURATION
=====================

Target platform:
- Android
- WebGL

Screen orientation:
- Portrait only
- Fixed orientation
- Disable Landscape Left
- Disable Landscape Right
- Disable Auto Rotation

Reference Resolution:
1080 x 1920

Canvas:
- Canvas Scaler
- Scale With Screen Size
- Reference Resolution: 1080 x 1920
- Match: 0.5

Camera:
- Orthographic
- Everything visible inside a 1080x1920 portrait frame.

The game MUST look identical on:

1080x1920
720x1280
1440x2560
1080x2400
1080x2340

No horizontal scrolling.
No vertical scrolling.
No page overflow on WebGL.

The entire game must always fit inside the viewport.

=====================
SAFE AREA
=====================

Support mobile safe areas.

UI must never overlap:
- notch
- dynamic island
- navigation bar
- home indicator

=====================
WEBGL
=====================

The generated WebGL build must work without modifying index.html.

The Unity canvas should automatically fit the browser viewport.

No browser page scrolling.

The game should always remain centered.

Resize correctly when the browser window changes size.

=====================
CAMERA FRAMING
=====================

The gameplay area is designed around a fixed portrait frame.

Never extend gameplay outside the visible screen.

Ground, pipes and UI must always remain visible.

=====================
RESPONSIVE RULES
=====================

Do not stretch sprites.

Keep aspect ratio.

Expand only empty background if necessary.

Gameplay dimensions remain constant across devices.


Work incrementally.

Step 1:
Create the Unity project structure.

Step 2:
Create all scripts.

Step 3:
Generate placeholder prefabs.

Step 4:
Configure physics.

Step 5:
Implement gameplay.

Step 6:
Implement UI.

Step 7:
Implement object pooling.

Step 8:
Polish gameplay feel.

After each step, verify there are no compiler errors before continuing.

Do not leave TODOs.

If something is missing, implement it completely.