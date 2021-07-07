// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.23
// 

using Colyseus.Schema;

public partial class ChatMessage : Schema {
	[Type(0, "string")]
	public string senderID = default(string);

	[Type(1, "string")]
	public string message = default(string);

	[Type(2, "number")]
	public float timestamp = default(float);
}

