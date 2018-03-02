/// ============================================
/// Tuple
/// Tuple is a group of generic classes introduced in .NET Framework 4.0.
/// These generic classes are simple but very handy in many cases.
/// We reimplemented them up to 6-dimension tuple for convenience.
/// ============================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TupleDescriptionAttribute : Attribute
{
	public int Dimension;
}

public class Tuple
{
	/// <summary>
	/// Copy values from sourceTuple into targetTuple.
	/// </summary>
	/// <typeparam name="T1">The type of target tuple.</typeparam>
	/// <typeparam name="T2">The type of source tuple.</typeparam>
	/// <param name="targetTuple">The target tuple.</param>
	/// <param name="sourceTuple">The source tuple.</param>
	/// <param name="sourceIndices">The indices of tuple elements to be copied from source tuple. Use -1 to ignore certain elements. Index values start from 1.</param>
	/// <returns>The modified target tuple if it's successfully assembled.</returns>
	public Tuple Assemble(Tuple sourceTuple, int[] sourceIndices) 
	{
		TupleDescriptionAttribute tgtAttrib = this.GetType().GetCustomAttributes(typeof(TupleDescriptionAttribute), true)[0] as TupleDescriptionAttribute;

		if (sourceIndices.Length != tgtAttrib.Dimension)
			return this;

		for (int i = 0; i < sourceIndices.Length; ++i)
		{
			int srcIndex = sourceIndices[i];

			if (srcIndex > Dimension)
				continue;
			if (srcIndex == -1)
				continue;

			this.GetType().GetField("Item" + (i + 1).ToString()).SetValue(this, 
				sourceTuple.GetType().GetField("Item" + srcIndex.ToString()).GetValue(sourceTuple));
		}

		return this;
	}

	/// <summary>
	/// Get/Set the item in the tuple.
	/// </summary>
	/// <param name="index">The index of the object you want to access. Starts from 1.</param>
	/// <returns>The specified item inside the tuple</returns>
	public object this [int index]
	{
		get
		{
			return this.GetType().GetField("Item" + index.ToString()).GetValue(this);
		}
		set
		{
			if(index <= 0 || index > Dimension) return;
			System.Reflection.FieldInfo fi = this.GetType().GetField("Item" + index.ToString());
			if(fi != null && value.GetType() == fi.FieldType || value.GetType().IsSubclassOf(fi.FieldType))
			{
				fi.SetValue(this, value);
			}
		}
	}

	private int dimension = -1;
	private bool checkedDimension = false;
	public int Dimension
	{
		get
		{
			if(checkedDimension == false)
			{
				TupleDescriptionAttribute selfAttrib = this.GetType().GetCustomAttributes(typeof(TupleDescriptionAttribute), true)[0] as TupleDescriptionAttribute;
				dimension = selfAttrib.Dimension;
				checkedDimension = true;
			}

			return dimension;
		}
	}
}

[TupleDescription(Dimension = 1)]
public class Tuple<T1> : Tuple, IEquatable<Tuple<T1>>
{
	public T1 Item1;
	public Tuple(T1 item1)
	{
		Item1 = item1;
	}
        
    bool IEquatable<Tuple<T1>>.Equals(Tuple<T1> o)
    {
        return object.Equals(o.Item1, this.Item1);
    }
}

[TupleDescription(Dimension = 2)]
public class Tuple<T1, T2> : Tuple, IEquatable<Tuple<T1, T2>>
{
	public T1 Item1;
	public T2 Item2;

	public Tuple()
	{
	}

	public Tuple(T1 item1, T2 item2)
	{
		Item1 = item1;
		Item2 = item2;
	}
    bool IEquatable<Tuple<T1, T2>>.Equals(Tuple<T1, T2> o)
    {
        return object.Equals(o.Item1, this.Item1)
            && object.Equals(o.Item2, this.Item2);
    }
}

[TupleDescription(Dimension = 3)]
public class Tuple<T1, T2, T3> : Tuple, IEquatable<Tuple<T1, T2, T3>>
{
	public T1 Item1;
	public T2 Item2;
	public T3 Item3;

	public Tuple()
	{
	}

	public Tuple(T1 item1, T2 item2, T3 item3)
	{
		Item1 = item1;
		Item2 = item2;
		Item3 = item3;
	}

    bool IEquatable<Tuple<T1, T2, T3>>.Equals(Tuple<T1, T2, T3> o)
    {
        return object.Equals(o.Item1, this.Item1)
            && object.Equals(o.Item2, this.Item2)
            && object.Equals(o.Item3, this.Item3);
    }
}

[TupleDescription(Dimension = 4)]
public class Tuple<T1, T2, T3, T4> : Tuple, IEquatable<Tuple<T1, T2, T3, T4>>
{
	public T1 Item1;
	public T2 Item2;
	public T3 Item3;
	public T4 Item4;

	public Tuple()
	{
	}

	public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
	{
		Item1 = item1;
		Item2 = item2;
		Item3 = item3;
		Item4 = item4;
	}

    bool IEquatable<Tuple<T1, T2, T3, T4>>.Equals(Tuple<T1, T2, T3, T4> o)
    {
        return object.Equals(o.Item1, this.Item1)
            && object.Equals(o.Item2, this.Item2)
            && object.Equals(o.Item3, this.Item3)
            && object.Equals(o.Item4, this.Item4);
    }
}

[TupleDescription(Dimension = 5)]
public class Tuple<T1, T2, T3, T4, T5> : Tuple, IEquatable<Tuple<T1, T2, T3, T4, T5>>
{
	public T1 Item1;
	public T2 Item2;
	public T3 Item3;
	public T4 Item4;
	public T5 Item5;

	public Tuple()
	{
	}

	public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
	{
		Item1 = item1;
		Item2 = item2;
		Item3 = item3;
		Item4 = item4;
		Item5 = item5;
	}

    bool IEquatable<Tuple<T1, T2, T3, T4, T5>>.Equals(Tuple<T1, T2, T3, T4, T5> o)
    {
        return object.Equals(o.Item1, this.Item1)
            && object.Equals(o.Item2, this.Item2)
            && object.Equals(o.Item3, this.Item3)
            && object.Equals(o.Item4, this.Item4)
            && object.Equals(o.Item5, this.Item5);
    }
}

[TupleDescription(Dimension = 6)]
public class Tuple<T1, T2, T3, T4, T5, T6> : Tuple, IEquatable<Tuple<T1, T2, T3, T4, T5, T6>>
{
	public T1 Item1;
	public T2 Item2;
	public T3 Item3;
	public T4 Item4;
	public T5 Item5;
	public T6 Item6;

	public Tuple()
	{
	}

	public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
	{
		Item1 = item1;
		Item2 = item2;
		Item3 = item3;
		Item4 = item4;
		Item5 = item5;
		Item6 = item6;
	}

    bool IEquatable<Tuple<T1, T2, T3, T4, T5, T6>>.Equals(Tuple<T1, T2, T3, T4, T5, T6> o)
    {
        return object.Equals(o.Item1, this.Item1)
            && object.Equals(o.Item2, this.Item2)
            && object.Equals(o.Item3, this.Item3)
            && object.Equals(o.Item4, this.Item4)
            && object.Equals(o.Item5, this.Item5)
            && object.Equals(o.Item6, this.Item6);
    }
}
