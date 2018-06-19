function startsWith(start, str)
   return string.sub(str, 1, string.len(start)) == start
end

-- ToString
assert.Equal("10", tostring(10))
assert.Equal("foo", tostring("foo"))
assert.True(startsWith("function: 0x", tostring(function() end)))
assert.True(startsWith("table: 0x", tostring({})))

-- ToNumber
assert.Equal(10, tonumber(10))
assert.Equal(15, tonumber("15"))
assert.Equal(1, tonumber({"a"}))
assert.Equal(nil, tonumber(function() end))

-- Type
assert.Equal("table", type({}))
assert.Equal("string", type(""))
assert.Equal("number", type(0))
assert.Equal("function", type(function() end))

-- Raw*
local tbl = {}
assert.Equal(0, rawlen(tbl))
rawset(tbl, "foo", "bar")
assert.Equal("bar", rawget(tbl, "foo"))
assert.Equal(1, rawlen(tbl))
assert.True(rawequal(1, 1))