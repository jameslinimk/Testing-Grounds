# Testing Grounds

[Gameplay footage](https://drive.google.com/file/d/1hKGrh_CQG6XxHdIXn6ju7mAZ7FJaHhGo/view?usp=sharing)

A game made for CS Seminar class. You play as a third-person wizard stuck in a zombie arena. You have 3 spells, a snipe-like spell, a rapid-fire spell and a shotgun-like spell. Increasingly faster and stronger zombies will spawn in waves and levels forever until you die. The goal is to survive as long as possible.

I first started with a simple prototype with a moveable cylinder and movement system. I manually implemented the full camera, movement, enemy, and spell system. Perfecting the movement took a long time, and I used vector-projection concepts from my math class to implement the movement system. I then added an enemy, using a simple NavMesh system to make it follow the player which would dynamically be replaced with a rigidbody for enemy knockback. At first, I had guns that would shoot, but I changed it to a spell system. When shooting, a ray is cast from the camera to the crosshair position to get the aim target, then a separate ray is cast from the player's right hand to the aim target, and the projectile is shot along that ray.

I then browsed for assets and animations, and used animation state-machines to implement animations. In addition, I learned how fbx animations work and how to manually keyframe tweak movements in blender. I also learned how the particle system worked for the spell projectiles. I then implemented the level and spawn system, end screen, and a simple UI. I tried to implement everything without the use of tutorials or packages, to fully learn the Unity engine, and I will continue to work on the project over the summer.

I made sure to make every system modular, so that I can easily add new spells, enemies, and levels. The code is structured in a way that allows for easy expansion, and some items on the roadmap are already implemented but not yet activated in the game.

## Controls

- **WASD** - Move
- **Mouse** - Look
- **Left Click** - Cast spell
- **1,2,3** - Switch spells
- **Space** - Roll
- **Alt** - Freelook

## Assets used

- [Toon Projectiles 2](https://assetstore.unity.com/packages/vfx/particles/spells/toon-projectiles-2-184946)
- [Polygon City Zombies](https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/polygon-city-zombies-low-poly-3d-art-by-synty-131930)
- [Low Poly Human](https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/free-low-poly-human-rpg-character-219979)
- [Gridbox Prototype Materials](https://assetstore.unity.com/packages/2d/textures-materials/gridbox-prototype-materials-129127)
- [Mixamo Animations](https://www.mixamo.com/)

## Future roadmap

- [ ] Pick up different spells (aura and area) and armors
  - Most of everything is half-implemented/in-code
- [ ] Different zombies
  - Framework to add more zombies is implemented
  - [ ] Ranged zombies
  - [ ] Brute zombies
  - [ ] Ninja zombies
- [ ] Bosses
- [ ] Multiple level areas
- [ ] Ending
- [ ] Sound
- [ ] Play screen
- [ ] Settings screen
