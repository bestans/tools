import sys
import template_base_pb2
import all_config_pb2

dataPath = template_base_pb2.excel_path_full()
dataPB = None

def MakeOneConfig(dataConfig, descList):
	if len(dataPath.paths) != len(descList):
		raise RuntimeError('MakeOneConfig failed:data list size not match:pathSize={},descSize={}'.format(len(dataPath.paths), len(descList)))

	for i in range(len(descList)):
		tempCellPath = dataPath.paths[i]
		tempCellDesc = descList[i]

		tempIndex = 0
		tempData = dataConfig
		while tempIndex < (len(tempCellPath.cell_path) - 1):
			tempSection = tempCellPath.cell_path[tempIndex]
			tempDataType = tempSection.data_type
			if tempSection.index <= 0:
				#not array
				tempData = getattr(tempData, tempSection.section, None)
			else:
				tempData = getattr(tempData, tempSection.section, None)
				while len(tempData) < tempSection.index:
					tempData.add()
				tempData = tempData[tempSection.index - 1]
		
			if tempData == None:
				raise RuntimeError('MakeOneConfig failed:error path info,column={}'.format(i+1))

			tempIndex = tempIndex + 1

		tempSection = tempCellPath.cell_path[tempIndex]
		tempDataType = tempSection.data_type
		if tempDataType == template_base_pb2.excel_section.INT32:
			tempValue = 0
			if tempCellDesc != '':
				tempValue = int(tempCellDesc)
			if tempSection.index <= 0:
				setattr(tempData, tempSection.section, tempValue)
			else:
				tempTempData = getattr(tempData, tempSection.section, None)
				tempTempData.append(tempValue)
		elif tempDataType == template_base_pb2.excel_section.INT64:
			tempValue = 0
			if tempCellDesc != '':
				tempValue = long(tempCellDesc)
			if tempSection.index <= 0:
				setattr(tempData, tempSection.section, tempValue)
			else:
				tempTempData = getattr(tempData, tempSection.section, None)
				tempTempData.append(tempValue)
		elif tempDataType == template_base_pb2.excel_section.STRING:
			if tempSection.index <= 0:
				setattr(tempData, tempSection.section, tempCellDesc)
			else:
				tempTempData = getattr(tempData, tempSection.section, None)
				tempTempData.append(tempCellDesc)
		elif tempDataType == template_base_pb2.excel_section.FLOAT or tempDataType == template_base_pb2.excel_section.DOUBLE:
			tempValue = 0.0
			if tempCellDesc != '':
				tempValue = float(tempCellDesc)
			if tempSection.index <= 0:
				setattr(tempData, tempSection.section, tempValue)
			else:
				tempTempData = getattr(tempData, tempSection.section, None)
				tempTempData.append(tempValue)
		else:
			raise RuntimeError('ParseLineDataError:invalid data type:column={},type={}'.format(i+1, tempDataType))

#begin:improt path and msg name all
def ImportPathData(pathDataDesc, msgNameAll):
	global dataPath
	dataPath.ParseFromString(pathDataDesc)
	tempDataPB = getattr(all_config_pb2, msgNameAll)
	global dataPB
	dataPB = tempDataPB()

#parse line data
def MakeLineData(id, descList):
	global dataPB
	if id in dataPB.configs.keys():
		raise RuntimeError('MakeLineData failed:duplicate id={}'.format(id))

	MakeOneConfig(dataPB.configs[id], descList)

#end:output all config data
def OutputData():
	return dataPB.SerializeToString()

def MakePathData():
	#id
	tempCellPath = dataPath.paths.add()
	tempSection = tempCellPath.cell_path.add()
	tempSection.section = 'id'
	tempSection.index = 0
	tempSection.data_type = template_base_pb2.excel_section.INT32;
	#count
	tempCellPath = dataPath.paths.add()
	tempSection = tempCellPath.cell_path.add()
	tempSection.section = 'count'
	tempSection.index = 0
	tempSection.data_type = template_base_pb2.excel_section.STRING;
	#value1
	tempCellPath = dataPath.paths.add()
	tempSection = tempCellPath.cell_path.add()
	tempSection.section = 'value'
	tempSection.index = 1
	tempSection.data_type = template_base_pb2.excel_section.INT32;
	#value2
	tempCellPath = dataPath.paths.add()
	tempSection = tempCellPath.cell_path.add()
	tempSection.section = 'value'
	tempSection.index = 2
	tempSection.data_type = template_base_pb2.excel_section.INT32;
	#skill-id
	tempCellPath = dataPath.paths.add()
	tempSection = tempCellPath.cell_path.add()
	tempSection.section = 'skill'
	tempSection.index = 1
	tempSection = tempCellPath.cell_path.add()
	tempSection.section = 'id'
	tempSection.index = 0
	tempSection.data_type = template_base_pb2.excel_section.FLOAT;
	#skill-rate
	tempCellPath = dataPath.paths.add()
	tempSection = tempCellPath.cell_path.add()
	tempSection.section = 'skill'
	tempSection.index = 1
	tempSection = tempCellPath.cell_path.add()
	tempSection.section = 'rate'
	tempSection.index = 0
	tempSection.data_type = template_base_pb2.excel_section.INT32;
	#skill2-id
	tempCellPath = dataPath.paths.add()
	tempSection = tempCellPath.cell_path.add()
	tempSection.section = 'skill'
	tempSection.index = 2
	tempSection = tempCellPath.cell_path.add()
	tempSection.section = 'id'
	tempSection.index = 0
	tempSection.data_type = template_base_pb2.excel_section.FLOAT;
	#skill2-rate
	tempCellPath = dataPath.paths.add()
	tempSection = tempCellPath.cell_path.add()
	tempSection.section = 'skill'
	tempSection.index = 2
	tempSection = tempCellPath.cell_path.add()
	tempSection.section = 'rate'
	tempSection.index = 0
	tempSection.data_type = template_base_pb2.excel_section.INT32;

def Test():
	tempDataPB = getattr(template_base_pb2, "test_python_all")
	dataPB=tempDataPB()
	MakePathData()
	descList = ["7","aa6","5","4","3.111","2","10.22","11"]
	MakeLineData(1, descList)
