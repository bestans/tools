#pragma once
#include "stdafx.h"
#include "PBHeader.h"
#include "template_base.pb.h"

using namespace std;
using namespace bestan::common::config;

namespace BestanDesc
{
	void split(const std::string& str, const std::string& pattern, std::vector<std::string>& result);
	std::vector<std::string> split(const std::string& str, const std::string& pattern);

	const string NAME_PATTERN = "#";
	const string TYPE_PATTERN = "_ARRAY";
	static const char* LINE_SIGN = "\n";
	static const char* PROTO_FILE = "all.proto.dat";

	enum DATA_TYPE
	{
		DATA_INVALID,
		DATA_INT,
		DATA_LONG,
		DATA_STRING,
		DATA_STRUCT,
		DATA_FLOAT,
		DATA_DOUBLE,
	};
	static map<DATA_TYPE, string> type2PBType = {
		{ DATA_INT, "int32" },
		{ DATA_LONG, "int64" },
		{ DATA_STRING, "bytes" },
		{ DATA_FLOAT, "float" },
		{ DATA_FLOAT, "double" },
	};
	static map<string, DATA_TYPE> typeMap = {
		{ "INT", DATA_INT },
		{ "LONG", DATA_LONG },
		{ "STRING", DATA_STRING },
		{ "ST", DATA_STRUCT },
		{ "FLOAT", DATA_FLOAT },
		{ "DOUBLE", DATA_DOUBLE },
	};
	struct DataDesc
	{
		std::vector<std::string> path;
		DATA_TYPE dataType = DATA_INVALID;
		bool isArray = false;
	};

	struct DataTypeKey
	{
		std::string typeName;
		int index = 0;
		bool isArray = false;
		DATA_TYPE dataType = DATA_INVALID;

		DataTypeKey(string typeNameP, int indexP, bool isArrayArg, DATA_TYPE dataTypeArg)
		{
			typeName = typeNameP;
			index = indexP;
			isArray = isArrayArg;
			dataType = dataTypeArg;
		}
		bool operator < (const DataTypeKey& other) const
		{
			if (typeName != other.typeName) return typeName < other.typeName;

			if (index != other.index) return index < other.index;

			return false;
		}

		string GetRealSection()
		{
			string s = typeName;
			if (dataType == DATA_STRUCT) {
				s += "_info";
			}
			if (isArray) {
				s += "_list";
			}
			return s;
		}
	};
	struct DataTypeDesc;
	struct DataTypeDesc
	{
		std::string typeFullName;
		std::vector<std::string> path;
		bool isArray = false;
		int arrayIndex = -1;
		DATA_TYPE dataType = DATA_INVALID;
		//std::vector<std::string, DataTypeDesc> list;

		map<DataTypeKey, DataTypeDesc> dataMap;
	};

	struct DataStruct
	{
		DATA_TYPE dataType = DATA_INVALID;
		bool isArray = false;
		map<string, DataStruct> subStructMap;
		string desc;

		DataStruct()
		{

		}
		DataStruct(DATA_TYPE dataTypeArg) {
			dataType = dataTypeArg;
		}
		DataStruct(DATA_TYPE dataTypeArg, bool isArrayArg) {
			dataType = dataTypeArg;
			isArray = isArrayArg;
		}
	};
	enum SEARCH_RESULT
	{
		SEARCH_FAILED,
		SEARCH_NOT_FOUND,
		SEARCH_HAS_USED,
		SEARCH_SUCCESS,
	};

	class ParseManager
	{
	private:
		string msgName;
		map<string, DataStruct> dataStructMap;
		map<DataTypeKey, DataTypeDesc> structMap;
		vector<vector<DataTypeKey>> indexPathAll;
		map<int, DATA_TYPE> index2Type;
		excel_table table_data;

		std::map<std::string, std::vector<DataDesc>> descMap;
		std::map<std::string, std::map<int, Message*>> dataMap;
		std::vector<DataDesc> currentStack;

	public:
		bool BeginParse(const string& inputFileName);
		bool BeginMessage(string msgName, const vector<string>& nameList, const vector<string>& typeList, const vector<string>& descList);
		bool AddLineData(int line, const vector<string>& dataList);
		bool EndMessage();

		bool ParsePath(excel_path_full& path);
		bool ParseDataStruct(stringstream& ss, const string& msgName, map<string, DataStruct>& struMap, int level);
		DATA_TYPE GetDataType(const string& section);
	public:
		static ParseManager& GetInstance() 
		{
			static ParseManager instance;
			return instance;
		}
	private:
		bool WriteProto(const string& proto);
	};

	class FileManager
	{
	public:
		static bool WriteContent(string fileName, string content, bool isAppend = true);
		static bool WriteBinary(string fileName, string content, bool isAppend = false);
	};

	class Utils
	{
	public:
		//转小写
		static void ToLowwer(string& str);
		//转大写
		static void ToUpper(string& str);
	};
}