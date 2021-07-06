// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.23
// 

using Colyseus.Schema;

public partial class ChatQueue : Schema {
	[Type(0, "array", typeof(ArraySchema<ChatMessage>))]
	public ArraySchema<ChatMessage> chatMessages = new ArraySchema<ChatMessage>();
}

