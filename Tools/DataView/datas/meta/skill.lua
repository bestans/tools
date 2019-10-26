configName = "skill"
configAlias = "技能"
tables = {
	{
		name = "ITEM_TYPE", type = "ENUM",  alias = "物品类型",
		items = {
			{ name = "ITEM_TYPE_AA", alias = "BB字段类型" },
			{ name = "ITEM_TYPE_BB", alias = "AA字段类型" },
			{ name = "ITEM_TYPE_CC", alias = "CC字段类型" },
		},
	},
	{
		name = "ItemAttr", type = "BEAN", alias = "物品类型",
		items = {
			{ name = "attrId", type = "int32", alias = "属性id" },
			{ name = "attrValue", type = "int32", alias = "属性值" },
		},
	},
	{
		name = "ItemBook", type = "TABLE", alias = "百家之书",
		items = {
			{ name = "itemFloat", type = "float", isList=true, listSize = 5, alias = "字段" },
			{ name = "itemType", type = "ITEM_TYPE", alias = "物品类型222" },
		},
	},
}
return _G