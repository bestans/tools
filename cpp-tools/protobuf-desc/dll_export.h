#pragma once

#define DLLEXPORT __declspec(dllexport)

extern "C" DLLEXPORT int test();
extern "C" DLLEXPORT int test2(int size, int* sizeList, char** strList);
extern "C" DLLEXPORT int test3(int size, void* data);
extern "C" DLLEXPORT int test4(int size, int* unitSize, char* data);


extern "C" DLLEXPORT bool BeginMessage(int msgSize, char* msgName, int dataSize, int* dataUnit, char* data);
extern "C" DLLEXPORT bool AddLineData(int line, int dataSize, int* dataUnit, char* data);
extern "C" DLLEXPORT bool EndMessage();

