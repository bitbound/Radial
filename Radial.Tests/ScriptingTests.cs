using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Radial.Models;
using Radial.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radial.Tests
{
    [TestClass]
    public class ScriptingTests
    {
        [TestMethod]
        public async Task Test()
        {
            var globalObj = new ScriptingGlobals()
            {
                PC = new PlayerCharacter()
                {
                    Name = "Dude"
                }
            };

            var options = ScriptOptions.Default
                .WithAllowUnsafe(true)
                .AddReferences(typeof(Program).Assembly)
                .AddImports(typeof(PlayerCharacter).Namespace);

            var result = await CSharpScript.RunAsync("return PC.Name;", options, globalObj, typeof(ScriptingGlobals));
            var returnValue = result.ReturnValue;

            Assert.AreEqual("Dude", returnValue);

            result = await result.ContinueWithAsync("DoStuff(); return PC.ChargeCurrent;", options);
            returnValue = result.ReturnValue;
            Assert.AreEqual((long)1, returnValue);

            result = await result.ContinueWithAsync("DoStuff(); return PC.ChargeCurrent;", options);
            returnValue = result.ReturnValue;
            Assert.AreEqual((long)2, returnValue);
            Assert.AreEqual(2, globalObj.PC.ChargeCurrent);

        }

        public class ScriptingGlobals
        {
            public PlayerCharacter PC { get; set; } = new();

            public void DoStuff()
            {
                PC.ChargeCurrent++;
            }
        }
    }
}
