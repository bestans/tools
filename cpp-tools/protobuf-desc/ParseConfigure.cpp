#include "stdafx.h"
#include "ParseConfigure.h"
#include "msg_cache.h"
#include "google/protobuf/compiler/importer.h"
#include "google/protobuf/dynamic_message.h"
#include "google/protobuf/compiler/importer.h"
#include "google/protobuf/message.h"
#include "google/protobuf/descriptor.h"
#include <fstream>
#include <sstream>

namespace BestanDesc
{

	void split(const std::string& str, const std::string& pattern, std::vector<std::string>& result)
	{
		if (pattern.empty()) {
			result.push_back(str);
			return;
		}

		std::string::size_type pos;
		size_t size = str.size();
		for (size_t i = 0; i < size;)
		{
			pos = str.find(pattern, i);
			size_t count = (pos == std::string::npos ? size - i : pos - i);
			result.push_back(str.substr(i, count));
			i += count + pattern.size();
		}
	}
	std::vector<std::string> split(const std::string& str, const std::string& pattern)
	{
		std::vector<std::string> result;
		split(str, pattern, result);
		return result;
	}

	bool ParseManager::BeginParse(const string& inputFileName)
	{
// 		stringstream ss;
// 
// 		ss << "syntax = \"proto3\";" << LINE_SIGN
// 			<< "package bestan.common.config;" << LINE_SIGN;
// 		if (!FileManager::WriteContent(fileName, ss.str(), false)) {
// 			cout << "��ʼ����ʱ��д�ļ�ʧ��" << endl;
// 			return false;
// 		}

		return true;
	}
	
	DATA_TYPE ParseManager::GetDataType(const string& section)
	{
		auto it = typeMap.find(section);
		if (it == typeMap.end())
			return DATA_INVALID;

		return it->second;
	}

	bool ParseManager::BeginMessage(string inputMsgName, const vector<string>& nameList, const vector<string>& typeList, const vector<string>& descList)
	{
		msgName = inputMsgName;
		Utils::ToLowwer(msgName);
		dataStructMap.clear();
		structMap.clear();
		indexPathAll.clear();
		index2Type.clear();
		table_data.Clear();

		if (nameList.empty() || nameList.size() != typeList.size() || nameList.size() != descList.size())
		{
			cout << "���ͽ�������Ϊ��" << endl;
			return false;
		}

		for (size_t j = 0; j < nameList.size(); ++j)
		{
			indexPathAll.push_back(vector<DataTypeKey>());
			auto& indexPath = indexPathAll[j];
			auto sectionDesc = descList[j];
			auto& section = nameList[j];
			auto sectionType = typeList[j];

			auto curMap = &structMap;
			auto pDataStructMap = &dataStructMap;

			auto sectionTypeSplit = split(sectionType, "$");
			vector<int> indexVec;
			if (sectionTypeSplit.size() == 2) {
				int index = atoi(sectionTypeSplit[1].c_str());
				while (index > 0) {
					int tempIndex = index % 100;
					if (tempIndex <= 0) {
						//��������
						cout << "��������" << endl;
						return false;
					}
					indexVec.insert(indexVec.begin(), tempIndex);
					index = index / 100;
				}
				if (indexVec.empty()) {
					//�����ǿյ�
					cout << "�����ǿյ�" << endl;
					return false;
				}

				sectionTypeSplit = split(sectionTypeSplit[0], "#");
			}
			else if (sectionTypeSplit.size() != 1) {
				cout << "�ֶ����ͷָ���$�ж��" << endl;
				return false;
			}
			else {
				sectionTypeSplit = split(sectionTypeSplit[0], "#");
			}
			auto sectionSplit = split(section, "#");
			if (sectionTypeSplit.empty() || sectionSplit.size() != sectionTypeSplit.size()) {
				//����ͬ�ֶ�������ƥ��
				cout << "�ֶ��������ֶ����ָ�������" << endl;
				return false;
			}

			int usedIndexNum = 0;
			for (size_t k = 0; k < sectionTypeSplit.size(); ++k) {
				auto tempSection = sectionSplit[k];
				Utils::ToLowwer(tempSection);

				auto tempSectionType = sectionTypeSplit[k];
				auto tempSectionTypeSplit = split(tempSectionType, "_ARRAY");
				if (tempSectionTypeSplit.size() != 1) {
					//���ʹ���
					cout << "�ֶ����Ϳ��ܰ�������ȷ��ARRAY" << endl;
					return false;
				}
				bool isArray = (tempSectionTypeSplit[0].length() != tempSectionType.length());
				tempSectionType = tempSectionTypeSplit[0];
				int tempIndex = 0;
				if (isArray) {
					if (usedIndexNum >= (int)indexVec.size()) {
						//������������ȷ
						cout << "�ֶ������������󣬲�ƥ��" << endl;
						return false;
					}
					tempIndex = indexVec[usedIndexNum++];
				}

				//��������
				auto dataType = GetDataType(tempSectionType);
				if (dataType == DATA_INVALID) {
					//�������������
					cout << "�������������:" << sectionDesc << "," << tempSectionType << endl;
					return false;
				}
				auto pTempDataStructMap = pDataStructMap;
				auto temp_data_it = pTempDataStructMap->find(tempSection);
				if (temp_data_it == pTempDataStructMap->end()) {
					//TODO
					auto& tempDataStructMap = *pDataStructMap;
					auto tempStru = DataStruct(dataType, isArray);
					if (k >= sectionTypeSplit.size() - 1)
					{
						tempStru.desc = sectionDesc;
					}
					tempDataStructMap[tempSection] = tempStru;
					pDataStructMap = &tempDataStructMap[tempSection].subStructMap;
				}
				else {
					pDataStructMap = &(temp_data_it->second.subStructMap);
				}

				//��������
				auto& tempDataMap = *curMap;
				DataTypeDesc* pDesc = nullptr;
				DataTypeKey tempKey(tempSection, tempIndex, isArray, dataType);
				auto temp_it = tempDataMap.find(tempKey);
				if (temp_it != tempDataMap.end()) {
					if (k >= sectionTypeSplit.size() - 1)
					{
						//�Ѿ����ظ�������
						cout << "�ֶ����������������ظ���" << endl;
						return false;
					}
					pDesc = &temp_it->second;
				}
				else
				{
					auto& tempDataDesc = tempDataMap[tempKey];
					tempDataDesc.dataType = dataType;
					pDesc = &tempDataDesc;
				}
				curMap = &(pDesc->dataMap);

				if (k == sectionTypeSplit.size() - 1) {
					//���һ��Ԫ��
					if (dataType == DATA_STRUCT) {
						cout << "û��ָ����������" << endl;
						return false;
					}
					index2Type[j] = dataType;
				}
				//����·��
				indexPath.push_back(tempKey);
			}
		}

		stringstream ss;
		if (!ParseDataStruct(ss, msgName, dataStructMap, 0)) {
			return false;
		}
		//дproto�ļ�
		if (!WriteProto(ss.str())) {
			return false;
		}
		ss.clear();

		//����msgName
		table_data.set_proto_msg_name(msgName);

		auto& path = *table_data.mutable_path();
		ParsePath(path);

		return true;
	}

