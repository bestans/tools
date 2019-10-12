configName = "common"
configAlias = "通用"
tables = {
	SkillDefine = 
		type = "TABLE", alias = "技能表",
		items = {
			{ name = "section", type = "float", isList=true, alias = "字段1" },
		},
	},
	
	SKILL_TYPE = {
		type = "ENUM",  alias = "技能类型",
		items = {
			{ name = "SKILL_TYPE_BB", alias = "BB字段类型" },
			{ name = "SKILL_TYPE_AA", alias = "AA字段类型" },
		},
	},
}
return _G