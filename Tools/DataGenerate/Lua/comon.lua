configName = "common"
configAlias = "ͨ��"
tables = {
	SkillDefine = 
		type = "TABLE", alias = "���ܱ�",
		items = {
			{ name = "section", type = "float", isList=true, alias = "�ֶ�1" },
		},
	},
	
	SKILL_TYPE = {
		type = "ENUM",  alias = "��������",
		items = {
			{ name = "SKILL_TYPE_BB", alias = "BB�ֶ�����" },
			{ name = "SKILL_TYPE_AA", alias = "AA�ֶ�����" },
		},
	},
}
return _G