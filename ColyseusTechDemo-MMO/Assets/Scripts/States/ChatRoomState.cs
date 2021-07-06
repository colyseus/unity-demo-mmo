// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.23
// 

using Colyseus.Schema;

public partial class ChatRoomState : Schema {
	[Type(0, "map", typeof(MapSchema<ChatQueue>))]
	public MapSchema<ChatQueue> chatQueue = new MapSchema<ChatQueue>();
}

