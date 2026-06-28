# HERALDS OF THE CEMÍ
## Online Bronx Superhero Action Game — Vertical Slice Design

## One-line pitch

A third-person online action game set in a fictionalized living Bronx district where players become Heralds, roam the city, master resonance powers, play streetball, create legal graffiti, form crews and protect community nodes from Aperture Urban Systems.

## Player fantasy

You are not a generic caped hero dropped into a backdrop. You are a neighborhood-born Herald whose power becomes stronger by learning the block, building relationships and routing pressure through people and places. The city is both the open world and the power system.

The core principle from JC's story remains intact: **you are not a wall or a battery; you are a path.**

## Format

- Third-person 3D action and social free roam
- Online multiplayer sessions
- Cooperative story and neighborhood missions
- Competitive and social streetball courts
- Crew creation, identity and shared progression
- PC-first s&box release
- Dense district design rather than a huge empty map

## First playable district

The first release should be one detailed, highly interactive fictional Bronx neighborhood rather than an inaccurate full-borough replica.

### 1. Cemí Plaza

The central social spawn with a community mural, crew boards, mission contacts, vendors and a visible map of neighborhood pressure.

### 2. Rafá's Bodega Row

A busy commercial strip with rooftops, alleys, NPC conversations, delivery missions and an early Aperture surveillance encounter.

### 3. The Court

A fenced streetball park supporting warmups, 1v1 and later 2v2/3v3 sessions. The surrounding crowd reacts to local players, crew rivalries and high-reputation Heralds.

### 4. Clinic Block

A protected community node. Missions include escorting supplies, defending the clinic from extraction equipment and routing resonance to keep systems operating during blackouts.

### 5. Elevated Line

A traversal corridor under and around the train structure. It provides rooftop routes, combat spaces, graffiti walls and the first major clue connected to Mikey's signal.

### 6. Highway Garden

A quieter community space used for grounding, recovery, movement tutorials and story scenes with Abuela.

### 7. House of Cemí

The rooftop headquarters and late-vertical-slice mission location. Players learn to combine resonance patterns and launch the Corridor Strike together.

## Core gameplay loop

1. Enter the neighborhood and meet players or crew members.
2. Move through the city using street and rooftop routes.
3. Discover community nodes, activity locations and Aperture pressure events.
4. Choose combat, story, streetball, graffiti or neighborhood work.
5. Earn Street Cred, resonance mastery, cosmetics and community trust.
6. Upgrade the player's Herald style and contribute to crew goals.
7. Unlock larger cooperative missions and new districts.

## Hero play

### Movement

The target feel is grounded but athletic:

- responsive third-person running and sprinting;
- vaulting and mantling;
- fire-escape and rooftop traversal;
- short resonance-assisted leaps rather than unlimited flight;
- rail and ledge movement where it improves flow;
- fast recovery into combat or street activities.

### Resonance abilities

Each ability should feel tied to routing, rhythm and place.

- **Resonance Strike:** focused short-range or aimed pulse against Aperture targets.
- **Ground:** convert movement into stability and recover resonance near community nodes.
- **Route:** redirect an incoming force, hazard or energy stream through another valid target.
- **Agreement Pattern:** synchronized crew ability using the rhythm three quick, one low, two wide.
- **Community Link:** temporary cooperative buff gained by connecting multiple neighborhood nodes.

The vertical slice begins with Resonance Strike and Herald Mode. Route, Ground and Agreement should be added once baseline networking and combat feel are stable.

## Streetball

Streetball must become a real game inside the game, not a menu minigame.

### Vertical-slice implementation

The first code foundation handles:

- two-player court joining;
- networked possession;
- score state;
- first-to-11, win-by-2 rules;
- winner rewards;
- mission progression.

### Production target

- networked physics ball;
- possession and loose-ball states;
- dribble packages;
- passing, steals and blocks;
- shot meter affected by range, contest and movement;
- rebounds and out-of-bounds;
- 1v1, 2v2 and 3v3 queues;
- court spectators;
- crew court records and seasonal ladders;
- cosmetic celebrations that never become pay-to-win.

## Graffiti

