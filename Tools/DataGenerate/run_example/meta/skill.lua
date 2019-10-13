configName = "skill"
configAlias = "技能"
tables = {
	{
		name = "ItemBook", type = "TABLE2", alias = "百家之书",
		items = {
			{ name = "itemFloat", type = "float", isList=true, listSize = 5, alias = "字段1" },
		},
	},
	
	{
		name = "ITEM_TYPE", type = "ENUM",  alias = "物品类型",
		items = {
			{ name = "ITEM_TYPE_AA", alias = "BB字段类型" },
			{ name = "ITEM_TYPE_BB", alias = "AA字段类型" },
		},
	},
}
return _G