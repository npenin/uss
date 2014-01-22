using System;
using System.Collections;
using Evaluant.OPath;
using Evaluant.OPath.Expressions;

using Evaluant.Uss.Common;
using Evaluant.Uss.Models;

namespace Evaluant.Uss.Xml
{
	class XPathTransformer : OPathVisitor
	{
		private Model _Model;
        private Stack _ExprLevel;   // indicate if transforming a path which return an node set or an expression which return a scalar
        private static readonly int EXPR = 0;
        private static readonly int PATH = 1;

		private string _Result = String.Empty;
		public string Result
		{
			get { return _Result; }
		}

        public XPathTransformer(bool allowAttributeAtEnd, Model model, Stack level)
        {
            _ExprLevel = level;
            _AllowAttributeAtEnd = allowAttributeAtEnd;
            _Model = model;
        }

		public XPathTransformer(bool allowAttributeAtEnd, Model model)
		{
            _ExprLevel = new Stack();
			_AllowAttributeAtEnd = allowAttributeAtEnd;
			_Model = model;
		}

		public XPathTransformer()
		{
            _ExprLevel = new Stack();
			_AllowAttributeAtEnd = false;
		}

		private bool _AllowAttributeAtEnd;
		
		public string ConvertToXPath(string opath)
		{
            OPathQuery query = new OPathQuery(opath);
			query.Compile();

			if(query.HasErrors)
				throw new OPathException(query);

			return ConvertToXPath(query);
		}

        public string ConvertToScalarXPath(string opath)
        {
            OPathQuery query = new OPathQuery(opath, OPathQueryTypeEnum.Expression);
            query.Compile();

            if (query.HasErrors)
                throw new OPathException(query);

            return ConvertToXPath(query);
        }

		internal string ConvertToXPath(OPathQuery query)
		{
            string xpath = String.Empty;

            switch(query.QueryType)
            {
                case OPathQueryTypeEnum.Path:
			        if(query.Path == null)
				        throw new ArgumentNullException("query.Path");

                    if (query.Path.Identifiers.Count == 0)
                        throw new ArgumentNullException("path", "Must have at least one Identifier");

                    _ExprLevel.Push(PATH);
                    xpath = ConvertToXPath(query.Path, true, false, false);
                    break;

                case OPathQueryTypeEnum.Expression:
                    if(query.Expression == null)
                        throw new ArgumentNullException("query.Expression");

                    _ExprLevel.Push(EXPR);
                    xpath = ConvertToXPath(query.Expression, true, false, false);
                    break;

            }

			return xpath;
		}

		/// <summary>
		/// Converts a Constraint to the corresponding xpath.
		/// </summary>
		/// <param name="constraint">Constraint.</param>
		/// <param name="attributeAtEnd"></param>
		/// <returns></returns>
		private string ConvertToXPath(Constraint constraint, bool attributeAtEnd)
		{
            _ExprLevel.Push((int)_ExprLevel.Peek());

			// We must do as if the first identifier is not a Type but a reference or attribute's name.
			// The first case is taken into account by the same method's Query overload
			
			XPathTransformer transformer = new XPathTransformer(attributeAtEnd, _Model, _ExprLevel);
			constraint.Accept(transformer);

            _ExprLevel.Pop();
			return transformer.Result;
		}

		/// <summary>
		/// Converts the Path to a xpath expression.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="firstIsType">True if the first identifer is a type.</param>
		/// <param name="lastIsAttribute">True if the latest identifer is an attribute.</param>
		/// <param name="isInConstraint"><see langword="true"/> if [is in constraint]; otherwise, <see langword="false"/>.</param>
		/// <returns></returns>
		private string ConvertToXPath(Path path, bool firstIsType, bool lastIsAttribute, bool isInConstraint)
		{
			
			// //Entity[@Id = (//Entity[@Type = 'Person']/Reference[@Role = 'Partners']/@RefId)]

			string xpath = String.Empty;
			Identifier ident = null;
			int index = 0;
			bool hasType = false;
			bool hasRef = false;

			// Process the first element differently if it is a Type (Person) and not a relation/atribute (Children/Age)
			if(firstIsType)
			{
				ident = path.Identifiers[index++];

				ArrayList types = new ArrayList();
				foreach(Models.Entity e in _Model.GetTree(ident.Value))
					types.Add(e.Type);

				string typeFilter;
				typeFilter = String.Concat("@Type = '", String.Join("' or @Type = '", (string[])types.ToArray(typeof(string))), "'");

				if(ident.Constraint == null) // Person
					xpath = String.Format("//Entity[{0}]", typeFilter);
				else // Person[...]
					xpath = String.Format("//Entity[({0}) and ({1})]", typeFilter, ConvertToXPath(ident.Constraint, true));

				hasType = true;
			}

			int lastIndex = lastIsAttribute ? path.Identifiers.Count - 1 : path.Identifiers.Count;

			// Process each succeeding Identifier
			for(; index < lastIndex; index++)
			{
				ident = path.Identifiers[index];
                
				if(hasRef || hasType)
					xpath += "/";

				if(isInConstraint)
				{
					if(ident.Constraint == null) // Person
						xpath = String.Format("id({0}Reference[@Role = '{1}']/@RefId)", xpath, ident.Value);
					else // Person[...]
						xpath = String.Format("id({0}Reference[@Role = '{1}']/@RefId)/self::*[{2}]", xpath, ident.Value, ConvertToXPath(ident.Constraint, false));
				}
				else
				{
					if(ident.Constraint == null) // Person
						xpath = String.Format("id( {0}Reference[@Role = '{1}']/@RefId)", xpath, ident.Value);
					else // Person[...]
						xpath = String.Format("id( {0}Reference[@Role = '{1}']/@RefId)/self::*[{2}]", xpath, ident.Value, ConvertToXPath(ident.Constraint, false));
				}

				hasRef = true;
			}

			// Attribute[@Name = 'attribute_name']
			if(lastIsAttribute)
			{
				ident = path.Identifiers[index];

				if(ident.Constraint != null)
					throw new PersistenceEngineException("Constraint not allowed here");

				if(hasRef || hasType)
					xpath += "/";

				xpath += String.Format("Attribute[@Name = '{0}']", ident.Value);
			}

			return xpath;
		}

