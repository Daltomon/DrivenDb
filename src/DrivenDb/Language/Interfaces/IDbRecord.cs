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

namespace DrivenDb
{
   public interface IDbRecord
   {      
      object[] PrimaryKey
      {
         get;
      }

      EntityState State
      {
         get;
      }

      string Schema
      {
         get;
      }

      DbTableAttribute Table
      {
         get;
      }

      DbSequenceAttribute Sequence
      {
         get;
      }

      DbTableAttribute TableOverride
      {
         get;
         set;
      }

      DbColumnAttribute IdentityColumn
      {
         get;
      }

      DbColumnAttribute[] PrimaryColumns
      {
         get;
      }

      IDictionary<string, DbColumnAttribute> Columns
      {
         get;
      }

      IEnumerable<string> Changes
      {
         get;
      }

      DateTime? LastModified
      {
         get;
      }

      DateTime? LastUpdated
      {
         get;
      }

      void SetIdentity(long identity);

      object GetProperty(string property);

      void SetProperty(string property, object value);

      void Reset();
   }
}