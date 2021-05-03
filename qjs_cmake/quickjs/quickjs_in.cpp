//
//  quickjs_in.c
//  quickjs_osx
//
//  Created by gen on 12/4/20.
//  Copyright © 2020 nioqio. All rights reserved.
//

#include <stdio.h>
#include <stdarg.h>
#include <map>
#include <string>
#include <vector>
#include <list>
#include <stack>
#include <set>
#include <thread>
#include <pthread.h>
#include <sstream>
#include "quickjs_ext.h"
#include "quickjs-libc.h"
#include "quickjs_in.h"
#include <memory.h>
#include <sys/time.h>
#include <cstring>

#define TYPE_NEW_OBJECT     0
#define TYPE_INVOKE         1
#define TYPE_GET_FIELD      2
#define TYPE_SET_FIELD      3
#define TYPE_GET_PROPERTY   4
#define TYPE_SET_PROPERTY   5
#define TYPE_STATIC_INVOKE  6
#define TYPE_STATIC_GET     7
#define TYPE_STATIC_SET     8
// dep
#define TYPE_ADD_DFIELD     9
#define TYPE_GET_DFIELD     10
#define TYPE_SET_DFIELD     11
#define TYPE_GENERIC_CLASS  12
#define TYPE_ARRAY_CLASS    13
#define TYPE_NEW_ARRAY      14
#define TYPE_CALL_DELEGATE  15
#define TYPE_STATIC_PGET    16
#define TYPE_STATIC_PSET    17
#define TYPE_CREATE_WORKER  18

using namespace std;
namespace qjs {
    struct QJS_Context;
    struct QJS_Class;
    struct QJS_Value;
}

using namespace qjs;

extern "C" {
void QJS_ClearResults(QJS_Context *ctx);
}

namespace qjs {

struct QJS_Instance;
struct QJS_Class;

void QJS_PrintError(QJS_Context *ctx, JSValue value, const char *prefix = NULL);

#define ITEM_TYPE_INT       0
#define ITEM_TYPE_LONG      1
#define ITEM_TYPE_DOUBLE    2
#define ITEM_TYPE_BOOL      3
#define ITEM_TYPE_STRING    4
#define ITEM_TYPE_OBJECT    5
#define ITEM_TYPE_CLASS     6
#define ITEM_TYPE_VALUE     7
#define ITEM_TYPE_PROMISE   8
#define ITEM_TYPE_NULL      9
// instance id
#define ITEM_TYPE_JS_OBJECT 10
// class id
#define ITEM_TYPE_JS_CLASS  11
// raw ptr
#define ITEM_TYPE_JS_VALUE  12
// raw ptr
#define ITEM_TYPE_JS_STRING 13

struct QJS_Item {
    uint16_t type = ITEM_TYPE_NULL;
    union {
        int32_t i;
        int64_t l;
        double d;
        bool b;
        void *p;
        const char *s;
    };
    
    void set(int value) {
        type = ITEM_TYPE_INT;
        i = value;
    }
    void set(int64_t value) {
        type = ITEM_TYPE_LONG;
        l = value;
    }
    void set(double value) {
        type = ITEM_TYPE_DOUBLE;
        d = value;
    }
    void set(bool value) {
        type = ITEM_TYPE_BOOL;
        b = value;
    }
    void set(const char *value) {
        type = ITEM_TYPE_STRING;
        s = value;
    }
    void setInstance(QJS_Instance *value);
    void setClass(QJS_Class *value);
    void setValue(JSValue value);
    
    void set(QJS_Context *ctx, JSValue value);
    void setNull() {
        type = ITEM_TYPE_NULL;
    }
    
    JSValue toValue(QJS_Context *ctx);
};

#if defined(__linux__) || defined(__APPLE__)
static int64_t get_time_ms(void)
{
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (uint64_t)ts.tv_sec * 1000 + (ts.tv_nsec / 1000000);
}
#else
/* more portable, but does not work if the date is updated */
static int64_t get_time_ms(void)
{
    struct timeval tv;
    gettimeofday(&tv, NULL);
    return (int64_t)tv.tv_sec * 1000 + (tv.tv_usec / 1000);
}
#endif

void QJS_PrintError(QJS_Context *ctx, JSValue value, const char *prefix);

map<string, string> op_map = {
    {"op_Addition", "+"},
    {"op_Subtraction", "-"},
    {"op_Multiply", "*"},
    {"op_Division", "/"},
    {"op_LogicalAnd", "&"},
    {"op_LogicalOr", "|"},
    {"op_Equality", "=="},
    {"op_LessThan", "<"},
    {"op_UnaryNegation", "neg"},
    {"op_UnaryPlus", "pos"}
};

    #define QJS_Log(rt, ...) QJS_Print(rt, 0, __VA_ARGS__)
    #define QJS_Warning(rt, ...) QJS_Print(rt, 1, __VA_ARGS__)
    #define QJS_Error(rt, ...) QJS_Print(rt, 2, __VA_ARGS__)

typedef void(*QJS_PrintHandler)(int type, const char *str);
typedef void(*QJS_ActionHandler)(int handler, QJS_Context *ctx, int class_id, int *ids, int length, int type, int argc);
typedef void(*QJS_DeleteObjectHandler)(int handler, int id);
typedef char *(*QJS_ModuleNameHandler)(int handler, QJS_Context *ctx, const char *base_name, const char *name);
typedef void(*QJS_LoadModuleHandler)(int handler, QJS_Context *ctx, const char *name);
typedef void(*QJS_LoadClassHandler)(int handler, QJS_Context *ctx, const char *name);

typedef struct QJS_Handlers {
    int handler;
    QJS_PrintHandler print;
    QJS_ActionHandler action;
    QJS_DeleteObjectHandler delete_object;
    QJS_LoadModuleHandler load_module;
    QJS_LoadClassHandler load_class;
    QJS_ModuleNameHandler module_name;
} QJS_Handlers;

struct QJS_Runtime {
    JSRuntime   *runtime = nullptr;
    QJS_Handlers handlers;
    
    QJS_Item *arguments;
    QJS_Item *results;
    
    void action(QJS_Context *ctx, int class_id, int *ids, int length, int type, int argc) {
        handlers.action(handlers.handler, ctx, class_id, ids, length, type, argc);
    }
    
    void loadModule(QJS_Context *ctx, const char *name) {
        handlers.load_module(handlers.handler, ctx, name);
    }
    
    void loadClass(QJS_Context *ctx, const char *name) {
        handlers.load_class(handlers.handler, ctx, name);
    }
    char *moduleName(QJS_Context *ctx, const char *base_name, const char *name) {
        return handlers.module_name(handlers.handler, ctx, base_name, name);
    }
};

void QJS_Print(QJS_Runtime *ptr, int type, const char *format, ...);

struct QJS_Instance {
    int sharp_id;
    JSValue value;
    QJS_Class *clazz;
    
    static void finalizer(JSRuntime *rt, JSValue value) {
        QJS_Runtime *runtime = (QJS_Runtime *)JS_GetRuntimeOpaque(rt);
        QJS_Instance *ins = (QJS_Instance *)JS_GetOpaque3(value);
        if (ins) {
            runtime->handlers.delete_object(runtime->handlers.handler, ins->sharp_id);
            delete ins;
        }
    }
    
    static void functionFinalizer(JSRuntime *rt, void *opaque) {
        QJS_Runtime *runtime = (QJS_Runtime *)JS_GetRuntimeOpaque(rt);
        QJS_Instance *ins = (QJS_Instance *)opaque;
        runtime->handlers.delete_object(runtime->handlers.handler, ins->sharp_id);
        delete ins;
    }
    
    static JSValue functionCallback(JSContext *context, JSValueConst this_val,
                                    int argc, JSValueConst *argv, int magic, JSValue *func_data);
};

struct QJS_Value {
    JSValue value;
    bool free;
};

#define PROMISE_STATE_DEPEND    0
#define PROMISE_STATE_SUCCESS   1
#define PROMISE_STATE_FAILED    2

struct QJS_Promise {
    JSValue value = JS_UNDEFINED;
    JSValue success = JS_UNDEFINED;
    JSValue failed = JS_UNDEFINED;
    int state = 0;
    
    void free(JSContext *ctx) {
        JS_FreeValue(ctx, value);
        JS_FreeValue(ctx, success);
        JS_FreeValue(ctx, failed);
    }
};

struct QJS_Context {
    QJS_Runtime *runtime;
    JSContext *context;
    JSAtom private_key;
    JSAtom class_private_key;
    JSAtom exports_key;
    JSAtom prototype_key;
    JSAtom toString_key;
    JSAtom toUnity_key;
    JSValue init_object;
    JSValue create_operators;
    JSAtom operator_set_atom;
    JSAtom push_atom;
    JSAtom slice_atom;
    JSAtom fields_atom;
    
    JSValue export_temp;
    
    map<string, QJS_Class *> class_map;
    map<int, QJS_Promise *> promise_map;
    
    QJS_Item *arguments;
    QJS_Item *results;
    
    vector<int> ids;
    
    set<QJS_Value *> cache;
    
    int64_t clear_time = 0;
    
    list<JSValue> temp_values;
    
    vector<JSValue> temp_results;
    
    ~QJS_Context();
    
    void clearTemp() {
        if (temp_values.size() > 0) {
            for (auto it = temp_values.begin(), _e = temp_values.end(); it != _e; ++it) {
                JS_FreeValue(context, *it);
            }
            temp_values.clear();
        }
    }
    
    void testClearTemp() {
        if (get_time_ms() > clear_time) {
            clear_time = get_time_ms() + 500;
            clearTemp();
        }
    }
    
    void addTemp(JSValue value) {
        temp_values.push_back(value);
    }
};

void QJS_LogValue(QJS_Context *ctx, JSValue value) {
    
    if (JS_IsException(value)) {
        JSValue ex = JS_GetException(ctx->context);
        QJS_PrintError(ctx, ex, "[LError]");
        JS_FreeValue(ctx->context, ex);
    } else if (JS_IsArray(ctx->context, value)) {
        QJS_Log(ctx->runtime, "[O] this is array.");
    } else {
        const char *str = JS_ToCString(ctx->context, value);
        QJS_Log(ctx->runtime, "[O]%s", str);
        JS_FreeCString(ctx->context, str);
    }
}

struct QJS_Constructor {
    int sharp_id;
    int argv_min;
    int argv_max;
};

struct QJS_Function {
    int sharp_id;
    bool is_static;
    int argv_min;
    int argv_max;
};

struct QJS_Field {
    int sharp_id;
    bool is_static;
};

#define GETTER_MASK (1<<1)
#define SETTER_MASK (1<<2)
#define STATIC_MASK (1<<3)
struct QJS_Property {
    int sharp_id;
    int mask;
};

bool is_equal(JSValue &val1, JSValue &val2) {
    return JS_VALUE_GET_TAG(val1) == JS_VALUE_GET_TAG(val2) && JS_VALUE_GET_PTR(val1) == JS_VALUE_GET_PTR(val2);
}

struct QJS_Class {
    JSClassID id = 0;
    int sharp_id;
    std::string fullname;
    std::string name;
    JSValue func;
    
