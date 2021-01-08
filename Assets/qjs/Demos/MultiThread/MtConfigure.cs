using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MtConfigure : qjs.Configure
{
    protected override Action _typeRegister => _GenMtConfigure.Register;
    public override void OnRegisterClass(Action<Type, HashSet<string>> RegisterClass)
    {
        RegisterClass(typeof(WaitForSeconds), null);
        RegisterClass(typeof(Thread), new HashSet<string> { "CurrentContext" });
    }
}
