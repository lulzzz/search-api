using MongoDB.Driver;
using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchV2.MongoDB
{
    public sealed class MongoTextSearch<TData> : ITextSearch<TData>
    {
        readonly IMongoCollection<TData> _mols;
        readonly FilterDefinitionBuilder<TData> _filterBuilder = Builders<TData>.Filter;
        readonly int _hitLimit;
        readonly string[] _textIndexFields;

        public MongoTextSearch(MongoConnector<TData> connector, int hitLimit, params string[] textIndexFields)
        {
            if (textIndexFields.Length < 1) throw new ArgumentException("there must be at least one text index field");

            _mols = connector.Mols;
            _hitLimit = hitLimit;
            _textIndexFields = textIndexFields.ToArray();

            var indexKeys = Builders<TData>.IndexKeys;
            _mols.Indexes.CreateMany(_textIndexFields.Select(f => new CreateIndexModel<TData>(indexKeys.Hashed(f))));
        }

        async Task<IEnumerable<TData>> ITextSearch<TData>.FindText(string text)
        {
            return await (await _mols.FindAsync(_filterBuilder.Or(_textIndexFields.Select(f => _filterBuilder.Eq(f, text))))).ToListAsync();
        }
    }
}