        /// <summary>
        /// Converts the Expression to a xpath expression.
        /// </summary>
        /// <param name="path">Expression.</param>
        /// <param name="firstIsType">True if the first identifer is a type.</param>
        /// <param name="lastIsAttribute">True if the latest identifer is an attribute.</param>
        /// <param name="isInConstraint"><see langword="true"/> if [is in constraint]; otherwise, <see langword="false"/>.</param>
        /// <returns></returns>
        private string ConvertToXPath(Call call, bool firstIsType, bool lastIsAttribute, bool isInConstraint)
        {
            // eval(count(...) + 3 + 2 * sum(...))

            string xpath = String.Empty;

            if (call.Name == "eval")
            {
                // evaluate a scalar expression
		        XPathTransformer transformer = new XPathTransformer(true, _Model, _ExprLevel);
		        call.Operands[0].Accept(transformer);
                xpath = transformer.Result;
            }

            return xpath;
        }


		// Process constraints

		public override void Visit(Constraint constraint) { throw new NotImplementedException(); }
		public override void Visit(Path path)
		{
            bool firstIsType = false;

            if ((int)_ExprLevel.Peek() == EXPR)
                firstIsType = true;
            _ExprLevel.Push(PATH);

            _Result = ConvertToXPath(path, firstIsType, _AllowAttributeAtEnd, true);
            
            _ExprLevel.Pop();
		}

		public override void Visit(Value val) 
		{
            _ExprLevel.Push((int)_ExprLevel.Peek());

			object value = val.GetValue();
			_Result = Utils.ConvertToString(value, value.GetType());

            _ExprLevel.Pop();
		}

		public override void Visit(Expression expression) { throw new NotImplementedException(); }

		public override void Visit(Function function) 
		{
            _ExprLevel.Push((int)_ExprLevel.Peek());

			XPathTransformer transformer;

			switch(function.Type)
			{
				case FunctionEnum.Average : 
                    transformer = new XPathTransformer(true, _Model, _ExprLevel);
                    function.Path.Accept(transformer);
                    _Result = String.Concat(" sum( ", transformer.Result, " ) div count( ", transformer.Result ," )");
                    break;
				
				case FunctionEnum.Count :
				    transformer = new XPathTransformer(false, _Model, _ExprLevel);
					function.Path.Accept(transformer);
					_Result = String.Concat(" count( ", transformer.Result, " )");
					break;

				case FunctionEnum.Exists :
                    transformer = new XPathTransformer(false, _Model, _ExprLevel);
					function.Path.Accept(transformer);
					_Result = String.Concat(" count( ", transformer.Result, " ) > 0");
					break;

				case FunctionEnum.IsNull :
                    transformer = new XPathTransformer(true, _Model, _ExprLevel);
					function.Path.Accept(transformer);
					_Result = String.Concat(" count( ", transformer.Result, " ) = 0");
					break;

				case FunctionEnum.Max : 
					/// TODO: Needs exslt extensions
                    throw new NotImplementedException("The max() function is not implemented for this engine");

				case FunctionEnum.Min :
                    /// TODO: Needs exslt extensions					
                    throw new NotImplementedException("The min() function is not implemented for this engine");

				case FunctionEnum.Sum : 
                    transformer = new XPathTransformer(true, _Model, _ExprLevel);
					function.Path.Accept(transformer);
					_Result = String.Concat(" sum( ", transformer.Result, " )");
					break;

			}

            _ExprLevel.Pop();
		}

