<#
.SYNOPSIS
Takes the supplied scheme string and resolves it to an array of values.

.DESCRIPTION
With ConvertTo-NamingArray the scheme string is evaluated like described in
(link). It replaces all placeholders with the actual data supplied.

.PARAMETER SchemeString
Format to resolve data into

.PARAMETER NamingData
Data to use for substitution of placeholders

.PARAMETER InputObject
Data to use for substitution of placeholders

.EXAMPLE
ConvertTo-NamingArray -SchemeString "{givenName(?countUP)} {surName}"" -InputObject @{"givenName"="Torben";"surName"="Soennecken"}
[
    "Torben Soennecken",
    "Torben1 Soennecken",
    "Torben2 Soennecken",
    ...
    "Torben9 Soennecken"
]

.NOTES
If a placeholder can not be substituted a terminating error is going to be thrown.
#>

function ConvertTo-NamingArray {
    [CmdletBinding(DefaultParameterSetName = "WithNaming")]
    [OutputType([String[]])]
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
            [adesso.BusinessProcesses.ConfigurationSubstitution.Templating.Resolver]::ConvertSchemeToArray($SchemeString, $NamingData)
        }
        "WithObject" {
            $naming = [adesso.BusinessProcesses.ConfigurationSubstitution.NamingData]::new($InputObject)
            [adesso.BusinessProcesses.ConfigurationSubstitution.Templating.Resolver]::ConvertSchemeToArray($SchemeString, $naming)
        }
    }
}