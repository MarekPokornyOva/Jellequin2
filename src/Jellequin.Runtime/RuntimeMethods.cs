#region using
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
#endregion using

namespace Jellequin.Runtime
{
/*#if !DebugRuntime
	[DebuggerStepThrough]
#endif*/
	public static class RuntimeMethods
	{
		#region fields/props/ctors
		internal readonly static Type _objectType=typeof(object);

		static RuntimeMethods()
		{
			RuntimeMethodsExtender = new NullRuntimeMethodsExtender();
		}

		public static IRuntimeMethodsExtender RuntimeMethodsExtender { get; set; }
		#endregion fields/props/ctors

		#region math op,BitwiseAnd,BitwiseOr,BitwiseNot,ShiftRight,ShiftLeft,Equals,EqualsStrictly,Compare,Debug,EvalToBool,Not
		public static object Add(object x, object y)
		{
			if ((x is string) || (y is string) || (x is IStringConvertible) || (y is IStringConvertible))
				return x.ToString() + y.ToString();

			Type tx = x.GetType();
			Type ty = y.GetType();
			#region by type
			if (tx == typeof(int))
			{
				int x2 = (int)x;
				if (ty == typeof(int))
					return x2 + ((int)y);
				if (ty == typeof(uint))
					return x2 + ((uint)y);
				if (ty == typeof(long))
					return x2 + ((long)y);
				/*if (ty==typeof(ulong))
					return x2+((ulong)y);*/
				if (ty == typeof(short))
					return x2 + ((short)y);
				if (ty == typeof(double))
					return x2 + ((double)y);
				if (ty == typeof(byte))
					return x2 + ((byte)y);
				if (ty == typeof(decimal))
					return x2 + ((decimal)y);
				if (ty == typeof(Single))
					return x2 + ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 + ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(uint))
			{
				uint x2 = (uint)x;
				if (ty == typeof(int))
					return x2 + ((int)y);
				if (ty == typeof(uint))
					return x2 + ((uint)y);
				if (ty == typeof(long))
					return x2 + ((long)y);
				if (ty == typeof(ulong))
					return x2 + ((ulong)y);
				if (ty == typeof(short))
					return x2 + ((short)y);
				if (ty == typeof(double))
					return x2 + ((double)y);
				if (ty == typeof(byte))
					return x2 + ((byte)y);
				if (ty == typeof(decimal))
					return x2 + ((decimal)y);
				if (ty == typeof(Single))
					return x2 + ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 + ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(long))
			{
				long x2 = (long)x;
				if (ty == typeof(int))
					return x2 + ((int)y);
				if (ty == typeof(uint))
					return x2 + ((uint)y);
				if (ty == typeof(long))
					return x2 + ((long)y);
				/*if (ty==typeof(ulong))
					return x2+((ulong)y);*/
				if (ty == typeof(short))
					return x2 + ((short)y);
				if (ty == typeof(double))
					return x2 + ((double)y);
				if (ty == typeof(byte))
					return x2 + ((byte)y);
				if (ty == typeof(decimal))
					return x2 + ((decimal)y);
				if (ty == typeof(Single))
					return x2 + ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 + ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(ulong))
			{
				ulong x2 = (ulong)x;
				/*if (ty==typeof(int))
					return x2+((int)y);*/
				if (ty == typeof(uint))
					return x2 + ((uint)y);
				/*if (ty==typeof(long))
					return x2+((long)y);*/
				if (ty == typeof(ulong))
					return x2 + ((ulong)y);
				/*if (ty==typeof(short))
					return x2+((short)y);*/
				if (ty == typeof(double))
					return x2 + ((double)y);
				if (ty == typeof(byte))
					return x2 + ((byte)y);
				if (ty == typeof(decimal))
					return x2 + ((decimal)y);
				if (ty == typeof(Single))
					return x2 + ((Single)y);
				/*if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 + ((IDoubleConvertible)y).ToDouble();*/
			}
			if (tx == typeof(short))
			{
				short x2 = (short)x;
				if (ty == typeof(int))
					return x2 + ((int)y);
				if (ty == typeof(uint))
					return x2 + ((uint)y);
				if (ty == typeof(long))
					return x2 + ((long)y);
				/*if (ty==typeof(ulong))
					return x2+((ulong)y);*/
				if (ty == typeof(short))
					return x2 + ((short)y);
				if (ty == typeof(double))
					return x2 + ((double)y);
				if (ty == typeof(byte))
					return x2 + ((byte)y);
				if (ty == typeof(decimal))
					return x2 + ((decimal)y);
				if (ty == typeof(Single))
					return x2 + ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 + ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(double))
			{
				double x2 = (double)x;
				if (ty == typeof(int))
					return x2 + ((int)y);
				if (ty == typeof(uint))
					return x2 + ((uint)y);
				if (ty == typeof(long))
					return x2 + ((long)y);
				if (ty == typeof(ulong))
					return x2 + ((ulong)y);
				if (ty == typeof(short))
					return x2 + ((short)y);
				if (ty == typeof(double))
					return x2 + ((double)y);
				if (ty == typeof(byte))
					return x2 + ((byte)y);
				/*if (ty==typeof(decimal))
					return x2+((decimal)y);*/
				if (ty == typeof(Single))
					return x2 + ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 + ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(byte))
			{
				byte x2 = (byte)x;
				if (ty == typeof(int))
					return x2 + ((int)y);
				if (ty == typeof(uint))
					return x2 + ((uint)y);
				if (ty == typeof(long))
					return x2 + ((long)y);
				if (ty == typeof(ulong))
					return x2 + ((ulong)y);
				if (ty == typeof(short))
					return x2 + ((short)y);
				if (ty == typeof(double))
					return x2 + ((double)y);
				if (ty == typeof(byte))
					return x2 + ((byte)y);
				if (ty == typeof(decimal))
					return x2 + ((decimal)y);
				if (ty == typeof(Single))
					return x2 + ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 + ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(decimal))
			{
				decimal x2 = (decimal)x;
				if (ty == typeof(int))
					return x2 + ((int)y);
				if (ty == typeof(uint))
					return x2 + ((uint)y);
				if (ty == typeof(long))
					return x2 + ((long)y);
				if (ty == typeof(ulong))
					return x2 + ((ulong)y);
				if (ty == typeof(short))
					return x2 + ((short)y);
				/*if (ty==typeof(double))
					return x2+((double)y);*/
				if (ty == typeof(byte))
					return x2 + ((byte)y);
				if (ty == typeof(decimal))
					return x2 + ((decimal)y);
				/*if (ty==typeof(Single))
					return x2+((Single)y);*/
				/*if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 + ((IDoubleConvertible)y).ToDouble();*/
			}
			if (tx == typeof(Single))
			{
				Single x2 = (Single)x;
				if (ty == typeof(int))
					return x2 + ((int)y);
				if (ty == typeof(uint))
					return x2 + ((uint)y);
				if (ty == typeof(long))
					return x2 + ((long)y);
				if (ty == typeof(ulong))
					return x2 + ((ulong)y);
				if (ty == typeof(short))
					return x2 + ((short)y);
				if (ty == typeof(double))
					return x2 + ((double)y);
				if (ty == typeof(byte))
					return x2 + ((byte)y);
				/*if (ty==typeof(decimal))
					return x2+((decimal)y);*/
				if (ty == typeof(Single))
					return x2 + ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 + ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(IDoubleConvertible))
			{
				if (ty == typeof(int))
					return ((IDoubleConvertible)x).ToDouble() + ((int)y);
				if (ty == typeof(uint))
					return ((IDoubleConvertible)x).ToDouble() + ((uint)y);
				if (ty == typeof(long))
					return ((IDoubleConvertible)x).ToDouble() + ((long)y);
				/*if (ty == typeof(ulong))
					return ((IDoubleConvertible)x).ToDouble() + ((ulong)y);*/
				if (ty == typeof(short))
					return ((IDoubleConvertible)x).ToDouble() + ((short)y);
				if (ty == typeof(double))
					return ((IDoubleConvertible)x).ToDouble() + ((double)y);
				if (ty == typeof(byte))
					return ((IDoubleConvertible)x).ToDouble() + ((byte)y);
				/*if (ty==typeof(decimal))
					return ((IDoubleConvertible)x).ToDouble() + ((decimal)y);*/
				if (ty == typeof(Single))
					return ((IDoubleConvertible)x).ToDouble() + ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return ((IDoubleConvertible)x).ToDouble() + ((IDoubleConvertible)y).ToDouble();
			}
			#endregion by type

			if (tx == typeof(ExternalEventInfoWithEvent))
			{
				//example: watcher.Created+=fileCreateCallback;
				ExternalEventInfoWithEvent eei = (ExternalEventInfoWithEvent)x;
				DynamDelCalls.CreateEventHandlerBridge(eei.Type.GetEvent(eei.EventName), eei.Target, (IInvokable)y);
				return x;
			}

			throw new InvalidOperationException();
		}

		public static object Sub(object x, object y)
		{
			Type tx = x.GetType();
			Type ty = y.GetType();
			#region by type
			if (tx == typeof(int))
			{
				int x2 = (int)x;
				if (ty == typeof(int))
					return x2 - ((int)y);
				if (ty == typeof(uint))
					return x2 - ((uint)y);
				if (ty == typeof(long))
					return x2 - ((long)y);
				/*if (ty==typeof(ulong))
					return x2-((ulong)y);*/
				if (ty == typeof(short))
					return x2 - ((short)y);
				if (ty == typeof(double))
					return x2 - ((double)y);
				if (ty == typeof(byte))
					return x2 - ((byte)y);
				if (ty == typeof(decimal))
					return x2 - ((decimal)y);
				if (ty == typeof(Single))
					return x2 - ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 - ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(uint))
			{
				uint x2 = (uint)x;
				if (ty == typeof(int))
					return x2 - ((int)y);
				if (ty == typeof(uint))
					return x2 - ((uint)y);
				if (ty == typeof(long))
					return x2 - ((long)y);
				if (ty == typeof(ulong))
					return x2 - ((ulong)y);
				if (ty == typeof(short))
					return x2 - ((short)y);
				if (ty == typeof(double))
					return x2 - ((double)y);
				if (ty == typeof(byte))
					return x2 - ((byte)y);
				if (ty == typeof(decimal))
					return x2 - ((decimal)y);
				if (ty == typeof(Single))
					return x2 - ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 - ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(long))
			{
				long x2 = (long)x;
				if (ty == typeof(int))
					return x2 - ((int)y);
				if (ty == typeof(uint))
					return x2 - ((uint)y);
				if (ty == typeof(long))
					return x2 - ((long)y);
				/*if (ty==typeof(ulong))
					return x2-((ulong)y);*/
				if (ty == typeof(short))
					return x2 - ((short)y);
				if (ty == typeof(double))
					return x2 - ((double)y);
				if (ty == typeof(byte))
					return x2 - ((byte)y);
				if (ty == typeof(decimal))
					return x2 - ((decimal)y);
				if (ty == typeof(Single))
					return x2 - ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 - ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(ulong))
			{
				ulong x2 = (ulong)x;
				/*if (ty==typeof(int))
					return x2-((int)y);*/
				if (ty == typeof(uint))
					return x2 - ((uint)y);
				/*if (ty==typeof(long))
					return x2-((long)y);*/
				if (ty == typeof(ulong))
					return x2 - ((ulong)y);
				/*if (ty==typeof(short))
					return x2-((short)y);*/
				if (ty == typeof(double))
					return x2 - ((double)y);
				if (ty == typeof(byte))
					return x2 - ((byte)y);
				if (ty == typeof(decimal))
					return x2 - ((decimal)y);
				if (ty == typeof(Single))
					return x2 - ((Single)y);
				/*if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 - ((IDoubleConvertible)y).ToDouble();*/
			}
			if (tx == typeof(short))
			{
				short x2 = (short)x;
				if (ty == typeof(int))
					return x2 - ((int)y);
				if (ty == typeof(uint))
					return x2 - ((uint)y);
				if (ty == typeof(long))
					return x2 - ((long)y);
				/*if (ty==typeof(ulong))
					return x2-((ulong)y);*/
				if (ty == typeof(short))
					return x2 - ((short)y);
				if (ty == typeof(double))
					return x2 - ((double)y);
				if (ty == typeof(byte))
					return x2 - ((byte)y);
				if (ty == typeof(decimal))
					return x2 - ((decimal)y);
				if (ty == typeof(Single))
					return x2 - ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 - ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(double))
			{
				double x2 = (double)x;
				if (ty == typeof(int))
					return x2 - ((int)y);
				if (ty == typeof(uint))
					return x2 - ((uint)y);
				if (ty == typeof(long))
					return x2 - ((long)y);
				if (ty == typeof(ulong))
					return x2 - ((ulong)y);
				if (ty == typeof(short))
					return x2 - ((short)y);
				if (ty == typeof(double))
					return x2 - ((double)y);
				if (ty == typeof(byte))
					return x2 - ((byte)y);
				/*if (ty==typeof(decimal))
					return x2-((decimal)y);*/
				if (ty == typeof(Single))
					return x2 - ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 - ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(byte))
			{
				byte x2 = (byte)x;
				if (ty == typeof(int))
					return x2 - ((int)y);
				if (ty == typeof(uint))
					return x2 - ((uint)y);
				if (ty == typeof(long))
					return x2 - ((long)y);
				if (ty == typeof(ulong))
					return x2 - ((ulong)y);
				if (ty == typeof(short))
					return x2 - ((short)y);
				if (ty == typeof(double))
					return x2 - ((double)y);
				if (ty == typeof(byte))
					return x2 - ((byte)y);
				if (ty == typeof(decimal))
					return x2 - ((decimal)y);
				if (ty == typeof(Single))
					return x2 - ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 - ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(decimal))
			{
				decimal x2 = (decimal)x;
				if (ty == typeof(int))
					return x2 - ((int)y);
				if (ty == typeof(uint))
					return x2 - ((uint)y);
				if (ty == typeof(long))
					return x2 - ((long)y);
				if (ty == typeof(ulong))
					return x2 - ((ulong)y);
				if (ty == typeof(short))
					return x2 - ((short)y);
				/*if (ty==typeof(double))
					return x2-((double)y);*/
				if (ty == typeof(byte))
					return x2 - ((byte)y);
				if (ty == typeof(decimal))
					return x2 - ((decimal)y);
				/*if (ty==typeof(Single))
					return x2-((Single)y);*/
				/*if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 - ((IDoubleConvertible)y).ToDouble();*/
			}
			if (tx == typeof(Single))
			{
				Single x2 = (Single)x;
				if (ty == typeof(int))
					return x2 - ((int)y);
				if (ty == typeof(uint))
					return x2 - ((uint)y);
				if (ty == typeof(long))
					return x2 - ((long)y);
				if (ty == typeof(ulong))
					return x2 - ((ulong)y);
				if (ty == typeof(short))
					return x2 - ((short)y);
				if (ty == typeof(double))
					return x2 - ((double)y);
				if (ty == typeof(byte))
					return x2 - ((byte)y);
				/*if (ty==typeof(decimal))
					return x2-((decimal)y);*/
				if (ty == typeof(Single))
					return x2 - ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 - ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(IDoubleConvertible))
			{
				if (ty == typeof(int))
					return ((IDoubleConvertible)x).ToDouble() - ((int)y);
				if (ty == typeof(uint))
					return ((IDoubleConvertible)x).ToDouble() - ((uint)y);
				if (ty == typeof(long))
					return ((IDoubleConvertible)x).ToDouble() - ((long)y);
				/*if (ty == typeof(ulong))
					return ((IDoubleConvertible)x).ToDouble() - ((ulong)y);*/
				if (ty == typeof(short))
					return ((IDoubleConvertible)x).ToDouble() - ((short)y);
				if (ty == typeof(double))
					return ((IDoubleConvertible)x).ToDouble() - ((double)y);
				if (ty == typeof(byte))
					return ((IDoubleConvertible)x).ToDouble() - ((byte)y);
				/*if (ty==typeof(decimal))
					return ((IDoubleConvertible)x).ToDouble() - ((decimal)y);*/
				if (ty == typeof(Single))
					return ((IDoubleConvertible)x).ToDouble() - ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return ((IDoubleConvertible)x).ToDouble() - ((IDoubleConvertible)y).ToDouble();
			}
			#endregion by type
			throw new InvalidOperationException();
		}

