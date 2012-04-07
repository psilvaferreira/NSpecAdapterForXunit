#NSpec adapter for xUnit#

Allows you to run [NSpec](http://nspec.org "NSpec") specifications with the [xUnit](https://xunit.codeplex.com/ "xUnit") test runner.

Each [NSpec](http://nspec.org "NSpec") example will be treated as a separate Test Method.

## Install ##
<pre class="nuget-button">Install-Package NSpecAdapterForXunit</pre>

## Usage ##

Decorate your [NSpec](http://nspec.org "NSpec") specification class with the `RunWithNSpecAttribute` and you're set.

Alternatevly, if you want to run just one method with [xUnit](https://xunit.codeplex.com/ "xUnit"), decorate only that method with `SpecificationAttribute`

## Sample ##

    namespace NSpecAdapterForXunit.Sample
    {
	    using System;
	    using NSpec;
	
	    [RunWithNSpec]
	    public class Sample1 : nspec
	    {
	        void given_a_valid_date()
	        {
	            DateTime date = DateTime.Now;
	
	            before = () =>
	            {
	                date = DateTime.Now;
	            };
	
	            it["month should be greater than or equal to 1"] = () =>
	            {
	                date.Month.should_be_greater_or_equal_to(1);
	            };
	
	            it["month should be less than or equal to 12"] = () =>
	            {
	                date.Month.should_be_less_or_equal_to(12);
	            };
	
	            context["in a crazy world"] = () =>
	            {
	                it["day should be less than zero"] = () =>
	                {
	                    date.Day.should_be_less_than(0);
	                };
	            };
	        }
	    }
	}

	C:\Test> xunit.console.clr4.exe NSpecAdapterForXunit.Sample.dll

	xUnit.net console test runner (64-bit .NET 4.0.30319.261)
	Copyright (C) 2007-11 Microsoft Corporation.

	xunit.dll:     Version 1.9.0.1566
	Test assembly: NSpecAdapterForXunit.Sample.dll

	given a valid date. in a crazy world. day should be less than zero [FAIL]
     Expected: less than 0
     But was:  7
	Stack Trace:
      at NUnit.Framework.Assert.That(Object actual, IResolveConstraint expression, String message, Object[] args)
      at NUnit.Framework.Assert.Less(IComparable arg1, IComparable arg2)
      at NSpec.AssertionExtensions.should_be_less_than(IComparable arg1, IComparable arg2)
      Sample1.cs(32,0): at NSpecAdapterForXunit.Sample.Samle1.<>c__DisplayClass5.<given_a_valid_date>b__4()
      at NSpec.Domain.Example.Run(nspec nspec)
      at NSpec.Domain.Context.RunAndHandleException(Action`1 action, nspec nspec, Exception& exceptionToSet)

	3 total, 1 failed, 0 skipped, took 0.421 seconds

<script type="text/javascript">
    (function () {
        var nb = document.createElement('script'); nb.type = 'text/javascript'; nb.async = true;
        nb.src = 'http://s.prabir.me/nuget-button/0.1/nuget-button.min.js';
        var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(nb, s);
    })();
</script>
