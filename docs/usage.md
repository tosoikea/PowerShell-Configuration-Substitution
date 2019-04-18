## Scheme Structure

A valid scheme is built up of multiple parts.
The most important are the **substitution variables**.
These specify the names of the values to place into the result.

**{variableName...}** 
-  They start and end with curly brackets
-  The name of the variable comes directly after the curly bracket

When constructing valid values with the supplied InputObject and Scheme String multiple operations can be applied. These alter the value to be placed into the result.

{variableName **(...)** }
- They are surrounded by brackets
- They are placed inside the substitution variable
- They only apply to the corresponding substitution variable
- They can be chained (multiple operations)  
=> {variableName **(...)(...)(...)**}

**1. lower**  
Converts the supplied value to it's lower representation.  
e.g. TEST => test  
**2. upper**  
Converts the supplied value to it's upper representation.  
e.g.  test => TEST  
**3. split**  
Splits values in two steps.  
3.1 White spaces  
If a value is separated by whitespaces, every part is regarded as it's own value. They are the starting point for the second step.  
3.2 Hyphens  
After the values are separated by white spaces, they are once again split by hyphens. For this separation process it takes the result of the first step.

```powershell
> $container = @{myVar="Hans-Peter Wurst"}
> ConvertTo-NamingArray -SchemeString "{myVar(split)}" -InputObject $container

Wurst
Hans-Peter
Peter
Hans
```
**4.countUP**  
Counts from 1 up to 9.
```powershell
> $container = @{myVar="Test"}
> ConvertTo-NamingArray -SchemeString "{myVar(countUP)}" -InputObject $container

Test1
Test2
...
Test9
```
**5.countDOWN**  
Counts from 9 down to 1.
```powershell
> $container = @{myVar="Test"}
> ConvertTo-NamingArray -SchemeString "{myVar(countUP)}" -InputObject $container

Test9
Test8
...
Test1
```
**6.replace[,]**  
Defines simple regex replacement. The value to the left of the comma defines the search regex, while the right value defines it's replacement.  
Forbidden character : { } ( ) [ ] 
```powershell
> $container = @{myVar="Test"}
> $v = ConvertTo-NamingArray -SchemeString "{myVar(replace[$,_Addition])}" -InputObject $container

Test_Addition
```
**7.Selection**  
Simple index based selection. A comma separated list of indexes can also be specified.  
```powershell
> $container = @{myVar="Test"}
> $v = ConvertTo-NamingArray -SchemeString "{myVar(0)}" -InputObject $container

T
```

## Failover
As explained earlier, operations can be chained. When chaining operations they will all be applied onto the the value, resulting in a single result value.  
If multiple values are desired, the failover character **?** can be used. If an operation or substitution variable starts with this character, it will be marked as failover.  
During execution this module will determine all result paths inside the scheme string and build the results accordingly.

Simple Failover :
```powershell
> $container = @{myVar="Test"}
> $v = ConvertTo-NamingArray -SchemeString "{myVar(lower)(?0)}" -InputObject $container

test
t
```

Chained Failover :
```powershell
> $container = @{myVar="Test"}
> $v = ConvertTo-NamingArray -SchemeString "{myVar(lower)(?0?1,2?upper)}" -InputObject $container

test
t
es
TEST
```
