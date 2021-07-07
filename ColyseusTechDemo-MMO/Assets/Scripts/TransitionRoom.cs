using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionRoom : MonoBehaviour
{
    [SerializeField]
    private Animator entryDoorAnimator = null;

    [SerializeField]
    private Animator exitDoorAnimator = null;

    [SerializeField]
    private float doorAnimTime = 1;

    [SerializeField]
    private ParticleSystem[] decontaminateFX = null;

    public ParticleSystemForceField forceField;

    public Transform entryCameraTarget = null;
    public Transform exitCameraTarget = null;

    private const string DoorBlend = "DoorBlend";

    private Coroutine _entryDoorRoutine = null;
    private Coroutine _exitDoorRoutine = null;

    private Dictionary<string, ParticleSystemForceField> _forceFields =
        new Dictionary<string, ParticleSystemForceField>();

    private void Start()
    {
        forceField.gameObject.SetActive(false);
    }

    public void AddForceField(NetworkedEntity entity)
    {
        if (_forceFields.ContainsKey(entity.Id) == false)
        {
            ParticleSystemForceField ff = Instantiate(forceField, entity.transform.position, Quaternion.identity,
                forceField.transform.parent);

            ff.gameObject.SetActive(true);

            _forceFields.Add(entity.Id, ff);
        }
    }

    public void RemoveForceField(string entityId)
    {
        if (_forceFields.ContainsKey(entityId))
        {
            ParticleSystemForceField ff = _forceFields[entityId];

            _forceFields.Remove(entityId);

            Destroy(ff.gameObject);
        }
    }

    private void Update()
    {
        UpdateForceFields();
    }

    private void UpdateForceFields()
    {
        List<string> keys = new List<string>(_forceFields.Keys);

        for (int i = keys.Count - 1; i >= 0; i--)
        {
            NetworkedEntity entity = NetworkedEntityFactory.Instance.GetEntityByID(keys[i]);

            if (!entity)
            {
                RemoveForceField(keys[i]);
                continue;
            }

            _forceFields[keys[i]].transform.position = entity.transform.position;
        }
    }

    public void OpenDoor(bool isEntry, Action onComplete)
    {
        if (isEntry)
        {
            if (_entryDoorRoutine != null)
            {
                StopCoroutine(_entryDoorRoutine);
            }

            _entryDoorRoutine = StartCoroutine(AnimateDoor(isEntry, true, onComplete));
        }
        else
        {
            if (_exitDoorRoutine != null)
            {
                StopCoroutine(_exitDoorRoutine);
            }

            _exitDoorRoutine = StartCoroutine(AnimateDoor(isEntry, true, onComplete));
        }
    }

    public void CloseDoor(bool isEntry, Action onComplete)
    {
        if (isEntry)
        {
            if (_entryDoorRoutine != null)
            {
                StopCoroutine(_entryDoorRoutine);
            }

            _entryDoorRoutine = StartCoroutine(AnimateDoor(isEntry, false, onComplete));
        }
        else
        {
            if (_exitDoorRoutine != null)
            {
                StopCoroutine(_exitDoorRoutine);
            }

            _exitDoorRoutine = StartCoroutine(AnimateDoor(isEntry, false, onComplete));
        }

    }

    public void RunDecontamination(bool isEntry, Action onComplete)
    {
        StartCoroutine(Decontaminate(isEntry, onComplete));
    }

    public void StopDecontamination()
    {
        forceField.gameObject.SetActive(false);

        for (int i = 0; i < decontaminateFX.Length; i++)
        {
            decontaminateFX[i].Stop();
        }
    }

    public void SetForceFieldPosition(Vector3 position)
    {
        forceField.transform.position = position;
    }

    private IEnumerator Decontaminate(bool isEntry, Action onComplete)
    {
        for (int i = 0; i < decontaminateFX.Length; i++)
        {
            decontaminateFX[i].Play();
        }

        yield return new WaitForSeconds(3.0f);

        OpenDoor(isEntry, onComplete);
    }

    private IEnumerator AnimateDoor(bool isEntry, bool open, Action onComplete)
    {
        Animator anim = isEntry ? entryDoorAnimator : exitDoorAnimator;

        float targetValue = open ? 0 : 1;
        float blendValue;

        while ((blendValue = anim.GetFloat(DoorBlend)) != targetValue)
        {
            blendValue = Mathf.MoveTowards(blendValue, targetValue, Time.deltaTime / Mathf.Max(doorAnimTime, 0.001f));

            anim.SetFloat(DoorBlend, blendValue);

            yield return null;
        }

        onComplete?.Invoke();
    }
}
