using System;
using System.Linq;
using System.Linq.Expressions;

namespace Spinit.CosmosDb
{
    internal static class ExpressionExtensions
    {
        /// <summary>
        /// Remaps an expression on a type to another type that have the original type as a property.
        /// Used to remap expressions basen on an entity to an expression based on a DbEntry, 
        /// ie (MyEntity) x => x.MyEntityProp == "SomeValue" to (DbEntry&lt;MyEntity&gt;) x => x.Original.MyEntityProp = "SomeValue"
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        internal static Expression<Func<TTarget, TResult>> RemapTo<TTarget, TSource, TResult>(this Expression<Func<TSource, TResult>> source, Expression<Func<TTarget, TSource>> selector)
        {
            var expression = Remapper<TTarget, TSource, TResult>.Map(source, selector) as LambdaExpression;
            return Expression.Lambda<Func<TTarget, TResult>>(expression.Body, selector.Parameters);
        }

        private class Remapper<TTarget, TSource, TResult> : ExpressionVisitor
        {
            private readonly Expression<Func<TTarget, TSource>> _selector;

            Remapper(Expression<Func<TTarget, TSource>> selector)
            {
                _selector = selector;
            }

            public static Expression Map(Expression<Func<TSource, TResult>> expression, Expression<Func<TTarget, TSource>> selector)
            {
                return new Remapper<TTarget, TSource, TResult>(selector).Visit(expression);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter && node.Expression.Type == typeof(TSource))
                    return Expression.PropertyOrField(Visit(_selector.Body), node.Member.Name);
                else
                    return base.VisitMember(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                return Expression.Lambda<Func<TTarget, TResult>>(Visit(node.Body), _selector.Parameters);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                node = _selector.Parameters.Single(x => x.Name == node.Name);
                return base.VisitParameter(node);
            }
        }
    }
}
