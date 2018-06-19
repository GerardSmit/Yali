local t = {}

function t.test()
	return "works"
end

assert.Equal("works", t.test())