using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CH.Models
{
	static public class Extensions
	{
		static public string GetPropertyName(this LambdaExpression expression)
		{
			return GetPropertyName(expression.Body);
		}

		static public string GetPropertyName(this Expression expression)
		{
			if (expression.NodeType == ExpressionType.Convert)
				return GetPropertyName(((UnaryExpression)expression).Operand);

			if (expression.NodeType == ExpressionType.MemberAccess)
			{
				string flatText = expression.ToString();
				string[] array = flatText.Split('.');
				if (array.Length > 1)
					return string.Join(".", array.Skip(1));
			}

			return expression.ToString();
		}
	}
}
