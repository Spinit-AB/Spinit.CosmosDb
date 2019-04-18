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
            return this;
        }

        public CollectionModelBuilder<TEntity> Collection<TEntity>(Expression<Func<TDatabase, ICosmosDbCollection<TEntity>>> collectionSelector)
            where TEntity : class, ICosmosEntity
        {
            var collectionId = collectionSelector.GetCollectionId();
            var collectionModel = _databaseModel.CollectionModels.Single(x => x.CollectionId == collectionId); // TODO: add indexed property => Model.CollectionModels[collectionId]
            return new CollectionModelBuilder<TEntity>(collectionModel);
        }

        internal DatabaseModel Build()
        {
            return _databaseModel;
        }
    }
}
