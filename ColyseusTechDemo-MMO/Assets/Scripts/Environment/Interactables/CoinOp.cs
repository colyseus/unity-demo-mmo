using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinOp : Interactable
{
    [SerializeField]
    private Transform playerRoot = null;

    [SerializeField]
    private Animation rockingAnimation = null;

    private NetworkedEntity ridingEntity;
    private Transform originalParentTransform;
    private Vector3 originalEntityPos;
    private Quaternion originaEntityRot;

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

        //Snap player into root, begin "riding"
        ridingEntity = entity;
        ridingEntity.SetIgnoreMovementSync(true);
        originalParentTransform = ridingEntity.transform.parent;
        originalEntityPos = ridingEntity.transform.position;
        originaEntityRot = ridingEntity.transform.rotation;

        //Disable the player's controls
        ridingEntity.SetMovementEnabled(false);
        StartCoroutine(TransferPlayer(true, () =>
        {
            rockingAnimation.Play();
        }));
    }

    /// <summary>
    /// Transition the player either on to or off of the seat
    /// </summary>
    /// <param name="ontoSeat"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    private IEnumerator TransferPlayer(bool ontoSeat, Action onComplete)
    {
        float t = 0.0f;
        float dur = 0.5f;

        while (t < dur)
        {
            ridingEntity.transform.position = Vector3.Lerp(ontoSeat ? originalEntityPos : playerRoot.position,
                ontoSeat ? playerRoot.position : originalEntityPos, t / dur);

            ridingEntity.transform.rotation = Quaternion.Lerp(ontoSeat ? originaEntityRot : playerRoot.rotation,
                ontoSeat ? playerRoot.rotation : originaEntityRot, t / dur);

            yield return new WaitForEndOfFrame();

            t += Time.deltaTime;
        }

        ridingEntity.transform.SetParent(ontoSeat ? playerRoot : originalParentTransform, true);

        //Additional short delay
        yield return new WaitForSeconds(0.5f);

        onComplete.Invoke();
    }

    protected override void OnInteractableReset()
    {
        base.OnInteractableReset();

        //Stop animating and move the player off of the seat
        rockingAnimation.Stop();
        StartCoroutine(TransferPlayer(false, () =>
        {
            //Restore the user's controls
            ridingEntity.SetIgnoreMovementSync(false);
            ridingEntity.SetMovementEnabled(true);
        }));
    }
}
