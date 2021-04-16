cmake_minimum_required(VERSION 3.19)

set(CMAKE_CXX_STANDARD 14)

project(qjs)

add_definitions(-DCONFIG_VERSION=\"qjs_u\" -DEMSCRIPTEN -DDUMP_LEAKS -DCONFIG_BIGNUM)

include_directories(quickjs)

set(LIB_TYPE SHARED)
if (${CMAKE_SYSTEM_NAME} MATCHES "Android")
elseif (${CMAKE_SYSTEM_NAME} MATCHES "iOS")
        set(LIB_TYPE STATIC)
elseif (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
        set(LIB_TYPE MACOSX_BUNDLE)
endif()

set(SRC 
quickjs/quickjs_ext.c
quickjs/quickjs_in.cpp
quickjs/libbf.c
quickjs/qjscalc.c
quickjs/libregexp.c
quickjs/libunicode.c
quickjs/cutils.c)

if (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
add_executable(
        qjs
        ${LIB_TYPE}

        ${SRC}
)
else() 
add_library(
        qjs
        ${LIB_TYPE}

        ${SRC}
)
endif()

if (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
set_target_properties(qjs PROPERTIES
    BUNDLE True
    MACOSX_BUNDLE_GUI_IDENTIFIER com.qlp.qjs
    MACOSX_BUNDLE_BUNDLE_NAME qjs
    MACOSX_BUNDLE_BUNDLE_VERSION "0.1"
    MACOSX_BUNDLE_SHORT_VERSION_STRING "0.1"
    BUNDLE_EXTENSION "bundle"
)
set(CMAKE_XCODE_ATTRIBUTE_MACH_O_TYPE "mh_bundle")
endif()