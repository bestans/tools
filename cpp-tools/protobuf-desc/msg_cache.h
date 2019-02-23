#pragma once
#include <map>
#include "PBHeader.h"
#include <sstream>
#include <iostream>
#include <vector>

namespace BestanDesc
{
	enum EM_CACHE {
		EM_CACHE_MYSQL_PARAM = 0,
		EM_CACHE_MYSQL_RESULT,
		EM_CACHE_COUNT,
	};

	class MessageStubManager
	{
	public:
		typedef std::map<int, const Message*>	 MSG_STUB_MAP;
		typedef std::map<const Descriptor*, int>  DESC_2_TYPE_MAP;

		MessageStubManager();
		~MessageStubManager();

		bool Init();
		const MSG_STUB_MAP& GetMap() { return msg_stub_map; }

	public:
		static MessageStubManager* GetInstance()
		{
			static MessageStubManager instance;
			return &instance;
		}
		Message* CreateMessage(int type);

		int GetMessageType(const Descriptor* desc) 
		{
			if (desc_stub_map.find(desc) == desc_stub_map.end())
			{
				return -1;
			}
			return desc_stub_map[desc];
		}

		const Message* GetMessage(int pb_type) 
		{
			if (msg_stub_map.find(pb_type) != msg_stub_map.end()) {
				return msg_stub_map[pb_type];
			}
			return nullptr;
		}

		const Message* GetMessage(const std::string& templName);

		const Descriptor* GetMessageDesc(int pb_type);
		
	private:
		google::protobuf::compiler::DiskSourceTree* m_DiskTree;
		google::protobuf::compiler::Importer * m_Importer;
		google::protobuf::DynamicMessageFactory* m_MessageFactory;
		google::protobuf::compiler::MultiFileErrorCollector* m_ErrorCollector;

		// msg_type to msg
		MSG_STUB_MAP msg_stub_map;
		// msg_desc to msg_type
		DESC_2_TYPE_MAP desc_stub_map;
	};
}