		public static object Mul(object x, object y)
		{
			Type tx = x.GetType();
			Type ty = y.GetType();
			#region by type
			if (tx == typeof(int))
			{
				int x2 = (int)x;
				if (ty == typeof(int))
					return x2 * ((int)y);
				if (ty == typeof(uint))
					return x2 * ((uint)y);
				if (ty == typeof(long))
					return x2 * ((long)y);
				/*if (ty==typeof(ulong))
					return x2*((ulong)y);*/
				if (ty == typeof(short))
					return x2 * ((short)y);
				if (ty == typeof(double))
					return x2 * ((double)y);
				if (ty == typeof(byte))
					return x2 * ((byte)y);
				if (ty == typeof(decimal))
					return x2 * ((decimal)y);
				if (ty == typeof(Single))
					return x2 * ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 * ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(uint))
			{
				uint x2 = (uint)x;
				if (ty == typeof(int))
					return x2 * ((int)y);
				if (ty == typeof(uint))
					return x2 * ((uint)y);
				if (ty == typeof(long))
					return x2 * ((long)y);
				if (ty == typeof(ulong))
					return x2 * ((ulong)y);
				if (ty == typeof(short))
					return x2 * ((short)y);
				if (ty == typeof(double))
					return x2 * ((double)y);
				if (ty == typeof(byte))
					return x2 * ((byte)y);
				if (ty == typeof(decimal))
					return x2 * ((decimal)y);
				if (ty == typeof(Single))
					return x2 * ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 * ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(long))
			{
				long x2 = (long)x;
				if (ty == typeof(int))
					return x2 * ((int)y);
				if (ty == typeof(uint))
					return x2 * ((uint)y);
				if (ty == typeof(long))
					return x2 * ((long)y);
				/*if (ty==typeof(ulong))
					return x2*((ulong)y);*/
				if (ty == typeof(short))
					return x2 * ((short)y);
				if (ty == typeof(double))
					return x2 * ((double)y);
				if (ty == typeof(byte))
					return x2 * ((byte)y);
				if (ty == typeof(decimal))
					return x2 * ((decimal)y);
				if (ty == typeof(Single))
					return x2 * ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 * ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(ulong))
			{
				ulong x2 = (ulong)x;
				/*if (ty==typeof(int))
					return x2*((int)y);*/
				if (ty == typeof(uint))
					return x2 * ((uint)y);
				/*if (ty==typeof(long))
					return x2*((long)y);*/
				if (ty == typeof(ulong))
					return x2 * ((ulong)y);
				/*if (ty==typeof(short))
					return x2*((short)y);*/
				if (ty == typeof(double))
					return x2 * ((double)y);
				if (ty == typeof(byte))
					return x2 * ((byte)y);
				if (ty == typeof(decimal))
					return x2 * ((decimal)y);
				if (ty == typeof(Single))
					return x2 * ((Single)y);
				/*if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 * ((IDoubleConvertible)y).ToDouble();*/
			}
			if (tx == typeof(short))
			{
				short x2 = (short)x;
				if (ty == typeof(int))
					return x2 * ((int)y);
				if (ty == typeof(uint))
					return x2 * ((uint)y);
				if (ty == typeof(long))
					return x2 * ((long)y);
				/*if (ty==typeof(ulong))
					return x2*((ulong)y);*/
				if (ty == typeof(short))
					return x2 * ((short)y);
				if (ty == typeof(double))
					return x2 * ((double)y);
				if (ty == typeof(byte))
					return x2 * ((byte)y);
				if (ty == typeof(decimal))
					return x2 * ((decimal)y);
				if (ty == typeof(Single))
					return x2 * ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 * ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(double))
			{
				double x2 = (double)x;
				if (ty == typeof(int))
					return x2 * ((int)y);
				if (ty == typeof(uint))
					return x2 * ((uint)y);
				if (ty == typeof(long))
					return x2 * ((long)y);
				if (ty == typeof(ulong))
					return x2 * ((ulong)y);
				if (ty == typeof(short))
					return x2 * ((short)y);
				if (ty == typeof(double))
					return x2 * ((double)y);
				if (ty == typeof(byte))
					return x2 * ((byte)y);
				/*if (ty==typeof(decimal))
					return x2*((decimal)y);*/
				if (ty == typeof(Single))
					return x2 * ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 * ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(byte))
			{
				byte x2 = (byte)x;
				if (ty == typeof(int))
					return x2 * ((int)y);
				if (ty == typeof(uint))
					return x2 * ((uint)y);
				if (ty == typeof(long))
					return x2 * ((long)y);
				if (ty == typeof(ulong))
					return x2 * ((ulong)y);
				if (ty == typeof(short))
					return x2 * ((short)y);
				if (ty == typeof(double))
					return x2 * ((double)y);
				if (ty == typeof(byte))
					return x2 * ((byte)y);
				if (ty == typeof(decimal))
					return x2 * ((decimal)y);
				if (ty == typeof(Single))
					return x2 * ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 * ((IDoubleConvertible)y).ToDouble();
			}
			if (tx == typeof(decimal))
			{
				decimal x2 = (decimal)x;
				if (ty == typeof(int))
					return x2 * ((int)y);
				if (ty == typeof(uint))
					return x2 * ((uint)y);
				if (ty == typeof(long))
					return x2 * ((long)y);
				if (ty == typeof(ulong))
					return x2 * ((ulong)y);
				if (ty == typeof(short))
					return x2 * ((short)y);
				/*if (ty==typeof(double))
					return x2*((double)y);*/
				if (ty == typeof(byte))
					return x2 * ((byte)y);
				if (ty == typeof(decimal))
					return x2 * ((decimal)y);
				/*if (ty==typeof(Single))
					return x2*((Single)y);*/
				/*if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 * ((IDoubleConvertible)y).ToDouble();*/
			}
			if (tx == typeof(Single))
			{
				Single x2 = (Single)x;
				if (ty == typeof(int))
					return x2 * ((int)y);
				if (ty == typeof(uint))
					return x2 * ((uint)y);
				if (ty == typeof(long))
					return x2 * ((long)y);
				if (ty == typeof(ulong))
					return x2 * ((ulong)y);
				if (ty == typeof(short))
					return x2 * ((short)y);
				if (ty == typeof(double))
					return x2 * ((double)y);
				if (ty == typeof(byte))
					return x2 * ((byte)y);
				/*if (ty==typeof(decimal))
					return x2*((decimal)y);*/
				if (ty == typeof(Single))
					return x2 * ((Single)y);
				if (typeof(IDoubleConvertible).IsAssignableFrom(ty))
					return x2 * ((IDoubleConvertible)y).ToDouble();
			}
			#endregion by type
			throw new InvalidOperationException();
		}

		public static object Div(object x, object y)
		{
			Type tx = x.GetType();
			Type ty = y.GetType();
			#region by type
			if (tx == typeof(int))
			{
				int x2 = (int)x;
				if (ty == typeof(int))
					return x2 / ((int)y);
				if (ty == typeof(uint))
					return x2 / ((uint)y);
				if (ty == typeof(long))
					return x2 / ((long)y);
				/*if (ty==typeof(ulong))
					return x2/((ulong)y);*/
				if (ty == typeof(short))
					return x2 / ((short)y);
				if (ty == typeof(double))
					return x2 / ((double)y);
				if (ty == typeof(byte))
					return x2 / ((byte)y);
				if (ty == typeof(decimal))
					return x2 / ((decimal)y);
				if (ty == typeof(Single))
					return x2 / ((Single)y);
			}
			if (tx == typeof(uint))
			{
				uint x2 = (uint)x;
				if (ty == typeof(int))
					return x2 / ((int)y);
				if (ty == typeof(uint))
					return x2 / ((uint)y);
				if (ty == typeof(long))
					return x2 / ((long)y);
				if (ty == typeof(ulong))
					return x2 / ((ulong)y);
				if (ty == typeof(short))
					return x2 / ((short)y);
				if (ty == typeof(double))
					return x2 / ((double)y);
				if (ty == typeof(byte))
					return x2 / ((byte)y);
				if (ty == typeof(decimal))
					return x2 / ((decimal)y);
				if (ty == typeof(Single))
					return x2 / ((Single)y);
			}
			if (tx == typeof(long))
			{
				long x2 = (long)x;
				if (ty == typeof(int))
					return x2 / ((int)y);
				if (ty == typeof(uint))
					return x2 / ((uint)y);
				if (ty == typeof(long))
					return x2 / ((long)y);
				/*if (ty==typeof(ulong))
					return x2/((ulong)y);*/
				if (ty == typeof(short))
					return x2 / ((short)y);
				if (ty == typeof(double))
					return x2 / ((double)y);
				if (ty == typeof(byte))
					return x2 / ((byte)y);
				if (ty == typeof(decimal))
					return x2 / ((decimal)y);
				if (ty == typeof(Single))
					return x2 / ((Single)y);
			}
			if (tx == typeof(ulong))
			{
				ulong x2 = (ulong)x;
				/*if (ty==typeof(int))
					return x2/((int)y);*/
				if (ty == typeof(uint))
					return x2 / ((uint)y);
				/*if (ty==typeof(long))
					return x2/((long)y);*/
				if (ty == typeof(ulong))
					return x2 / ((ulong)y);
				/*if (ty==typeof(short))
					return x2/((short)y);*/
				if (ty == typeof(double))
					return x2 / ((double)y);
				if (ty == typeof(byte))
					return x2 / ((byte)y);
				if (ty == typeof(decimal))
					return x2 / ((decimal)y);
				if (ty == typeof(Single))
					return x2 / ((Single)y);
			}
			if (tx == typeof(short))
			{
				short x2 = (short)x;
				if (ty == typeof(int))
					return x2 / ((int)y);
				if (ty == typeof(uint))
					return x2 / ((uint)y);
				if (ty == typeof(long))
					return x2 / ((long)y);
				/*if (ty==typeof(ulong))
					return x2/((ulong)y);*/
				if (ty == typeof(short))
					return x2 / ((short)y);
				if (ty == typeof(double))
					return x2 / ((double)y);
				if (ty == typeof(byte))
					return x2 / ((byte)y);
				if (ty == typeof(decimal))
					return x2 / ((decimal)y);
				if (ty == typeof(Single))
					return x2 / ((Single)y);
			}
			if (tx == typeof(double))
			{
				double x2 = (double)x;
				if (ty == typeof(int))
					return x2 / ((int)y);
				if (ty == typeof(uint))
					return x2 / ((uint)y);
				if (ty == typeof(long))
					return x2 / ((long)y);
				if (ty == typeof(ulong))
					return x2 / ((ulong)y);
				if (ty == typeof(short))
					return x2 / ((short)y);
				if (ty == typeof(double))
					return x2 / ((double)y);
				if (ty == typeof(byte))
					return x2 / ((byte)y);
				/*if (ty==typeof(decimal))
					return x2/((decimal)y);*/
				if (ty == typeof(Single))
					return x2 / ((Single)y);
			}
			if (tx == typeof(byte))
			{
				byte x2 = (byte)x;
				if (ty == typeof(int))
					return x2 / ((int)y);
				if (ty == typeof(uint))
					return x2 / ((uint)y);
				if (ty == typeof(long))
					return x2 / ((long)y);
				if (ty == typeof(ulong))
					return x2 / ((ulong)y);
				if (ty == typeof(short))
					return x2 / ((short)y);
				if (ty == typeof(double))
					return x2 / ((double)y);
				if (ty == typeof(byte))
					return x2 / ((byte)y);
				if (ty == typeof(decimal))
					return x2 / ((decimal)y);
				if (ty == typeof(Single))
					return x2 / ((Single)y);
			}
			if (tx == typeof(decimal))
			{
				decimal x2 = (decimal)x;
				if (ty == typeof(int))
					return x2 / ((int)y);
				if (ty == typeof(uint))
					return x2 / ((uint)y);
				if (ty == typeof(long))
					return x2 / ((long)y);
				if (ty == typeof(ulong))
					return x2 / ((ulong)y);
				if (ty == typeof(short))
					return x2 / ((short)y);
				/*if (ty==typeof(double))
					return x2/((double)y);*/
				if (ty == typeof(byte))
					return x2 / ((byte)y);
				if (ty == typeof(decimal))
					return x2 / ((decimal)y);
				/*if (ty==typeof(Single))
					return x2/((Single)y);*/
			}
			if (tx == typeof(Single))
			{
				Single x2 = (Single)x;
				if (ty == typeof(int))
					return x2 / ((int)y);
				if (ty == typeof(uint))
					return x2 / ((uint)y);
				if (ty == typeof(long))
					return x2 / ((long)y);
				if (ty == typeof(ulong))
					return x2 / ((ulong)y);
				if (ty == typeof(short))
					return x2 / ((short)y);
				if (ty == typeof(double))
					return x2 / ((double)y);
				if (ty == typeof(byte))
					return x2 / ((byte)y);
				/*if (ty==typeof(decimal))
					return x2/((decimal)y);*/
				if (ty == typeof(Single))
					return x2 / ((Single)y);
			}
			#endregion by type
			throw new InvalidOperationException();
		}

