"use strict";
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
exports.ChatRoom = void 0;
const colyseus_1 = require("colyseus");
const RoomState_1 = require("./schema/RoomState");
const logger = require("../helpers/logger");
class ChatRoom extends colyseus_1.Room {
    constructor() {
        super(...arguments);
        this.messageLifetime = 5000;
        this.serverTime = 0;
    }
    onCreate(options) {
        this.setState(new RoomState_1.ChatRoomState());
        logger.info("*********************** MMO CHAT ROOM CREATED ***********************");
        console.log(options);
        logger.info("***********************");
        if (options["messageLifetime"]) {
            this.messageLifetime = options["messageLifetime"];
        }
        this.registerForMessages();
        // Set the frequency of the patch rate
        this.setPatchRate(1000 / 20);
        // Set the Simulation Interval callback
        this.setSimulationInterval(dt => {
            this.serverTime += dt;
            this.pruneMessages();
            if (this.state == null) {
                console.log("We have no state!");
            }
        });
    }
    onJoin(client, options) {
        logger.info("*********************** MMO CHAT ROOM JOINED ***********************");
        console.log(options);
        logger.info("***********************");
        let emptyChatQueue = new RoomState_1.ChatQueue();
        this.state.chatQueue.set(client.sessionId, emptyChatQueue);
    }
    // Callback when a client has left the room
    onLeave(client, consented) {
        return __awaiter(this, void 0, void 0, function* () {
            this.state.chatQueue.delete(client.sessionId);
        });
    }
    onDispose() {
        console.log("room", this.roomId, "disposing...");
    }
    registerForMessages() {
        this.onMessage("sendChat", (client, message) => {
            this.handleNewMessage(client, message);
        });
    }
    handleNewMessage(client, message) {
        let newChatMessage = new RoomState_1.ChatMessage().assign({
            senderID: client.sessionId,
            message: message.message,
            timestamp: this.serverTime + this.messageLifetime
        });
        this.placeMessageInQueue(client, newChatMessage);
    }
    placeMessageInQueue(client, newChatMessage) {
        //If there are other chat messages for this client, we need to give them time to be displayed before we show this one
        //If we want to auto expire the oldest message per client, this logic needs to be replaced
        let modifiedTimestamp = newChatMessage.timestamp;
        let chatQueue = this.state.chatQueue.get(client.id);
        chatQueue.chatMessages.forEach((chatMessage) => {
            if (chatMessage.senderID == client.id) {
                //If the timestamp for this message is too close to the desired new timestamp, make it later
                let diff = modifiedTimestamp - chatMessage.timestamp;
                if (diff < this.messageLifetime) {
                    modifiedTimestamp = chatMessage.timestamp + this.messageLifetime;
                }
            }
        });
        newChatMessage.timestamp = modifiedTimestamp;
        chatQueue.chatMessages.push(newChatMessage);
    }
    //Iterate through all messages. any that have a timestamp <= the current serverTime get removed from the array
    pruneMessages() {
        this.state.chatQueue.forEach((queue, id) => {
            queue.chatMessages.forEach((message, index) => {
                if (this.serverTime >= message.timestamp) {
                    queue.chatMessages.splice(index, 1);
                }
            });
        });
    }
}
exports.ChatRoom = ChatRoom;
