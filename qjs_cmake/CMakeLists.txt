cmake_minimum_required(VERSION 3.5)

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
elseif (${CMAKE_SYSTEM_NAME} MATCHES "Win64")
        set(CMAKE_C_COMPILER x86_64-w64-mingw32-gcc)
        set(CMAKE_STRIP x86_64-w64-mingw32-strip)
        set(CMAKE_AR x86_64-w64-mingw32-gcc-ar)
        set(M_FLAG -m64) 
elseif (${CMAKE_SYSTEM_NAME} MATCHES "Win32")
        set(CMAKE_C_COMPILER i686-w64-mingw32-gcc)
        set(CMAKE_STRIP i686-w64-mingw32-strip)
        set(CMAKE_AR i686-w64-mingw32-gcc-ar)
        set(M_FLAG -m32) 
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
elseif((${CMAKE_SYSTEM_NAME} MATCHES "Win64") OR (${CMAKE_SYSTEM_NAME} MATCHES "Win32")) 
        target_link_libraries(
                qjs

                ${M_FLAG}
                -lm -static-libgcc -static-libstdc++ -Wl,-Bstatic -lstdc++ -lpthread -Wl,-Bdynamic
        )
endif()