		public static object Modulo(object x, object y)
		{
			Type tx = x.GetType();
			Type ty = y.GetType();
			#region by type
			if (tx == typeof(int))
			{
				int x2 = (int)x;
				if (ty == typeof(int))
					return x2 % ((int)y);
				if (ty == typeof(uint))
					return x2 % ((uint)y);
				if (ty == typeof(long))
					return x2 % ((long)y);
				/*if (ty==typeof(ulong))
					return x2%((ulong)y);*/
				if (ty == typeof(short))
					return x2 % ((short)y);
				if (ty == typeof(double))
					return x2 % ((double)y);
				if (ty == typeof(byte))
					return x2 % ((byte)y);
				if (ty == typeof(decimal))
					return x2 % ((decimal)y);
				if (ty == typeof(Single))
					return x2 % ((Single)y);
			}
			if (tx == typeof(uint))
			{
				uint x2 = (uint)x;
				if (ty == typeof(int))
					return x2 % ((int)y);
				if (ty == typeof(uint))
					return x2 % ((uint)y);
				if (ty == typeof(long))
					return x2 % ((long)y);
				if (ty == typeof(ulong))
					return x2 % ((ulong)y);
				if (ty == typeof(short))
					return x2 % ((short)y);
				if (ty == typeof(double))
					return x2 % ((double)y);
				if (ty == typeof(byte))
					return x2 % ((byte)y);
				if (ty == typeof(decimal))
					return x2 % ((decimal)y);
				if (ty == typeof(Single))
					return x2 % ((Single)y);
			}
			if (tx == typeof(long))
			{
				long x2 = (long)x;
				if (ty == typeof(int))
					return x2 % ((int)y);
				if (ty == typeof(uint))
					return x2 % ((uint)y);
				if (ty == typeof(long))
					return x2 % ((long)y);
				/*if (ty==typeof(ulong))
					return x2%((ulong)y);*/
				if (ty == typeof(short))
					return x2 % ((short)y);
				if (ty == typeof(double))
					return x2 % ((double)y);
				if (ty == typeof(byte))
					return x2 % ((byte)y);
				if (ty == typeof(decimal))
					return x2 % ((decimal)y);
				if (ty == typeof(Single))
					return x2 % ((Single)y);
			}
			if (tx == typeof(ulong))
			{
				ulong x2 = (ulong)x;
				/*if (ty==typeof(int))
					return x2%((int)y);*/
				if (ty == typeof(uint))
					return x2 % ((uint)y);
				/*if (ty==typeof(long))
					return x2%((long)y);*/
				if (ty == typeof(ulong))
					return x2 % ((ulong)y);
				/*if (ty==typeof(short))
					return x2%((short)y);*/
				if (ty == typeof(double))
					return x2 % ((double)y);
				if (ty == typeof(byte))
					return x2 % ((byte)y);
				if (ty == typeof(decimal))
					return x2 % ((decimal)y);
				if (ty == typeof(Single))
					return x2 % ((Single)y);
			}
			if (tx == typeof(short))
			{
				short x2 = (short)x;
				if (ty == typeof(int))
					return x2 % ((int)y);
				if (ty == typeof(uint))
					return x2 % ((uint)y);
				if (ty == typeof(long))
					return x2 % ((long)y);
				/*if (ty==typeof(ulong))
					return x2%((ulong)y);*/
				if (ty == typeof(short))
					return x2 % ((short)y);
				if (ty == typeof(double))
					return x2 % ((double)y);
				if (ty == typeof(byte))
					return x2 % ((byte)y);
				if (ty == typeof(decimal))
					return x2 % ((decimal)y);
				if (ty == typeof(Single))
					return x2 % ((Single)y);
			}
			if (tx == typeof(double))
			{
				double x2 = (double)x;
				if (ty == typeof(int))
					return x2 % ((int)y);
				if (ty == typeof(uint))
					return x2 % ((uint)y);
				if (ty == typeof(long))
					return x2 % ((long)y);
				if (ty == typeof(ulong))
					return x2 % ((ulong)y);
				if (ty == typeof(short))
					return x2 % ((short)y);
				if (ty == typeof(double))
					return x2 % ((double)y);
				if (ty == typeof(byte))
					return x2 % ((byte)y);
				/*if (ty==typeof(decimal))
					return x2%((decimal)y);*/
				if (ty == typeof(Single))
					return x2 % ((Single)y);
			}
			if (tx == typeof(byte))
			{
				byte x2 = (byte)x;
				if (ty == typeof(int))
					return x2 % ((int)y);
				if (ty == typeof(uint))
					return x2 % ((uint)y);
				if (ty == typeof(long))
					return x2 % ((long)y);
				if (ty == typeof(ulong))
					return x2 % ((ulong)y);
				if (ty == typeof(short))
					return x2 % ((short)y);
				if (ty == typeof(double))
					return x2 % ((double)y);
				if (ty == typeof(byte))
					return x2 % ((byte)y);
				if (ty == typeof(decimal))
					return x2 % ((decimal)y);
				if (ty == typeof(Single))
					return x2 % ((Single)y);
			}
			if (tx == typeof(decimal))
			{
				decimal x2 = (decimal)x;
				if (ty == typeof(int))
					return x2 % ((int)y);
				if (ty == typeof(uint))
					return x2 % ((uint)y);
				if (ty == typeof(long))
					return x2 % ((long)y);
				if (ty == typeof(ulong))
					return x2 % ((ulong)y);
				if (ty == typeof(short))
					return x2 % ((short)y);
				/*if (ty==typeof(double))
					return x2%((double)y);*/
				if (ty == typeof(byte))
					return x2 % ((byte)y);
				if (ty == typeof(decimal))
					return x2 % ((decimal)y);
				/*if (ty==typeof(Single))
					return x2%((Single)y);*/
			}
			if (tx == typeof(Single))
			{
				Single x2 = (Single)x;
				if (ty == typeof(int))
					return x2 % ((int)y);
				if (ty == typeof(uint))
					return x2 % ((uint)y);
				if (ty == typeof(long))
					return x2 % ((long)y);
				if (ty == typeof(ulong))
					return x2 % ((ulong)y);
				if (ty == typeof(short))
					return x2 % ((short)y);
				if (ty == typeof(double))
					return x2 % ((double)y);
				if (ty == typeof(byte))
					return x2 % ((byte)y);
				/*if (ty==typeof(decimal))
					return x2%((decimal)y);*/
				if (ty == typeof(Single))
					return x2 % ((Single)y);
			}
			#endregion by type
			throw new InvalidOperationException();
		}

		public static object BitwiseAnd(object x, object y)
		{
			Type tx = x.GetType();
			Type ty = y.GetType();
			#region by type
			if (tx == typeof(int))
			{
				int x2 = (int)x;
				if (ty == typeof(int))
					return x2 & ((int)y);
				if (ty == typeof(uint))
					return x2 & ((uint)y);
				if (ty == typeof(long))
					return x2 & ((long)y);
				if (ty == typeof(short))
					return x2 & ((short)y);
				if (ty == typeof(byte))
					return x2 & ((byte)y);
			}
			if (tx == typeof(uint))
			{
				uint x2 = (uint)x;
				if (ty == typeof(int))
					return x2 & ((int)y);
				if (ty == typeof(uint))
					return x2 & ((uint)y);
				if (ty == typeof(long))
					return x2 & ((long)y);
				if (ty == typeof(ulong))
					return x2 & ((ulong)y);
				if (ty == typeof(short))
					return x2 & ((short)y);
				if (ty == typeof(byte))
					return x2 & ((byte)y);
			}
			if (tx == typeof(long))
			{
				long x2 = (long)x;
				if (ty == typeof(int))
					return x2 & ((int)y);
				if (ty == typeof(uint))
					return x2 & ((uint)y);
				if (ty == typeof(long))
					return x2 & ((long)y);
				if (ty == typeof(short))
					return x2 & ((short)y);
				if (ty == typeof(byte))
					return x2 & ((byte)y);
			}
			if (tx == typeof(ulong))
			{
				ulong x2 = (ulong)x;
				if (ty == typeof(uint))
					return x2 & ((uint)y);
				if (ty == typeof(ulong))
					return x2 & ((ulong)y);
				if (ty == typeof(byte))
					return x2 & ((byte)y);
			}
			if (tx == typeof(short))
			{
				short x2 = (short)x;
				if (ty == typeof(int))
					return x2 & ((int)y);
				if (ty == typeof(uint))
					return x2 & ((uint)y);
				if (ty == typeof(long))
					return x2 & ((long)y);
				if (ty == typeof(short))
					return x2 & ((short)y);
				if (ty == typeof(byte))
					return x2 & ((byte)y);
			}
			if (tx == typeof(byte))
			{
				byte x2 = (byte)x;
				if (ty == typeof(int))
					return x2 & ((int)y);
				if (ty == typeof(uint))
					return x2 & ((uint)y);
				if (ty == typeof(long))
					return x2 & ((long)y);
				if (ty == typeof(ulong))
					return x2 & ((ulong)y);
				if (ty == typeof(short))
					return x2 & ((short)y);
				if (ty == typeof(byte))
					return x2 & ((byte)y);
			}
			if ((tx == typeof(bool)) && (ty == typeof(bool)))
				return ((bool)x) & ((bool)y);
			#endregion by type
			throw new InvalidOperationException();
		}

		public static object BitwiseOr(object x, object y)
		{
			Type tx = x.GetType();
			Type ty = y.GetType();
			#region by type
#pragma warning disable 0675
			if (tx == typeof(int))
			{
				int x2 = (int)x;
				if (ty == typeof(int))
					return x2 | ((int)y);
				if (ty == typeof(uint))
					return x2 | ((uint)y);
				if (ty == typeof(long))
					return x2 | ((long)y);
				if (ty == typeof(short))
					return x2 | ((short)y);
				if (ty == typeof(byte))
					return x2 | ((byte)y);
			}
			if (tx == typeof(uint))
			{
				uint x2 = (uint)x;
				if (ty == typeof(int))
					return x2 | ((int)y);
				if (ty == typeof(uint))
					return x2 | ((uint)y);
				if (ty == typeof(long))
					return x2 | ((long)y);
				if (ty == typeof(ulong))
					return x2 | ((ulong)y);
				if (ty == typeof(short))
					return x2 | ((short)y);
				if (ty == typeof(byte))
					return x2 | ((byte)y);
			}
			if (tx == typeof(long))
			{
				long x2 = (long)x;
				if (ty == typeof(int))
					return x2 | ((int)y);
				if (ty == typeof(uint))
					return x2 | ((uint)y);
				if (ty == typeof(long))
					return x2 | ((long)y);
				if (ty == typeof(short))
					return x2 | ((short)y);
				if (ty == typeof(byte))
					return x2 | ((byte)y);
			}
			if (tx == typeof(ulong))
			{
				ulong x2 = (ulong)x;
				if (ty == typeof(uint))
					return x2 | ((uint)y);
				if (ty == typeof(ulong))
					return x2 | ((ulong)y);
				if (ty == typeof(byte))
					return x2 | ((byte)y);
			}
			if (tx == typeof(short))
			{
				short x2 = (short)x;
				if (ty == typeof(int))
					return x2 | ((int)y);
				if (ty == typeof(uint))
					return x2 | ((uint)y);
				if (ty == typeof(long))
					return x2 | ((long)y);
				if (ty == typeof(short))
					return x2 | ((short)y);
				if (ty == typeof(byte))
					return x2 | ((byte)y);
			}
			if (tx == typeof(byte))
			{
				byte x2 = (byte)x;
				if (ty == typeof(int))
					return x2 | ((int)y);
				if (ty == typeof(uint))
					return x2 | ((uint)y);
				if (ty == typeof(long))
					return x2 | ((long)y);
				if (ty == typeof(ulong))
					return x2 | ((ulong)y);
				if (ty == typeof(short))
					return x2 | ((short)y);
				if (ty == typeof(byte))
					return x2 | ((byte)y);
			}
			if ((tx == typeof(bool)) || (ty == typeof(bool)))
				return ((bool)x) | ((bool)y);
#pragma warning restore 0675
			#endregion by type
			throw new InvalidOperationException();
		}

