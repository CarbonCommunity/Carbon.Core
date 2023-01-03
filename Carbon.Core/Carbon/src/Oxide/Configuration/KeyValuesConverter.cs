using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core.Configuration;

public class KeyValuesConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Dictionary<string, object>) || objectType == typeof(List<object>);
	}

	private void Throw(string message)
	{
		throw new Exception(message);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (objectType == typeof(Dictionary<string, object>))
		{
			var dictionary = (existingValue as Dictionary<string, object>) ?? new Dictionary<string, object>();

			if (reader.TokenType == JsonToken.StartArray)
			{
				return dictionary;
			}

			while (reader.Read() && reader.TokenType != JsonToken.EndObject)
			{
				if (reader.TokenType != JsonToken.PropertyName)
				{
					Throw("Unexpected token: " + reader.TokenType.ToString());
				}

				var key = reader.Value as string;

				if (!reader.Read())
				{
					Throw("Unexpected end of json");
				}

				switch (reader.TokenType)
				{
					case JsonToken.StartObject:
						dictionary[key] = serializer.Deserialize<Dictionary<string, object>>(reader);
						continue;
					case JsonToken.StartArray:
						dictionary[key] = serializer.Deserialize<List<object>>(reader);
						continue;
					case JsonToken.Integer:
						{
							string text = reader.Value.ToString();
							int num;
							if (int.TryParse(text, out num))
							{
								dictionary[key] = num;
								continue;
							}
							dictionary[key] = text;
							continue;
						}
					case JsonToken.Float:
					case JsonToken.String:
					case JsonToken.Boolean:
					case JsonToken.Null:
					case JsonToken.Date:
					case JsonToken.Bytes:
						dictionary[key] = reader.Value;
						continue;
				}

				Throw("Unexpected token: " + reader.TokenType.ToString());
			}
			return dictionary;
		}
		else
		{
			if (objectType == typeof(List<object>))
			{
				var list = (existingValue as List<object>) ?? new List<object>();

				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					switch (reader.TokenType)
					{
						case JsonToken.StartObject:
							list.Add(serializer.Deserialize<Dictionary<string, object>>(reader));
							continue;
						case JsonToken.StartArray:
							list.Add(serializer.Deserialize<List<object>>(reader));
							continue;
						case JsonToken.Integer:
							{
								string text2 = reader.Value.ToString();
								int num2;
								if (int.TryParse(text2, out num2))
								{
									list.Add(num2);
									continue;
								}
								list.Add(text2);
								continue;
							}
						case JsonToken.Float:
						case JsonToken.String:
						case JsonToken.Boolean:
						case JsonToken.Null:
						case JsonToken.Date:
						case JsonToken.Bytes:
							list.Add(reader.Value);
							continue;
					}

					Throw("Unexpected token: " + reader.TokenType.ToString());
				}
				return list;
			}
			return existingValue;
		}
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value is Dictionary<string, object>)
		{
			var source = (Dictionary<string, object>)value;
			writer.WriteStartObject();
			foreach (KeyValuePair<string, object> keyValuePair in from i in source
																  orderby i.Key
																  select i)
			{
				writer.WritePropertyName(keyValuePair.Key, true);
				serializer.Serialize(writer, keyValuePair.Value);
			}

			writer.WriteEndObject();
			return;
		}
		if (value is List<object>)
		{
			var list = (List<object>)value;
			writer.WriteStartArray();

			foreach (object value2 in list)
			{
				serializer.Serialize(writer, value2);
			}

			writer.WriteEndArray();
		}
	}
}
