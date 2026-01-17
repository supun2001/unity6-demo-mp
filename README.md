# Unity 6 Multiplayer Demo (Colyseus)

A real-time multiplayer demo built with **Unity 6** and **Colyseus** (Node.js/TypeScript). This project demonstrates a complete lobby system, player synchronization, skin customization, and optimized network architecture.

## 🚀 Features

### **Core Gameplay**
- **Real-time Multiplayer**: Synchronized player movement and rotation.
- **Lobby System**:
  - **Create Game**: Hosts a new room.
  - **Join Game**: Joins an existing room via code.
  - **Room Code**: Displayed in HUD for easy sharing.
- **Ready System**: Players must mark themselves as "Ready" before the game starts.

### **Customization (Skin System)**
- **Skin Selection**: 
  - Players can cycle through available skins in the Main Menu.
  - Supports custom UI previews (Sprite) separate from 3D textures.
  - Selection is saved locally (`PlayerPrefs`) and automatically synced to the server on join.
- **Synchronization**: Remote players see your selected skin immediately upon spawning.

### **Technical Optimizations**
- **Network Throttling**: Player state updates are throttled (20Hz / 50ms interval) to reduce bandwidth usage while maintaining smoothness via interpolation.
- **Event-Driven UI**: The Lobby UI uses an event-driven architecture (listening to `OnPlayerAdded`, `OnPlayerRemoved`, `OnStateChange`) instead of polling frames, significantly reducing CPU usage and garbage collection.
- **Colyseus Schema**: Type-safe synchronization for player position, rotation, animation state, and skins.

## 📂 Project Structure

- **`client/mp-demo`**: The Unity Client project.
- **`server`**: The Colyseus (Node.js) server project.

## 🛠️ Setup & Running

### **1. Server Setup**
The server handles room management and state synchronization.

```bash
cd server
npm install
npm run start
```
*Server runs on `ws://localhost:2567` by default.*

### **2. Client Setup (Unity)**
1. Open Unity Hub and add the project from `client/mp-demo`.
2. Ensure you have **Unity 6** (or compatible version) installed.
3. Open the main **Lobby** scene.
4. Press **Play**.
5. Keep the server running in the background.

## 🎮 Controls

- **Movement**: `W`, `A`, `S`, `D`
- **Jump**: `Space`
- **Look**: Mouse
- **Unlock Cursor**: `Escape` (to interact with UI)

## 🔧 Key Scripts

- **`NetworkManager.cs`**: Handles connection, room joining, and spawns player objects. Manages global network events.
- **`LobbyUI.cs`**: Manages the Main Menu and HUD. Handles visual updates for player lists, ready states, and skin selection.
- **`NetworkPlayer.cs`**: synchronizes local movement to the server and interpolates remote players. Implements network throttling.
- **`PlayerAppearance.cs`**: applied synced skin data (Texture) to the player's model.
- **`SkinRegistry.cs`** *(ScriptableObject)*: Database of available skins (Texture + UI Sprite).

## 📄 Schema (Server)

The server synchronizes the following `Player` state:
- `x, y, z` (Position)
- `rotationY`
- `state` (Animation parameters)
- `isReady` (Lobby status)
- `skinIndex` (Visual customization)