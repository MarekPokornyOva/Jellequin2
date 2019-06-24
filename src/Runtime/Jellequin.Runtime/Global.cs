#region using
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections;
using System.Diagnostics;
#endregion using

namespace Jellequin.Runtime
{
	#region MathObject
	public class MathObject
	{
		/*Math Object Properties
	E 	Returns Euler's number (approx. 2.718)
LN2 	Returns the natural logarithm of 2 (approx. 0.693)
LN10 	Returns the natural logarithm of 10 (approx. 2.302)
LOG2E 	Returns the base-2 logarithm of E (approx. 1.442)
LOG10E 	Returns the base-10 logarithm of E (approx. 0.434)
PI 	Returns PI (approx. 3.14)
SQRT1_2 	Returns the square root of 1/2 (approx. 0.707)
SQRT2 	Returns the square root of 2 (approx. 1.414)

Math Object Methods
Method 	Description
abs(x) 	Returns the absolute value of x
acos(x) 	Returns the arccosine of x, in radians
asin(x) 	Returns the arcsine of x, in radians
atan(x) 	Returns the arctangent of x as a numeric value between -PI/2 and PI/2 radians
atan2(y,x) 	Returns the arctangent of the quotient of its arguments
ceil(x) 	Returns x, rounded upwards to the nearest integer
cos(x) 	Returns the cosine of x (x is in radians)
exp(x) 	Returns the value of Ex
floor(x) 	Returns x, rounded downwards to the nearest integer
log(x) 	Returns the natural logarithm (base E) of x
max(x,y,z,...,n) 	Returns the number with the highest value
min(x,y,z,...,n) 	Returns the number with the lowest value
pow(x,y) 	Returns the value of x to the power of y
random() 	Returns a random number between 0 and 1
round(x) 	Rounds x to the nearest integer
sin(x) 	Returns the sine of x (x is in radians)
sqrt(x) 	Returns the square root of x
tan(x) 	Returns the tangent of an angle
	*/
	}
	#endregion MathObject

	#region DateObject
	public class DateObject:IDoubleConvertible,IStringConvertible
	{
		DateTime _value;
		readonly static CultureInfo _enus=CultureInfo.GetCultureInfo("en-US");
		readonly static DateTime _unixDt=new DateTime(1970,1,1);
		#region ctors
		public DateObject()
		{
			_value=DateTime.Now;
		}

		public DateObject(object value)
		{
			Type t=value.GetType();
			if (t==typeof(double))
				_value=_unixDt.AddMilliseconds(Convert.ToDouble(value));
			else if (t==typeof(DateTime))
				_value=(DateTime)value;
			else
				DateTime.Parse(value.ToString());
		}

		public DateObject(object year,object month,object day,object hours,object minutes,object seconds,object milliseconds)
		{
			_value=new DateTime(Convert.ToInt32(year),Convert.ToInt32(month),Convert.ToInt32(day),Convert.ToInt32(hours),Convert.ToInt32(minutes),Convert.ToInt32(seconds),Convert.ToInt32(milliseconds));
		}

		internal static DateObject CreateInternal(DateTime value)
		{
			return new DateObject() { _value=value };
		}
		#endregion ctors

		#region JS functions
		//Returns the day of the month (from 1-31)
		public object getDate()
		{
			return (double)_value.Day;
		}

		//Returns the day of the week (from 0-6)
		public object getDay()
		{
			return (double)_value.DayOfWeek;
		}

		//Returns the year (four digits)
		public object getFullYear()
		{
			return (double)_value.Year;
		}

		//Returns the hour (from 0-23)
		public object getHours()
		{
			return (double)_value.Hour;
		}

		//Returns the milliseconds (from 0-999)
		public object getMilliseconds()
		{
			return (double)_value.Millisecond;
		}

		//Returns the minutes (from 0-59)
		public object getMinutes()
		{
			return (double)_value.Minute;
		}

		//Returns the month (from 0-11)
		public object getMonth()
		{
			return (double)_value.Month;
		}

		//Returns the seconds (from 0-59)
		public object getSeconds()
		{
			return (double)_value.Second;
		}

		//Returns the number of milliseconds since midnight Jan 1, 1970
		public object getTime()
			=> GetTimeInternal();

