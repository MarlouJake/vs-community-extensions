using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = GraphQLAnalyzers.Test.CSharpAnalyzerVerifier<
    GraphQLAnalyzers.NoMethodOverloadsAnalyzer>;

namespace GraphQLAnalyzers.Test
{
    [TestClass]
    public class NoMethodOverloadsAnalyzerTests
    {
        [TestMethod]
        public async Task NoOverload_NoDiagnostic()
        {
            var test = @"
            public partial class Query
            {
                public void MethodA() { }
                public void MethodB() { }
            }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task Overload_Detected()
        {
            var test = @"
            public partial class Query
            {
                public void {|#0:MyMethod|}() { }
                public void {|#1:MyMethod|}(int x) { } // overload
            }";

            var expected1 = VerifyCS.Diagnostic("GQLDA001NOL").WithLocation(0).WithArguments("MyMethod");
            var expected2 = VerifyCS.Diagnostic("GQLDA001NOL").WithLocation(1).WithArguments("MyMethod");

            await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
        }


        [TestMethod]
        public async Task NoDiagnosticInNonQueryOrMutation()
        {
            var test = @"
            public partial class OtherClass
            {
                public void MyMethod() { }
                public void MyMethod(int x) { } // no diagnostic because not in Query or Mutation
            }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}
