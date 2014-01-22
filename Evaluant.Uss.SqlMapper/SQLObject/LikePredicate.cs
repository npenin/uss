using System;
using SQLObject.Renderer;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de LikePredicat.
	/// </summary>
	public class LikePredicate : LogicExpression
	{
		private ISQLExpression _MatchValue;
		private string _Pattern;
		private char _CharacterEscape = ' ';

		public LikePredicate(ISQLExpression match_value, string pattern)
		{
			_MatchValue = match_value;
			_Pattern = pattern;
		}

		public LikePredicate(ISQLExpression match_value, string pattern, char character_escape)
		{
			_MatchValue = match_value;
			_Pattern = pattern;
			_CharacterEscape = character_escape;
		}

		public ISQLExpression MatchValue
		{
			get { return _MatchValue; }
		}

		public string Pattern
		{
			get { return _Pattern; }
		}

		public char CharacterEscape
		{
			get { return _CharacterEscape; }
		}

		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
