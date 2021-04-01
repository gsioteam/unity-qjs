const MonoBehaviour = unity("UnityEngine.MonoBehaviour");
const FieldType = unity("qjs.FieldType");
const Vector3 = unity("UnityEngine.Vector3");

class A extends MonoBehaviour {
    constructor() {
        super(...arguments);
    }

    Start() {
        this.transform.position = this.test4;
    }

    Update() {
        this.transform.position += vec3(0.01, 0, 0);
    }
};
A.field("test4", FieldType.Vector3, vec3(1, 2));

export default A;