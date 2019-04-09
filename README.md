![Build status](https://ci.appveyor.com/api/projects/status/gsioxb902o895gta?svg=true)
# PowerShell Configuration Substitution

## Introduction
This module essentially provides a simple way of substituting placeholder variables inside
a scheme string with runtime data.

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
A scheme can have different, valid paths depending on the mandatority of an operation.
The operation tree includes every possible, valid constellation of operations.

e.g {?failoverVar}.{mandatoryVar}
=>
.mandatoryVar
failoverVar.mandatoryVar
e.g {variable(lower)(?0)(?countUp)} with variable = TEST
=>
test
t1
t2
..
t9
e.g {variable(lower)(?0?countUP)} with variable = TEST
=>
lol
l
lol1
..
lol9



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
