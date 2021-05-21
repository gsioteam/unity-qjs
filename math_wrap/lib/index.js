const mat = require('gl-matrix');

function makeType(meta, is_vec, unity_name, init) {
    const unity_type = unity(unity_name);
    function type() {
        if (this instanceof type) {
            this.m = meta.create();
            let argv;
            if (arguments.length > 0 && typeof(arguments[0].length) != 'undefined') {
                argv = arguments[0];
            } else {
                argv = arguments;
            }
            for (let i = 0, t = Math.min(this.m.length, argv.length); i < t; ++i) {
                this.m[i] = argv[i];
            }
        } else {
            return new type(...arguments);
        }
    };
    let m = meta.create();
    let prop = {};
    function define(i) {
        prop[i] = {
            get: function() {
                return this.m[i];
            },
            set: function (v) {
                this.m[i] = v;
            }
        };
    }
    for (let i = 0, t = m.length; i < t; ++i) {
        define(i);
    }
    Object.defineProperties(type.prototype, prop);
    let operators = {};
    let operators_set = [operators];
    for (let name in meta) {
        let func = meta[name];
        type.prototype[name] = function() {
            func(this.m, this.m, ...arguments);
            return this;
        };
        switch (name){
            case 'add': {
                operators['+'] = function(a, b) {
                    let obj = new type();
                    func(obj.m, a.m, b.m);
                    return obj;
                }
                break;
            }
            case 'subtract': {
                operators['-'] = function (a, b) {
                    let obj = new type();
                    func(obj.m, a.m, b.m);
                    return obj;
                }
                break;
            }
            case 'multiply': {
                operators['*'] = function (a, b) {
                    let obj = new type();
                    func(obj.m, a.m, b.m);
                    return obj;
                }
                break;
            }
            case 'divide': {
                operators['/'] = function (a, b) {
                    let obj = new type();
                    func(obj.m, a.m, b.m);
                    return obj;
                }
                break;
            }
            case 'negate': {
                operators['neg'] = function (a) {
                    let obj = new type();
                    func(obj.m, a.m);
                    return obj;
                }
                break;
            }
            case 'scale': {
                if (is_vec) {
                    operators_set.push({
                        left: Number,
                        '*'(a, b) {
                            let obj = new type();
                            func(obj.m, b.m, a);
                            return obj;
                        }
                    });
                    operators_set.push({
                        right: Number,
                        '*'(b, a) {
                            let obj = new type();
                            func(obj.m, b.m, a);
                            return obj;
                        }
                    });
                }
                break;
            }
            case 'multiplyScalar': {
                if (!is_vec) {
                    operators_set.push({
                        left: Number,
                        '*'(a, b) {
                            let obj = new type();
                            func(obj.m, b.m, a);
                            return obj;
                        }
                    });
                    operators_set.push({
                        right: Number,
                        '*'(b, a) {
                            let obj = new type();
                            func(obj.m, b.m, a);
                            return obj;
                        }
                    });
                }
            }
        }
    }
    if (init) {
        init(unity_type, operators_set);
    }

    type.prototype[Symbol.operatorSet] = Operators.create(...operators_set);
    type.prototype.toString = function() {
        return `(${this.m.join(',')})`;
    };
    type.prototype.toUnity = function() {
        return new unity_type(...this.m);
    };
    return type;
}

globalThis.vec2 = makeType(mat.vec2, true, 'UnityEngine.Vector2', function (unity_type, operators_set) {
    operators_set.push({
        left: unity_type,
        '+'(a, b) {
            return vec2(a.x, a.y) + b;
        },
        '-'(a, b) {
            return vec2(a.x, a.y) - b;
        }
    });
    operators_set.push({
        right: unity_type,
        '+'(b, a) {
            return b + vec2(a.x, a.y);
        },
        '-'(b, a) {
            return b - vec2(a.x, a.y);
        }
    });
});
globalThis.vec3 = makeType(mat.vec3, true, 'UnityEngine.Vector3', function (unity_type, operators_set) {
    operators_set.push({
        left: unity_type,
        '+'(a, b) {
            return vec3(a.x, a.y, a.z) + b;
        },
        '-'(a, b) {
            return vec3(a.x, a.y, a.z) - b;
        }
    });
    operators_set.push({
        right: unity_type,
        '+'(b, a) {
            return b + vec3(a.x, a.y, a.z);
        },
        '-'(b, a) {
            return b - vec3(a.x, a.y, a.z);
        }
    });
});
globalThis.vec4 = makeType(mat.vec4, true, 'UnityEngine.Vector4', function (unity_type, operators_set) {
    operators_set.push({
        left: unity_type,
        '+'(a, b) {
            return vec4(a.x, a.y, a.z, a.w) + b;
        },
        '-'(a, b) {
            return vec4(a.x, a.y, a.z, a.w) - b;
        }
    });
    operators_set.push({
        right: unity_type,
        '+'(b, a) {
            return b + vec4(a.x, a.y, a.z, a.w);
        },
        '-'(b, a) {
            return b - vec4(a.x, a.y, a.z, a.w);
        }
    });
});
globalThis.mat4 = makeType(mat.mat4, false, 'UnityEngine.Matrix4x4');
globalThis.quat = makeType(mat.quat, false, 'UnityEngine.Quaternion');

const EventTarget = require('@jsantell/event-target');

const PRIVATE = Symbol('Worker');

class Worker extends EventTarget {
    
    constructor(path) {
        super();
        this[PRIVATE] = _createWorker(path);
        this[PRIVATE].onGetMessage = (message)=>{
            this.dispatchEvent('message', JSON.parse(message));
        };
    }

    postMessage(message) {
        if (this[PRIVATE] == null) {
            return;
        }
        let data = {data: message, type: 'message'};
        this[PRIVATE].postMessage(JSON.stringify(data));
    }

    terminate() {
        this[PRIVATE].terminate();
        this[PRIVATE] = null;
    }
}

globalThis.Worker = Worker;