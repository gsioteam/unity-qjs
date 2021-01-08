onmessage = function (e) {
    console.log("in worker : " + JSON.stringify(e));
}

postMessage("message from worker");