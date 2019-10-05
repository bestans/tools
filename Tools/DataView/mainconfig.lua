local config =
{
	--搜索下拉菜单
	searchMenus = {
		"menu1",
		"menu2",
		"menu3",
	},
	--数据列
	dataGridColumns = {
		{ title="title1", bindKey="FirstName", },
		{ title="title2", bindKey="FirstName", },
		{ title="title3", bindKey="FirstName", },
	},
	--数据格子宽度
	dataGridWidth = 100,
	--数据列最大宽度额外值
	dataGridExtraMaxWidth = 25,
	--数据页签宽度
	tableItemWidth = 60,
	--左右箭头页签宽度
	tableArrowItemWidth = 20,
	--数据页签余留宽度
	tableItemExtraWidth = 5,
	--主界面最小宽度
	winMinWidth = 200,
	--主界面最小高度
	winMinHeight = 100,
}

return config