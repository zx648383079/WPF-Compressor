using System;
using EcmaScript.NET;

namespace ZoDream.Compressor.Helper.Compressor
{
    public class JavaScriptIdentifier : JavaScriptToken
    {
        public JavaScriptIdentifier(string value,
                                    ScriptOrFunctionScope declaredScope) : base(Token.NAME,
                                                                                value)
        {
            MarkedForMunging = true;
            DeclaredScope = declaredScope;
        }

        public int RefCount { get; set; }
        public String MungedValue { get; set; }
        public ScriptOrFunctionScope DeclaredScope { get; set; }
        public bool MarkedForMunging { get; set; }
    }
}