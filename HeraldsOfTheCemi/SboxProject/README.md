# HERALDS OF THE CEMÍ — s&box 3D Multiplayer Game

This folder is the start of the real game: a third-person, online, free-roam Bronx superhero experience built for **s&box**, not a browser game.

## Game vision

Players enter a fictionalized living Bronx district as young Heralds who can hear and route the Undertone. The city is the main hub and social world. Players can:

- roam streets, rooftops, parks, courts, bodegas, train corridors and community spaces;
- use resonance-based superhero abilities against Aperture drones and extraction operations;
- play streetball activities;
- claim legal graffiti walls and build a visual reputation;
- accept narrative, crew and neighborhood missions;
- create or join crews;
- earn Street Cred, Resonance mastery and neighborhood trust;
- play cooperatively in an owner-authoritative online multiplayer session.

## What this branch implements

This is a **vertical-slice gameplay foundation**. It includes current s&box Component-based code for:

- networked player progression and crew identity;
- owner-only requests sent to the host for validation;
- PlayerController use/interact integration;
- resonance strike combat against Aperture drones;
- mission terminals and mission progress;
- graffiti-wall claiming;
- a networked streetball court scoring loop;
- district discovery;
- a Razor HUD.

It does **not** pretend that the final Bronx city, finished character art, animations, vehicles, matchmaking backend or production streetball physics already exist. Those require scenes, models, animation, audio and playtesting inside the s&box editor.

## Required scene setup

1. Create a new **Game Project** in s&box.
2. Copy this folder's `Code` directory into the project.
3. Create a startup scene named `scenes/bronx_hub.scene`.
4. Add a player prefab with:
   - `PlayerController` configured for third-person play;
   - a Citizen-compatible character renderer/animator;
   - `CemiPlayerState`;
   - `CemiHeroAbilities`;
   - `CemiInteractionController`;
   - a `ScreenPanel` and `CemiHud`.
5. Set the player prefab's network mode to **Object** before spawning it. `[Sync]` values require `NetworkMode.Object`.
6. Place activity objects in the scene and set their network mode to **Object**:
   - `MissionTerminal`;
   - `GraffitiWall`;
   - `StreetballCourt`;
   - `CrewBoard`;
   - `ApertureDrone`;
   - `DistrictMarker`.
7. Add colliders to interactable objects so `PlayerController` can target them.
8. Configure custom input actions in Project Settings:
   - `attack1` — Resonance Strike;
   - `power` — toggle Herald mode;
   - built-in `use` — interact.
9. Test using s&box's multiplayer test clients before publishing.

## Authority model

- Player input originates on the owning client.
- Gameplay-changing requests use owner-only RPCs to the host.
- The host validates distance, cooldowns and mission/activity state.
- Shared progression uses `[Sync( SyncFlags.FromHost )]`.
- Cosmetic effects are broadcast only after host approval.

This prevents the prototype from trusting arbitrary client-side score, crew, mission or combat changes.

## Immediate production milestones

### Milestone 1 — Playable blockout

Build one dense Bronx district containing a court, legal graffiti alley, bodega, community clinic, elevated-train corridor, rooftops and the House of Cemí. Use graybox geometry first.

### Milestone 2 — Core movement and powers

Tune third-person movement, mantling, rooftop traversal, Resonance Strike, pulse routing, damage, knockback and enemy encounters.

### Milestone 3 — Real streetball

Replace the scoring-loop prototype with a networked physics ball, possession, dribbling, passing, shot timing, rebounds, fouls and 1v1/2v2/3v3 court sessions.

### Milestone 4 — Crews and city missions

Add invitations, roles, crew colors/emblems, shared reputation, territory events, cooperative story missions and persistent storage.

### Milestone 5 — Content and polish

Create original Bronx-inspired assets rather than copying real storefront branding or exact private residences. Add characters, animation, music, voices, VFX, accessibility, moderation and server tools.

## Lore direction

JC is not simply a battery or wall. He is a path: he routes pressure through community nodes. Aperture Urban Systems tries to harvest neighborhood resilience. The multiplayer game expands that idea so every player can become a Herald with a distinct resonance style while protecting the city together.
