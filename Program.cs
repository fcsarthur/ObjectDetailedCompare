using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

public class Program
{
	public static void Main()
	{
		var p1 = new Person(1, "Arthur", 24);
		var p1Edit = new Person(1, null, 25);

		var lista = p1.DetailedCompare(p1Edit);
		Console.WriteLine("Changed fields count: " + lista.Count);

		foreach (var a in lista)
		{
			Console.WriteLine("Prop: " + a.Property + " | OldVal: " + a.OldValue + " | NewVal: " + a.NewValue);
		}
	}
}

public class Person
{
	public Person(int _id, string _name, int _age)
	{
		this.Id = _id;
		this.Name = _name;
		this.Age = _age;
	}

	public int Id { get; set; }

	[IsDetailedComparedProp]
	public string Name { get; set; }

	[DisplayName("Changed Age Prop")]
	[IsDetailedComparedProp]
	public int Age { get; set; }
}

public class IsDetailedComparedPropAttribute : Attribute
{
}

public static class Extensions
{
	public static List<PropChange> DetailedCompare<T>(this T val1, T val2)
	{
		var changes = new List<PropChange>();
		var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(pi => pi.GetCustomAttributes(typeof(IsDetailedComparedPropAttribute), true).Length > 0).ToArray();
		
		foreach (var property in properties)
		{
			string propName = property.Name;

            try
            {
				var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Single();
				propName = attribute.DisplayName;
            }
            catch (Exception ex){ }

			var propChange = new PropChange
			{
				Property = propName,
				OldValue = property.GetValue(val1),
				NewValue = property.GetValue(val2)
			};

			if (propChange.OldValue == null && propChange.NewValue == null)
				continue;

			if ((propChange.OldValue == null && propChange.NewValue != null) || (propChange.OldValue != null && propChange.NewValue == null)
			)
			{
				changes.Add(propChange);
				continue;
			}

			if (!Equals(propChange.OldValue, propChange.NewValue))
				changes.Add(propChange);
		}

		return changes;
	}

	public class PropChange
	{
		public string Property { get; set; }
		public object OldValue { get; set; }
		public object NewValue { get; set; }
	}
}