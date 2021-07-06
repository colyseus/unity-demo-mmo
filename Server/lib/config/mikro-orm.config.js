"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const UserEntity_1 = require("../entities/UserEntity");
const options = {
    type: 'mongo',
    entities: [UserEntity_1.User],
    dbName: 'TechDemo4'
};
exports.default = options;
