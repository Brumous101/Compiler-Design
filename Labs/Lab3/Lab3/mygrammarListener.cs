//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\Brumo\Documents\repos\Compiler-Design\Labs\Lab3\Lab3\\mygrammar.txt by ANTLR 4.9.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="mygrammarParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.2")]
[System.CLSCompliant(false)]
public interface ImygrammarListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="mygrammarParser.start"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStart([NotNull] mygrammarParser.StartContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="mygrammarParser.start"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStart([NotNull] mygrammarParser.StartContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="mygrammarParser.stmtList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStmtList([NotNull] mygrammarParser.StmtListContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="mygrammarParser.stmtList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStmtList([NotNull] mygrammarParser.StmtListContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="mygrammarParser.assign"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAssign([NotNull] mygrammarParser.AssignContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="mygrammarParser.assign"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAssign([NotNull] mygrammarParser.AssignContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="mygrammarParser.funcCall"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFuncCall([NotNull] mygrammarParser.FuncCallContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="mygrammarParser.funcCall"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFuncCall([NotNull] mygrammarParser.FuncCallContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="mygrammarParser.numList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNumList([NotNull] mygrammarParser.NumListContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="mygrammarParser.numList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNumList([NotNull] mygrammarParser.NumListContext context);
}