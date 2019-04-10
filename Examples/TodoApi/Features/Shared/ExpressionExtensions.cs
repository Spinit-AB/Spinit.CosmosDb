using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TodoApi.Features.Shared
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Combines two expressions using AndAlso (&&) and uses parameters from the first expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            if (first == null)
                return second;
            return first.Compose(second, Expression.AndAlso);
        }

        /// <summary>
        /// Combines two expressions using OrElse (||) and uses parameters from the first expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            if (first == null)
                return second;
            return first.Compose(second, Expression.OrElse);
        }

        private static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> mergeFunc)
        {
            var map = first.Parameters
                .Select((f, i) => new { f, s = second.Parameters[i] })
                .ToDictionary(p => p.s, p => p.f);
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
            return Expression.Lambda<T>(mergeFunc(first.Body, secondBody), first.Parameters);
        }

        private class ParameterRebinder : ExpressionVisitor
        {
            private readonly IDictionary<ParameterExpression, ParameterExpression> _parametersMap;

            ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> parametersMap)
            {
                _parametersMap = parametersMap ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> parametersMap, Expression expression)
            {
                return new ParameterRebinder(parametersMap).Visit(expression);
            }

            protected override Expression VisitParameter(ParameterExpression parameter)
            {
                if (_parametersMap.TryGetValue(parameter, out ParameterExpression replacement))
                {
                    parameter = replacement;
                }

                return base.VisitParameter(parameter);
            }
        }
    }
}
