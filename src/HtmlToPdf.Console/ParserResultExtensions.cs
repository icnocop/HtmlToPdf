// <copyright file="ParserResultExtensions.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdf.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// Parser Result Extension Methods
    /// </summary>
    internal static class ParserResultExtensions
    {
        /// <summary>
        /// Gets the AutoBuild help text from the result.
        /// </summary>
        /// <typeparam name="T">The type of options</typeparam>
        /// <returns>
        /// The AutoBuild help text in its string representation, or an empty string.
        /// </returns>
        internal static string GetAutoBuildHelpText<T>()
        {
            var typeInfo = GetTypeInfoInstance<T>();
            if (typeInfo != null)
            {
                var missingRequiredOptionError = GetMissingRequiredOptionError();
                var notParsed = GetNotParsedInstanceAs<T>(typeInfo, new[] { missingRequiredOptionError });
                if (notParsed != null)
                {
                    return notParsed.GetHelpText().ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the help text.
        /// </summary>
        /// <typeparam name="T">The parser result type.</typeparam>
        /// <param name="parserResult">The parser result.</param>
        /// <returns>The help text.</returns>
        internal static HelpText GetHelpText<T>(this ParserResult<T> parserResult)
        {
            return HelpText.AutoBuild(parserResult, h =>
            {
                h.AutoHelp = false;
                h.AutoVersion = false;
                return h;
            })
            .AddPreOptionsLines(EmbeddedResource.GetCommandLinePreOptions())
            .AddPostOptionsLines(EmbeddedResource.GetCommandLinePostOptions());
        }

        private static MissingRequiredOptionError GetMissingRequiredOptionError()
        {
            Type type = typeof(MissingRequiredOptionError);
            ConstructorInfo constructor = ((System.Reflection.TypeInfo)type).DeclaredConstructors.Single();
            return constructor.Invoke(new object[] { NameInfo.EmptyName }) as MissingRequiredOptionError;
        }

        private static object GetTypeInfoInstance<T>()
        {
            Type type = typeof(CommandLine.TypeInfo);
            MethodInfo method = type.GetRuntimeMethods().Single(x => (x.Name == "Create") && (x.GetParameters().Length == 1));
            return method.Invoke(null, new object[] { typeof(T) });
        }

        private static ParserResult<T> GetNotParsedInstanceAs<T>(object typeInfo, IEnumerable<Error> errors)
        {
            Type genericType = typeof(CommandLine.NotParsed<T>);
            System.Reflection.TypeInfo genericTypeInfo = genericType.GetTypeInfo();
            IEnumerable<ConstructorInfo> constructors = genericTypeInfo.DeclaredConstructors;
            ConstructorInfo firstConstructor = constructors.First();
            return firstConstructor.Invoke(new object[] { typeInfo, errors }) as ParserResult<T>;
        }
    }
}
