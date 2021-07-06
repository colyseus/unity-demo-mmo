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
exports.User = void 0;
const core_1 = require("@mikro-orm/core");
const Position_1 = require("../rooms/schema/Position");
const Rotation_1 = require("../rooms/schema/Rotation");
const AvatarState_1 = require("../rooms/schema/AvatarState");
const BaseEntity_1 = require("./BaseEntity");
/**
 * Entity to represent the user in the database and throughout the server
 */
let User = class User extends BaseEntity_1.BaseEntity {
    constructor() {
        super(...arguments);
        this.progress = "0,0";
        this.prevGrid = "0,0";
    }
};
__decorate([
    core_1.Property(),
    __metadata("design:type", String)
], User.prototype, "username", void 0);
__decorate([
    core_1.Property(),
    __metadata("design:type", String)
], User.prototype, "email", void 0);
__decorate([
    core_1.Property(),
    __metadata("design:type", String)
], User.prototype, "password", void 0);
__decorate([
    core_1.Property(),
    __metadata("design:type", String)
], User.prototype, "pendingSessionId", void 0);
__decorate([
    core_1.Property(),
    __metadata("design:type", Number)
], User.prototype, "pendingSessionTimestamp", void 0);
__decorate([
    core_1.Property(),
    __metadata("design:type", String)
], User.prototype, "activeSessionId", void 0);
__decorate([
    core_1.Property(),
    __metadata("design:type", String)
], User.prototype, "progress", void 0);
__decorate([
    core_1.Property(),
    __metadata("design:type", String)
], User.prototype, "prevGrid", void 0);
__decorate([
    core_1.Property(),
    __metadata("design:type", Position_1.Position)
], User.prototype, "position", void 0);
__decorate([
    core_1.Property(),
    __metadata("design:type", Rotation_1.Rotation)
], User.prototype, "rotation", void 0);
__decorate([
    core_1.Property(),
    __metadata("design:type", AvatarState_1.AvatarState)
], User.prototype, "avatar", void 0);
__decorate([
    core_1.Property(),
    __metadata("design:type", Number)
], User.prototype, "coins", void 0);
User = __decorate([
    core_1.Entity()
], User);
exports.User = User;
