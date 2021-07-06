import { Room, Client, ServerError, matchMaker } from "colyseus";
import { InteractableState, NetworkedEntityState, RoomState } from "./schema/RoomState";
import { DI } from "../config/database.config";
import * as matchmakerHelper from "../helpers/matchmakerHelper";
import * as interactableObjectFactory from "../helpers/interactableObjectFactory";
import { User } from "../entities/UserEntity";
import { AvatarState } from "./schema/AvatarState";
import { Vector, Vector2, Vector3 } from "../helpers/Vectors";

const logger = require("../helpers/logger");

export class MMORoom extends Room<RoomState> {

  progress: string;
  defaultObjectReset: number = 5000;

  onCreate(options: any) {
    this.setState(new RoomState());

    logger.info("*********************** MMO ROOM CREATED ***********************");
    console.log(options);
    logger.info("***********************");

    if (options["roomId"] != null) {
      this.roomId = options["roomId"];
    }

    this.maxClients = 150;
    this.progress = options["progress"] || "0,0";

    // Set initial state
    this.setState(new RoomState());

    // Register message handlers for messages from the client
    this.registerForMessages();

    // Set the frequency of the patch rate
    this.setPatchRate(50);

    // Set the Simulation Interval callback
    this.setSimulationInterval(dt => {
      this.state.serverTime += dt;
      this.checkObjectReset();
    });
  }

  /** onAuth is called before onJoin */
  async onAuth(client: Client, options: any, request: any) {

    const userRepo = DI.em.fork().getRepository(User);

    // Check for a user with a pending sessionId that matches this client's sessionId
    let user: User = await userRepo.findOne({ pendingSessionId: client.sessionId });

    if (user) {
      // A user with the pendingSessionId does exist

      // Update user; clear their pending session Id and update their active session Id
      user.activeSessionId = client.sessionId;
      user.pendingSessionId = "";

      // Save the user changes to the database
      await userRepo.flush();

      // Returning the user object equates to returning a "truthy" value that allows the onJoin function to continue
      return user;
    }
    else {
      // No user object was found with the pendingSessionId like we expected
      logger.error(`On Auth - No user found for session Id - ${client.sessionId}`);

      throw new ServerError(400, "Bad session!");
    }
  }

  async onJoin(client: Client, options: any, auth: any) {

    // Create a new instance of NetworkedEntityState for this client and assign initial state values
    let newNetworkedUser = new NetworkedEntityState().assign({
      id: client.id,
      timestamp: this.state.serverTime,
      username: auth.username
    });

    if (auth.position != null) {
      newNetworkedUser.assign({
        xPos: auth.position.x,
        yPos: auth.position.y,
        zPos: auth.position.z,
      });

    }

    if (auth.rotation != null) {
      newNetworkedUser.assign({
        xRot: auth.rotation.x,
        yRot: auth.rotation.y,
        zRot: auth.rotation.z,
      });

    }

    if (auth.avatar != null) {
      newNetworkedUser.avatar = new AvatarState().assign({
        skinColor: auth.avatar.skinColor,
        shirtColor: auth.avatar.shirtColor,
        pantsColor: auth.avatar.pantsColor,
        hatColor: auth.avatar.hatColor,
        hatChoice: auth.avatar.hatChoice,
      });
    }

    // Sets the coin value of the networked user defaulting to 0 if none exists
    newNetworkedUser.coins = auth.coins || 0;

    // Add the networked user to the collection; 
    // This will trigger the OnAdd event of the state's "networkedUsers" collection on the client
    // and the client will spawn a character object for this use.
    this.state.networkedUsers.set(client.id, newNetworkedUser);
  }

