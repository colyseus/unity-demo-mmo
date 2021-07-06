/// <summary>
/// Object to represent <see cref="UnityEngine.Vector3"/> across messages sent to the room on the server/>
/// </summary>
public class Vector3Obj
{
    public float x;
    public float y;
    public float z;

    public Vector3Obj(float x = 0, float y = 0, float z = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

/// <summary>
/// Object to represent <see cref="UnityEngine.Vector2"/> across messages sent to the room on the server/>
/// </summary>
public class Vector2Obj
{
    public float x;
    public float y;

    public Vector2Obj(float x = 0, float y = 0)
    {
        this.x = x;
        this.y = y;
    }
}
