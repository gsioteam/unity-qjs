public static class _GenMtConfigure {
    public static bool doNotModify = true;
    public static void Register() {
        if (doNotModify) return;
        object o = null;
        {
            UnityEngine.WaitForSeconds v = (UnityEngine.WaitForSeconds)o;
            v = new UnityEngine.WaitForSeconds((System.Single)o);
            v.Equals((System.Object)o);
            v.GetHashCode();
            v.ToString();
        }
        {
            System.Threading.Thread v = (System.Threading.Thread)o;
            v = new System.Threading.Thread((System.Threading.ThreadStart)o);
            v = new System.Threading.Thread((System.Threading.ThreadStart)o,(System.Int32)o);
            v = new System.Threading.Thread((System.Threading.ParameterizedThreadStart)o);
            v = new System.Threading.Thread((System.Threading.ParameterizedThreadStart)o,(System.Int32)o);
            var p1 = v.CurrentUICulture;
            var p2 = v.CurrentCulture;
            var p3 = System.Threading.Thread.CurrentPrincipal;
            System.Threading.Thread.CurrentPrincipal = (System.Security.Principal.IPrincipal)o;
            var p4 = System.Threading.Thread.CurrentThread;
            var p5 = v.IsThreadPoolThread;
            var p6 = v.IsAlive;
            var p7 = v.IsBackground;
            v.IsBackground = (System.Boolean)o;
            var p8 = v.Name;
            v.Name = (System.String)o;
            var p9 = v.ThreadState;
            var pA = v.ManagedThreadId;
            v.Start();
            v.Start((System.Object)o);
            v.Join((System.TimeSpan)o);
            System.Threading.Thread.Sleep((System.TimeSpan)o);
            System.Threading.Thread.AllocateDataSlot();
            System.Threading.Thread.AllocateNamedDataSlot((System.String)o);
            System.Threading.Thread.GetNamedDataSlot((System.String)o);
            System.Threading.Thread.FreeNamedDataSlot((System.String)o);
            System.Threading.Thread.GetData((System.LocalDataStoreSlot)o);
            System.Threading.Thread.SetData((System.LocalDataStoreSlot)o,(System.Object)o);
            System.Threading.Thread.GetDomain();
            v.Abort();
            v.Abort((System.Object)o);
            System.Threading.Thread.SpinWait((System.Int32)o);
            System.Threading.Thread.BeginCriticalRegion();
            System.Threading.Thread.EndCriticalRegion();
            System.Threading.Thread.BeginThreadAffinity();
            System.Threading.Thread.EndThreadAffinity();
            v.GetApartmentState();
            v.SetApartmentState((System.Threading.ApartmentState)o);
            v.TrySetApartmentState((System.Threading.ApartmentState)o);
            v.GetHashCode();
            v.DisableComObjectEagerCleanup();
            v.Equals((System.Object)o);
            v.ToString();
        }
    }
}