		double GetTimeInternal()
			=> (_value - _unixDt).TotalMilliseconds;

		//Returns the time difference between GMT and local time, in minutes
		public object getTimezoneOffset()
		{
			throw new NotImplementedException();
		}

		//Returns the day of the month, according to universal time (from 1-31)
		public object getUTCDate()
		{
			return _value.ToUniversalTime();
		}

		//Returns the day of the week, according to universal time (from 0-6)
		public object getUTCDay()
		{
			return (double)_value.ToUniversalTime().DayOfWeek;
		}

		//Returns the year, according to universal time (four digits)
		public object getUTCFullYear()
		{
			return (double)_value.ToUniversalTime().Year;
		}

		//Returns the hour, according to universal time (from 0-23)
		public object getUTCHours()
		{
			return (double)_value.ToUniversalTime().Hour;
		}

		//Returns the milliseconds, according to universal time (from 0-999)
		public object getUTCMilliseconds()
		{
			return (double)_value.ToUniversalTime().Millisecond;
		}

		//Returns the minutes, according to universal time (from 0-59)
		public object getUTCMinutes()
		{
			return (double)_value.ToUniversalTime().Minute;
		}

		//Returns the month, according to universal time (from 0-11)
		public object getUTCMonth()
		{
			return (double)_value.ToUniversalTime().Month;
		}

		//Returns the seconds, according to universal time (from 0-59)
		public object getUTCSeconds()
		{
			return (double)_value.ToUniversalTime().Second;
		}

		//Deprecated. Use the getFullYear() method instead
		public object getYear()
		{
			return (double)_value.Year;
		}

		//Parses a date string and returns the number of milliseconds since midnight of January 1, 1970
		public object parse()
		{
			throw new NotImplementedException();
		}

		//Sets the day of the month of a date object
		public object setDate(object value)
		{
			return CreateInternal(this._value.AddDays(Convert.ToDouble(value)-this._value.Day));
		}

		//Sets the year (four digits) of a date object
		public object setFullYear(object value)
		{
			return CreateInternal(this._value.AddYears(Convert.ToInt32(value)-this._value.Year));
		}

		//Sets the hour of a date object
		public object setHours(object value)
		{
			return CreateInternal(this._value.AddHours(Convert.ToDouble(value)-this._value.Hour));
		}

		//Sets the milliseconds of a date object
		public object setMilliseconds(object value)
		{
			return CreateInternal(this._value.AddMilliseconds(Convert.ToDouble(value)-this._value.Millisecond));
		}

		//Set the minutes of a date object
		public object setMinutes(object value)
		{
			return CreateInternal(this._value.AddMinutes(Convert.ToDouble(value)-this._value.Minute));
		}

		//Sets the month of a date object
		public object setMonth(object value)
		{
			return CreateInternal(this._value.AddMonths(Convert.ToInt32(value)-this._value.Month));
		}

		//Sets the seconds of a date object
		public object setSeconds(object value)
		{
			return CreateInternal(this._value.AddSeconds(Convert.ToDouble(value)-this._value.Second));
		}

		//Sets a date and time by adding or subtracting a specified number of milliseconds to/from midnight January 1, 1970
		public object setTime()
		{
			throw new NotImplementedException();
		}

		//Sets the day of the month of a date object, according to universal time
		public object setUTCDate(object value)
		{
			DateTime v=_value.ToUniversalTime();
			return CreateInternal(v.AddDays(Convert.ToDouble(value)-v.Day));
		}

		//Sets the year of a date object, according to universal time (four digits)
		public object setUTCFullYear(object value)
		{
			DateTime v=_value.ToUniversalTime();
			return CreateInternal(v.AddYears(Convert.ToInt32(value)-v.Year));
		}

		//Sets the hour of a date object, according to universal time
		public object setUTCHours(object value)
		{
			DateTime v=_value.ToUniversalTime();
			return CreateInternal(v.AddHours(Convert.ToDouble(value)-v.Hour));
		}

		//Sets the milliseconds of a date object, according to universal time
		public object setUTCMilliseconds(object value)
		{
			DateTime v=_value.ToUniversalTime();
			return CreateInternal(v.AddMilliseconds(Convert.ToDouble(value)-v.Millisecond));
		}

