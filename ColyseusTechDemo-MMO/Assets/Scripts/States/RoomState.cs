// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.23
// 

using Colyseus.Schema;

public partial class RoomState : Schema {
	[Type(0, "map", typeof(MapSchema<NetworkedEntityState>))]
	public MapSchema<NetworkedEntityState> networkedUsers = new MapSchema<NetworkedEntityState>();

	[Type(1, "map", typeof(MapSchema<InteractableState>))]
	public MapSchema<InteractableState> interactableItems = new MapSchema<InteractableState>();

	[Type(2, "number")]
	public float serverTime = default(float);
}

