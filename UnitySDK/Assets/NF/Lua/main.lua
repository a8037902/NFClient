--require 'test'
print('main: this source was signatured!')

--local test_module
ScriptList={
	{tbl=nil, tblName="test_module"},
	--{tbl=nil, tblName="baijiale_game_module"},
	--{tbl=nil, tblName="test_game_module"},
	--{tbl=nil, tblName="test_pb_module"},
	--{tbl=nil, tblName="test_pb_msg_module"},
	--{tbl=nil, tblName="test_http_module"},
}

function module_load()
	for i=1, #(ScriptList) do
		ScriptList[i].tbl = require(ScriptList[i].tblName)
	end
end

module_load()

function init()
    print("main init")
	for i=1, #(ScriptList) do
		--test_module:init()
		ScriptList[i].tbl:init()
	end
end

function afterinit()
    print("main afterinit")
	for i=1, #(ScriptList) do
		ScriptList[i].tbl:afterinit()
	end
end

function execute()
	for i=1, #(ScriptList) do
		ScriptList[i].tbl:execute()
	end
end

function BeforeShut()
    print("main BeforeShut")
	for i=1, #(ScriptList) do
		ScriptList[i].tbl:BeforeShut()
	end
end

function Shut()
    print("main Shut")
	for i=1, #(ScriptList) do
		ScriptList[i].tbl:Shut()
	end
end