		//Set the minutes of a date object, according to universal time
		public object setUTCMinutes(object value)
		{
			DateTime v=_value.ToUniversalTime();
			return CreateInternal(v.AddMinutes(Convert.ToDouble(value)-v.Minute));
		}

		//Sets the month of a date object, according to universal time
		public object setUTCMonth(object value)
		{
			DateTime v=_value.ToUniversalTime();
			return CreateInternal(v.AddMonths(Convert.ToInt32(value)-v.Month));
		}

		//Set the seconds of a date object, according to universal time
		public object setUTCSeconds(object value)
		{
			DateTime v=_value.ToUniversalTime();
			return CreateInternal(v.AddSeconds(Convert.ToDouble(value)-v.Second));
		}

		//Deprecated. Use the setFullYear() method instead
		public object setYear(object value)
		{
			DateTime v=_value.ToUniversalTime();
			return CreateInternal(v.AddYears(Convert.ToInt32(value)-v.Year));
		}

		//Converts the date portion of a Date object into a readable string
		public object toDateString()
		{
			return this._value.ToString("ddd, dd MMM yyyy HH':'mm':'ss",_enus);
		}

		//Deprecated. Use the toUTCString() method instead
		public object toGMTString()
		{
			return this._value.ToString("R",_enus);
		}

		//Returns the date as a string, using the ISO standard
		public object toISOString()
		{
			return this._value.ToString("yyyy-MM-dd HH':'mm':'ss",_enus);
		}

		//Returns the date as a string, formated as a JSON date
		public object toJSON()
		{
			throw new NotImplementedException();
		}

		//Returns the date portion of a Date object as a string, using locale conventions
		public object toLocaleDateString()
		{
			return this._value.Date.ToString("ddd, dd MMM yyyy HH':'mm':'ss");
		}

		//Returns the time portion of a Date object as a string, using locale conventions
		public object toLocaleTimeString()
		{
			return this._value.ToString("HH':'mm':'ss");
		}

		//Converts a Date object to a string, using locale conventions
		public object toLocaleString()
		{
			return this._value.ToString("ddd, dd MMM yyyy HH':'mm':'ss");
		}

		//Converts a Date object to a string
		public object toString()
		{
			return ToStringInternal();
		}

		string ToStringInternal()
			=> this._value.ToString("ddd, dd MMM yyyy HH':'mm':'ss", _enus);

		//Converts the time portion of a Date object to a string
		public object toTimeString()
		{
			return this._value.ToString("HH':'mm':'ss",_enus);
		}

		//Converts a Date object to a string, according to universal time
		public object toUTCString()
		{
			return this._value.ToUniversalTime().ToString("ddd, dd MMM yyyy HH':'mm':'ss",_enus);
		}

		//Returns the number of milliseconds in a date string since midnight of January 1, 1970, according to universal time
		public object UTC()
		{
			return (this._value.ToUniversalTime()-_unixDt).TotalMilliseconds;
		}

		//Returns the primitive value of a Date object
		public object valueOf()
		{
			return _value;
		}

		public override string ToString()
			=> ToStringInternal();

		public double ToDouble()
			=> GetTimeInternal();
		#endregion JS functions
	}
	#endregion DateObject

	#region ArrayObject
#if !DebugRuntime
	[DebuggerStepThrough]
#endif
	public class ArrayObject
	{
		List<object> _keys = new List<object>();
		List<object> _values = new List<object>();

		#region ctors
		public ArrayObject()
		{}

		public ArrayObject(IDictionary<object,object> items)
		{
			foreach (KeyValuePair<object, object> item in items)
			{
				_keys.Add(item.Key);
				_values.Add(item.Value);
			}
		}
		#endregion ctors

		#region JS properties
		public virtual int length
		{
			get { return _keys.Count; }
		}
		#endregion JS properties

		#region JS functions
		//Joins two or more arrays, and returns a copy of the joined arrays
		public ArrayObject concat(/*params */ArrayObject/*[]*/ toConcat)
		{
			ArrayObject result=this.Clone();
			/*foreach (ArrayObject item in toConcat)
			{*/
			ArrayObject item = toConcat;
				int a = 0;
				foreach (object subItem in item._keys)
				{
					result._keys.Add(subItem);
					result._values.Add(item._values[a++]);
				}
			//}
			return result;
		}

