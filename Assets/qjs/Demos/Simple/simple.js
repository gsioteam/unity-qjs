const MonoBehaviour = unity("UnityEngine.MonoBehaviour");
const FieldType = unity("qjs.FieldType");
const Vector3 = unity("UnityEngine.Vector3");
const Camera = unity("UnityEngine.Camera");

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
A.field("test5", FieldType.Object, null, Camera);
A.field("test6", FieldType.Array, null, MonoBehaviour);

export default A;