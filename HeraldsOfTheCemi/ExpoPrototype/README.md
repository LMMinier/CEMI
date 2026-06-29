# Heralds of the CEMI — Expo Go 3D Prototype

A lightweight, playable 3D mission-loop prototype for Expo SDK 54. It is designed for testing the game concept on a phone before building the full PC game in s&box.

## What is playable

- Third-person movement through a procedural Bronx-inspired city block
- Touch D-pad controls
- Graffiti mission
- Streetball mission
- Neighborhood-defense mission
- Objective beacons, reputation score, and free-roam state

## Run in Expo Snack

1. Open https://snack.expo.dev
2. Select Expo SDK 54.
3. Replace the default `App.js` with this folder's `App.js`.
4. Add the dependency `expo-gl` with version `~16.0.10` if Snack does not add it automatically.
5. Run on an iOS or Android device with Expo Go in landscape mode.

Do not enable remote JavaScript debugging while testing GLView. The graphics context must run on the device.

## Run locally

```bash
npm install
npx expo start
```

## Important architecture note

Expo Go cannot run s&box or Source 2 content. This project is the mobile gameplay prototype. The production s&box game should recreate the same mission definitions, interaction zones, progression loop, and map landmarks using s&box C# components and Source 2 assets.
