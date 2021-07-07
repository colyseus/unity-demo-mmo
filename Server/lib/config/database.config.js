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
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.connect = exports.DI = void 0;
const core_1 = require("@mikro-orm/core");
const mikro_orm_config_1 = __importDefault(require("./mikro-orm.config"));
const UserEntity_1 = require("../entities/UserEntity");
exports.DI = {};
function connect() {
    return __awaiter(this, void 0, void 0, function* () {
        mikro_orm_config_1.default.clientUrl = process.env.DEMO_DATABASE;
        exports.DI.orm = yield core_1.MikroORM.init(mikro_orm_config_1.default);
        exports.DI.em = exports.DI.orm.em;
        exports.DI.userRepository = exports.DI.orm.em.getRepository(UserEntity_1.User);
    });
}
exports.connect = connect;