    struct Info {
        list<QJS_Constructor> constructors;
        map<string, QJS_Field> fields;
        map<string, list<QJS_Function>> functions;
        map<string, list<QJS_Function>> static_functions;
        map<string, QJS_Property> properties;
        map<string, int> enums;
    } *info;
    
    struct {
        vector<QJS_Constructor> constructors;
        vector<QJS_Field> fields;
        vector<list<QJS_Function>> functions;
        vector<list<QJS_Function>> static_functions;
        vector<QJS_Property> properties;
    } members;
    
    QJS_Class(QJS_Runtime *runtime, const char *name) {
        info = new Info;
        id = JS_NewClassID(&id);
        fullname = name;
        this->name = fullname.substr(fullname.find_last_of('.') + 1);
        JSClassDef def = {
            .class_name = this->name.c_str(),
            .finalizer = QJS_Instance::finalizer,
        };
        JS_NewClass(runtime->runtime, id, &def);
    }
    
    ~QJS_Class() {
        if (info) {
            delete info;
        }
    }
    
    static JSValue init(JSContext *context, JSValueConst this_val,
                        int argc, JSValueConst *argv, int magic) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        JSValue obj = JS_UNDEFINED;
        JSValue data = JS_GetProperty(context, this_val, ctx->class_private_key);
        if (JS_IsBigInt(context, data)) {
            int64_t ptr;
            
            if (JS_ToBigInt64(context, &ptr, data) == 0) {
                QJS_Class *clazz = (QJS_Class *)ptr;

                if (argc == 2 && is_equal(argv[0], ctx->init_object)) {
                    const char *wstr = JS_ToCString(context, this_val);
                    JS_FreeCString(context, wstr);
                    
                    JSValue proto = JS_GetProperty(context, this_val, ctx->prototype_key);
                    if (JS_IsException(proto))
                        goto fail;
                    obj = JS_NewObjectProtoClass(context, proto, clazz->id);
                    JS_FreeValue(context, proto);
                    if (JS_IsException(obj))
                        goto fail;
                    
                    QJS_Instance *ins = new QJS_Instance;
                    
                    ins->value = obj;
                    ins->clazz = clazz;
                    if (JS_ToInt32(context, &ins->sharp_id, argv[1])) {
                        delete ins;
                        JS_FreeValue(context, obj);
                        goto fail;
                    }
                    JS_SetOpaque(obj, ins);
                    JS_SetProperty(context, obj, ctx->private_key, JS_NewBigInt64(context, (int64_t)ins));

                    JS_FreeValue(context, data);
                    return obj;
                } else {
                    ctx->ids.clear();
                    for (auto it = clazz->members.constructors.begin(),
                         _e = clazz->members.constructors.end();
                         it != _e; ++it) {
                        auto &con = *it;
                        if (con.argv_min <= argc && con.argv_max >= argc) {
                            ctx->ids.push_back(con.sharp_id);
                        }
                    }
                    if (ctx->ids.size() == 0) {
                        goto fail;
                    }

                    QJS_Instance *ins = new QJS_Instance;
                    ins->clazz = clazz;
                    ctx->arguments[0].set((int64_t)ins);
                    for (int i = 0; i < argc; ++i) {
                        ctx->arguments[1 + i].set(ctx, argv[i]);
                    }
                    ctx->runtime->action(ctx, clazz->sharp_id,
                                         ctx->ids.data(),
                                         (int)ctx->ids.size(),
                                         TYPE_NEW_OBJECT,
                                         argc + 1);

                    if (ctx->results[0].type == ITEM_TYPE_INT) {
                        int type = ctx->results[0].i;
                        if (type == 0) {
                            JSValue proto = JS_GetProperty(context, this_val, ctx->prototype_key);
                            if (JS_IsException(proto))
                                goto fail;
                            obj = JS_NewObjectProtoClass(context, proto, clazz->id);
                            JS_FreeValue(context, proto);
                            if (JS_IsException(obj))
                                goto fail;
                            
                            ins->value = obj;
                            ins->sharp_id = ctx->results[1].i;
                            JS_SetOpaque(obj, ins);
                            JS_SetProperty(context, obj, ctx->private_key, JS_NewBigInt64(context, (int64_t)ins));
                        } else if (type == 1) {
                            QJS_Promise *promise = ctx->promise_map[ctx->results[1].i];
                            obj = JS_DupValue(context, promise->value);
                        } else {
                            obj = JS_NewObject(context);
                        }
                    }

                    JS_FreeValue(context, data);
                    return obj;
                }
            }
            QJS_Log(ctx->runtime, "Error %lld", ptr);
        } else {
            QJS_Log(ctx->runtime, "new object not a big int !");
        }
        fail:
        QJS_Log(ctx->runtime, "new object failed !");
        JS_FreeValue(context, data);
        return JS_NewObject(context);
    }
    
