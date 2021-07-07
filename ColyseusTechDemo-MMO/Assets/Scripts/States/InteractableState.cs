// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.23
// 

using Colyseus.Schema;

public partial class InteractableState : Schema {
	[Type(0, "string")]
	public string id = default(string);

	[Type(1, "boolean")]
	public bool inUse = default(bool);

	[Type(2, "string")]
	public string interactableType = default(string);

	[Type(3, "number")]
	public float availableTimestamp = default(float);

	[Type(4, "number")]
	public float coinChange = default(float);

	[Type(5, "number")]
	public float useDuration = default(float);
}

