# Brick Smasher

A Unity 2D game project - a Breakout-style game with Super Smash Bros.-inspired knockback physics.

## Game Concept

Instead of destroying bricks on contact, the ball applies knockback to bricks based on impact velocity and angle. Bricks accumulate "damage" that increases their knockback multiplier. Players must launch bricks off the screen/stage boundaries to eliminate them.

## Core Mechanics

- **Knockback Physics**: Bricks react to ball impacts with physics-based knockback
- **Damage Accumulation**: Higher damage = greater knockback received (like Smash Bros. percentage)
- **Stage Boundaries**: Bricks must be knocked past screen edges to be eliminated
- **Skill-Based Gameplay**: Emphasis on precise paddle control and reaction timing

## Target Platforms

- **Primary**: Mobile (iOS/Android)
- **Secondary**: PC/Console (future consideration)

## Project Structure

```
Assets/
├── Scenes/          # Game scenes
├── Scripts/         # C# game logic
├── Prefabs/         # Reusable game objects
├── Sprites/         # 2D artwork
├── Audio/           # Sound effects and music
└── Settings/        # URP and render settings
```

## Technical Notes

- Unity 2D with Universal Render Pipeline (URP)
- Input System package for cross-platform input handling
- Physics2D for ball and brick interactions

## Planned Features

- Multiple ball types with different knockback properties
- Various paddle styles affecting ball trajectory and power
- Level progression with different brick layouts and stage hazards