		//Search the array for an element and returns it's position
		public int indexOf(object item)
		{
			return this._values.IndexOf(item,0);
		}

		//Search the array for an element and returns it's position
		public int indexOf(object item,int start)
		{
			return this._values.IndexOf(item, start);
		}

		//Joins all elements of an array into a string
		public string join()
		{
			return join(",");
		}

		//Joins all elements of an array into a string
		public string join(string separator)
		{
			StringBuilder sb=new StringBuilder();
			bool isFirst=true;
			foreach (object item in this._values)
			{
				if (isFirst)
					isFirst=false;
				else
					sb.Append(',');
				sb.Append(item.ToString());
			}
			return sb.ToString();
		}

		//Search the array for an element, starting at the end, and returns it's position
		public int lastIndexOf(object item)
		{
			throw new NotImplementedException();
		}

		//Search the array for an element, starting at the end, and returns it's position
		public int lastIndexOf(object item,int start)
		{
			throw new NotImplementedException();
		}

		//Removes the last element of an array, and returns that element
		public object pop()
		{
			int lastIndex=this._values.Count-1;
			object result=this._values[lastIndex];
			this._keys.RemoveAt(lastIndex);
			this._values.RemoveAt(lastIndex);
			return result;
		}

		//Adds new elements to the end of an array, and returns the new length
		public int push(/*params */object/*[]*/ items)
		{
			/*foreach (object item in items)
			{*/
				object item = items;
				this._keys.Add(this._keys.Count);
				this._values.Add(item);
			//}
			return this._values.Count;
		}

		//Reverses the order of the elements in an array
		public ArrayObject reverse()
		{
			ArrayObject result=this.Clone();
			result._keys.Reverse();
			result._values.Reverse();
			return result;
		}

		//Removes the first element of an array, and returns that element
		public object shift()
		{
			if (this._values.Count==0)
				return false;

			object result = this._values[0];
			this._keys.RemoveAt(0);
			this._values.RemoveAt(0);
			return result;
		}

		//Selects a part of an array, and returns the new array
		public ArrayObject slice(int start)
		{
			return slice(start,this._keys.Count);
		}

		//Selects a part of an array, and returns the new array
		public ArrayObject slice(int start,int end)
		{
			ArrayObject result = new ArrayObject();
			for (int a = start; a < end; a++)
			{
				result._keys.Add(this._keys[a]);
				result._values.Add(this._values[a]);
			}
			return result;
		}

		//Sorts the elements of an array
		public ArrayObject sort()
		{
			this._values.Sort();
			return this;
		}

		//Sorts the elements of an array
		public ArrayObject sort(object sortFunction)
		{
			this._values.Sort(new SortComparer(sortFunction));
			return this;
		}

		#region helpers
		class SortComparer:IComparer<object>
		{
			readonly object _sortFunction;

			public SortComparer(object sortFunction)
				=> _sortFunction=sortFunction;

			public int Compare(object x,object y)
				=> (int)RuntimeMethods.CallMethod(_sortFunction,new object[] { x,y });
		}
		#endregion helpers

		//Adds/Removes elements from an array
		public ArrayObject splice(int index,int howmany,/*params */object/*[]*/ items)
		{
			if (howmany > 0)
			{
				this._keys.RemoveRange(index, howmany);
				this._values.RemoveRange(index, howmany);
			}
			/*for (var a = index; a < items.Length; a++)
				this._keys.Insert(a,a);
			this._values.InsertRange(index,items);*/
			this._keys.Insert(index, index);
			this._values.Insert(index, items);
			return this;
		}

		//Converts an array to a string, and returns the result
		public string toString()
		{
			return join();
		}

		//Adds new elements to the beginning of an array, and returns the new length
		public int unshift(/*params */object/*[]*/ items)
		{
			this.splice(0, 0, items);
			return this._keys.Count;
		}

		//Returns the primitive value of an array
		public string valueOf()
		{
			return join();
		}
		#endregion JS functions

		public virtual object GetItem(object index)
		{
			int a = this._keys.IndexOf(index);
			return a == -1 ? null : this._values[a];
		}

		public virtual void SetItem(object index,object item)
		{
			int a = this._keys.IndexOf(index);
			if (a == -1)
			{
				this._keys.Add(index);
				this._values.Add(item);
			}
			else
				this._values[a]=item;
		}

