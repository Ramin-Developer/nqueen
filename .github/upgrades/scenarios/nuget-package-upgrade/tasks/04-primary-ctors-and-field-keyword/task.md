# 04-primary-ctors-and-field-keyword: Apply primary constructors and C# 14 field keyword

Convert trivial constructor-assigned fields to primary constructors (e.g. `Solution`, test helpers) and replace backing fields with the `field` keyword where a property only validates/assigns (e.g. `BitmaskSolver.DelayInMillisec`). Skip WPF code-behind where unsafe.

**Done when**: Conversions applied where trivial; build clean, tests pass.
