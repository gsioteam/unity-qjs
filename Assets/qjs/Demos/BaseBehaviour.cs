using System.Collections;
using System.Collections.Generic;
using qjs;
using UnityEngine;

public class BaseBehaviour : MonoBehaviour
{
    public Container js;

    protected readonly static JSAtom AwakeAtom = Container.GetAtom("Awake");
    protected void Awake()
    {
        if (isActiveAndEnabled && !js.Value.IsNull) js.Value.Call(AwakeAtom);
    }

    protected readonly static JSAtom StartAtom = Container.GetAtom("Start");
    protected void Start()
    {
        if (isActiveAndEnabled && !js.Value.IsNull) js.Value.Call(StartAtom);
    }

    protected readonly static JSAtom UpdateAtom = Container.GetAtom("Update");
    protected void Update()
    {
        if (isActiveAndEnabled && !js.Value.IsNull) js.Value.Call(UpdateAtom);
    }

    protected readonly static JSAtom FixedUpdateAtom = Container.GetAtom("FixedUpdate");
    protected void FixedUpdate()
    {
        if (isActiveAndEnabled && !js.Value.IsNull) js.Value.Call(FixedUpdateAtom);
    }

    protected readonly static JSAtom OnDestroyAtom = Container.GetAtom("OnDestroy");
    protected void OnDestroy()
    {
        if (isActiveAndEnabled && !js.Value.IsNull) js.Value.Call(OnDestroyAtom);
    }

    protected readonly static JSAtom OnEnableAtom = Container.GetAtom("OnEnable");
    protected void OnEnable()
    {
        if (isActiveAndEnabled && !js.Value.IsNull) js.Value.Call(OnEnableAtom);
    }

    protected readonly static JSAtom OnDisableAtom = Container.GetAtom("OnDisable");
    protected void OnDisable()
    {
        if (isActiveAndEnabled && !js.Value.IsNull) js.Value.Call(OnDisableAtom);
    }

    protected readonly static JSAtom OnApplicationQuitAtom = Container.GetAtom("OnApplicationQuit");
    protected void OnApplicationQuit()
    {
        if (isActiveAndEnabled && !js.Value.IsNull) js.Value.Call(OnApplicationQuitAtom);
    }
}
