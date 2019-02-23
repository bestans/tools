#include "stdafx.h"
#include "dll_export.h"

using namespace BestanDesc;

DLLEXPORT int test()
{
	return 10;
}

DLLEXPORT int test2(int size, int* sizeList, char** strList)
{
	cout << "size=" << size << endl;
	for (int i = 0; i < size; ++i) {
		int count = sizeList[i];
		cout << "connt=" << count << endl;
		string str(strList[i], count);
		cout << str << endl;
	}
	return 11;
}

DLLEXPORT int test3(int size, void* data)
{
	cout << "size=" << size << endl;
	char* str = new char[size + 1];
	memset(str, 0, size + 1);
	memcpy(str, data, size);
	cout << str << endl;
	return 13;
}

DLLEXPORT int test4(int size, int* unitSize, char* data)
{
	cout << "size=" << size;
	cout << "data" << endl;
	for (int i = 0; i < size; ++i)
	{
		cout << unitSize[i] << endl;
	}
	cout << data << endl;
	return 14;
}

DLLEXPORT bool BeginMessage(int msgSize, char* msgName, int dataSize, int* dataUnit, char* data)
{
	vector<string> nameList, typeList, descList;
	string msg(msgName, msgSize);
	cout << "msg=" << msg << endl;
	int count = dataSize / 3;
	if (dataSize % 3 != 0 || count <= 0) {
		cout << "数据大小不是3的倍数：dataSize=" << dataSize << endl;
		return false;
	}
	vector<string>* pList[] = {
		&descList,
		&nameList,
		&typeList,
	};
	int curSize = 0;
	cout << "dataSize=" << dataSize << endl;
	for (int i = 0; i < dataSize; i += 1) {
		int tempSize = dataUnit[i];
		string tempStr = string(data + curSize, tempSize);
		cout << tempStr << endl;
		pList[i / count]->push_back(tempStr);
		curSize += tempSize;
	}
	cout << "curSize=" << curSize << endl;

	return ParseManager::GetInstance().BeginMessage(msg, nameList, typeList, descList);
}

DLLEXPORT bool AddLineData(int line, int dataSize, int* dataUnit, char* data)
{
	vector<string> dataList;

	int curSize = 0;
	for (int i = 0; i < dataSize; ++i) {
		int tempSize = dataUnit[i];
		string tempStr(data + curSize, tempSize);
		curSize += tempSize;
		cout << tempStr << endl;
		dataList.push_back(tempStr);
	}
	return ParseManager::GetInstance().AddLineData(line, dataList);
}

DLLEXPORT bool EndMessage()
{
	return ParseManager::GetInstance().EndMessage();
}