		public static object BitwiseNot(object x)
		{
			Type tx = x.GetType();
			#region by type
			if (tx == typeof(bool))
				return !(bool)x;
			if (tx == typeof(int))
				return ~(int)x;
			if (tx == typeof(uint))
				return ~(uint)x;
			if (tx == typeof(long))
				return ~(long)x;
			if (tx == typeof(ulong))
				return ~(ulong)x;
			if (tx == typeof(short))
				return ~(short)x;
			if (tx == typeof(byte))
				return ~(byte)x;
			#endregion by type
			throw new InvalidOperationException();
		}

		public static object ShiftLeft(object x, object y)
		{
			Type tx = x.GetType();
			Type ty = y.GetType();
			#region by type
			if (tx == typeof(int))
			{
				int x2 = (int)x;
				if (ty == typeof(int))
					return x2 << ((int)y);
				if (ty == typeof(short))
					return x2 << ((short)y);
				if (ty == typeof(byte))
					return x2 << ((byte)y);
			}
			if (tx == typeof(uint))
			{
				uint x2 = (uint)x;
				if (ty == typeof(int))
					return x2 << ((int)y);
				if (ty == typeof(short))
					return x2 << ((short)y);
				if (ty == typeof(byte))
					return x2 << ((byte)y);
			}
			if (tx == typeof(long))
			{
				long x2 = (long)x;
				if (ty == typeof(int))
					return x2 << ((int)y);
				if (ty == typeof(short))
					return x2 << ((short)y);
				if (ty == typeof(byte))
					return x2 << ((byte)y);
			}
			if (tx == typeof(ulong))
			{
				ulong x2 = (ulong)x;
				if (ty == typeof(byte))
					return x2 << ((byte)y);
			}
			if (tx == typeof(short))
			{
				short x2 = (short)x;
				if (ty == typeof(int))
					return x2 << ((int)y);
				if (ty == typeof(short))
					return x2 << ((short)y);
				if (ty == typeof(byte))
					return x2 << ((byte)y);
			}
			if (tx == typeof(byte))
			{
				byte x2 = (byte)x;
				if (ty == typeof(int))
					return x2 << ((int)y);
				if (ty == typeof(short))
					return x2 << ((short)y);
				if (ty == typeof(byte))
					return x2 << ((byte)y);
			}
			#endregion by type
			throw new InvalidOperationException();
		}

		public static object ShiftRight(object x, object y)
		{
			Type tx = x.GetType();
			Type ty = y.GetType();
			#region by type
			if (tx == typeof(int))
			{
				int x2 = (int)x;
				if (ty == typeof(int))
					return x2 >> ((int)y);
				if (ty == typeof(short))
					return x2 >> ((short)y);
				if (ty == typeof(byte))
					return x2 >> ((byte)y);
			}
			if (tx == typeof(uint))
			{
				uint x2 = (uint)x;
				if (ty == typeof(int))
					return x2 >> ((int)y);
				if (ty == typeof(short))
					return x2 >> ((short)y);
				if (ty == typeof(byte))
					return x2 >> ((byte)y);
			}
			if (tx == typeof(long))
			{
				long x2 = (long)x;
				if (ty == typeof(int))
					return x2 >> ((int)y);
				if (ty == typeof(short))
					return x2 >> ((short)y);
				if (ty == typeof(byte))
					return x2 >> ((byte)y);
			}
			if (tx == typeof(ulong))
			{
				ulong x2 = (ulong)x;
				if (ty == typeof(byte))
					return x2 >> ((byte)y);
			}
			if (tx == typeof(short))
			{
				short x2 = (short)x;
				if (ty == typeof(int))
					return x2 >> ((int)y);
				if (ty == typeof(short))
					return x2 >> ((short)y);
				if (ty == typeof(byte))
					return x2 >> ((byte)y);
			}
			if (tx == typeof(byte))
			{
				byte x2 = (byte)x;
				if (ty == typeof(int))
					return x2 >> ((int)y);
				if (ty == typeof(short))
					return x2 >> ((short)y);
				if (ty == typeof(byte))
					return x2 >> ((byte)y);
			}
			#endregion by type
			throw new InvalidOperationException();
		}

		public static new bool Equals(object x, object y)
		{
			bool xNull = x == null;
			bool yNull = y == null;
			if (xNull && yNull)
				return true;
			if (xNull || yNull)
				return false;

			if ((x is string) || (y is string))
				return x.ToString() == y.ToString();

			if (x.Equals(y))
				return true;

			Type tx = x.GetType();
			Type ty = y.GetType();
			#region by type
			if (tx == typeof(int))
			{
				int x2 = (int)x;
				if (ty == typeof(int))
					return x2 == ((int)y);
				if (ty == typeof(uint))
					return x2 == ((uint)y);
				if (ty == typeof(long))
					return x2 == ((long)y);
				/*if (ty==typeof(ulong))
					return x2+((ulong)y);*/
				if (ty == typeof(short))
					return x2 == ((short)y);
				if (ty == typeof(double))
					return x2 == ((double)y);
				if (ty == typeof(byte))
					return x2 == ((byte)y);
				if (ty == typeof(decimal))
					return x2 == ((decimal)y);
				if (ty == typeof(Single))
					return x2 == ((Single)y);
			}
			if (tx == typeof(uint))
			{
				uint x2 = (uint)x;
				if (ty == typeof(int))
					return x2 == ((int)y);
				if (ty == typeof(uint))
					return x2 == ((uint)y);
				if (ty == typeof(long))
					return x2 == ((long)y);
				if (ty == typeof(ulong))
					return x2 == ((ulong)y);
				if (ty == typeof(short))
					return x2 == ((short)y);
				if (ty == typeof(double))
					return x2 == ((double)y);
				if (ty == typeof(byte))
					return x2 == ((byte)y);
				if (ty == typeof(decimal))
					return x2 == ((decimal)y);
				if (ty == typeof(Single))
					return x2 == ((Single)y);
			}
			if (tx == typeof(long))
			{
				long x2 = (long)x;
				if (ty == typeof(int))
					return x2 == ((int)y);
				if (ty == typeof(uint))
					return x2 == ((uint)y);
				if (ty == typeof(long))
					return x2 == ((long)y);
				/*if (ty==typeof(ulong))
					return x2+((ulong)y);*/
				if (ty == typeof(short))
					return x2 == ((short)y);
				if (ty == typeof(double))
					return x2 == ((double)y);
				if (ty == typeof(byte))
					return x2 == ((byte)y);
				if (ty == typeof(decimal))
					return x2 == ((decimal)y);
				if (ty == typeof(Single))
					return x2 == ((Single)y);
			}
			if (tx == typeof(ulong))
			{
				ulong x2 = (ulong)x;
				/*if (ty==typeof(int))
					return x2+((int)y);*/
				if (ty == typeof(uint))
					return x2 == ((uint)y);
				/*if (ty==typeof(long))
					return x2+((long)y);*/
				if (ty == typeof(ulong))
					return x2 == ((ulong)y);
				/*if (ty==typeof(short))
					return x2+((short)y);*/
				if (ty == typeof(double))
					return x2 == ((double)y);
				if (ty == typeof(byte))
					return x2 == ((byte)y);
				if (ty == typeof(decimal))
					return x2 == ((decimal)y);
				if (ty == typeof(Single))
					return x2 == ((Single)y);
			}
			if (tx == typeof(short))
			{
				short x2 = (short)x;
				if (ty == typeof(int))
					return x2 == ((int)y);
				if (ty == typeof(uint))
					return x2 == ((uint)y);
				if (ty == typeof(long))
					return x2 == ((long)y);
				/*if (ty==typeof(ulong))
					return x2+((ulong)y);*/
				if (ty == typeof(short))
					return x2 == ((short)y);
				if (ty == typeof(double))
					return x2 == ((double)y);
				if (ty == typeof(byte))
					return x2 == ((byte)y);
				if (ty == typeof(decimal))
					return x2 == ((decimal)y);
				if (ty == typeof(Single))
					return x2 == ((Single)y);
			}
			if (tx == typeof(double))
			{
				double x2 = (double)x;
				if (ty == typeof(int))
					return x2 == ((int)y);
				if (ty == typeof(uint))
					return x2 == ((uint)y);
				if (ty == typeof(long))
					return x2 == ((long)y);
				if (ty == typeof(ulong))
					return x2 == ((ulong)y);
				if (ty == typeof(short))
					return x2 == ((short)y);
				if (ty == typeof(double))
					return x2 == ((double)y);
				if (ty == typeof(byte))
					return x2 == ((byte)y);
				/*if (ty==typeof(decimal))
					return x2+((decimal)y);*/
				if (ty == typeof(Single))
					return x2 == ((Single)y);
			}
			if (tx == typeof(byte))
			{
				byte x2 = (byte)x;
				if (ty == typeof(int))
					return x2 == ((int)y);
				if (ty == typeof(uint))
					return x2 == ((uint)y);
				if (ty == typeof(long))
					return x2 == ((long)y);
				if (ty == typeof(ulong))
					return x2 == ((ulong)y);
				if (ty == typeof(short))
					return x2 == ((short)y);
				if (ty == typeof(double))
					return x2 == ((double)y);
				if (ty == typeof(byte))
					return x2 == ((byte)y);
				if (ty == typeof(decimal))
					return x2 == ((decimal)y);
				if (ty == typeof(Single))
					return x2 == ((Single)y);
			}
			if (tx == typeof(decimal))
			{
				decimal x2 = (decimal)x;
				if (ty == typeof(int))
					return x2 == ((int)y);
				if (ty == typeof(uint))
					return x2 == ((uint)y);
				if (ty == typeof(long))
					return x2 == ((long)y);
				if (ty == typeof(ulong))
					return x2 == ((ulong)y);
				if (ty == typeof(short))
					return x2 == ((short)y);
				/*if (ty==typeof(double))
					return x2+((double)y);*/
				if (ty == typeof(byte))
					return x2 == ((byte)y);
				if (ty == typeof(decimal))
					return x2 == ((decimal)y);
				/*if (ty==typeof(Single))
					return x2+((Single)y);*/
			}
			if (tx == typeof(Single))
			{
				Single x2 = (Single)x;
				if (ty == typeof(int))
					return x2 == ((int)y);
				if (ty == typeof(uint))
					return x2 == ((uint)y);
				if (ty == typeof(long))
					return x2 == ((long)y);
				if (ty == typeof(ulong))
					return x2 == ((ulong)y);
				if (ty == typeof(short))
					return x2 == ((short)y);
				if (ty == typeof(double))
					return x2 == ((double)y);
				if (ty == typeof(byte))
					return x2 == ((byte)y);
				/*if (ty==typeof(decimal))
					return x2+((decimal)y);*/
				if (ty == typeof(Single))
					return x2 == ((Single)y);
			}
			#endregion by type
			return object.Equals(x, y);
		}

		public static bool EqualsStrictly(object x, object y)
		{
			return Marshal.ReferenceEquals(x, y);
		}

		static readonly Type _doubleType = typeof(double);
		public static int Compare(object x, object y)
		{
			Type tx = x.GetType();
			if ((tx == y.GetType()) && (x is IComparable))
				return ((IComparable)x).CompareTo(y);
			if ((TypeConverter.ConvertTryConvert(ref x, _doubleType)) && (TypeConverter.ConvertTryConvert(ref y, _doubleType)))
				return ((double)x).CompareTo((double)y);
			throw new RuntimeException(RuntimeExceptionReason.Compare);
		}

/*#if !DebugRuntime
		[DebuggerHidden]
#endif*/
[DebuggerHidden]
		public static void Debug()
		{
			if (Debugger.Launch())
				Debugger.Break();
		}

		public static object EvalToBool(object value)
		{
			return (value != null) && (!false.Equals(value)) && (!0.Equals(value)) && (!((double)0).Equals(value)) && (!"".Equals(value));
		}

		public static object Not(object value)
		{
			if (false.Equals(value))
				return true;
			if (true.Equals(value))
				return false;
			if (0.Equals(value))
				return 1;
			if (value is int)
				return 0;
			throw new RuntimeException(RuntimeExceptionReason.Compare);
		}
		#endregion math op,BitwiseAnd,BitwiseOr,BitwiseNot,ShiftRight,ShiftLeft,Equals,EqualsStrictly,Compare,Debug,EvalToBool,Not

		#region GetMember
		static readonly object[] _emptyObjectArray = new object[0];
		internal static readonly Type _typeType = typeof(Type);
		static int GetMemberInternal(object objectVar, string memberName, out object resultObject, out MemberInfo mi, out object objectVarWrap, out Type objectVarType)
		{
			if (objectVar is ExternalLibraryObject elo)
			{
				if (objectVar is NamespaceObject nsO)
					memberName=nsO._name + "." + memberName;
				Type t = elo._assembly.GetType(memberName);
				resultObject= t == null ? (object)new NamespaceObject() { _assembly = elo._assembly, _name = memberName } : (object)new StaticObject() { Type = t };
				mi = null;
				objectVarWrap = null;
				objectVarType = null;
				return 1;
			}

			bool isAddEventListener;
			if ((isAddEventListener = (memberName == "addEventListener")) || (memberName == "removeEventListener"))
			{
				resultObject = isAddEventListener;
				mi = null;

				if (objectVar is StaticObject)
				{
					objectVarWrap = null;
					objectVarType = ((StaticObject)objectVar).Type;
				}
				else
				{
					objectVarWrap = GetJsWrapper(objectVar);
					objectVarType = objectVarWrap.GetType();
				}
				return 3;
			}

			MemberInfo[] mis;
			if (objectVar is StaticObject)
			{
				objectVarWrap = null;
				objectVarType = ((StaticObject)objectVar).Type;
				mis = objectVarType.GetMember(memberName, BindingFlags.Public | BindingFlags.Static);
			}
			else
			{
				objectVarWrap=objectVar;
				objectVarType=objectVarWrap.GetType();
				mis=objectVarType.GetMember(memberName,BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static);
				if (mis.Length==0)
				{
					objectVarWrap=GetJsWrapper(objectVar);
					objectVarType=objectVarWrap.GetType();
					mis=objectVarType.GetMember(memberName,BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static);
				}
			}

			resultObject=null;
			if (mis.Length==0)
			{
				mi=null;
				return 0;
			}
			mi = mis[0];
			return 2;
		}

