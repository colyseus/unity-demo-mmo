"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.ChatRoomState = exports.ChatQueue = exports.ChatMessage = exports.RoomState = exports.InteractableState = exports.NetworkedEntityState = void 0;
const schema_1 = require("@colyseus/schema");
const AvatarState_1 = require("./AvatarState");
// TODO: replace individual position and rotation components with Position and Rotation schemas
class NetworkedEntityState extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.id = "ID";
        this.chatID = "ID";
        //Position
        this.xPos = 0.0;
        this.yPos = 0.0;
        this.zPos = 0.0;
        //Rotation
        this.xRot = 0.0;
        this.yRot = 0.0;
        this.zRot = 0.0;
        this.wRot = 0.0;
        this.avatar = new AvatarState_1.AvatarState();
        this.coins = 0.0;
        //Interpolation values
        this.timestamp = 0.0;
        this.username = "";
    }
}
__decorate([
    schema_1.type("string"),
    __metadata("design:type", String)
], NetworkedEntityState.prototype, "id", void 0);
__decorate([
    schema_1.type("string"),
    __metadata("design:type", String)
], NetworkedEntityState.prototype, "chatID", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], NetworkedEntityState.prototype, "xPos", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], NetworkedEntityState.prototype, "yPos", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], NetworkedEntityState.prototype, "zPos", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], NetworkedEntityState.prototype, "xRot", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], NetworkedEntityState.prototype, "yRot", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], NetworkedEntityState.prototype, "zRot", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], NetworkedEntityState.prototype, "wRot", void 0);
__decorate([
    schema_1.type(AvatarState_1.AvatarState),
    __metadata("design:type", AvatarState_1.AvatarState)
], NetworkedEntityState.prototype, "avatar", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], NetworkedEntityState.prototype, "coins", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], NetworkedEntityState.prototype, "timestamp", void 0);
__decorate([
    schema_1.type("string"),
    __metadata("design:type", String)
], NetworkedEntityState.prototype, "username", void 0);
exports.NetworkedEntityState = NetworkedEntityState;
class InteractableState extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.id = "ID";
        this.inUse = false;
        this.interactableType = "";
        this.availableTimestamp = 0.0;
        this.coinChange = 0.0;
        this.useDuration = 0.0;
    }
}
__decorate([
    schema_1.type("string"),
    __metadata("design:type", String)
], InteractableState.prototype, "id", void 0);
__decorate([
    schema_1.type("boolean"),
    __metadata("design:type", Boolean)
], InteractableState.prototype, "inUse", void 0);
__decorate([
    schema_1.type("string"),
    __metadata("design:type", String)
], InteractableState.prototype, "interactableType", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], InteractableState.prototype, "availableTimestamp", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], InteractableState.prototype, "coinChange", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], InteractableState.prototype, "useDuration", void 0);
exports.InteractableState = InteractableState;
class RoomState extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.networkedUsers = new schema_1.MapSchema();
        this.interactableItems = new schema_1.MapSchema();
        this.serverTime = 0.0;
    }
    getUserPosition(sessionId) {
        if (this.networkedUsers.has(sessionId)) {
            const user = this.networkedUsers.get(sessionId);
            return {
                x: user.xPos,
                y: user.yPos,
                z: user.zPos
            };
        }
        return null;
    }
    setUserPosition(sessionId, position) {
        if (this.networkedUsers.has(sessionId)) {
            const user = this.networkedUsers.get(sessionId);
            user.xPos = position.x;
            user.yPos = position.y;
            user.zPos = position.z;
        }
    }
    getUserRotation(sessionId) {
        if (this.networkedUsers.has(sessionId)) {
            const user = this.networkedUsers.get(sessionId);
            return {
                x: user.xRot,
                y: user.yRot,
                z: user.zRot
            };
        }
        return null;
    }
    getUserAvatarState(sessionId) {
        if (this.networkedUsers.has(sessionId)) {
            const user = this.networkedUsers.get(sessionId);
            return user.avatar;
        }
        return null;
    }
}
__decorate([
    schema_1.type({ map: NetworkedEntityState }),
    __metadata("design:type", Object)
], RoomState.prototype, "networkedUsers", void 0);
__decorate([
    schema_1.type({ map: InteractableState }),
    __metadata("design:type", Object)
], RoomState.prototype, "interactableItems", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], RoomState.prototype, "serverTime", void 0);
exports.RoomState = RoomState;
//Chat related schemas
class ChatMessage extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.senderID = "";
        this.message = "";
        this.timestamp = 0.0;
    }
}
__decorate([
    schema_1.type("string"),
    __metadata("design:type", String)
], ChatMessage.prototype, "senderID", void 0);
__decorate([
    schema_1.type("string"),
    __metadata("design:type", String)
], ChatMessage.prototype, "message", void 0);
__decorate([
    schema_1.type("number"),
    __metadata("design:type", Number)
], ChatMessage.prototype, "timestamp", void 0);
exports.ChatMessage = ChatMessage;
//An array of messages for a user
class ChatQueue extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.chatMessages = new schema_1.ArraySchema();
    }
}
__decorate([
    schema_1.type({ array: ChatMessage }),
    __metadata("design:type", Object)
], ChatQueue.prototype, "chatMessages", void 0);
exports.ChatQueue = ChatQueue;
class ChatRoomState extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.chatQueue = new schema_1.MapSchema();
    }
}
__decorate([
    schema_1.type({ map: ChatQueue }),
    __metadata("design:type", Object)
], ChatRoomState.prototype, "chatQueue", void 0);
exports.ChatRoomState = ChatRoomState;
