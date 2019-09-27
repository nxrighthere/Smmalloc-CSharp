cmake_minimum_required(VERSION 2.6)
project(smmalloc CXX)

set(SMMALLOC_STATIC "0" CACHE BOOL "Create a static library")
set(SMMALLOC_SHARED "0" CACHE BOOL "Create a shared library")
set(SMMALLOC_STATS "0" CACHE BOOL "Add support for stats gathering")

if (SMMALLOC_STATS)
    add_definitions(-DSMMALLOC_STATS_SUPPORT)
endif()

if (SMMALLOC_STATIC)
    add_library(smmalloc_static STATIC smmalloc.cpp)

    if (NOT LINUX)
        SET_TARGET_PROPERTIES(smmalloc_static PROPERTIES PREFIX "")
    endif()
endif()

if (SMMALLOC_SHARED)
    if (CMAKE_CXX_COMPILER_ID MATCHES GNU)
        set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} -static")
    elseif (CMAKE_CXX_COMPILER_ID MATCHES MSVC)
        set(CMAKE_CXX_FLAGS_RELEASE "/MT")
    elseif (CMAKE_CXX_COMPILER_ID MATCHES Clang)
        set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} -s")
    endif()

    add_library(smmalloc SHARED smmalloc.cpp)

    if (WIN32)
        SET_TARGET_PROPERTIES(smmalloc PROPERTIES PREFIX "")
    endif()
endif()
