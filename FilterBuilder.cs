    public static class PredicateBuilder
    {
        private const string AND = " and ";
        private const string OR = " or ";

        private static Dictionary<ExpressionType, String> ops = new Dictionary<ExpressionType, String>();

        static PredicateBuilder()
        {
            ops.Add( ExpressionType.Equal," = ");
            ops.Add( ExpressionType.NotEqual," != ");
            ops.Add( ExpressionType.GreaterThan," > ");
            ops.Add( ExpressionType.GreaterThanOrEqual," >= ");
            ops.Add( ExpressionType.LessThan," <  ");
            ops.Add(ExpressionType.LessThanOrEqual, " <=  ");
            ops.Add(ExpressionType.And, " and  ");
            ops.Add(ExpressionType.AndAlso, " and  ");
            ops.Add(ExpressionType.Or, " or  ");
            ops.Add(ExpressionType.OrElse, " or  ");
        }

        public static string MakeFilter<T>(Expression<Func<T, bool>> predicate)
        {
            var member = predicate.Body as BinaryExpression;
            if (predicate == null || member == null)
            {
                throw new ArgumentNullException("Your predicate is not in correct format!!!");
            }

            var sql = CheckExpression(member);
            return sql;

        }

        static string makeMethodCallPredicate(MethodCallExpression expression)
        {
            StringBuilder sql = new StringBuilder();
            if (expression.Method.Name == "StartsWith")
            {
                sql.Append(string.Format("{0} like '{1}%'", (expression.Object as MemberExpression).Member.Name, expression.Arguments[0]).Replace("\"", string.Empty));

            }
            return sql.ToString();
        }

        static string makeOperationPredicate(BinaryExpression expression)
        {
            StringBuilder sql = new StringBuilder();
            if (expression.Left.NodeType == ExpressionType.MemberAccess)
            {
                sql.Append((expression.Left as MemberExpression).Member.Name);

            }
            if (expression.Left.NodeType == ExpressionType.Constant)
            {
                sql.Append((expression.Left as ConstantExpression).Value);
            }

            sql.Append(ops[expression.NodeType]);         

            if (expression.Right.NodeType == ExpressionType.MemberAccess)
            {
                sql.Append((expression.Right as MemberExpression).Member.Name);
            }

            if (expression.Right.NodeType == ExpressionType.Constant)
            {
                var value = getValue(expression.Right as ConstantExpression);

                if (value == "null")
                {
                    sql.Replace("=", "is");
                }

                sql.Append(value);
            }

            return sql.ToString();
        }

        static object getValue(ConstantExpression expression)
        {

            switch (expression.Type.ToString())
            {
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                    return expression.Value;
                case "System.Object":
                    return "null";
                default:
                    return "'" + expression.Value + "'";
            }
        }

        static string CheckExpression(BinaryExpression expression)
        {
            StringBuilder sql = new StringBuilder();
            if (isOperation(expression.NodeType))
            {
                sql.Append(makeOperationPredicate(expression));
            }
            if (expression.Left.NodeType == ExpressionType.AndAlso)
            {
                sql.Append(CheckExpression(expression.Left as BinaryExpression));
            }

            if (expression.Left.NodeType == ExpressionType.OrElse)
            {
                sql.Append(CheckExpression(expression.Left as BinaryExpression));
            }

            if (expression.Left.NodeType == ExpressionType.Call)
            {
                sql.Append(makeMethodCallPredicate(expression.Left as MethodCallExpression));
            }

            if (isOperation(expression.Left.NodeType))
            {
                sql.Append(makeOperationPredicate(expression.Left as BinaryExpression));
            }

            if (expression.NodeType == ExpressionType.OrElse)
            {
                sql.Append(OR);
            }

            if (expression.NodeType == ExpressionType.AndAlso)
            {
                sql.Append(AND);
            }

            if (expression.Right.NodeType == ExpressionType.AndAlso)
            {
                sql.Append(CheckExpression(expression.Right as BinaryExpression));
            }

            if (expression.Right.NodeType == ExpressionType.OrElse)
            {
                sql.Append(CheckExpression(expression.Right as BinaryExpression));
            }

            if (expression.Right.NodeType == ExpressionType.Call)
            {

                sql.Append(makeMethodCallPredicate(expression.Right as MethodCallExpression));
            }

            if (isOperation(expression.Right.NodeType))
            {
                sql.Append(makeOperationPredicate(expression.Right as BinaryExpression));
            }

            return sql.ToString();
        }

        static bool isOperation(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:

                case ExpressionType.NotEqual:

                case ExpressionType.GreaterThan:

                case ExpressionType.GreaterThanOrEqual:

                case ExpressionType.LessThan:

                case ExpressionType.LessThanOrEqual:
                    return true;

            }
            return false;
        }
    }
