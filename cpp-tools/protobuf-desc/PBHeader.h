#pragma once
#pragma once
#include <vector>
#include <string>

namespace google {
	namespace protobuf {
		class Descriptor;
		class FieldDescriptor;
		class FileDescriptor;
		class Reflection;
		class DescriptorPool;
		class Message;
		class EnumValueDescriptor;
		class EnumDescriptor;
		class DynamicMessageFactory;
		namespace compiler {
			class DiskSourceTree;
			class Importer;
			class MultiFileErrorCollector;
		}
	}
}

namespace BestanDesc
{
	typedef ::google::protobuf::Message Message;
	typedef ::google::protobuf::Descriptor Descriptor;
	typedef ::google::protobuf::FieldDescriptor FieldDescriptor;

	//void split(const std::string& str, const std::string& pattern, std::vector<std::string>& result);
	//std::vector<std::string> split(const std::string& str, const std::string& pattern);


#define sizeofarray(arr) (sizeof(arr) / sizeof(arr[0]))

#define SAFE_DELETE(x) if ((x) != nullptr) { delete (x); (x) = nullptr; }
#define SAFE_DELETE_ARRAY(x) if ((x) != nullptr) { delete[] (x); (x) = nullptr; }

}