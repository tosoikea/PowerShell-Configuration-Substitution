    
$projectRoot = Resolve-Path "$PSScriptRoot\.."
$moduleRoot = Split-Path (Resolve-Path "$projectRoot\*\*.psd1")
$moduleName = Split-Path $moduleRoot -Leaf

Import-Module (Join-Path $moduleRoot "$moduleName.psd1") -force


InModuleScope configuration-substitution {
    Describe 'Resolve SchemeString'{
        It 'allows plain text' {
            [String[]] $naming = @()
            $naming += ConvertTo-NamingArray -SchemeString "simple" -InputObject @{}
            $naming.Length -eq 1 | Should -Be $true
            $naming[0] -eq "simple" | Should -Be $true
        }
        It 'handles simple substitution' {
            [String[]] $naming = @()
            $naming += ConvertTo-NamingArray -SchemeString "{simple}" -InputObject @{simple="test"}
            $naming.Length -eq 1 | Should -Be $true
            $naming[0] -eq "test" | Should -Be $true
        }
        It 'handles simple operation failover'{
            [String[]] $naming = @()
            $naming += ConvertTo-NamingArray -SchemeString "{test(lower)(?0)(?countUP)}" -InputObject @{test="SIMPLE"}
            $expected = @("simple","s1","s2","s3","s4","s5","s6","s7","s8","s9")

            $naming.Length -eq $expected.Length | Should -Be $true
            for([int]$eI = 0; $eI -lt $expected.Length;$eI++){
                $naming[$eI] -eq $expected[$eI] | Should -Be $true
            }
        }
    }
}