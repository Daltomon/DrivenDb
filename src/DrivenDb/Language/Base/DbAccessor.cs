/**************************************************************************************
 * Original Author : Anthony Leatherwood (adleatherwood@gmail.com)
 * Source Location : http://drivendb.codeplex.com
 *
 * This source is subject to the Microsoft Public License.
 * Link: http://www.microsoft.com/en-us/openness/licenses.aspx
 *
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 **************************************************************************************/

using DrivenDb.Language.Base;
using DrivenDb.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace DrivenDb.Base
{
   internal class DbAccessor : IDbAccessor
   {      
      private readonly IDb m_Db;
      private readonly IDbAggregator m_Aggregator;
      private readonly IDbMapper m_Mapper;
      private readonly IDbScripter m_Scripter;

      public DbAccessor(IDbScripter scripter, IDbMapper mapper, IDb db, IDbAggregator aggregator)
      {
         m_Scripter = scripter;
         m_Mapper = mapper;
         m_Db = db;
         m_Aggregator = aggregator;

         CommandTimeout = 600; // ten minutes
      }

      public int CommandTimeout
      {
         get;
         set;
      }

      public T ReadIdentity<T, K>(K key)
         where T : IDbRecord, new()
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptIdentitySelect<T>(command, key);

            LogMessage(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               return m_Mapper.MapEntity<T>(command.CommandText, reader);
            }
         }
      }

      public IOnJoiner<P, C> ReadRelated<P, C>(P parent)
         where P : IDbRecord, new()
         where C : IDbRecord, new()
      {
         return new EntityJoiner<P, C>(CommandTimeout, m_Db, m_Scripter, m_Mapper, new[] {parent});
      }

      public IOnJoiner<P, C> ReadRelated<P, C>(IEnumerable<P> parents)
         where P : IDbRecord, new()
         where C : IDbRecord, new()
      {
         return new EntityJoiner<P, C>(CommandTimeout, m_Db, m_Scripter, m_Mapper, parents);
      }

      public IEnumerable<T> ReadValues<T>(string query, params object[] parameters)
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               return m_Mapper.MapValues<T>(reader);
            }
         }
      }

      public T ReadValue<T>(string query, params object[] parameters)
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               return m_Mapper.MapValue<T>(reader);
            }
         }
      }

      public IEnumerable<T> ReadAnonymous<T>(T model, string query, params object[] parameters)
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               return m_Mapper.MapAnonymous(model, command.CommandText, reader);
            }
         }
      }

      public IEnumerable<T> ReadType<T>(string query, params object[] parameters)
         where T : new()
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               return m_Mapper.MapType<T>(command.CommandText, reader);
            }
         }
      }

      public IEnumerable<T> ReadType<T>(Func<T> factory, string query, params object[] parameters)
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               return m_Mapper.MapType(command.CommandText, reader, factory);
            }
         }
      }

      public T ReadEntity<T>(string query, params object[] parameters)
         where T : IDbRecord, new()
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               return m_Mapper.MapEntity<T>(command.CommandText, reader);
            }
         }
      }

      public IEnumerable<T> ReadEntities<T>(string query, params object[] parameters)
         where T : IDbRecord, new()
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               return m_Mapper.MapEntities<T>(command.CommandText, reader);
            }
         }
      }

      public DbSet<T1, T2> ReadEntities<T1, T2>(string query, params object[] parameters)
         where T1 : IDbRecord, new()
         where T2 : IDbRecord, new()
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               var set1 = m_Mapper.ParallelMapEntities<T1>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(2, 1);
               var set2 = m_Mapper.ParallelMapEntities<T2>(command.CommandText, reader);

               return new DbSet<T1, T2>(set1, set2);
            }
         }
      }
      
      public DbSet<T1, T2, T3> ReadEntities<T1, T2, T3>(string query, params object[] parameters)
         where T1 : IDbRecord, new()
         where T2 : IDbRecord, new()
         where T3 : IDbRecord, new()
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               var set1 = m_Mapper.ParallelMapEntities<T1>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(3, 1);
               var set2 = m_Mapper.ParallelMapEntities<T2>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(3, 2);
               var set3 = m_Mapper.ParallelMapEntities<T3>(command.CommandText, reader);

               return new DbSet<T1, T2, T3>(set1, set2, set3);
            }
         }
      }

      public DbSet<T1, T2, T3, T4> ReadEntities<T1, T2, T3, T4>(string query, params object[] parameters)
         where T1 : IDbRecord, new()
         where T2 : IDbRecord, new()
         where T3 : IDbRecord, new()
         where T4 : IDbRecord, new()
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               var set1 = m_Mapper.ParallelMapEntities<T1>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(4, 1);
               var set2 = m_Mapper.ParallelMapEntities<T2>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(4, 2);
               var set3 = m_Mapper.ParallelMapEntities<T3>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(4, 3);
               var set4 = m_Mapper.ParallelMapEntities<T4>(command.CommandText, reader);

               return new DbSet<T1, T2, T3, T4>(set1, set2, set3, set4);
            }
         }
      }

      public DbSet<T1, T2, T3, T4, T5> ReadEntities<T1, T2, T3, T4, T5>(string query, params object[] parameters)
         where T1 : IDbRecord, new()
         where T2 : IDbRecord, new()
         where T3 : IDbRecord, new()
         where T4 : IDbRecord, new()
         where T5 : IDbRecord, new()
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               var set1 = m_Mapper.ParallelMapEntities<T1>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 1);
               var set2 = m_Mapper.ParallelMapEntities<T2>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 2);
               var set3 = m_Mapper.ParallelMapEntities<T3>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 3);
               var set4 = m_Mapper.ParallelMapEntities<T4>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 4);
               var set5 = m_Mapper.ParallelMapEntities<T5>(command.CommandText, reader);

               return new DbSet<T1, T2, T3, T4, T5>(set1, set2, set3, set4, set5);
            }
         }
      }

      public void WriteEntity(IDbRecord entity)
      {
         WriteEntities(new[] { entity });
      }

      public void WriteEntity(IDbRecord entity, bool returnId)
      {
         WriteEntities(new[] { entity }, returnId);
      }

      public void WriteEntities(IEnumerable<IDbRecord> entities)
      {
         WriteEntities(null, null, entities, null, null, null, null, true);
      }

      public void WriteEntities(IEnumerable<IDbRecord> entities, bool returnIds)
      {
         WriteEntities(null, null, entities, null, null, null, null, returnIds);
      }

      public void WriteAggregate(IDbAggregate aggregate)
      {
         m_Aggregator.WriteAggregate(this, aggregate);
      }

      public void WriteAggregates(IEnumerable<IDbAggregate> aggregates)
      {
         m_Aggregator.WriteAggregates(this, aggregates);
      }

      internal void TransactEntity<T>(IDbConnection connection, IDbTransaction transaction, T entity, bool returnId)
         where T : IDbRecord
      {
         TransactEntities(connection, transaction, new[] { entity }, returnId);
      }

      internal void TransactEntities<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities, bool returnIds)
         where T : IDbRecord
      {
         WriteEntities(connection, transaction, entities, null, null, null, null, returnIds);
      }

      internal void WriteEntities<T>(
         IDbConnection openConnection,
         IDbTransaction openTransaction,
         IEnumerable<T> entities,
         Action<IDbCommand, int, T> insert,
         Action<IDbCommand, int, T> update,
         Action<IDbCommand, int, T> delete,
         Action<int, T, object[]> output,
         bool returnIds
         )
         where T : IDbRecord
      {
         if (entities == null || !entities.Any() || entities.All(e => e.State == EntityState.Current))
         {
            return;
         }

         var connection = openConnection ?? m_Db.CreateConnection();

         using (var disposables = new DisposableCollection())
         {
            if (openConnection == null)
            {
               disposables.Add(connection);
            }

            if (connection.State != ConnectionState.Open)
            {
               connection.Open();
            }

            var command = connection.CreateCommand();
            var transaction = openTransaction ?? command.Connection.BeginTransaction();

            if (openTransaction == null)
            {
               disposables.Add(transaction);
            }

            command.CommandTimeout = CommandTimeout;

            disposables.Add(command);

            command.Transaction = transaction;

            var array = entities.ToArray();
            var committed = false;

            try
            {
               for (var i = 0; i < array.Length; i++)
               {
                  var entity = array[i];

                  switch (entity.State)
                  {
                     case EntityState.New:
                        if (insert != null)
                           insert(command, i, entity);
                        else
                           m_Scripter.ScriptInsert(command, i, entity, returnIds);
                        break;

                     case EntityState.Modified:
                        if (update != null)
                           update(command, i, entity);
                        else
                           m_Scripter.ScriptUpdate(command, i, entity);
                        break;

                     case EntityState.Deleted:
                        if (delete != null)
                           delete(command, i, entity);
                        else
                           m_Scripter.ScriptDelete(command, i, entity);
                        break;
                  }

                  if (command.Parameters.Count >= m_Db.ParameterLimit || i.ToString().Length >= m_Db.ParameterNameLimit || i == array.Length - 1)
                  {
                     if (String.IsNullOrWhiteSpace(command.CommandText))
                     {
                        break;
                     }

                     LogMessage(command.CommandText);

                     if (returnIds)
                     {
                        using (var reader = command.ExecuteReader())
                        {
                           do
                           {
                              while (reader.Read())
                              {
                                 var index = reader.GetInt32(0);

                                 if (array[index].State == EntityState.New)
                                 {
                                    if (reader.GetFieldType(1) == typeof(int))
                                    {
                                       var identity = reader.GetInt32(1);
                                       array[index].SetIdentity(identity);
                                    }
                                    else
                                    {
                                       var identity = reader.GetInt64(1);
                                       array[index].SetIdentity(identity);
                                    }
                                 }

                                 if (array[index].State != EntityState.New && output != null)
                                 {
                                    var values = new object[reader.FieldCount];

                                    reader.GetValues(values);

                                    output(index, array[index], values);
                                 }
                              }
                           } while (reader.NextResult());
                        }
                     }
                     else
                     {
                        command.ExecuteNonQuery();
                     }

                     if (i < array.Length - 1)
                     {
                        command = connection.CreateCommand();
                        command.CommandTimeout = CommandTimeout;
                        command.Transaction = transaction;

                        disposables.Add(command);
                     }
                  }
               }

               if (openTransaction == null)
               {
                  transaction.Commit();
               }

               committed = true;

               var changes = GetChanges(entities);

               array.ToList().ForEach(e =>
                  {
                     if (  (returnIds && e.State != EntityState.Deleted)
                        || (!returnIds && e.State != EntityState.New && e.State != EntityState.Deleted)
                        )
                     {
                        e.Reset();
                     }
                  });

               NotifyChanges(changes);
            }
            catch(Exception)
            {
               try
               {
                  if (!committed)
                  {
                     transaction.Rollback();
                  }
               }
               // ReSharper disable EmptyGeneralCatchClause
               catch
               // ReSharper restore EmptyGeneralCatchClause
               {
               }

               throw;
            }
         }
      }

      internal void Execute(IDbConnection connection, IDbTransaction transaction, string query, params object[] parameters)
      {
         using (var command = connection.CreateCommand())
         {
            if (connection.State != ConnectionState.Open)
            {
               connection.Open();
            }

            command.CommandTimeout = CommandTimeout;
            command.Transaction = transaction;

            m_Scripter.ScriptExecute(command, query, parameters);

            LogMessage(command.CommandText);

            command.ExecuteNonQuery();
         }
      }

      public void Execute(string query, params object[] parameters)
      {
         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            if (connection.State != ConnectionState.Open)
            {
               connection.Open();
            }

            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptExecute(command, query, parameters);

            LogMessage(command.CommandText);

            command.ExecuteNonQuery();
         }
      }

      public event EventHandler<DbChangeEventArgs> Inserted;

      public event EventHandler<DbChangeEventArgs> Updated;

      public event EventHandler<DbChangeEventArgs> Deleted;

      public TextWriter Log
      {
          get;
          set;
      }

      IFallbackAccessorSlim IDbAccessorSlim.Fallback
      {
         get { return new FallbackAccessorSlim(this); }
      }

      public IDbScope CreateScope()
      {
         return new DbScope(m_Db, this);
      }

      private void NotifyChanges(IEnumerable<DbChange> changes)
      {
         if (changes == null || !changes.Any())
         {
            return;
         }

         var groups = changes.GroupBy(c => c.ChangeType).ToArray();

         foreach (var group in groups)
         {
            if (group.Key == DbChangeType.Inserted && Inserted != null)
            {
               Inserted(this, new DbChangeEventArgs(group.ToArray()));
            }

            if (group.Key == DbChangeType.Updated && Updated != null)
            {
               Updated(this, new DbChangeEventArgs(group.ToArray()));
            }

            if (group.Key == DbChangeType.Deleted && Deleted != null)
            {
               Deleted(this, new DbChangeEventArgs(group.ToArray()));
            }
         }
      }

      private IEnumerable<DbChange> GetChanges<T>(IEnumerable<T> entities)
         where T : IDbRecord
      {
         if (Inserted != null || Updated != null || Deleted != null)
         {
            var changes = new List<DbChange>();

            foreach (var entity in entities)
            {
               if (
                     (entity.State == EntityState.New && Inserted != null)
                  || (entity.State == EntityState.Modified && Updated != null)
                  || (entity.State == EntityState.Deleted && Deleted != null)
                  )
               {
                  DbChangeType changeType;

                  switch (entity.State)
                  {
                     case EntityState.New:
                        changeType = DbChangeType.Inserted;
                        break;

                     case EntityState.Modified:
                        changeType = DbChangeType.Updated;
                        break;

                     case EntityState.Deleted:
                        changeType = DbChangeType.Deleted;
                        break;

                     default:
                        throw new InvalidOperationException(String.Format("Invalid EntityState value of '{0}'", entity.State));
                  }

                  var affected = entity.State == EntityState.Modified
                                    ? entity.Changes.ToArray()
                                    : default(IEnumerable<string>);

                  var change = new DbChange(changeType, entity.Table.Name, affected, entity);

                  changes.Add(change);
               }
            }

            return changes;
         }

         return null;
      }

      private void LogMessage(string message)
      {
          if (Log != null)
          {
              Log.WriteLine(message);
          }
      }
   }
}