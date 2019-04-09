<#
.SYNOPSIS
Takes the supplied scheme string and resolves it to a hashtable of values.

.DESCRIPTION
With ConvertTo-NamingDictionarythe scheme string is evaluated like described in
(link). It replaces all placeholders with the actual data supplied.
The result is split into :
1. value = placeholders substituted with corresponding value, no failover parameters evaluated
2. failover = all failover possibilites

.PARAMETER SchemeString
Format to resolve data into

.PARAMETER NamingData
Data to use for substitution of placeholders

.PARAMETER InputObject
Data to use for substitution of placeholders

.EXAMPLE
onvertTo-NamingDictionary -SchemeString "{givenName(?countUP)} {surName}"" -InputObject @{"givenName"="Torben";"surName"="Soennecken"}
@{
    "value" = "Torben Soennecken"
    "failover" = @(
        "Torben1 Soennecken",
        "Torben2 Soennecken",
        ...
        "Torben9 Soennecken"
    )
}

.NOTES
If a placeholder can not be substituted a terminating error is going to be thrown.
#>
function ConvertTo-NamingDictionary {
    [CmdletBinding(DefaultParameterSetName = "WithNaming")]
    [OutputType([Hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [String]    
        $SchemeString,
        [Parameter(Mandatory = $true, ParameterSetName = "WithNaming")]
        [adesso.BusinessProcesses.ConfigurationSubstitution.NamingData]
        $NamingData,
        [Parameter(Mandatory = $true, ParameterSetName = "WithObject")]
        [PSObject]
        $InputObject
    )

    switch ($PSCmdlet.ParameterSetName) {
        "WithNaming" {
            [adesso.BusinessProcesses.ConfigurationSubstitution.Templating.Resolver]::ConvertSchemeToDictionary($SchemeString, $NamingData)
        }
        "WithObject" {
            $naming = [adesso.BusinessProcesses.ConfigurationSubstitution.NamingData]::new($InputObject)
            [adesso.BusinessProcesses.ConfigurationSubstitution.Templating.Resolver]::ConvertSchemeToDictionary($SchemeString, $naming)
        }
    }
}