    static JSValue field_getter(JSContext *context, JSValueConst this_val, int magic) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        JSValue data = JS_GetProperty(context, this_val, ctx->private_key);
        if (JS_IsBigInt(context, data)) {
            int64_t ptr;
            JS_ToBigInt64(context, &ptr, data);
            QJS_Instance *ins = (QJS_Instance *)ptr;
            JS_FreeValue(context, data);
            auto& field = ins->clazz->members.fields[magic];
            
            ctx->arguments[0].setInstance(ins);
            ctx->runtime->action(ctx, ins->clazz->sharp_id,
                                 &field.sharp_id, 1,
                                 TYPE_GET_FIELD,
                                 1);
            return ctx->results[0].toValue(ctx);
        }
        JS_FreeValue(context, data);
        return JS_UNDEFINED;
    }
    
    static JSValue field_setter(JSContext *context, JSValueConst this_val, JSValue val, int magic) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        JSValue data = JS_GetProperty(context, this_val, ctx->private_key);
        if (JS_IsBigInt(context, data)) {
            int64_t ptr;
            if (JS_ToBigInt64(context, &ptr, data) == 0) {
                QJS_Instance *ins = (QJS_Instance *)ptr;
                JS_FreeValue(context, data);
                auto& field = ins->clazz->members.fields[magic];
                ctx->arguments[0].setInstance(ins);
                ctx->arguments[1].set(ctx, val);
                ctx->runtime->action(ctx,
                                     ins->clazz->sharp_id,
                                     &field.sharp_id, 1,
                                     TYPE_SET_FIELD,
                                     2);
                return JS_UNDEFINED;
            }
        }
        JS_FreeValue(context, data);
        return JS_UNDEFINED;
    }
    
    static JSValue static_getter(JSContext *context, JSValueConst this_val, int argc,
                                 JSValueConst *argv, int magic, JSValue *func_data) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        if (JS_IsBigInt(context, func_data[0])) {
            int64_t ptr;
            if (JS_ToBigInt64(context, &ptr, func_data[0]) == 0) {
                QJS_Class *clazz = (QJS_Class *)ptr;
                auto& field = clazz->members.fields[magic];
                ctx->runtime->action(ctx,
                                     clazz->sharp_id,
                                     &field.sharp_id, 1,
                                     TYPE_STATIC_GET,
                                     0);
                return ctx->results[0].toValue(ctx);
            }
        }
        return JS_UNDEFINED;
    }
    
    static JSValue static_setter(JSContext *context, JSValueConst this_val, int argc,
                                 JSValueConst *argv, int magic, JSValue *func_data) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        if (argc > 0 && JS_IsBigInt(context, func_data[0])) {
            int64_t ptr;
            if (JS_ToBigInt64(context, &ptr, func_data[0]) == 0) {
                QJS_Class *clazz = (QJS_Class *)ptr;
                auto& field = clazz->members.fields[magic];
                ctx->arguments[0].set(ctx, argv[0]);
                ctx->runtime->action(ctx,
                                     clazz->sharp_id,
                                     &field.sharp_id, 1,
                                     TYPE_STATIC_SET,
                                     1);
                return JS_UNDEFINED;
            }
        }
        return JS_UNDEFINED;
    }
    
    static JSValue static_property_getter(JSContext *context, JSValueConst this_val, int argc,
                                          JSValueConst *argv, int magic, JSValue *func_data) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        if (JS_IsBigInt(context, func_data[0])) {
            int64_t ptr;
            if (JS_ToBigInt64(context, &ptr, func_data[0]) == 0) {
                QJS_Class *clazz = (QJS_Class *)ptr;
                auto& property = clazz->members.properties[magic];
                ctx->runtime->action(ctx,
                                     clazz->sharp_id,
                                     &property.sharp_id, 1,
                                     TYPE_STATIC_PGET,
                                     0);
                return ctx->results[0].toValue(ctx);
            }
        }
        return JS_UNDEFINED;
    }
    
    static JSValue static_property_setter(JSContext *context, JSValueConst this_val, int argc,
                                          JSValueConst *argv, int magic, JSValue *func_data) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        if (argc > 0 && JS_IsBigInt(context, func_data[0])) {
            int64_t ptr;
            if (JS_ToBigInt64(context, &ptr, func_data[0]) == 0) {
                QJS_Class *clazz = (QJS_Class *)ptr;
                auto& property = clazz->members.properties[magic];
                ctx->arguments[0].set(ctx, argv[0]);
                ctx->runtime->action(ctx,
                                     clazz->sharp_id,
                                     &property.sharp_id, 1,
                                     TYPE_STATIC_PSET,
                                     1);
                return JS_UNDEFINED;
            }
        }
        return JS_UNDEFINED;
    }
    
    static JSValue property_getter(JSContext *context, JSValueConst this_val, int magic) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        JSValue data = JS_GetProperty(context, this_val, ctx->private_key);
        if (JS_IsBigInt(context, data)) {
            int64_t ptr;
            JS_ToBigInt64(context, &ptr, data);
            QJS_Instance *ins = (QJS_Instance *)ptr;
            auto& property = ins->clazz->members.properties[magic];
            if (property.mask & GETTER_MASK) {
                JS_FreeValue(context, data);
                ctx->arguments[0].setInstance(ins);
                ctx->runtime->action(ctx,
                                     ins->clazz->sharp_id,
                                     &property.sharp_id, 1,
                                     TYPE_GET_PROPERTY,
                                     1);
                return ctx->results[0].toValue(ctx);
            }
        }
        JS_FreeValue(context, data);
        return JS_UNDEFINED;
    }
    
    static JSValue property_setter(JSContext *context, JSValueConst this_val, JSValue val, int magic) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        JSValue data = JS_GetProperty(context, this_val, ctx->private_key);
        if (JS_IsBigInt(context, data)) {
            int64_t ptr;
            if (JS_ToBigInt64(context, &ptr, data) == 0) {
                QJS_Instance *ins = (QJS_Instance *)ptr;
                auto& property = ins->clazz->members.properties[magic];
                if (property.mask & SETTER_MASK) {
                    JS_FreeValue(context, data);
                    ctx->arguments[0].setInstance(ins);
                    ctx->arguments[1].set(ctx, val);
                    ctx->runtime->action(ctx,
                                         ins->clazz->sharp_id,
                                         &property.sharp_id, 1,
                                         TYPE_SET_PROPERTY,
                                         2);
                    return JS_UNDEFINED;
                }
            }
        }
        JS_FreeValue(context, data);
        return JS_UNDEFINED;
    }
    
    static JSValue call_function(JSContext *context, JSValueConst this_val,
                                 int argc, JSValueConst *argv, int magic) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        JSValue data = JS_GetProperty(context, this_val, ctx->private_key);
        if (JS_IsBigInt(context, data)) {
            int64_t ptr;
            if (JS_ToBigInt64(context, &ptr, data) == 0) {
                QJS_Instance *ins = (QJS_Instance *)ptr;
                auto &functions = ins->clazz->members.functions[magic];
                auto &ids = ctx->ids;
                ids.clear();
                for (auto it = functions.begin(), _e = functions.end(); it != _e; ++it) {
                    if (it->argv_max >= argc && it->argv_min <= argc) {
                        ids.push_back(it->sharp_id);
                    }
                }
                if (ids.size() > 0) {
                    JS_FreeValue(context, data);
                    ctx->arguments[0].setInstance(ins);
                    for (int i = 0; i < argc; ++i) {
                        ctx->arguments[1 + i].set(ctx, argv[i]);
                    }
                    ctx->runtime->action(ctx,
                                         ins->clazz->sharp_id,
                                         ids.data(), (int)ids.size(),
                                         TYPE_INVOKE,
                                         1 + argc);
                    return ctx->results[0].toValue(ctx);
                }
            }
        }
        JS_FreeValue(context, data);
        return JS_UNDEFINED;
    }
    
    static JSValue call_static_function(JSContext *context, JSValueConst this_val,
                                        int argc, JSValueConst *argv, int magic,
                                        JSValue *func_data) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        if (JS_IsBigInt(context, func_data[0])) {
            int64_t ptr;
            if (JS_ToBigInt64(context, &ptr, func_data[0]) == 0) {
                QJS_Class *clazz = (QJS_Class *)ptr;
                auto &functions = clazz->members.static_functions[magic];
                ctx->ids.clear();
                for (auto it = functions.begin(), _e = functions.end(); it != _e; ++it) {
                    if (it->argv_max >= argc && it->argv_min <= argc) {
                        ctx->ids.push_back(it->sharp_id);
                    }
                }
                if (ctx->ids.size() > 0) {
                    for (int i = 0; i < argc; ++i) {
                        ctx->arguments[i].set(ctx, argv[i]);
                    }
                    ctx->runtime->action(ctx,
                                         clazz->sharp_id,
                                         ctx->ids.data(),
                                         (int)ctx->ids.size(),
                                         TYPE_STATIC_INVOKE,
                                         argc);
                    return ctx->results[0].toValue(ctx);
                }
            }
        }
        return JS_UNDEFINED;
    }
    
    static JSValue toString(JSContext *context, JSValueConst this_val,
                            int argc, JSValueConst *argv) {
        JSAtom ToStringAtom = JS_NewAtom(context, "ToString");
        if (JS_HasProperty(context, this_val, ToStringAtom)) {
            JSValue func = JS_GetProperty(context, this_val, ToStringAtom);
            if (JS_IsFunction(context, func)) {
                JSValue argv[0];
                JSValue ret = JS_Call(context, func, this_val, 0, argv);
                JS_FreeValue(context, func);
                JS_FreeAtom(context, ToStringAtom);
                return ret;
            }
            JS_FreeValue(context, func);
        }
        JS_FreeAtom(context, ToStringAtom);
        return JS_NewString(context, "[UnityObject]");
    }
    
    static JSValue set_dfield(JSContext *context, JSValueConst this_val,
                              int argc, JSValueConst *argv, int magic, JSValue *func_data) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        JSValue data = JS_GetProperty(context, this_val, ctx->private_key);
        if (JS_IsBigInt(context, data)) {
            int64_t ptr;
            if (JS_ToBigInt64(context, &ptr, data) == 0 && argc >= 1) {
                JS_FreeValue(context, data);
                QJS_Instance *ins = (QJS_Instance *)ptr;
                ctx->arguments[0].setInstance(ins);
                ctx->arguments[1].set(ctx, func_data[0]);
                ctx->arguments[2].set(ctx, argv[0]);
                ctx->runtime->action(ctx,
                                     ins->clazz->sharp_id,
                                     ctx->ids.data(), 0,
                                     TYPE_SET_DFIELD,
                                     3);
                
                return JS_UNDEFINED;
            }
        }
        JS_FreeValue(context, data);
        return JS_UNDEFINED;
    }
    
    
    static JSValue get_dfield(JSContext *context, JSValueConst this_val,
                              int argc, JSValueConst *argv, int magic, JSValue *func_data) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        JSValue data = JS_GetProperty(context, this_val, ctx->private_key);
        if (JS_IsBigInt(context, data)) {
            int64_t ptr;
            if (JS_ToBigInt64(context, &ptr, data) == 0) {
                JS_FreeValue(context, data);
                QJS_Instance *ins = (QJS_Instance *)ptr;
                ctx->arguments[0].setInstance(ins);
                ctx->arguments[1].set(ctx, func_data[0]);
                ctx->runtime->action(ctx,
                                     ins->clazz->sharp_id,
                                     ctx->ids.data(), 0,
                                     TYPE_GET_DFIELD,
                                     2);
                return ctx->results[0].toValue(ctx);
            }
        }
        JS_FreeValue(context, data);
        return JS_UNDEFINED;
    }
    
    static JSValue addField(JSContext *context, JSValueConst this_val,
                            int argc, JSValueConst *argv) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        if (!JS_IsConstructor(context, this_val)) return JS_UNDEFINED;
        
        if (argc >= 2 && JS_IsString(argv[0])) {
            JSPropertyEnum *ptab;
            uint32_t len;
            bool fieldsExist = false;
            if (JS_GetOwnPropertyNames(context, &ptab, &len, this_val, JS_GPN_STRING_MASK) == 0) {
                for (int i = 0; i < len; ++i) {
                    JSPropertyEnum &en = ptab[i];
                    if (ctx->fields_atom == en.atom) {
                        fieldsExist = true;
                        break;
                    }
                }
                
                js_free(context, ptab);
            }
            JSValue array;
            if (!fieldsExist) {
                JSValue arr = JS_GetProperty(context, this_val, ctx->fields_atom);
                array = JS_Invoke(context, arr, ctx->slice_atom, 0, nullptr);
                JS_SetProperty(context, this_val, ctx->fields_atom, JS_DupValue(context, array));
                JS_FreeValue(context, arr);
            } else {
                array = JS_GetProperty(context, this_val, ctx->fields_atom);
            }
            
            JSValue data = JS_NewArray(context);
            len = min(argc, 4);
            for (int i = 0; i < len; ++i) {
                JS_Invoke(context, data, ctx->push_atom, 1, &argv[i]);
            }
            JS_Invoke(context, array, ctx->push_atom, 1, &data);
            JS_FreeValue(context, data);
            
            JSValue prototype = JS_GetPropertyStr(context, this_val, "prototype");
            JSAtom atom = JS_ValueToAtom(context, argv[0]);
            
            JS_DefinePropertyGetSet(context,
                                    prototype,
                                    atom,
                                    JS_NewCFunctionData(context,
                                                        get_dfield,
                                                        0,
                                                        0,
                                                        1,
                                                        &argv[0]),
                                    JS_NewCFunctionData(context,
                                                        set_dfield,
                                                        0,
                                                        0,
                                                        1,
                                                        &argv[0]),
                                    0);
            JS_FreeAtom(context, atom);
            JS_FreeValue(context, prototype);
            
            JS_FreeValue(context, array);
        }
        return JS_UNDEFINED;
    }
    
    static JSValue generic(JSContext *context, JSValueConst this_val, int argc, JSValueConst *argv) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        if (JS_HasProperty(context, this_val, ctx->class_private_key)) {
            JSValue data = JS_GetProperty(context, this_val, ctx->class_private_key);
            int64_t ptr;
            JS_ToBigInt64(context, &ptr, data);
            JS_FreeValue(context, data);
            QJS_Class *clazz = (QJS_Class *)ptr;
            
            for (int i = 0; i < argc; ++i) {
                JSValue val = argv[i];
                if (JS_HasProperty(context, val, ctx->class_private_key)) {
                    JSValue data = JS_GetProperty(context, this_val, ctx->class_private_key);
                    int64_t ptr;
                    JS_ToBigInt64(context, &ptr, data);
                    JS_FreeValue(context, data);
                    QJS_Class *clazz = (QJS_Class *)ptr;
                    ctx->arguments[i].set(clazz->sharp_id);
                } else {
                    QJS_Error(ctx->runtime, "Wrong argument.");
                    return JS_UNDEFINED;
                }
            }
            ctx->runtime->action(ctx,
                                 clazz->sharp_id,
                                 ctx->ids.data(), 0,
                                 TYPE_GENERIC_CLASS,
                                 argc);
            return ctx->results[0].toValue(ctx);
        }
        return JS_UNDEFINED;
    }
    
    static JSValue array(JSContext *context, JSValueConst this_val, int argc, JSValueConst *argv) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        if (JS_HasProperty(context, this_val, ctx->class_private_key)) {
            JSValue data = JS_GetProperty(context, this_val, ctx->class_private_key);
            int64_t ptr;
            JS_ToBigInt64(context, &ptr, data);
            JS_FreeValue(context, data);
            QJS_Class *clazz = (QJS_Class *)ptr;
            ctx->runtime->action(ctx,
                                 clazz->sharp_id,
                                 ctx->ids.data(), 0,
                                 TYPE_ARRAY_CLASS,
                                 0);
            return ctx->results[0].toValue(ctx);
        }
        return JS_UNDEFINED;
    }

    void submit(QJS_Context *ctx) {
        int length = 0;
        
        vector<JSCFunctionListEntry> properties;
        for (auto it = info->constructors.begin(), _e = info->constructors.end(); it != _e; ++it) {
            auto &cons = *it;
            if (cons.argv_max > length) {
                length = cons.argv_max;
            }
        }
        
        JSContext *context = ctx->context;
        JSValue proto = JS_NewObject(context);
        JSValue func = JS_NewCFunctionMagic(context, QJS_Class::init, name.c_str(), length, JS_CFUNC_constructor, sharp_id);
//        JSValue func = JS_NewCFunction2(ctx->context, QJS_Class::init, name.c_str(), length, JS_CFUNC_constructor, sharp_id);

        members.constructors = vector<QJS_Constructor>();
        for (auto it = info->constructors.begin(), _e = info->constructors.end(); it != _e; ++it) {
            members.constructors.push_back(*it);
        }
        
        JSValue thisData = JS_NewBigInt64(context, (long)this);
        for (auto it = info->fields.begin(), _e = info->fields.end(); it != _e; ++it) {
            int16_t size = (int16_t)members.fields.size();
            members.fields.push_back(it->second);
            if (it->second.is_static) {
                JSAtom atom = JS_NewAtom(context, it->first.c_str());
                JS_DefinePropertyGetSet(context,
                                        func, atom,
                                        JS_NewCFunctionData(context,
                                                            QJS_Class::static_getter,
                                                            0, size,
                                                            1, &thisData),
                                        JS_NewCFunctionData(context,
                                                            QJS_Class::static_setter,
                                                            1, size,
                                                            1, &thisData),
                                        0);
                JS_FreeAtom(context, atom);
            } else {
                properties.push_back(JS_CGETSET_MAGIC_DEF(it->first.c_str(), field_getter, field_setter, size));
            }
        }

        for (auto it = info->properties.begin(), _e = info->properties.end(); it != _e; ++it) {
            int16_t size = (int16_t)members.properties.size();
            auto &pro = it->second;
            members.properties.push_back(pro);
            if (pro.mask & STATIC_MASK) {
                JSAtom atom = JS_NewAtom(context, it->first.c_str());
                JS_DefinePropertyGetSet(context,
                                        func, atom,
                                        JS_NewCFunctionData(context,
                                                            QJS_Class::static_property_getter,
                                                            0, size,
                                                            1, &thisData),
                                        JS_NewCFunctionData(context,
                                                            QJS_Class::static_property_setter,
                                                            0, size,
                                                            1, &thisData),
                                        0);
                JS_FreeAtom(context, atom);
            } else {
                properties.push_back(JS_CGETSET_MAGIC_DEF(it->first.c_str(), (pro.mask & GETTER_MASK) ? property_getter : nullptr, (pro.mask & SETTER_MASK) ? property_setter : nullptr, size));
            }
        }

        for (auto it = info->functions.begin(), _e = info->functions.end(); it != _e; ++it) {
            int16_t size = (int16_t)members.functions.size();
            auto &list = it->second;
            uint8_t max = 0;
            for (auto it = list.begin(), _e = list.end(); it != _e; ++it) {
                if (max < it->argv_max) {
                    max = it->argv_max;
                }
            }
            members.functions.push_back(list);
            
            const char *strname = it->first.c_str();
            JSValue func = JS_NewCFunctionMagic(context, call_function, strname, max, JS_CFUNC_generic_magic, size);
            JS_SetPropertyStr(context, proto, strname, func);
            
//            properties.push_back(JS_CFUNC_MAGIC_DEF(it->first.c_str(), max, call_function, size));
//
//            if (it->first == "ToString") {
//                properties.push_back(JS_CFUNC_MAGIC_DEF("toString", max, call_function, size));
//            }
        }
        
        bool has_operator = false;
        for (auto it = info->static_functions.begin(), _e = info->static_functions.end(); it != _e; ++it) {
            int16_t size = (int16_t)members.static_functions.size();
            auto &list = it->second;
            uint8_t max = 0;
            for (auto it = list.begin(), _e = list.end(); it != _e; ++it) {
                if (max < it->argv_max) {
                    max = it->argv_max;
                }
            }
            members.static_functions.push_back(list);
            string key = it->first;
            if (key.compare(0, 3, "op_") == 0) {
                auto it = op_map.find(key);
                if (it != op_map.end()) {
                    key = it->second;
                    has_operator = true;
                }
            }
            
            JS_SetPropertyStr(context,
                              func,
                              key.c_str(),
                              JS_NewCFunctionData(context,
                                                  call_static_function,
                                                  max,
                                                  size,
                                                  1,
                                                  &thisData)
                              );
        }
        
        for (auto it = info->enums.begin(), _e = info->enums.end(); it != _e; ++it) {
            JS_SetPropertyStr(context,
                              func,
                              it->first.c_str(),
                              JS_NewInt32(context, it->second));
        }
        
//        JS_SetProperty(context, proto, ctx->toString_key, JS_NewCFunction(context, toString, "toString", 0));

        JS_SetProperty(context, func, ctx->fields_atom, JS_NewArray(context));
        JS_SetPropertyStr(context, proto, "toString",JS_NewCFunction(context, toString, "toString", 0));
        JS_SetPropertyStr(context, func, "field", JS_NewCFunction(context, addField, "field", 4));
        if (has_operator) {
            JSValue operators = JS_Call(context,
                                        ctx->create_operators,
                                        func,
                                        1,
                                        &func);
            if (JS_IsException(operators)) {
                QJS_Error(ctx->runtime, "Error when process operators");
            } else {
                JS_SetProperty(context,
                               proto,
                               ctx->operator_set_atom,
                               operators);
            }
        }
        
        JS_SetPropertyFunctionList(context, proto, properties.data(), (int)properties.size());
        JS_SetConstructor(context, func, proto);
        JS_SetClassProto(context, id, proto);
        
        JS_SetProperty(context, func, ctx->class_private_key, JS_DupValue(context, thisData));
        
        JS_SetPropertyStr(context, func, "generic", JS_NewCFunction(context, generic, "generic", 0));
        JS_SetPropertyStr(context, func, "array", JS_NewCFunction(context, array, "array", 0));
        
        JS_FreeValue(context, thisData);
        
        delete info;
        info = nullptr;
        
        this->func = func;
    }
};

