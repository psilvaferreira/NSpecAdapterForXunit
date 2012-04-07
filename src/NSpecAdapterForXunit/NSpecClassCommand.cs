/* Copyright (c) 2012 Pedro Ferreira
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
 * THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace NSpecAdapterForXunit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NSpec;
    using Xunit.Sdk;

    /// <summary>
    /// Implements an <see cref="T:ITestClassCommand"/> that can create commands to all examples contained in a given specification class.
    /// </summary>
    public class NSpecClassCommand : ITestClassCommand
    {
        /// <summary>
        /// Gets the object instance that is under test. May return null if you wish
        /// the test framework to create a new object instance for each test method.
        /// </summary>
        public object ObjectUnderTest
        {
            get
            {
                return null; 
            }
        }

        /// <summary>
        /// Gets or sets the type that is being tested
        /// </summary>
        public ITypeInfo TypeUnderTest
        {
            get;
            set;
        }

        /// <summary>
        /// Allows the test class command to choose the next test to be run from the list of
        /// tests that have not yet been run, thereby allowing it to choose the run order.
        /// </summary>
        /// <param name="testsLeftToRun">The tests remaining to be run</param>
        /// <returns>
        /// The index of the test that should be run
        /// </returns>
        public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
        {
            return 0;
        }

        /// <summary>
        /// Execute actions to be run after all the test methods of this test class are run.
        /// </summary>
        /// <returns>
        /// Returns the <see cref="T:System.Exception"/> thrown during execution, if any; null, otherwise
        /// </returns>
        public Exception ClassFinish()
        {
            return null;
        }

        /// <summary>
        /// Execute actions to be run before any of the test methods of this test class are run.
        /// </summary>
        /// <returns>
        /// Returns the <see cref="T:System.Exception"/> thrown during execution, if any; null, otherwise
        /// </returns>
        public Exception ClassStart()
        {
            return null;
        }

        /// <summary>
        /// Enumerates the test commands for a given test method in this test class.
        /// </summary>
        /// <param name="testMethod">The method under test</param>
        /// <returns>
        /// The test commands for the given test method
        /// </returns>
        public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
        {
            if (testMethod == null)
            {
                throw new ArgumentNullException("testMethod");
            }

            return ExampleCommand.CreateForMethod(testMethod);
        }

        /// <summary>
        /// Enumerates the methods which are test methods in this test class.
        /// </summary>
        /// <returns>
        /// The test methods
        /// </returns>
        public IEnumerable<IMethodInfo> EnumerateTestMethods()
        {
            //must inherit from nspec
            if (!this.TypeUnderTest.Type.IsSubclassOf(typeof(nspec)))
            {
                return Enumerable.Empty<IMethodInfo>();
            }

            var bindingFlags = 
                BindingFlags.DeclaredOnly |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance;

            //all instance methods that are declared in the type
            return this.TypeUnderTest.Type
                .GetMethods(bindingFlags)
                .Select(m => Xunit.Sdk.Reflector.Wrap(m));
        }

        /// <summary>
        /// Determines if a given <see cref="T:Xunit.Sdk.IMethodInfo"/> refers to a test method.
        /// </summary>
        /// <param name="testMethod">The test method to validate</param>
        /// <returns>
        /// True if the method is a test method; false, otherwise
        /// </returns>
        public bool IsTestMethod(IMethodInfo testMethod)
        {
            if (testMethod == null)
            {
                throw new ArgumentNullException("testMethod");
            }

            return testMethod.Class.Type.IsSubclassOf(typeof(nspec));
        }
    }
}