  // Callback when a client has left the room
  async onLeave(client: Client, consented: boolean) {

    const userRepo = DI.em.fork().getRepository(User);

    // Find the user object in the database by their activeSessionId
    let user: User = await userRepo.findOne({ activeSessionId: client.sessionId });

    if (user) {
      // Clear the user's active session
      user.activeSessionId = "";
      user.position = this.state.getUserPosition(client.sessionId);
      user.rotation = this.state.getUserRotation(client.sessionId);

      // Save the user's changes to the database
      await userRepo.flush();
    }

    try {
      if (consented) {
        throw new Error("consented leave!");
      }

      logger.info("let's wait for reconnection for client: " + client.id);
      const newClient = await this.allowReconnection(client, 3);
      logger.info("reconnected! client: " + newClient.id);

    } catch (e) {
      logger.info("disconnected! client: " + client.id);
      logger.silly(`*** Removing Networked User and Entity ${client.id} ***`);

      //remove user
      let entityStateToLeave = this.state.networkedUsers.get(client.id);
      if (entityStateToLeave) {
        this.state.networkedUsers.delete(client.id);
      }
    }
  }

  onDispose() {
    console.log("room", this.roomId, "disposing...");
  }

  /**
   * Callback for the "entityUpdate" message from the client to update an entity
   * @param {*} clientID The sessionId of the client we want to update
   * @param {*} data The data containing the data we want to update the newtworkedUser with
   */
  onEntityUpdate(clientID: string, data: any) {

    // Assumes that index 0 is going to be the sessionId of the user
    if (this.state.networkedUsers.has(`${data[0]}`) === false) {
      logger.info(`Attempted to update client with id ${data[0]} but room state has no record of it`)
      return;
    }

    let stateToUpdate = this.state.networkedUsers.get(data[0]);

    let startIndex = 1;

    for (let i = startIndex; i < data.length; i += 2) {
      const property = data[i];
      let updateValue = data[i + 1];
      if (updateValue === "inc") {
        updateValue = data[i + 2];
        i++; // inc i once more since we had a inc;
      }

      (stateToUpdate as any)[property] = updateValue;
    }

    stateToUpdate.timestamp = parseFloat(this.state.serverTime.toString());
  }

  /**
   * Message handler for when a user is moving between rooms.
   * @param client Client for the user moving between rooms
   * @param gridDelta The delta values of the grid change.
   * @param position The player's position they should be at in the next room.
   * @returns 
   */
  async onGridUpdate(client: Client, gridDelta: Vector2, position: Vector3) {

    const userRepo = DI.em.fork().getRepository(User);

    // Check that the client is in the room
    if (this.state.networkedUsers.has(client.sessionId) === false) {

      logger.error(`*** On Grid Update -  User not in room - can't update their grid! - ${client.sessionId} ***`);
      return;
    }

    // Grid change must be greater that 0 in any direction
    if (gridDelta.x === 0 && gridDelta.y === 0) {

      logger.error(`*** On Grid Update -  No grid change detected! ***`);
      return;
    }

    // Get the user object by the active session Id
    let user = await userRepo.findOne({ activeSessionId: client.sessionId });

    if (!user) {

      logger.error(`*** On Grid Update - Error finding player! - ${client.sessionId} ***`);
      return;
    }

    // Calculate the new grid
    let progress = user ? user.progress : "0,0" || "0,0";

    const currentGrid: string[] = progress.split(",");
    const currentX = Number(currentGrid[0]);
    const currentY = Number(currentGrid[1]);

    const newGrid: Vector2 = new Vector2(currentX + gridDelta.x, currentY + gridDelta.y);

    if (isNaN(newGrid.x) || isNaN(newGrid.y)) {

      logger.error(`*** On Grid Update - Error calculating new grid position! X = ${newGrid.x}  Y = ${newGrid.y} ***`);
      return;
    }

    const newGridString: string = `${newGrid.x},${newGrid.y}`;

    // Get seat reservation for the player's new grid
    const seatReservation: matchMaker.SeatReservation = await matchmakerHelper.matchMakeToRoom("lobby_room", newGridString);

    if (!seatReservation) {

      logger.error(`*** On Grid Update - Error getting seat reservation at grid \"${newGridString}\" ***`);
      return;
    }

    // Update the user to reflect the grid change
    user.progress = newGridString;
    user.prevGrid = progress;
    user.pendingSessionId = seatReservation.sessionId;
    user.position = new Vector3(position.x, position.y, position.z);
    user.rotation = this.state.getUserRotation(client.sessionId);
    user.updatedAt = new Date();

    // Save the user's changes to the database
    await userRepo.flush();

    const gridUpdate: any = {
      newGridPosition: newGrid,
      prevGridPosition: new Vector2(currentX, currentY),
      seatReservation
    };

    // Send client the new seat reservation and grid data
    client.send("movedToGrid", gridUpdate);
  }