		public virtual bool HasItem(object index)
		{
			return this._keys.IndexOf(index)!=-1;
		}

		public virtual bool DeleteItem(object index)
		{
			int a = this._keys.IndexOf(index);
			if (a == -1)
				return false;
			this._keys.RemoveAt(a);
			this._values.RemoveAt(a);
			return true;
		}

		#region helpers
		ArrayObject Clone()
		{
			ArrayObject result = new ArrayObject();
			int a = 0;
			foreach (object subItem in this._keys)
			{
				result._keys.Add(subItem);
				result._values.Add(this._values[a++]);
			}
			return result;
		}
		#endregion helpers

		public object convertItems(Func<object,object> converter)
		{
			ArrayObject result=new ArrayObject();
			int len=this.length;
			for (var a=0;a<len;a++)
				result.push(converter(this.GetItem(a)));
			return result;
		}

		public object where(Func<object,object> condition)
		{
			ArrayObject result=new ArrayObject();
			int len=this.length;
			for (var a=0;a<len;a++)
			{
				object val=this.GetItem(a);
				if ((bool)condition(val))
					result.push(val);
			}
			return result;
		}
	}
	#endregion ArrayObject

	#region RegExpObject
	public class RegExpObject
	{
		//http://www.w3schools.com/jsref/jsref_obj_regexp.asp

		//prepsat na skutecne typy
		public bool global {get; internal set;}
		public bool ignoreCase { get; internal set; }
		public int lastIndex {get; set;}
		public bool multiline {get; internal set;}
		public string source { get; internal set; }
		Regex _regex;

		public RegExpObject(object pattern, object switches):this((string)pattern,(string)switches)
		{}

		public RegExpObject(string pattern,string switches)
		{
			switches=switches??"";

			this.source=pattern;
			this.ignoreCase=switches.Contains("i");
			this.multiline=switches.Contains("m");
			this.global=switches.Contains("g");

			RegexOptions options=RegexOptions.ECMAScript|RegexOptions.CultureInvariant;
			if (ignoreCase)
				options|=RegexOptions.IgnoreCase;
			if (multiline)
				options|=RegexOptions.Multiline;

			this._regex=new Regex(pattern,options);
		}

		//Compiles a regular expression
		public object compile(string source,string flags)
		{
			return new RegExpObject(source,flags);
		}

		//Tests for a match in a string. Returns the first match
		public string exec(string input)
		{
			Match match=null;
			string inputStr=(string)input;
			if ((bool)this.global)
			{
				int num=lastIndex;
				if (num<=0)
					match=this._regex.Match(inputStr);
				else
					if (num<=inputStr.Length)
						match=_regex.Match(inputStr,num);
			}
			else
				match=_regex.Match((string)input);

			if (match==null||!match.Success)
			{
				this.lastIndex=0;
				return null;
			}
			lastIndex=match.Index+1;
			return match.ToString();
		}

		//Tests for a match in a string. Returns true or false
		public bool test(string input)
		{
			return exec(input)!=null;
		}

		internal Regex RawRegex { get { return _regex; } }
	}
	#endregion RegExpObject

	#region StringObject
	public class StringObject:IStringConvertible
	{
		//http://www.w3schools.com/jsref/jsref_obj_string.asp
		string _value;
		#region ctors
		public StringObject(string value)
		{
			_value=value;
		}
		#endregion ctors

		#region JS properties
		public int length
		{
			get { return _value.Length; }
		}
		#endregion JS properties

		#region JS functions
		//Returns the character at the specified index
		public object charAt(object index)
		{
			return _value[Convert.ToInt32(index)].ToString();
		}

		//Returns the Unicode of the character at the specified index
		public object charCodeAt(object index)
		{
			return (byte)_value[Convert.ToInt32(index)];
		}

		//Joins two or more strings, and returns a copy of the joined strings
		public object concat(object string1,object string2)
		{
			return ((string)string1)+(string)string2;
		}

		//Converts Unicode values to characters
		public object fromCharCode(object n1)
		{
			return ((char)Convert.ToInt32(n1)).ToString();
		}

		//Returns the position of the first found occurrence of a specified value in a string
		public object indexOf(object searchvalue)
		{
			return this.indexOf(searchvalue,0);
		}

