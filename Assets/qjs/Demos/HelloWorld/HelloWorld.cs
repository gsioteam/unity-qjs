using UnityEngine;
using qjs;
using System;

class A
{
    public int arg = 5;
    public int arg2 = 10;

    public int text
    {
        get
        {
            return 8;
        }
    }

    public void test2()
    {
    }

    public static void sayHelloWorld()
    {
        Debug.Log("Hello World!");
    }
}

public class HelloWorld : MonoBehaviour
{

    delegate void TestAction(int i);

    private void OnEnable()
    {
    }

    void Start()
    {
        qjs.QuickJS quickJS = new qjs.QuickJS();
        // Register types
        quickJS.RegisterClass<A>();
        quickJS.RegisterClass<Vector3>();

        // Get c# types in JS, via full class name.
        quickJS.Eval("const A = unity('A'); const Vector3 = unity('UnityEngine.Vector3');");
        DateTime dateTime;

        dateTime = DateTime.Now;
        {
            JSValue ret = quickJS.Eval("let a = new A(); a;");
            // Call by atom will faster a little.
            JSAtom test2 = quickJS.NewAtom("test2");
            for (int i = 0; i < 10000; ++i)
            {
                ret.Call(test2);
            }
        }
        Debug.Log("Call js: " + (DateTime.Now - dateTime));

        dateTime = DateTime.Now;
        {
            // Call c# method from JS.
            quickJS.Eval("a = new A(); for (var i = 0; i < 10000; ++i) { a.test2(); } ");
        }
        Debug.Log("Call c#: " + (DateTime.Now - dateTime));

        dateTime = DateTime.Now;
        {
            // Caculate via c#  
            quickJS.Eval("a = new Vector3(1,2,3); let b = new Vector3(1,2,3); for (var i = 0; i < 10000; ++i) { let c = a + b; } ");
        }
        Debug.Log("JS bind: " + (DateTime.Now - dateTime));

        dateTime = DateTime.Now;
        {
            // Caculate via js, and vec3 can be used same as Vector3
            quickJS.Eval("a = vec3(1,2,3);  b = vec3(1,2,3); for (var i = 0; i < 10000; ++i) { let c = a + b; } ");
        }
        Debug.Log("JS: " + (DateTime.Now - dateTime));

        quickJS.Eval("A.sayHelloWorld();");

        quickJS.Destroy();

    }

    void Update()
    {
    }

    private void OnApplicationQuit()
    {
    }
}
