const MonoBehaviour = unity("UnityEngine.MonoBehaviour")
const md5 = require('md5');

class npm_demo extends MonoBehaviour
{
    // Start is called before the first frame update
    Start()
    {
        console.log(md5('hello'));
    }

    // Update is called once per frame
    Update()
    {
        
    }
}

export default npm_demo;