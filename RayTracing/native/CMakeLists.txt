cmake_minimum_required(VERSION 3.10)
project(rt)

set(CMAKE_CXX_STANDARD 17)

include_directories(.)
include_directories(external)

add_library(rt SHARED export.cpp)

if(WIN32)
    set_target_properties(rt PROPERTIES PREFIX "")
endif()

set_target_properties(rt PROPERTIES 
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}"
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}"
)