  /** Register the message handlers for messages that come from the client */
  registerForMessages() {
    // Set the callback for the "entityUpdate" message
    this.onMessage("entityUpdate", (client, entityUpdateArray) => {

      if (this.state.networkedUsers.has(`${entityUpdateArray[0]}`) === false) return;
      this.onEntityUpdate(client.id, entityUpdateArray);
    });

    this.onMessage("objectInteracted", (client, objectInfoArray) => {
      this.handleObjectInteraction(client, objectInfoArray);
    });

    this.onMessage("transitionArea", (client: Client, transitionData: Vector[]) => {

      if (!transitionData || transitionData.length < 2) {
        logger.error(`*** Grid Change Error! Missing data for grid change! ***`);

        return;
      }

      this.onGridUpdate(client, transitionData[0] as Vector2, transitionData[1] as Vector3);
    });

    this.onMessage("avatarUpdate", (client: Client, state: any) => {
      this.handleAvatarUpdate(client, state);
    });
  }

  async handleObjectInteraction(client: Client, objectInfo: any) {

    const userRepo = DI.em.fork().getRepository(User);

    //If the server is not yet aware of this item, lets change that
    if (this.state.interactableItems.has(objectInfo[0]) === false) {
      let interactable = interactableObjectFactory.getStateForType(objectInfo[1]);
      interactable.assign({
        id: objectInfo[0],
        inUse: false
      });
      this.state.interactableItems.set(objectInfo[0], interactable);
    }

    //Get the interactable item
    let interactableObject = this.state.interactableItems.get(objectInfo[0]);
    if (interactableObject.inUse) {
      //console.log("Attempted to use an object that was already in use!");
    }
    else {
      let interactingState = this.state.networkedUsers.get(client.id);
      if (interactingState != null && interactableObject != null) {
        if (this.handleObjectCost(interactableObject, interactingState)) {

          interactableObject.inUse = true;
          interactableObject.availableTimestamp = this.state.serverTime + interactableObject.useDuration;

          this.broadcast("objectUsed", { interactedObjectID: interactableObject.id, interactingStateID: interactingState.id });

          let userObj: User = await userRepo.findOne({ activeSessionId: client.sessionId });

          if (userObj) {
            userObj.coins = interactingState.coins;

            await userRepo.flush();
          }
        }
      }
    }
  }

  handleObjectCost(object: InteractableState, user: NetworkedEntityState): boolean {
    let cost: number = object.coinChange;
    let worked: boolean = false;

    //Its a gain, no need to check
    if (cost >= 0) {
      user.coins += cost;
      worked = true;
    }
    //Check if user can afford this
    if (cost < 0) {
      if (Math.abs(cost) <= user.coins) {
        user.coins += cost;
        worked = true;
      }
      else {
        worked = false;
      }
    }

    return worked;
  }

  checkObjectReset() {
    this.state.interactableItems.forEach((state: InteractableState) => {
      if (state.inUse && state.availableTimestamp <= this.state.serverTime) {
        state.inUse = false;
        state.availableTimestamp = 0.0;
      }
    });
  }

  handleAvatarUpdate(client: Client, state: any) {
    let user = this.state.networkedUsers.get(client.id);
    if (user != null) {

      let avatarState = new AvatarState().assign({
        skinColor: state[0],
        shirtColor: state[1],
        pantsColor: state[2],
        hatColor: state[3],
        hatChoice: state[4]
      });

      user.avatar = avatarState;
      this.saveAvatarUpdate(client);
    }
  }

  async saveAvatarUpdate(client: Client) {
    const userRepo = DI.em.fork().getRepository(User);
    let user: User = await userRepo.findOne({ activeSessionId: client.sessionId });
    if (user) {
      let avatarState = this.state.getUserAvatarState(client.sessionId);
      user.avatar = avatarState;
      await userRepo.flush();
    }
  }
}