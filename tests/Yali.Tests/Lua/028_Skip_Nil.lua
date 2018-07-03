local t = {
	[1] = 1,
	[2] = nil,
	[3] = 3
}
local found = {}

for k, v in pairs(t) do 
	found[k] = k == v
end

assert.Equal(2, #t)
assert.True(found[1])
assert.False(found[2])
assert.True(found[3])