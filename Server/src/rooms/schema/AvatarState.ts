import { Schema, type } from "@colyseus/schema";

export class AvatarState extends Schema {
    @type("string") skinColor: string = "default";
    @type("string") shirtColor: string = "default";
    @type("string") pantsColor: string = "default";
    @type("string") hatColor: string = "default";
    @type("number") hatChoice: number = 1;
  }