		public static object GetMember(object objectVar, string memberName)
		{
			if (objectVar==null)
				throw new NullReferenceException();

			if (objectVar is IJsObject jsObject)
				//return jsObject.GetValue(memberName);
				if (jsObject.HasMember(memberName))
					return jsObject.GetValue(memberName);

			switch (GetMemberInternal(objectVar, memberName, out object resultObject, out MemberInfo mmi, out object objectVarWrap, out Type objectVarType))
			{
				case 0:
					throw new RuntimeException(RuntimeExceptionReason.NoMemberOnExternalVariable, (objectVar is StaticObject ? objectVarType : objectVar.GetType()).Name, memberName);
				case 1:
					return resultObject;
				case 2:
					MethodInfo mi = mmi as MethodInfo;
					if (mi == null)
					{
						if (objectVarType.IsEnum)
							return Enum.Parse(objectVarType, mmi.Name);
						if (mmi is EventInfo ei)
							return new ExternalEventInfoWithEvent { Target = objectVarWrap, Type = objectVarType, EventName = memberName };

						Func<object> result = CreateGetMethodWithTarget(mmi, objectVarWrap);
						return result();
					}
					else
						return new ExternalMethodInfo { Target = objectVarWrap, Type = objectVarType, MethodName = memberName };
				case 3:
					return new ExternalEventInfoWithOperation { Target = objectVarWrap, Type = objectVarType, IsAdd = (bool)resultObject };
				default:
					throw new InvalidProgramException($"Internal error on {nameof(GetMember)}");
			}
		}

		#region CreateGetMethod/WithTarget
		//http://jachman.wordpress.com/2006/08/22/2000-faster-using-dynamic-method-calls/
		//static Type[] _createGetMethodArgs = new Type[] { _objectType };
		static Dictionary<MemberInfo, DynamicMethod> _createGetMethodCache = new Dictionary<MemberInfo, DynamicMethod>();
		private static DynamicMethod CreateGetMethod(MemberInfo member)
		{
			if (_createGetMethodCache.TryGetValue(member, out DynamicMethod result))
				return result;

			PropertyInfo pi = member as PropertyInfo;
			FieldInfo fi = pi == null ? member as FieldInfo : null;
			MemberInfo mi = (MemberInfo)member;
			bool instanceHolder = !(pi == null ? fi.IsStatic : pi.GetGetMethod().IsStatic);
			bool isProp = fi == null;
			Type memberType = isProp ? pi.PropertyType : fi.FieldType;
			Type declaringType = mi.DeclaringType;
			result = new DynamicMethod(declaringType.AssemblyQualifiedName + "_Get" + mi.Name, _objectType, /*_createGetMethodArgs*/ new Type[] { _objectType }, declaringType, true);
			_createGetMethodCache.Add(member, result);

			ILGenerator gen = result.GetILGenerator();
			if (instanceHolder)
			{
				gen.Emit(OpCodes.Ldarg_0);
				if (declaringType.IsClass)
					gen.Emit(OpCodes.Castclass, declaringType);
				else if (declaringType.IsValueType)
					gen.Emit(OpCodes.Unbox, declaringType);
			}
			if (isProp)
			{
				MethodInfo mei = pi.GetGetMethod();
				gen.Emit(instanceHolder&&mei.IsVirtual ? OpCodes.Callvirt : OpCodes.Call,mei);
			}
			else
				if ((fi.IsLiteral)&&(!instanceHolder))
				{
					object val=fi.GetRawConstantValue();

					if (memberType==typeof(int))
						gen.Emit(OpCodes.Ldc_I4,(int)val);
					else if (memberType==typeof(double))
						gen.Emit(OpCodes.Ldc_R8,(double)val);
					else if (memberType==typeof(bool))
						gen.Emit((bool)val ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
					else
						throw new RuntimeException(RuntimeExceptionReason.Unsupported);
				}
				else
					gen.Emit(instanceHolder ? OpCodes.Ldfld : OpCodes.Ldsfld,fi);
			if (memberType.IsValueType)
				gen.Emit(OpCodes.Box, memberType);
			gen.Emit(OpCodes.Ret);
			return result;
		}

		private static Func<object> CreateGetMethodWithTarget(MemberInfo member, object target)
		{
			return (Func<object>)CreateGetMethod(member).CreateDelegate(typeof(Func<object>), target);
		}
		#endregion CreateGetMethod/WithTarget
		#endregion GetMember

		#region SetMember
		public static void SetMember(object objectVar, string memberName, object value)
		{
			if (objectVar==null)
				throw new NullReferenceException();

			IJsObject jsObject = objectVar as DynamicJsObject;
			if (jsObject!=null)
			{
				jsObject.SetValue(memberName, value);
				return;
			}

			if (value is ExternalEventInfoWithEvent)
				return; //all work has been done on Add

			object objectVarWrap = GetJsWrapper(objectVar);
			//object objectVarWrap = objectVar;

			Type objectVarType;
			MemberInfo[] mis;
			if (objectVar is StaticObject)
			{
				objectVarWrap=null;
				objectVarType=((StaticObject)objectVar).Type;
				mis=objectVarType.GetMember(memberName, BindingFlags.Public|BindingFlags.Static);
			}
			else
			{
				objectVarWrap=GetJsWrapper(objectVar);
				objectVarType=objectVarWrap.GetType();
				mis=objectVarType.GetMember(memberName, BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static);
			}

			if (mis.Length==0)
				throw new RuntimeException(RuntimeExceptionReason.NoMemberOnExternalVariable, objectVar.GetType().Name, memberName);
			MemberInfo mmi = mis[0];
			if (mmi is MethodInfo)
				throw new RuntimeException(RuntimeExceptionReason.NoMemberOnExternalVariable, objectVar.GetType().Name, memberName);

			Action<object> result = CreateSetMethodWithTarget(mmi, objectVarWrap);
			result(value);
		}

		#region CreateSetMethod/WithTarget
		readonly static Type[] _createSetMethodArgs = new Type[] { _objectType, _objectType };
		readonly static MethodInfo _converter = typeof(Convert).GetMethod(nameof(Convert.ChangeType), BindingFlags.Public | BindingFlags.Static, null, new Type[] { _objectType, _typeType }, null);
		readonly static MethodInfo _getTypeFromHandle = _typeType.GetMethod(nameof(Type.GetTypeFromHandle));
		readonly static Dictionary<MemberInfo, DynamicMethod> _createSetMethodCache = new Dictionary<MemberInfo, DynamicMethod>();
		private static DynamicMethod CreateSetMethod(MemberInfo member)
		{
			if (_createSetMethodCache.TryGetValue(member, out DynamicMethod result))
				return result;

			PropertyInfo pi = member as PropertyInfo;
			FieldInfo fi = pi == null ? member as FieldInfo : null;
			MemberInfo mi = (MemberInfo)member;
			bool instanceHolder = !(pi == null ? fi.IsStatic : pi.GetGetMethod().IsStatic);
			bool isProp = fi == null;
			Type memberType = isProp ? pi.PropertyType : fi.FieldType;
			Type declaringType = mi.DeclaringType;
			result = new DynamicMethod(declaringType.AssemblyQualifiedName + "_Set" + mi.Name, typeof(void), _createSetMethodArgs, declaringType, true);
			_createSetMethodCache.Add(member, result);

			ILGenerator gen = result.GetILGenerator();
			if (instanceHolder)
			{
				gen.Emit(OpCodes.Ldarg_0);
				if (declaringType.IsClass)
					gen.Emit(OpCodes.Castclass, declaringType);
				else if (declaringType.IsValueType)
					gen.Emit(OpCodes.Unbox, declaringType);
			}
			gen.Emit(OpCodes.Ldarg_1);
			if (memberType.IsValueType)
			{
				gen.Emit(OpCodes.Ldtoken,memberType.IsEnum?typeof(int):memberType);
				gen.Emit(OpCodes.Call, _getTypeFromHandle);
				gen.Emit(OpCodes.Call, _converter);
				gen.Emit(OpCodes.Unbox_Any, memberType);
			}
			else
				gen.Emit(OpCodes.Castclass, memberType);

			if (isProp)
			{
				MethodInfo mei = pi.GetSetMethod();
				gen.Emit(instanceHolder&&mei.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, mei);
			}
			else
				gen.Emit(instanceHolder ? OpCodes.Stfld : OpCodes.Stsfld, fi);
			gen.Emit(OpCodes.Ret);
			return result;
		}

		private static Action<object> CreateSetMethodWithTarget(MemberInfo member, object target)
		{
			return (Action<object>)CreateSetMethod(member).CreateDelegate(typeof(Action<object>), target);
		}
		#endregion CreateSetMethod/WithTarget
		#endregion SetMember

		#region HasMember
		public static bool HasMember(object objectVar, string memberName)
		{
			switch (GetMemberInternal(objectVar, memberName, out object resultObject, out MemberInfo mmi, out object objectVarWrap, out Type objectVarType))
			{
				case 0:
					return false;
				//case 1: //this shouldn't happen in runtime
				case 2:
					return true;
				default:
					throw new InvalidProgramException($"Internal error on {nameof(HasMember)}");
			}
		}
		#endregion HasMember

		#region DeleteMember
		public static void DeleteMember(object instance, string memberName)
		{
			if (instance is IJsObject jsInstance)
				jsInstance.DeleteMember(memberName);
			throw new RuntimeException(RuntimeExceptionReason.DeleteOnStaticObject);
		}
		#endregion DeleteMember

		#region GetExternalLibrary
		public static object GetExternalLibrary(object instance, string alias, string predefinition)
		{
			object inst = GetExternalLibrary_GetRootInstance(instance);
			Assembly asm = null;
			FieldInfo fi;
			if ((fi = inst.GetType().GetField("~externalLibraryResolver", BindingFlags.NonPublic | BindingFlags.Instance)) != null)
				if (fi.GetValue(inst) is Action<ResolveExternalLibraryEventArgs> externalLibraryResolver)
				{
					ResolveExternalLibraryEventArgs args = new ResolveExternalLibraryEventArgs() { LibraryName = alias, Predefinition = predefinition };
					externalLibraryResolver(args);
					object library = args.Assembly;
					asm = library is string ? Assembly.LoadFrom((string)library) : library as Assembly;
				}
			if (asm == null)
				throw new RuntimeException(RuntimeExceptionReason.CantResolveLibrary, new object[] { alias, predefinition });
			return new ExternalLibraryObject() { _assembly = asm };
		}

		static object GetExternalLibrary_GetRootInstance(object instance)
		{
			while (true)
			{
				FieldInfo fi = instance.GetType().GetField("_parScope");
				if (fi == null)
					return instance;
				instance = fi.GetValue(instance);
			}
		}
		#endregion GetExternalLibrary

		#region GetArrayItem/SetArrayItem
		public static object GetArrayItem(object array, object index)
		{
			if (array is ArrayObject ao)
				return ao.GetItem(index);
			if ((array is string s) && (index is int indexInt))
				return s[indexInt];

			if (RuntimeMethodsExtender.ArrayGetItem(array, index, out object result))
				return result;

			Type arrayType = array.GetType();
			if ((typeof(IList).IsAssignableFrom(arrayType)) && (TypeConverter.ConvertTryConvert(ref index, typeof(int))))
				return ((IList)array)[(int)index];
			if (typeof(IDictionary).IsAssignableFrom(arrayType))
				return ((IDictionary)array)[index];
			if (index != null)
			{
				object objectVar = array.GetType();
				Type indexType = index.GetType();

				PropertyInfo[] pis = Array.FindAll(arrayType.GetProperties(BindingFlags.Public | BindingFlags.Instance), item => item.Name == "Item");
				foreach (PropertyInfo item in pis)
				{
					Type t = item.GetIndexParameters()[0].ParameterType;
					if (TypeConverter.GetConvertMethod(indexType, t, out MethodInfo indexConvertMethod))
					{
						Func<object, object, object> res = (Func<object, object, object>)CreateGetMethodWithIndex(item, indexType, indexConvertMethod).CreateDelegate(typeof(Func<object, object, object>));
						return res(array, index);
					}
				}
			}
			throw new RuntimeException(RuntimeExceptionReason.UnknownArrayType);
		}

		public static bool HasArrayItem(object array, object index)
		{
			if (array is ArrayObject)
				return ((ArrayObject)array).HasItem(index);

			if (RuntimeMethodsExtender.ArrayHasItem(array, index, out bool result))
				return result;

			Type arrayType = array.GetType();
			if ((typeof(IList).IsAssignableFrom(arrayType)) && (TypeConverter.ConvertTryConvert(ref index, typeof(int))))
				return ((IList)array).Count > (int)index;
			if (typeof(IDictionary).IsAssignableFrom(array.GetType()))
				return ((IDictionary)array).Contains(index);
			throw new RuntimeException(RuntimeExceptionReason.UnknownArrayType);
		}

		public static object DeleteArrayItem(object array, object index)
		{
			if (array is ArrayObject)
				return ((ArrayObject)array).DeleteItem(index);

			if (RuntimeMethodsExtender.ArrayDeleteItem(array, index, out bool result))
				return result;

			Type arrayType = array.GetType();
			if ((typeof(IList).IsAssignableFrom(arrayType)) && (TypeConverter.ConvertTryConvert(ref index, typeof(int))))
			{
				IList list = (IList)array;
				int indexInt = (int)index;
				result = list.Count > indexInt;
				list.RemoveAt(indexInt);
				return result;
			}
			if (typeof(IDictionary).IsAssignableFrom(array.GetType()))
			{
				IDictionary dict = (IDictionary)array;
				result=dict.Contains(index);
				dict.Remove(index);
				return result;
			}
			throw new RuntimeException(RuntimeExceptionReason.UnknownArrayType);
		}

		#region CreateGetMethodWithIndex
		//http://jachman.wordpress.com/2006/08/22/2000-faster-using-dynamic-method-calls/
		readonly static Type[] _createGetMethodArgsWithIndex = new Type[] { _objectType, _objectType };
		private static DynamicMethod CreateGetMethodWithIndex(PropertyInfo member, Type indexType, MethodInfo indexConvertMethod)
		{
			if (_createGetMethodCache.TryGetValue(member, out DynamicMethod result))
				return result;

			PropertyInfo pi = member;
			MemberInfo mi = member;
			bool instanceHolder = !pi.GetGetMethod().IsStatic;
			Type memberType = pi.PropertyType;
			Type declaringType = mi.DeclaringType;
			result = new DynamicMethod(declaringType.AssemblyQualifiedName + "_Get" + mi.Name, _objectType, _createGetMethodArgsWithIndex, member.DeclaringType, true);
			_createGetMethodCache.Add(member, result);

			ILGenerator gen = result.GetILGenerator();
			if (instanceHolder)
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Castclass, declaringType);
			}

			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Castclass, indexType);

