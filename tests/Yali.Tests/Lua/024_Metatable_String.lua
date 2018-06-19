local mt = getmetatable("")

mt.__call = function()
    return "works"
end

local str = "test"
assert.Equal("works", str())

mt.__call = nil