	bool ParseManager::ParsePath(excel_path_full& path)
	{
		for (auto& it : indexPathAll) {
			auto& cell_path = *(path.add_paths());
			for (auto& sectionIt : it) {
				auto& section_path = *cell_path.add_cell_path();
				section_path.set_index(sectionIt.index);
				section_path.set_section(sectionIt.GetRealSection());
			}
		}
		return true;
	}
	void WriteTab(stringstream& ss, int count)
	{
		for (int i = 0; i < count; ++i)
		{
			ss << "	";
		}
	}
	bool ParseManager::ParseDataStruct(stringstream& ss, const string& msgName, map<string, DataStruct>& struMap, int level)
	{
		if (level == 0) {
			ss << LINE_SIGN;
		}
		WriteTab(ss, level);
		ss << "message " << msgName << LINE_SIGN;

		WriteTab(ss, level);
		ss << "{" << LINE_SIGN;

		for (auto& it : struMap) {
			auto& stru = it.second;

			if (stru.dataType != DATA_STRUCT) {
				auto pbtype_it = type2PBType.find(stru.dataType);
				if (pbtype_it == type2PBType.end()) {
					cout << "������������ͣ�" << stru.dataType << endl;
					return false;
				}
				continue;
			}

			//�����ṹ��
			ParseDataStruct(ss, it.first, stru.subStructMap, level + 1);
		}

		int pb_index = 0;
		for (auto& it : struMap) {
			auto& stru = it.second;

			WriteTab(ss, level);
			ss << "	";
			string sectionName = it.first;
			const string* pSection = &(it.first);
			if (stru.dataType != DATA_STRUCT) {
				auto pbtype_it = type2PBType.find(stru.dataType);
				if (pbtype_it == type2PBType.end()) {
					cout << "������������ͣ�" << stru.dataType << endl;
					return false;
				}

				pSection = &(pbtype_it->second);
			}
			else
			{
				sectionName += "_info";
			}

			if (stru.isArray)
			{
				sectionName += "_list";
				ss << "repeated ";
			}

			ss << *pSection << " " << sectionName << " = " << (++pb_index) << ";";
			if (!stru.desc.empty()) {
				ss << " //" << stru.desc;
			}
			ss << LINE_SIGN;
		}

		WriteTab(ss, level);
		ss << "}" << LINE_SIGN;
		return true;
	}

