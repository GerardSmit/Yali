using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yali.Extensions;
using Yali.Native;
using Yali.Native.Proxy;
using Yali.Native.Value;
using Yali.Native.Value.Functions;
using Yali.Runtime;
using Yali.Runtime.Ast;

namespace Yali
{
    public class Engine
    {
        private readonly LuaParser _parser = new LuaParser();
        private readonly StatementInterpreter _statementInterpreter;
        private readonly ExpressionIntepreter _expressionIntepreter;
        private readonly ConcurrentDictionary<Type, LuaTable> _proxyTables;
        private readonly ConcurrentDictionary<Type, LuaTable> _metaTables;

        public Engine()
        {
            Global = new LuaTable();
            StringMetaTable = new LuaTable();
            _statementInterpreter = new StatementInterpreter(this);
            _expressionIntepreter = new ExpressionIntepreter(this);
            _proxyTables = new ConcurrentDictionary<Type, LuaTable>();
            _metaTables = new ConcurrentDictionary<Type, LuaTable>();

            Global.NewIndexRaw("_G", Global);
        }

        public static Engine Create()
        {
            return new Engine();
        }

        public static Engine CreateDefault()
        {
            return Create()
                .AddLuaLibrary()
                .AddStringLibrary()
                .AddMathLibrary()
                .AddBit32Library();
        }

        public LuaTable StringMetaTable { get; set; }

        public LuaTable Global { get; }

        public LuaTable GetProxyTable(Type type)
        {
            return _proxyTables.GetOrAdd(type, t =>
            {
                var cache = LuaProxyCache.Get(t);
                var table = new LuaTable();

                foreach (var kv in cache.StaticProperties)
                {
                    // TODO: Implement set.
                    table.NewIndexRaw(kv.Key, LuaObject.FromObject(kv.Value.Info.GetValue(null)));
                }

                foreach (var kv in cache.Methods)
                {
                    table.NewIndexRaw(kv.Key, kv.Value);
                }
                
                return table;
            });
        }

        public LuaTable GetProxyMetaTable(Type type)
        {
            return _metaTables.GetOrAdd(type, t =>
            {
                var mt = new LuaTable();
                mt.NewIndexRaw("__index", GetProxyTable(t));

                if (typeof(ICallableProxy).IsAssignableFrom(t))
                {
                    mt.NewIndexRaw("__call", LuaCallableFunction.Instance);
                }

                return mt;
            });
        }

        public Task<LuaArguments> ExecuteAsync(string str, CancellationToken token = default)
        {
            return ExecuteAsync(str, Lua.Args(), token);
        }

        public Task<LuaArguments> ExecuteAsync(string str, LuaArguments args, CancellationToken token = default)
        {
            return ExecuteAsync(Parse(str), args, token);
        }

        public Task<LuaArguments> ExecuteAsync(LuaFunction func, CancellationToken token = default)
        {
            return func.CallAsync(this, Lua.Args(), token);
        }

        public Task<LuaArguments> ExecuteAsync(LuaFunction func, LuaArguments args, CancellationToken token = default)
        {
            return func.CallAsync(this, args, token);
        }

        public LuaFunction Parse(string str)
        {
            var functionDefinition = new FunctionDefinition
            {
                Arguments = new List<Argument>(),
                Body = _parser.ParseString(str),
                Varargs = true
            };

            return new LuaInterpreterFunction(this, functionDefinition, Global, true);
        }

        public Task ExecuteStatement(IStatement stat, LuaState state, CancellationToken token = default)
        {
            if (state.FunctionState.DidReturn || token.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            switch (stat)
            {
                case Assignment assignment:
                    return _statementInterpreter.ExecuteAssignment(assignment, state, token);
                case LocalAssignment localAssignment:
                    return _statementInterpreter.ExecuteLocalAssignment(localAssignment, state, token);
                case FunctionCall call:
                    return _statementInterpreter.ExecuteFunctionCall(call, state, token);
                case Block block:
                    return _statementInterpreter.ExecuteBlock(block, state, token);
                case IfStat ifStat:
                    return _statementInterpreter.ExecuteIfStat(ifStat, state, token);
                case ReturnStat returnStat:
                    return _statementInterpreter.ExecuteReturnStat(returnStat, state, token);
                case WhileStat whileStat:
                    return _statementInterpreter.ExecuteWhileStat(whileStat, state, token);
                case RepeatStat repeatStat:
                    return _statementInterpreter.ExecuteRepeatStat(repeatStat, state, token);
                case GenericFor genericFor:
                    return _statementInterpreter.ExecuteGenericFor(genericFor, state, token);
                case NumericFor numericFor:
                    return _statementInterpreter.ExecuteNumericFor(numericFor, state, token);
                default:
                    throw new NotImplementedException(stat.GetType().Name);
            }
        }

        public async Task<LuaArguments> EvaluateExpression(IList<IExpression> exprs, LuaState state, CancellationToken token = default)
        {
            var results = new List<LuaObject>();

            for (var i = 0; i < exprs.Count; i++)
            {
                var addAll = i == exprs.Count - 1;
                var objects = await EvaluateExpression(exprs[i], state, token);

                if (!addAll)
                {
                    results.Add(objects[0]);
                }
                else if (objects.Length == 0)
                {
                    results.Add(LuaNil.Instance);
                }
                else
                {
                    results.AddRange(objects);
                }
            }

            return Lua.Args(results);
        }

        public Task<LuaArguments> EvaluateExpression(IExpression expr, LuaState state, CancellationToken token = default)
        {
            switch (expr)
            {
                case Runtime.Ast.NumberLiteral numberLiteral:
                    return Lua.ArgsAsync(LuaObject.FromNumber(numberLiteral.Value));
                case Runtime.Ast.StringLiteral stringLiteral:
                    return Lua.ArgsAsync(LuaObject.FromString(stringLiteral.Value));
                case BoolLiteral literal:
                    return Lua.ArgsAsync(LuaObject.FromBool(literal.Value));
                case NilLiteral _:
                    return Lua.ArgsAsync(LuaNil.Instance);
                case BinaryExpression binaryExpression:
                    return _expressionIntepreter.EvaluateBinaryExpressionAsync(binaryExpression, state, token);
                case UnaryExpression expression:
                    return _expressionIntepreter.EvaluateUnaryExpression(expression, state, token);
                case Variable variable:
                    return _expressionIntepreter.EvaluateGetVariableAsync(variable, state, token);
                case TableAccess access:
                    return _expressionIntepreter.EvaluateTableAccess(access, state, token);
                case FunctionCall call:
                    return _expressionIntepreter.EvaluateFunctionCall(call, state, token);
                case FunctionDefinition definition:
                    return _expressionIntepreter.EvaluateFunctionDefinition(definition, state, token);
                case VarargsLiteral _:
                    return _expressionIntepreter.EvaluateVarargs(state);
                case TableConstructor constructor:
                    return _expressionIntepreter.EvaluateTableConstructor(constructor, state, token);
                default:
                    throw new NotImplementedException(expr.GetType().Name);
            }
        }
    }
}

