---
external help file: configuration-substitution-help.xml
Module Name: configuration-substitution
online version:
schema: 2.0.0
---

# ConvertTo-NamingArray

## SYNOPSIS
Takes the supplied scheme string and resolves it to an array of values.

## SYNTAX

### WithNaming (Default)
```
ConvertTo-NamingArray -SchemeString <String> -NamingData <NamingData> [<CommonParameters>]
```

### WithObject
```
ConvertTo-NamingArray -SchemeString <String> -InputObject <PSObject> [<CommonParameters>]
```

## DESCRIPTION
With ConvertTo-NamingArray the scheme string is evaluated like described in
(link).
It replaces all placeholders with the actual data supplied.

## EXAMPLES

### BEISPIEL 1
```
ConvertTo-NamingArray -SchemeString "{givenName(?countUP)} {surName}"" -InputObject @{"givenName"="Torben";"surName"="Soennecken"}
```

\[
    "Torben Soennecken",
    "Torben1 Soennecken",
    "Torben2 Soennecken",
    ...
    "Torben9 Soennecken"
\]

## PARAMETERS

### -SchemeString
Format to resolve data into

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NamingData
Data to use for substitution of placeholders

```yaml
Type: NamingData
Parameter Sets: WithNaming
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Data to use for substitution of placeholders

```yaml
Type: PSObject
Parameter Sets: WithObject
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### System.String[]
## NOTES
If a placeholder can not be substituted a terminating error is going to be thrown.

## RELATED LINKS
