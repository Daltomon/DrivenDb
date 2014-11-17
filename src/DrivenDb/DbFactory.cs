﻿/**************************************************************************************
 * Original Author : Anthony Leatherwood (adleatherwood@gmail.com)                              
 * Source Location : https://github.com/Fastlite/DrivenDb    
 *  
 * This source is subject to the Microsoft Public License.
 * Link: http://www.microsoft.com/en-us/openness/licenses.aspx
 *  
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
 **************************************************************************************/

using System;
using System.Data;
using DrivenDb.Base;
using DrivenDb.MsSql;
using DrivenDb.SqLite;
using DrivenDb.MySql;
using DrivenDb.Oracle;

namespace DrivenDb
{
   // TODO: review tests to make sure they are universal and clean up after themselves tryf
   public static class DbFactory
   {
      public static IDbAccessor CreateAccessor(DbAccessorType type, IDb db)
      {         
         var mapper = new DbMapper(db);

         Func<ISqlBuilder> builders;
         IValueJoiner joiner;

         switch (type)
         {
            case DbAccessorType.MsSql:
               {
                  Func<IMsSqlBuilder> msbuilders = () => new MsSqlBuilder();

                  var msjoiner = new MsSqlValueJoiner();
                  var msscripter = new MsSqlScripter(db, msjoiner, msbuilders);

                  return new MsSqlAccessor(msscripter, mapper, db);
               }
            case DbAccessorType.SqLite:
               {
                  builders = () => new SqLiteBuilder();
                  joiner = new SqLiteValueJoiner();
               }
               break;
            case DbAccessorType.MySql:
               {
                  builders = () => new MySqlBuilder();
                  joiner = new MySqlValueJoiner();
               }
               break;
            case DbAccessorType.Oracle:
               {
                  builders = () => new OracleBuilder();
                  joiner = new OracleValueJoiner();
               }
               break;
            default:
               throw new InvalidOperationException(String.Format("Unsupported DbAccessorType value of '{0}'", type));
         }

         var scripter = new DbScripter(db, joiner, builders);

         return new DbAccessor(scripter, mapper, db);
      }

      public static IDbAccessor CreateAccessor(DbAccessorType type, Func<IDbConnection> connections)
      {
         return CreateAccessor(type, new Db(type, AccessorExtension.None, connections));
      }

      public static IDbAccessor CreateAccessor(DbAccessorType type, AccessorExtension extensions, Func<IDbConnection> connections)
      {
         return CreateAccessor(type, new Db(type, extensions, connections));
      }

      public static IDbAccessor CreateAccessor(Func<ISqlBuilder> builder, AccessorExtension extensions, Func<IDbConnection> connections)
      {

         var db = new Db(extensions, connections);

         return new DbAccessor(new DbScripter(db, new ValueJoiner(), builder), new DbMapper(db), db);
      }

      public static IDbAccessorSlim CreateSlimAccessor(DbAccessorType type, IDb db)
      {
         return CreateAccessor(type, db);
      }

      public static IDbAccessorSlim CreateSlimAccessor(DbAccessorType type, Func<IDbConnection> connections)
      {
         return CreateAccessor(type, new Db(type, AccessorExtension.None, connections));
      }

      public static IDbAccessorSlim CreateSlimAccessor(DbAccessorType type, AccessorExtension extensions, Func<IDbConnection> connections)
      {
         return CreateAccessor(type, new Db(type, extensions, connections));
      }

      public static IDbAccessorSlim CreateSlimAccessor(Func<ISqlBuilder> builder, AccessorExtension extensions, Func<IDbConnection> connections)
      {
         var db = new Db(extensions, connections);

         return new DbAccessor(new DbScripter(db, new ValueJoiner(), builder), new DbMapper(db), new Db(extensions, connections));
      }
   }
}