QJS_Context::~QJS_Context() {
    for (auto it = class_map.begin(), _e = class_map.end(); it != _e; ++it) {
        delete it->second;
    }
}

void QJS_Item::setInstance(QJS_Instance *value) {
    type = ITEM_TYPE_JS_OBJECT;
    i = value->sharp_id;
}

void QJS_Item::setClass(QJS_Class *value) {
    type = ITEM_TYPE_JS_CLASS;
    i = value->sharp_id;
}

void QJS_Item::setValue(JSValue value) {
    type = ITEM_TYPE_JS_VALUE;
    p = JS_VALUE_GET_OBJ(value);
}

void QJS_Item::set(QJS_Context *ctx, JSValue value) {
    JSContext *context = ctx->context;
    auto tag = JS_VALUE_GET_TAG(value);
    switch (tag) {
        case JS_TAG_INT: {
            int32_t v = 0;
            JS_ToInt32(context, &v, value);
            set(v);
            return;
        }
        case JS_TAG_BIG_INT: {
            int64_t v = 0;
            JS_ToBigInt64(context, &v, value);
            set(v);
            return;
        }
        case JS_TAG_BIG_FLOAT: {
            double v = 0;
            JS_ToFloat64(context, &v, value);
            set(v);
            return;
        }
        case JS_TAG_FLOAT64: {
            double v = 0;
            JS_ToFloat64(context, &v, value);
            set(v);
            return;
        }
        case JS_TAG_BOOL: {
            set((bool)JS_ToBool(context, value));
            return;
        }
        case JS_TAG_STRING: {
            type = ITEM_TYPE_JS_STRING;
            p = JS_VALUE_GET_PTR(value);
            return;
        }
        case JS_TAG_OBJECT: {
            if (JS_HasProperty(context, value, ctx->private_key)) {
                JSValue data = JS_GetProperty(context, value, ctx->private_key);
                int64_t ptr = 0;
                JS_ToBigInt64(context, &ptr, data);
                JS_FreeValue(context, data);
                QJS_Instance *ins = (QJS_Instance *)ptr;
                setInstance(ins);
            }
            else if (JS_HasProperty(context, value, ctx->class_private_key)) {
                JSValue data = JS_GetProperty(context, value, ctx->class_private_key);
                int64_t ptr = 0;
                JS_ToBigInt64(context, &ptr, data);
                JS_FreeValue(context, data);
                QJS_Class *clazz = (QJS_Class *)ptr;
                if (is_equal(clazz->func, value)) {
                    setClass(clazz);
                } else {
                    setValue(value);
                }
            }
            else {
                setValue(value);
            }
            return;
        }
            
        default:
        {
            if (JS_TAG_IS_FLOAT64(tag)) {
                double v = 0;
                JS_ToFloat64(context, &v, value);
                set(v);
                return;
            }
        }
            break;
    }
    setNull();
}

JSValue promise_init(JSContext *context, JSValueConst this_val, int argc, JSValueConst *argv, int magic, JSValue *func_data) {
    QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
    int64_t ptr;
    JS_ToBigInt64(context, &ptr, *func_data);
    QJS_Promise *pro = (QJS_Promise *)ptr;
    if (argc >= 2) {
        pro->success = JS_DupValue(context, argv[0]);
        pro->failed = JS_DupValue(context, argv[1]);
    } else {
        QJS_Log(ctx->runtime, "- WTF ? %d", argc);
    }
    return JS_UNDEFINED;
}

JSValue QJS_Item::toValue(QJS_Context *ctx) {
    JSContext *context = ctx->context;
    switch (type) {
        case ITEM_TYPE_INT:
            return JS_NewInt32(context, i);
        case ITEM_TYPE_LONG:
            return JS_NewBigInt64(context, l);
        case ITEM_TYPE_DOUBLE:
            return JS_NewFloat64(context, d);
        case ITEM_TYPE_BOOL:
            return JS_NewBool(context, b);
        case ITEM_TYPE_STRING:
            return JS_NewString(context, s);
        case ITEM_TYPE_OBJECT:
        {
            QJS_Instance *instance = (QJS_Instance *)p;
            return JS_DupValue(context, instance->value);
        }
        case ITEM_TYPE_CLASS:
        {
            QJS_Class *clazz = (QJS_Class *)p;
            return JS_DupValue(context, clazz->func);
        }
        case ITEM_TYPE_VALUE:
        {
            QJS_Value *value = (QJS_Value *)p;
            return JS_DupValue(context, value->value);
        }
        case ITEM_TYPE_PROMISE:
        {
            JSContext *context = ctx->context;
            int promise = i;
            auto it = ctx->promise_map.find(promise);
            if (it == ctx->promise_map.end()) {
                return JS_NULL;
            } else {
                return JS_DupValue(context, it->second->value);
            }
        }
            
        default:
            break;
    }
    return JS_NULL;
}

