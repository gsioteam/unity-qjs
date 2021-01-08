const MonoBehaviour = unity("UnityEngine.MonoBehaviour");
const FieldType = unity("qjs.FieldType");
const Vector3 = unity("UnityEngine.Vector3");

class A extends MonoBehaviour {
    constructor() {
        super(...arguments);
        this.addField("test4", FieldType.Vector3, vec3(1, 2));
    }

    Start() {
        this.transform.position = this.test4;
    }

    Update() {
        var vec = this.transform.position;
        vec += vec3(0.01, 0, 0);
        this.transform.position = vec;
    }
};

export default A;