# Island Survival

A single-player 3D island survival game built with **Unity 2022.3 LTS** and the **Universal Render Pipeline (URP)**.

**v1.1 — Visual Upgrade:** procedural high-detail island, premium runtime UI, richer day/night atmosphere, textured materials, wind, water animation, and polished HUD.

---

## Features

- Procedural tropical island with beaches, forests, rocky coasts, pond, cave, landmarks
- First-person controller with smooth movement
- Interaction, gathering, inventory, hotbar, crafting, building
- Hunger / thirst / energy survival
- Day/night cycle with sunrise & sunset color science
- Weather (clear / cloudy / rain)
- Save / load
- Fully redesigned runtime UI (panels, buttons, bars, icons, hotbar)

---

## Requirements

| Item | Value |
|------|--------|
| Unity | **2022.3 LTS** (2022.3.50f1 target) |
| Pipeline | URP 14.x |
| GPU target | RTX 4050 class (also GTX 1660+) |

---

## Open the project

```bash
git clone https://github.com/mtigang/Island-Survival.git
```

Unity Hub → Open → `Island-Survival` → Play `MainMenu` or `GameWorld`.

The world and UI are **runtime-bootstrapped** — press Play and everything is generated.

---

## Controls

| Input | Action |
|-------|--------|
| WASD / Mouse | Move / Look |
| Shift / Ctrl / Space | Sprint / Crouch / Jump |
| E | Interact |
| Tab / C / B | Inventory / Craft / Build |
| 1–9 | Hotbar / craft / build |
| R / LMB / Q | Rotate / Place / Cancel build |
| Esc / F5 | Pause / Quick save |

---

## Visual upgrade notes (v1.1)

- `VisualAssetFactory` — procedural terrain textures, UI sprites, item icons
- Higher-resolution island mesh, biome tinting, foam, wind sway
- Animated water surface + shore foam
- Moon light + richer sun gradients
- QualitySettings tuned for mid-range GPUs
- Complete HUD / menu redesign with sliced panels and hotbar icons

---

## Limitations

- Unity Editor Play Mode was **not executed** in the authoring environment.
- Geometry is still procedural primitives (not scanned AAA foliage). For commercial fidelity, swap in licensed vegetation/water packs.
- Post-processing stack volumes depend on local URP asset assignment after first open.

## License

MIT — see `LICENSE`
