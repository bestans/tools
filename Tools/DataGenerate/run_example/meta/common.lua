configName = "common"
configAlias = "通用"
tables = {
	{
		name = "SkillDefine", type = "TABLE", alias = "技能表",
		items = {
			{ name = "section", type = "float", isList=true, listSize = 2, alias = "字段" },
		},
	},
	
	{
		name = "SKILL_TYPE", type = "ENUM",  alias = "技能类型",
		items = {
			{ name = "SKILL_TYPE_BB", alias = "BB字段类型" },
			{ name = "SKILL_TYPE_AA", alias = "AA字段类型" },
		},
	},
}
return _G