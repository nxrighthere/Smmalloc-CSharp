cmake_minimum_required(VERSION 2.6)
project(smmalloc CXX)

set(SMMALLOC_STATIC "0" CACHE BOOL "Create a static library")
set(SMMALLOC_SHARED "0" CACHE BOOL "Create a shared library")
set(SMMALLOC_STATS "0" CACHE BOOL "Add support for stats gathering")

set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} -static")

include_directories(${PROJECT_SOURCE_DIR})

if (SMMALLOC_STATS)
    add_definitions(-DSMMALLOC_STATS_SUPPORT)
endif()

if (SMMALLOC_STATIC)
    add_library(smmalloc_static STATIC smmalloc.cpp)

    if (WIN32)
        target_link_libraries(smmalloc_static)
    endif()
endif()

if (SMMALLOC_SHARED)
    add_library(smmalloc SHARED smmalloc.cpp)

    if (WIN32)
        target_link_libraries(smmalloc)
    endif()
endif()