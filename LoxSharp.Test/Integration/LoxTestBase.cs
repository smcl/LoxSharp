using System;
using System.IO;
using Xunit;

namespace LoxSharp.Test.Integration
{
    public abstract class LoxTestBase
    {
        private readonly Lox _lox;
        private readonly TextWriter _stdOut;
        private readonly TextWriter _stdErr;

        protected abstract string Source { get; }
        protected virtual string[] ExpectedStdOutLines { get => new string[] { string.Empty }; }
        protected virtual string[] ExpectedStdErrLines { get => new string[] { string.Empty }; }

        public LoxTestBase()
        {
            _stdOut = new StringWriter();
            _stdErr = new StringWriter();
            _lox = new Lox(_stdOut, _stdErr);
        }

        [Fact]
        public void Execute()
        {
            _lox.Run(Source);
            CompareOutput(_stdErr, ExpectedStdErrLines);
            CompareOutput(_stdOut, ExpectedStdOutLines);
        }

        private void CompareOutput(TextWriter actualOutputWriter, string[] expectedOutputLines)
        {
            var actualOutputLines = actualOutputWriter.ToString().TrimEnd().Split(Environment.NewLine);

            Assert.Equal(expectedOutputLines.Length, actualOutputLines.Length);

            for (var i = 0; i < actualOutputLines.Length; i++)
            {
                Assert.Equal(expectedOutputLines[i], actualOutputLines[i]);
            }
        }
    }
}
