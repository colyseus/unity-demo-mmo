// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.23
// 

using Colyseus.Schema;

public partial class AvatarState : Schema {
	[Type(0, "string")]
	public string skinColor = default(string);

	[Type(1, "string")]
	public string shirtColor = default(string);

	[Type(2, "string")]
	public string pantsColor = default(string);

	[Type(3, "string")]
	public string hatColor = default(string);

	[Type(4, "number")]
	public float hatChoice = default(float);
}

