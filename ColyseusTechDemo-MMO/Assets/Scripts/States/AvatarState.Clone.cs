using Colyseus.Schema;
using UnityEngine.Networking;

public partial class AvatarState : Schema {
    public AvatarState Clone()
    {
        return new AvatarState()
        {
            skinColor = skinColor, shirtColor = shirtColor, pantsColor = pantsColor, hatChoice = hatChoice,
            hatColor = hatColor
        };
    }

    public object[] ToNetSendObjects()
    {
        return new object[] { skinColor, shirtColor, pantsColor, hatColor, hatChoice};
    }
}

