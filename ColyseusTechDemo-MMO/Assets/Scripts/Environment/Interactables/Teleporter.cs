using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : Interactable
{
    [SerializeField]
    private Teleporter landingTeleporter = null;

    [SerializeField]
    private Transform teleportRoot = null;

    [SerializeField]
    private Transform teleportExit = null;

    [SerializeField]
    private ParticleSystem teleportEffect = null;

    private NetworkedEntity usingEntity;

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

        usingEntity = entity;
        usingEntity.SetIgnoreMovementSync(true);
        
        //Disable the player's controls
        usingEntity.SetMovementEnabled(false);
        StartCoroutine(TransferPlayer(true, () =>
        {
            MoveUserToLandingPad();
        }));
    }

    /// <summary>
    /// Transition the player either on to or out of the teleporter
    /// </summary>
    /// <param name="intoTeleporter"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    private IEnumerator TransferPlayer(bool intoTeleporter, Action onComplete)
    {
        if (!intoTeleporter)
        {
            teleportEffect.Play();
            yield return new WaitForSeconds(teleportEffect.main.duration / 2.0f);
        }
        float t = 0.0f;
        float dur = 0.5f;

        Vector3 exitPos = intoTeleporter ? usingEntity.transform.position : teleportExit.position;
        Quaternion exitRot = intoTeleporter ? usingEntity.transform.rotation : teleportExit.rotation;

        while (t < dur)
        {
            usingEntity.transform.position = Vector3.Lerp(intoTeleporter ? exitPos : teleportRoot.position,
                intoTeleporter ? teleportRoot.position : exitPos, t / dur);

            usingEntity.transform.rotation = Quaternion.Lerp(intoTeleporter ? exitRot : teleportRoot.rotation,
                intoTeleporter ? teleportRoot.rotation : exitRot, t / dur);

            yield return new WaitForEndOfFrame();

            t += Time.deltaTime;
        }

        if (intoTeleporter)
        {
            teleportEffect.Play();
            yield return new WaitForSeconds(teleportEffect.main.duration / 2.0f);
            usingEntity.transform.position = landingTeleporter.teleportRoot.position;
        }
        else
        {
            //Additional short delay
            yield return new WaitForSeconds(0.5f);
        }

        onComplete.Invoke();

        if (intoTeleporter)
        {
            yield return new WaitForSeconds(2.0f);
        }

        teleportEffect.Stop();
    }

    /// <summary>
    /// Call <see cref="ExitTeleporter"/> on the <see cref="landingTeleporter"/>
    /// </summary>
    private void MoveUserToLandingPad()
    {
        landingTeleporter.ExitTeleporter(usingEntity);
    }

    /// <summary>
    /// Have an entity exit this teleporter, called by the teleporter the user entered
    /// </summary>
    /// <param name="entity"></param>
    public void ExitTeleporter(NetworkedEntity entity)
    {

        usingEntity = entity;
        StartCoroutine(TransferPlayer(false, () =>
        {
            usingEntity.SetIgnoreMovementSync(false);
            usingEntity.SetMovementEnabled(true);
        }));
    }
}
