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
	/// Allows output parameters to be passed to reflection dynamic.
	/// This support does not exist in C# 4.0 dynamic out of the box.
	/// </summary>
	abstract partial class OutValue
	{
		/// <summary>
		/// Creates a value setter delegating reference
		/// to be used as an output parameter when invoking the 
		/// dynamic object.
		/// </summary>
		/// <param name="setter">The value to pass as out to the dynamic invocation.</param>
		public static OutValue<T> Create<T>(Action<T> setter)
		{
			return new OutValue<T>(setter);
		}

		/// <summary>
		/// Sets the value.
		/// </summary>
		internal abstract object Value { set; }
	}

	/// <summary>
	/// Allows output parameters to be passed to reflection dynamic.
	/// This support does not exist in C# 4.0 dynamic out of the box.
	/// </summary>
	partial class OutValue<T> : OutValue
	{
		private Action<T> setter;

		/// <summary>
		/// Initializes a new instance of the <see cref="OutValue&lt;T&gt;"/> class.
		/// </summary>
		public OutValue(Action<T> setter)
		{
			this.setter = setter;
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		internal override object Value
		{
			set { this.setter((T)value); }
		}
	}
}
