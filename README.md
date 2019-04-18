![Build status](https://ci.appveyor.com/api/projects/status/gsioxb902o895gta?svg=true)
[![Documentation Status](https://readthedocs.org/projects/configuration-substitution/badge/?version=latest)](https://configuration-substitution.readthedocs.io/en/latest/?badge=latest)
# PowerShell Configuration Substitution

## Introduction
This module essentially provides a simple way of substituting placeholder variables inside
a scheme string with runtime data.

The module is built to work with a plethora of PSObjects to improve simplicity of calls.
This allows the direct usage of the results e.g. provided by the ActiveDirectory module.

ConvertTo-NamingArray -SchemeString "{givenName(0)}{surname}" -InputObject (Get-ADUser -Identity test)

**Documentation can be found at : https://configuration-substitution.readthedocs.io/en/latest/**

## Structure

### General

A scheme string is build up of multiple parts.
1. Variable
    **{*name*....}**
    2.1 The used name directly corellates to the name inside the supplied
    data structure during runtime
    2.2 A variable can be marked as failover with a starting **?**
2. Operation (lower,upper,split,countUP,countDOWN,split,**replace**,**0/1/2/...**)
    **{...(*operation*)}**
    2.1 The operation is evaluated upon the value supplied during runtime.
    e.g. scheme = "{givenName(lower)}"
    => from *TORBEN* to *torben*
    2.2 An operation can be marked as failover with a starting **?**
3. String data
    **i am string data**
    3.1 Plain text with no further evaluation.

### Failover
During evaluation of a scheme an operation tree is built.
A scheme can have different, valid paths depending on the mandatory of an operation.
The operation tree includes every possible, valid constellation of operations.

**{?failoverVar}.{mandatoryVar}**
- failoverVar is marked as a failover variable
- valid paths are
1. **.mandatory**
2. **failoverVar**.**mandatoryVar**


**{variable(lower)(?0)(?countUp)}** with variable = TEST
- the usage of the selector (0) and countUP operation are marked as failover
- valid paths are
1. test
2. t1
3. t2
..
10. t9


**{variable(lower)(?0?countUP)}** with variable = TEST
- the usage of the selector (0) is marked as failover with an additional failover to the countUP operation
- valid paths are
1. test
2. t
3. test1
4. test2
..
11. test9



### Examples
1. {?givenName(lower)(0?0,1?0,2?0,1,2)}{surName(lower)(?split)}
2. {givenName(replace[ ,])(replace[\\.,])}.{surName(replace[ ,])(?countUP)}
3. {surName(?replace[$, ])(?countUP)}, {givenName}


## Example Usage
I heavily use this module to generate naming values for user creations.
With an example scheme string of "{givenName(lower)}.{surName(lower)(?countUP)}@{PrincipalName}" a multitude
of viable UserPrincipalNames can be generated.

## License
This project is licensed under the MIT License