			if (indexConvertMethod != null)
				gen.Emit(OpCodes.Call, indexConvertMethod);
			gen.Emit(instanceHolder ? OpCodes.Callvirt : OpCodes.Call, pi.GetGetMethod());
			if (memberType.IsValueType)
				gen.Emit(OpCodes.Box, memberType);
			gen.Emit(OpCodes.Ret);
			return result;
		}
		#endregion CreateGetMethodWithIndex

		public static void SetArrayItem(object array, object index, object value)
		{
			if (array is ArrayObject)
			{
				((ArrayObject)array).SetItem(index, value);
				return;
			}

			if (RuntimeMethodsExtender.ArraySetItem(array, index, value))
				return;

			Type arrayType = array.GetType();
			if ((typeof(IList).IsAssignableFrom(arrayType)) && (TypeConverter.ConvertTryConvert(ref index, typeof(int))))
			{
				((IList)array)[(int)index] = value;
				return;
			}
			if (typeof(IDictionary).IsAssignableFrom(array.GetType()))
			{
				((IDictionary)array)[index] = value;
				return;
			}
			throw new RuntimeException(RuntimeExceptionReason.UnknownArrayType);
		}
		#endregion GetArrayItem/SetArrayItem

		#region GetJsWrapper
		static object GetJsWrapper(object value)
		{
			if (value==null)
				return null;

			Type t = value.GetType();
			if (t == typeof(string))
				return new StringObject((string)value);
			if (t == typeof(DateTime))
				return new DateObject((DateTime)value);
			if ((typeof(IList).IsAssignableFrom(t)) && (!typeof(ArrayObject).IsAssignableFrom(t)))
				return new ArrayObjectLateBinding((IList)value);
			return value;
		}
		#endregion GetJsWrapper

		#region GetFuncType
		readonly static Dictionary<int,Type> _getFuncTypeCache=new Dictionary<int,Type>();
		readonly static string _thisAsmFullName=typeof(Func<>).Assembly.FullName;
		public static Type GetFuncType(int parmsCount)
		{
			if (!_getFuncTypeCache.TryGetValue(parmsCount, out Type result))
			{
				StringBuilder sb = new StringBuilder("System.Func`").Append(parmsCount + 1).Append('[');
				sb.Insert(sb.Length, "[System.Object],", parmsCount);
				sb.Append("[System.Object]]");
				result = Type.GetType(sb.ToString());
				_getFuncTypeCache.Add(parmsCount, result);
			}
			return result;
		}
		#endregion GetFuncType

		#region RuntimeMethodsExtender
		public interface IRuntimeMethodsExtender
		{
			bool ArrayGetItem(object array, object index, out object value);
			bool ArraySetItem(object array, object index, object value);
			bool ArrayHasItem(object array, object index, out bool result);
			bool ArrayDeleteItem(object array, object index, out bool deleted);
			bool DynamicCall(MethodInfo method, ParameterInfo[] pis, object target, object[] arguments, out object result);
		}

