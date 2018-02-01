//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from VineParser.g4 by ANTLR 4.7.1

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

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.1")]
[System.CLSCompliant(false)]
public partial class VineParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		WS=1, VINE_DECLARATION=2, TAG_OPEN=3, COMMENT=4, SCRIPT=5, TAG_NAME=6, 
		TAG_CLOSE=7, TAG_SLASH_CLOSE=8, TAG_SLASH=9, TAG_WS=10, ATTRIBUTE_NAME=11, 
		TAG_EQUALS=12, ATTRIBUTE_VALUE=13, ATTRIBUTE=14;
	public const int
		RULE_document = 0, RULE_element = 1, RULE_content = 2, RULE_misc = 3, 
		RULE_comment = 4, RULE_script = 5, RULE_attribute = 6, RULE_attributeName = 7, 
		RULE_attributeValue = 8;
	public static readonly string[] ruleNames = {
		"document", "element", "content", "misc", "comment", "script", "attribute", 
		"attributeName", "attributeValue"
	};

	private static readonly string[] _LiteralNames = {
		null, null, null, "'<'", null, null, null, "'>'", "'/>'", "'/'", null, 
		null, "'='"
	};
	private static readonly string[] _SymbolicNames = {
		null, "WS", "VINE_DECLARATION", "TAG_OPEN", "COMMENT", "SCRIPT", "TAG_NAME", 
		"TAG_CLOSE", "TAG_SLASH_CLOSE", "TAG_SLASH", "TAG_WS", "ATTRIBUTE_NAME", 
		"TAG_EQUALS", "ATTRIBUTE_VALUE", "ATTRIBUTE"
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

	public override string GrammarFileName { get { return "VineParser.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static VineParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

		public VineParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public VineParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}
	public partial class DocumentContext : ParserRuleContext {
		public ITerminalNode VINE_DECLARATION() { return GetToken(VineParser.VINE_DECLARATION, 0); }
		public ElementContext[] element() {
			return GetRuleContexts<ElementContext>();
		}
		public ElementContext element(int i) {
			return GetRuleContext<ElementContext>(i);
		}
		public MiscContext[] misc() {
			return GetRuleContexts<MiscContext>();
		}
		public MiscContext misc(int i) {
			return GetRuleContext<MiscContext>(i);
		}
		public DocumentContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_document; } }
		public override void EnterRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.EnterDocument(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.ExitDocument(this);
		}
	}

	[RuleVersion(0)]
	public DocumentContext document() {
		DocumentContext _localctx = new DocumentContext(Context, State);
		EnterRule(_localctx, 0, RULE_document);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 18; Match(VINE_DECLARATION);
			State = 23;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << WS) | (1L << TAG_OPEN) | (1L << COMMENT) | (1L << TAG_WS))) != 0)) {
				{
				State = 21;
				ErrorHandler.Sync(this);
				switch (TokenStream.LA(1)) {
				case TAG_OPEN:
					{
					State = 19; element();
					}
					break;
				case WS:
				case COMMENT:
				case TAG_WS:
					{
					State = 20; misc();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				State = 25;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
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

	public partial class ElementContext : ParserRuleContext {
		public ITerminalNode[] TAG_OPEN() { return GetTokens(VineParser.TAG_OPEN); }
		public ITerminalNode TAG_OPEN(int i) {
			return GetToken(VineParser.TAG_OPEN, i);
		}
		public ITerminalNode[] TAG_NAME() { return GetTokens(VineParser.TAG_NAME); }
		public ITerminalNode TAG_NAME(int i) {
			return GetToken(VineParser.TAG_NAME, i);
		}
		public ITerminalNode[] TAG_CLOSE() { return GetTokens(VineParser.TAG_CLOSE); }
		public ITerminalNode TAG_CLOSE(int i) {
			return GetToken(VineParser.TAG_CLOSE, i);
		}
		public ITerminalNode TAG_SLASH() { return GetToken(VineParser.TAG_SLASH, 0); }
		public MiscContext[] misc() {
			return GetRuleContexts<MiscContext>();
		}
		public MiscContext misc(int i) {
			return GetRuleContext<MiscContext>(i);
		}
		public AttributeContext[] attribute() {
			return GetRuleContexts<AttributeContext>();
		}
		public AttributeContext attribute(int i) {
			return GetRuleContext<AttributeContext>(i);
		}
		public ContentContext[] content() {
			return GetRuleContexts<ContentContext>();
		}
		public ContentContext content(int i) {
			return GetRuleContext<ContentContext>(i);
		}
		public ITerminalNode TAG_SLASH_CLOSE() { return GetToken(VineParser.TAG_SLASH_CLOSE, 0); }
		public ElementContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_element; } }
		public override void EnterRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.EnterElement(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.ExitElement(this);
		}
	}

	[RuleVersion(0)]
	public ElementContext element() {
		ElementContext _localctx = new ElementContext(Context, State);
		EnterRule(_localctx, 2, RULE_element);
		int _la;
		try {
			int _alt;
			State = 73;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,9,Context) ) {
			case 1:
				EnterOuterAlt(_localctx, 1);
				{
				State = 26; Match(TAG_OPEN);
				State = 27; Match(TAG_NAME);
				State = 31;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,2,Context);
				while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						State = 28; misc();
						}
						} 
					}
					State = 33;
					ErrorHandler.Sync(this);
					_alt = Interpreter.AdaptivePredict(TokenStream,2,Context);
				}
				State = 37;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << WS) | (1L << COMMENT) | (1L << TAG_WS) | (1L << ATTRIBUTE_NAME))) != 0)) {
					{
					{
					State = 34; attribute();
					}
					}
					State = 39;
					ErrorHandler.Sync(this);
					_la = TokenStream.LA(1);
				}
				State = 40; Match(TAG_CLOSE);
				State = 45;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,5,Context);
				while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						State = 43;
						ErrorHandler.Sync(this);
						switch ( Interpreter.AdaptivePredict(TokenStream,4,Context) ) {
						case 1:
							{
							State = 41; content();
							}
							break;
						case 2:
							{
							State = 42; misc();
							}
							break;
						}
						} 
					}
					State = 47;
					ErrorHandler.Sync(this);
					_alt = Interpreter.AdaptivePredict(TokenStream,5,Context);
				}
				State = 48; Match(TAG_OPEN);
				State = 49; Match(TAG_SLASH);
				State = 50; Match(TAG_NAME);
				State = 54;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << WS) | (1L << COMMENT) | (1L << TAG_WS))) != 0)) {
					{
					{
					State = 51; misc();
					}
					}
					State = 56;
					ErrorHandler.Sync(this);
					_la = TokenStream.LA(1);
				}
				State = 57; Match(TAG_CLOSE);
				}
				break;
			case 2:
				EnterOuterAlt(_localctx, 2);
				{
				State = 58; Match(TAG_OPEN);
				State = 59; Match(TAG_NAME);
				State = 63;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,7,Context);
				while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						State = 60; misc();
						}
						} 
					}
					State = 65;
					ErrorHandler.Sync(this);
					_alt = Interpreter.AdaptivePredict(TokenStream,7,Context);
				}
				State = 69;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << WS) | (1L << COMMENT) | (1L << TAG_WS) | (1L << ATTRIBUTE_NAME))) != 0)) {
					{
					{
					State = 66; attribute();
					}
					}
					State = 71;
					ErrorHandler.Sync(this);
					_la = TokenStream.LA(1);
				}
				State = 72; Match(TAG_SLASH_CLOSE);
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

	public partial class ContentContext : ParserRuleContext {
		public ElementContext element() {
			return GetRuleContext<ElementContext>(0);
		}
		public MiscContext[] misc() {
			return GetRuleContexts<MiscContext>();
		}
		public MiscContext misc(int i) {
			return GetRuleContext<MiscContext>(i);
		}
		public CommentContext comment() {
			return GetRuleContext<CommentContext>(0);
		}
		public ScriptContext script() {
			return GetRuleContext<ScriptContext>(0);
		}
		public ContentContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_content; } }
		public override void EnterRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.EnterContent(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.ExitContent(this);
		}
	}

	[RuleVersion(0)]
	public ContentContext content() {
		ContentContext _localctx = new ContentContext(Context, State);
		EnterRule(_localctx, 4, RULE_content);
		int _la;
		try {
			int _alt;
			State = 90;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,12,Context) ) {
			case 1:
				EnterOuterAlt(_localctx, 1);
				{
				State = 78;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << WS) | (1L << COMMENT) | (1L << TAG_WS))) != 0)) {
					{
					{
					State = 75; misc();
					}
					}
					State = 80;
					ErrorHandler.Sync(this);
					_la = TokenStream.LA(1);
				}
				State = 81; element();
				State = 85;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,11,Context);
				while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						State = 82; misc();
						}
						} 
					}
					State = 87;
					ErrorHandler.Sync(this);
					_alt = Interpreter.AdaptivePredict(TokenStream,11,Context);
				}
				}
				break;
			case 2:
				EnterOuterAlt(_localctx, 2);
				{
				State = 88; comment();
				}
				break;
			case 3:
				EnterOuterAlt(_localctx, 3);
				{
				State = 89; script();
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

	public partial class MiscContext : ParserRuleContext {
		public CommentContext comment() {
			return GetRuleContext<CommentContext>(0);
		}
		public ITerminalNode WS() { return GetToken(VineParser.WS, 0); }
		public ITerminalNode TAG_WS() { return GetToken(VineParser.TAG_WS, 0); }
		public MiscContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_misc; } }
		public override void EnterRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.EnterMisc(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.ExitMisc(this);
		}
	}

	[RuleVersion(0)]
	public MiscContext misc() {
		MiscContext _localctx = new MiscContext(Context, State);
		EnterRule(_localctx, 6, RULE_misc);
		try {
			State = 95;
			ErrorHandler.Sync(this);
			switch (TokenStream.LA(1)) {
			case COMMENT:
				EnterOuterAlt(_localctx, 1);
				{
				State = 92; comment();
				}
				break;
			case WS:
				EnterOuterAlt(_localctx, 2);
				{
				State = 93; Match(WS);
				}
				break;
			case TAG_WS:
				EnterOuterAlt(_localctx, 3);
				{
				State = 94; Match(TAG_WS);
				}
				break;
			default:
				throw new NoViableAltException(this);
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

	public partial class CommentContext : ParserRuleContext {
		public ITerminalNode COMMENT() { return GetToken(VineParser.COMMENT, 0); }
		public CommentContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_comment; } }
		public override void EnterRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.EnterComment(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.ExitComment(this);
		}
	}

	[RuleVersion(0)]
	public CommentContext comment() {
		CommentContext _localctx = new CommentContext(Context, State);
		EnterRule(_localctx, 8, RULE_comment);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 97; Match(COMMENT);
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

	public partial class ScriptContext : ParserRuleContext {
		public ITerminalNode SCRIPT() { return GetToken(VineParser.SCRIPT, 0); }
		public ScriptContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_script; } }
		public override void EnterRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.EnterScript(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.ExitScript(this);
		}
	}

	[RuleVersion(0)]
	public ScriptContext script() {
		ScriptContext _localctx = new ScriptContext(Context, State);
		EnterRule(_localctx, 10, RULE_script);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 99; Match(SCRIPT);
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

	public partial class AttributeContext : ParserRuleContext {
		public AttributeNameContext attributeName() {
			return GetRuleContext<AttributeNameContext>(0);
		}
		public ITerminalNode TAG_EQUALS() { return GetToken(VineParser.TAG_EQUALS, 0); }
		public AttributeValueContext attributeValue() {
			return GetRuleContext<AttributeValueContext>(0);
		}
		public MiscContext[] misc() {
			return GetRuleContexts<MiscContext>();
		}
		public MiscContext misc(int i) {
			return GetRuleContext<MiscContext>(i);
		}
		public AttributeContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_attribute; } }
		public override void EnterRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.EnterAttribute(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.ExitAttribute(this);
		}
	}

	[RuleVersion(0)]
	public AttributeContext attribute() {
		AttributeContext _localctx = new AttributeContext(Context, State);
		EnterRule(_localctx, 12, RULE_attribute);
		int _la;
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			State = 104;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << WS) | (1L << COMMENT) | (1L << TAG_WS))) != 0)) {
				{
				{
				State = 101; misc();
				}
				}
				State = 106;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 107; attributeName();
			State = 111;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << WS) | (1L << COMMENT) | (1L << TAG_WS))) != 0)) {
				{
				{
				State = 108; misc();
				}
				}
				State = 113;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 114; Match(TAG_EQUALS);
			State = 118;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << WS) | (1L << COMMENT) | (1L << TAG_WS))) != 0)) {
				{
				{
				State = 115; misc();
				}
				}
				State = 120;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 121; attributeValue();
			State = 125;
			ErrorHandler.Sync(this);
			_alt = Interpreter.AdaptivePredict(TokenStream,17,Context);
			while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					State = 122; misc();
					}
					} 
				}
				State = 127;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,17,Context);
			}
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

	public partial class AttributeNameContext : ParserRuleContext {
		public ITerminalNode ATTRIBUTE_NAME() { return GetToken(VineParser.ATTRIBUTE_NAME, 0); }
		public AttributeNameContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_attributeName; } }
		public override void EnterRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.EnterAttributeName(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.ExitAttributeName(this);
		}
	}

	[RuleVersion(0)]
	public AttributeNameContext attributeName() {
		AttributeNameContext _localctx = new AttributeNameContext(Context, State);
		EnterRule(_localctx, 14, RULE_attributeName);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 128; Match(ATTRIBUTE_NAME);
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

	public partial class AttributeValueContext : ParserRuleContext {
		public ITerminalNode ATTRIBUTE_VALUE() { return GetToken(VineParser.ATTRIBUTE_VALUE, 0); }
		public AttributeValueContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_attributeValue; } }
		public override void EnterRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.EnterAttributeValue(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IVineParserListener typedListener = listener as IVineParserListener;
			if (typedListener != null) typedListener.ExitAttributeValue(this);
		}
	}

	[RuleVersion(0)]
	public AttributeValueContext attributeValue() {
		AttributeValueContext _localctx = new AttributeValueContext(Context, State);
		EnterRule(_localctx, 16, RULE_attributeValue);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 130; Match(ATTRIBUTE_VALUE);
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
		'\x5964', '\x3', '\x10', '\x87', '\x4', '\x2', '\t', '\x2', '\x4', '\x3', 
		'\t', '\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', '\x5', '\x4', 
		'\x6', '\t', '\x6', '\x4', '\a', '\t', '\a', '\x4', '\b', '\t', '\b', 
		'\x4', '\t', '\t', '\t', '\x4', '\n', '\t', '\n', '\x3', '\x2', '\x3', 
		'\x2', '\x3', '\x2', '\a', '\x2', '\x18', '\n', '\x2', '\f', '\x2', '\xE', 
		'\x2', '\x1B', '\v', '\x2', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\a', '\x3', ' ', '\n', '\x3', '\f', '\x3', '\xE', '\x3', '#', '\v', '\x3', 
		'\x3', '\x3', '\a', '\x3', '&', '\n', '\x3', '\f', '\x3', '\xE', '\x3', 
		')', '\v', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\a', '\x3', 
		'.', '\n', '\x3', '\f', '\x3', '\xE', '\x3', '\x31', '\v', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\a', '\x3', '\x37', 
		'\n', '\x3', '\f', '\x3', '\xE', '\x3', ':', '\v', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\a', '\x3', '@', '\n', '\x3', 
		'\f', '\x3', '\xE', '\x3', '\x43', '\v', '\x3', '\x3', '\x3', '\a', '\x3', 
		'\x46', '\n', '\x3', '\f', '\x3', '\xE', '\x3', 'I', '\v', '\x3', '\x3', 
		'\x3', '\x5', '\x3', 'L', '\n', '\x3', '\x3', '\x4', '\a', '\x4', 'O', 
		'\n', '\x4', '\f', '\x4', '\xE', '\x4', 'R', '\v', '\x4', '\x3', '\x4', 
		'\x3', '\x4', '\a', '\x4', 'V', '\n', '\x4', '\f', '\x4', '\xE', '\x4', 
		'Y', '\v', '\x4', '\x3', '\x4', '\x3', '\x4', '\x5', '\x4', ']', '\n', 
		'\x4', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x5', '\x5', '\x62', 
		'\n', '\x5', '\x3', '\x6', '\x3', '\x6', '\x3', '\a', '\x3', '\a', '\x3', 
		'\b', '\a', '\b', 'i', '\n', '\b', '\f', '\b', '\xE', '\b', 'l', '\v', 
		'\b', '\x3', '\b', '\x3', '\b', '\a', '\b', 'p', '\n', '\b', '\f', '\b', 
		'\xE', '\b', 's', '\v', '\b', '\x3', '\b', '\x3', '\b', '\a', '\b', 'w', 
		'\n', '\b', '\f', '\b', '\xE', '\b', 'z', '\v', '\b', '\x3', '\b', '\x3', 
		'\b', '\a', '\b', '~', '\n', '\b', '\f', '\b', '\xE', '\b', '\x81', '\v', 
		'\b', '\x3', '\t', '\x3', '\t', '\x3', '\n', '\x3', '\n', '\x3', '\n', 
		'\x2', '\x2', '\v', '\x2', '\x4', '\x6', '\b', '\n', '\f', '\xE', '\x10', 
		'\x12', '\x2', '\x2', '\x2', '\x91', '\x2', '\x14', '\x3', '\x2', '\x2', 
		'\x2', '\x4', 'K', '\x3', '\x2', '\x2', '\x2', '\x6', '\\', '\x3', '\x2', 
		'\x2', '\x2', '\b', '\x61', '\x3', '\x2', '\x2', '\x2', '\n', '\x63', 
		'\x3', '\x2', '\x2', '\x2', '\f', '\x65', '\x3', '\x2', '\x2', '\x2', 
		'\xE', 'j', '\x3', '\x2', '\x2', '\x2', '\x10', '\x82', '\x3', '\x2', 
		'\x2', '\x2', '\x12', '\x84', '\x3', '\x2', '\x2', '\x2', '\x14', '\x19', 
		'\a', '\x4', '\x2', '\x2', '\x15', '\x18', '\x5', '\x4', '\x3', '\x2', 
		'\x16', '\x18', '\x5', '\b', '\x5', '\x2', '\x17', '\x15', '\x3', '\x2', 
		'\x2', '\x2', '\x17', '\x16', '\x3', '\x2', '\x2', '\x2', '\x18', '\x1B', 
		'\x3', '\x2', '\x2', '\x2', '\x19', '\x17', '\x3', '\x2', '\x2', '\x2', 
		'\x19', '\x1A', '\x3', '\x2', '\x2', '\x2', '\x1A', '\x3', '\x3', '\x2', 
		'\x2', '\x2', '\x1B', '\x19', '\x3', '\x2', '\x2', '\x2', '\x1C', '\x1D', 
		'\a', '\x5', '\x2', '\x2', '\x1D', '!', '\a', '\b', '\x2', '\x2', '\x1E', 
		' ', '\x5', '\b', '\x5', '\x2', '\x1F', '\x1E', '\x3', '\x2', '\x2', '\x2', 
		' ', '#', '\x3', '\x2', '\x2', '\x2', '!', '\x1F', '\x3', '\x2', '\x2', 
		'\x2', '!', '\"', '\x3', '\x2', '\x2', '\x2', '\"', '\'', '\x3', '\x2', 
		'\x2', '\x2', '#', '!', '\x3', '\x2', '\x2', '\x2', '$', '&', '\x5', '\xE', 
		'\b', '\x2', '%', '$', '\x3', '\x2', '\x2', '\x2', '&', ')', '\x3', '\x2', 
		'\x2', '\x2', '\'', '%', '\x3', '\x2', '\x2', '\x2', '\'', '(', '\x3', 
		'\x2', '\x2', '\x2', '(', '*', '\x3', '\x2', '\x2', '\x2', ')', '\'', 
		'\x3', '\x2', '\x2', '\x2', '*', '/', '\a', '\t', '\x2', '\x2', '+', '.', 
		'\x5', '\x6', '\x4', '\x2', ',', '.', '\x5', '\b', '\x5', '\x2', '-', 
		'+', '\x3', '\x2', '\x2', '\x2', '-', ',', '\x3', '\x2', '\x2', '\x2', 
		'.', '\x31', '\x3', '\x2', '\x2', '\x2', '/', '-', '\x3', '\x2', '\x2', 
		'\x2', '/', '\x30', '\x3', '\x2', '\x2', '\x2', '\x30', '\x32', '\x3', 
		'\x2', '\x2', '\x2', '\x31', '/', '\x3', '\x2', '\x2', '\x2', '\x32', 
		'\x33', '\a', '\x5', '\x2', '\x2', '\x33', '\x34', '\a', '\v', '\x2', 
		'\x2', '\x34', '\x38', '\a', '\b', '\x2', '\x2', '\x35', '\x37', '\x5', 
		'\b', '\x5', '\x2', '\x36', '\x35', '\x3', '\x2', '\x2', '\x2', '\x37', 
		':', '\x3', '\x2', '\x2', '\x2', '\x38', '\x36', '\x3', '\x2', '\x2', 
		'\x2', '\x38', '\x39', '\x3', '\x2', '\x2', '\x2', '\x39', ';', '\x3', 
		'\x2', '\x2', '\x2', ':', '\x38', '\x3', '\x2', '\x2', '\x2', ';', 'L', 
		'\a', '\t', '\x2', '\x2', '<', '=', '\a', '\x5', '\x2', '\x2', '=', '\x41', 
		'\a', '\b', '\x2', '\x2', '>', '@', '\x5', '\b', '\x5', '\x2', '?', '>', 
		'\x3', '\x2', '\x2', '\x2', '@', '\x43', '\x3', '\x2', '\x2', '\x2', '\x41', 
		'?', '\x3', '\x2', '\x2', '\x2', '\x41', '\x42', '\x3', '\x2', '\x2', 
		'\x2', '\x42', 'G', '\x3', '\x2', '\x2', '\x2', '\x43', '\x41', '\x3', 
		'\x2', '\x2', '\x2', '\x44', '\x46', '\x5', '\xE', '\b', '\x2', '\x45', 
		'\x44', '\x3', '\x2', '\x2', '\x2', '\x46', 'I', '\x3', '\x2', '\x2', 
		'\x2', 'G', '\x45', '\x3', '\x2', '\x2', '\x2', 'G', 'H', '\x3', '\x2', 
		'\x2', '\x2', 'H', 'J', '\x3', '\x2', '\x2', '\x2', 'I', 'G', '\x3', '\x2', 
		'\x2', '\x2', 'J', 'L', '\a', '\n', '\x2', '\x2', 'K', '\x1C', '\x3', 
		'\x2', '\x2', '\x2', 'K', '<', '\x3', '\x2', '\x2', '\x2', 'L', '\x5', 
		'\x3', '\x2', '\x2', '\x2', 'M', 'O', '\x5', '\b', '\x5', '\x2', 'N', 
		'M', '\x3', '\x2', '\x2', '\x2', 'O', 'R', '\x3', '\x2', '\x2', '\x2', 
		'P', 'N', '\x3', '\x2', '\x2', '\x2', 'P', 'Q', '\x3', '\x2', '\x2', '\x2', 
		'Q', 'S', '\x3', '\x2', '\x2', '\x2', 'R', 'P', '\x3', '\x2', '\x2', '\x2', 
		'S', 'W', '\x5', '\x4', '\x3', '\x2', 'T', 'V', '\x5', '\b', '\x5', '\x2', 
		'U', 'T', '\x3', '\x2', '\x2', '\x2', 'V', 'Y', '\x3', '\x2', '\x2', '\x2', 
		'W', 'U', '\x3', '\x2', '\x2', '\x2', 'W', 'X', '\x3', '\x2', '\x2', '\x2', 
		'X', ']', '\x3', '\x2', '\x2', '\x2', 'Y', 'W', '\x3', '\x2', '\x2', '\x2', 
		'Z', ']', '\x5', '\n', '\x6', '\x2', '[', ']', '\x5', '\f', '\a', '\x2', 
		'\\', 'P', '\x3', '\x2', '\x2', '\x2', '\\', 'Z', '\x3', '\x2', '\x2', 
		'\x2', '\\', '[', '\x3', '\x2', '\x2', '\x2', ']', '\a', '\x3', '\x2', 
		'\x2', '\x2', '^', '\x62', '\x5', '\n', '\x6', '\x2', '_', '\x62', '\a', 
		'\x3', '\x2', '\x2', '`', '\x62', '\a', '\f', '\x2', '\x2', '\x61', '^', 
		'\x3', '\x2', '\x2', '\x2', '\x61', '_', '\x3', '\x2', '\x2', '\x2', '\x61', 
		'`', '\x3', '\x2', '\x2', '\x2', '\x62', '\t', '\x3', '\x2', '\x2', '\x2', 
		'\x63', '\x64', '\a', '\x6', '\x2', '\x2', '\x64', '\v', '\x3', '\x2', 
		'\x2', '\x2', '\x65', '\x66', '\a', '\a', '\x2', '\x2', '\x66', '\r', 
		'\x3', '\x2', '\x2', '\x2', 'g', 'i', '\x5', '\b', '\x5', '\x2', 'h', 
		'g', '\x3', '\x2', '\x2', '\x2', 'i', 'l', '\x3', '\x2', '\x2', '\x2', 
		'j', 'h', '\x3', '\x2', '\x2', '\x2', 'j', 'k', '\x3', '\x2', '\x2', '\x2', 
		'k', 'm', '\x3', '\x2', '\x2', '\x2', 'l', 'j', '\x3', '\x2', '\x2', '\x2', 
		'm', 'q', '\x5', '\x10', '\t', '\x2', 'n', 'p', '\x5', '\b', '\x5', '\x2', 
		'o', 'n', '\x3', '\x2', '\x2', '\x2', 'p', 's', '\x3', '\x2', '\x2', '\x2', 
		'q', 'o', '\x3', '\x2', '\x2', '\x2', 'q', 'r', '\x3', '\x2', '\x2', '\x2', 
		'r', 't', '\x3', '\x2', '\x2', '\x2', 's', 'q', '\x3', '\x2', '\x2', '\x2', 
		't', 'x', '\a', '\xE', '\x2', '\x2', 'u', 'w', '\x5', '\b', '\x5', '\x2', 
		'v', 'u', '\x3', '\x2', '\x2', '\x2', 'w', 'z', '\x3', '\x2', '\x2', '\x2', 
		'x', 'v', '\x3', '\x2', '\x2', '\x2', 'x', 'y', '\x3', '\x2', '\x2', '\x2', 
		'y', '{', '\x3', '\x2', '\x2', '\x2', 'z', 'x', '\x3', '\x2', '\x2', '\x2', 
		'{', '\x7F', '\x5', '\x12', '\n', '\x2', '|', '~', '\x5', '\b', '\x5', 
		'\x2', '}', '|', '\x3', '\x2', '\x2', '\x2', '~', '\x81', '\x3', '\x2', 
		'\x2', '\x2', '\x7F', '}', '\x3', '\x2', '\x2', '\x2', '\x7F', '\x80', 
		'\x3', '\x2', '\x2', '\x2', '\x80', '\xF', '\x3', '\x2', '\x2', '\x2', 
		'\x81', '\x7F', '\x3', '\x2', '\x2', '\x2', '\x82', '\x83', '\a', '\r', 
		'\x2', '\x2', '\x83', '\x11', '\x3', '\x2', '\x2', '\x2', '\x84', '\x85', 
		'\a', '\xF', '\x2', '\x2', '\x85', '\x13', '\x3', '\x2', '\x2', '\x2', 
		'\x14', '\x17', '\x19', '!', '\'', '-', '/', '\x38', '\x41', 'G', 'K', 
		'P', 'W', '\\', '\x61', 'j', 'q', 'x', '\x7F',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
