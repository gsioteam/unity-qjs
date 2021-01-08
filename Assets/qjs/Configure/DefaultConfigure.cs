
namespace qjs
{
    public static class DefaultConfigure
    {
        public static bool doNotModify = true;
        public static void Register()
        {
            if (doNotModify) return;
            object o = null;
            {
                UnityEngine.MonoBehaviour v = (UnityEngine.MonoBehaviour)o;
                v = new UnityEngine.MonoBehaviour();
                var p1 = v.tag;
                v.tag = (System.String)o;
                var p2 = v.name;
                v.name = (System.String)o;
                v.IsInvoking();
                v.CancelInvoke();
                v.Invoke((System.String)o, (System.Single)o);
                v.InvokeRepeating((System.String)o, (System.Single)o, (System.Single)o);
                v.CancelInvoke((System.String)o);
                v.IsInvoking((System.String)o);
                v.StartCoroutine((System.String)o);
                v.StartCoroutine((System.String)o, (System.Object)o);
                v.StartCoroutine((System.Collections.IEnumerator)o);
                v.StopCoroutine((System.Collections.IEnumerator)o);
                v.StopCoroutine((UnityEngine.Coroutine)o);
                UnityEngine.MonoBehaviour.print((System.Object)o);
                v.GetComponent((System.Type)o);
                UnityEngine.Component p3 = (UnityEngine.Component)o;
                v.TryGetComponent((System.Type)o, out p3);
                v.GetComponentInChildren((System.Type)o, (System.Boolean)o);
                v.GetComponentInChildren((System.Type)o);
                v.GetComponentsInChildren((System.Type)o, (System.Boolean)o);
                v.GetComponentsInChildren((System.Type)o);
                v.GetComponentInParent((System.Type)o);
                v.GetComponentsInParent((System.Type)o, (System.Boolean)o);
                v.GetComponentsInParent((System.Type)o);
                v.GetComponents((System.Type)o);
                v.GetComponents((System.Type)o, (System.Collections.Generic.List<UnityEngine.Component>)o);
                v.CompareTag((System.String)o);
                v.SendMessageUpwards((System.String)o, (System.Object)o);
                v.SendMessageUpwards((System.String)o);
                v.SendMessageUpwards((System.String)o, (UnityEngine.SendMessageOptions)o);
                v.SendMessage((System.String)o, (System.Object)o);
                v.SendMessage((System.String)o);
                v.SendMessage((System.String)o, (UnityEngine.SendMessageOptions)o);
                v.BroadcastMessage((System.String)o, (System.Object)o);
                v.BroadcastMessage((System.String)o);
                v.BroadcastMessage((System.String)o, (UnityEngine.SendMessageOptions)o);
                v.GetInstanceID();
                v.GetHashCode();
                v.Equals((System.Object)o);
                v.ToString();
            }
            {
                UnityEngine.Transform v = (UnityEngine.Transform)o;
                var p4 = v.position;
                v.position = (UnityEngine.Vector3)o;
                var p5 = v.localPosition;
                v.localPosition = (UnityEngine.Vector3)o;
                var p6 = v.eulerAngles;
                v.eulerAngles = (UnityEngine.Vector3)o;
                var p7 = v.localEulerAngles;
                v.localEulerAngles = (UnityEngine.Vector3)o;
                var p8 = v.right;
                v.right = (UnityEngine.Vector3)o;
                var p9 = v.up;
                v.up = (UnityEngine.Vector3)o;
                var pA = v.forward;
                v.forward = (UnityEngine.Vector3)o;
                var pB = v.rotation;
                v.rotation = (UnityEngine.Quaternion)o;
                var pC = v.localRotation;
                v.localRotation = (UnityEngine.Quaternion)o;
                var pD = v.localScale;
                v.localScale = (UnityEngine.Vector3)o;
                var pE = v.parent;
                v.parent = (UnityEngine.Transform)o;
                var pF = v.worldToLocalMatrix;
                var p10 = v.localToWorldMatrix;
                var p11 = v.root;
                var p12 = v.lossyScale;
                var p13 = v.hierarchyCapacity;
                v.hierarchyCapacity = (System.Int32)o;
                var p14 = v.hierarchyCount;
                var p15 = v.tag;
                v.tag = (System.String)o;
                var p16 = v.name;
                v.name = (System.String)o;
                v.SetParent((UnityEngine.Transform)o);
                v.SetPositionAndRotation((UnityEngine.Vector3)o, (UnityEngine.Quaternion)o);
                v.Translate((UnityEngine.Vector3)o, (UnityEngine.Space)o);
                v.Translate((UnityEngine.Vector3)o);
                v.Translate((System.Single)o, (System.Single)o, (System.Single)o, (UnityEngine.Space)o);
                v.Translate((System.Single)o, (System.Single)o, (System.Single)o);
                v.Translate((UnityEngine.Vector3)o, (UnityEngine.Transform)o);
                v.Translate((System.Single)o, (System.Single)o, (System.Single)o, (UnityEngine.Transform)o);
                v.Rotate((UnityEngine.Vector3)o, (UnityEngine.Space)o);
                v.Rotate((UnityEngine.Vector3)o);
                v.Rotate((System.Single)o, (System.Single)o, (System.Single)o, (UnityEngine.Space)o);
                v.Rotate((System.Single)o, (System.Single)o, (System.Single)o);
                v.Rotate((UnityEngine.Vector3)o, (System.Single)o, (UnityEngine.Space)o);
                v.Rotate((UnityEngine.Vector3)o, (System.Single)o);
                v.RotateAround((UnityEngine.Vector3)o, (UnityEngine.Vector3)o, (System.Single)o);
                v.LookAt((UnityEngine.Transform)o, (UnityEngine.Vector3)o);
                v.LookAt((UnityEngine.Transform)o);
                v.LookAt((UnityEngine.Vector3)o, (UnityEngine.Vector3)o);
                v.LookAt((UnityEngine.Vector3)o);
                v.TransformDirection((UnityEngine.Vector3)o);
                v.TransformDirection((System.Single)o, (System.Single)o, (System.Single)o);
                v.InverseTransformDirection((UnityEngine.Vector3)o);
                v.InverseTransformDirection((System.Single)o, (System.Single)o, (System.Single)o);
                v.TransformVector((UnityEngine.Vector3)o);
                v.TransformVector((System.Single)o, (System.Single)o, (System.Single)o);
                v.InverseTransformVector((UnityEngine.Vector3)o);
                v.InverseTransformVector((System.Single)o, (System.Single)o, (System.Single)o);
                v.TransformPoint((UnityEngine.Vector3)o);
                v.TransformPoint((System.Single)o, (System.Single)o, (System.Single)o);
                v.InverseTransformPoint((UnityEngine.Vector3)o);
                v.InverseTransformPoint((System.Single)o, (System.Single)o, (System.Single)o);
                v.Find((System.String)o);
                v.GetEnumerator();
                v.GetComponent((System.Type)o);
                UnityEngine.Component p17 = (UnityEngine.Component)o;
                v.TryGetComponent((System.Type)o, out p17);
                v.GetComponentInChildren((System.Type)o, (System.Boolean)o);
                v.GetComponentInChildren((System.Type)o);
                v.GetComponentsInChildren((System.Type)o, (System.Boolean)o);
                v.GetComponentsInChildren((System.Type)o);
                v.GetComponentInParent((System.Type)o);
                v.GetComponentsInParent((System.Type)o, (System.Boolean)o);
                v.GetComponentsInParent((System.Type)o);
                v.GetComponents((System.Type)o);
                v.GetComponents((System.Type)o, (System.Collections.Generic.List<UnityEngine.Component>)o);
                v.CompareTag((System.String)o);
                v.SendMessageUpwards((System.String)o, (System.Object)o);
                v.SendMessageUpwards((System.String)o);
                v.SendMessageUpwards((System.String)o, (UnityEngine.SendMessageOptions)o);
                v.SendMessage((System.String)o, (System.Object)o);
                v.SendMessage((System.String)o);
                v.SendMessage((System.String)o, (UnityEngine.SendMessageOptions)o);
                v.BroadcastMessage((System.String)o, (System.Object)o);
                v.BroadcastMessage((System.String)o);
                v.BroadcastMessage((System.String)o, (UnityEngine.SendMessageOptions)o);
                v.GetInstanceID();
                v.GetHashCode();
                v.Equals((System.Object)o);
                v.ToString();
            }
        }
    }

}