/*#if !DebugRuntime
		[DebuggerStepThrough]
#endif*/
		class NullRuntimeMethodsExtender : IRuntimeMethodsExtender
		{
			public bool ArrayGetItem(object array, object index, out object value)
			{
				value = null;
				return false;
			}

			public bool ArraySetItem(object array, object index, object value)
			{
				return false;
			}

			public bool ArrayHasItem(object array, object index, out bool result)
			{
				result=false;
				return false;
			}

			public bool ArrayDeleteItem(object array, object index, out bool deleted)
			{
				deleted = false;
				return false;
			}

			public bool DynamicCall(MethodInfo method, ParameterInfo[] pis, object target, object[] arguments, out object result)
			{
				result = null;
				return false;
			}
		}
		#endregion RuntimeMethodsExtender

		#region TypeConverters
		/*#if !DebugRuntime
				[DebuggerStepThrough]
		#endif*/
		static class TypeConverter
		{
			static TypeConverter()
			{
				FindConvertMethods();
			}

			internal static bool ConvertTryConvert(ref object value, Type targetType)
			{
				if (value == null)
					return false;

				Type valueType = value.GetType();
				if ((targetType == valueType) || (targetType.IsAssignableFrom(valueType)))
					return true;

				if ((!_convertMethods.TryGetValue(targetType, out Dictionary<Type,MethodInfo> part)) || (!part.TryGetValue(valueType, out MethodInfo mi)))
					return false;

				value = mi.Invoke(null, new object[] { value });
				return true;
			}

			internal static bool GetConvertMethod(Type sourceType, Type targetType, out MethodInfo result)
			{
				result = null;
				if ((targetType == sourceType) || (targetType.IsAssignableFrom(sourceType)))
					return true;
				return ((_convertMethods.TryGetValue(targetType, out Dictionary<Type,MethodInfo> part)) && (part.TryGetValue(sourceType, out result)));
			}

			static Dictionary<Type, Dictionary<Type, MethodInfo>> _convertMethods = new Dictionary<Type, Dictionary<Type, MethodInfo>>();
			static void FindConvertMethods()
			{
				MethodInfo[] mis = typeof(Convert).GetMethods(BindingFlags.Public | BindingFlags.Static);
				foreach (MethodInfo mi in mis)
				{
					if (!mi.Name.StartsWith("To"))
						continue;
					Type type1 = mi.ReturnType;
					if (!type1.IsValueType)
						continue;
					ParameterInfo[] parms = mi.GetParameters();
					if (parms.Length != 1)
						continue;
					Type type2 = parms[0].ParameterType;
					if (!type2.IsValueType)
						continue;
					if (type1 == type2)
						continue;

					if (!_convertMethods.TryGetValue(type1, out Dictionary<Type,MethodInfo> part))
					{
						part = new Dictionary<Type, MethodInfo>();
						_convertMethods.Add(type1, part);
					}
					part.Add(type2, mi);
				}
			}
		}
		#endregion TypeConverters

		#region GetPropertyEnumerator (ForIn)
		public static IEnumerator<object> GetPropertyEnumerator(object obj)
		{
			return (obj is IJsObject jsObj
				? jsObj.EnumMembers().Cast<object>()
				: ReflectionJsObject.GetProperties(obj.GetType()))
					.GetEnumerator();
		}
		#endregion GetPropertyEnumerator (ForIn)

		#region InstanceOf/TypeOf
		public static object InstanceOf(object obj, object type)
		{
			return type.GetType().IsAssignableFrom(type.GetType());
		}

		public static object TypeOf(object obj)
		{
			if (obj == null)
				return "object";

			Type type = obj.GetType();
			return
				type == typeof(bool) ? "boolean" :
				(type==typeof(int))||(type==typeof(byte))||(type==typeof(char))||(type==typeof(short))||(type==typeof(ushort))||(type==typeof(sbyte))
						||(type==typeof(uint))||(type==typeof(long))||(type==typeof(ulong))||(type==typeof(float))||(type==typeof(double))||(type==typeof(decimal))
					? "number" :
				type==typeof(string) ? "string" :
				(type.IsGenericType) && (type.GetGenericTypeDefinition().FullName.StartsWith("System.Func")) ? "function" :
				"object";
		}
		#endregion InstanceOf/TypeOf

		#region MakeJsException
		public static JsError MakeJsError(Exception ex)
		{
			return new JsError(ex);
		}
		#endregion MakeJsException

		#region dynam call
		public static object CreateInstance(object type, object[] args)
		{
			if (type is StaticObject so)
			{
				int argsLength = args.Length;
				Type typeReal=so.Type;
				ConstructorInfo ci=FindMethod(typeReal.GetConstructors(BindingFlags.Public|BindingFlags.Instance).Select(x=>(x,x.GetParameters())).ToArray(),args);
				if (ci==null)
				{
					if ((typeof(MulticastDelegate).IsAssignableFrom(typeReal)) && (argsLength>0) && (args[0] is IJsFunction jsFunc))
					{
						ci = typeReal.GetConstructors(BindingFlags.Public|BindingFlags.Instance).FirstOrDefault(x => x.GetParameters().Length==2);
						return ci.DirectInvoke(new object[] { jsFunc,jsFunc.GetType().GetMethod(nameof(IJsFunction.Invoke)).MethodHandle.GetFunctionPointer() });
					}

					throw new RuntimeException(RuntimeExceptionReason.NoCompatibleMethod);
				}
				return ci.DirectInvoke(args);
			}

			if (type is IJsFunction jsFun)
				return jsFun.Instantiate(args);

			//throw new NotImplementedException();
			throw new RuntimeException(RuntimeExceptionReason.NoCompatibleMethod);


			/*
			 				Lookup funAsLookup;
				Member funAsMember;

				if (IsType(node.Function, out funAsLookup))
				{
					string objName = funAsLookup.Name;
					Type[] types = new Type[argsCount];
					for (int a = 0; a<argsCount; a++)
						types[a]=_objectType;
					ConstructorInfo ci;
					switch (objName)
					{
						case "Array":
							gen.Emit(ILOpCode.Newobj, GetRuntimeType(typeof(Jellequin.Runtime.ArrayObject)).GetConstructor(Type.EmptyTypes));
							funInfo.Pushes++;
							break;
						case "Date":
							ci=GetRuntimeType(typeof(Jellequin.Runtime.DateObject)).GetConstructors().FirstOrDefault(x => x.GetParameters().Length==argsCount);
							if (ci==null)
								throw new RuntimeException(RuntimeExceptionReason.NoCompatibleMethod, objName, ".ctor", argsCount);
							CompileNode(node.Arguments);
							funInfo.Pushes-=argsCount;
							gen.Emit(ILOpCode.Newobj, ci);
							funInfo.Pushes++;
							break;
						case "RegExp":
							if (argsCount>2)
								throw new RuntimeException(RuntimeExceptionReason.NoCompatibleMethod, objName, ".ctor", argsCount);
							CompileNode(node.Arguments);
							if (argsCount==1)
							{
								gen.Emit(ILOpCode.Ldnull);
								funInfo.Pushes++;
								Array.Resize(ref types, 2);
								types[1]=_objectType;
							}
							gen.Emit(ILOpCode.Newobj, GetRuntimeType(typeof(Jellequin.Runtime.RegExpObject)).GetConstructor(types));
							funInfo.Pushes--;
							break;
						case "Error":
						case "TypeError":
							if (argsCount!=1)
								throw new RuntimeException(RuntimeExceptionReason.NoCompatibleMethod, objName, ".ctor", argsCount);
							CompileNode(node.Arguments);
							funInfo.Pushes-=argsCount;
							gen.Emit(ILOpCode.Newobj, GetRuntimeType(typeof(Jellequin.Runtime.JsError)).GetConstructor(types));
							funInfo.Pushes++;
							break;
						default:
							/*if (_useDynamicJsMembers)
								GetVar(funAsLookup);
							else
							{
								VarInfo customObj;
								if (!funInfo.Vars.TryGetValue(objName, out customObj))
									throw new CompilerException(CompilerExceptionReason.UnknownObjectType, new object[] { objName });

								gen.Emit(ILOpCode.Ldarg_0);
								if (customObj.Field==null) //property uses parent's one
									gen.Emit(ILOpCode.Call, customObj.Property.GetMethod);
								else
									gen.Emit(ILOpCode.Ldfld, customObj.Field);
								funInfo.Pushes++;
							}
							CompileNode(node.Arguments);
							funInfo.Pushes-=argsCount;
							gen.Emit(ILOpCode.Callvirt, RuntimeMethods.GetFuncType(argsCount).GetMethod("Invoke"));/

			if (_useDynamicJsMembers)
				GetVar(funAsLookup);
			else
			{
				VarInfo customObj;
				if (!funInfo.Vars.TryGetValue(objName, out customObj))
					throw new CompilerException(CompilerExceptionReason.UnknownObjectType, new object[] { objName });

				gen.Emit(ILOpCode.Ldarg_0);
				if (customObj.Field == null) //property uses parent's one
					gen.Emit(ILOpCode.Call, customObj.Property.GetMethod);
				else
					gen.Emit(ILOpCode.Ldfld, customObj.Field);
				funInfo.Pushes++;
			}

			gen.Emit(ILOpCode.Ldc_i4, argsCount);
			gen.Emit(ILOpCode.Newarr, _objectType);
			funInfo.Pushes++;
			int a = 0;
			foreach (AstNode arg in node.Arguments)
			{
				gen.Emit(ILOpCode.Dup);
				gen.Emit(ILOpCode.Ldc_i4, a++);
				CompileNode(arg);
				gen.Emit(ILOpCode.Stelem_ref);
				funInfo.Pushes--;
			}
			CallRuntimeMethod("CreateInstance");
			funInfo.Pushes--;
			break;
		}
	}
				else if (IsType(node.Function, out funAsMember))
				{
					CompileNode(funAsMember);
	/*Type type = typeof(StaticObject);
	gen.Emit(ILOpCode.Castclass, type);
	gen.Emit(ILOpCode.Ldfld, type.GetField("Type"));/
	CompileNode(node.Arguments);
	funInfo.Pushes-=argsCount;
					CallRuntimeMethod(RuntimeMethods.GetDelegateConstuctor(argsCount));
}
				else
					throw new NotImplementedException();
			
			 */
		}

		public static object CallMethod(object obj, object[] args)
		{
			if (obj is IInvokable jsFun)
				return jsFun.Invoke(args);
			if (obj is Delegate del)
				return del.DirectInvoke(args);

			if (obj is ExternalEventInfoWithOperation eei)
			{
				if (args.Length < 2)
					throw new RuntimeException(RuntimeExceptionReason.NoCompatibleMethod);
				if (eei.IsAdd)
				{
					DynamDelCalls.CreateEventHandlerBridge(eei.Type.GetEvent((string)args[0]), eei.Target, (IInvokable)args[1]);
					return null;
				}
				else
					throw new RuntimeException(RuntimeExceptionReason.NoCompatibleMethod);
			}

			throw new RuntimeException(RuntimeExceptionReason.NoCompatibleMethod);
		}
		#endregion dynam call

		#region FindMethod
		internal static T FindMethod<T>(IEnumerable<(T Method, ParameterInfo[] Parameters)> methods,object[] arguments) where T : class
		{
			int argsCount = arguments.Length;
			Type[] valTypes = arguments.Select(x => x?.GetType()).ToArray();
			IEnumerable<(T Method, ParameterInfo[] Parameters)> baseQuery=methods.Where(x => x.Parameters.Length==argsCount);
			if (argsCount==0)
			{
				(T Method, ParameterInfo[] Parameters) res=baseQuery.FirstOrDefault();
				return res.Equals(default((T Method, ParameterInfo[] Parameters))) ? null : res.Method;
			}
			return baseQuery
				.Select(x =>
				{
					int score = 0;
					int a = 0;
					foreach (ParameterInfo pi in x.Parameters)
					{
						Type parmType = pi.ParameterType;
						Type valType = valTypes[a++];

						if (valType==null&&parmType.IsClass) //null can be assigned to class -> 2 points
							score+=2;
						else if (parmType.Equals(valType)) //same parm type gets 2 points
							score+=2;
						else if (parmType.IsAssignableFrom(valType)) //compatible parm type gets 2 points
							score+=2;
						else if (TypeConverter.GetConvertMethod(valType,parmType,out MethodInfo indexConvertMethod)) //convertible parm type gets 1 points
							score+=1;
					}

					return new { x.Method,Score = score };
				})
				.Where(x => x.Score>=argsCount)
				.OrderByDescending(x => x.Score).FirstOrDefault()?.Method;
		}
		#endregion FindMethod
	}

	#region DynamDelCalls
	/*#if !DebugRuntime
			[DebuggerStepThrough]
	#endif*/
	public static class DynamDelCalls
	{
		readonly static MethodInfo _translateTypeMi = typeof(DynamDelCalls).GetMethod(nameof(DynamDelCalls.TranslateType), BindingFlags.Public|BindingFlags.Static);
		readonly static Type _typeType = typeof(Type);
		readonly static MethodInfo _getTypeFromHandle = _typeType.GetMethod(nameof(Type.GetTypeFromHandle));
		readonly static Type _objectType = typeof(object);
		readonly static Type _objectArrayType = typeof(object[]);

		//#if !DebugRuntime
		[DebuggerHidden]
		//#endif
		public static object DirectInvoke(this Delegate del, object[] args)
		{
			return DirectInvoke(del.Method, del.Target, args);
		}

		public static object DirectInvoke(MethodBase method, object target, object[] args)
		{
			Type declaringType = method.DeclaringType;
			//DynamicMethod bridge = new DynamicMethod($"{declaringType.AssemblyQualifiedName}{declaringType.Name}{method.Name}_Bridge", _objectType, new[] { _objectType, _objectArrayType }, declaringType, true);
			DynamicMethod bridge = new DynamicMethod($"{declaringType.AssemblyQualifiedName}{declaringType.Name}{method.Name}_Bridge", _objectType, new[] { _objectType, _objectArrayType }, typeof(DynamDelCalls).Assembly.ManifestModule, true);
			ILGenerator gen = bridge.GetILGenerator();

			if ((!method.IsStatic) && (!method.IsConstructor))
			{
				gen.Emit(OpCodes.Ldarg_0);
				Type targetType = target.GetType();
				if (targetType.IsValueType)
					gen.Emit(OpCodes.Unbox, targetType);
			}

			int a = 0;
			ParameterInfo[] pis = method.GetParameters();
			foreach (ParameterInfo pi in pis)
			{
				Label labElse = gen.DefineLabel();
				Label labEnd = gen.DefineLabel();
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Call, typeof(Array).GetProperty(nameof(Array.Length)).GetMethod);
				gen.Emit(OpCodes.Ldc_I4, a + 1);
				gen.Emit(OpCodes.Blt, labElse);

				//get arg
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Ldc_I4, a);
				gen.Emit(OpCodes.Ldelem_Ref);
				gen.Emit(OpCodes.Br, labEnd);

				//default
				gen.MarkLabel(labElse);
				if (pi.ParameterType.IsValueType)
				{
					gen.Emit(OpCodes.Ldc_I4_0);
					gen.Emit(OpCodes.Box, typeof(int));
				}
				else
					gen.Emit(OpCodes.Ldnull);

				//convert
				gen.MarkLabel(labEnd);
				gen.Emit(OpCodes.Ldtoken, pi.ParameterType);
				gen.Emit(OpCodes.Call, _getTypeFromHandle);
				gen.Emit(OpCodes.Call, _translateTypeMi);
				if (pi.ParameterType.IsValueType)
					gen.Emit(OpCodes.Unbox_Any, pi.ParameterType);

				a++;
			}

			if (method is MethodInfo mi)
			{
				gen.Emit(OpCodes.Call, mi);
				Type returnType = mi.ReturnType;
				if (returnType.Equals(typeof(void)))
					gen.Emit(OpCodes.Ldnull);
				else if (returnType.IsValueType)
					gen.Emit(OpCodes.Box, returnType);
			}
			else if (method is ConstructorInfo ci)
				gen.Emit(OpCodes.Newobj, ci);
			else
				//throw new NotImplementedException();
				throw new InvalidProgramException($"Internal error on {nameof(DynamDelCalls)}.{nameof(DirectInvoke)}");
			gen.Emit(OpCodes.Ret);

			return ((Func<object, object[], object>)bridge.CreateDelegate(typeof(Func<object, object[], object>)))(target, args ?? new object[0]);
		}

		public static object DirectInvoke(this ConstructorInfo ci, object[] args)
		{
			return DirectInvoke(ci, null, args);
		}

		public static object TranslateType(object value, Type targetType)
		{
			if (targetType.Equals(_objectType))
				return value;

			if (value==null)
				return value;

			if (targetType.IsAssignableFrom(value.GetType()))
				return value;

			/*if (value is Delegate del)
				//return targetType.GetConstructors()[0].Invoke(new object[] { del.Target,del.Method.MethodHandle.GetFunctionPointer() });
				return Delegate.CreateDelegate(targetType, del.Target, del.Method);*/

			return Convert.ChangeType(value, targetType);
		}

		internal static void CreateEventHandlerBridge(EventInfo ei, object target, IInvokable handler)
		{
			Type declaringType = ei.DeclaringType;
			Type eventHandlerType = ei.EventHandlerType;
			ParameterInfo[] eventHandlerTypeParms = eventHandlerType.GetMethod(nameof(MethodInfo.Invoke)).GetParameters();
			Type[] parmTypes = new Type[eventHandlerTypeParms.Length + 1];
			parmTypes[0] = typeof(object);
			int a = 0;
			foreach (ParameterInfo pi in eventHandlerTypeParms)
				parmTypes[++a] = pi.ParameterType;
			DynamicMethod dm = new DynamicMethod($"{declaringType.AssemblyQualifiedName}_Add{ei.Name}", typeof(void), parmTypes, declaringType, true);
			ILGenerator gen = dm.GetILGenerator();
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldc_I4, 2);
			gen.Emit(OpCodes.Newarr, _objectType);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Ldc_I4_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Stelem_Ref);
			gen.Emit(OpCodes.Dup);
			gen.Emit(OpCodes.Ldc_I4_1);
			gen.Emit(OpCodes.Ldarg_2);
			gen.Emit(OpCodes.Stelem_Ref);
			gen.Emit(OpCodes.Call, handler.GetType().GetMethod(nameof(MethodInfo.Invoke)));
			gen.Emit(OpCodes.Pop);
			gen.Emit(OpCodes.Ret);

			ei.AddEventHandler(target, dm.CreateDelegate(eventHandlerType, handler));
		}
	}
	#endregion DynamDelCalls

	#region ExternalVirtualObject
	internal class ExternalLibraryObject
	{
		internal Assembly _assembly;
	}

	internal class NamespaceObject : ExternalLibraryObject
	{
		internal string _name;
	}

	public class StaticObject
	{
		public Type Type;
	}

/*#if !DebugRuntime
	[DebuggerStepThrough]
#endif*/
	public class ExternalMethodInfo:IInvokable
	{
		internal object Target;
		internal Type Type;
		internal string MethodName;

		public object Invoke(object[] arguments)
		{
			int parmsCount = arguments.Length;
			MethodInfo mi = RuntimeMethods.FindMethod(Type.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static)
				.Where(x=>(x.Name==MethodName)&&(!x.IsGenericMethodDefinition))
				.Select(x=>(x,x.GetParameters())),arguments);
			if (mi==null)
				throw new RuntimeException(RuntimeExceptionReason.NoCompatibleMethod, Type, MethodName, parmsCount);
			return DynamDelCalls.DirectInvoke(mi, Target, arguments);
		}
	}

/*#if !DebugRuntime
	[DebuggerStepThrough]
#endif*/
	public class ExternalEventInfoBase
	{
		internal object Target;
		internal Type Type;
	}

	public class ExternalEventInfoWithEvent : ExternalEventInfoBase
	{
		internal string EventName;
	}

	public class ExternalEventInfoWithOperation:ExternalEventInfoBase
	{
		internal bool IsAdd;
	}

	public delegate void ResolveExternalLibraryEventHandler(object sender, ResolveExternalLibraryEventArgs e);
	public class ResolveExternalLibraryEventArgs
	{
		public string LibraryName { get; internal set; }
		public string Predefinition { get; internal set; }
		public object Assembly { get; set; }
	}
	#endregion ExternalVirtualObject

	#region JsException
