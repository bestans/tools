--自定义表格名
customConfigs = {
	"skill",
}

--lua定义表格所在路径
tableLuaDefinePath = "meta/"

--proto配置所在目录
tableProtoPath = "proto\\"

--excel所在根目录
tableExcelRootPath = "excel"

--excel对应的数据文件所在目录
tableTemplDataPath = "templ/"

--lua表格通用配置文件名
tableLuaCommonName = "common.lua"

--数据配置文件
dataConfigPath = "run_config/table_data.lua"

--表格生成
tableGenerateConfigName = "table_generate"

--基础类型
baseTypes = {
	["int32"] = "整数类型",
	["int64"] = "长整数类型",
	["float"] = "浮点数类型",
	["double"] = "双精度浮点数类型",
	["bool"] = "布尔类型",
	["string"] = "字符串类型",
}
--定义枚举描述
enumTypeDesc = "枚举类型"

--excel属性配置
excelPropConfigPath = "run_config/excel_config.lua"

return _G