	bool ParseManager::AddLineData(int line, const vector<string>& dataList)
	{
		if (dataList.empty()) {
			cout << "������Ϊ��" << endl;
			return false;
		}
		int index = atoi(dataList[0].c_str());
		auto& table = *table_data.mutable_table();
		auto it = table.find(index);
		if (it != table.end()) {
			cout << "�����ظ�������:line=" << line << ",index=" << index << endl;
			return false;
		}
		auto& line_data = table[index];
		for (size_t i = 0; i < dataList.size(); ++i)
		{
			auto typeIt = index2Type.find(i);
			if (typeIt == index2Type.end()) {
				cout << "�Ҳ����������ͣ�index=" << i << endl;
				return false;
			}
			auto& cell_data = *line_data.add_cell_data();
			auto& dataDesc = dataList[i];
			auto dataType = typeIt->second;
			switch (dataType)
			{
			case BestanDesc::DATA_INT:
				cell_data.set_data_type(excel_cell_data::INT32);
				cell_data.set_int32_value(atoi(dataDesc.c_str()));
				break;
			case BestanDesc::DATA_LONG:
				cell_data.set_data_type(excel_cell_data::INT64);
				cell_data.set_int64_value(atol(dataDesc.c_str()));
				break;
			case BestanDesc::DATA_STRING:
				cell_data.set_data_type(excel_cell_data::STRING);
				cell_data.set_bytes_value(dataDesc);
				break;
			case BestanDesc::DATA_FLOAT:
				cell_data.set_data_type(excel_cell_data::FLOAT);
				cell_data.set_float_value((float)atof(dataDesc.c_str()));
				break;
			case BestanDesc::DATA_DOUBLE:
				cell_data.set_data_type(excel_cell_data::DOUBLE);
				cell_data.set_double_value(atof(dataDesc.c_str()));
				break;
			default:
				cout << "�������������:" << dataType << endl;
				return false;
			}
		}
		return true;
	}

	bool ParseManager::EndMessage()
	{
		if (msgName.empty()) {
			cout << "msgNameΪ��" << endl;
			return false;
		}
		int size = table_data.ByteSize();
		if (size <= 0) {
			cout << "����Ϊ��" << endl;
			return false;
		}

		ofstream outFile(msgName + ".dat", ios::out | ios::binary);
		if (!outFile) {
			cout << "���ļ�" << PROTO_FILE << "ʧ��" << endl;
			return false;
		}
		if (!table_data.SerializePartialToOstream(&outFile)) {
			cout << "��������д��" << PROTO_FILE << "ʧ��" << endl;
			return false;
		}
		outFile.close();
		return true;
	}

	bool ParseManager::WriteProto(const string& proto)
	{
		if (proto.empty()) {
			cout << "proto�����ǿյ�" << endl;
			return false;
		}

		fstream file(PROTO_FILE, ios::out | ios::binary | ios::app);
		if (!file) {
			cout << "���ļ�" << PROTO_FILE << "ʧ��" << endl;
			return false;
		}
		excel_proto proto_data;
		file.seekg(0, file.end);
		size_t size = (size_t)file.tellg();
		file.seekg(0, file.beg);

		auto& all_proto = *proto_data.mutable_all_proto();
		cout << "read proto size=" << size << endl;
		if (size > 0)
		{
			if (!proto_data.ParseFromIstream(&file)) {
				cout << "�ļ�" << PROTO_FILE << "�����𻵣���������" << endl;
				return false;
			}
		}
		file.close();

		ofstream outFile(PROTO_FILE, ios::out | ios::binary);
		if (!outFile) {
			cout << "���ļ�" << PROTO_FILE << "ʧ��" << endl;
			return false;
		}
		all_proto[msgName] = proto;
		proto_data.SerializePartialToOstream(&outFile);
		outFile.close();
		return true;
	}
	bool FileManager::WriteContent(string fileName, string content, bool isAppend /* = true */)
	{
		int mode = ios::out;
		if (isAppend) {
			mode |= ios::app;
		}
		else
		{
			//mode |= ios::_Noreplace;
		}
		ofstream file(fileName, mode);
		if (!file) {
			//���ļ�
			cout << "���ļ�" << fileName << "ʧ�ܣ������Ѿ�����" << endl;
			return false;
		}

		file << content;
		file.close();
		return true;
	}

	bool FileManager::WriteBinary(string fileName, string content, bool isAppend /* = false */)
	{
		int mode = ios::out | ios::binary;
		if (isAppend) {
			mode |= ios::app;
		}
		else
		{
			//mode |= ios::_Noreplace;
		}
		ofstream file(fileName, mode);
		if (!file) {
			//���ļ�
			cout << "���ļ�" << fileName << "ʧ�ܣ������Ѿ�����" << endl;
			return false;
		}

		file << content;
		file.close();
		return true;
	}

	void Utils::ToLowwer(string& str) {
		transform(str.begin(), str.end(), str.begin(), ::tolower);
	}

	void Utils::ToUpper(string& str) {
		transform(str.begin(), str.end(), str.begin(), ::toupper);
	}
}