using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPodium : Interactable
{
    [SerializeField]
    private ParticleSystem[] interactionEffects = null;

    [SerializeField]
    private Material inUseMat;
    [SerializeField]
    private Material availableMat;

    [SerializeField]
    private Renderer buttonRenderer;

    public override void PlayerInRange(NetworkedEntity entity)
    {
        //Only display in range stuff if the local user is the one in range!
        if (entity.isMine)
            base.PlayerInRange(entity);
    }

    public override void PlayerLeftRange(NetworkedEntity entity)
    {
        //Only display in range stuff if the local user is the one in range!
        if (entity.isMine)
            base.PlayerLeftRange(entity);
    }

    public override void OnSuccessfulUse(NetworkedEntity entity)
    {
        base.OnSuccessfulUse(entity);
        //Button podium also has interaction effects, so play them!
        for (int i = 0; i < interactionEffects.Length; ++i)
        {
            interactionEffects[i].Play();
        }
    }

    public override void SetInUse(bool inUse)
    {
        base.SetInUse(inUse);
        //Set the button material to the appropriate one
        buttonRenderer.material = isInUse ? inUseMat : availableMat;
    }
}