		public override void Visit(BinaryOperator binaryop) 
		{
            _ExprLevel.Push((int)_ExprLevel.Peek());

			XPathTransformer transformer = new XPathTransformer(true, _Model, _ExprLevel);
			
			binaryop.LeftOperand.Accept(transformer);

			string leftValue = String.Empty, rightValue = String.Empty;

			if(binaryop.LeftOperand is Value)
			{
				if(binaryop.RightOperand is Function)
					leftValue = transformer.Result;
				else
					leftValue = FormatValue(transformer.Result);
			}
			else
				leftValue = transformer.Result;

			binaryop.RightOperand.Accept(transformer);

			if(binaryop.RightOperand is Value)
			{
				if(binaryop.LeftOperand is Function)
					rightValue = transformer.Result;
				else
					rightValue = FormatValue(transformer.Result);
			}
			else
				rightValue = transformer.Result;


			_Result = String.Format("({0})", BinaryOperatorToString(binaryop.Type, leftValue, rightValue));

            _ExprLevel.Pop();
		}

		public override void Visit(UnaryOperator unaryop) 
		{
            _ExprLevel.Push((int)_ExprLevel.Peek());

			XPathTransformer transformer = new XPathTransformer(true, _Model, _ExprLevel);
			unaryop.Operand.Accept(transformer);

			_Result = UnaryOperatorToString(unaryop.Type) + "(" + transformer.Result + " )";

            _ExprLevel.Pop();
		}

		public override void Visit(Identifier identifier) { throw new NotImplementedException(); }
		
		public override void Visit(Call call) 
		{
            _ExprLevel.Push((int)_ExprLevel.Peek());

			if(call.Name == "id")
			{
				XPathTransformer transformer = new XPathTransformer(false, _Model, _ExprLevel);
				call.Operands[0].Accept(transformer);

				string strQuote = transformer.Result.IndexOf("'") > 0 ? "\"" : "'"; // if id contains ' limiter is " else '
				_Result = String.Concat(" @Id = concat( @Type, ':', ", strQuote, transformer.Result, strQuote, ")");

				for(int i=1; i<call.Operands.Count; i++)
				{
					call.Operands[i].Accept(transformer);
					strQuote = transformer.Result.IndexOf("'") > 0 ? "\"" : "'"; // if id contains ' limiter is " else '
					_Result += String.Concat(" or @Id = concat( @Type, ':', ", strQuote, transformer.Result, strQuote, ")");
				}

			}

            _ExprLevel.Pop();
		} 

		private static string UnaryOperatorToString(UnaryOperatorEnum unaryop)
		{
			switch(unaryop)
			{
				case UnaryOperatorEnum.Minus : return " - ";
				case UnaryOperatorEnum.Not : return " not ";
			}

			return String.Empty;
		}

		private static string BinaryOperatorToString(BinaryOperatorEnum binaryop, string leftValue, string rightValue)
		{
			string pattern = String.Empty;

			switch(binaryop)
			{
				case BinaryOperatorEnum.And : pattern =  " {0} and {1}"; break;
				case BinaryOperatorEnum.Div : pattern = " {0} div {1}"; break;
				case BinaryOperatorEnum.Equal : pattern = " {0} = {1} "; break;
				case BinaryOperatorEnum.Greater : pattern = " {0} > {1} "; break;
				case BinaryOperatorEnum.GreaterOrEqual : pattern = " {0} >= {1} "; break;
				case BinaryOperatorEnum.Lesser : pattern = " {0} < {1} "; break;
				case BinaryOperatorEnum.LesserOrEqual : pattern = " {0} <= {1} "; break;
				case BinaryOperatorEnum.Minus : pattern = " {0} - {1} "; break;
				case BinaryOperatorEnum.Modulo : pattern = " {0} mod {1} "; break;
				case BinaryOperatorEnum.NotEqual : pattern = " {0} != {1} "; break;
				case BinaryOperatorEnum.Or : pattern = " {0} or {1} "; break;
				case BinaryOperatorEnum.Plus : pattern = " {0} + {1} "; break;
				case BinaryOperatorEnum.Times : pattern = " {0} * {1} "; break;
				case BinaryOperatorEnum.Contains : pattern = " contains({0}, {1}) "; break;
				case BinaryOperatorEnum.BeginsWith : pattern = " starts-with({0}, {1}) "; break;
				case BinaryOperatorEnum.EndsWith : pattern = " ends-with({0}, {1}) "; break;
			}

			return String.Format(pattern, leftValue, rightValue);
		}

		private string FormatValue(string s)
		{
			// The norm says it must be enclosed in " ... " if it contains a single quote
			if(s.IndexOf("'") != -1)
				return "\"" + s + "\"";
			else
				return "'" + s + "'";
		}
	}
}
