﻿// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework.Api;
using NUnit.Framework.Extensibility;
using NUnit.Framework.Internal;

namespace NUnit.Framework.Builders
{
    /// <summary>
    /// Class used by NUnitLite to create test fixtures from Types
    /// </summary>
    public class NUnitLiteTestFixtureBuilder : ISuiteBuilder
    {
        private NUnitLiteTestCaseBuilder testCaseBuilder = new NUnitLiteTestCaseBuilder();

        /// <summary>
        /// Determines whether this instance can build a fixture from the specified type.
        /// </summary>
        /// <param name="type">The type to use as a fixture</param>
        /// <returns>
        /// 	<c>true</c> if this instance can build from the specified type; otherwise, <c>false</c>.
        /// </returns>
        public bool CanBuildFrom(Type type)
        {
            if (type.IsAbstract && !type.IsSealed)
                return false;

            if (type.IsDefined(typeof(TestFixtureAttribute), true))
                return true;

            if (!type.IsPublic && !type.IsNestedPublic)
                return false;

            foreach (MethodInfo method in type.GetMethods())
            {
                if (method.IsDefined(typeof(TestAttribute), true) ||
                    method.IsDefined(typeof(TestCaseAttribute), true))
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Builds a fixture from the specified Type.
        /// </summary>
        /// <param name="type">The type to use as a fixture.</param>
        /// <returns></returns>
        public Test BuildFrom(Type type)
        {
            TestFixture suite = new TestFixture(type);

            suite.ApplyCommonAttributes(type);

            //object[] attrs = type.GetCustomAttributes(typeof(PropertyAttribute), true);
            //foreach (PropertyAttribute attr in attrs)
            //    foreach( DictionaryEntry entry in attr.Properties )
            //        suite.Properties[entry.Key] = entry.Value;

            //IgnoreAttribute ignore = (IgnoreAttribute)Reflect.GetAttribute(type, typeof(IgnoreAttribute), false);
            //if (ignore != null)
            //{
            //    suite.RunState = RunState.Ignored;
            //    suite.IgnoreReason = ignore.GetReason();
            //}

            // Note: Type.EmptyTypes is not supported on all CLR versions we support.
            if (type.GetConstructor(new Type[0]) == null)
            {
                suite.RunState = RunState.NotRunnable;
                suite.Properties.Set(PropertyNames.SkipReason, string.Format("Class {0} has no default constructor", type.Name));
                return suite;
            }

            foreach (MethodInfo method in type.GetMethods())
            {
                if (testCaseBuilder.CanBuildFrom(method))
                    suite.Add(testCaseBuilder.BuildFrom(method));
            }

            return suite;
        }
    }
}