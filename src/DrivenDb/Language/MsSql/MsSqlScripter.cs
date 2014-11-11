﻿using System;
using System.Data;
using DrivenDb.Base;

namespace DrivenDb.MsSql
{
   class MsSqlScripter : DbScripter, IMsSqlScripter
   {
      private new readonly Func<IMsSqlBuilder> m_Builders;

      public MsSqlScripter(IDb db, IValueJoiner joiner, Func<IMsSqlBuilder> builders) : base(db, joiner, builders)
      {
         m_Builders = builders;
      }

      public void ScriptInsertWithScopeIdentity<T>(IDbCommand command, T entity, int index, bool returnId)
         where T : IDbEntity, new()
      {
         var builder = m_Builders();
         var parameters = InitializeBuilderForInsert(builder, entity);

         AppendQuery(command, index, builder.ToInsertWithScopeIdentity(index, returnId), parameters);
      }

      //public void ScriptInsertOutputDeleted<T>(IDbCommand command, int index, T entity, string[] deleted) 
      //   where T : IDbEntity, new()
      //{         
      //   var builder = m_Builders();
      //   var parameters = InitializeBuilderForInsert(builder, entity);

      //   AppendQuery(command, index, builder.ToInsertOutputDeleted(entity, index, deleted), parameters);
      //}

      public void ScriptUpdateOutputDeleted<T>(IDbCommand command, int index, T entity, string[] deleted)
         where T : IDbEntity, new()
      {         
         var builder = m_Builders();          
         var parameters = InitializeBuilderForUpdate(builder, entity);

         AppendQuery(command, index, builder.ToUpdateOutputDeleted(index, deleted), parameters);
      }

      public void ScriptDeleteOutputDeleted<T>(IDbCommand command, int index, T entity, string[] deleted)
         where T : IDbEntity, new()
      {
         var builder = m_Builders();
         var parameters = InitializeBuilderForDelete(builder, entity);

         AppendQuery(command, index, builder.ToDeleteOutputDeleted(index, deleted), parameters);
      }
   }
}