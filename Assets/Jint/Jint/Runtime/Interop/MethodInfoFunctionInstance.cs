﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jint.Native;
using Jint.Native.Function;

namespace Jint.Runtime.Interop
{
    public sealed class MethodInfoFunctionInstance : FunctionInstance
    {
        private readonly MethodInfo[] _methods;

        public MethodInfoFunctionInstance(Engine engine, MethodInfo[] methods)
            : base(engine, null, null, false)
        {
            _methods = methods;
            Prototype = engine.Function.PrototypeObject;
        }

        public override JsValue Call(JsValue thisObject, JsValue[] arguments)
        {
            return Invoke(_methods, thisObject, arguments);
        }

        public JsValue Invoke(MethodInfo[] methodInfos, JsValue thisObject, JsValue[] jsArguments)
        {
            var arguments = ProcessParamsArrays(jsArguments, methodInfos);
            var methods = TypeConverter.FindBestMatch(Engine, methodInfos, arguments);
            var converter = Engine.ClrTypeConverter;

            foreach (var method in methods)
            {
                var parameters = new object[arguments.Length];
                var argumentsMatch = true;

                for (var i = 0; i < arguments.Length; i++)
                {
                    var parameterType = method.GetParameters()[i].ParameterType;

                    if (parameterType == typeof(JsValue))
                    {
                        parameters[i] = arguments[i];
                    }
                    else
                    {
                        if (!converter.TryConvert(arguments[i].ToObject(), parameterType, CultureInfo.InvariantCulture, out parameters[i]))
                        {
                            argumentsMatch = false;
                            break;
                        }

                        var lambdaExpression = parameters[i] as LambdaExpression;
                        if (lambdaExpression != null)
                        {
                            parameters[i] = lambdaExpression.Compile();
                        }
                    }
                }

                if (!argumentsMatch)
                {
                    continue;
                }

                // todo: cache method info
                return JsValue.FromObject(Engine, method.Invoke(thisObject.ToObject(), parameters.ToArray()));
            }

            var methodName = methodInfos.Length > 0 ? methodInfos[0].Name : "?";

            var argumentLog = new string[arguments.Length];
            for (int i = 0, len = arguments.Length; i < len; i++)
            {
                argumentLog[i] = arguments[i].ToString();
            }

            throw new JavaScriptException(
                Engine.TypeError,
                string.Format("No public method found for {0}::{1}({2})",
                thisObject.ToObject().GetType().Name,
                methodName,
                string.Join(", ", argumentLog)));
        }

        private JsValue[] ProcessParamsArrays(JsValue[] jsArguments, IEnumerable<MethodInfo> methodInfos)
        {
            foreach (var methodInfo in methodInfos)
            {
                var parameters = methodInfo.GetParameters();
#if NETFX_CORE
                if (!parameters.Any(p => p.IsDefined(typeof(ParamArrayAttribute))))
                    continue;
#else
                if (!parameters.Any(p => Attribute.IsDefined(p, typeof(ParamArrayAttribute))))
                    continue;
#endif

                var nonParamsArgumentsCount = parameters.Length - 1;
                if (jsArguments.Length < nonParamsArgumentsCount)
                    continue;

                var newArgumentsCollection = jsArguments.Take(nonParamsArgumentsCount).ToList();
                var argsToTransform = jsArguments.Skip(nonParamsArgumentsCount).ToList();

                if (argsToTransform.Count == 1 && argsToTransform.FirstOrDefault().IsArray())
                    continue;

                var jsArray = Engine.Array.Construct(Arguments.Empty);
                Engine.Array.PrototypeObject.Push(jsArray, argsToTransform.ToArray());

                newArgumentsCollection.Add(new JsValue(jsArray));
                return newArgumentsCollection.ToArray();
            }

            return jsArguments;
        }

    }
}
