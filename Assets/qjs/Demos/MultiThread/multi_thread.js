const MonoBehaviour = unity("UnityEngine.MonoBehaviour")
const WaitForSeconds = unity("UnityEngine.WaitForSeconds");
const Thread = unity("System.Threading.Thread");

class multi_thread extends MonoBehaviour
{
    // Start is called before the first frame update
    async Start()
    {
        // before multithread have test coroutine
        console.log("before coroutine");
        await new WaitForSeconds(3);
        console.log("after coroutine");

        // test thread
        let thread = new Thread(function () {
            // this code is running in subthread, but remember that
            // js engine in the same context will lock the thread.
            // this is not the real multi thread. If you want real
            // multi thread, use Worker instead.
            console.log("In " + Thread.CurrentThread.Name);
        });
        thread.Name = "SubThread";
        thread.Start();

        // test Worker
        let worker = new Worker("worker.js");
        worker.onmessage = function (e) {
            console.log(e.data);
        };
        worker.postMessage(["hello", "worker"]);
    }

    // Update is called once per frame
    Update()
    {
        
    }
}

export default multi_thread;