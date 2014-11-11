#region Apache Licensed
/*
 Copyright 2013 Daniel Cazzulino

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Dynamic
{
	/// <summary>
	/// Allows by-ref values to be passed to reflection dynamic.
	/// This support does not exist in C# 4.0 dynamic out of the box.
	/// </summary>
	abstract partial class RefValue
	{
		/// <summary>
		/// Creates a value getter/setter delegating reference
		/// to be used by reference when invoking the 
		/// dynamic object.
		/// </summary>
		/// <param name="getter">The getter of the by-ref value to the dynamic invocation.</param>
		/// <param name="setter">The setter of the by-ref value to the dynamic invocation.</param>
		public static RefValue<T> Create<T>(Func<T> getter, Action<T> setter)
		{
			return new RefValue<T>(getter, setter);
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		internal abstract object Value { get; set; }
	}

	/// <summary>
	/// Allows by-ref values to be passed to reflection dynamic.
	/// This support does not exist in C# 4.0 dynamic out of the box.
	/// </summary>
	partial class RefValue<T> : RefValue
	{
		private Func<T> getter;
		private Action<T> setter;

		/// <summary>
		/// Initializes a new instance of the <see cref="RefValue&lt;T&gt;"/> class.
		/// </summary>
		public RefValue(Func<T> getter, Action<T> setter)
		{
			this.getter = getter;
			this.setter = setter;
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		internal override object Value
		{
			get { return this.getter(); }
			set { this.setter((T)value); }
		}
	}
}