Graffiti is an authored creative and reputation system placed on designated legal or story-approved walls.

### Vertical-slice implementation

- interactable mural walls;
- last player and crew claim;
- synchronized claim state;
- Street Cred and mission progress.

### Production target

- a curated library of original tags, letters, symbols and Cemí-inspired motifs;
- color and layer selection;
- spray animation and sound;
- placement validation restricted to approved surfaces;
- moderation-safe user combinations;
- rotating crew mural events;
- no copying of living artists' work or real unauthorized tags.

## Crews

Crews are persistent social teams, not simply party names.

### Crew systems

- create, name and invite;
- founder, captain and member roles;
- emblem, colors and outfit accents;
- shared Street Cred and neighborhood trust;
- cooperative mission board;
- crew court record;
- mural history;
- crew hideout upgrades;
- optional rival events without forced griefing;
- moderation, reporting and name filtering.

The current vertical slice provides synchronized crew identity and world-based crew boards. Persistence, invitations and roles require a server-backed profile layer.

## Mission families

### Story missions

Follow JC, Sofía, Manny, Elena, Abuela and the mystery of Mikey while investigating Aperture's resonance-harvesting corridor.

### Neighborhood missions

Defend community nodes, restore services, deliver supplies, escort residents and remove Aperture equipment.

### Crew missions

Coordinate multiple roles, hold linked locations, complete synchronized Agreement patterns and compete for seasonal records.

### Activity missions

Win court games, finish movement routes, complete mural sets and discover hidden city stories.

### Dynamic pressure events

Short multiplayer events appear across the district: drones scan a block, an extractor destabilizes a node, or a signal must be routed between rooftops.

## Multiplayer authority

The host must own important game truth:

- damage and enemy defeat;
- activity joining and score;
- mission progress and rewards;
- crew changes;
- Street Cred and unlocks;
- graffiti claims;
- district discovery.

Clients own responsive input and presentation. Client requests are distance-checked and state-checked before the host changes synchronized values. Cosmetic broadcasts occur only after approval.

## Art direction

- stylized realism rather than photoreal imitation;
- bold manga/comic framing in UI, mission intros and impact effects;
- authentic density, color, signage rhythm, rooftops, trains and community spaces;
- original businesses and architecture inspired by the Bronx without copying exact private residences or trademarked storefronts;
- Cemí forms integrated thoughtfully into the supernatural visual language;
- day, sunset, rain and night lighting as later environment states.

## Audio direction

The Undertone should function as gameplay information:

- neighborhood nodes have distinct low-frequency signatures;
- danger introduces Aperture interference;
- the Agreement rhythm is audible before it is visible;
- court ambience, train movement, conversations and music make the hub feel occupied;
- soundtrack direction can mix Bronx hip-hop percussion, Afro-Caribbean rhythm, ambient electronics and cinematic texture while using original music.

## Vertical-slice mission

### Corridor Warning

1. Spawn at Cemí Plaza and learn movement.
2. Discover Rafá's Bodega Row.
3. Join or create a crew identity.
4. Complete one mural tag.
5. Play one court match.
6. Accept Sofía's signal mission.
7. Defeat three Aperture drones under the elevated line.
8. Activate the clinic and garden community nodes.
9. Reach the House of Cemí rooftop.
10. Use a cooperative Agreement sequence to stop the corridor test.
11. End on Elena's reveal that the interference signature matches the month Mikey disappeared.

## Definition of a real playable vertical slice

The build is not ready merely because scripts exist. It becomes a playable vertical slice when all of the following are true inside s&box:

- two or more test clients can join;
- each player can spawn, move and see the others;
- the district graybox has collision and navigation;
- combat damage and rewards replicate correctly;
- one mission can be accepted and completed;
- one graffiti wall can be claimed;
- one streetball match can be joined and won;
- crew identity appears on the HUD;
- the session can reach a clear mission ending;
- disconnects and rejoining do not corrupt the session.

## Production reality

A polished online city game is a multi-discipline project. Code alone cannot create the finished map, character models, animation, VFX, sound, UI art, mission cinematics, server operations and content volume. The correct strategy is to prove one dense district and one complete multiplayer loop, then expand without losing quality.
