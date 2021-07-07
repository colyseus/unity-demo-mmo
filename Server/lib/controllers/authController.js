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
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.logIn = exports.signUp = exports.prepEmail = void 0;
const database_config_1 = require("../config/database.config");
const UserEntity_1 = require("../entities/UserEntity");
const logger_1 = __importDefault(require("../helpers/logger"));
const matchmakerHelper = __importStar(require("../helpers/matchmakerHelper"));
const Position_1 = require("../rooms/schema/Position");
const Rotation_1 = require("../rooms/schema/Rotation");
// Middleware
//===============================================
function prepEmail(req, res, next) {
    if (req.body.email) {
        try {
            req.body.email = req.body.email.toLowerCase();
        }
        catch (err) {
            logger_1.default.error(`Error converting email to lower case`);
        }
    }
    next();
}
exports.prepEmail = prepEmail;
//===============================================
/**
 * Update the user as logged in and assign a pending session Id
 * @param user
 * @param sessionId
 */
function updateUserForNewSession(user, sessionId) {
    user.pendingSessionId = sessionId;
    user.pendingSessionTimestamp = Date.now();
    user.updatedAt = new Date();
    user.position = new Position_1.Position().assign({
        x: 0,
        y: 1,
        z: 0
    });
    user.rotation = new Rotation_1.Rotation().assign({
        x: 0,
        y: 0,
        z: 0
    });
}
/**
 * Simple function for creating a new user account.
 * With successful account creation the user will be matchmaked into the first room.
 * @param req
 * @param res
 * @returns
 */
function signUp(req, res) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            // Check if the necessary parameters exist
            if (req.body.username == null || req.body.email == null || req.body.password == null) {
                logger_1.default.error(`*** Sign Up Error - New user must have a username, email, and password!`);
                throw "New user must have a username, email, and password!";
                return;
            }
            const userRepo = database_config_1.DI.em.fork().getRepository(UserEntity_1.User);
            // Check if an account with the email already exists
            let user = yield userRepo.findOne({ email: req.body.email });
            let seatReservation;
            if (user == null) {
                // Create a new user
                user = userRepo.create({
                    username: req.body.username,
                    email: req.body.email,
                    password: req.body.password
                });
                // Match make the user into a room
                seatReservation = yield matchmakerHelper.matchMakeToRoom("lobby_room", user.progress);
                updateUserForNewSession(user, seatReservation.sessionId);
                // Save the new user to the database
                yield userRepo.persistAndFlush(user);
            }
            else {
                logger_1.default.error(`*** Sign Up Error - User with that email already exists!`);
                throw "User with that email already exists!";
                return;
            }
            const newUserObj = Object.assign({}, user);
            delete newUserObj.password;
            res.status(200).json({
                error: false,
                output: {
                    seatReservation,
                    user: newUserObj
                }
            });
        }
        catch (error) {
            res.status(400).json({
                error: true,
                output: error
            });
        }
    });
}
exports.signUp = signUp;
/**
 * Simple function to sign user in.
 * It performs a simple check if the provided password matches in the user account.
 * With a successful sign in the user will be matchmaked into the room where they left off or into the first room.
 * @param req
 * @param res
 */
function logIn(req, res) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const userRepo = database_config_1.DI.em.fork().getRepository(UserEntity_1.User);
            // Check if the necessary parameters exist
            if (req.body.email == null || req.body.password == null) {
                throw "Missing email or password";
                return;
            }
            // Check if an account with the email exists
            let user = yield userRepo.findOne({ email: req.body.email });
            // Check if passwords match
            let validPassword = user != null ? user.password == req.body.password : false;
            if (user == null || validPassword == false) {
                throw "Incorrect email or password";
                return;
            }
            // Check if the user is already logged in
            if (user.activeSessionId) {
                logger_1.default.error(`User is already logged in- \"${user.activeSessionId}\"`);
                throw "User is already logged in";
                return;
            }
            // Wait a minimum of 30 seconds when a pending session Id currently exists
            // before letting the user sign in again
            if (user.pendingSessionId && user.pendingSessionTimestamp && (Date.now() - user.pendingSessionTimestamp) <= 30000) {
                let timeLeft = (Date.now() - user.pendingSessionTimestamp) / 1000;
                logger_1.default.error(`Can't log in right now, try again in ${timeLeft} seconds!`);
                throw `Can't log in right now, try again in ${timeLeft} seconds!`;
                return;
            }
            // Match make the user into 
            const seatReservation = yield matchmakerHelper.matchMakeToRoom("lobby_room", user.progress);
            updateUserForNewSession(user, seatReservation.sessionId);
            yield userRepo.flush();
            // Don't include the password in the user object sent back to the client
            const userCopy = Object.assign({}, user);
            delete userCopy.password;
            // Send the user data and seat reservation back to the client
            // where the seat reservation can be used by the client to
            // consume the seat reservation and join the room.
            res.status(200).json({
                error: false,
                output: {
                    seatReservation,
                    user: userCopy
                }
            });
        }
        catch (error) {
            res.status(400).json({
                error: true,
                output: error
            });
        }
    });
}
exports.logIn = logIn;
