using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace qjs
{
    public class BaseBehaviour : QuickBehaviour
    {
        protected readonly static JSAtom AwakeAtom = GetAtom("Awake");
        protected void Awake()
        {
            if (isActiveAndEnabled && !Value.IsNull) Value.Call(AwakeAtom);
        }

        protected readonly static JSAtom StartAtom = GetAtom("Start");
        protected void Start()
        {
            if (isActiveAndEnabled && !Value.IsNull) Value.Call(StartAtom);
        }

        protected readonly static JSAtom UpdateAtom = GetAtom("Update");
        protected void Update()
        {
            if (isActiveAndEnabled && !Value.IsNull) Value.Call(UpdateAtom);
        }

        protected readonly static JSAtom FixedUpdateAtom = GetAtom("FixedUpdate");
        protected void FixedUpdate()
        {
            if (isActiveAndEnabled && !Value.IsNull) Value.Call(FixedUpdateAtom);
        }

        protected readonly static JSAtom OnDestroyAtom = GetAtom("OnDestroy");
        protected void OnDestroy()
        {
            if (isActiveAndEnabled && !Value.IsNull) Value.Call(OnDestroyAtom);
        }

        protected readonly static JSAtom OnEnableAtom = GetAtom("OnEnable");
        protected void OnEnable()
        {
            if (isActiveAndEnabled && !Value.IsNull) Value.Call(OnEnableAtom);
        }

        protected readonly static JSAtom OnDisableAtom = GetAtom("OnDisable");
        protected void OnDisable()
        {
            if (isActiveAndEnabled && !Value.IsNull) Value.Call(OnDisableAtom);
        }

        protected readonly static JSAtom OnApplicationQuitAtom = GetAtom("OnApplicationQuit");
        protected void OnApplicationQuit()
        {
            if (isActiveAndEnabled && !Value.IsNull) Value.Call(OnApplicationQuitAtom);
        }

    }

}