/*#if !DebugRuntime
	[DebuggerStepThrough]
#endif*/
	public class JsError : Exception
	{
		readonly Exception _ex;
		readonly int _number;
		readonly string _stackTrace;

		public JsError(Exception ex) : base(ex.Message, ex)
		{
			_ex = ex;
			if (ex is JsError jsError)
				this._number = jsError._number;
		}

		public JsError(object message)
		{
			_ex = new Exception((string)message) { Source = nameof(Jellequin) };
			_stackTrace = new StackTrace(1).ToString();
		}

		public override IDictionary Data
		{
			get { return _ex.Data; }
		}

		public override Exception GetBaseException()
		{
			return _ex.GetBaseException();
		}

		public override string Message
		{
			get { return _ex.Message; }
		}

		public override string Source
		{
			get { return _ex.Source; }
			set { _ex.Source = value; }
		}

		public override string StackTrace
		{
			get { return _stackTrace ?? _ex.StackTrace; }
		}

		#pragma warning disable IDE1006 //make the property compatible with JS
		public string description
		{
			get { return this.Message; }
		}

		public string message
		{
			get { return this.Message; }
		}
		#pragma warning restore IDE1006
	}
	#endregion JsException

	#region RuntimeException
	/*#if !DebugRuntime
			[DebuggerStepThrough]
	#endif*/
	public class RuntimeException : Exception
	{
		public RuntimeException(RuntimeExceptionReason reason)
		{
			Reason = reason;
		}

		public RuntimeException(RuntimeExceptionReason reason, params object[] messageData) : this(reason)
		{
			MessageData = messageData;
		}

		public RuntimeExceptionReason Reason { get; private set; }

		public object[] MessageData { get; private set; }

		public override string Message
		{
			get
			{
				switch (Reason)
				{
					case RuntimeExceptionReason.Compare:
						return "Can't compare values.";
					case RuntimeExceptionReason.NoExternalVariable:
						return string.Format("There is no external variable \"{0}\" declared.", MessageData);
					case RuntimeExceptionReason.NoMemberOnExternalVariable:
						return string.Format("There is no compatible member \"{1}\" on external variable \"{0}\".", MessageData);
					case RuntimeExceptionReason.NoPropertyOnExternalVariable:
						return string.Format("There is no compatible property \"{1}\" on external variable \"{0}\".", MessageData);
					case RuntimeExceptionReason.UnknownArrayType:
						return "Unknown array type - use extender.";
					case RuntimeExceptionReason.NoCompatibleMethod:
						return string.Format("There is no compatible method \"{1}\" with {2} object parameters.", MessageData);
					case RuntimeExceptionReason.CantResolveLibrary:
						return string.Format("Can't resolve external library based on \"{1}\" definition.", MessageData);
					case RuntimeExceptionReason.DeleteOnStaticObject:
						return "Can't delete member on non-dynamic JS object.";
					case RuntimeExceptionReason.Unsupported:
						return "Unsupported feature.";
					default:
						return "Unknown error";
				}
			}
		}
	}

	public enum RuntimeExceptionReason { Compare, NoExternalVariable, NoMemberOnExternalVariable, NoPropertyOnExternalVariable, UnknownArrayType, CantResolveLibrary, NoCompatibleMethod, DeleteOnStaticObject, Unsupported }
	#endregion RuntimeException

	#region IJsObject/IInvokable/IJsFunction
	public interface IJsObject
	{
		object GetValue(string name); //self+parentScope
		void SetValue(string name, object value); //self+parentScope
		bool HasMember(string name); //self+parentScope
		void DeleteMember(string name); //self+parentScope
		IEnumerable<string> EnumMembers();
	}

	public interface IInvokable
	{
		object Invoke(object[] arguments);
    }

	#region IInvokableExtensions.ToFunc
	public static class IInvokableExtensions
	{
		public static Func<TResult> ToFunc<TResult>(this IInvokable invokable)
		{
			Func<object[], object> f = GetInvokeFunc(invokable);
			return () => (TResult)f(new object[0]);
		}

		public static Func<T,TResult> ToFunc<T,TResult>(this IInvokable invokable)
		{
			Func<object[], object> f = GetInvokeFunc(invokable);
			return (p1) => (TResult)f(new object[] { p1 });
		}

		public static Func<T1, T2, TResult> ToFunc<T1, T2, TResult>(this IInvokable invokable)
		{
			Func<object[], object> f=GetInvokeFunc(invokable);
			return (p1, p2) => (TResult)f(new object[] { p1, p2});
		}

		public static Func<T1, T2, T3, TResult> ToFunc<T1, T2, T3, TResult>(this IInvokable invokable)
		{
			Func<object[], object> f = GetInvokeFunc(invokable);
			return (p1, p2, p3) => (TResult)f(new object[] { p1, p2, p3 });
		}

		public static Func<T1, T2, T3, T4, TResult> ToFunc<T1, T2, T3, T4, TResult>(this IInvokable invokable)
		{
			Func<object[], object> f = GetInvokeFunc(invokable);
			return (p1, p2, p3, p4) => (TResult)f(new object[] { p1, p2, p3, p4 });
		}

		public static Func<T1, T2, T3, T4, T5, TResult> ToFunc<T1, T2, T3, T4, T5, TResult>(this IInvokable invokable)
		{
			Func<object[], object> f = GetInvokeFunc(invokable);
			return (p1, p2, p3, p4, p5) => (TResult)f(new object[] { p1, p2, p3, p4, p5 });
		}

		public static Func<T1, T2, T3, T4, T5, T6, TResult> ToFunc<T1, T2, T3, T4, T5, T6, TResult>(this IInvokable invokable)
		{
			Func<object[], object> f = GetInvokeFunc(invokable);
			return (p1, p2, p3, p4, p5, p6) => (TResult)f(new object[] { p1, p2, p3, p4, p5, p6 });
		}

		public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(this IInvokable invokable)
		{
			Func<object[], object> f = GetInvokeFunc(invokable);
			return (p1, p2, p3, p4, p5, p6, p7) => (TResult)f(new object[] { p1, p2, p3, p4, p5, p6, p7 });
		}

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this IInvokable invokable)
		{
			Func<object[], object> f = GetInvokeFunc(invokable);
			return (p1, p2, p3, p4, p5, p6, p7, p8) => (TResult)f(new object[] { p1, p2, p3, p4, p5, p6, p7, p8 });
		}

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this IInvokable invokable)
		{
			Func<object[], object> f = GetInvokeFunc(invokable);
			return (p1, p2, p3, p4, p5, p6, p7, p8, p9) => (TResult)f(new object[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 });
		}

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> ToFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this IInvokable invokable)
		{
			Func<object[], object> f = GetInvokeFunc(invokable);
			return (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10) => (TResult)f(new object[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 });
		}

		internal static Func<object[], object> GetInvokeFunc(object invokable)
		{
			if (invokable is IInvokable inv)
				return inv.Invoke;

			MethodInfo mi = invokable.GetType().GetMethod("Invoke");
			return (Func<object[], object>)typeof(Func<object[], object>).GetConstructors()[0].Invoke(new object[] { invokable, mi.MethodHandle.GetFunctionPointer() });
		}
	}

	public static class IJsObjectExtensions
	{
		public static Func<TResult> GetFunc<TResult>(this IJsObject jsObject, string name)
		{
			/*Func<object[], object> f = GetInvokeFunc(jsObject, name);
			return () => (TResult)f(new object[0]);*/
			return GetInvokeFunc(jsObject, name).Invoke<TResult>;
		}

		public static Func<T, TResult> GetFunc<T, TResult>(this IJsObject jsObject, string name)
		{
			/*Func<object[], object> f = GetInvokeFunc(jsObject, name);
			return p => (TResult)f(new object[] { p });*/
			return GetInvokeFunc(jsObject, name).Invoke<T, TResult>;
		}

		public static Func<T1, T2, TResult> GetFunc<T1, T2, TResult>(this IJsObject jsObject, string name)
		{
			/*Func<object[], object> f = GetInvokeFunc(jsObject, name);
			return (p1, p2) => (TResult)f(new object[] { p1, p2 });*/
			return GetInvokeFunc(jsObject, name).Invoke<T1, T2, TResult>;
		}

		public static Func<T1, T2, T3, TResult> GetFunc<T1, T2, T3, TResult>(this IJsObject jsObject, string name)
		{
			/*Func<object[], object> f = GetInvokeFunc(jsObject, name);
			return (p1, p2, p3) => (TResult)f(new object[] { p1, p2, p3 });*/
			return GetInvokeFunc(jsObject, name).Invoke<T1, T2, T3, TResult>;
		}

		public static Func<T1, T2, T3, T4, TResult> GetFunc<T1, T2, T3, T4, TResult>(this IJsObject jsObject, string name)
		{
			/*Func<object[], object> f = GetInvokeFunc(jsObject, name);
			return (p1, p2, p3, p4) => (TResult)f(new object[] { p1, p2, p3, p4 });*/
			return GetInvokeFunc(jsObject, name).Invoke<T1, T2, T3, T4, TResult>;
		}

		public static Func<T1, T2, T3, T4, T5, TResult> GetFunc<T1, T2, T3, T4, T5, TResult>(this IJsObject jsObject, string name)
		{
			/*Func<object[], object> f = GetInvokeFunc(jsObject, name);
			return (p1, p2, p3, p4, p5) => (TResult)f(new object[] { p1, p2, p3, p4, p5 });*/
			return GetInvokeFunc(jsObject, name).Invoke<T1, T2, T3, T4, T5, TResult>;
		}

		public static Func<T1, T2, T3, T4, T5, T6, TResult> GetFunc<T1, T2, T3, T4, T5, T6, TResult>(this IJsObject jsObject, string name)
		{
			/*Func<object[], object> f = GetInvokeFunc(jsObject, name);
			return (p1, p2, p3, p4, p5, p6) => (TResult)f(new object[] { p1, p2, p3, p4, p5, p6 });*/
			return GetInvokeFunc(jsObject, name).Invoke<T1, T2, T3, T4, T5, T6, TResult>;
		}

		public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> GetFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(this IJsObject jsObject, string name)
		{
			/*Func<object[], object> f = GetInvokeFunc(jsObject, name);
			return (p1, p2, p3, p4, p5, p6, p7) => (TResult)f(new object[] { p1, p2, p3, p4, p5, p6, p7 });*/
			return GetInvokeFunc(jsObject, name).Invoke<T1, T2, T3, T4, T5, T6, T7, TResult>;
		}

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> GetFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this IJsObject jsObject, string name)
		{
			/*Func<object[], object> f = GetInvokeFunc(jsObject, name);
			return (p1, p2, p3, p4, p5, p6, p7, p8) => (TResult)f(new object[] { p1, p2, p3, p4, p5, p6, p7, p8 });*/
			return GetInvokeFunc(jsObject, name).Invoke<T1, T2, T3, T4, T5, T6, T7, T8, TResult>;
		}

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> GetFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this IJsObject jsObject, string name)
		{
			/*Func<object[], object> f = GetInvokeFunc(jsObject, name);
			return (p1, p2, p3, p4, p5, p6, p7, p8, p9) => (TResult)f(new object[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 });*/
			return GetInvokeFunc(jsObject, name).Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>;
		}

		public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> GetFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this IJsObject jsObject, string name)
		{
			/*Func<object[], object> f = GetInvokeFunc(jsObject, name);
			return (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10) => (TResult)f(new object[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 });*/
			return GetInvokeFunc(jsObject, name).Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>;
		}

		class WrapFunc
		{
			internal Func<object[], object> func;

			[DebuggerHidden]
			internal TResult Invoke<TResult>()
				=> (TResult)func(new object[0]);

			[DebuggerHidden]
			internal TResult Invoke<T, TResult>(T p)
				=> (TResult)func(new object[] { p});

			[DebuggerHidden]
			internal TResult Invoke<T1, T2, TResult>(T1 p1, T2 p2)
				=> (TResult)func(new object[] { p1, p2 });

			[DebuggerHidden]
			internal TResult Invoke<T1, T2, T3, TResult>(T1 p1, T2 p2, T3 p3)
				=> (TResult)func(new object[] { p1, p2, p3});

			[DebuggerHidden]
			internal TResult Invoke<T1, T2, T3, T4, TResult>(T1 p1, T2 p2, T3 p3, T4 p4)
				=> (TResult)func(new object[] { p1, p2, p3, p4});

			[DebuggerHidden]
			internal TResult Invoke<T1, T2, T3, T4, T5, TResult>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
				=> (TResult)func(new object[] { p1, p2, p3, p4, p5});

			[DebuggerHidden]
			internal TResult Invoke<T1, T2, T3, T4, T5, T6, TResult>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
				=> (TResult)func(new object[] { p1, p2, p3, p4, p5, p6});

			[DebuggerHidden]
			internal TResult Invoke<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
				=> (TResult)func(new object[] { p1, p2, p3, p4, p5, p6, p7});

			[DebuggerHidden]
			internal TResult Invoke<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)
				=> (TResult)func(new object[] { p1, p2, p3, p4, p5, p6, p7, p8});

			[DebuggerHidden]
			internal TResult Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9)
				=> (TResult)func(new object[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 });

			[DebuggerHidden]
			internal TResult Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10)
				=> (TResult)func(new object[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 });
		}

		static WrapFunc GetInvokeFunc(IJsObject jsObject, string name)
		{
			return new WrapFunc { func = IInvokableExtensions.GetInvokeFunc(jsObject.GetValue(name)) };
		}
	}
	#endregion IInvokableExtensions.ToFunc

	public interface IJsFunction:IJsObject,IInvokable
	{
        object Instantiate(object[] arguments);
    }

	public class ReflectionJsObject:IJsObject
	{
		Type _type;
		public ReflectionJsObject()
		{
			_type = this.GetType();
		}

		public object GetValue(string name)
		{
			return _type.GetProperty(name)?.GetValue(this);
		}

		public bool HasMember(string name)
		{
			return _type.GetProperty(name) != null;
		}

		public void SetValue(string name, object value)
		{
			_type.GetProperty(name)?.SetValue(this, value);
		}

		public void DeleteMember(string name)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> EnumMembers()
		{
			return GetProperties(_type);
		}

		internal static IEnumerable<string> GetProperties(Type type)
		{
			return type.GetProperties().Select(x => x.Name);
		}
	}

	public class DynamicJsObject : IJsObject
	{
		Dictionary<string, object> _vals = new Dictionary<string, object>();
		public virtual object GetValue(string name)
		{
			if (_vals.TryGetValue(name, out object res))
				return res;
			IJsObject parScope = ParScope();
			return ParScopeHasMember(parScope, name) ? parScope.GetValue(name) : null;
		}

		public virtual bool HasMember(string name)
		{
			return _vals.ContainsKey(name) || ParScopeHasMember(ParScope(), name);
		}

		public virtual void SetValue(string name, object value)
		{
			IJsObject parScope = ParScope();
			if ((!_vals.ContainsKey(name)) && (ParScopeHasMember(parScope, name)))
			{
				parScope.SetValue(name, value);
				return;
			}
			_vals[name] = value;
		}

		public virtual void DeleteMember(string name)
		{
			ParScope()?.DeleteMember(name);
			_vals.Remove(name);
		}

		public virtual IEnumerable<string> EnumMembers()
		{
			IEnumerable<string> res = _vals.Select(item=>item.Key);
			IJsObject parScope = ParScope();
			return (parScope==null ? res : res.Union(parScope.EnumMembers()));
		}

		bool ParScopeHasMember(IJsObject parScope,string name)
		{
			return parScope!=null&&parScope.HasMember(name);
		}

		bool _parScopeSet;
		IJsObject _parScope;
		protected virtual IJsObject ParScope()
		{
			if (_parScopeSet)
				return _parScope;
			_parScope = this.GetType().GetField("~~parScope", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(this) as IJsObject;
			_parScopeSet = true;
			return _parScope;
		}
	}

	public class StaticJsObject : ReflectionJsObject
	{ }
	#endregion IJsObject/IInvokable/IJsFunction
}
