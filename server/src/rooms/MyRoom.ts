import { Room, Client } from "@colyseus/core";
import { MyRoomState } from "./schema/MyRoomState";
import { Player } from "./schema/Player";

export class MyRoom extends Room<MyRoomState> {
  maxClients = 4;
  state = new MyRoomState();

  onCreate(options: any) {
    console.log("Room created!", options);

    //Handle player movement
    this.onMessage("playerUpdate", (client, message) => {
      const player = this.state.players.get(client.sessionId);
      if (!player) return;

      // Position & Rotation
      player.x = message.x;
      player.y = message.y;
      player.z = message.z;
      player.rotationY = message.rotationY;

      // Velocity
      player.velocityX = message.velocityX;
      player.velocityY = message.velocityY;
      player.velocityZ = message.velocityZ;

      // Animation States
      player.animInputX = message.animInputX;
      player.animInputY = message.animInputY;
      player.isGrounded = message.isGrounded;
      player.isJumping = message.isJumping;

      // Camera
      player.cameraRotationX = message.cameraRotationX;
      player.cameraRotationY = message.cameraRotationY;

      player.timestamp = Date.now();
    });

    //Set update rate (60 times per second)
    this.setSimulationInterval((deltaTime) =>
      this.update(deltaTime), 1000 / 60
    );
  }

  onJoin(client: Client, options: any) {
    console.log(client.sessionId, "joined!");

    //Create a new player
    const player = new Player();
    player.sessionId = client.sessionId;

    //Spawn at random position
    player.x = Math.random() * 10 - 5;
    player.y = 0;
    player.z = Math.random() * 10 - 5;

    //Add player to state
    this.state.players.set(client.sessionId, player);
  }

  onLeave(client: Client, consented: boolean) {
    console.log(client.sessionId, "left!");

    //Remove player from state
    this.state.players.delete(client.sessionId);
  }

  onDispose() {
    console.log("room", this.roomId, "disposing...");
  }

  update(deltaTime: number) {
    //server-side game logics
  }

}
