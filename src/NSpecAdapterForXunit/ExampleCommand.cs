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
    using System.Xml;
    using NSpec;
    using NSpec.Domain;
    using Xunit.Sdk;

    /// <summary>
    /// Implements a <see cref="T:Xunit.Sdk.ITestCommand"/> that invokes all NSpec <see cref="T:NSpec.Domain.Example"/> in one method.
    /// </summary>
    internal class ExampleCommand : ITestCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:NSpecAdapterForxUnit.ExampleCommand"/> to a specific <see cref="T:NSpec.Domain.Example"/>.
        /// </summary>
        /// <param name="method">The method that contains the <paramref name="example"/>.</param>
        /// <param name="example">The example to run.</param>
        /// <param name="instance">The instance to run the example.</param>
        /// <exception cref="T:System.ArgumentNullException">One of the arguments is <see langword="null"/>. </exception>
        public ExampleCommand(IMethodInfo method, Example example, nspec instance)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (example == null)
            {
                throw new ArgumentNullException("example");
            }

            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            this.Method = method;
            this.Example = example;
            this.Instance = instance;
        }

        /// <summary>
        /// Gets the method that contains the <see cref="P:Example"/>.
        /// </summary>
        /// <value>The method that contains the <see cref="P:Example"/>.</value>
        public IMethodInfo Method
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="T:NSpec.Domain.Example"/> to run.
        /// </summary>
        /// <value>The <see cref="T:NSpec.Domain.Example"/> to run.</value>
        public Example Example
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="NSpec.nspec"/> instance to run the example.
        /// </summary>
        /// <value>The <see cref="NSpec.nspec"/> instance to run the example.</value>
        public nspec Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the display name of the test method.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return Example.FullName();
            }
        }

        /// <summary>
        /// Determines if the test runner infrastructure should create a new instance of the
        /// test class before running the test.
        /// </summary>
        public bool ShouldCreateInstance
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if the test should be limited to running a specific amount of time
        /// before automatically failing.
        /// </summary>
        /// <returns>The timeout value, in milliseconds; if zero, the test will not have
        /// a timeout.</returns>
        public int Timeout
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Creates the start XML to be sent to the callback when the test is about to start
        /// running.
        /// </summary>
        /// <returns>
        /// Return the <see cref="T:System.Xml.XmlNode"/> of the start node, or null if the test
        /// is known that it will not be running.
        /// </returns>
        public XmlNode ToStartXml()
        {
            return new XmlDocument().CreateElement("NSpec");
        }

        /// <summary>
        /// Executes the test method.
        /// </summary>
        /// <param name="testClass">The instance of the test class</param>
        /// <returns>
        /// Returns information about the test run
        /// </returns>
        public MethodResult Execute(object testClass)
        {
            //run the example
            try
            {
                this.Example.Context.Exercise(this.Example, this.Instance);
                if (this.Example.Passed) //all good. the test passed.
                {
                    return new PassedResult(
                        this.Method.Name, //methodName
                        this.Method.TypeName, //typeName
                        this.DisplayName, //displayName
                        null); //traits
                }
                else if (this.Example.Failed()) //oh noes!
                {
                    var ex = this.Example.Exception; //TODO: is this always != null?
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }

                    return new FailedResult(
                        this.Method.Name, //methodDate
                        this.Method.TypeName, //typeName
                        this.DisplayName, //displayName
                        null, //traits
                        ex.GetType().Name, //exceptionType
                        ex.Message, //message
                        ex.StackTrace); //stackTrace
                }
                else //TODO: this should be pending examples
                {
                    //TODO: how to mark the test as inconclusive?
                    return new SkipResult(
                        this.Method.Name, //methodName
                        this.Method.TypeName, //typeName
                        this.DisplayName, //displayName
                        null, //traits
                        "Pending"); //reason
                }
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                return new FailedResult(
                    this.Method.Name, //methodDate
                    this.Method.TypeName, //typeName
                    this.DisplayName, //displayName
                    null, //traits
                    ex.GetType().Name, //exceptionType
                    ex.Message, //message
                    ex.StackTrace); //stackTrace
            }
        }

        /// <summary>
        /// Obtains all test commands to call all examples contained in a given <see cref="T:IMethodInfo"/>.
        /// </summary>
        /// <param name="method">The method to scan.</param>
        /// <returns>All test commands to call all examples contained in <paramref name="method"/>.</returns>
        internal static IEnumerable<ITestCommand> CreateForMethod(IMethodInfo method)
        {
            //create a new instance of our specification
            var instance = method.CreateInstance() as nspec;
            if (instance == null) //not a nspec instance, nothing for us here.
            {
                return Enumerable.Empty<ITestCommand>();
            }

            var context = new MethodContext(method.MethodInfo);

            context.Build(instance);

            var commands = new List<ITestCommand>();

            BuildCommandList(method, instance, context, commands);

            return commands;
        }

        /// <summary>
        /// Recursively adds commands to all examples in a given <see cref="T:Context"/>.
        /// </summary>
        /// <param name="method">The method where the examples belong.</param>
        /// <param name="instance">The <see cref="T:nspec"/> instance.</param>
        /// <param name="context">The parent context.</param>
        /// <param name="commands">A list where the commands will be stored.</param>
        private static void BuildCommandList(IMethodInfo method, nspec instance, Context context, List<ITestCommand> commands)
        {
            foreach (var example in context.Examples)
            {
                commands.Add(new ExampleCommand(method, example, instance));
            }

            foreach (var childContext in context.ChildContexts())
            {
                BuildCommandList(method, instance, childContext, commands);
            }
        }
    }
}
