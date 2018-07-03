local user = userManager.create("Foo", "Bar")
local User = getmetatable(user).__index

function User:say(message)
	return self.firstname .. " says: " .. message
end

assert.Equal("Foo says: Hello!", user:say("Hello!"))