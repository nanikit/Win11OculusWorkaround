namespace OculusWin11Fix.Services {
  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;
  using System;

  public class SafeStringEnumConverter<T> : StringEnumConverter {
    public T DefaultValue { get; }

    public SafeStringEnumConverter(T defaultValue) {
      DefaultValue = defaultValue;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
      try {
        return base.ReadJson(reader, objectType, existingValue, serializer);
      }
      catch {
        return DefaultValue;
      }
    }
  }
}
