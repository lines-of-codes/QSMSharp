using System.Reflection;

namespace QSM.Core.ServerSettings;

public abstract class PropertyModificationModel
{
	private readonly Dictionary<string, PropertyInfo> _properties = [];

	protected PropertyModificationModel()
	{
		PropertyInfo[] props = GetType().GetProperties();
		
		foreach (PropertyInfo prop in props)
		{
			ServerPropertyAttribute? attr = prop.GetCustomAttribute<ServerPropertyAttribute>();
			if (attr == null) continue;
			
			_properties[attr.Name] = prop;
		}
	}

	public void Load(ServerProperties props)
	{
		foreach (var pair in _properties)
		{
			if (!props.Properties.TryGetValue(pair.Key, out string? value))
				continue;

			pair.Value.SetValue(this, Convert.ChangeType(value, pair.Value.PropertyType));
		}
	}
	
	public void Apply(ServerProperties props)
	{
		foreach (var pair in _properties)
		{
			var obj = pair.Value.GetValue(this);
			if (obj == null) continue;

			props.Properties[pair.Key] = obj.ToString() ?? string.Empty;
		}
	}
}