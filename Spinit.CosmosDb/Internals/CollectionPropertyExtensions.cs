using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Spinit.CosmosDb
{
    internal static class CollectionPropertyExtensions
    {
        internal static string GetCollectionId<TDatabase, TEntity>(this Expression<Func<TDatabase, ICosmosDbCollection<TEntity>>> collectionSelector)
            where TDatabase : CosmosDatabase
            where TEntity : class, ICosmosEntity
        {
            var collectionProperty = GetCollectionPropertyInfo(collectionSelector);
            return GetCollectionId(collectionProperty);
        }

        internal static string GetCollectionId(this PropertyInfo collectionProperty)
        {
            var result = collectionProperty.GetCustomAttribute<CollectionIdAttribute>()?.CollectionId;
            if (string.IsNullOrEmpty(result))
                return collectionProperty.Name;
            return result;
        }

        internal static PropertyInfo GetCollectionPropertyInfo<TDatabase, TEntity>(this Expression<Func<TDatabase, ICosmosDbCollection<TEntity>>> collectionSelector)
            where TDatabase : CosmosDatabase
            where TEntity : class, ICosmosEntity
        {
            var lambdaExpressionBody = collectionSelector.Body;

            if (lambdaExpressionBody as MemberExpression != null)
            {
                return ((MemberExpression)lambdaExpressionBody).Member as PropertyInfo;
            }

            if (lambdaExpressionBody as UnaryExpression != null)
            {
                var unary = ((UnaryExpression)lambdaExpressionBody);
                return ((MemberExpression)unary.Operand).Member as PropertyInfo;
            }

            throw new NotSupportedException("Not supported at the moment");
        }
    }
}
