# Island Survival

A single-player 3D island survival game built with **Unity 2022.3 LTS** and the **Universal Render Pipeline (URP)**.

Explore a procedurally generated tropical island, gather resources, craft tools, build a shelter, and manage hunger, thirst, and energy through a full day/night and weather cycle.

![Genre](https://img.shields.io/badge/genre-Survival-green)
![Engine](https://img.shields.io/badge/engine-Unity%202022.3%20LTS-black)
![Platform](https://img.shields.io/badge/platform-Windows%20PC-blue)
![License](https://img.shields.io/badge/code-MIT-yellow)

---

## Features

- **Procedural tropical island** — beach, forest, rocky coast, hills, pond, cave, clearing
- **First-person controller** — walk, sprint, jump, crouch, mouse look
- **Interaction system** — unified raycast interact prompt (`E`)
- **Resource gathering** — wood, stone, leaves, fiber, fruit, freshwater
- **Inventory + hotbar** — stacking, use consumables, capacity limits
- **Crafting** — tools, campfire, shelter parts, storage (data-driven recipes)
- **Building** — foundation, wall, floor, roof, doorway, storage, campfire with preview
- **Survival stats** — hunger, thirst, energy with warnings and recovery
- **Day / night cycle** — dynamic sun, ambient light, fog
- **Weather** — clear, cloudy, light rain, heavy rain
- **Save / load** — position, inventory, stats, time, weather, buildings
- **Main menu + in-game HUD** — polished runtime UI (no missing references)

---

## Requirements

| Item | Value |
|------|--------|
| Unity Editor | **2022.3.50f1** (2022.3 LTS recommended) |
| Render Pipeline | URP 14.x (via Package Manager) |
| OS | Windows 10/11 (primary target) |
| RAM | 8 GB minimum, 16 GB recommended |
| GPU | GTX 1050 / RTX 4050 class or better |

---

## Opening the Project

1. Install **Unity Hub** and **Unity 2022.3 LTS**.
2. Clone this repository:
   ```bash
   git clone https://github.com/mtigang/Island-Survival.git
   ```
3. In Unity Hub → **Open** → select the `Island-Survival` folder.
4. Wait for package resolve (URP, UGUI, TextMeshPro).
5. Open scene `Assets/Scenes/MainMenu.unity` or press Play on `GameWorld.unity`.

> The gameplay scene uses a **runtime bootstrap**. Press Play and the island, player, systems, and UI are created automatically. You do not need to wire scene references by hand.

If URP materials appear magenta on first open:

1. Window → Rendering → Render Pipeline Converter (or assign a URP Asset under Project Settings → Graphics).
2. Or simply press Play — the bootstrap creates URP/Lit materials at runtime when available, with Standard shader fallback.

---

## Controls

| Input | Action |
|-------|--------|
| WASD | Move |
| Mouse | Look |
| Left Shift | Sprint |
| Left Ctrl | Crouch |
| Space | Jump |
| E | Interact / Gather |
| Tab | Inventory |
| C | Crafting menu |
| B | Building menu |
| 1–9 | Hotbar / craft / build selection |
| R | Rotate building piece |
| LMB | Confirm build |
| Q | Cancel build |
| RMB | Use selected consumable |
| Esc | Pause |
| F5 | Quick save |
| M | Main menu (while paused) |

---

## Gameplay Loop

1. Spawn on the beach.
2. Gather wood, stone, fiber, fruit.
3. Find the freshwater pond inland.
4. Open **C** and craft a wooden tool / campfire / shelter parts.
5. Open **B** and place a foundation and walls before nightfall.
6. Eat fruit, drink water, manage energy while exploring.

---

## Project Structure

```
Assets/
  Scenes/           MainMenu, GameWorld
  Scripts/
    Core/           GameManager, items, events
    Player/         First-person controller
    Interaction/    Raycast interaction framework
    Inventory/      Grid inventory + hotbar
    Resources/      Gatherable world objects
    Crafting/       Recipe system
    Building/       Placement + persistence
    Survival/       Hunger / thirst / energy
    Environment/    Island generator, day/night
    Weather/        Weather states + rain
    Audio/          Ambient + SFX helpers
    UI/             HUD + main menu
    SaveSystem/     JSON save/load
    Bootstrap/      Runtime scene setup
  Materials/
  Prefabs/
  ScriptableObjects/
Packages/           manifest (URP, Input System, UGUI)
ProjectSettings/
```

---

## Architecture Notes

- Systems are modular and communicate via `GameEvents`.
- Item definitions are created at runtime by `ItemDatabase` (no broken ScriptableObject links).
- The island is generated procedurally (`IslandGenerator`) so the project has no dependency on external binary terrain assets.
- Building and gathering objects are primitive-based with PBR-tinted materials for reliability.

---

## Known Limitations

- Unity Editor was **not available** in the CI/authoring environment; **Play Mode was not executed here**. Compile and Play tests must be verified locally in Unity Hub.
- Visuals use procedural meshes and solid materials rather than hand-authored high-poly scanned assets. For production art, replace trees/rocks/water with licensed packs (CC0 / Unity Asset Store).
- Audio is procedurally generated soft noise/tones. Replace with licensed field recordings for release quality.
- Scene YAML references bootstrap scripts via fixed GUIDs; if Unity reimports and breaks links, add `GameWorldBootstrap` / `MainMenuBootstrap` to an empty GameObject in each scene.

---

## License

- **Code**: MIT (see `LICENSE`)
- **Third-party**: Unity packages retain their own licenses. Any future art/audio assets must keep attribution in `Docs/THIRD_PARTY.md`.

---

## Repository

**https://github.com/mtigang/Island-Survival**
