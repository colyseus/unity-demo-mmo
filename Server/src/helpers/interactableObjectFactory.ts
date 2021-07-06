import { InteractableState } from "../rooms/schema/RoomState"

 /**
 * Begin the process to matchmake into a room.
 * @param room The room name for the user to matchmake into.
 * @param progress The room filter representing the grid the user is currently in.
 * @returns The seat reservation for the room.
 */
// export async function matchMakeToRoom(room: string = "lobby_room", progress: string = "0,0"): Promise<matchMaker.SeatReservation> {
//     return await matchMaker.joinOrCreate(room, { progress });
// }

export function getStateForType(type: string) : InteractableState {
    let state : InteractableState = new InteractableState();

    //Any new types need an appropriate constructor in here or they will return empty
    switch(type){
        case("DEFAULT"):
        {
            state.assign({
                coinChange : 0,
                interactableType : type,
                useDuration : 5100.0
            });
            break;
        }
        case("BUTTON_PODIUM"):
        {
            state.assign({
                coinChange : 1,
                interactableType : type,
                useDuration : 10000.0
            });
            break;
        }
        case("COIN_OP"):
        {
            state.assign({
                coinChange : -1,
                interactableType : type,
                useDuration : 5100.0
            });
            break;
        }
        case("TELEPORTER"):
        {
            state.assign({
                coinChange : -2,
                interactableType : type,
                useDuration : 5100.0
            });
            break;
        }
    }

    return state;
}