.NET Core using roslyn to detect possible problematic code
For example:
- SettableStaticFieldOrProperty: public static string StaticProperty { get; set; }
- NullableCastToNonNullable: int? b = (int)std.Age; // Age is nullable