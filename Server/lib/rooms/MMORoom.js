"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    Object.defineProperty(o, k2, { enumerable: true, get: function() { return m[k]; } });
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.MMORoom = void 0;
const colyseus_1 = require("colyseus");
const RoomState_1 = require("./schema/RoomState");
const database_config_1 = require("../config/database.config");
const matchmakerHelper = __importStar(require("../helpers/matchmakerHelper"));
const interactableObjectFactory = __importStar(require("../helpers/interactableObjectFactory"));
const UserEntity_1 = require("../entities/UserEntity");
const Position_1 = require("./schema/Position");
const AvatarState_1 = require("./schema/AvatarState");
const Vectors_1 = require("../helpers/Vectors");
const logger = require("../helpers/logger");
class MMORoom extends colyseus_1.Room {
    constructor() {
        super(...arguments);
        this.defaultObjectReset = 5000;
    }
    onCreate(options) {
        this.setState(new RoomState_1.RoomState());
        logger.info("*********************** MMO ROOM CREATED ***********************");
        console.log(options);
        logger.info("***********************");
        if (options["roomId"] != null) {
            this.roomId = options["roomId"];
        }
        this.maxClients = 150;
        this.progress = options["progress"] || "0,0";
        this.setState(new RoomState_1.RoomState());
        this.registerForMessages();
        // Set the frequency of the patch rate
        this.setPatchRate(1000 / 20);
        // Set the Simulation Interval callback
        this.setSimulationInterval(dt => {
            this.state.serverTime += dt;
            this.checkObjectReset();
        });
    }
    onAuth(client, options, request) {
        return __awaiter(this, void 0, void 0, function* () {
            const userRepo = database_config_1.DI.em.fork().getRepository(UserEntity_1.User);
            // Check for a user with a pending sessionId
            let user = yield userRepo.findOne({ pendingSessionId: client.sessionId });
            if (user) {
                // Update user; clear their pending session Id and update their active session Id
                user.activeSessionId = client.sessionId;
                user.pendingSessionId = "";
                yield userRepo.flush();
                return user;
            }
            else {
                logger.error(`On Auth - No user found for session Id - ${client.sessionId}`);
                throw new colyseus_1.ServerError(400, "Bad session!");
            }
        });
    }
    onJoin(client, options, auth) {
        return __awaiter(this, void 0, void 0, function* () {
            logger.info(`Client joined!- ${client.sessionId} ***`);
            let newNetworkedUser = new RoomState_1.NetworkedEntityState().assign({
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
                newNetworkedUser.avatar = new AvatarState_1.AvatarState().assign({
                    skinColor: auth.avatar.skinColor,
                    shirtColor: auth.avatar.shirtColor,
                    pantsColor: auth.avatar.pantsColor,
                    hatColor: auth.avatar.hatColor,
                    hatChoice: auth.avatar.hatChoice,
                });
            }
            newNetworkedUser.coins = auth.coins || 0;
            this.state.networkedUsers.set(client.id, newNetworkedUser);
        });
    }
    // Callback when a client has left the room
    onLeave(client, consented) {
        return __awaiter(this, void 0, void 0, function* () {
            const userRepo = database_config_1.DI.em.fork().getRepository(UserEntity_1.User);
            logger.info(`*** User Leave - ${client.id} ***`);
            let user = yield userRepo.findOne({ activeSessionId: client.sessionId });
            // Clear the user's active session
            if (user) {
                user.activeSessionId = "";
                user.position = this.state.getUserPosition(client.sessionId);
                user.rotation = this.state.getUserRotation(client.sessionId);
                yield userRepo.flush();
            }
            try {
                if (consented) {
                    throw new Error("consented leave!");
                }
                logger.info("let's wait for reconnection for client: " + client.id);
                const newClient = yield this.allowReconnection(client, 3);
                logger.info("reconnected! client: " + newClient.id);
            }
            catch (e) {
                logger.info("disconnected! client: " + client.id);
                logger.silly(`*** Removing Networked User and Entity ${client.id} ***`);
                //remove user
                let entityStateToLeave = this.state.networkedUsers.get(client.id);
                if (entityStateToLeave) {
                    this.state.networkedUsers.delete(client.id);
                }
            }
        });
    }
    onDispose() {
        console.log("room", this.roomId, "disposing...");
    }
    /**
     * Callback for the "entityUpdate" message from the client to update an entity
     * @param {*} clientID
     * @param {*} data
     */
    onEntityUpdate(clientID, data) {
        if (this.state.networkedUsers.has(`${data[0]}`) === false) {
            logger.info(`Attempted to update client with id ${data[0]} but room state has no record of it`);
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
            stateToUpdate[property] = updateValue;
        }
        stateToUpdate.timestamp = parseFloat(this.state.serverTime.toString());
    }
    onGridUpdate(client, gridDelta, position) {
        return __awaiter(this, void 0, void 0, function* () {
            const userRepo = database_config_1.DI.em.fork().getRepository(UserEntity_1.User);
            // Check that the client is in the room
            if (this.state.networkedUsers.has(client.sessionId) == false) {
                logger.error(`*** On Grid Update -  User not in room - can't update their grid! - ${client.sessionId} ***`);
                return;
            }
            // Grid change must be greater that 0 in any direction
            if (gridDelta.x == 0 && gridDelta.y == 0) {
                logger.error(`*** On Grid Update -  No grid change detected! ***`);
                return;
            }
            // Get the user object by the active session Id
            let user = yield userRepo.findOne({ activeSessionId: client.sessionId });
            if (user == null) {
                logger.error(`*** On Grid Update - Error finding player! - ${client.sessionId} ***`);
                return;
            }
            // Calculate the new grid
            let progress = user ? user.progress : "0,0" || "0,0";
            const currentGrid = progress.split(",");
            const currentX = Number(currentGrid[0]);
            const currentY = Number(currentGrid[1]);
            const newGrid = new Vectors_1.Vector2(currentX + gridDelta.x, currentY + gridDelta.y);
            //logger.silly(`*** On Grid Update - calculate new grid = ${newGrid[0]},${newGrid[1]} ***`);
            if (isNaN(newGrid.x) || isNaN(newGrid.y)) {
                logger.error(`*** On Grid Update - Error calculating new grid position! X = ${newGrid.x}  Y = ${newGrid.y} ***`);
                return;
            }
            const newGridString = `${newGrid.x},${newGrid.y}`;
            // Get seat reservation for player's new grid
            const seatReservation = yield matchmakerHelper.matchMakeToRoom("lobby_room", newGridString);
            if (seatReservation == null) {
                logger.error(`*** On Grid Update - Error getting seat reservation at grid \"${newGridString}\" ***`);
                return;
            }
            // Update the user to reflect the grid change
            user.progress = newGridString;
            user.prevGrid = progress;
            user.pendingSessionId = seatReservation.sessionId;
            user.position = new Position_1.Position().assign(position);
            user.rotation = this.state.getUserRotation(client.sessionId);
            user.updatedAt = new Date();
            yield userRepo.flush();
            const gridUpdate = {
                newGridPosition: newGrid,
                prevGridPosition: new Vectors_1.Vector2(currentX, currentY),
                seatReservation
            };
            // Send client new seat reservation and grid data
            client.send("movedToGrid", gridUpdate);
        });
    }
    registerForMessages() {
        // Set the callback for the "entityUpdate" message
        this.onMessage("entityUpdate", (client, entityUpdateArray) => {
            if (this.state.networkedUsers.has(`${entityUpdateArray[0]}`) === false)
                return;
            this.onEntityUpdate(client.id, entityUpdateArray);
        });
        this.onMessage("objectInteracted", (client, objectInfoArray) => {
            this.handleObjectInteraction(client, objectInfoArray);
        });
        this.onMessage("transitionArea", (client, transitionData) => {
            if (transitionData == null || transitionData.length < 2) {
                logger.error(`*** Grid Change Error! Missing data for grid change! ***`);
                return;
            }
            this.onGridUpdate(client, transitionData[0], transitionData[1]);
        });
        this.onMessage("avatarUpdate", (client, state) => {
            this.handleAvatarUpdate(client, state);
        });
    }
    handleObjectInteraction(client, objectInfo) {
        return __awaiter(this, void 0, void 0, function* () {
            const userRepo = database_config_1.DI.em.fork().getRepository(UserEntity_1.User);
            //If the server is not yet aware of this item, lets change that
            if (this.state.interactableItems.has(objectInfo[0]) == false) {
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
                        let userObj = yield userRepo.findOne({ activeSessionId: client.sessionId });
                        if (userObj) {
                            userObj.coins = interactingState.coins;
                            yield userRepo.flush();
                        }
                    }
                }
            }
        });
    }
    handleObjectCost(object, user) {
        let cost = object.coinChange;
        let worked = false;
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
        this.state.interactableItems.forEach((state) => {
            if (state.inUse && state.availableTimestamp <= this.state.serverTime) {
                state.inUse = false;
                state.availableTimestamp = 0.0;
            }
        });
    }
    handleAvatarUpdate(client, state) {
        let user = this.state.networkedUsers.get(client.id);
        if (user != null) {
            let avatarState = new AvatarState_1.AvatarState().assign({
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
    saveAvatarUpdate(client) {
        return __awaiter(this, void 0, void 0, function* () {
            const userRepo = database_config_1.DI.em.fork().getRepository(UserEntity_1.User);
            let user = yield userRepo.findOne({ activeSessionId: client.sessionId });
            if (user) {
                let avatarState = this.state.getUserAvatarState(client.sessionId);
                user.avatar = avatarState;
                yield userRepo.flush();
            }
        });
    }
}
exports.MMORoom = MMORoom;
