/**************************************************************************************
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

namespace DrivenDb
{
   internal class Db : IDb
   {      
      private readonly DbAccessorType _type;
      private readonly AccessorExtension _extensions;
      private readonly Func<IDbConnection> _connections;

      public Db(AccessorExtension extensions, Func<IDbConnection> connections)
         : this(DbAccessorType.MsSql, extensions, connections)
      {         
      }

      public Db(DbAccessorType type, AccessorExtension extensions, Func<IDbConnection> connections)
      {         
         _type = type;
         _extensions = extensions;
         _connections = connections;

         switch(_type)
         {
            case DbAccessorType.MsSql:
               ParameterLimit = 1000;     // half of what is available
               ParameterNameLimit = 64;   // half of what is available
               break;
            default:
               ParameterLimit = 1000;     // using mssql's as a base
               ParameterNameLimit = 64;   // using mssql's as a base
               break;
         }
      }

      public DbAccessorType Type
      {
         get { return _type; }
      }

      public int ParameterLimit
      {
         get;
         private set;
      }

      public int ParameterNameLimit
      {
         get;
         private set;
      }

      public bool AllowEnumerableParameters
      {
         get { return (_extensions & AccessorExtension.AllowEnumerableParameters) == AccessorExtension.AllowEnumerableParameters; }
      }

      public bool LimitDateParameters
      {
         get { return (_extensions & AccessorExtension.LimitDateParameters) == AccessorExtension.LimitDateParameters; }
      }

      public bool AllowUnmappedColumns
      {
         get { return (_extensions & AccessorExtension.AllowUnmappedColumns) == AccessorExtension.AllowUnmappedColumns; }
      }

      public bool CaseInsensitiveColumnMapping
      {
         get { return (_extensions & AccessorExtension.CaseInsensitiveColumnMapping) == AccessorExtension.CaseInsensitiveColumnMapping; }
      }

      public bool PrivateMemberColumnMapping
      {
         get { return (_extensions & AccessorExtension.PrivateMemberColumnMapping) == AccessorExtension.PrivateMemberColumnMapping; }
      }

      public IDbConnection CreateConnection()
      {
         return _connections();
      }
   }
}
