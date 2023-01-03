using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oxide.Core.Libraries.Covalence;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Oxide;

public interface IPlayerManager
{
	IEnumerable<IPlayer> All { get; }
	IEnumerable<IPlayer> Connected { get; }

	IPlayer FindPlayerById(string id);
	IPlayer FindPlayerByObj(object obj);
	IPlayer FindPlayer(string partialNameOrId);
	IEnumerable<IPlayer> FindPlayers(string partialNameOrId);
}

public interface ICovalence
{
	IPlayerManager Players { get; }
	IServer Server { get; }
}

public interface IServer
{
	string Name { get; set; }
	System.Net.IPAddress Address { get; }
	System.Net.IPAddress LocalAddress { get; }

	ushort Port { get; }
	string Version { get; }
	string Protocol { get; }

	System.Globalization.CultureInfo Language { get; }

	int Players { get; }
	int MaxPlayers { get; set; }

	DateTime Time { get; set; }

	void Ban(string id, string reason, TimeSpan duration = default(TimeSpan));
	TimeSpan BanTimeRemaining(string id);
	bool IsBanned(string id);
	void Unban(string id);

	void Save();

	void Broadcast(string message, string prefix, params object[] args);
	void Broadcast(string message);
	void Command(string command, params object[] args);
}

public class Formatter
{
	private static List<Element> Parse(List<Token> tokens)
	{
		var i = 0;
		var s = new Stack<Entry>();
		s.Push(new Entry(null, Element.Tag(ElementType.String)));
		while (i < tokens.Count)
		{
			var t = tokens[i++];
			Action<Element> action = delegate (Element el)
			{
				s.Push(new Entry(t.Pattern, el));
			};
			var element = s.Peek().Element;
			var type = t.Type;
			var tokenType = closeTags[element.Type];
			if (type == tokenType.GetValueOrDefault() & tokenType != null)
			{
				s.Pop();
				s.Peek().Element.Body.Add(element);
			}
			else
			{
				switch (t.Type)
				{
					case TokenType.String:
						element.Body.Add(Element.String(t.Val));
						break;
					case TokenType.Bold:
						action(Element.Tag(ElementType.Bold));
						break;
					case TokenType.Italic:
						action(Element.Tag(ElementType.Italic));
						break;
					case TokenType.Color:
						action(Element.ParamTag(ElementType.Color, t.Val));
						break;
					case TokenType.Size:
						action(Element.ParamTag(ElementType.Size, t.Val));
						break;
					default:
						element.Body.Add(Element.String(t.Pattern));
						break;
				}
			}
		}
		while (s.Count > 1)
		{
			var entry = s.Pop();
			var body = s.Peek().Element.Body;
			body.Add(Element.String(entry.Pattern));
			body.AddRange(entry.Element.Body);
		}
		return s.Pop().Element.Body;
	}

	public static List<Element> Parse(string text)
	{
		return Parse(Lexer.Lex(text));
	}

	private static Tag Translation(Element e, Dictionary<ElementType, Func<object, Tag>> translations)
	{
		Func<object, Tag> func;
		if (!translations.TryGetValue(e.Type, out func))
		{
			return new Tag("", "");
		}
		return func(e.Val);
	}

	private static string ToTreeFormat(List<Element> tree, Dictionary<ElementType, Func<object, Tag>> translations)
	{
		var stringBuilder = new StringBuilder();
		foreach (var element in tree)
		{
			if (element.Type == ElementType.String)
			{
				stringBuilder.Append(element.Val);
			}
			else
			{
				var tag = Translation(element, translations);
				stringBuilder.Append(tag.Open);
				stringBuilder.Append(ToTreeFormat(element.Body, translations));
				stringBuilder.Append(tag.Close);
			}
		}
		return stringBuilder.ToString();
	}

	private static string ToTreeFormat(string text, Dictionary<ElementType, Func<object, Tag>> translations)
	{
		return ToTreeFormat(Parse(text), translations);
	}

	private static string RGBAtoRGB(object rgba)
	{
		return rgba.ToString().Substring(0, 6);
	}

	public static string ToPlaintext(string text)
	{
		return ToTreeFormat(text, new Dictionary<ElementType, Func<object, Tag>>());
	}

