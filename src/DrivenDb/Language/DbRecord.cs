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
using System.Linq;
using System.Runtime.Serialization;
using DrivenDb.Utility;

// ReSharper disable StaticFieldInGenericType
namespace DrivenDb
{
   public delegate void StateChangedEvent(EntityState previous, EntityState current);

   [DataContract]
   public abstract class DbRecord<T> : IDbRecord 
      where T : IDbRecord
   {
      protected static readonly EntityAccessor<T> _accessor = new EntityAccessor<T>(true);      
      private static readonly bool? _isIdentity32;

      protected static readonly DbTableAttribute _table;
      protected static readonly DbSequenceAttribute _sequence;
      protected static readonly KeyValuePair<string, DbColumnAttribute> _identityColumn;
      protected static readonly KeyValuePair<string, DbColumnAttribute>[] _primaryColumns;
      protected static readonly IDictionary<string, DbColumnAttribute> _columns;

      static DbRecord()
      {
         var type = typeof(T);

         _table = AttributeHelper.GetTableAttribute(type);
         _sequence = AttributeHelper.GetSequenceAttribute(type);
         _columns = AttributeHelper.GetColumnAttributes(type);

         _primaryColumns = _columns
            .Where(c => c.Value.IsPrimaryKey)
            .ToArray();

         _identityColumn = _columns
            .FirstOrDefault(c => c.Value.IsDbGenerated);

         if (_identityColumn.Value != null)
         {
            _isIdentity32 = _accessor.GetPropertyInfo(_identityColumn.Key).PropertyType == typeof(int);
         }
      }

      [DataMember]
      protected HashSet<string> _changes = new HashSet<string>();

      [DataMember]
      protected DateTime? _lastModified;

      [DataMember]
      protected DateTime? _lastUpdated;

      [DataMember]
      protected EntityState _preDeletedState;

      [DataMember]
      protected EntityState _state;

      protected T _instance;
      
      protected DbRecord()
      {
         Initialize();
      }

      protected void ChangeState(EntityState	state)
      {
         var previous = _state;

         _state = state;

         if (previous != state && StateChanged != null)
         {
            StateChanged(previous, state);
         }
      }

      public event StateChangedEvent StateChanged;

      [OnDeserialized]
      private void OnDeserialized(StreamingContext context)
      {
         Initialize();
      }

      private void Initialize()
      {
         _instance = (T) (object) this;
      }

      public IDbRecord AsRecord()
      {
         return this;
      }

      object[] IDbRecord.PrimaryKey
      {
         get
         {
            var result = new List<object>();

            foreach (var column in _primaryColumns)
            {
               result.Add(_accessor.GetPropertyValue<object>(_instance, column.Key));
            }

            return result.ToArray();
         }
      }

      DateTime? IDbRecord.LastUpdated
      {
         get { return _lastUpdated; }
      }

      DateTime? IDbRecord.LastModified
      {
         get { return _lastModified; }
      }

      EntityState IDbRecord.State
      {
         get { return _state; }
      }

      string IDbRecord.Schema
      {
         get { return _table.Schema; }
      }

      DbTableAttribute IDbRecord.Table
      {
         get { return _table; }
      }

      DbSequenceAttribute IDbRecord.Sequence
      {
         get { return _sequence; }
      }

      DbTableAttribute IDbRecord.TableOverride
      {
         get;
         set;
      }

      DbColumnAttribute IDbRecord.IdentityColumn
      {
         get { return _identityColumn.Value; }
      }

      DbColumnAttribute[] IDbRecord.PrimaryColumns
      {
         get { return _primaryColumns.Select(c => c.Value).ToArray(); }
      }

      IDictionary<string, DbColumnAttribute> IDbRecord.Columns
      {
         get { return _columns; }
      }

      IEnumerable<string> IDbRecord.Changes
      {
         get { return _changes; }
      }

      void IDbRecord.SetIdentity(long identity)
      {
         if (_identityColumn.Value != null && _isIdentity32.HasValue)
         {
            if (_isIdentity32.Value)
            {
               _accessor.SetPropertyValue(_instance, _identityColumn.Value.Name, (int) identity);
            }
            else
            {
               _accessor.SetPropertyValue(_instance, _identityColumn.Value.Name, identity);
            }
         }
      }

      object IDbRecord.GetProperty(string property)
      {
         return _accessor.GetPropertyValue<object>(_instance, property);
      }

      void IDbRecord.SetProperty(string property, object value)
      {
         _accessor.SetPropertyValue(_instance, property, value);
      }

      void IDbRecord.Reset()
      {
         ChangeState(EntityState.Current);

         _lastModified = null;
         _lastUpdated = DateTime.Now;
         _changes.Clear();
      }
   }
}