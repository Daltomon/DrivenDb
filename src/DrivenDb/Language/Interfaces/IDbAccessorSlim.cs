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

using System;
using System.Collections.Generic;
using System.IO;

namespace DrivenDb
{
   public interface IDbAccessorSlim : IDbMonitor
   {
      int CommandTimeout
      {
         get;
         set;
      }

      T ReadIdentity<T, K>(K key)
         where T : IDbRecord, new();

      IOnJoiner<P, C> ReadRelated<P, C>(P parent)
         where P : IDbRecord, new()
         where C : IDbRecord, new();
       
      IOnJoiner<P, C> ReadRelated<P, C>(IEnumerable<P> parents)
         where P : IDbRecord, new()
         where C : IDbRecord, new();

      IEnumerable<T> ReadValues<T>(string query, params object[] parameters);

      T ReadValue<T>(string query, params object[] parameters);

      IEnumerable<T> ReadAnonymous<T>(T model, string query, params object[] parameters);

      IEnumerable<T> ReadType<T>(string query, params object[] parameters)
         where T : new();

      IEnumerable<T> ReadType<T>(Func<T> factory, string query, params object[] parameters);

      T ReadEntity<T>(string query, params object[] parameters)
         where T : IDbRecord, new();

      IEnumerable<T> ReadEntities<T>(string query, params object[] parameters)
         where T : IDbRecord, new();

      IDbScope CreateScope();

      void WriteEntity(IDbRecord entity);

      void WriteEntities(IEnumerable<IDbRecord> entities);

      void Execute(string query, params object[] parameters);

      TextWriter Log
      {
         get;
         set;
      }

      IFallbackAccessorSlim Fallback
      {
         get;
      }      
   }
}