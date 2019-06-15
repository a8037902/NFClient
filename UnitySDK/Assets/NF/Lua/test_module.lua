test_module = {}
print('test_module: this source was signatured!')

function test_module:init()
    print("test_module init")
end

function test_module:afterinit()
    print("test_module afterinit")
end

function test_module:execute()

end

function test_module:BeforeShut()
    print("test_module BeforeShut")
end

function test_module:Shut()
    print("test_module Shut")
end

return test_module