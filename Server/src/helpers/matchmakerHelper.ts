import { matchMaker } from "colyseus";

/**
* Begin the process to matchmake into a room.
* @param room The room name for the user to matchmake into.
* @param progress The room filter representing the grid the user is currently in.
* @returns The seat reservation for the room.
*/
export async function matchMakeToRoom(room: string = "lobby_room", progress: string = "0,0"): Promise<matchMaker.SeatReservation> {
    return await matchMaker.joinOrCreate(room, { progress });
}