		public object indexOf(object searchvalue,object start)
		{
			return (double)_value.IndexOf((string)searchvalue,Convert.ToInt32(start));
		}

		//Returns the position of the last found occurrence of a specified value in a string
		public object lastIndexOf(object searchvalue,object start)
		{
			return (double)_value.LastIndexOf((string)searchvalue,Convert.ToInt32(start));
		}

		//Searches for a match between a regular expression and a string, and returns the matches
		public object match(object regexp)
		{
			ArrayObject result=new ArrayObject();
			foreach (Match item in ((RegExpObject)regexp).RawRegex.Matches(_value))
				result.push(item.Value);
			return result;
		}

		public object repeat(object count)
		{
			if (_value==null)
				return null;

			StringBuilder sb = new StringBuilder();
			int c = Convert.ToInt32(count);
			for (int a = 0; a<c; a++)
				sb.Append(_value);
			return sb.ToString();
		}

		//Searches for a match between a substring (or regular expression) and a string, and replaces the matched substring with a new substring
		public object replace(object searchvalue,object newvalue)
		{
			return searchvalue.GetType()==typeof(RegExpObject)
				?((RegExpObject)searchvalue).RawRegex.Replace(_value,newvalue.ToString())
				:_value.Replace(searchvalue.ToString(),newvalue.ToString());
		}

		//Searches for a match between a regular expression and a string, and returns the position of the match
		public object search(object searchvalue)
		{
			if (searchvalue.GetType()==typeof(RegExpObject))
			{
				Match m=((RegExpObject)searchvalue).RawRegex.Match(_value);
				return (double)(m.Success?m.Index:-1);
			}
			else
				return (double)_value.IndexOf(searchvalue.ToString());
		}

		//Extracts a part of a string and returns a new string
		public object slice(object start,object end)
		{
			int s=Convert.ToInt32(start);
			return _value.Substring(s,Convert.ToInt32(end)-s+1);
		}

		//Splits a string into an array of substrings
		public object split(object separator,object limit)
		{
			ArrayObject result=new ArrayObject();
			if ((separator as string)=="")
				foreach (char item in _value)
					result.push(item);
			else if (separator.GetType()==typeof(RegExpObject))
				foreach (string item in ((RegExpObject)separator).RawRegex.Split(_value))
					result.push(item);
			else
				foreach (string item in _value.Split(new string[]{separator.ToString()},StringSplitOptions.None))
					result.push(item);
				
			return result;
		}

		//Extracts the characters from a string, beginning at a specified start position, and through the specified number of character
		public object substr(object start,object length)
		{
			int s=Convert.ToInt32(start);
			int l=Convert.ToInt32(length);
			if (l>_value.Length-s)
				l=_value.Length-s;
			return _value.Substring(s,l);
		}

		//Extracts the characters from a string, between two specified indices
		public object substring(object from,object to)
		{
			int s=Convert.ToInt32(from);
			int e=Convert.ToInt32(to)-s;
			if (e>=_value.Length)
				e=_value.Length-s;
			return _value.Substring(s,e);
		}

		//Converts a string to lowercase letters
		public object toLowerCase()
		{
			return _value.ToLower();
		}

		//Converts a string to uppercase letters
		public object toUpperCase()
		{
			return _value.ToUpper();
		}

		//Returns the primitive value of a String object
		public object valueOf()
		{
			return _value;
		}
		#endregion JS functions

		public string ToStringValue()
			=> ToString();
	}
	#endregion StringObject

	#region ArrayObjectLateBinding
#if !DebugRuntime
	[DebuggerStepThrough]
#endif
	internal class ArrayObjectLateBinding:ArrayObject
	{
		IList _list;
		internal ArrayObjectLateBinding(IList list)
		{
			_list=list;
		}

		public override int length
		{
			get { return _list.Count; }
		}

		public override object GetItem(object index)
		{
			return this._list[Convert.ToInt32(index)];
		}

		public override void SetItem(object index,object item)
		{
			this._list[Convert.ToInt32(index)]=item;
		}
	}
	#endregion ArrayObjectLateBinding

	#region convertibles
	interface IDoubleConvertible
	{
		double ToDouble();
	}

	interface IStringConvertible
	{
		string ToString();
	}
	#endregion convertibles
}
