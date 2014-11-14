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
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DrivenDb
{
   [DataContract]
   public abstract class DbEntity<T> : DbRecord<T>
      where T : DbEntity<T>, IDbRecord, INotifyPropertyChanged, new()
   {
      protected DbEntity()
      {
         Initialize();
      }

      protected virtual void BeforeSerialization()
      {
         // noop
      }

      protected virtual void AfterDeserialization()
      {
         // noop
      }

      public void Delete()
      {
         if (_state != EntityState.Deleted)
         {
            _preDeletedState = _state;

            ChangeState(EntityState.Deleted);
         }
      }

      public void Undelete()
      {
         if (_state == EntityState.Deleted)
         {
            _state = _preDeletedState;

            ChangeState(_preDeletedState);
         }
      }

      public T ToNew()
      {
         var result = new T();

         result.Update(this._instance);

         if (_identityColumn.Value != null)
         {
            var property = _accessor.GetPropertyInfo(_identityColumn.Key);

            var value = property.PropertyType.IsValueType
               ? Activator.CreateInstance(property.PropertyType)
               : null;

            result.SetProperty(_identityColumn.Key, value);
            result._changes.Remove(_identityColumn.Key);
         }

         result._state = EntityState.New;

         return result;
      }

      public T ToUpdate()
      {
         var result = new T();

         result.Update(this._instance);

         if (_identityColumn.Value != null)
         {
            result._changes.Remove(_identityColumn.Key);
         }

         result._state = EntityState.Modified;

         return result;
      }

      public TOther ToMapped<TOther>()
         where TOther : new()
      {
         var result = new TOther();

         MapTo(result);

         return result;
      }

      public void MapTo<TOther>(TOther other)
      {
         var targets = other.GetType()
            .GetProperties();

         foreach (var target in targets)
         {
            if (target.CanWrite && _accessor.CanReadProperty(target.Name))
            {
               target.SetValue(other, _accessor.GetPropertyValue<object>(this._instance, target.Name), null);
            }
         }
      }

      public void Update(T other)         
      {         
         var properties = _accessor.GetProperties();

         foreach (var property in properties)
         {
            if (_accessor.CanReadProperty(property.Name)
               && _accessor.CanWriteProperty(property.Name)
               )
            {
               AsRecord().SetProperty(property.Name, other.GetProperty(property.Name));
            }
         }

         _lastModified = DateTime.Now;
      }

      public T Clone()
      {
         var result = new T();
         var properties = _accessor.GetProperties();

         foreach (var property in properties)
         {
            if (_accessor.CanReadProperty(property.Name)
               && _accessor.CanWriteProperty(property.Name)
               )
            {
               var value = _accessor.GetPropertyValue<object>(_instance, property.Name);

               _accessor.SetPropertyValue(result, property.Name, value);
            }
         }

         result.Reset();

         foreach (var change in _changes)
         {
            result._changes.Add(change);
         }

         result._lastModified = this._lastModified;
         result._lastUpdated = this._lastUpdated;
         result._state = this._state;

         return result;
      }

      public void Merge(T other) 
      {         
         var lastModified = _lastModified;
         var lastUpdated = _lastUpdated;
         var changes = new HashSet<string>(_changes);

         if (_lastUpdated < other.LastUpdated)
         {
            var properties = _accessor.GetProperties();

            foreach (var property in properties)
            {
               if (!changes.Contains(property.Name))
               {
                  AsRecord().SetProperty(property.Name, other.GetProperty(property.Name));
               }
            }
         }

         foreach (var change in other.Changes)
         {
            if (!changes.Contains(change))
            {
               AsRecord().SetProperty(change, other.GetProperty(change));
               changes.Add(change);
            }
         }

         _changes = new HashSet<string>(changes);

         var state = other.State == EntityState.Deleted || other.State == EntityState.Modified
                      ? other.State
                      : _state;

         ChangeState(state);

         _lastModified = lastModified.HasValue && lastModified > other.LastModified
                             ? lastModified
                             : other.LastModified;

         _lastUpdated = lastUpdated > other.LastUpdated
                            ? lastUpdated
                            : other.LastUpdated;
      }

      [OnSerializing]
      private void OnSerializing(StreamingContext context)
      {
         BeforeSerialization();
      }

      [OnDeserialized]
      private void OnDeserialized(StreamingContext context)
      {
         Initialize();
         AfterDeserialization();
      }

      private void Initialize()
      {
         _instance.PropertyChanged += (s, e) =>
         {
            if (_columns.ContainsKey(e.PropertyName))
            {
               _changes.Add(e.PropertyName);
               _lastModified = DateTime.Now;

               if (_state != EntityState.New)
               {
                  ChangeState(EntityState.Modified);
               }
            }
         };
      }
   }
}