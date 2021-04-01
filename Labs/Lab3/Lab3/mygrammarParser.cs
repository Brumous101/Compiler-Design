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

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.2")]
[System.CLSCompliant(false)]
public partial class mygrammarParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		ID=1, EQ=2, LP=3, RP=4, SEMI=5, NUM=6, COMMA=7, STRINGCONST=8;
	public const int
		RULE_start = 0, RULE_stmtList = 1, RULE_assign = 2, RULE_funcCall = 3, 
		RULE_numList = 4;
	public static readonly string[] ruleNames = {
		"start", "stmtList", "assign", "funcCall", "numList"
	};

	private static readonly string[] _LiteralNames = {
	};
	private static readonly string[] _SymbolicNames = {
		null, "ID", "EQ", "LP", "RP", "SEMI", "NUM", "COMMA", "STRINGCONST"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "mygrammar.txt"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static mygrammarParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

		public mygrammarParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public mygrammarParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	public partial class StartContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public StmtListContext stmtList() {
			return GetRuleContext<StmtListContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Eof() { return GetToken(mygrammarParser.Eof, 0); }
		public StartContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_start; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			ImygrammarListener typedListener = listener as ImygrammarListener;
			if (typedListener != null) typedListener.EnterStart(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			ImygrammarListener typedListener = listener as ImygrammarListener;
			if (typedListener != null) typedListener.ExitStart(this);
		}
	}

	[RuleVersion(0)]
	public StartContext start() {
		StartContext _localctx = new StartContext(Context, State);
		EnterRule(_localctx, 0, RULE_start);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 10;
			stmtList();
			State = 11;
			Match(Eof);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class StmtListContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public AssignContext assign() {
			return GetRuleContext<AssignContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public StmtListContext stmtList() {
			return GetRuleContext<StmtListContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public FuncCallContext funcCall() {
			return GetRuleContext<FuncCallContext>(0);
		}
		public StmtListContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_stmtList; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			ImygrammarListener typedListener = listener as ImygrammarListener;
			if (typedListener != null) typedListener.EnterStmtList(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			ImygrammarListener typedListener = listener as ImygrammarListener;
			if (typedListener != null) typedListener.ExitStmtList(this);
		}
	}

	[RuleVersion(0)]
	public StmtListContext stmtList() {
		StmtListContext _localctx = new StmtListContext(Context, State);
		EnterRule(_localctx, 2, RULE_stmtList);
		try {
			State = 18;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,0,Context) ) {
			case 1:
				EnterOuterAlt(_localctx, 1);
				{
				State = 13;
				assign();
				State = 14;
				stmtList();
				}
				break;
			case 2:
				EnterOuterAlt(_localctx, 2);
				{
				State = 16;
				funcCall();
				}
				break;
			case 3:
				EnterOuterAlt(_localctx, 3);
				{
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class AssignContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode ID() { return GetToken(mygrammarParser.ID, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode EQ() { return GetToken(mygrammarParser.EQ, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode NUM() { return GetToken(mygrammarParser.NUM, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode SEMI() { return GetToken(mygrammarParser.SEMI, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode STRINGCONST() { return GetToken(mygrammarParser.STRINGCONST, 0); }
		public AssignContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_assign; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			ImygrammarListener typedListener = listener as ImygrammarListener;
			if (typedListener != null) typedListener.EnterAssign(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			ImygrammarListener typedListener = listener as ImygrammarListener;
			if (typedListener != null) typedListener.ExitAssign(this);
		}
	}

	[RuleVersion(0)]
	public AssignContext assign() {
		AssignContext _localctx = new AssignContext(Context, State);
		EnterRule(_localctx, 4, RULE_assign);
		try {
			State = 28;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,1,Context) ) {
			case 1:
				EnterOuterAlt(_localctx, 1);
				{
				State = 20;
				Match(ID);
				State = 21;
				Match(EQ);
				State = 22;
				Match(NUM);
				State = 23;
				Match(SEMI);
				}
				break;
			case 2:
				EnterOuterAlt(_localctx, 2);
				{
				State = 24;
				Match(ID);
				State = 25;
				Match(EQ);
				State = 26;
				Match(STRINGCONST);
				State = 27;
				Match(SEMI);
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class FuncCallContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode ID() { return GetToken(mygrammarParser.ID, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode LP() { return GetToken(mygrammarParser.LP, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public NumListContext numList() {
			return GetRuleContext<NumListContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode RP() { return GetToken(mygrammarParser.RP, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode SEMI() { return GetToken(mygrammarParser.SEMI, 0); }
		public FuncCallContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_funcCall; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			ImygrammarListener typedListener = listener as ImygrammarListener;
			if (typedListener != null) typedListener.EnterFuncCall(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			ImygrammarListener typedListener = listener as ImygrammarListener;
			if (typedListener != null) typedListener.ExitFuncCall(this);
		}
	}

	[RuleVersion(0)]
	public FuncCallContext funcCall() {
		FuncCallContext _localctx = new FuncCallContext(Context, State);
		EnterRule(_localctx, 6, RULE_funcCall);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 30;
			Match(ID);
			State = 31;
			Match(LP);
			State = 32;
			numList();
			State = 33;
			Match(RP);
			State = 34;
			Match(SEMI);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class NumListContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode NUM() { return GetToken(mygrammarParser.NUM, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode COMMA() { return GetToken(mygrammarParser.COMMA, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public NumListContext numList() {
			return GetRuleContext<NumListContext>(0);
		}
		public NumListContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_numList; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			ImygrammarListener typedListener = listener as ImygrammarListener;
			if (typedListener != null) typedListener.EnterNumList(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			ImygrammarListener typedListener = listener as ImygrammarListener;
			if (typedListener != null) typedListener.ExitNumList(this);
		}
	}

	[RuleVersion(0)]
	public NumListContext numList() {
		NumListContext _localctx = new NumListContext(Context, State);
		EnterRule(_localctx, 8, RULE_numList);
		try {
			State = 40;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,2,Context) ) {
			case 1:
				EnterOuterAlt(_localctx, 1);
				{
				State = 36;
				Match(NUM);
				State = 37;
				Match(COMMA);
				State = 38;
				numList();
				}
				break;
			case 2:
				EnterOuterAlt(_localctx, 2);
				{
				State = 39;
				Match(NUM);
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x3', '\n', '-', '\x4', '\x2', '\t', '\x2', '\x4', '\x3', '\t', 
		'\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', '\x5', '\x4', '\x6', 
		'\t', '\x6', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x5', '\x3', '\x15', 
		'\n', '\x3', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', 
		'\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x5', '\x4', '\x1F', 
		'\n', '\x4', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', 
		'\x5', '\x3', '\x5', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', 
		'\x6', '\x5', '\x6', '+', '\n', '\x6', '\x3', '\x6', '\x2', '\x2', '\a', 
		'\x2', '\x4', '\x6', '\b', '\n', '\x2', '\x2', '\x2', '+', '\x2', '\f', 
		'\x3', '\x2', '\x2', '\x2', '\x4', '\x14', '\x3', '\x2', '\x2', '\x2', 
		'\x6', '\x1E', '\x3', '\x2', '\x2', '\x2', '\b', ' ', '\x3', '\x2', '\x2', 
		'\x2', '\n', '*', '\x3', '\x2', '\x2', '\x2', '\f', '\r', '\x5', '\x4', 
		'\x3', '\x2', '\r', '\xE', '\a', '\x2', '\x2', '\x3', '\xE', '\x3', '\x3', 
		'\x2', '\x2', '\x2', '\xF', '\x10', '\x5', '\x6', '\x4', '\x2', '\x10', 
		'\x11', '\x5', '\x4', '\x3', '\x2', '\x11', '\x15', '\x3', '\x2', '\x2', 
		'\x2', '\x12', '\x15', '\x5', '\b', '\x5', '\x2', '\x13', '\x15', '\x3', 
		'\x2', '\x2', '\x2', '\x14', '\xF', '\x3', '\x2', '\x2', '\x2', '\x14', 
		'\x12', '\x3', '\x2', '\x2', '\x2', '\x14', '\x13', '\x3', '\x2', '\x2', 
		'\x2', '\x15', '\x5', '\x3', '\x2', '\x2', '\x2', '\x16', '\x17', '\a', 
		'\x3', '\x2', '\x2', '\x17', '\x18', '\a', '\x4', '\x2', '\x2', '\x18', 
		'\x19', '\a', '\b', '\x2', '\x2', '\x19', '\x1F', '\a', '\a', '\x2', '\x2', 
		'\x1A', '\x1B', '\a', '\x3', '\x2', '\x2', '\x1B', '\x1C', '\a', '\x4', 
		'\x2', '\x2', '\x1C', '\x1D', '\a', '\n', '\x2', '\x2', '\x1D', '\x1F', 
		'\a', '\a', '\x2', '\x2', '\x1E', '\x16', '\x3', '\x2', '\x2', '\x2', 
		'\x1E', '\x1A', '\x3', '\x2', '\x2', '\x2', '\x1F', '\a', '\x3', '\x2', 
		'\x2', '\x2', ' ', '!', '\a', '\x3', '\x2', '\x2', '!', '\"', '\a', '\x5', 
		'\x2', '\x2', '\"', '#', '\x5', '\n', '\x6', '\x2', '#', '$', '\a', '\x6', 
		'\x2', '\x2', '$', '%', '\a', '\a', '\x2', '\x2', '%', '\t', '\x3', '\x2', 
		'\x2', '\x2', '&', '\'', '\a', '\b', '\x2', '\x2', '\'', '(', '\a', '\t', 
		'\x2', '\x2', '(', '+', '\x5', '\n', '\x6', '\x2', ')', '+', '\a', '\b', 
		'\x2', '\x2', '*', '&', '\x3', '\x2', '\x2', '\x2', '*', ')', '\x3', '\x2', 
		'\x2', '\x2', '+', '\v', '\x3', '\x2', '\x2', '\x2', '\x5', '\x14', '\x1E', 
		'*',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}