local str = "ABCDE"

-- Self test
assert.Equal(string.byte, str.byte)
assert.Equal(65, str:byte(1));

-- Byte
assert.Equal(65, string.byte("ABCDE"));
assert.Equal(nil, string.byte("ABCDE", 0));
assert.Equal(nil, string.byte("ABCDE", 100));

-- Char
assert.Equal("ABC", string.char(65, 66, 67))

-- Find
assert.Equal(7, string.find("Hello Lua user", "Lua"))
assert.Equal(nil, string.find("Hello Lua user", "banana"))

assert.Equal(7, string.find("Hello Lua user", "Lua", 1))
assert.Equal(nil, string.find("Hello Lua user", "Lua", 8))
assert.Equal(13, string.find("Hello Lua user", "e", -5))

assert.Equal(10, string.find("Hello Lua user", "%su"))
assert.Equal(nil, string.find("Hello Lua user", "%su", 1, true))

-- Match
assert.Equal("2 questions", string.match("I have 2 questions for you.", "%d+ %a+"))

-- Lower
assert.Equal("abc", string.lower("ABC"))

-- Upper
assert.Equal("ABC", string.upper("abc"))

-- Len
assert.Equal(3, string.len("abc"))

-- Gsub
assert.Equal("Hello Lua user", string.gsub("Hello banana", "banana", "Lua user"))
assert.Equal("bAnAna", string.gsub("banana", "a", "A", 2))

assert.Equal("ban-an-a", string.gsub("banana", "(an)", "%1-"))
assert.Equal("bAnAnA", string.gsub("banana", "(a)", string.upper))
assert.Equal("bnanaa", string.gsub("banana", "(a)(n)", function(a,b) return b..a end))

-- Sub
assert.Equal("Lua user", string.sub("Hello Lua user", 7))
assert.Equal("Lua lover", string.sub("Hello Lua lover", -9))
assert.Equal("Net", string.sub("Hello Net user", 7, 9))
assert.Equal("C#", string.sub("Hello C# user", -7, -5))

-- Rep
assert.Equal("foofoofoo", string.rep("foo", 3))

-- Patterns
assert.True(string.match("a", "^.$"))
assert.False(string.match("a", "^%.$"))

assert.True(string.match("b", "^%l$"))
assert.False(string.match("b", "^%L$"))

assert.True(string.match("C", "^%u$"))
assert.False(string.match("C", "^%U$"))

assert.True(string.match("d", "^%a$"))
assert.False(string.match("d", "^%A$"))

assert.True(string.match(";", "^%p$"))
assert.False(string.match(";", "^%P$"))

assert.True(string.match("Foo123", "^%w+$"))
assert.False(string.match("Foo123", "^%W+$"))

assert.True(string.match("123", "^%d+$"))
assert.False(string.match("123", "^%D+$"))

assert.True(string.match("   ", "^%s+$"))
assert.False(string.match("   ", "^%S+$"))

assert.True(string.match("00FF00", "^%x+$"))
assert.False(string.match("00FF00", "^%X+$"))

-- E-mail pattern from https://stackoverflow.com/a/30090333/2540618:
local emailPattern = "^[A-Za-z0-9%.]+@[%a%d]+%.[%a%d]+$";
assert.True(string.match("foo@bar.baz", emailPattern))
assert.False(string.match("foo", emailPattern))

-- Format
assert.Equal('Hello "Lua user!"', string.format("%s %q", "Hello", "Lua user!"))
assert.Equal("Lua", string.format("%c%c%c", 76,117,97))
assert.Equal("3.141593e+000, 3.141593E+000", string.format("%e, %E", math.pi,math.pi))
assert.Equal("3.141593, 3.14159", string.format("%f, %g", math.pi,math.pi))
assert.Equal("-100, -100, 4294967196", string.format("%d, %i, %u", -100,-100,-100))
assert.Equal("37777777634, ffffff9c, FFFFFF9C", string.format("%o, %x, %X", -100,-100,-100))