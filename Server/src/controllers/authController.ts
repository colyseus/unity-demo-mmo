import { matchMaker } from "colyseus";
import { DI } from "../config/database.config";
import { User } from "../entities/UserEntity";
import logger from "../helpers/logger";
import * as matchmakerHelper from "../helpers/matchmakerHelper";
import { Vector3 } from "../helpers/Vectors";

const bcrypt = require ('bcrypt');
const saltRounds = 10;
// Middleware
//===============================================
/**
 * Forces the email to be all lower case for consistency
 */
export function prepEmail(req: any, res: any, next: any) {
    if (req.body.email) {
        try {
            req.body.email = req.body.email.toLowerCase();
        }
        catch (err) {
            logger.error(`Error converting email to lower case`);
        }
    }

    next();
}
//===============================================

/**
 * Update the user for a new room session; updates user's pending session Id and resets their position and rotation
 * @param user The user to update for the new session
 * @param sessionId The new session Id
 */
function updateUserForNewSession(user: User, sessionId: string) {

    user.pendingSessionId = sessionId;
    user.pendingSessionTimestamp = Date.now();
    user.updatedAt = new Date();

    user.position = new Vector3(0,1,0);
    user.rotation = new Vector3(0,0,0);
}

/**
 * Simple function for creating a new user account.
 * With successful account creation the user will be matchmaked into the first room.
 * @param req 
 * @param res 
 * @returns 
 */
export async function signUp(req: any, res: any) {
    try {
        // Check if the necessary parameters exist
        if (!req.body.username || !req.body.email || !req.body.password) {

            logger.error(`*** Sign Up Error - New user must have a username, email, and password!`);
            throw "New user must have a username, email, and password!";
            return;
        }

        const userRepo = DI.em.fork().getRepository(User);

        // Check if an account with the email already exists
        let user = await userRepo.findOne({ email: req.body.email });
        let seatReservation;

        if (!user) {

            let password = await encryptPassword(req.body.password);
            
            // Create a new user
            user = userRepo.create({
                username: req.body.username,
                email: req.body.email,
                password: password
            });

            // Match make the user into a room
            seatReservation = await matchmakerHelper.matchMakeToRoom("lobby_room", user.progress);

            updateUserForNewSession(user, seatReservation.sessionId);

            // Save the new user to the database
            await userRepo.persistAndFlush(user);
        }
        else {
            logger.error(`*** Sign Up Error - User with that email already exists!`);
            throw "User with that email already exists!";
            return;
        }

        const newUserObj = { ...user };
        delete newUserObj.password; // Don't send the user's password back to the client

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
}

/**
 * Simple function to sign user in. 
 * It performs a simple check if the provided password matches in the user account.
 * With a successful sign in the user will be matchmaked into the room where they left off or into the first room.
 * @param req 
 * @param res 
 */
export async function logIn(req: any, res: any) {
    try {

        const userRepo = DI.em.fork().getRepository(User);

        // Check if the necessary parameters exist
        if (!req.body.email || !req.body.password) {

            throw "Missing email or password";
            return;
        }

        // Check if an account with the email exists
        let user: User = await userRepo.findOne({ email: req.body.email });

        // Check if passwords match
        let validPassword: boolean = await compareEncrypted(req.body.password, user.password);

        if (!user || validPassword === false) {

            throw "Incorrect email or password";
            return;
        }

        // Check if the user is already logged in
        if (user.activeSessionId) {

            logger.error(`User is already logged in- \"${user.activeSessionId}\"`);

            throw "User is already logged in";
            return;
        }

        // Wait a minimum of 30 seconds when a pending session Id currently exists
        // before letting the user sign in again
        if (user.pendingSessionId && user.pendingSessionTimestamp && (Date.now() - user.pendingSessionTimestamp) <= 30000) {

            let timeLeft = (Date.now() - user.pendingSessionTimestamp) / 1000;
            logger.error(`Can't log in right now, try again in ${timeLeft} seconds!`);

            throw `Can't log in right now, try again in ${timeLeft} seconds!`;
            return;
        }

        // Match make the user into a room filtering based on the user's progress
        const seatReservation: matchMaker.SeatReservation = await matchmakerHelper.matchMakeToRoom("lobby_room", user.progress);

        updateUserForNewSession(user, seatReservation.sessionId);

        // Save the user updates to the database
        await userRepo.flush();

        // Don't include the password in the user object sent back to the client
        const userCopy = { ...user };
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

}

function encryptPassword(password: string) : Promise<string> {
    console.log("Encrypting password: " + password);
    //Encrypt the password
    return bcrypt.hash(password, saltRounds);
}

function compareEncrypted(password: string, hash: string) {
    return bcrypt.compare(password, hash);
}
