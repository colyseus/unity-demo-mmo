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
const arena_1 = __importDefault(require("@colyseus/arena"));
const monitor_1 = require("@colyseus/monitor");
const core_1 = require("@mikro-orm/core");
const express_1 = __importDefault(require("express"));
const logger = require("./helpers/logger");
const database_config_1 = require("./config/database.config");
const userRoutes_1 = __importDefault(require("./routes/userRoutes"));
/**
 * Import your Room files
 */
const MMORoom_1 = require("./rooms/MMORoom");
const ChatRoom_1 = require("./rooms/ChatRoom");
exports.default = arena_1.default({
    getId: () => "Your Colyseus App",
    initializeGameServer: (gameServer) => {
        /**
         * Define your room handlers:
         */
        gameServer.define('chat_room', ChatRoom_1.ChatRoom).filterBy(["roomID"]);
        gameServer.define('lobby_room', MMORoom_1.MMORoom).filterBy(["progress"]);
    },
    initializeExpress: (app) => {
        /**
         * Bind your custom express routes here:
         */
        // Body parser - reads data from request body into json object
        app.use(express_1.default.json());
        app.use(express_1.default.urlencoded({ extended: true, limit: "10kb" }));
        //
        // MikroORM: it is important to create a RequestContext before registering routes that access the database.
        // See => https://mikro-orm.io/docs/identity-map/
        //
        app.use((req, res, next) => core_1.RequestContext.create(database_config_1.DI.orm.em, next));
        // Register routes for our simple user auth
        app.use("/users", userRoutes_1.default);
        // Connect to our database
        database_config_1.connect().then(() => __awaiter(void 0, void 0, void 0, function* () {
            logger.silly(`*** Connected to Database! ***`);
        }));
        app.get("/", (req, res) => {
            res.send("It's time to kick ass and chew bubblegum!");
        });
        /**
         * Bind @colyseus/monitor
         * It is recommended to protect this route with a password.
         * Read more: https://docs.colyseus.io/tools/monitor/
         */
        app.use("/colyseus", monitor_1.monitor());
    },
    beforeListen: () => {
        /**
         * Before before gameServer.listen() is called.
         */
    }
});