JSValue QJS_Instance::functionCallback(JSContext *context, JSValue this_val, int argc, JSValue *argv, int magic, JSValue *func_data)  {
    QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
    int64_t ptr;
    if (JS_ToBigInt64(context, &ptr, *func_data) == 0) {
        QJS_Instance *ins = (QJS_Instance *)ptr;
        ctx->arguments[0].set(magic);
        for (int i = 0; i < argc; ++i) {
            ctx->arguments[1 + i].set(ctx, argv[i]);
        }
        ctx->runtime->action(ctx,
                             ins->clazz->sharp_id,
                             ctx->ids.data(), 0,
                             TYPE_CALL_DELEGATE,
                             1 + argc);
        return ctx->results[0].toValue(ctx);
    }
    
    return JS_UNDEFINED;
}

JSValue loadClass(JSContext *context, JSValueConst this_val,
                  int argc, JSValueConst *argv) {
    JSValue ret = JS_UNDEFINED;
    if (argc >= 1) {
        JSValue name = argv[0];
        if (JS_IsString(name)) {
            const char *str = JS_ToCString(context, name);
            QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
            auto it = ctx->class_map.find(str);
            if (it != ctx->class_map.end()) {
                JS_FreeCString(context, str);
                return JS_DupValue(context, it->second->func);
            }
            
            ctx->runtime->loadClass(ctx, str);
            ret = ctx->results[0].toValue(ctx);
            JS_FreeCString(context, str);
        }
    }
    return ret;
}

JSValue consolePrint(JSContext *ctx, int type, int argc, JSValueConst *argv) {
    string str;
    for (int i = 0; i < argc; ++i) {
        const char *cstr = JS_ToCString(ctx, argv[i]);
        if (cstr) {
            str += cstr;
            JS_FreeCString(ctx, cstr);
            if (i != argc - 1) {
                str += ',';
            }
        }
    }
    JSRuntime *runtime = JS_GetRuntime(ctx);
    QJS_Runtime *rt = (QJS_Runtime *)JS_GetRuntimeOpaque(runtime);
    QJS_Print(rt, type, "%s", str.c_str());
    return JS_UNDEFINED;
}

void QJS_Print(QJS_Runtime *rt, int type, const char *format, ...) {
    va_list vlist;
    char str[1024];
    va_start(vlist, format);
    vsnprintf(str, 1024, format, vlist);
    va_end(vlist);
    str[1023] = 0;
    if (rt && rt->handlers.print)
        rt->handlers.print(type, str);
}

void QJS_Print(QJS_Runtime *rt, int type, const char *format, va_list vlist) {
    char str[1024];
    vsnprintf(str, 1024, format, vlist);
    str[1023] = 0;
    if (rt && rt->handlers.print)
        rt->handlers.print(type, str);
}

struct QJS_Arguments {
    QJS_Item *arguments;
    int length = 0;
    QJS_Context *ctx;
    const char **free_item;
    int free_length;
};

void foreach_handler(JSContext *context, void *data, int argc, JSValueConst *argv) {
    if (argc > 0) {
        QJS_Arguments *arg = (QJS_Arguments *)data;
        arg->arguments[arg->length++].set(arg->ctx, argv[0]);
    }
}

JSValue to_array(JSContext *context, JSValueConst this_val, int argc, JSValueConst *argv) {
    QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
    if (argc >= 2) {
        JSValue func = argv[0];
        JSValue arr = argv[1];
        if (JS_HasProperty(context, func, ctx->class_private_key) && JS_IsArray(context, arr)) {
            JSValue data = JS_GetProperty(context, func, ctx->class_private_key);
            int64_t ptr;
            JS_ToBigInt64(context, &ptr, data);
            QJS_Class *clazz = (QJS_Class *)ptr;
            JSValue len = JS_GetPropertyStr(context, arr, "length");
            int length = 0;
            JS_ToInt32(context, &length, len);
            JS_FreeValue(context, len);
            
            QJS_Arguments arguments{
                .arguments = (QJS_Item *)malloc(sizeof(QJS_Item) * length),
                .ctx = ctx,
                .free_item = (const char **)malloc(length * sizeof(const char *)),
                .free_length = length
            };
            JS_ArrayForEach(context, arr, foreach_handler, &arguments);
            ctx->arguments[0].set(length);
            int64_t argvPtr = (int64_t)arguments.arguments;
            ctx->arguments[1].set(argvPtr);
            ctx->runtime->action(ctx,
                                 clazz->sharp_id,
                                 ctx->ids.data(), 0,
                                 TYPE_NEW_ARRAY, 2);
            for (int i = 0; i < arguments.free_length; ++i) {
                JS_FreeCString(context, arguments.free_item[i]);
            }
            free(arguments.free_item);
            return ctx->results[0].toValue(ctx);
        } else {
            QJS_Log(ctx->runtime, "wrong arguments in array().");
        }
    } else {
        QJS_Log(ctx->runtime, "wrong arguments number in array().");
    }
    return JS_UNDEFINED;
}

void pop_seg(string &str) {
    size_t last = str.find_last_of("/");
    if (last < str.length()) {
        str = str.substr(0, last);
    } else {
        str = "";
    }
}

bool isWordChar(char x) {
    return (x >= 'a' && x <= 'z') || (x >= 'A' && x <= 'Z') || (x >= '0' && x <= '9') || x == '_';
}

bool has_export(const string &strcode) {
    static string key("export");
    float found = false;
    size_t off = 0;
    while (off < strcode.size()) {
        size_t idx = strcode.find(key, off);
        if (idx < strcode.size()) {
            bool c1 = idx == 0 || !isWordChar(strcode[idx - 1]), c2 = (idx + key.size()) == strcode.size() || !isWordChar(strcode[idx + key.size()]);
            found = c1 && c2;
            if (found) break;
        }
        off = idx < strcode.size() ? idx + key.size() : idx;
    }
    return found;
}

char *module_name(JSContext *context,
                  const char *module_base_name,
                  const char *module_name,
                  void *opaque) {
    QJS_Runtime *rt = (QJS_Runtime *)opaque;
    QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
    return rt->moduleName(ctx, module_base_name, module_name);
}

JSModuleDef *module_loader(JSContext *context,
                           const char *module_name, void *opaque) {
    QJS_Runtime *rt = (QJS_Runtime *)opaque;
    QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
    
    rt->loadModule(ctx, module_name);

    JSModuleDef *module = nullptr;
    if (ctx->results[0].type == ITEM_TYPE_STRING) {
        string strcode(ctx->results[0].s);
        if (!has_export(strcode)) {
            stringstream ss;
            ss << "const module = {exports: {}}; let exports = module.exports;" << endl;
            ss << strcode << endl;
            ss << "export default module.exports;" << endl;
            strcode = ss.str();
        }
        JSValue val = JS_Eval(context, strcode.data(), (int)strcode.size(), module_name, JS_EVAL_TYPE_MODULE | JS_EVAL_FLAG_COMPILE_ONLY);
        if (!JS_IsException(val)) {
            module = (JSModuleDef *)JS_VALUE_GET_PTR(val);
            JS_FreeValue(context, val);
        }
    }
    return module;
    
//    return nullptr;
}

void QJS_PrintError(QJS_Context *ctx, JSValue value, const char *prefix) {
    stringstream ss;
    JSContext *context = ctx->context;
    const char *str = JS_ToCString(context, value);
    if (str) {
        ss << str << endl;
        JS_FreeCString(context, str);
    }
    
    JSValue stack = JS_GetPropertyStr(context, value, "stack");
    if (!JS_IsException(stack)) {
        str = JS_ToCString(context, stack);
        if (str) {
            ss << str << endl;
            JS_FreeCString(context, str);
        }
        JS_FreeValue(context, stack);
    }
    
    string output = ss.str();
    if (prefix) {
        QJS_Error(ctx->runtime, "%s: %s", prefix, output.c_str());
    } else {
        QJS_Error(ctx->runtime, "%s", output.c_str());
    }
}

bool is_object_type(const QJS_Item &item) {
    return (item.type == ITEM_TYPE_VALUE || item.type == ITEM_TYPE_OBJECT || item.type == ITEM_TYPE_CLASS);
}
JSValue get_value(const QJS_Item &item) {
    switch (item.type) {
        case ITEM_TYPE_VALUE:
            return ((QJS_Value *)item.p)->value;
        case ITEM_TYPE_OBJECT:
            return ((QJS_Instance *)item.p)->value;
        case ITEM_TYPE_CLASS:
            return ((QJS_Class *)item.p)->func;
            
        default:
            return JS_NULL;
    }
}

vector<uint8_t> init_code =
#include "pack.h"
;

}

