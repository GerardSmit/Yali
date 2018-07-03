local user = userManager.create("Foo", "Bar")

assert.Equal("Foo", user.firstname)
assert.Equal("Hello Foo Bar", user:greet())
assert.Throws(function() user.greet() end)