	public static string ToUnity(string text)
	{
		var dictionary = new Dictionary<ElementType, Func<object, Tag>>();
		dictionary[ElementType.Bold] = (_) => new Tag("<b>", "</b>");
		dictionary[ElementType.Italic] = (_) => new Tag("<i>", "</i>");
		dictionary[ElementType.Color] = (c) => new Tag(string.Format("<color=#{0}>", c), "</color>");
		dictionary[ElementType.Size] = (s) => new Tag(string.Format("<size={0}>", s), "</size>");
		return ToTreeFormat(text, dictionary);
	}

	public static string ToRustLegacy(string text)
	{
		var dictionary = new Dictionary<ElementType, Func<object, Tag>>();
		dictionary[ElementType.Color] = (c) => new Tag("[color #" + RGBAtoRGB(c) + "]", "[color #ffffff]");
		return ToTreeFormat(text, dictionary);
	}

	public static string ToRoKAnd7DTD(string text)
	{
		var dictionary = new Dictionary<ElementType, Func<object, Tag>>();
		dictionary[ElementType.Color] = (c) => new Tag("[" + RGBAtoRGB(c) + "]", "[e7e7e7]");
		return ToTreeFormat(text, dictionary);
	}

	public static string ToTerraria(string text)
	{
		var dictionary = new Dictionary<ElementType, Func<object, Tag>>();
		dictionary[ElementType.Color] = (c) => new Tag("[c/" + RGBAtoRGB(c) + ":", "]");
		return ToTreeFormat(text, dictionary);
	}

	static Formatter()
	{
		var dictionary = new Dictionary<string, string>();
		dictionary["aqua"] = "00ffff";
		dictionary["black"] = "000000";
		dictionary["blue"] = "0000ff";
		dictionary["brown"] = "a52a2a";
		dictionary["cyan"] = "00ffff";
		dictionary["darkblue"] = "0000a0";
		dictionary["fuchsia"] = "ff00ff";
		dictionary["green"] = "008000";
		dictionary["grey"] = "808080";
		dictionary["lightblue"] = "add8e6";
		dictionary["lime"] = "00ff00";
		dictionary["magenta"] = "ff00ff";
		dictionary["maroon"] = "800000";
		dictionary["navy"] = "000080";
		dictionary["olive"] = "808000";
		dictionary["orange"] = "ffa500";
		dictionary["purple"] = "800080";
		dictionary["red"] = "ff0000";
		dictionary["silver"] = "c0c0c0";
		dictionary["teal"] = "008080";
		dictionary["white"] = "ffffff";
		dictionary["yellow"] = "ffff00";
		colorNames = dictionary;
		var dictionary2 = new Dictionary<ElementType, TokenType?>();
		dictionary2[ElementType.String] = null;
		dictionary2[ElementType.Bold] = new Formatter.TokenType?(TokenType.CloseBold);
		dictionary2[ElementType.Italic] = new Formatter.TokenType?(TokenType.CloseItalic);
		dictionary2[ElementType.Color] = new Formatter.TokenType?(TokenType.CloseColor);
		dictionary2[ElementType.Size] = new Formatter.TokenType?(TokenType.CloseSize);
		closeTags = dictionary2;
	}

	private static readonly Dictionary<string, string> colorNames;

	private static readonly Dictionary<ElementType, TokenType?> closeTags;

	private class Token
	{
		public TokenType Type;
		public object Val;
		public string Pattern;
	}

	private enum TokenType
	{
		String,
		Bold,
		Italic,
		Color,
		Size,
		CloseBold,
		CloseItalic,
		CloseColor,
		CloseSize
	}

	private class Lexer
	{
		private char Current()
		{
			return text[position];
		}

		private void Next()
		{
			position++;
		}

		private void StartNewToken()
		{
			tokenStart = position;
		}

		private void StartNewPattern()
		{
			patternStart = position;
			StartNewToken();
		}

		private void Reset()
		{
			tokenStart = patternStart;
		}

		private string Token()
		{
			return text.Substring(tokenStart, position - tokenStart);
		}

		private void Add(TokenType type, object val = null)
		{
			var item = new Token
			{
				Type = type,
				Val = val,
				Pattern = text.Substring(patternStart, position - patternStart)
			};
			tokens.Add(item);
		}