using namespace qjs;
extern "C" {

QJS_Runtime *_temp_runtime = nullptr;
stringstream output;

void JS_Log(const char *format, ...) {
    if (_temp_runtime) {
        va_list vlist;
        va_start(vlist, format);
        char str[256];
        vsnprintf(str, 256, format, vlist);
        va_end(vlist);
        str[255] = 0;
        
        string sstr = str;
        output << sstr;
        if (sstr.find('\n') >= 0) {
            string all = output.str();
            QJS_Print(_temp_runtime, 0, "%s", all.c_str());
            stringstream temp;
            output.swap(temp);
        }
    }
}

void *QJS_Setup(QJS_Handlers handlers, QJS_Item *arguments, QJS_Item *results) {
    QJS_Runtime *rt = new QJS_Runtime;
    rt->runtime = JS_NewRuntime();
    rt->arguments = arguments;
    rt->results = results;
    JS_SetRuntimeOpaque(rt->runtime, rt);
//    js_std_init_handlers(rt->runtime);
    JS_SetModuleLoaderFunc(rt->runtime, module_name, module_loader, rt);
    
    rt->handlers = handlers;
    _temp_runtime = rt;
    return rt;
}

void QJS_Shutdown(QJS_Runtime *rt) {
//    js_std_free_handlers(rt->runtime);
    JS_FreeRuntime(rt->runtime);
    delete rt;
    if (_temp_runtime == rt) _temp_runtime = nullptr;
}

QJS_Context *QJS_NewContext(QJS_Runtime *rt) {
    QJS_Context *ctx = new QJS_Context();
    JSContext *context = ctx->context = JS_NewContext(rt->runtime);
    JS_AddIntrinsicOperators(context);
    JS_AddIntrinsicRequire(context);
    JS_SetContextOpaque(context, ctx);
    ctx->runtime = rt;
    ctx->arguments = rt->arguments;
    ctx->results = rt->results;
    ctx->private_key = JS_NewAtom(context, "_$tar");
    ctx->class_private_key = JS_NewAtom(context, "_$class");
    ctx->exports_key = JS_NewAtom(context, "exports");
    ctx->prototype_key = JS_NewAtom(context, "prototype");
    ctx->toString_key = JS_NewAtom(context, "toString");
    ctx->toUnity_key = JS_NewAtom(context, "toUnity");
    ctx->push_atom = JS_NewAtom(context, "push");
    ctx->slice_atom = JS_NewAtom(context, "slice");
    ctx->fields_atom = JS_NewAtom(context, "_$fields");
    ctx->init_object = JS_NewObject(context);
    
    JSValue global = JS_GetGlobalObject(context);

    JSValue unity = JS_NewCFunction(context, loadClass, "unity", 1);
    JSValue console = JS_NewObject(context);

    JS_SetPropertyStr(context, console, "log", JS_NewCFunction(context, [](JSContext *ctx, JSValueConst this_val, int argc, JSValueConst *argv){
        consolePrint(ctx, 0, argc, argv);
        return JS_UNDEFINED;
    }, "log", 1));
    JS_SetPropertyStr(context, console, "warn", JS_NewCFunction(context, [](JSContext *ctx, JSValueConst this_val, int argc, JSValueConst *argv){
        consolePrint(ctx, 1, argc, argv);
        return JS_UNDEFINED;
    }, "warn", 1));
    JS_SetPropertyStr(context, console, "error", JS_NewCFunction(context, [](JSContext *ctx, JSValueConst this_val, int argc, JSValueConst *argv){
        consolePrint(ctx, 2, argc, argv);
        return JS_UNDEFINED;
    }, "error", 1));
    JS_SetPropertyStr(context, global, "unity", unity);
    JS_SetPropertyStr(context, global, "console", console);
    
    JSValue Operators = JS_GetPropertyStr(context, global, "Operators");
    ctx->create_operators = JS_GetPropertyStr(context, Operators, "create");
    JS_FreeValue(context, Operators);
    
    JSValue Symbol = JS_GetPropertyStr(context, global, "Symbol");
    JSValue operatorSet = JS_GetPropertyStr(context, Symbol, "operatorSet");
    ctx->operator_set_atom = JS_ValueToAtom(context, operatorSet);
    JS_FreeValue(context, operatorSet);
    JS_FreeValue(context, Symbol);
    
    JS_SetPropertyStr(context, global, "array", JS_NewCFunction(context, to_array, "array", 2));
    
    JSAtom globalAtom = JS_NewAtom(context, "global");
    JS_DefinePropertyGetSet(context, global, globalAtom, JS_NewCFunction(context, [](JSContext *ctx, JSValueConst this_val, int argc, JSValueConst *argv){
        return JS_GetGlobalObject(ctx);
    }, "global", 0), JS_UNDEFINED, 0);
    JS_FreeAtom(context, globalAtom);
    
    JS_SetPropertyStr(context, global, "_createWorker", JS_NewCFunction(context, [](JSContext *context, JSValueConst this_val, int argc, JSValueConst *argv) {
        QJS_Context *ctx = (QJS_Context *)JS_GetContextOpaque(context);
        JSAtom atom = JS_GetScriptOrModuleName(context, 2);
        JSValue ret = JS_UNDEFINED;;
        if (atom != JS_ATOM_NULL) {
            const char *base = JS_AtomToCString(context, atom);
            ctx->arguments[0].set(base);
            ctx->arguments[1].set(ctx, argv[0]);
            
            ctx->runtime->action(ctx, 0, ctx->ids.data(), 0, TYPE_CREATE_WORKER, 2);
            ret = ctx->results[0].toValue(ctx);
            
            JS_FreeCString(context, base);
        } else {
            ctx->arguments[0].set("");
            ctx->arguments[1].set(ctx, argv[0]);
            
            ctx->runtime->action(ctx, 0, ctx->ids.data(), 0, TYPE_CREATE_WORKER, 2);
            ret = ctx->results[0].toValue(ctx);
        }
        return ret;
    }, "_createWorker", 1));
    
    JS_FreeValue(context, global);
    
    JSValue val = JS_ReadObject(context, init_code.data(), init_code.size(), JS_READ_OBJ_BYTECODE);
    if (!JS_IsException(val)) {
        JS_EvalFunction(context, val);
    }
    
    return ctx;
}

void QJS_DeleteContext(QJS_Context *ctx) {
    QJS_ClearResults(ctx);
    ctx->clearTemp();
    for (auto it = ctx->cache.begin(), _e = ctx->cache.end(); it != _e; ++it) {
        auto &val = *it;
        if (val->free) 
            JS_FreeValue(ctx->context, val->value);
        delete val;
    }
    ctx->cache.clear();
    for (auto it = ctx->class_map.begin(), _e = ctx->class_map.end(); it != _e; ++it) {
        JS_FreeValue(ctx->context, it->second->func);
        delete it->second;
    }
    ctx->class_map.clear();
    
    for (auto it = ctx->promise_map.begin(), _e = ctx->promise_map.end(); it != _e; ++it) {
        QJS_Promise *pro = it->second;
        pro->free(ctx->context);
        delete pro;
    }
    ctx->promise_map.clear();
    
    JS_FreeAtom(ctx->context, ctx->private_key);
    JS_FreeAtom(ctx->context, ctx->class_private_key);
    JS_FreeAtom(ctx->context, ctx->exports_key);
    JS_FreeAtom(ctx->context, ctx->prototype_key);
    JS_FreeAtom(ctx->context, ctx->toString_key);
    JS_FreeAtom(ctx->context, ctx->toUnity_key);
    JS_FreeAtom(ctx->context, ctx->push_atom);
    JS_FreeAtom(ctx->context, ctx->slice_atom);
    JS_FreeAtom(ctx->context, ctx->fields_atom);
    JS_FreeValue(ctx->context, ctx->init_object);
    JS_FreeValue(ctx->context, ctx->create_operators);
    JS_FreeContext(ctx->context);
    delete ctx;
}

QJS_Class *QJS_RegisterClass(QJS_Context *ctx, const char *name, int sharp_id) {
    auto  it = ctx->class_map.find(name);
    if (it == ctx->class_map.end()) {
        QJS_Class *cur_cls = new QJS_Class(ctx->runtime, name);
        ctx->class_map[name] = cur_cls;
        cur_cls->sharp_id = sharp_id;
        return cur_cls;
    } else
        return it->second;
}

void QJS_SubmitClass(QJS_Context *ctx, QJS_Class *clazz) {
    clazz->submit(ctx);
}

void QJS_AddConstructor(QJS_Class *clazz, int sharp_id, int argv_min, int argv_max) {
    clazz->info->constructors.push_back(QJS_Constructor{
        .sharp_id = sharp_id,
        .argv_min = argv_min,
        .argv_max = argv_max
    });
}

void QJS_AddField(QJS_Class *clazz, const char *name, int sharp_id, bool is_static) {
    clazz->info->fields[name] = QJS_Field {
        .sharp_id = sharp_id,
        .is_static = is_static
    };
}

void QJS_AddFunction(QJS_Class *clazz, const char *name, int sharp_id, bool is_static, int argv_min, int argv_max) {
    string name_str(name);
    map<string, list<QJS_Function>>::iterator it;
    if (is_static) {
        it = clazz->info->static_functions.find(name_str);
        if (it == clazz->info->static_functions.end()) {
            clazz->info->static_functions[name_str] = list<QJS_Function>();
            it = clazz->info->static_functions.find(name_str);
        }
    } else {
        it = clazz->info->functions.find(name_str);
        if (it == clazz->info->functions.end()) {
            clazz->info->functions[name_str] = list<QJS_Function>();
            it = clazz->info->functions.find(name_str);
        }
    }
    
    it->second.push_back(QJS_Function {
        .sharp_id = sharp_id,
        .is_static = is_static,
        .argv_min = argv_min,
        .argv_max = argv_max
    });
}

void QJS_AddProperty(QJS_Class *clazz, const char *name, int sharp_id, int mask) {
    clazz->info->properties[name] = QJS_Property {
        .sharp_id = sharp_id,
        .mask = mask
    };
}

void QJS_AddEnum(QJS_Class *clazz, const char *name, int value) {
    clazz->info->enums[name] = value;
}

QJS_Instance *QJS_NewInstance(QJS_Context *ctx, QJS_Class *clazz, int sharp_id) {
    ctx->testClearTemp();
    auto context = ctx->context;
    QJS_Instance *instance = new QJS_Instance;
//    QJS_Log(ctx->runtime, "New instance %s", clazz->name.c_str());
    instance->value = JS_NewObjectClass(context, clazz->id);
    instance->sharp_id = sharp_id;
    instance->clazz = clazz;
    JS_SetOpaque(instance->value, instance);
    JS_SetProperty(context, instance->value, ctx->private_key, JS_NewBigInt64(context, (int64_t)instance));
    ctx->addTemp(instance->value);
    return instance;
}

QJS_Instance *QJS_NewFunction(QJS_Context *ctx, QJS_Class *clazz, int sharp_id) {
    ctx->testClearTemp();
    auto context = ctx->context;
    QJS_Instance *instance = new QJS_Instance;
    
    JSValue data = JS_NewBigInt64(context, (int64_t)instance);
    instance->value = JS_NewCFunctionDataFinalizer(context, QJS_Instance::functionCallback,
                                                   0, sharp_id, 1, &data,
                                                   QJS_Instance::functionFinalizer, instance);
    instance->sharp_id = sharp_id;
    instance->clazz = clazz;
//    JS_SetOpaque(instance->value, instance);
//    JS_SetProperty(context, instance->value, ctx->private_key, data);
    ctx->addTemp(instance->value);
    return instance;
}

void QJS_ReleaseJSValue(QJS_Context *ctx, QJS_Value *value) {
    if (ctx->cache.find(value) != ctx->cache.end()) {
        if (value->free) {
            JS_FreeValue(ctx->context, value->value);
        }
        delete value;
        ctx->cache.erase(value);
    }
}

enum SharpAction{
    SharpCall = 0,
    SharpGet,
    SharpSet,
    SharpCallAtom,
    SharpGetAtom,
    SharpSetAtom,
    SharpInvoke,
    SharpNew,
    SharpNewArray,
    SharpNewObject,
    SharpNewBind,
    SharpGetIndex,
    SharpSetIndex,
    SharpPromiseComplete
};

void QJS_ClearResults(QJS_Context *ctx) {
    for (auto it = ctx->temp_results.begin(), _e = ctx->temp_results.end(); it != _e; ++it) {
        JS_FreeValue(ctx->context, *it);
    }
    ctx->temp_results.clear();
}


int QJS_Action(QJS_Context *ctx, int type, int argc) {
    QJS_Item *argv = ctx->arguments;
    QJS_Item *results = ctx->results;
    JSContext *context = ctx->context;
    JSValue ex = JS_GetException(context);
    if (!JS_IsNull(ex)) {
        QJS_PrintError(ctx, ex, "[Uncatch]]");
        JS_FreeValue(context, ex);
    }
    switch (type) {
        case SharpCall:
        {
            if (argc >= 2 && is_object_type(argv[0]) && argv[1].type == ITEM_TYPE_STRING) {
                const char *name = argv[1].s;
                JSValue value = get_value(argv[0]);
                vector<JSValue> params;
                if (argc > 2) {
                    params.reserve(argc - 2);
                    for (size_t i = 2; i < argc; ++i) {
                        params.push_back(argv[i].toValue(ctx));
                    }
                }
                
                JSAtom atom = JS_NewAtom(context, name);
                if (JS_HasProperty(context, value, atom)) {
                    JSValue ret = JS_Invoke(context, value, atom, (int)params.size(), params.data());
                    for (auto it = params.begin(), _e = params.end(); it != _e; ++it) {
                        JS_FreeValue(context, *it);
                    }
                    
                    if (JS_IsException(ret)) {
                        JSValue ex = JS_GetException(context);
                        QJS_PrintError(ctx, ex);
                        JS_FreeValue(context, ex);
                    }
                    results[0].set(ctx, ret);
                    ctx->temp_results.push_back(ret);
                } else {
                    results[0].setNull();
                }
                JS_FreeAtom(context, atom);
                return 1;
            }
        }
            break;
        case SharpGet: {
            if (argc >= 2 && is_object_type(argv[0]) && argv[1].type == ITEM_TYPE_STRING) {
                const char *name = argv[1].s;
                JSValue value = get_value(argv[0]);
                JSValue ret = JS_GetPropertyStr(context, value, name);
                results[0].set(ctx, ret);
                ctx->temp_results.push_back(ret);
                return 1;
            }
        }
            break;
        case SharpSet: {
            if (argc >= 3 && is_object_type(argv[0]) && argv[1].type == ITEM_TYPE_STRING) {
                const char *name = argv[1].s;
                JSValue value = get_value(argv[0]);
                JS_SetPropertyStr(context, value, name, argv[2].toValue(ctx));
                return 0;
            }
        }
            break;
        case SharpCallAtom:
        {
            if (argc >= 2 && is_object_type(argv[0]) && argv[1].type == ITEM_TYPE_INT) {
                uint32_t atom = argv[1].i;
                JSValue value = get_value(argv[0]);
                vector<JSValue> params;
                if (argc > 2) {
                    params.reserve(argc - 2);
                    for (size_t i = 2; i < argc; ++i) {
                        params.push_back(argv[i].toValue(ctx));
                    }
                }

                if (JS_HasProperty(context, value, atom)) {
                    JSValue ret = JS_Invoke(context, value, atom, (int)params.size(), params.data());
                    for (auto it = params.begin(), _e = params.end(); it != _e; ++it) {
                        JS_FreeValue(context, *it);
                    }
                    if (JS_IsException(ret)) {
                        JSValue ex = JS_GetException(context);
                        QJS_PrintError(ctx, ex);
                        JS_FreeValue(context, ex);
                        results[0].setNull();
                    } else {
                        results[0].set(ctx, ret);
                        ctx->temp_results.push_back(ret);
                    }
                } else {
                    results[0].setNull();
                }
            }
        }
            break;
        case SharpGetAtom: {
            if (argc >= 2 && is_object_type(argv[0]) && argv[1].type == ITEM_TYPE_INT) {
                uint32_t atom = argv[1].i;
                JSValue value = get_value(argv[0]);
                JSValue ret = JS_GetProperty(context, value, atom);
                results[0].set(ctx, ret);
                ctx->temp_results.push_back(ret);
                return 1;
            }
        }
            break;
        case SharpSetAtom: {
            if (argc >= 3 && is_object_type(argv[0]) && argv[1].type == ITEM_TYPE_INT) {
                uint32_t atom = argv[1].i;
                JSValue value = get_value(argv[0]);
                JS_SetProperty(context, value, atom, argv[2].toValue(ctx));
                return 0;
            }
        }
            break;
        case SharpInvoke: {
            if (argc >= 2 && is_object_type(argv[0])) {
                JSValue value = get_value(argv[0]);
                if (JS_IsFunction(context, value)) {
                    vector<JSValue> params;
                    if (argc > 2) {
                        params.reserve(argc - 2);
                        for (size_t i = 2; i < argc; ++i) {
                            params.push_back(argv[i].toValue(ctx));
                        }
                    }
                    
                    JSValue this_value = argv[1].type == ITEM_TYPE_VALUE ? ((QJS_Value *)argv[1].p)->value : JS_NULL;
                    JSValue ret = JS_Call(context, value, this_value, (int)params.size(), params.data());
                    for (auto it = params.begin(), _e = params.end(); it != _e; ++it) {
                        JS_FreeValue(context, *it);
                    }
                    if (JS_IsException(ret)) {
                        JSValue ex = JS_GetException(context);
                        QJS_PrintError(ctx, ex, "[Invoke]");
                        JS_FreeValue(context, ex);
                    } else {
                        results[0].set(ctx, ret);
                        ctx->temp_results.push_back(ret);
                        return 1;
                    }
                }
            }
        }
            break;
        case SharpNew: {
            if (argc >= 1 && is_object_type(argv[0])) {
                JSValue value = get_value(argv[0]);
                if (JS_IsConstructor(context, value)) {
                    vector<JSValue> params;
                    if (argc > 1) {
                        params.reserve(argc - 1);
                        for (size_t i = 1; i < argc; ++i) {
                            params.push_back(argv[i].toValue(ctx));
                        }
                    }
                    JSValue ret = JS_CallConstructor(context, value, (int)params.size(), params.data());
                    for (auto it = params.begin(), _e = params.end(); it != _e; ++it) {
                        JS_FreeValue(context, *it);
                    }
                    if (JS_IsException(ret)) {
                        JSValue ex = JS_GetException(context);
                        QJS_PrintError(ctx, ex, "[New]");
                        JS_FreeValue(context, ex);
                    } else {
                        results[0].set(ctx, ret);
                        ctx->temp_results.push_back(ret);
                        return 1;
                    }
                } else {
                    QJS_Error(ctx->runtime, "Target is not a constructor");
                }
            }
        }
            break;
        case SharpNewArray: {
            JSValue ret = JS_NewArray(context);
            results[0].set(ctx, ret);
            ctx->temp_results.push_back(ret);
            return 1;
        }
        case SharpNewObject: {
            JSValue ret = JS_NewObject(context);
            results[0].set(ctx, ret);
            ctx->temp_results.push_back(ret);
            return 1;
        }
        case SharpNewBind: {
            if (argc >= 2 && is_object_type(argv[0]) && argv[1].type == ITEM_TYPE_INT) {
                JSValue value = get_value(argv[0]);
                if (JS_IsConstructor(context, value)) {
                    JSValue idValue = JS_NewInt32(context, argv[1].i);
                    JSValue params[2] {
                        ctx->init_object,
                        idValue
                    };
                    JSValue ret = JS_CallConstructor(context, value, 2, params);
                    JS_FreeValue(context, idValue);
                    if (JS_HasProperty(context, ret, ctx->private_key)) {
                        results[0].set(ctx, ret);
                        JSValue value = JS_GetProperty(context, ret, ctx->private_key);
                        results[1].set(ctx, value);
                        if (JS_IsException(ret)) {
                            JSValue ex = JS_GetException(context);
                            QJS_PrintError(ctx, ex, "[NewBind]");
                            JS_FreeValue(context, ex);
                        }
                        const char *str = JS_ToCString(context, value);
                        JS_FreeCString(context, str);
                        
                        ctx->temp_results.push_back(ret);
                        ctx->temp_results.push_back(value);
                        return 2;
                    } else {
                        QJS_Error(ctx->runtime, "Target is not a bindability object");
                        JS_FreeValue(context, ret);
                    }
                } else {
                    QJS_Error(ctx->runtime, "Target is not a constructor");
                }
            } else {
                QJS_Error(ctx->runtime, "[NewBind] Wrong arguments");
            }
        }
            break;
            
        case SharpGetIndex: {
            if (argc >= 2 && is_object_type(argv[0]) && argv[1].type == ITEM_TYPE_INT) {
                JSValue value = get_value(argv[0]);
                JSValue ret = JS_GetPropertyUint32(context, value, argv[1].i);
                results[0].set(ctx, ret);
                ctx->temp_results.push_back(ret);
                return 1;
            }
            break;
        }
        case SharpSetIndex: {
            if (argc >= 3 && is_object_type(argv[0]) && argv[1].type == ITEM_TYPE_INT) {
                JSValue value = get_value(argv[0]);
                JS_SetPropertyUint32(context, value, argv[1].i, argv[2].toValue(ctx));
            }
            break;
        }
        case SharpPromiseComplete: {
            if (argc >= 2 && argv[0].type == ITEM_TYPE_INT && argv[1].type == ITEM_TYPE_INT) {
                int id = argv[0].i, state = argv[1].i;
                auto it = ctx->promise_map.find(id);
                if (it != ctx->promise_map.end()) {
                    QJS_Promise *pro = it->second;
                    if (JS_IsUndefined(pro->success)) {
                        pro->state = state;
                    } else {
                        if (state == 1) {
                            JSValue n = JS_NULL;
                            JSValue ret = JS_Call(context, pro->success, n, 1, &n);
                            if (JS_IsException(ret)) {
                                JSValue ex = JS_GetException(context);
                                QJS_PrintError(ctx, ex);
                                JS_FreeValue(context, ex);
                            } else {
                                while (JS_IsJobPending(ctx->runtime->runtime)) {
                                    JSContext *context;
                                    JS_ExecutePendingJob(ctx->runtime->runtime, &context);
                                }
                            }
                        } else {
                            JSValue n = JS_NULL;
                            JSValue ret = JS_Call(context, pro->failed, n, 1, &n);
                            if (JS_IsException(ret)) {
                                JSValue ex = JS_GetException(context);
                                QJS_PrintError(ctx, ex);
                                JS_FreeValue(context, ex);
                            } else {
                                while (JS_IsJobPending(ctx->runtime->runtime)) {
                                    JSContext *context;
                                    JS_ExecutePendingJob(ctx->runtime->runtime, &context);
                                }
                            }
                        }
                        pro->free(context);
                        delete pro;
                        ctx->promise_map.erase(it);
                    }
                }
            }
            break;
        }
            
        default: {
            QJS_Error(ctx->runtime, "Unsupport action:%d", type);
        }
            break;
    }
    return 0;
}

void QJS_Eval(QJS_Context *ctx, const char *code, const char *filename) {
    QJS_Item *results = ctx->results;
//    QJS_Log(ctx->runtime, "%d %x", ctx->operator_set_atom, ctx->context);
    JSValue val = JS_Eval(ctx->context, code, strlen(code), filename, JS_EVAL_TYPE_GLOBAL);
    if (JS_IsException(val)) {
        JSValue ex = JS_GetException(ctx->context);
        QJS_PrintError(ctx, ex, "[Eval]");
        JS_FreeValue(ctx->context, ex);
        results[0].setNull();
    } else {
        results[0].set(ctx, val);
        ctx->temp_results.push_back(val);
    }
}

void QJS_Load(QJS_Context *ctx, const char *code, const char *filename) {
    QJS_Item *results = ctx->results;
    string strcode(code);
    
    if (!has_export(strcode)) {
        stringstream ss;
        ss << "const module = {exports: {}}; let exports = module.exports;" << endl;
        ss << code << endl;
        ss << "export default module.exports;" << endl;
        strcode = ss.str();
    }
    
    JSContext *context = ctx->context;
    
    JSValue ret = JS_Eval(context, strcode.c_str(), strcode.size(), filename, JS_EVAL_TYPE_MODULE | JS_EVAL_FLAG_COMPILE_ONLY);
    if (JS_IsException(ret)) {
        JSValue ex = JS_GetException(ctx->context);
        QJS_PrintError(ctx, ex, "[Load]");
        JS_FreeValue(context, ex);
        results[0].setNull();
    } else {
        int tag = JS_VALUE_GET_TAG(ret);
        if (tag == JS_TAG_MODULE) {
            JSValue val = JS_EvalFunction(context, ret);
            if (JS_IsException(val)) {
                JSValue ex = JS_GetException(ctx->context);
                QJS_PrintError(ctx, ex, "[Load]");
                JS_FreeValue(context, ex);
                results[0].setNull();
            } else {
                JSModuleDef *module = (JSModuleDef *)JS_VALUE_GET_PTR(ret);
                JSValue data = JS_GetModuleDefault(ctx->context, module);
                results[0].set(ctx, data);
                JS_FreeValue(context, data);
                JS_FreeValue(context, val);
            }
        } else {
            results[0].setNull();
        }
    }
}

uint32_t QJS_NewAtom(QJS_Context *ctx, const char *str) {
    return JS_NewAtom(ctx->context, str);
}

void QJS_FreeAtom(QJS_Context *ctx, uint32_t atom) {
    JS_FreeAtom(ctx->context, atom);
}

void QJS_ToString(QJS_Context *ctx, QJS_Value *value, char *output) {
    size_t len;
    const char * str = JS_ToCStringLen(ctx->context, &len, value->value);
    if (len >= 256) {
        memcpy(output, str, 255);
        output[255] = 0;
    } else {
        memcpy(output, str, len);
        output[len] = 0;
    }
}

bool QJS_Equals(QJS_Value *value1, QJS_Value *value2) {
    return is_equal(value1->value, value2->value);
}

bool QJS_IsInt8Array(QJS_Context *ctx, QJS_Value *value) {
    return JS_IsInt8Array(ctx->context, value->value);
}
bool QJS_IsUint8Array(QJS_Context *ctx, QJS_Value *value) {
    return JS_IsUint8Array(ctx->context, value->value);
}
bool QJS_IsInt16Array(QJS_Context *ctx, QJS_Value *value) {
    return JS_IsInt16Array(ctx->context, value->value);
}
bool QJS_IsUint16Array(QJS_Context *ctx, QJS_Value *value) {
    return JS_IsUint16Array(ctx->context, value->value);
}
bool QJS_IsInt32Array(QJS_Context *ctx, QJS_Value *value) {
    return JS_IsInt32Array(ctx->context, value->value);
}
bool QJS_IsUint32Array(QJS_Context *ctx, QJS_Value *value) {
    return JS_IsUint32Array(ctx->context, value->value);
}
bool QJS_IsInt64Array(QJS_Context *ctx, QJS_Value *value) {
    return JS_IsInt64Array(ctx->context, value->value);
}
bool QJS_IsUint64Array(QJS_Context *ctx, QJS_Value *value) {
    return JS_IsUint64Array(ctx->context, value->value);
}

bool QJS_IsFloat32Array(QJS_Context *ctx, QJS_Value *value) {
    return JS_IsFloat32Array(ctx->context, value->value);
}
bool QJS_IsFloat64Array(QJS_Context *ctx, QJS_Value *value) {
    return JS_IsFloat64Array(ctx->context, value->value);
}
bool QJS_IsFunction(QJS_Context *ctx, QJS_Value *value) {
    return JS_IsFunction(ctx->context, value->value);
}
bool QJS_IsFunctionPtr(QJS_Context *ctx, void *p) {
    return JS_IsFunction(ctx->context, JS_MKPTR(JS_TAG_OBJECT, p));
}

int QJS_GetFunctionLength(QJS_Context *ctx, QJS_Value *value) {
    JSContext *context = ctx->context;
    JSValue val = JS_GetPropertyStr(context, value->value, "length");
    int len = 0;
    JS_ToInt32(context, &len, val);
    JS_FreeValue(context, val);
    return len;
}

int QJS_GetFunctionLengthPtr(QJS_Context *ctx, void *p) {
    JSContext *context = ctx->context;
    JSValue value = JS_MKPTR(JS_TAG_OBJECT, p);
    JSValue val = JS_GetPropertyStr(context, value, "length");
    int len = 0;
    JS_ToInt32(context, &len, val);
    JS_FreeValue(context, val);
    return len;
}

uint32_t QJS_GetTypedArrayLength(QJS_Context *ctx, QJS_Value *value) {
    return JS_GetTypedArrayLength(ctx->context, value->value);
}

bool QJS_CopyArrayBuffer(QJS_Context *ctx, QJS_Value *value, void *dis, uint32_t offset, uint32_t length) {
    size_t byte_offset, byte_length, bytes_per_element;
    JSValue buffer = JS_GetTypedArrayBuffer(ctx->context, value->value, &byte_offset, &byte_length, &bytes_per_element);
    
    size_t size, doffset = offset * bytes_per_element, dlength = min(length * bytes_per_element, byte_length - doffset);
    uint8_t *buf = JS_GetArrayBuffer(ctx->context, &size, buffer);
    bool ret;
    if (buf) {
        memcpy(dis, buf + doffset, dlength);
        ret = true;
    } else {
        QJS_Error(ctx->runtime, "Can not copy array");
        ret = false;
    }
    JS_FreeValue(ctx->context, buffer);
    return ret;
}

const char * QJS_ToStringPtr(QJS_Context *ctx, void *p) {
    JSValue val = JS_MKPTR(JS_TAG_STRING, p);
    return JS_ToCString(ctx->context, val);
}

void QJS_FreeStringPtr(QJS_Context *ctx, const char * ptr) {
    JS_FreeCString(ctx->context, ptr);
}

QJS_Value *QJS_RetainInstance(QJS_Context *ctx, QJS_Instance *instance) {
    QJS_Value *ret = new QJS_Value{
        .value = JS_DupValue(ctx->context, instance->value),
        .free = true
    };
    ctx->cache.insert(ret);
    return ret;
}

QJS_Value *QJS_RetainClass(QJS_Context *ctx, QJS_Class *clazz) {
    QJS_Value *ret = new QJS_Value{
        .value = JS_DupValue(ctx->context, clazz->func),
        .free = true
    };
    ctx->cache.insert(ret);
    return ret;
}

QJS_Value *QJS_RetainValue(QJS_Context *ctx, void *ptr, int *sharp_id, int *sharp_type) {
    JSValue value = JS_MKPTR(JS_TAG_OBJECT, ptr);
    JSContext *context = ctx->context;
    if (*sharp_id == 0 && JS_HasProperty(context, value, ctx->toUnity_key)) {
        JSValue target = JS_Invoke(context, value, ctx->toUnity_key, 0, nullptr);
        if (JS_IsException(target)) {
            JSValue ex = JS_GetException(context);
            QJS_PrintError(ctx, ex);
            JS_FreeValue(context, ex);
        } else {
            if (JS_HasProperty(context, target, ctx->private_key)) {
                JSValue data = JS_GetProperty(context, target, ctx->private_key);
                int64_t ptr;
                if (JS_ToBigInt64(context, &ptr, data) == 0) {
                    QJS_Instance *ins = (QJS_Instance *)ptr;
                    *sharp_id = ins->sharp_id;
                    *sharp_type = 1;
                }
                JS_FreeValue(context, data);
            } else if (JS_HasProperty(context, target, ctx->class_private_key)) {
                JSValue data = JS_GetProperty(context, target, ctx->private_key);
                int64_t ptr;
                if (JS_ToBigInt64(context, &ptr, data) == 0) {
                    QJS_Class *clazz = (QJS_Class *)ptr;
                    *sharp_id = clazz->sharp_id;
                    *sharp_type = 2;
                }
                JS_FreeValue(context, data);
            }
            QJS_Value *ret = new QJS_Value{
                .value = target,
                .free = true
            };
            ctx->cache.insert(ret);
            return ret;
        }
    }
    QJS_Value *ret = new QJS_Value{
        .value = JS_DupValue(ctx->context, value),
        .free = true
    };
    ctx->cache.insert(ret);
    return ret;
}

void QJS_NewPromise(QJS_Context *ctx, int promise) {
    if (ctx->promise_map.count(promise) > 0) {
        QJS_Error(ctx->runtime, "Promise is exist");
        return;
    }
    JSContext *context = ctx->context;
    JSValue ctor = JS_GetPromiseConstructor(context);
    QJS_Promise *pro = new QJS_Promise();
    JSValue data = JS_NewBigInt64(context, (int64_t)pro);
    JSValue func = JS_NewCFunctionData(context, promise_init, 2, 0, 1, &data);
    JS_FreeValue(context, data);
    JSValue value = JS_CallConstructor(context, ctor, 1, &func);
    JS_FreeValue(context, func);
    if (JS_IsException(value)) {
        delete pro;
        JSValue ex = JS_GetException(context);
        QJS_PrintError(ctx, ex);
        JS_FreeValue(context, ex);
    } else {
        pro->value = value;
        ctx->promise_map[promise] = pro;
    }
}

void QJS_Step(QJS_Context *ctx) {
    while (JS_IsJobPending(ctx->runtime->runtime)) {
        JSContext *context;
        JS_ExecutePendingJob(ctx->runtime->runtime, &context);
    }
}

void *QJS_Malloc(QJS_Context *ctx, int size) {
    return js_malloc(ctx->context, size);
}

}
