using System;
using System.Linq;
using System.Linq.Expressions;

namespace Spinit.CosmosDb
{
    public class DatabaseModelBuilder<TDatabase>
        where TDatabase : CosmosDatabase
    {
        private DatabaseModel _databaseModel { get; }

        internal DatabaseModelBuilder(DatabaseModel databaseModel)
        {
            _databaseModel = databaseModel;
        }

        public DatabaseModelBuilder<TDatabase> DatabaseId(string databaseId)
        {
            _databaseModel.DatabaseId = databaseId;
            foreach (var collectionModel in _databaseModel.CollectionModels)
            {
                collectionModel.DatabaseId = databaseId;
            }
            return this;
        }

        public CollectionModelBuilder<TEntity> Collection<TEntity>(Expression<Func<TDatabase, ICosmosDbCollection<TEntity>>> collectionSelector)
            where TEntity : class, ICosmosEntity
        {
            var propertyName = collectionSelector.GetCollectionPropertyName();
            var collectionModel = _databaseModel.CollectionModels.Single(x => x.PropertyName == propertyName); // TODO: add indexed property => Model.CollectionModels[collectionId]
            return new CollectionModelBuilder<TEntity>(collectionModel);
        }

        internal DatabaseModel Build()
        {
            return _databaseModel;
        }
    }
}
