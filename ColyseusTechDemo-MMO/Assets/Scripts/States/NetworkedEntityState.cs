// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.23
// 

using Colyseus.Schema;

public partial class NetworkedEntityState : Schema {
	[Type(0, "string")]
	public string id = default(string);

	[Type(1, "string")]
	public string chatID = default(string);

	[Type(2, "number")]
	public float xPos = default(float);

	[Type(3, "number")]
	public float yPos = default(float);

	[Type(4, "number")]
	public float zPos = default(float);

	[Type(5, "number")]
	public float xRot = default(float);

	[Type(6, "number")]
	public float yRot = default(float);

	[Type(7, "number")]
	public float zRot = default(float);

	[Type(8, "number")]
	public float wRot = default(float);

	[Type(9, "ref", typeof(AvatarState))]
	public AvatarState avatar = new AvatarState();

	[Type(10, "number")]
	public float coins = default(float);

	[Type(11, "number")]
	public float timestamp = default(float);

	[Type(12, "string")]
	public string username = default(string);
}

