using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DrivenDb
{
   internal static class AttributeHelper
   {
      public static DbTableAttribute GetTableAttribute(Type type)
      {
         return type.GetCustomAttributes(true)
            .Where(a => a.GetType().FullName == "DrivenDb.DbTableAttribute"
                     || a.GetType().FullName == "System.Data.Linq.Mapping.TableAttribute")
            .OrderBy(a => a.GetType().FullName)
            .Select(a =>
            {
               var dbTable = a as DbTableAttribute;

               if (dbTable != null)
               {
                  return dbTable;
               }

               var nameProperty = a.GetType()
                  .GetProperty("Name");

               var schema = default(string);
               var name = default(string);

               if (nameProperty != null)
               {
                  name = (string) nameProperty
                     .GetGetMethod(true)
                     .Invoke(a, null);

                  var split = name.Split('.');

                  if (split.Count() == 2)
                  {
                     schema = split[0].Replace("[", "").Replace("]", "");
                     name = split[1].Replace("[", "").Replace("]", "");
                  }
               }

               return new DbTableAttribute()
               {
                  Schema = schema,
                  Name = name,
               };
            })
            .First();
      }

      public static DbSequenceAttribute GetSequenceAttribute(Type type)
      {
         return type.GetCustomAttributes(true)
            .OfType<DbSequenceAttribute>()
            .SingleOrDefault();
      }

      public static IDictionary<string, DbColumnAttribute> GetColumnAttributes(Type type)
      {
         var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
         var columns = new Dictionary<string, DbColumnAttribute>();

         foreach (var property in properties)
         {
            if (property.Name == "Record" || property.Name == "Entity")
            {
               continue;
            }

            var found = property.GetCustomAttributes(true)
               .Where(a => a.GetType().FullName == "DrivenDb.DbColumnAttribute"
                        || a.GetType().FullName == "System.Data.Linq.Mapping.ColumnAttribute")
               .OrderBy(a => a.GetType().FullName)
               .FirstOrDefault();

            if (found != null)
            {
               var dbColumn = found as DbColumnAttribute;

               if (dbColumn == null)
               {
                  var foundType = found.GetType();
                  var generatedProperty = foundType.GetProperty("IsDbGenerated");
                  var primaryProperty = foundType.GetProperty("IsPrimaryKey");
                  var nameProperty = foundType.GetProperty("Name");

                  dbColumn = new DbColumnAttribute()
                  {
                     IsDbGenerated = (bool) generatedProperty.GetGetMethod(true).Invoke(found, null),
                     IsPrimaryKey = (bool) primaryProperty.GetGetMethod(true).Invoke(found, null),
                     Name = (string) nameProperty.GetGetMethod(true).Invoke(found, null),
                  };
               }

               dbColumn.Name = dbColumn.Name ?? property.Name;

               columns.Add(property.Name, dbColumn);
            }
         }

         return columns;
      }
   }
}
