    
$projectRoot = Resolve-Path "$PSScriptRoot\.."
$moduleRoot = Split-Path (Resolve-Path "$projectRoot\*\*.psd1")
$moduleName = Split-Path $moduleRoot -Leaf

Import-Module (Join-Path $moduleRoot "$moduleName.psd1") -force



InModuleScope configuration-substitution {
    function Test-NamingArray {
        param([Parameter(Mandatory)][String[]]$Expected, [Parameter(Mandatory)][String[]]$Given)
        
        $Given.Length -eq $Expected.Length | Should -Be $true
        for ([int]$eI = 0; $eI -lt $Expected.Length; $eI++) {
            $Given[$eI] -eq $Expected[$eI] | Should -Be $true
        }
    }
    Describe 'Resolve SchemeString' {
        [hashtable] $base = @{simple="TEst";split="Hans-Peter Wurst"}
        It 'allows plain text' {
            [String] $naming = ConvertTo-NamingArray -SchemeString "simple" -InputObject @{ }
            $naming -eq "simple" | Should -Be $true
        }
        It 'handles simple substitution' {
            [String] $naming = ConvertTo-NamingArray -SchemeString "{simple}" -InputObject $base
            $naming -eq "TEst" | Should -Be $true
        }
        It 'handles lower' {
            [String] $naming = ConvertTo-NamingArray -SchemeString "{simple(lower)}" -InputObject $base
            $naming -eq "test" | Should -Be $true
        }
        It 'handles upper' {
            [String] $naming = ConvertTo-NamingArray -SchemeString "{simple(upper)}" -InputObject $base
            $naming -eq "TEST" | Should -Be $true
        }
        It 'handles split' {
            [String[]] $naming = ConvertTo-NamingArray -SchemeString "{split(split)}" -InputObject $base
            $expected = @("Wurst","Hans-Peter","Peter","Hans")
            
            Test-NamingArray -Expected $expected -Given $naming
        }
        It 'handles countUP' {
            [String[]] $naming = ConvertTo-NamingArray -SchemeString "{simple(countUP)}" -InputObject $base
            $expected = @("TEst1", "TEst2", "TEst3", "TEst4", "TEst5", "TEst6", "TEst7", "TEst8", "TEst9")
            
            Test-NamingArray -Expected $expected -Given $naming
        } 
        It 'handles countDOWN' {
            [String[]] $naming = ConvertTo-NamingArray -SchemeString "{simple(countDOWN)}" -InputObject $base
            $expected = @("TEst9", "TEst8", "TEst7", "TEst6", "TEst5", "TEst4", "TEst3", "TEst2", "TEst1")
            
            Test-NamingArray -Expected $expected -Given $naming
        }
        It 'handles selection' {
            [String] $naming = ConvertTo-NamingArray -SchemeString "{simple(0)}" -InputObject $base
            $naming -eq "T" | Should -Be $true
            
            [String] $namingSecond = ConvertTo-NamingArray -SchemeString "{simple(0,2)}" -InputObject $base
            $namingSecond -eq "Ts" | Should -Be $true
        }
    }

    Describe 'Failover'{
        It 'handles simple operation failover'{
            [String[]] $naming = @()
            $naming += ConvertTo-NamingArray -SchemeString "{test(lower)(?0)(?countUP)}" -InputObject @{test="SIMPLE"}
            $expected = @("simple","s1","s2","s3","s4","s5","s6","s7","s8","s9")

            Test-NamingArray -Expected $expected -Given $naming
        }
        It 'handles complex operation failover'{
            [String[]] $naming = @()
            $naming += ConvertTo-NamingArray -SchemeString "{first(lower)(?0?1,2)}{?second(?lower)}" -InputObject @{first = "SIMPLE";second="COMPLEX"}
            $expected = @("simple","simpleCOMPLEX","simplecomplex","s","sCOMPLEX","scomplex","im","imCOMPLEX","imcomplex")
            
            Test-NamingArray -Expected $expected -Given $naming
        }
    }

}