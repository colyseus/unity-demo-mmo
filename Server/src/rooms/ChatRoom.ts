import { Room, Client, ServerError } from "colyseus";
import { ChatMessage, ChatRoomState, ChatQueue } from "./schema/RoomState";
import { DI } from "../config/database.config";

const logger = require("../helpers/logger");

export class ChatRoom extends Room<ChatRoomState> {

  messageLifetime: number = 5000;
  serverTime: number = 0;

  onCreate (options: any) {
    this.setState(new ChatRoomState());
    
    logger.info("*********************** MMO CHAT ROOM CREATED ***********************");
    console.log(options);
    logger.info("***********************");

    if(options["messageLifetime"]){
      this.messageLifetime = options["messageLifetime"];
    }

    this.registerForMessages();

    // Set the frequency of the patch rate
    this.setPatchRate(1000 / 20);

    // Set the Simulation Interval callback
    this.setSimulationInterval(dt => {
      this.serverTime += dt;
      this.pruneMessages();
      if(!this.state){
        console.log("We have no state!")
      }
    } );
  }

  onJoin (client: Client, options: any) {
    logger.info("*********************** MMO CHAT ROOM JOINED ***********************");
    console.log(options);
    logger.info("***********************");

    let emptyChatQueue = new ChatQueue();

    this.state.chatQueue.set(client.sessionId, emptyChatQueue);
  }

  // Callback when a client has left the room
  async onLeave(client: Client, consented: boolean) {
    logger.info(`***Chat User Leave - ${client.id} ***`);
    this.state.chatQueue.delete(client.sessionId);
  }

  onDispose() {
    console.log("room", this.roomId, "disposing...");
  }

  registerForMessages(){
    this.onMessage("sendChat", (client: Client, message: any) =>{
      this.handleNewMessage(client, message)});
  }

  handleNewMessage(client: Client, message: any){
    let newChatMessage = new ChatMessage().assign({
      senderID: client.sessionId,
      message: message.message,
      timestamp: this.serverTime + this.messageLifetime
    });

    this.placeMessageInQueue(client, newChatMessage);
  }

  placeMessageInQueue(client:Client, newChatMessage : ChatMessage){
    //If there are other chat messages for this client, we need to give them time to be displayed before we show this one
    //If we want to auto expire the oldest message per client, this logic needs to be replaced
    let modifiedTimestamp = newChatMessage.timestamp;
    let chatQueue : ChatQueue = this.state.chatQueue.get(client.id);
    chatQueue.chatMessages.forEach((chatMessage) =>{
      if(chatMessage.senderID === client.id){
        //If the timestamp for this message is too close to the desired new timestamp, make it later
        let diff = modifiedTimestamp - chatMessage.timestamp;
        if(diff < this.messageLifetime){
          modifiedTimestamp = chatMessage.timestamp + this.messageLifetime;
        }
      }
    });

    newChatMessage.timestamp = modifiedTimestamp;
    chatQueue.chatMessages.push(newChatMessage);
  }

  //Iterate through all messages. any that have a timestamp <= the current serverTime get removed from the array
  pruneMessages(){
    this.state.chatQueue.forEach((queue, id) =>{
      queue.chatMessages.forEach((message, index)=>{
        if(this.serverTime >= message.timestamp){
          queue.chatMessages.splice(index, 1);
        }
      });
    });
  }
}
