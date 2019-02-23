#include "stdafx.h"
#include "msg_cache.h"
#include "google/protobuf/compiler/importer.h"
#include "google/protobuf/dynamic_message.h"
#include "google/protobuf/compiler/importer.h"
#include "google/protobuf/message.h"
#include "google/protobuf/descriptor.h"
//#include "proto_meta.h"

using namespace ::google::protobuf;
using namespace std;

namespace BestanDesc
{
	std::stringstream error1_stream;
	struct ProtoErrorCollector : google::protobuf::compiler::MultiFileErrorCollector {
		void AddError(
			const string & filename,
			int line,
			int column,
			const string & message)
		{
			error1_stream << "file name:" << filename << ":" << line << " error:" << message << endl;
		}
	};

	MessageStubManager::MessageStubManager()
	{
		m_DiskTree = new google::protobuf::compiler::DiskSourceTree();
		m_DiskTree->MapPath("", ".");
		m_ErrorCollector = new ProtoErrorCollector;
		m_Importer = new google::protobuf::compiler::Importer(m_DiskTree, m_ErrorCollector);
		m_MessageFactory = nullptr;
	}
	MessageStubManager::~MessageStubManager()
	{
		delete m_DiskTree;
		delete m_ErrorCollector;
		delete m_Importer;
// 		if (m_MessageFactory) {
// 			delete m_MessageFactory;
// 		}
	}

	bool MessageStubManager::Init()
	{
		if (m_MessageFactory != nullptr) {
			return false;
		}
// 		static const char* proto_files[] = { "proto/test.proto" };
// 		for (size_t i = 0; i < sizeofarray(proto_files); ++i)
// 		{
// 			auto ret = m_Importer->Import(proto_files[i]);
// 			if (!ret) {
// 				// improt failed
// 				return false;
// 			}
// 		}
// 
// 		m_MessageFactory = new DynamicMessageFactory(m_Importer->pool());
// 
// 		const google::protobuf::DescriptorPool* pDescPool = m_Importer->pool();

// 		auto pFile = m_Importer->Import("test.proto");
// 		//const google::protobuf::DescriptorPool* pDescPool = google::protobuf::DescriptorPool::generated_pool();
// 		//const google::protobuf::FileDescriptor* fd = pDescPool->FindFileByName("test.proto");
// 		//auto pFile = pDescPool->FindFileByName("test.proto");
// 		if (!pFile) {
// 			cout << "找不到文件" << endl;
// 			return false;
// 		}
// 		cout << "msg_count=" << pFile->message_type_count() << endl;
// 		for (int i = 0; i < pFile->message_type_count(); ++i) {
// 			auto pMessage = pFile->message_type(i);
// 			cout << pFile->message_type(i)->full_name() << endl;
// 
// 			auto pMessage = google::protobuf::MessageFactory::generated_factory()->GetPrototype(pFile->message_type(i));
// 			if (!pMessage) {
// 				cout << "找不到message" << endl;
// 				continue;
// 			}
// 		}
// 		for (int i = 0; i < pEnumDesc->value_count(); ++i)
// 		{
// 			const EnumValueDescriptor* evd = pEnumDesc->value(i);
// 			int type = evd->number();
// 			string name = evd->name();
// 			string name2(name);
// 			std::transform(name.begin(), name.end(), name2.begin(), ::tolower);
// 			const Descriptor* d = pDescPool->FindMessageTypeByName(name2);
// 			if (!d) {
// 				return false;
// 			}
// 			
// 			const Message* msg = google::protobuf::MessageFactory::generated_factory()->GetPrototype(d);
// 			if (!msg) {
// 				return false;
// 			}
// 			msg->GetReflection();
// 
// 			if (msg_stub_map.find(type) != msg_stub_map.end()) {
// 				//重复
// 				return false;
// 			}
// 			if (desc_stub_map.find(msg->GetDescriptor()) != desc_stub_map.end()) {
// 				// 重复
// 				return false;
// 			}
// 			msg_stub_map[type] = msg;
// 			desc_stub_map[msg->GetDescriptor()] = type;
// 
// 			cout << "type = " << type << " ,desc = " << (int64)(msg->GetDescriptor());
// 		}

		return true;
	}

	Message* MessageStubManager::CreateMessage(int type) {
		if (msg_stub_map.find(type) != msg_stub_map.end()) {
			return msg_stub_map[type]->New();
		}
		return nullptr;
	}

	const Descriptor* MessageStubManager::GetMessageDesc(int pb_type)
	{
		const Message* pMsg = GetMessage(pb_type);
		if (pMsg != nullptr) {
			return pMsg->GetDescriptor();
		}
		return nullptr;
	}
}