		private void WritePatternString()
		{
			if (patternStart >= position)
			{
				return;
			}
			var num = tokenStart;
			tokenStart = patternStart;
			Add(TokenType.String, Token());
			tokenStart = num;
		}

		private static bool IsValidColorCode(string val)
		{
			if (val.Length == 6 || val.Length == 8)
			{
				return val.All((c) => c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F');
			}
			return false;
		}

		private static object ParseColor(string val)
		{
			string text;
			if (!colorNames.TryGetValue(val.ToLower(), out text) && !IsValidColorCode(val))
			{
				return null;
			}
			text = text ?? val;
			if (text.Length == 6)
			{
				text += "ff";
			}
			return text;
		}

		private static object ParseSize(string val)
		{
			int num;
			if (int.TryParse(val, out num))
			{
				return num;
			}
			return null;
		}

		private State EndTag(TokenType t)
		{
			Next();
			return delegate ()
			{
				if (Current() == ']')
				{
					Next();
					Add(t, null);
					StartNewPattern();
					return new State(Str);
				}
				Reset();
				return new State(Str);
			};
		}

		private State ParamTag(TokenType t, Func<string, object> parse)
		{
			Next();
			StartNewToken();
			State s = null;
			s = delegate ()
			{
				if (Current() != ']')
				{
					Next();
					return s;
				}
				var obj = parse(Token());
				if (obj == null)
				{
					Reset();
					return new State(Str);
				}
				Next();
				Add(t, obj);
				StartNewPattern();
				return new State(Str);
			};
			return s;
		}

		private State CloseTag()
		{
			var c = Current();
			if (c <= '+')
			{
				if (c == '#')
				{
					return EndTag(TokenType.CloseColor);
				}
				if (c == '+')
				{
					return EndTag(TokenType.CloseSize);
				}
			}
			else
			{
				if (c == 'b')
				{
					return EndTag(TokenType.CloseBold);
				}
				if (c == 'i')
				{
					return EndTag(TokenType.CloseItalic);
				}
			}
			Reset();
			return new State(Str);
		}

		private State Tag()
		{
			var c = Current();
			if (c <= '+')
			{
				if (c == '#')
				{
					return ParamTag(TokenType.Color, new Func<string, object>(ParseColor));
				}
				if (c == '+')
				{
					return ParamTag(TokenType.Size, new Func<string, object>(ParseSize));
				}
			}
			else
			{
				if (c == '/')
				{
					Next();
					return new State(CloseTag);
				}
				if (c == 'b')
				{
					return EndTag(TokenType.Bold);
				}
				if (c == 'i')
				{
					return EndTag(TokenType.Italic);
				}
			}
			Reset();
			return new State(Str);
		}

		private State Str()
		{
			if (Current() == '[')
			{
				WritePatternString();
				StartNewPattern();
				Next();
				return new State(Tag);
			}
			Next();
			return new State(Str);
		}

		public static List<Token> Lex(string text)
		{
			var lexer = new Lexer
			{
				text = text
			};
			var state = new State(lexer.Str);
			while (lexer.position < lexer.text.Length)
			{
				state = state();
			}
			lexer.WritePatternString();
			return lexer.tokens;
		}

		private string text;

		private int patternStart;

		private int tokenStart;

		private int position;

		private List<Token> tokens = new List<Token>();

		private delegate State State();
	}

	private class Entry
	{
		public Entry(string pattern, Element e)
		{
			Pattern = pattern;
			Element = e;
		}

		public string Pattern;

		public Element Element;
	}

	private class Tag
	{
		public Tag(string open, string close)
		{
			Open = open;
			Close = close;
		}

		public string Open;

		public string Close;
	}
}

public class Element
{
	private Element(ElementType type, object val)
	{
		Type = type;
		Val = val;
	}

	public static Element String(object s)
	{
		return new Element(ElementType.String, s);
	}

	public static Element Tag(ElementType type)
	{
		return new Element(type, null);
	}

	public static Element ParamTag(ElementType type, object val)
	{
		return new Element(type, val);
	}

	public ElementType Type;

	public object Val;

	public List<Element> Body = new List<Element>();
}

public enum ElementType
{
	String,
	Bold,
	Italic,
	Color,
	Size
}
